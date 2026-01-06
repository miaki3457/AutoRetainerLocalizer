using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Localizer
{
    public class TranslationRewriter : CSharpSyntaxRewriter
    {
        private readonly Dictionary<string, string> _dictionary;
        private readonly string _apiKey;
        private static readonly HttpClient _client = new HttpClient();

        private readonly string[] _uiKeywords = { "Text", "Button", "Label", "Combo", "Header", "Section", "Tooltip", "MenuItem", "Checkbox", "Help", "Notify", "Info", "FormatToken", "InputInt", "Widget", "EnumCombo" };
        private readonly string[] _blackList = { "PushID", "GetConfig", "Log", "Debug", "Print", "ExecuteCommand", "ToString", "GetField", "GetProperty" };

        public TranslationRewriter(Dictionary<string, string> dictionary, string apiKey = "")
        {
            _dictionary = dictionary;
            _apiKey = apiKey;
        }

        public override SyntaxNode VisitInterpolatedStringText(InterpolatedStringTextSyntax node)
        {
            return base.VisitInterpolatedStringText(node);
        }

        public override SyntaxNode VisitInterpolatedStringExpression(InterpolatedStringExpressionSyntax node)
        {
            var sb = new StringBuilder();
            int placeholderIndex = 0;
            var interpolations = new List<InterpolationSyntax>();

            foreach (var content in node.Contents)
            {
                if (content is InterpolatedStringTextSyntax text)
                    sb.Append(text.TextToken.ValueText);
                else if (content is InterpolationSyntax interp)
                {
                    sb.Append($"{{{placeholderIndex++}}}");
                    interpolations.Add(interp);
                }
            }

            string templateKey = sb.ToString();

            if (ShouldTranslate(node, templateKey))
            {
                // 嘗試三種匹配模式
                if (_dictionary.TryGetValue(templateKey, out var translated) ||
                    _dictionary.TryGetValue(templateKey.Trim(), out translated) ||
                    _dictionary.TryGetValue(NormalizeKey(templateKey), out translated))
                {
                    // 傳入 node 以便獲取原始的開頭和結尾 Token (解決 $""" 問題)
                    return ReconstructInterpolatedString(node, translated, interpolations);
                }
            }

            return base.VisitInterpolatedStringExpression(node);
        }

        public override SyntaxNode VisitLiteralExpression(LiteralExpressionSyntax node)
        {
            if (node.IsKind(SyntaxKind.StringLiteralExpression))
            {
                string originalText = node.Token.ValueText;
                if (ShouldTranslate(node, originalText))
                {
                    if (_dictionary.TryGetValue(originalText, out var translated) ||
                        _dictionary.TryGetValue(originalText.Trim(), out translated) ||
                        _dictionary.TryGetValue(NormalizeKey(originalText), out translated))
                    {
                        return SyntaxFactory.LiteralExpression(SyntaxKind.StringLiteralExpression, SyntaxFactory.Literal(translated));
                    }
                }
            }
            return base.VisitLiteralExpression(node);
        }

        private bool ShouldTranslate(SyntaxNode node, string text)
        {
            if (string.IsNullOrWhiteSpace(text)) return false;

            var invocation = node.Ancestors().OfType<InvocationExpressionSyntax>().FirstOrDefault();
            if (invocation != null)
            {
                string methodName = GetMethodName(invocation);
                if (_uiKeywords.Any(k => methodName.Contains(k))) return true;
            }

            var methodDecl = node.Ancestors().OfType<MethodDeclarationSyntax>().FirstOrDefault();
            if (methodDecl != null)
            {
                string methodName = methodDecl.Identifier.Text;
                if (methodName.Contains("Format") || methodName.Contains("Get") ||
                    methodName.Contains("Draw") || methodName.Contains("Tooltip"))
                {
                    return IsHumanText(text);
                }
            }
            
            var property = node.Ancestors().OfType<PropertyDeclarationSyntax>().FirstOrDefault();
            if (property != null)
            {
                string propName = property.Identifier.Text;
                if (propName == "Name" || propName == "Path") return true;
            }

            return false;
        }

        private bool IsHumanText(string text)
        {
            string clean = text.Trim(' ', '\n', '\r', '\"', '\\', 't');
            if (clean.Length == 0) return false;
            return clean.Any(c => char.IsLetter(c) || char.GetUnicodeCategory(c) == System.Globalization.UnicodeCategory.OtherLetter);
        }

        private string NormalizeKey(string text)
        {
            if (string.IsNullOrEmpty(text)) return text;
            return Regex.Replace(text, @"\s+", " ").Trim();
        }

        private string GetMethodName(InvocationExpressionSyntax invocation)
        {
            if (invocation.Expression is MemberAccessExpressionSyntax m) return m.Name.Identifier.Text;
            if (invocation.Expression is IdentifierNameSyntax i) return i.Identifier.Text;
            return invocation.Expression.ToString();
        }

        private SyntaxNode ReconstructInterpolatedString(InterpolatedStringExpressionSyntax node, string translatedTemplate, List<InterpolationSyntax> interpolations)
        {
            var contents = new List<InterpolatedStringContentSyntax>();
            bool isRawString = node.StringStartToken.ValueText.StartsWith("$" + "\"\"\"");
        
            string leadingWhitespace = "";
            if (isRawString)
            {
                // 獲取結尾引號的列位置 (Column) 作為基準
                var lineSpan = node.StringEndToken.GetLocation().GetLineSpan();
                leadingWhitespace = new string(' ', lineSpan.StartLinePosition.Character);
            }
        
            var matches = Regex.Matches(translatedTemplate, @"\{(\d+)\}");
            int lastIndex = 0;
        
            string ProcessText(string rawText, bool isFirstPart)
            {
                if (string.IsNullOrEmpty(rawText)) return rawText;
                if (!isRawString) return rawText.Replace("\"", "\\\"");
        
                // 1. 先處理掉翻譯文本中可能意外帶有的多餘頭尾換行與空格
                // 2. 將每一行內容重新對齊到基準縮排
                var lines = rawText.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.None);
                
                for (int i = 0; i < lines.Length; i++)
                {
                    lines[i] = lines[i].TrimStart(); // 去掉字典裡可能自帶的舊縮排
                }
        
                string joined = string.Join("\n" , lines);        
                return isFirstPart ? leadingWhitespace + joined : joined;
            }
        
            // 標記是否為字串的最開頭部分
            bool isFirst = true;
        
            foreach (Match match in matches)
            {
                if (match.Index > lastIndex)
                {
                    string textPart = translatedTemplate.Substring(lastIndex, match.Index - lastIndex);
                    string finalPart = ProcessText(textPart, isFirst);
                    isFirst = false; // 處理過第一段後關閉標記
        
                    contents.Add(SyntaxFactory.InterpolatedStringText(
                        SyntaxFactory.Token(SyntaxFactory.TriviaList(), SyntaxKind.InterpolatedStringTextToken, finalPart, textPart, SyntaxFactory.TriviaList())));
                }
        
                int idx = int.Parse(match.Groups[1].Value);
                if (idx < interpolations.Count) contents.Add(interpolations[idx]);
                lastIndex = match.Index + match.Length;
            }
        
            if (lastIndex < translatedTemplate.Length)
            {
                string rest = translatedTemplate.Substring(lastIndex);
                string finalRest = ProcessText(rest, isFirst);
        
                contents.Add(SyntaxFactory.InterpolatedStringText(
                    SyntaxFactory.Token(SyntaxFactory.TriviaList(), SyntaxKind.InterpolatedStringTextToken, finalRest, rest, SyntaxFactory.TriviaList())));
            }
        
            return SyntaxFactory.InterpolatedStringExpression(
                node.StringStartToken,
                SyntaxFactory.List(contents),
                node.StringEndToken);
        }
    }
}

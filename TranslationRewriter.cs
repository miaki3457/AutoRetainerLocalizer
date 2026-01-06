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
            var matches = Regex.Matches(translatedTemplate, @"\{(\d+)\}");
            int lastIndex = 0;

            // 1. 偵測是否為 Raw String ($"""...)
            string startTokenText = node.StringStartToken.ToString();
            bool isRawString = startTokenText.Contains("\"\"\"");

            // 2. 如果是 Raw String，提取它的縮排量 (Indentation)
            // 我們從 StringEndToken (最後的 """) 所在的行首計算空格數
            string indentation = "";
            if (isRawString)
            {
                var lineSpan = node.StringEndToken.GetLocation().GetLineSpan();
                // 取得結尾標記所在行的起始位置，藉此推算基礎縮排
                indentation = new string(' ', lineSpan.StartLinePosition.Character);
            }

            foreach (Match match in matches)
            {
                if (match.Index > lastIndex)
                {
                    string textPart = translatedTemplate.Substring(lastIndex, match.Index - lastIndex);
                    contents.Add(CreateTextContent(textPart, isRawString, indentation));
                }

                int idx = int.Parse(match.Groups[1].Value);
                if (idx < interpolations.Count)
                {
                    contents.Add(interpolations[idx]);
                }
                lastIndex = match.Index + match.Length;
            }

            if (lastIndex < translatedTemplate.Length)
            {
                string rest = translatedTemplate.Substring(lastIndex);
                contents.Add(CreateTextContent(rest, isRawString, indentation));
            }

            return SyntaxFactory.InterpolatedStringExpression(
                node.StringStartToken,
                SyntaxFactory.List(contents),
                node.StringEndToken);
        }

        private InterpolatedStringContentSyntax CreateTextContent(string text, bool isRawString, string indentation)
        {
            string finalValueText = text;
            string finalRawText = text;

            if (isRawString && text.Contains("\n"))
            {
                var lines = text.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.None);
                for (int i = 1; i < lines.Length; i++)
                {
                    lines[i] = indentation + lines[i];
                }
                finalValueText = string.Join(Environment.NewLine, lines);
                finalRawText = finalValueText; 
            }
            else if (!isRawString)
            {
                finalRawText = text.Replace("\"", "\\\""); // 一般字串需要轉義
            }

            return SyntaxFactory.InterpolatedStringText(
                SyntaxFactory.Token(
                    SyntaxFactory.TriviaList(),
                    SyntaxKind.InterpolatedStringTextToken,
                    finalValueText,
                    finalRawText,
                    SyntaxFactory.TriviaList()));
        }
    }
}

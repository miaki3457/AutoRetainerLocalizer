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
            
            // 1. 偵測是否為原始字串 ($"""...)
            bool isRawString = node.StringStartToken.ValueText.StartsWith("$" + "\"\"\"");
        
            // 2. 獲取基準縮排 (抓取結尾引號所在的列數作為空格數)
            string leadingWhitespace = "";
            if (isRawString)
            {
                var lineSpan = node.StringEndToken.GetLocation().GetLineSpan();
                int column = lineSpan.StartLinePosition.Character;
                leadingWhitespace = new string(' ', column);
            }
        
            var matches = Regex.Matches(translatedTemplate, @"\{(\d+)\}");
            int lastIndex = 0;
        
            // 定義一個內部處理函式，統一處理多行縮排與轉義
            string ProcessText(string rawText)
            {
                if (string.IsNullOrEmpty(rawText)) return rawText;
                
                // 如果是原始字串，我們要把換行符號後方補上縮排空格
                // 如果是一般字串，則需要轉義引號
                string processed = isRawString 
                    ? rawText.Replace("\n", "\n" + leadingWhitespace) 
                    : rawText.Replace("\"", "\\\"");
                    
                return processed;
            }
        
            foreach (Match match in matches)
            {
                // 處理插值與插值之間的純文字
                if (match.Index > lastIndex)
                {
                    string textPart = translatedTemplate.Substring(lastIndex, match.Index - lastIndex);
                    string finalPart = ProcessText(textPart);
        
                    contents.Add(SyntaxFactory.InterpolatedStringText(
                        SyntaxFactory.Token(
                            SyntaxFactory.TriviaList(),
                            SyntaxKind.InterpolatedStringTextToken,
                            finalPart, // 顯示在編輯器上的樣子
                            textPart,  // 實際的值
                            SyntaxFactory.TriviaList())));
                }
        
                // 插入原本的插值運算式 (例如 {0}, {itemName})
                int idx = int.Parse(match.Groups[1].Value);
                if (idx < interpolations.Count)
                {
                    contents.Add(interpolations[idx]);
                }
                lastIndex = match.Index + match.Length;
            }
        
            // 處理最後一段剩餘的文字
            if (lastIndex < translatedTemplate.Length)
            {
                string rest = translatedTemplate.Substring(lastIndex);
                string finalRest = ProcessText(rest);
        
                contents.Add(SyntaxFactory.InterpolatedStringText(
                    SyntaxFactory.Token(
                        SyntaxFactory.TriviaList(),
                        SyntaxKind.InterpolatedStringTextToken,
                        finalRest,
                        rest,
                        SyntaxFactory.TriviaList())));
            }
        
            // 重新組合成插值字串運算式，並保留原始的開頭與結尾 Token
            return SyntaxFactory.InterpolatedStringExpression(
                node.StringStartToken,
                SyntaxFactory.List(contents),
                node.StringEndToken);
        }
    }
}

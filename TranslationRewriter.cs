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

        private readonly string[] _uiKeywords = { "Text", "Button", "Label", "Combo", "Header", "Section", "Tooltip", "MenuItem", "Checkbox", "Help", "Notify", "Info", "FormatToken" };
        private readonly string[] _blackList = { "PushID", "GetConfig", "Log", "Debug", "Print", "ExecuteCommand", "ToString" };

        public TranslationRewriter(Dictionary<string, string> dictionary, string apiKey = "")
        {
            _dictionary = dictionary;
            _apiKey = apiKey;
        }

        // 處理內插字串內部的純文字部分 (如 $"Entrust plan \"...\"")
        public override SyntaxNode VisitInterpolatedStringText(InterpolatedStringTextSyntax node)
        {
            // 在這裡攔截內插字串中的文字碎片是不夠的，因為字典 Key 是整句範本
            // 所以我們維持在 VisitInterpolatedStringExpression 處理整句
            return base.VisitInterpolatedStringText(node);
        }

        public override SyntaxNode VisitLiteralExpression(LiteralExpressionSyntax node)
        {
            if (node.IsKind(SyntaxKind.StringLiteralExpression))
            {
                string originalText = node.Token.ValueText;
                if (ShouldTranslate(node, originalText))
                {
                    if (_dictionary.TryGetValue(originalText, out var translated))
                    {
                        // 使用 SyntaxFactory.Literal 會正確處理轉義字符 (如引號和換行)
                        return SyntaxFactory.LiteralExpression(SyntaxKind.StringLiteralExpression, SyntaxFactory.Literal(translated));
                    }
                    // AI 翻譯略 (邏輯同前)...
                }
            }
            return base.VisitLiteralExpression(node);
        }

        public override SyntaxNode VisitInterpolatedStringExpression(InterpolatedStringExpressionSyntax node)
        {
            // 1. 將整句拼湊成帶 {0} 的範本
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

            // 2. 判斷與翻譯
            if (ShouldTranslate(node, templateKey))
            {
                if (_dictionary.TryGetValue(templateKey, out var translated))
                {
                    return ReconstructInterpolatedString(translated, interpolations);
                }
                // AI 翻譯 (templateKey) ...
            }

            return base.VisitInterpolatedStringExpression(node);
        }

        private bool ShouldTranslate(SyntaxNode node, string text)
        {
            // 先清理字串，看看裡面有沒有實質內容（字母或中文）
            if (string.IsNullOrWhiteSpace(text)) return false;

            // 1. 檢查是否在 UI 方法呼叫中
            // 使用 Ancestors() 確保無論嵌套多深（括號、加法）都能向上找到方法名
            var invocation = node.Ancestors().OfType<InvocationExpressionSyntax>().FirstOrDefault();
            if (invocation != null)
            {
                string methodName = GetMethodName(invocation);
                if (_uiKeywords.Any(k => methodName.Contains(k))) return true;
            }

            // 2. 檢查是否在 Return 語句中 (針對 FormatToken 等方法)
            var methodDecl = node.Ancestors().OfType<MethodDeclarationSyntax>().FirstOrDefault();
            if (methodDecl != null)
            {
                string methodName = methodDecl.Identifier.Text;
                // 擴大匹配範圍，只要方法名稱看起來是提供文字或 UI 繪製的
                if (methodName.Contains("Format") || methodName.Contains("Get") ||
                    methodName.Contains("Draw") || methodName.Contains("Tooltip"))
                {
                    // 在這些方法內，只要包含字母或中文就允許翻譯 (即使開頭是空格)
                    return IsHumanText(text);
                }
            }

            return false;
        }

        private bool IsHumanText(string text)
        {
            // 去除空格、換行與引號後檢查
            string clean = text.Trim(' ', '\n', '\r', '\"', '\\', 't');
            if (clean.Length == 0) return false;

            // 只要內容包含任何 字母 或 中文字元 就視為人類語言
            return clean.Any(c => char.IsLetter(c) || char.GetUnicodeCategory(c) == System.Globalization.UnicodeCategory.OtherLetter);
        }

        private string GetMethodName(InvocationExpressionSyntax invocation)
        {
            if (invocation.Expression is MemberAccessExpressionSyntax m) return m.Name.Identifier.Text;
            if (invocation.Expression is IdentifierNameSyntax i) return i.Identifier.Text;
            return invocation.Expression.ToString();
        }

        private SyntaxNode ReconstructInterpolatedString(string translatedTemplate, List<InterpolationSyntax> interpolations)
        {
            var contents = new List<InterpolatedStringContentSyntax>();
            // 匹配 {0}, {1} 等預留位置
            var matches = Regex.Matches(translatedTemplate, @"\{(\d+)\}");
            int lastIndex = 0;

            foreach (Match match in matches)
            {
                // 處理變數前的文字
                if (match.Index > lastIndex)
                {
                    string textPart = translatedTemplate.Substring(lastIndex, match.Index - lastIndex);

                    // 【關鍵修正】：手動轉義引號。
                    // 在內插字串節點中，內部的引號必須寫成 \" 才能編譯通過。
                    string escapedText = textPart.Replace("\"", "\\\"");

                    contents.Add(SyntaxFactory.InterpolatedStringText(
                        SyntaxFactory.Token(
                            SyntaxFactory.TriviaList(),
                            SyntaxKind.InterpolatedStringTextToken,
                            escapedText, // 這裡必須是轉義後的字串 (text)
                            textPart,    // 這裡可以是原始值 (value)
                            SyntaxFactory.TriviaList())));
                }

                // 插入原始的變數節點 (如 {plan.Name})
                int idx = int.Parse(match.Groups[1].Value);
                if (idx < interpolations.Count)
                {
                    contents.Add(interpolations[idx]);
                }
                lastIndex = match.Index + match.Length;
            }

            // 處理剩餘的文字
            if (lastIndex < translatedTemplate.Length)
            {
                string rest = translatedTemplate.Substring(lastIndex);
                string escapedRest = rest.Replace("\"", "\\\"");

                contents.Add(SyntaxFactory.InterpolatedStringText(
                    SyntaxFactory.Token(
                        SyntaxFactory.TriviaList(),
                        SyntaxKind.InterpolatedStringTextToken,
                        escapedRest,
                        rest,
                        SyntaxFactory.TriviaList())));
            }

            return SyntaxFactory.InterpolatedStringExpression(
                SyntaxFactory.Token(SyntaxKind.InterpolatedStringStartToken),
                SyntaxFactory.List(contents),
                SyntaxFactory.Token(SyntaxKind.InterpolatedStringEndToken));
        }
    }
}
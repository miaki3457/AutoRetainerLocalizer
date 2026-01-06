using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace Localizer
{
    public class TranslationRewriter : CSharpSyntaxRewriter
    {
        private readonly Dictionary<string, string> _dictionary;
        private readonly string[] _uiKeywords = { "Text", "Button", "Label", "Combo", "Header", "Section", "Tooltip", "MenuItem", "Checkbox", "Help", "Notify", "Info", "FormatToken", "InputInt", "Widget", "EnumCombo", "TextWrapped" };

        public TranslationRewriter(Dictionary<string, string> dictionary, string apiKey = "")
        {
            _dictionary = dictionary;
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
                if (TryGetTranslation(templateKey, out var translated))
                {
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
                    if (TryGetTranslation(originalText, out var translated))
                    {
                        return SyntaxFactory.LiteralExpression(SyntaxKind.StringLiteralExpression, SyntaxFactory.Literal(translated));
                    }
                }
            }
            return base.VisitLiteralExpression(node);
        }

        private bool TryGetTranslation(string key, out string translated)
        {
            return _dictionary.TryGetValue(key, out translated) ||
                   _dictionary.TryGetValue(key.Trim(), out translated) ||
                   _dictionary.TryGetValue(NormalizeKey(key), out translated);
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

            return false;
        }

        private string NormalizeKey(string text) => Regex.Replace(text, @"\s+", " ").Trim();

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

            // 1. 偵測是否為 Raw String
            bool isRawString = node.StringStartToken.Text.Contains("\"\"\"");
            
            // 2. 獲取基礎縮排
            string indentation = "";
            if (isRawString)
            {
                var lineSpan = node.StringEndToken.GetLocation().GetLineSpan();
                indentation = new string(' ', lineSpan.StartLinePosition.Character);
            }

            foreach (Match match in matches)
            {
                if (match.Index > lastIndex)
                {
                    string textPart = translatedTemplate.Substring(lastIndex, match.Index - lastIndex);
                    contents.Add(CreateTextContent(textPart, isRawString, indentation, isFirstPart: lastIndex == 0));
                }

                int idx = int.Parse(match.Groups[1].Value);
                if (idx < interpolations.Count) contents.Add(interpolations[idx]);
                lastIndex = match.Index + match.Length;
            }

            if (lastIndex < translatedTemplate.Length)
            {
                string rest = translatedTemplate.Substring(lastIndex);
                contents.Add(CreateTextContent(rest, isRawString, indentation, isFirstPart: lastIndex == 0));
            }

            return SyntaxFactory.InterpolatedStringExpression(node.StringStartToken, SyntaxFactory.List(contents), node.StringEndToken);
        }

        private InterpolatedStringContentSyntax CreateTextContent(string text, bool isRawString, string indentation, bool isFirstPart)
        {
            string finalValue = text;

            if (isRawString)
            {
                // 處理多行縮排核心邏輯
                var lines = text.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.None);
                for (int i = 0; i < lines.Length; i++)
                {
                    if (i > 0 || (isFirstPart && string.IsNullOrEmpty(lines[i]) && lines.Length > 1))
                    {
                        lines[i] = indentation + lines[i];
                    }
                }
                finalValue = string.Join(Environment.NewLine, lines);
            }

            return SyntaxFactory.InterpolatedStringText(
                SyntaxFactory.Token(
                    SyntaxFactory.TriviaList(),
                    SyntaxKind.InterpolatedStringTextToken,
                    finalValue,
                    isRawString ? finalValue : finalValue.Replace("\"", "\\\""),
                    SyntaxFactory.TriviaList()));
        }
    }
}

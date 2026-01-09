using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;

namespace Localizer
{
    public class TranslationRewriter : CSharpSyntaxRewriter
    {
        private readonly Dictionary<string, string> _dictionary;
        private readonly string _jsonPath;
        // 使用 HashSet 確保單次運行中，同一個新字串只會被記錄一次
        public HashSet<string> MissingTranslations { get; } = new HashSet<string>();

        private readonly string[] _uiKeywords = { "Text", "Button", "Label", "Combo", "Header", "Section", "Tooltip", "MenuItem", "Checkbox", "Help", "Notify", "Info", "FormatToken", "InputInt", "Widget", "EnumCombo" };
        private readonly string[] _blackList = { "PushID", "GetConfig", "Log", "Debug", "Print", "ExecuteCommand", "ToString", "GetField", "GetProperty", "SetFilter", "Tag", "GetTag", "InternalName", "Database", "HasTag", "AddTag", "Find" };

        public TranslationRewriter(Dictionary<string, string> dictionary, string jsonPath)
        {
            _dictionary = dictionary;
            _jsonPath = jsonPath;
        }

        // --- 核心翻譯與記錄邏輯 ---

        private bool TryGetTranslation(string original, out string translated)
        {
            if (string.IsNullOrWhiteSpace(original))
            {
                translated = null;
                return false;
            }

            // 嘗試三種匹配模式
            if (_dictionary.TryGetValue(original, out translated) ||
                _dictionary.TryGetValue(original.Trim(), out translated) ||
                _dictionary.TryGetValue(NormalizeKey(original), out translated))
            {
                return true;
            }

            // 若找不到翻譯，記錄到缺失清單
            if (!_dictionary.ContainsKey(original))
            {
                MissingTranslations.Add(original);
            }
            return false;
        }

        /// <summary>
        /// 將所有新發現的字串寫回 JSON 檔案
        /// </summary>
        public void SaveMissingTranslations()
        {
            if (MissingTranslations.Count == 0) return;

            try
            {
                JObject json;
                if (File.Exists(_jsonPath))
                {
                    string content = File.ReadAllText(_jsonPath);
                    json = JObject.Parse(content);
                }
                else
                {
                    json = new JObject();
                }

                bool added = false;
                foreach (var key in MissingTranslations)
                {
                    if (json[key] == null)
                    {
                        // 預設將 Value 設為 Key，方便後續搜尋與翻譯
                        json[key] = key;
                        added = true;
                    }
                }

                if (added)
                {
                    // 使用 Indented 格式讓 JSON 易於閱讀
                    File.WriteAllText(_jsonPath, json.ToString(Formatting.Indented), Encoding.UTF8);
                    Console.WriteLine($"[INFO] 已將 {MissingTranslations.Count} 個新字串寫入 {_jsonPath}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERROR] 儲存 JSON 時發生錯誤: {ex.Message}");
            }
        }

        // --- Roslyn 節點訪問覆寫 ---

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
                        return SyntaxFactory.LiteralExpression(SyntaxKind.StringLiteralExpression, SyntaxFactory.Literal(translated))
                            // .WithLeadingTrivia(node.GetLeadingTrivia())
                            .WithTrailingTrivia(node.GetTrailingTrivia());
                    }
                }
            }
            return base.VisitLiteralExpression(node);
        }

        // --- 輔助判斷方法 ---

        private bool ShouldTranslate(SyntaxNode node, string text)
        {
            if (string.IsNullOrWhiteSpace(text)) return false;

            var invocation = node.Ancestors().OfType<InvocationExpressionSyntax>().FirstOrDefault();
            if (text.StartsWith("##") || text.StartsWith("Component") || text.StartsWith("\u")) return false;
            if (invocation != null)
            {
                string methodName = GetMethodName(invocation);
                if (_blackList.Any(b => methodName.Equals(b, StringComparison.OrdinalIgnoreCase))) return false;
                if (_uiKeywords.Any(k => methodName.Contains(k))) return true;
            }

            // 檢查方法定義名稱
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

            // 檢查屬性名稱
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
            string clean = text.Trim(' ', '\n', '\r', '\"', '\\', 't', '#', '_'); // 增加過濾符號
            if (clean.Length <= 1) return false; // 太短的通常是代號或符號        
            if (char.IsLower(clean[0])) return false; // 過濾字首小寫
            // 判斷是否含有字母或中文字元
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
                // 預設給予 12 格縮排
                leadingWhitespace = new string(' ', 12);
            }

            var matches = Regex.Matches(translatedTemplate, @"\{(\d+)\}");
            int lastIndex = 0;

            string ProcessText(string rawText, bool isFirstPart)
            {
                if (string.IsNullOrEmpty(rawText)) return rawText;
                if (!isRawString) return rawText.Replace("\"", "\\\"");

                var lines = rawText.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.None);
                for (int i = 0; i < lines.Length; i++)
                {
                    lines[i] = lines[i].TrimStart();
                }

                string joined = string.Join("\n" + leadingWhitespace, lines);
                return isFirstPart ? leadingWhitespace + joined : joined;
            }

            bool isFirst = true;

            foreach (Match match in matches)
            {
                if (match.Index > lastIndex)
                {
                    string textPart = translatedTemplate.Substring(lastIndex, match.Index - lastIndex);
                    string finalPart = ProcessText(textPart, isFirst);
                    isFirst = false;

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
                node.StringEndToken)
                // .WithLeadingTrivia(node.GetLeadingTrivia())   // 前導空格
                .WithTrailingTrivia(node.GetTrailingTrivia()); // 後繼空格
        }
    }
}

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Localizer; 
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Newtonsoft.Json;

class Program
{
    static void Main(string[] args)
    {
        string rootPath = Environment.CurrentDirectory;
        
        if (!Directory.Exists(Path.Combine(rootPath, "AutoRetainer")))
        {
            rootPath = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..", "..", ".."));
        }

        string sourcePath = Path.Combine(rootPath, "AutoRetainer", "AutoRetainer", "UI");
        string dictPath = Path.Combine(rootPath, "zh-TW.json");

        Console.WriteLine($"[資訊] 當前工作目錄: {Environment.CurrentDirectory}");
        Console.WriteLine($"[資訊] 預計源碼路徑: {sourcePath}");

        if (!Directory.Exists(sourcePath))
        {
            Console.WriteLine($"[錯誤] 找不到 AutoRetainer 資料夾！");
            return;
        }

        // 讀取現有的字典
        var dictionary = File.Exists(dictPath)
            ? JsonConvert.DeserializeObject<Dictionary<string, string>>(File.ReadAllText(dictPath))
            : new Dictionary<string, string>();

        var files = Directory.GetFiles(sourcePath, "*.cs", SearchOption.AllDirectories);
        Console.WriteLine($"找到 {files.Length} 個檔案，準備開始掃描...");

        // 1: 建立一個持久的 rewriter 實例
        // 這樣 MissingTranslations 可以在處理所有檔案時持續累積
        var rewriter = new TranslationRewriter(dictionary ?? new(), dictPath);

        foreach (var file in files)
        {
            string code = File.ReadAllText(file);
            SyntaxTree tree = CSharpSyntaxTree.ParseText(code);
            var root = tree.GetRoot();

            // 2: 使用同一個 rewriter 進行 Visit
            var result = rewriter.Visit(root);

            if (result != root)
            {
                File.WriteAllText(file, result.ToFullString());
                Console.WriteLine($"[已更新] {Path.GetRelativePath(rootPath, file)}");
            }
        }

        // 3: 處理完所有檔案後，一次性寫入未翻譯字串
        Console.WriteLine("正在檢查是否有新發現的字串需寫入字典...");
        rewriter.SaveMissingTranslations();

        Console.WriteLine("中文化處理與字典更新完成！");
    }
}

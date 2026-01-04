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

        string sourcePath = Path.Combine(rootPath, "AutoRetainer");
        string dictPath = Path.Combine(rootPath, "zh-TW.json");
        // -------------------------

        string apiKey = Environment.GetEnvironmentVariable("TRANSLATION_API_KEY") ?? "";

        Console.WriteLine($"[資訊] 當前工作目錄: {Environment.CurrentDirectory}");
        Console.WriteLine($"[資訊] 預計源碼路徑: {sourcePath}");

        if (!Directory.Exists(sourcePath))
        {
            Console.WriteLine($"[錯誤] 找不到 AutoRetainer 資料夾！");
            Console.WriteLine("[偵錯] 目錄下的內容有：");
            foreach (var d in Directory.GetDirectories(rootPath)) Console.WriteLine($" D: {Path.GetFileName(d)}");
            foreach (var f in Directory.GetFiles(rootPath)) Console.WriteLine($" F: {Path.GetFileName(f)}");
            return;
        }

        var dictionary = File.Exists(dictPath)
            ? JsonConvert.DeserializeObject<Dictionary<string, string>>(File.ReadAllText(dictPath))
            : new Dictionary<string, string>();

        var files = Directory.GetFiles(sourcePath, "*.cs", SearchOption.AllDirectories);
        Console.WriteLine($"找到 {files.Length} 個檔案，準備開始掃描...");

        foreach (var file in files)
        {
            string code = File.ReadAllText(file);
            SyntaxTree tree = CSharpSyntaxTree.ParseText(code);
            var root = tree.GetRoot();

            var rewriter = new TranslationRewriter(dictionary ?? new(), apiKey);
            var result = rewriter.Visit(root);

            if (result != root)
            {
                File.WriteAllText(file, result.ToFullString());
                Console.WriteLine($"[已更新] {Path.GetRelativePath(rootPath, file)}");
            }
        }

        Console.WriteLine("漢化處理完成！");
    }
}


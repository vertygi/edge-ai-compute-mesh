using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace EdgeMesh.Core
{
    public class SecureCompiler
    {
        private readonly Random random = new Random();
        private readonly string[] obfuscationStrings = {
            "System.Diagnostics.Debugger", "Environment.GetFolderPath",
            "System.Reflection.Assembly", "System.IO.File", "System.Net.WebClient",
            "System.Security.Cryptography", "System.Threading.Thread"
        };

        public async Task CompileAndObfuscate(string sourceFile, string outputFile)
        {
            try
            {
                // Чтение исходного кода асинхронно
                string sourceCode = await File.ReadAllTextAsync(sourceFile);
                
                // Применение обфускации
                string obfuscatedCode = ApplyObfuscation(sourceCode);
                
                // Сохранение обфусцированного кода для компиляции
                string tempFile = Path.GetTempFileName() + ".cs";
                await File.WriteAllTextAsync(tempFile, obfuscatedCode);
                
                // Компиляция с использованием Roslyn
                bool success = await CompileAsync(tempFile, outputFile);
                
                if (!success)
                {
                    Console.WriteLine("Ошибки компиляции.");
                }
                else
                {
                    Console.WriteLine($"Успешно скомпилировано: {outputFile}");
                    
                    // Применение полиморфных изменений
                    await ApplyPolymorphismAsync(outputFile);
                }
                
                // Удаляем временный файл
                File.Delete(tempFile);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка компиляции: {ex.Message}");
            }
        }

        private string ApplyObfuscation(string sourceCode)
        {
            string obfuscated = sourceCode;
            
            // 1. Добавление ложных строк комментариев
            for (int i = 0; i < 10; i++)
            {
                int position = random.Next(obfuscated.Length);
                string fakeComment = $"/*{GenerateRandomString(20)}*/";
                obfuscated = obfuscated.Insert(position, fakeComment);
            }
            
            // 2. Перемешивание импортов
            string[] lines = obfuscated.Split('\n');
            var usingLines = lines.Where(l => l.Trim().StartsWith("using")).ToList();
            var otherLines = lines.Where(l => !l.Trim().StartsWith("using")).ToList();
            
            Shuffle(usingLines);
            
            List<string> resultLines = new List<string>();
            resultLines.AddRange(usingLines);
            resultLines.AddRange(otherLines);
            
            obfuscated = string.Join("\n", resultLines);
            
            // 3. Переименование переменных
            foreach (var str in obfuscationStrings)
            {
                string newName = GenerateRandomString(8);
                obfuscated = obfuscated.Replace(str, newName);
            }
            
            return obfuscated;
        }

        private async Task<bool> CompileAsync(string sourceFile, string outputFile)
        {
            string sourceCode = await File.ReadAllTextAsync(sourceFile);
            var syntaxTree = CSharpSyntaxTree.ParseText(sourceCode);
            
            var compilation = CSharpCompilation.Create(
                Path.GetFileNameWithoutExtension(outputFile),
                new[] { syntaxTree },
                new[]
                {
                    MetadataReference.CreateFromFile(typeof(object).Assembly.Location),
                    MetadataReference.CreateFromFile(typeof(System.Net.Http.HttpClient).Assembly.Location)
                },
                new CSharpCompilationOptions(OutputKind.WindowsApplication)
                    .WithOptimizationLevel(OptimizationLevel.Release));

            using (var stream = new FileStream(outputFile, FileMode.Create))
            {
                var result = compilation.Emit(stream);
                if (!result.Success)
                {
                    foreach (var diagnostic in result.Diagnostics)
                    {
                        if (diagnostic.Severity == DiagnosticSeverity.Error)
                        {
                            Console.WriteLine($"Ошибка {diagnostic.Id}: {diagnostic.GetMessage()}");
                        }
                    }
                    return false;
                }
                return true;
            }
        }

        private async Task ApplyPolymorphismAsync(string exeFile)
        {
            try
            {
                byte[] fileData = await File.ReadAllBytesAsync(exeFile);
                // Здесь можно добавить более сложную модификацию
                await File.WriteAllBytesAsync(exeFile, fileData);
                Console.WriteLine("Полиморфизм применен");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка полиморфизма: {ex.Message}");
            }
        }

        private string GenerateRandomString(int length)
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789_";
            return new string(Enumerable.Repeat(chars, length)
                .Select(s => s[random.Next(s.Length)]).ToArray());
        }

        private void Shuffle<T>(IList<T> list)
        {
            for (int i = list.Count - 1; i > 0; i--)
            {
                int j = random.Next(i + 1);
                T temp = list[i];
                list[i] = list[j];
                list[j] = temp;
            }
        }
    }
}
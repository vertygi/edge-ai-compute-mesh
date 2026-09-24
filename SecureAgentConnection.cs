using System;
using System.Net.Sockets;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Net.Security;
using System.IO;

namespace EdgeMesh.Core
{
    public class SecureAgentConnection
    {
        private readonly TcpClient client;
        private readonly Stream stream; // Changed from NetworkStream to Stream
        private readonly SslStream sslStream;
        private string sessionKey;
        private readonly RSACryptoServiceProvider rsaProvider = new RSACryptoServiceProvider();

        public SecureAgentConnection(TcpClient client, SslStream sslStream)
        {
            this.client = client;
            this.sslStream = sslStream;
            this.stream = sslStream; // Now valid, as both are Stream
        }

        public TcpClient Client => client;

        public async Task<bool> Authenticate()
        {
            try
            {
                // Отправка публичного ключа
                string publicKey = rsaProvider.ToXmlString(false);
                byte[] keyBytes = Encoding.UTF8.GetBytes(publicKey);
                byte[] lengthBytes = BitConverter.GetBytes(keyBytes.Length);
                await stream.WriteAsync(lengthBytes, 0, 4);
                await stream.WriteAsync(keyBytes, 0, keyBytes.Length);

                // Получение сессионного ключа
                lengthBytes = new byte[4];
                await stream.ReadAsync(lengthBytes, 0, 4);
                int length = BitConverter.ToInt32(lengthBytes, 0);
                byte[] encryptedKey = new byte[length];
                await stream.ReadAsync(encryptedKey, 0, length);

                byte[] decryptedKey = rsaProvider.Decrypt(encryptedKey, false);
                sessionKey = Encoding.UTF8.GetString(decryptedKey);

                // Отправка публичного ключа для подписи
                string signingPublicKey = rsaProvider.ToXmlString(false);
                keyBytes = Encoding.UTF8.GetBytes(signingPublicKey);
                lengthBytes = BitConverter.GetBytes(keyBytes.Length);
                await stream.WriteAsync(lengthBytes, 0, 4);
                await stream.WriteAsync(keyBytes, 0, keyBytes.Length);

                return true;
            }
            catch (Exception ex)
            {
                await Logger.LogAsync($"Ошибка аутентификации: {ex.Message}");
                return false;
            }
        }

        public async Task ProcessCommands()
        {
            try
            {
                while (client.Connected)
                {
                    string encryptedMessage = await ReceiveEncryptedMessage();
                    if (string.IsNullOrEmpty(encryptedMessage))
                        break;

                    string message = DecryptString(encryptedMessage, sessionKey);
                    await Logger.LogAsync($"Получено сообщение: {message}");

                    string response = $"Получено: {message}";
                    string encryptedResponse = EncryptString(response, sessionKey);
                    await SendEncryptedMessage(encryptedResponse);

                    await Task.Delay(1000);
                }
            }
            catch (Exception ex)
            {
                await Logger.LogAsync($"Ошибка обработки команд: {ex.Message}");
            }
        }

        public async Task<string> ReceiveEncryptedMessage()
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

        public async Task SendEncryptedMessage(string message)
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

        public string EncryptString(string plainText, string key)
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

        public string DecryptString(string cipherText, string key)
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
}
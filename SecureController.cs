using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Net.Security;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using System.Linq;

namespace EdgeMesh.Core
{
    public class SecureController
    {
        // Хранение статуса агентов
        private class AgentStatus
        {
            public string AgentId { get; set; }
            public bool Online { get; set; }
            public DateTime LastPing { get; set; }
            public double CpuUsage { get; set; }
            public double RamUsage { get; set; }
            public List<string> Logs { get; set; } = new List<string>();
        }
        private readonly Dictionary<string, AgentStatus> agentStatuses = new Dictionary<string, AgentStatus>();

        private TcpListener listener;
        private List<SecureAgentConnection> connectedAgents = new List<SecureAgentConnection>();
        private readonly object lockObject = new object();
        private readonly RSACryptoServiceProvider rsaForSigning = new RSACryptoServiceProvider();

        // Новый метод: получить список всех агентов и их статусы
        public async Task StartListening()
        {
            try
            {
                listener = new TcpListener(IPAddress.Any, 8443);
                listener.Start();
                await Logger.LogAsync("EdgeMesh Controller started на порту 8443");
                
                while (true)
                {
                    TcpClient client = await listener.AcceptTcpClientAsync();
                    _ = HandleAgentConnection(client);
                }
            }
            catch (Exception ex)
            {
                await Logger.LogAsync($"EdgeMesh Controller error: {ex.Message}");
            }
        }

        private async Task HandleAgentConnection(TcpClient client)
        {
            try
            {
                // Используйте SslStream для шифрования
                var sslStream = new SslStream(client.GetStream(), false);
                X509Certificate2 cert = new X509Certificate2("server.pfx", "password");
                await sslStream.AuthenticateAsServerAsync(cert, false, false);

                var agent = new SecureAgentConnection(client, sslStream);
                string agentId = client.Client.RemoteEndPoint.ToString();
                lock (lockObject)
                {
                    connectedAgents.Add(agent);
                    // Новый статус
                    agentStatuses[agentId] = new AgentStatus
                    {
                        AgentId = agentId,
                        Online = true,
                        LastPing = DateTime.Now,
                        CpuUsage = 0,
                        RamUsage = 0
                    };
                }
                await Logger.LogAsync($"Новое подключение от {agentId}");

                if (await agent.Authenticate())
                {
                    await Logger.LogAsync($"Агент {agentId} успешно аутентифицирован");
                    OnAgentConnected?.Invoke(agentId);
                    await agent.ProcessCommands();
                }
                else
                {
                    await Logger.LogAsync($"Ошибка аутентификации агента {agentId}");
                    lock (lockObject)
                    {
                        agentStatuses[agentId].Online = false;
                    }
                }

                lock (lockObject)
                {
                    connectedAgents.Remove(agent);
                    agentStatuses[agentId].Online = false;
                    OnAgentDisconnected?.Invoke(agentId);
                }
            }
            catch (Exception ex)
            {
                string agentId = client.Client.RemoteEndPoint.ToString();
                lock (lockObject)
                {
                    if (agentStatuses.ContainsKey(agentId))
                        agentStatuses[agentId].Logs.Add($"Ошибка: {ex.Message}");
                }
                await Logger.LogAsync($"Ошибка обработки подключения: {ex.Message}");
            }
        }

        // Новый метод: получить список всех агентов и их статусы
        public List<object> GetAllAgentStatuses()
        {
            lock (lockObject)
            {
                return agentStatuses.Values.Select(a => new {
                    a.AgentId,
                    a.Online,
                    a.LastPing,
                    a.CpuUsage,
                    a.RamUsage
                }).ToList<object>();
            }
        }

        // Новый метод: получить логи агента
        public List<string> GetAgentLogs(string agentId)
        {
            lock (lockObject)
            {
                if (agentStatuses.ContainsKey(agentId))
                    return new List<string>(agentStatuses[agentId].Logs);
                return new List<string>();
            }
        }

        // Новый метод: обновить загрузку агента (вызывать из ProcessCommands агента)
        public void UpdateAgentLoad(string agentId, double cpu, double ram)
        {
            lock (lockObject)
            {
                if (agentStatuses.ContainsKey(agentId))
                {
                    agentStatuses[agentId].CpuUsage = cpu;
                    agentStatuses[agentId].RamUsage = ram;
                    agentStatuses[agentId].LastPing = DateTime.Now;
                }
            }
        }

        public async Task SendCommandToAgent(string agentId, string command)
        {
            try
            {
                SecureAgentConnection targetAgent = null;
                lock (lockObject)
                {
                    targetAgent = connectedAgents.Find(agent => agent.Client.Client.RemoteEndPoint.ToString() == agentId);
                }

                if (targetAgent != null)
                {
                    byte[] commandBytes = Encoding.UTF8.GetBytes(command);
                    byte[] signature = rsaForSigning.SignData(commandBytes, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);
                    string signedCommand = $"{Convert.ToBase64String(commandBytes)}|{Convert.ToBase64String(signature)}";

                    await targetAgent.SendEncryptedMessage(targetAgent.EncryptString(signedCommand, targetAgent.SessionKey()));
                    await Logger.LogAsync($"Команда отправлена агенту {agentId}: {command}");
                }
                else
                {
                    await Logger.LogAsync($"Агент {agentId} не найден");
                }
            }
            catch (Exception ex)
            {
                await Logger.LogAsync($"Ошибка отправки команды агенту {agentId}: {ex.Message}");
            }
        }

        public async Task SendUpdateToAgent(string agentId, string updateFilePath)
        {
            try
            {
                SecureAgentConnection targetAgent = null;
                lock (lockObject)
                {
                    targetAgent = connectedAgents.Find(agent => agent.Client.Client.RemoteEndPoint.ToString() == agentId);
                }

                if (targetAgent != null)
                {
                    byte[] updateBytes = await File.ReadAllBytesAsync(updateFilePath);
                    string updateData = Convert.ToBase64String(updateBytes);
                    string command = $"update:{updateData}";

                    byte[] commandBytes = Encoding.UTF8.GetBytes(command);
                    byte[] signature = rsaForSigning.SignData(commandBytes, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);
                    string signedCommand = $"{Convert.ToBase64String(commandBytes)}|{Convert.ToBase64String(signature)}";

                    await targetAgent.SendEncryptedMessage(targetAgent.EncryptString(signedCommand, targetAgent.SessionKey()));
                    await Logger.LogAsync($"Обновление отправлено агенту {agentId}");
                }
                else
                {
                    await Logger.LogAsync($"Агент {agentId} не найден");
                }
            }
            catch (Exception ex)
            {
                await Logger.LogAsync($"Ошибка отправки обновления агенту {agentId}: {ex.Message}");
            }
        }

        public async Task SendSelfInstallCommand(string agentId)
        {
            await SendCommandToAgent(agentId, "selfinstall");
        }

        public async Task SendSelfUninstallCommand(string agentId)
        {
            await SendCommandToAgent(agentId, "selfuninstall");
        }

        public string GetPublicKeyForSigning()
        {
            return rsaForSigning.ToXmlString(false);
        }

        public event Action<string> OnAgentConnected;
        public event Action<string> OnAgentDisconnected;

        public string GetAgentSessionKey(string agentId)
        {
            lock (lockObject)
            {
                var agent = connectedAgents.Find(a => a.Client.Client.RemoteEndPoint.ToString() == agentId);
                return agent?.SessionKey();
            }
        }
    }

    public static class SecureAgentConnectionExtensions
    {
        public static string SessionKey(this SecureAgentConnection agent)
        {
            return typeof(SecureAgentConnection).GetField("sessionKey", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).GetValue(agent).ToString();
        }
    }
}
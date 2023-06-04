using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using EmbedIO;
using EmbedIO.Actions;

namespace UdpMouse
{
    public class UdpMouseWebServer
    {
        private MainWindow main;
        public UdpMouseWebServer(int port, MainWindow mainWindow)
        {
            this.main = mainWindow;
            try
            {
                var url = "http://*:" + port + "/";
                using (var cts = new CancellationTokenSource())
                {
                    Task serverTask = this.RunWebServerAsync(url, cts.Token, port);
                    serverTask.RunSynchronously();
                }
            }
            catch (Exception e)
            {

            }
        }

        private WebServer server;
        private async Task RunWebServerAsync(string url, CancellationToken cancellationToken, int port)
        {
            this.server = this.CreateWebServer(url, port);
            await this.server.RunAsync(cancellationToken).ConfigureAwait(false);
        }

        private WebServer CreateWebServer(string url, int port)
        {
#pragma warning disable CA2000 // Call Dispose on object - this is a factory method.
            WebServer server = new WebServer(o => o
                    .WithUrlPrefix(url)
                    .WithMode(HttpListenerMode.EmbedIO))
                    .WithLocalSessionManager()
                    .WithModule(new ActionModule("/", HttpVerbs.Get, ctx =>
                    {
                        IHttpRequest request = ctx.Request;
                        NameValueCollection queryData = ctx.GetRequestQueryData();
                        ctx.Response.Headers.Add("Server", "UDP Mouse Web server");
                        switch (request.Url.AbsolutePath)
                        {
                            case "/":
                                return this.SendFile(ctx, "index.html");
                            case "/move":
                                this.main.DoMouseAction(int.Parse(queryData["x"]), int.Parse(queryData["y"]), InputCommand.InputAction.Move);
                                return ctx.SendDataAsync("MOVE");
                            case "/click":
                                this.main.DoMouseAction(int.Parse(queryData["x"]), int.Parse(queryData["y"]), InputCommand.InputAction.Click);
                                return ctx.SendDataAsync("CLICK");
                            case "/dblclick":
                                this.main.DoMouseAction(int.Parse(queryData["x"]), int.Parse(queryData["y"]), InputCommand.InputAction.DblClick);
                                return ctx.SendDataAsync("DBCLICK");
                            default:
                                return this.SendFile(ctx, "index.html");
                        }
                        
                    }));

            return server;
        }

        public async Task SendFile(IHttpContext ctx, string absoluteHtmlPath)
        {
            string localFilePath = GetLocalFilePath(absoluteHtmlPath);
            if (File.Exists(localFilePath))
            {
                await this.SendBytes(ctx, File.ReadAllBytes(localFilePath), GetContentType(localFilePath));
            }
            else
            {
                await ctx.SendDataAsync("file does not exist: " + absoluteHtmlPath);
            }
        }

        public static string GetLocalFilePath(string absoluteHtmlPath)
        {
            return string.Format("{0}/Html/{1}", @AppDomain.CurrentDomain.BaseDirectory, absoluteHtmlPath);
        }

        public static string GetContentType(string fileName)
        {
            return MimeMapping.MimeUtility.GetMimeMapping(fileName);
        }

        public async Task SendBytes(IHttpContext ctx, byte[] data, string contentType = "image/jpg")
        {
            if (data != null && data.Length > 0)
            {
                ctx.Response.ContentType = contentType;
                using (var stream = ctx.OpenResponseStream())
                {
                    await stream.WriteAsync(data, 0, data.Length);
                }
            }
            else
            {
                await ctx.SendDataAsync("invalid data");
            }
        }
    }


}

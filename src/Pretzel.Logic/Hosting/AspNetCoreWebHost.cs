using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using System.IO;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;

namespace Pretzel.Logic.Hosting
{
    public class AspNetCoreWebHost : IDisposable
    {
        IWebHost webHost;
        public int Port { get; }
        public bool IsRunning { get; private set; }
        public string BasePath { get; }
        public bool Debug { get; }

        public AspNetCoreWebHost(string basePath)
            : this(basePath, 8080, true)
        {
        }

        public AspNetCoreWebHost(string basePath, int port, bool debug)
        {
            IsRunning = false;
            BasePath = string.IsNullOrWhiteSpace(basePath) ? Directory.GetCurrentDirectory() : Path.GetFullPath(basePath);
            Port = port;
            Debug = debug;
        }

        public async Task<bool> Start()
        {
            if (IsRunning)
            {
                return false;
            }

            webHost = WebHost.CreateDefaultBuilder()
                .ConfigureLogging(l =>
                {
                    if (!Debug) l.ClearProviders();
                })
                .UseUrls($"https://localhost:{Port}")
                .Configure(config => config
                    .UseHttpsRedirection()
                    .UseDefaultFiles()
                    .UseStaticFiles()
                )
                .UseWebRoot(BasePath).Build();

            await webHost.StartAsync();
            IsRunning = true;

            return true;
        }

        public async Task<bool> Stop()
        {
            if (!IsRunning)
            {
                return false;
            }

            await webHost?.StopAsync();
            webHost?.Dispose();
            webHost = null;

            return true;
        }

        #region IDisposable
        private bool isDisposed;

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing && !isDisposed)
            {
                webHost?.Dispose();

                IsRunning = false;
                isDisposed = true;
            }
        }

        ~AspNetCoreWebHost()
        {
            Dispose(false);
        }

        #endregion
    }
}

using System;
using System.Net.Http;
using System.Text;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;

/*
 * IMPORTANT:
 * This sample requires C# 7.1 or later.
 * Requires the following Nuget Packages: 
 *      Microsoft.Extensions.Hosting, Version 2.1.1
 *      Microsoft.Extensions.Hosting.Abstractions, Version 2.1.1
 *      Microsoft.Extensions.Http, Version 2.1.1
 */

namespace AudioSample
{
    public class Program
    {
        public static async Task Main(string[] args)
        {

            var hostBuilder = new HostBuilder()
                .ConfigureServices((hostContext, services) =>
                {
                    services.AddHttpClient();
                    services.AddSingleton<IHostedService, AudioService>();
                });

            await hostBuilder.RunConsoleAsync();
        }
    }
}
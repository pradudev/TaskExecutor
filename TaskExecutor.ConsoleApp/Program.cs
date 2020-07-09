using Microsoft.Extensions.Configuration;
using Serilog;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Threading.Tasks;
using TaskExecutor.ConsoleApp.ConfigModels;

namespace TaskExecutor.ConsoleApp
{
    internal class Program
    {
        public static void Main(string[] args)
        {
            // Initialize .Net Core configuration
            IConfiguration config = new ConfigurationBuilder()
                      .AddJsonFile("appsettings.json", false, true)
                      .Build();

            var commands = config.GetSection("Commands").Get<List<CommandConfig>>();

            // Initialize serilog logger
            string assemblyPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            Log.Logger = new LoggerConfiguration()
                 .WriteTo.Console(Serilog.Events.LogEventLevel.Debug)
                 .WriteTo.File(Path.Join(assemblyPath, "logs/log-.txt"),
                    rollingInterval: RollingInterval.Day,
                    outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] {Message:lj}{NewLine}{Exception}")
                 .MinimumLevel.Debug()
                 .Enrich.FromLogContext()
                 .CreateLogger();

            // For testing
            //args = new string[] { "A001" };

            if (args.Length == 0)
            {
                Log.Error("No argument was supplied");
                return;
            }

            try
            {
                Execute(args[0], commands, Log.Logger).Wait();
                Log.Information("TaskExecutor completed");
            }
            catch (Exception ex)
            {
                Log.Error(ex, "An error accoured");
            }

            // Console.ReadLine();
        }

        private async static Task Execute(string cmd, List<CommandConfig> commands, ILogger logger)
        {
            var command = commands.FirstOrDefault(x =>
                x.Arg.Equals(cmd, StringComparison.InvariantCultureIgnoreCase)
                && x.IsActive);

            if (command == null)
            {
                logger.Error($"Invalid command: {cmd}");
            }

            var watch = new Stopwatch();
            watch.Start();
            using (var httpClient = new HttpClient())
            {
                httpClient.Timeout = new TimeSpan(0, command.TimeoutInMinutes, 0);

                logger.Information($"Sending GET request to {command.Url}");

                using (var resp = await httpClient.GetAsync(command.Url))
                {
                    logger.Information($"Response Status Code: {resp.StatusCode}");
                }
            }
            watch.Stop();
            logger.Information($"Total Response Time: {watch.ElapsedMilliseconds}ms");
        }
    }
}
using Elastic.Apm.SerilogEnricher;
using Elastic.CommonSchema.Serilog;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Serilog;
using Serilog.Events;
using Serilog.Sinks.Elasticsearch;
using System;
using System.IO;
using System.Linq;

namespace series_log_elasticsearch
{
    public class Program
    {
        public static readonly string EnvironmentName = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");
        public static readonly string ApmElasticSearchUrl = Environment.GetEnvironmentVariable("APM_ELASTICSEARCH_URL");
        public static readonly string ApmElasticUser = Environment.GetEnvironmentVariable("ELASTIC_USER");
        public static readonly string ApmElasticPassword = Environment.GetEnvironmentVariable("ELASTIC_PASSWORD");
        public static readonly string AppName = "series-log-elasticsearch";

        public static void Main(string[] args)
        {
            var configuration = GetConfiguration();
            Log.Logger = CreateSerilogLogger(AppName, configuration);

            try
            {
                Log.Information("Configuring web host ({AppName})...", AppName);
                var host = BuilWebdHost(args);

                Log.Information("Starting web host ({AppName})...", AppName);
                host.Run();
            }
            catch (Exception ex)
            {
                Log.Fatal(ex, "Program terminated unexpectedly ({AppName})!", AppName);
            }
            finally
            {
                Log.CloseAndFlush();
            }
        }

        public static IHost BuilWebdHost(string[] args)
        {
            return Host.CreateDefaultBuilder(args)
                .ConfigureAppConfiguration((builderContext, builder) =>
                {
                    BuildConfiguration(builder);
                })
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>();
                })
                .UseSerilog(Log.Logger)
                .Build(); ;
        }

        public static IConfiguration GetConfiguration(string[] extraConfigFolders = null)
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory());

            BuildConfiguration(builder, extraConfigFolders);

            return builder.Build();
        }

        public static void BuildConfiguration(IConfigurationBuilder builder, string[] extraConfigFolders = null)
        {
            if (extraConfigFolders != null)
                foreach (var folder in extraConfigFolders)
                {
                    if (Directory.Exists(folder) && Directory.EnumerateFiles(folder, "*.json", SearchOption.AllDirectories).Any())
                    {
                        foreach (var jsonFilename in Directory.EnumerateFiles(folder, "*.json", SearchOption.AllDirectories))
                            builder.AddJsonFile(jsonFilename);
                    }
                }

            builder.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);
            builder.AddJsonFile($"appsettings.{EnvironmentName}.json", optional: true, reloadOnChange: true);
            builder.AddJsonFile($"appsettings.local.json", optional: true, reloadOnChange: true);
            //builder.AddJsonFile($"message-channel.json", optional: true, reloadOnChange: true);
            builder.AddEnvironmentVariables();
        }

        public static Serilog.ILogger CreateSerilogLogger(string appName, IConfiguration configuration)
        {
            var loggerConfig = new LoggerConfiguration()
                .MinimumLevel.Verbose()
                .Enrich.WithProperty("AppName", appName)
                .Enrich.FromLogContext()
                .Enrich.WithProperty("Environment", EnvironmentName)
                .ReadFrom.Configuration(configuration)
                .Filter.ByExcluding(log =>
                {
                    if (log.Level == LogEventLevel.Error) return false;
                    LogEventPropertyValue name;
                    return log.Properties.TryGetValue("RequestPath", out name) && (name as ScalarValue)?.Value as string == "/ping";
                });
            Serilog.Debugging.SelfLog.Enable(Console.WriteLine);

            var writeToValue = Environment.GetEnvironmentVariable("WRITE_LOG_TO");

            if (Enum.TryParse(typeof(WriteLogTo), writeToValue, out var result) && result is WriteLogTo writeTo)
            {
                if ((writeTo & WriteLogTo.Console) != WriteLogTo.None)
                {
                    loggerConfig = loggerConfig.WriteTo.Console();
                }
                if ((writeTo & WriteLogTo.File) != WriteLogTo.None)
                {
                    var filePath = configuration["Serilog:FilePath"];
                    loggerConfig = loggerConfig.WriteTo.File(filePath
                        , outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] {Message:lj}{NewLine}{Exception}"
                        , rollingInterval: RollingInterval.Day
                        , rollOnFileSizeLimit: true
                        , shared: true
                        //, buffered: true
                        );
                }
                if ((writeTo & WriteLogTo.APM) != WriteLogTo.None
                    && !string.IsNullOrEmpty(ApmElasticSearchUrl)
                    && !string.IsNullOrEmpty(ApmElasticUser)
                    && !string.IsNullOrEmpty(ApmElasticPassword))
                {
                    loggerConfig = loggerConfig.Enrich.WithElasticApmCorrelationInfo()
                            .WriteTo.Elasticsearch(new ElasticsearchSinkOptions(new Uri(ApmElasticSearchUrl))
                            {
                                CustomFormatter = new EcsTextFormatter(),
                                AutoRegisterTemplate = false,
                                AutoRegisterTemplateVersion = AutoRegisterTemplateVersion.ESv7,
                                DetectElasticsearchVersion = true,
                                RegisterTemplateFailure = RegisterTemplateRecovery.IndexAnyway,
                                ModifyConnectionSettings = x => x.BasicAuthentication(ApmElasticUser, ApmElasticPassword),
                                FailureCallback = e => Console.WriteLine("Unable to submit event " + e.RenderMessage()),
                                EmitEventFailure = EmitEventFailureHandling.WriteToSelfLog |
                                       EmitEventFailureHandling.WriteToFailureSink |
                                       EmitEventFailureHandling.RaiseCallback,
                            });
                }
            }

            return loggerConfig.CreateLogger();
        }

    }

    public enum WriteLogTo
    {
        None = 0,
        APM = 1,
        File = 2,
        Console = 4,
    }
}

using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Hosting.Internal;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.UserSecrets;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.ObjectPool;
using System;
using System.Diagnostics;
using System.Reflection;
using Vecc.GhostTemplating.Client;
using Vecc.GhostTemplating.RazorSupport;

namespace Vecc.GhostTemplating
{
    public static class ServiceProvider
    {
        public static IServiceProvider Build(string[] args)
        {
            var services = new ServiceCollection();
            services.AddLogging(builder =>
            {
                builder.AddConsole();
                builder.SetMinimumLevel(LogLevel.Trace);
            });
            var assembly = typeof(Program).Assembly;
            var attribute = assembly.GetCustomAttribute<UserSecretsIdAttribute>();

            var configurationBuilder = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json", true)
                .AddUserSecrets<Program>(false)
                .AddEnvironmentVariables()
                .AddCommandLine(args);

            var configuration = configurationBuilder.Build();

            services.AddOptions();
            services.AddSingleton<IHostingEnvironment>(new HostingEnvironment { ApplicationName = Assembly.GetEntryAssembly()?.GetName().Name });
            services.AddSingleton((IServiceProvider serviceProvider) => new DiagnosticListener("DummySource"));
            services.AddSingleton<DiagnosticSource>((IServiceProvider serviceProvider) => new DiagnosticListener("DummySource"));
            services.AddSingleton<GhostClient>();
            services.AddTransient<ObjectPoolProvider, DefaultObjectPoolProvider>();
            services.Configure<TemplatingOptions>(configuration.GetSection(nameof(TemplatingOptions)));

            services.AddMvcCore()
                .AddRazorViewEngine(options =>
                {
                    options.AllowRecompilingViewsOnFileChange = false;
                    options.FileProviders.Add(new FileProvider());
                });

            return services.BuildServiceProvider();
        }
    }
}

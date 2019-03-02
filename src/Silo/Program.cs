﻿using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Orleans;
using Orleans.Hosting;
using Serilog;
using Serilog.Events;
using Silo.Options;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;

namespace Silo
{
    [ExcludeFromCodeCoverage]
    public static class Program
    {
        private const string EnvironmentVariablePrefix = "ORLEANS_";

        public static Task Main(string[] args)
        {
            var host = new HostBuilder()
                .ConfigureHostConfiguration(configure =>
                {
                    configure.AddJsonFile("hostsettings.json", true, true);
                    configure.AddEnvironmentVariables(EnvironmentVariablePrefix);
                    configure.AddCommandLine(args);
                })
                .ConfigureAppConfiguration((hosting, configure) =>
                {
                    configure
                        .AddJsonFile("appsettings.json", true, true)
                        .AddJsonFile($"appsettings.{hosting.HostingEnvironment.EnvironmentName}.json", true, true)
                        .AddEnvironmentVariables(EnvironmentVariablePrefix)
                        .AddCommandLine(args);
                })
                .ConfigureServices((hosting, services) =>
                {
                    // helps discover free ports
                    services.AddSingleton<INetworkPortFinder, NetworkPortFinder>();

                    // add options for the silo hosted service
                    services.Configure<SiloHostedServiceOptions>(_ =>
                    {
                        // configure general settings
                        _.SiloPortRange.Start = hosting.Configuration.GetValue<int>("Orleans:Ports:Silo:Start");
                        _.SiloPortRange.End = hosting.Configuration.GetValue<int>("Orleans:Ports:Silo:End");
                        _.GatewayPortRange.Start = hosting.Configuration.GetValue<int>("Orleans:Ports:Gateway:Start");
                        _.GatewayPortRange.End = hosting.Configuration.GetValue<int>("Orleans:Ports:Gateway:End");
                        _.DashboardPortRange.Start = hosting.Configuration.GetValue<int>("Orleans:Ports:Dashboard:Start");
                        _.DashboardPortRange.End = hosting.Configuration.GetValue<int>("Orleans:Ports:Dashboard:End");

                        
                        
                        // configure the clustering provider
                        _.ClusteringProvider = hosting.Configuration.GetValue<SiloHostedServiceClusteringProvider>(
                            "Orleans:Providers:Clustering:Provider");

                        // configure the adonet clustering provider
                        _.AdoNetClusteringConnectionString = hosting.Configuration.GetConnectionString(hosting.Configuration.GetValue<string>(
                            "Orleans:Providers:Clustering:AdoNet:ConnectionStringName"));
                        _.AdoNetClusteringInvariant = hosting.Configuration.GetValue<string>(
                            "Orleans:Providers:Clustering:AdoNet:Invariant");

                        
                        
                        // configure the default storage provider
                        _.DefaultStorageProvider = hosting.Configuration.GetValue<SiloHostedServiceStorageProvider>(
                            "Orleans:Providers:Storage:Default:Provider");

                        // configure the adonet default storage provider
                        _.AdoNetDefaultStorageConnectionString = hosting.Configuration.GetConnectionString(hosting.Configuration.GetValue<string>(
                            "Orleans:Providers:Storage:Default:AdoNet:ConnectionStringName"));
                        _.AdoNetDefaultStorageInvariant = hosting.Configuration.GetValue<string>(
                            "Orleans:Providers:Storage:Default:AdoNet:Invariant");



                        // configure the pubsub storage provider
                        _.PubSubStorageProvider = hosting.Configuration.GetValue<SiloHostedServiceStorageProvider>(
                            "Orleans:Providers:Storage:PubSub:Provider");

                        // configure the adonet pubsub storage provider
                        _.AdoNetPubSubStorageConnectionString = hosting.Configuration.GetConnectionString(hosting.Configuration.GetValue<string>(
                            "Orleans:Providers:Storage:PubSub:AdoNet:ConnectionStringName"));
                        _.AdoNetPubSubStorageInvariant = hosting.Configuration.GetValue<string>(
                            "Orleans:Providers:Storage:PubSub:AdoNet:Invariant");




                        _.ReminderProvider = SiloHostedServiceReminderProvider.AdoNet;




                        _.ClusterId = hosting.Configuration.GetValue<string>("Orleans:ClusterId");
                        _.ServiceId = hosting.Configuration.GetValue<string>("Orleans:ServiceId");
                    });

                    // add the silo hosted service and the services it makes available
                    services.AddSingleton<SiloHostedService>();
                    services.AddSingleton<IHostedService>(_ => _.GetService<SiloHostedService>());
                    services.AddSingleton(_ => _.GetService<SiloHostedService>().ClusterClient);

                    // add options for the api hosted service
                    services.Configure<SupportApiOptions>(options =>
                    {
                        options.Title = hosting.Configuration.GetValue<string>("Api:Title");
                        options.PortRange.Start = hosting.Configuration.GetValue<int>("Api:Port:Start");
                        options.PortRange.End = hosting.Configuration.GetValue<int>("Api:Port:End");
                    });

                    // add the back-end api service
                    services.AddSingleton<SupportApiHostedService>();
                    services.AddSingleton<IHostedService>(_ => _.GetService<SupportApiHostedService>());
                })
                .ConfigureLogging((hosting, configure) =>
                {
                    configure.AddSerilog(new LoggerConfiguration()
                        .WriteTo.Console(
                            restrictedToMinimumLevel: hosting.Configuration.GetValue<LogEventLevel>("Serilog:Console:RestrictedToMinimumLevel"))
                        .WriteTo.MSSqlServer(
                            connectionString: hosting.Configuration.GetConnectionString("Orleans"),
                            schemaName: hosting.Configuration["Serilog:MSSqlServer:SchemaName"],
                            tableName: hosting.Configuration["Serilog:MSSqlServer:TableName"],
                            restrictedToMinimumLevel: hosting.Configuration.GetValue<LogEventLevel>("Serilog:MSSqlServer:RestrictedToMinimumLevel"))
                        .CreateLogger());
                })
                .UseConsoleLifetime()
                .Build();

            // write the port configuration on the console title
            var silo = host.Services.GetService<SiloHostedService>();
            var api = host.Services.GetService<SupportApiHostedService>();
            Console.Title = $"{nameof(IHost)}: Silo: {silo.SiloPort}, Gateway: {silo.GatewayPort}, Dashboard: {silo.DashboardPort}, Api: {api.Port}";

            return host.RunAsync();
        }
    }
}
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime;
using System.Threading.Tasks;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Prometheus;
using Prometheus.DotNetRuntime;
using Prometheus.DotNetRuntime.StatsCollectors;

namespace AspNetCoreExample
{
    public class Program
    {
        public static IDisposable Collector;
        
        public static void Main(string[] args)
        {
            if (Environment.GetEnvironmentVariable("NOMON") == null)
            {
                Console.WriteLine("Enabling prometheus-net.DotNetStats...");
                Collector = CreateCollector();
            }

            CreateWebHostBuilder(args).Build().Run();
        }

        public static IDisposable CreateCollector()
        {
            return DotNetRuntimeStatsBuilder.Default()
                .WithThreadPoolSchedulingStats()
                .WithContentionStats(SampleEvery.OneEvent)
                .WithGcStats()
                .WithJitStats(SampleEvery.OneEvent)
                .WithThreadPoolStats()
                .WithExceptionStats()
                //.WithCustomCollector(new RuntimeCounterCollector(refreshTimeSeconds: 1))
                //.WithErrorHandler(ex => Console.WriteLine("ERROR: " + ex.ToString()))
                .WithDebuggingMetrics(true)
                .StartCollecting();
        }

        public static IWebHostBuilder CreateWebHostBuilder(string[] args) =>
            WebHost.CreateDefaultBuilder(args)
                .ConfigureKestrel(opts =>
                {
                    opts.AllowSynchronousIO = true;
                })
                .UseStartup<Startup>();
    }
}
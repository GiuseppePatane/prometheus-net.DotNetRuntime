using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Runtime;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;

namespace Prometheus.DotNetRuntime
{
    internal sealed class DotNetRuntimeStatsCollector : 
        IDisposable
    {
        private static readonly Dictionary<CollectorRegistry, DotNetRuntimeStatsCollector> Instances = new Dictionary<CollectorRegistry, DotNetRuntimeStatsCollector>();
        
        private DotNetEventListener[] _eventListeners;
        private readonly ImmutableHashSet<IEventSourceCollector> _statsCollectors;
        private readonly bool _enabledDebugging;
        private readonly Action<Exception> _errorHandler;
        private readonly CollectorRegistry _registry;
        private readonly object _lockInstance = new object();

        internal DotNetRuntimeStatsCollector(ImmutableHashSet<IEventSourceCollector> statsCollectors, Action<Exception> errorHandler, bool enabledDebugging, CollectorRegistry registry)
        {
            _statsCollectors = statsCollectors;
            _enabledDebugging = enabledDebugging;
            _errorHandler = errorHandler ?? (e => { });
            _registry = registry;
            lock (_lockInstance)
            {
                if (Instances.ContainsKey(registry))
                {
                    throw new InvalidOperationException(".NET runtime metrics are already being collected. Dispose() of your previous collector before calling this method again.");
                }

                Instances.Add(registry, this);
            }
        }

        public void RegisterMetrics(CollectorRegistry registry)
        {
            var metrics = Metrics.WithCustomRegistry(registry);

            foreach (var sc in _statsCollectors.OfType<IEventSourceStatsCollector>())
            {
                sc.RegisterMetrics(metrics);
            }
            
            // Metrics have been registered, start the event listeners
            _eventListeners = _statsCollectors
                .Select(sc => new DotNetEventListener(sc, _errorHandler, _enabledDebugging))
                .ToArray();

            SetupConstantMetrics(metrics);
        }

        public void UpdateMetrics()
        {
            foreach (var sc in _statsCollectors.OfType<IEventSourceStatsCollector>())
            {
                try
                {
                    sc.UpdateMetrics();
                }
                catch (Exception e)
                {
                    _errorHandler(e);
                }
            }
        }

        public void Dispose()
        {
            try
            {
                if (_eventListeners == null)
                    return;

                foreach (var listener in _eventListeners)
                    listener?.Dispose();
            }
            finally
            {
                lock (_lockInstance)
                {
                    Instances.Remove(_registry);
                }
            }
        }
        
        private void SetupConstantMetrics(MetricFactory metrics)
        {
            // These metrics are fairly generic in name, catch any exceptions on trying to create them 
            // in case prometheus-net or another plugin has registered them.
            try
            {
                var buildInfo = metrics.CreateGauge(
                    "dotnet_build_info",
                    "Build information about prometheus-net.DotNetRuntime and the environment",
                    "version",
                    "target_framework",
                    "runtime_version",
                    "os_version",
                    "process_architecture",
                    "gc_mode"
                );

                buildInfo.Labels(
                        this.GetType().Assembly.GetName().Version.ToString(),
                        Assembly.GetEntryAssembly().GetCustomAttribute<TargetFrameworkAttribute>().FrameworkName,
                        RuntimeInformation.FrameworkDescription,
                        RuntimeInformation.OSDescription,
                        RuntimeInformation.ProcessArchitecture.ToString(),
                        GCSettings.IsServerGC ? "Server" : "Workstation"
                    )
                    .Set(1);
            }
            catch (Exception e)
            {
                _errorHandler(e);
            }

            try
            {
                var processorCount = metrics.CreateGauge("process_cpu_count", "The number of processor cores available to this process.");
                processorCount.Set(Environment.ProcessorCount);
            }
            catch (Exception e)
            {
                _errorHandler(e);
            }
        }
    }
}
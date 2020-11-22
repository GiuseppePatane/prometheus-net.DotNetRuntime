using System;
using System.Diagnostics.Tracing;
using Prometheus.DotNetRuntime.EventSources;

namespace Prometheus.DotNetRuntime
{
    /// <summary>
    /// Extends <see cref="IEventSourceCollector"/> to specify the aggregation of events and measuring them using prometheus metrics.
    /// </summary>
    public interface IEventSourceStatsCollector : IEventSourceCollector
    {
        /// <summary>
        /// Called when the instance is associated with a collector registry, so that the collectors managed
        /// by this instance can be registered.
        /// </summary>
        void RegisterMetrics(MetricFactory metrics);

        /// <summary>
        /// Called before each collection. Any values in collectors managed by this instance should now be brought up to date.
        /// </summary>
        void UpdateMetrics();
    }
}
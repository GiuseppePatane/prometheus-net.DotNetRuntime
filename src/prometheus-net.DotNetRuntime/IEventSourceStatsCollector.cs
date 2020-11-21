using System;
using System.Diagnostics.Tracing;
using Prometheus.DotNetRuntime.EventSources;

namespace Prometheus.DotNetRuntime
{
    /// <summary>
    /// Defines an interface register for and receive .NET runtime events. Events can then be aggregated
    /// and measured as prometheus metrics.
    /// </summary>
    public interface IEventSourceStatsCollector
    {
        /// <summary>
        /// The unique id of the event source to receive events from.
        /// </summary>
        Guid EventSourceGuid { get; }
        
        /// <summary>
        /// The keywords to enable in the event source.
        /// </summary>
        /// <remarks>
        /// Keywords act as a "if-any-match" filter- specify multiple keywords to obtain multiple categories of events
        /// from the event source.
        /// </remarks>
        EventKeywords Keywords { get; }
        
        /// <summary>
        /// The level of events to receive from the event source.
        /// </summary>
        EventLevel Level { get; }
        
        /// <summary>
        /// Process a received event.
        /// </summary>
        /// <remarks>
        /// Implementors should listen to events and perform some kind of aggregation, emitting this to prometheus.
        /// </remarks>
        void ProcessEvent(EventWrittenEventArgs e);
        
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
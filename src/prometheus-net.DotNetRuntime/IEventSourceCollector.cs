using System;
using System.Diagnostics.Tracing;

namespace Prometheus.DotNetRuntime
{
    /// <summary>
    /// Defines an interface to register for and receive .NET runtime events.
    /// </summary>
    public interface IEventSourceCollector
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
        /// Implementors should listen to events and perform some kind of processing.
        /// </remarks>
        void ProcessEvent(EventWrittenEventArgs e);
    }
}
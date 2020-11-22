namespace Prometheus.DotNetRuntime
{
    /// <summary>
    /// An <see cref="IEventSourceCollector"/> that listens for diagnostic event counters.
    /// </summary>
    /// <remarks>
    /// See https://docs.microsoft.com/en-us/dotnet/core/diagnostics/event-counters for more info on event counters. 
    /// </remarks>
    public interface IEventSourceCounterCollector : IEventSourceCollector
    {
        int RefreshPeriodSeconds { get; }
    }
    
    /// <summary>
    /// A <see cref="IEventSourceCounterCollector"/> that exposes counters as a typed object.
    /// </summary>
    /// <typeparam name="TCounters"></typeparam>
    public interface IEventSourceCounterCollector<TCounters> : IEventSourceCounterCollector
    {
        TCounters Counters { get; }
    }
}
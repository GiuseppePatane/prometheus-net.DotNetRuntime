using System;
using System.Collections.Generic;
using System.Diagnostics.Tracing;
using Prometheus.DotNetRuntime.EventCounters;
using Prometheus.DotNetRuntime.EventSources;
using EventCounter = Prometheus.DotNetRuntime.EventCounters.EventCounter;
using IncrementingEventCounter = Prometheus.DotNetRuntime.EventCounters.IncrementingEventCounter;

namespace Prometheus.DotNetRuntime.StatsCollectors
{
    // TODO Test + Document
    public class RuntimeCounterCollector : IEventSourceCounterCollector<RuntimeCounterCollector.RuntimeCounters>
    {
        private readonly MeanEventCounter _threadPoolThreadCount = new MeanEventCounter("threadpool-thread-count");
        private readonly MeanEventCounter _threadPoolQueueLength = new MeanEventCounter("threadpool-queue-length");
        private readonly IncrementingEventCounter _threadPoolCompletedItemsCount = new IncrementingEventCounter("threadpool-completed-items-count");
        private readonly IncrementingEventCounter _monitorLockContentionCount = new IncrementingEventCounter("monitor-lock-contention-count");
        private readonly MeanEventCounter _activeTimerCount = new MeanEventCounter("active-timer-count");
        private readonly IncrementingEventCounter _exceptionCount = new IncrementingEventCounter("exception-count");
        private readonly MeanEventCounter _numAssembliesLoaded = new MeanEventCounter("assembly-count");
        private readonly MeanEventCounter _ilBytesJitted = new MeanEventCounter("il-bytes-jitted");
        private readonly MeanEventCounter _methodsJittedCount = new MeanEventCounter("methods-jitted-count");
        private readonly IncrementingEventCounter _allocRate = new IncrementingEventCounter("alloc-rate");
        
        private readonly Dictionary<string, EventCounter> _counterDictionary;

        public RuntimeCounterCollector(int refreshTimeSeconds)
        {
            RefreshPeriodSeconds = refreshTimeSeconds;
            Counters = new RuntimeCounters(this);
            _counterDictionary = EventCounter.GenerateDictionary(this);
;        }
        
        public Guid EventSourceGuid => SystemRuntimeEventSource.Id;
        public EventKeywords Keywords => EventKeywords.All;
        public EventLevel Level => EventLevel.LogAlways;
        public int RefreshPeriodSeconds { get; }
        public RuntimeCounters Counters { get; }

        public void ProcessEvent(EventWrittenEventArgs e)
        {
            if (!e.EventName.Equals("EventCounters"))
            {
                return;
            }

            if (!(e.Payload[0] is IDictionary<string, object> eventData) || !eventData.TryGetValue("Name", out var counterName))
            {
                return;
            }

            if (!_counterDictionary.TryGetValue((string) counterName, out var counter))
                return;
            
            counter.TryReadSampleFrom(eventData);
        }

        public class RuntimeCounters
        {
            private readonly RuntimeCounterCollector _parent;

            public RuntimeCounters(RuntimeCounterCollector parent)
            {
                _parent = parent;
            }

            public IObservable<MeanCounterValue> ThreadPoolThreadCount => _parent._threadPoolThreadCount;
            public IObservable<MeanCounterValue> ThreadPoolQueueLength => _parent._threadPoolQueueLength;
            public IObservable<IncrementingCounterValue> ThreadPoolCompletedItemsCount => _parent._threadPoolCompletedItemsCount;
            public IObservable<IncrementingCounterValue> MonitorLockContentionCount => _parent._monitorLockContentionCount;
            public IObservable<MeanCounterValue> ActiveTimerCount => _parent._activeTimerCount;
            public IObservable<IncrementingCounterValue> ExceptionCount => _parent._exceptionCount;
            public IObservable<MeanCounterValue> NumAssembliesLoaded => _parent._numAssembliesLoaded;
            public IObservable<MeanCounterValue> IlBytesJitted => _parent._ilBytesJitted;
            public IObservable<MeanCounterValue> MethodsJittedCount => _parent._methodsJittedCount;
            public IObservable<IncrementingCounterValue> AllocRate => _parent._allocRate;
        }
    }
}
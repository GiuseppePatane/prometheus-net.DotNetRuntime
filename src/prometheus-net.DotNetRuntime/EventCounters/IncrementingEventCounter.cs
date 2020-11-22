using System;
using System.Collections;
using System.Collections.Generic;
using System.Reactive.Subjects;

namespace Prometheus.DotNetRuntime.EventCounters
{
    public class IncrementingEventCounter : EventCounter, IObservable<IncrementingCounterValue>
    {
        private readonly Subject<IncrementingCounterValue> _subject = new Subject<IncrementingCounterValue>();

        public IncrementingEventCounter(string name) : base(name)
        {
        }

        public IDisposable Subscribe(IObserver<IncrementingCounterValue> observer)
        {
            return _subject.Subscribe(observer);
        }

        public override bool TryReadSampleFrom(IDictionary<string, object> eventData)
        {
            if (!eventData.TryGetValue("Increment", out var increment))
                return false;
            
            _subject.OnNext(new IncrementingCounterValue((double)increment));
            return true;
        }
    }

    public readonly struct IncrementingCounterValue
    {
        public IncrementingCounterValue(double value)
        {
            IncrementedBy = value;
        }

        public double IncrementedBy { get; }
    }
}
using System;
using System.Collections.Generic;
using System.Reactive.Subjects;

namespace Prometheus.DotNetRuntime.EventCounters
{
    public class MeanEventCounter : EventCounter, IObservable<MeanCounterValue>
    {
        private readonly Subject<MeanCounterValue> _subject = new Subject<MeanCounterValue>();

        public MeanEventCounter(string name) : base(name)
        {
        }

        public IDisposable Subscribe(IObserver<MeanCounterValue> observer)
        {
            return _subject.Subscribe(observer);
        }

        public override bool TryReadSampleFrom(IDictionary<string, object> eventData)
        {
            if (!eventData.TryGetValue("Mean", out var mean) || !eventData.TryGetValue("Count", out var count))
                return false;
            
            _subject.OnNext(new MeanCounterValue((int)count, (double)mean));
            return true;
        }
    }

    public readonly struct MeanCounterValue
    {
        public MeanCounterValue(int count, double mean)
        {
            Count = count;
            Mean = mean;
        }

        public int Count { get; }
        public double Mean { get; }

        public double Total => Count * Mean;
    }
}
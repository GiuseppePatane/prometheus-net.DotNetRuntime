using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Prometheus.DotNetRuntime.EventCounters
{
    public abstract class EventCounter
    {
        public EventCounter(string name)
        {
            Name = name;
        }
        
        public string Name { get; }

        public abstract bool TryReadSampleFrom(IDictionary<string, object> eventData);

        public static Dictionary<string, EventCounter> GenerateDictionary<TFrom>(TFrom owningType)
        {
            return owningType.GetType()
                .GetFields(BindingFlags.Instance | BindingFlags.DeclaredOnly | BindingFlags.NonPublic)
                .Where(x => x.FieldType.BaseType == typeof(EventCounter))
                .Select(x => x.GetValue(owningType) as EventCounter)
                .ToDictionary(k => k.Name, k => k);

        }
    }
}
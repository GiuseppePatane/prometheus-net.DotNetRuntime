using System;
using Prometheus.DotNetRuntime.StatsCollectors;

namespace Prometheus.DotNetRuntime
{

    public class CollectorOptions<TCollector>
    {
        
    }

    public interface ILevel
    {
    }

    public interface ITraceInfo : ILevel
    {
        Level TraceInfo => Level.TraceInfo;
    }
    
    public interface ITraceVerbose : ILevel
    {
        public Level Verbose => Level.TraceVerbose;
    }


    

    public class Builder
    {
        public CollectorsSelector Collect { get; } = new CollectorsSelector(this);
    }

    public class CollectorsSelector
    {
        public CollectorsSelector(Builder builder)
        {
            Builder = builder;
        }
        
        internal Builder Builder { get; }

        public Gc GcStats => new Gc(Builder);
        public Jit JitStats { get; }
        public ThreadPoolStats ThreadPoolStats { get; }
    }
    
    public abstract class CollectorSelector<TLevels> 
        where TLevels : ILevel, new()
    {
        private TLevels _levels;

        public CollectorSelector(Builder builder)
        {
            Builder = builder;
            _levels = new TLevels();
        }
        
        internal Builder Builder { get; }
        
        internal Level SelectedDefaultLevel { get; set; }
        
        public CollectorsSelector And { get; }
        
        // must return a level
        public ConditionBuilder<TLevels> UseLevel { get; }
        
        // must return a builder
        public CollectorSelector<TLevels> DefaultLevel(Func<TLevels, Level> selector)
        {
            SelectedDefaultLevel = selector(_levels);
            return this;
        }
    }

    public class Gc : CollectorSelector<Gc.Levels>
    {
        public Gc(Builder builder) : base(builder)
        {
        }
        
        public class Levels : ITraceInfo, ITraceVerbose
        {
            public Level TraceInfo => Level.TraceInfo;
        }
    }

    public static class Extensions2
    {
        public static void Info(this ITraceInfo e)
        {
            
        }
    }
    

    public class Jit : CollectorSelector<Jit.Levels>
    {
        public Jit(Builder builder) : base(builder)
        {
        }
        
        public class Levels
        {
        }
    }

    public class ThreadPoolStats : CollectorSelector<ThreadPoolStats.Levels>
    {
        public ThreadPoolStats(Builder builder) : base(builder)
        {
        }
        
        public class Levels
        {
        }
    }

    public class CollectorOptions<TLevels, TCounters>
    {
        public CollectorOptions<TLevels, TCounters> DefaultLevel(Func<TLevels, Level> levelSelector)
        {
            return this;
        }

        public CollectorOptions<TLevels, TCounters> UseLevel(Func<TLevels, Level> levelSelector, Func<EnableContext<TCounters>, bool> enableCondition, Func<EnableContext<TCounters>, bool> disableCondition)
        {
            return this;
        }
    }

    public class Demo
    {
        public static void Demo2()
        {
            // need to be able to specify a default level
            var x = new Builder()
                .Collect.GcStats
                    .DefaultLevel(x => x.TraceInfo)
                .And.JitStats
                .And.ThreadPoolStats;
        }
        
    }


    public class ConditionContext<TCounters>
    {
        public DateTime ProcessStartedAt { get; }
        public TCounters Counters { get; }
    }
    
    public class EnableContext<TCounters> : ConditionContext<TCounters> 
    {
        public TimeSpan? TimeSinceLastEnabled { get; }
    }

    public class DisableContext<TCounters> : ConditionContext<TCounters>
    {
        public TimeSpan TimeEnabled { get; }
        // TODO should be counter?
        public double EventsSec { get; }
    }

    public class JitCollectorMetrics
    {
        public double EventsPerSecond { get; set; }
        public double NumMethodsJitted { get; set; }
        public double BytesJitted { get; set; }
    }

    public static class BuilderExtensions
    {
        public static DotNetRuntimeStatsBuilder WithGcStats(Func<CollectorOptions<GcOptions>> configure)
        {
            var collector = new GcStatsCollector();
            var options = new CollectorOptions<GcOptions>();
            configure(options);
        }

        // must hold not only thingos but buckets + stuff
        // what thingo will hold context? Seems natural to put it on the collector
        // for each collector we need to expose:
        // 1. Options available (levels + metric settings + other) - dedicated class?
        // 2. Levels it can collect - dedicated class allows for deep documentation on what the level exposes
        // 3. Context (counters) it exposes to make decisions
        public class GcOptions : ITraceInfo, ITraceVerbose
        {
        }
        // public static CollectorOptions<GcStatsCollector> WithGcCollector(this Factory factory)
        // {
        //     
        // }
    }

    
    

    public enum Level
    {
        Counters = 0,
        // Reserved for future use
        //TraceError = 1
        //TraceWarning = 2
        TraceInfo = 3,
        TraceVerbose = 4,
        // Reserved for future use
        TraceAll = 5
    }

    public class Context
    {
        public Level CurrentLevel { get; set; }
        public double EventsSec { get; set; }
        
    }

    public interface IStrategy
    {
        Level DetermineLevel(Level currentLevel);
    }

    public class Strategy
    {
        public void Run()
        {
            Level currentLevel;
            while (true)
            {
                // TODO calculate desired level
                var desiredLevel = null;

                if (desiredLevel != currentLevel)
                {
                    // change level
                }

                // must track current level for each collector
                // what would the impact be of changing the level too frequently? metrics would appear incomplete, perhaps unscaled
            }
        }
        /*
         * Examples of conditions:
         * - While JIT activity is high, enable (while num methods JIT'd > 10, keep tracking accurate stats)
         * - While threadpool queueing is high, enable
         *
         *
         * What should the default be?
         * - Show detail when needed (e.g. JIT/ GC/ etc. is suspect)
         * - Cap detail when it's incurring a too greater cost
         *
         * We should make the user explicitly choose which one is best for them 
         */
    }
}
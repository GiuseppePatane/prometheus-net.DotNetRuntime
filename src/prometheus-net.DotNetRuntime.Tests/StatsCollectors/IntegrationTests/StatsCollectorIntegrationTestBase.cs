using System;
using System.Threading;
using System.Threading.Tasks;
using NUnit.Framework;
using Prometheus.DotNetRuntime;
using Prometheus.DotNetRuntime.StatsCollectors;

namespace Prometheus.DotNetRuntime.Tests.StatsCollectors.IntegrationTests
{
    [TestFixture]
    internal abstract class StatsCollectorIntegrationTestBase<TStatsCollector> 
        where TStatsCollector : IEventSourceStatsCollector
    {
        private DotNetEventListener _eventListener;
        protected TStatsCollector StatsCollector { get; private set; }

        [SetUp]
        public void SetUp()
        {
            StatsCollector = CreateStatsCollector();
            StatsCollector.RegisterMetrics(Metrics.WithCustomRegistry(Metrics.NewCustomRegistry()));
            _eventListener = new DotNetEventListener(StatsCollector, null, false);
            
            // wait for event listener thread to spin up
            while (!_eventListener.StartedReceivingEvents)
            {
                Thread.Sleep(10); 
                Console.Write("Waiting.. ");
                
            }
            Console.WriteLine("EventListener should be active now.");
        }

        [TearDown]
        public void TearDown()
        {
            _eventListener.Dispose();
        }

        protected abstract TStatsCollector CreateStatsCollector();
    }
}
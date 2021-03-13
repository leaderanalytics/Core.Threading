using System;
using System.Collections.Generic;
using System.Text;
using NUnit;
using NUnit.Framework;
using System.Xml.Serialization;
using System.Diagnostics;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using System.Collections.Concurrent;

namespace LeaderAnalytics.Core.Threading.Tests
{
    [TestFixture]
    public class BatchThrottleTests
    {
        BatchThrottleAsync batchThrottle;
        private ConcurrentDictionary<string,string> dict;

        public BatchThrottleTests()
        {
            batchThrottle = new BatchThrottleAsync(20);
            dict = new ConcurrentDictionary<string, string>();
        }

        private async Task DoNothing(string id)
        {
            for (int i = 0; i < 10; i++)
            {
                await batchThrottle.WaitForBatch();
                string s = id + i.ToString();
                dict.TryAdd(s, s);
            }
        }


        [Test]
        public async Task Test1()
        {
            Task t1 = Task.Run(() => DoNothing("A"));
            Task t2 = Task.Run(() => DoNothing("B"));
            Task t3 = Task.Run(() => DoNothing("C"));
            await Task.WhenAll(t1, t2, t3);
        }
    }
}

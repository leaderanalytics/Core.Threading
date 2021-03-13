using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace LeaderAnalytics.Core.Threading
{
    public class AsyncManualResetEvent
    {
        //https://devblogs.microsoft.com/pfxteam/building-async-coordination-primitives-part-1-asyncmanualresetevent/

        private volatile TaskCompletionSource<bool> m_tcs = new TaskCompletionSource<bool>();

        public Task WaitAsync() => m_tcs.Task;

        public void Set() => m_tcs.TrySetResult(true);

        public void Reset()
        {
            while (true)
            {
                var newtcs = m_tcs;

                if (!newtcs.Task.IsCompleted || Interlocked.CompareExchange(ref m_tcs, new TaskCompletionSource<bool>(), newtcs) == newtcs)
                    return;

            }
        }
    }
}

using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace LeaderAnalytics.Core.Threading
{
    public class BatchThrottleAsync : IDisposable
    {
        private int batchCount;
        private readonly int batchSize;
        private Timer timer;
        private AsyncManualResetEvent mre;
        private bool disposed;
        private bool blockNow;

        public BatchThrottleAsync(int batchSize = 120, int window = 60000)
        {
            if (batchSize <= 0)
                throw new Exception("batchSize must be greater than zero.");

            if (window <= 0)
                throw new Exception("window must be greater than zero.");

            this.batchSize = batchSize;
            mre = new AsyncManualResetEvent();
            timer = new Timer(x => ResetBatch(), null, 0, window);
        }
        
        public async Task WaitForBatch()
        {
            Interlocked.Add(ref batchCount, 1);

            if ((batchCount >= batchSize + 1) || blockNow)
                mre.Reset(); // block

            await mre.WaitAsync();
        }

        /// <summary>
        /// Forces the throttle to start blocking immediately.
        /// </summary>
        public void BlockNow() => blockNow = true;

        private void ResetBatch()
        {
            batchCount = 0;
            blockNow = false;
            mre.Set(); // unblock
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposed)
            {
                if (disposing)
                {
                    timer.Dispose();
                    disposed = true;
                }
            }

        }
    }
}

﻿using System;
using System.Threading;
using System.Threading.Tasks;

namespace TinyInsightsLib
{
    public class TinyDependency : IDisposable
    {
        public TinyDependency()
        {
            StartTime = DateTimeOffset.Now;
        }

        public string DependencyType { get; internal set; }
        public string DependencyName { get; internal set; }
        public DateTimeOffset StartTime { get; private set; }
        public TimeSpan Duration { get; private set; }

        private SemaphoreSlim semaphore = new SemaphoreSlim(1,1);

        private bool isFinished;

        public void Dispose()
        {
            Task.Run(() => Finish(true));
        }

        public Task Finish(bool sucess, int resultCode = 0)
        {
            return Finish(sucess, resultCode, null);
        }

        public Task Finish(bool sucess, Exception exception)
        {
            return Finish(sucess, 0, exception);
        }

        public async Task Finish(bool sucess, int resultCode, Exception exception = null)
        {
            await semaphore.WaitAsync();

            if (!isFinished)
            {
                Duration = DateTimeOffset.Now - StartTime;

                await TinyInsights.TrackDependencyAsync(DependencyType, DependencyName, StartTime, Duration, sucess, resultCode, exception);

                isFinished = true;

                semaphore.Release(); 
            }
        }
    }
}

using HawkEye.Logging;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace HawkEye.Utils
{
    internal class ConcurrentResourceProvider<R> : IDisposable
    {
        private LoggingSection logging;
        private ConcurrentQueue<R> resources;
        private bool disposed;

        public bool ResourceAvailable { get => !resources.IsEmpty; }
        public int AvailableResourceCount { get => resources.Count; }

        public ConcurrentResourceProvider() => disposed = false;

        public void Setup(R[] resourceArray)
        {
            logging = new LoggingSection(this);
            resources = new ConcurrentQueue<R>();

            foreach (R resource in resourceArray)
                Feed(resource);
        }

        public void Feed(R resource)
        {
            if (disposed && typeof(IDisposable).IsAssignableFrom(typeof(R)))
                ((IDisposable)resource).Dispose();
            else
                resources.Enqueue(resource);
        }

        public R AwaitResource(int delay = 100)
        {
            while (!ResourceAvailable)
                Thread.Sleep(delay);

            resources.TryDequeue(out R result);
            return result;
        }

        public async Task<R> AwaitResourceAsync(int delay = 100)
        {
            while (!ResourceAvailable)
                await Task.Delay(delay);

            resources.TryDequeue(out R result);
            return result;
        }

        public void Dispose()
        {
            if (typeof(IDisposable).IsAssignableFrom(typeof(R)))
                while (ResourceAvailable)
                {
                    resources.TryDequeue(out R result);
                    ((IDisposable)result).Dispose();
                }

            resources = null;
            disposed = true;
        }
    }
}
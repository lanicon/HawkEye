using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace HawkEye
{
    internal class Program
    {
        public static bool IsRunning { get; private set; }

        private static void Main(string[] args)
        {
            Thread.CurrentThread.Name = "Main";
            IsRunning = true;

            Services.Initiate();

            Services.CommandHandler.TakeControl();

            Exit();
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        private static void Exit()
        {
            Services.Dispose();
        }

        public static void Stop() => IsRunning = false;
    }
}
using System;
using System.Collections.Concurrent;
using System.Threading;

namespace Server
{
    public class Loop 
    {
        public IOLoop Owner { get; private set; }

        public Loop(IOLoop owner)
        {
            Owner = owner;
        }
    }

    public class IOLoop
    {
        public Loop EventLoop { get; private set; }

        public IOLoop()
        {
            Synchronize = true;
            EventLoop = new Loop(this);
        }


        public bool Synchronize { get; set; }
        private volatile bool stop;
        private readonly ConcurrentQueue<Action> actions = new ConcurrentQueue<Action>();
        private readonly AutoResetEvent ev = new AutoResetEvent(false);

        


        public void Start()
        {
            while (!stop)
            {
                ev.WaitOne();
                Action act;
                while (actions.TryDequeue(out act))
                {
                    act();
                }
            }
        }

        public void Stop()
        {
            stop = true;
            ev.Set();
        }

        public void BlockInvoke(Action t)
        {
            if (Synchronize)
            {
                var o = new object();
                bool done = false;
                NonBlockInvoke(() =>
                {
                    done = true;
                    Monitor.Pulse(o);
                });
                while (!done)
                {
                    Monitor.Wait(o);
                }
                    
            }
            else
                t();
        }

        public void NonBlockInvoke(Action t)
        {
            if (Synchronize)
            {
                actions.Enqueue(t);
                ev.Set();
            }
            else
            {
                t();
            }
        }
    }
}
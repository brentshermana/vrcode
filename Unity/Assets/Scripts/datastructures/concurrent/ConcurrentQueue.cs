using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using System.Threading;
using UnityEditor.Experimental.Build.AssetBundle;

namespace vrcode.datastructures.concurrent
{
    public class ConcurrentQueue<T>
    {
        private readonly object syncLock = new object();
        private Queue<T> queue;
        private Semaphore count_sem;

        public int Count
        {
            get
            {
                lock (syncLock)
                {
                    return queue.Count;
                }
            }
        }



        public ConcurrentQueue()

        {
            this.queue = new Queue<T>();
            this.count_sem = new Semaphore(0, int.MaxValue);
        }



        public T Peek()
        {
            lock (syncLock)
            {
                return queue.Peek();
            }
        }

        public bool TryEnqueue(T obj)
        {
            if (!Monitor.TryEnter(syncLock)) return false;
            else
            {
                queue.Enqueue(obj);
                Monitor.Exit(syncLock);
                count_sem.Release();
                return true;
            }
        }

        public T TryDeque(ref bool success, TimeSpan timeout)
        {
            T ret = default(T);
            if (count_sem.WaitOne(timeout))
            {
                if (Monitor.TryEnter(syncLock, timeout))
                {
                    ret = queue.Dequeue();
                    Monitor.Exit(syncLock);
                    success = true;
                }
                else
                {
                    success = false;
                }
            }
            else
            {
                success = false;
            }

            return ret;
        }
        
        public T TryDequeue(ref bool success)
        {
            return TryDeque(ref success, TimeSpan.Zero);
        }

        public void Enqueue(T obj)
        {
            lock (syncLock)

            {
                queue.Enqueue(obj);
            }

            count_sem.Release();
        }



        public T Dequeue()

        {
            // outside lock to avoid waiting on sem within lock
            // which would result in a deadlock
            count_sem.WaitOne();
            lock (syncLock)
            {
                return queue.Dequeue();
            }
        }
    }
}
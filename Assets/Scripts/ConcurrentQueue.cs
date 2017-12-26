using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using System.Threading;

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

    public T TryDequeue(ref bool success)
    {
        T ret = default(T);
        if (!Monitor.TryEnter(syncLock))
        {
            success = false;
        }
        else
        {
            if (queue.Count > 0)
            {
                count_sem.WaitOne(); // only acceptable within the lock because we know this won't block
                ret = queue.Dequeue();
                success = true;
            }
            else
            {
                success = false;
            }
            Monitor.Exit(syncLock);
        }
        return ret;
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

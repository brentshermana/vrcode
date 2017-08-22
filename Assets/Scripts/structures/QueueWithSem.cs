using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

public class QueueWithSem<T> {

    private Semaphore sem;
    private Queue<T> queue;

    public QueueWithSem() {
        queue = new Queue<T>();
        sem = new Semaphore(0, int.MaxValue);
    }

    //returns null if the lock can't be acquired immediately
    public T getWait()
    {
        sem.WaitOne(int.MaxValue);
        Monitor.Enter(queue);
        T ret = queue.Dequeue();
        Monitor.Exit(queue);
        return ret;
    }

    public bool putNoWait(T value)
    {
        if (!Monitor.TryEnter(queue))
            return false;
        else
        {
            queue.Enqueue(value);
            sem.Release();
            Monitor.Exit(queue);
            return true;
        }
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using System.Threading;

public class ConcurrentQueue<T>
{
    private readonly object syncLock = new object();
    private Queue<T> queue;
    
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

    }



    public T Dequeue()

    {

        lock (syncLock)

        {

            return queue.Dequeue();

        }

    }



    public void Clear()

    {

        lock (syncLock)

        {

            queue.Clear();

        }

    }



    public T[] CopyToArray()

    {

        lock (syncLock)

        {

            if (queue.Count == 0)

            {

                return new T[0];

            }



            T[] values = new T[queue.Count];

            queue.CopyTo(values, 0);

            return values;

        }

    }



    public static ConcurrentQueue<T> InitFromArray(IEnumerable<T> initValues)

    {

        var queue = new ConcurrentQueue<T>();



        if (initValues == null)

        {

            return queue;

        }



        foreach (T val in initValues)

        {

            queue.Enqueue(val);

        }



        return queue;

    }

}

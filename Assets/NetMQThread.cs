//using Boo.Lang;
using NetMQ;
using NetMQ.Sockets;
using System.Diagnostics;
using System.Threading;
using System.Collections.Generic;

public class NetMQThread
{
    private readonly Thread _listenerWorker;

    private bool _listenerCancelled;

    public delegate void MessageDelegate(ActionableJsonMessage[] message);

    private readonly MessageDelegate _messageDelegate;

    private readonly Stopwatch _contactWatch;

    private const long ContactThreshold = 1000;

    private readonly ConcurrentQueue<ActionableJsonMessage> inQueue;
    private readonly ConcurrentQueue<ActionableJsonMessage> outQueue;

    public bool Connected;

    private void ListenerWork()
    {
        AsyncIO.ForceDotNet.Force();
        using (var server = new ResponseSocket())
        {
            server.Bind("tcp://localhost:12346");

            while (!_listenerCancelled)
            {
                Connected = _contactWatch.ElapsedMilliseconds < ContactThreshold;
                byte[] message;
                if (!server.TryReceiveFrameBytes(out message)) continue;

                _contactWatch.Reset();
                _contactWatch.Start();

                ActionableJsonMessage[] messages = MyJson.fromBytes(message);
                foreach (ActionableJsonMessage m in messages)
                {
                    if (m.Type != "NOP")
                    {
                        UnityEngine.Debug.Log("NetMQThread read " + m.ToString());
                        inQueue.Enqueue(m);
                    }
                }

                List<ActionableJsonMessage> outMessages = new List<ActionableJsonMessage>();
                bool success = true;
                while (success)
                {
                    ActionableJsonMessage m = outQueue.TryDequeue(ref success);
                    if (success)
                    {
                        outMessages.Add(m);
                    }
                }
                if (outMessages.Count == 0)
                {
                    ActionableJsonMessage nop = new ActionableJsonMessage("NOP", "", null);
                    outMessages.Add(nop);
                }
                byte[] outwardmessage = MyJson.toBytes(outMessages.ToArray());
                server.SendFrame(outwardmessage);
            }
        }
        NetMQConfig.Cleanup();
    }

    public NetMQThread(ConcurrentQueue<ActionableJsonMessage> inq, ConcurrentQueue<ActionableJsonMessage> outq)
    {
        inQueue = inq;
        outQueue = outq;
        _contactWatch = new Stopwatch();
        _contactWatch.Start();
        _listenerWorker = new Thread(ListenerWork);
    }

    public void Start()
    {
        _listenerCancelled = false;
        _listenerWorker.Start();
    }

    public void Stop()
    {
        _listenerCancelled = true;
        _listenerWorker.Join();
    }
}
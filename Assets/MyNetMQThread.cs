using UnityEngine;

using NetMQ;
using NetMQ.Sockets;
using System.Diagnostics;
using System.Threading;
using System.Collections.Generic;


public class MyNetMQThread
{
    private readonly Thread _listenerWorker;

    private bool _listenerCancelled;

    public delegate void MessageDelegate(ActionableJsonMessage[] message);

    private readonly MessageDelegate _messageDelegate;

    private readonly Stopwatch _contactWatch;

    private const long ContactThreshold = 1000;

    public bool Connected;

    private void ListenerWork()
    {
        AsyncIO.ForceDotNet.Force();
        using (var server = new ResponseSocket())
        {
            server.Bind("tcp://localhost:12345");

            while (!_listenerCancelled)
            {
                Connected = _contactWatch.ElapsedMilliseconds < ContactThreshold;
                byte[] message;
                if (!server.TryReceiveFrameBytes(out message)) continue;

                _contactWatch.Reset();
                _contactWatch.Start();

                string messageStr = MyJson.stringFromBytes(message);
                UnityEngine.Debug.Log("Received : " + messageStr);

                byte[] outwardmessage = MyJson.stringToBytes("Message from Unity");
                server.SendFrame(outwardmessage);
            }
        }
        NetMQConfig.Cleanup();
    }

    public MyNetMQThread()
    {
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

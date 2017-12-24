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

    private void RecvBlocking(NetMQSocket socket, out byte[] message) {
        socket.Poll();
        bool success = socket.TryReceiveFrameBytes(out message);
        UnityEngine.Debug.Assert(success);
    }

    private void ListenerWork()
    {
        AsyncIO.ForceDotNet.Force();

        byte[] message;

        PairSocket server = new PairSocket();
        server.Bind("tcp://localhost:6000");
        // initial handshake
        RecvBlocking(server, out message);
        server.SendFrame(MyConvert.stringToBytes("okay, start"));
        // main loop
        while (!_listenerCancelled)
        {
            // Connected = _contactWatch.ElapsedMilliseconds < ContactThreshold;

            // if (!server.TryReceiveFrameBytes(out message)) continue;
            RecvBlocking(server, out message);

            string messageStr = MyConvert.stringFromBytes(message);
            UnityEngine.Debug.Log("Received : " + messageStr);

            RPCObject obj = MyConvert.rpcobj(message); // TODO: expected to throw an error sometimes

            // garbage reply:
            byte[] outwardmessage = MyConvert.stringToBytes("Message from Unity");
            server.SendFrame(outwardmessage);


            //_contactWatch.Reset();
            //_contactWatch.Start();
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

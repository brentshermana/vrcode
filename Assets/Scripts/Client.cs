using System.Collections.Concurrent;
using System.Threading;
using NetMQ;
using UnityEngine;
using NetMQ.Sockets;

public class Client : MonoBehaviour
{
    private NetMQListener _netMqListener;

    private void HandleMessage(string message)
    {
        // TODO
    }

    private void Start()
    {
        _netMqListener = new NetMQListener(HandleMessage);
        _netMqListener.Start();
    }

    private void Update()
    {
        _netMqListener.Update();
    }

    private void OnDestroy()
    {
        if (_netMqListener != null)
            _netMqListener.Stop();
    }
}

public class NetMQListener
{
    private readonly Thread _listenerWorker;

    private bool _listenerCancelled;

    public delegate void MessageDelegate(string message);

    private readonly MessageDelegate _messageDelegate;

    private readonly ConcurrentQueue<string> _messageQueue = new ConcurrentQueue<string>();

    private bool sending = true;

    public NetMQListener(MessageDelegate messageDelegate)
    {
        _messageDelegate = messageDelegate;
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

    private void ListenerWork()
    {
        AsyncIO.ForceDotNet.Force();
        using (var subSocket = new RequestSocket())
        {
            subSocket.Options.ReceiveHighWatermark = 1000;
            subSocket.Connect("tcp://localhost:6000");
            // subSocket.Subscribe("");
            while (!_listenerCancelled)
            {
                string frameString = "blah";
                bool success = false;

                if (!sending) {
                    success = subSocket.TryReceiveFrameString(out frameString);
                }
                else {
                    success = OutgoingSocketExtensions.TrySendFrame(subSocket, frameString);
                }

                // if the transaction was successful, 
                if (success) {
                    sending = !sending;
                }
            }
            subSocket.Close();
        }
        NetMQConfig.Cleanup();
    }

    public void Update()
    {
        bool success = true;
        while (success && _messageQueue.Count > 0)
        {
            string message = _messageQueue.TryDequeue(ref success);
            if (success)
            {
                _messageDelegate(message);
            }
        }
    }
}

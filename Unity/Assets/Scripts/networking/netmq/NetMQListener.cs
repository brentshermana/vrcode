using System.Threading;
using NetMQ;
using NetMQ.Sockets;

namespace vrcode.networking.netmq
{
    public class NetMQListener
    {
        private readonly Thread _listenerWorker;

        private bool _listenerCancelled;

        public delegate void MessageDelegate(string message);

        private readonly MessageDelegate _messageDelegate;

        private readonly vrcode.datastructures.concurrent.ConcurrentQueue<string> _messageQueue = new vrcode.datastructures.concurrent.ConcurrentQueue<string>();

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
}
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using NetMQ;
using NetMQ.Sockets;
using vrcode.datastructures.concurrent;
using vrcode.networking.message;

namespace vrcode.networking.netmq
{
    
    
    public class NetMqRpcIO
    {
        
        private readonly Thread worker;

        private bool should_quit = false;

        // waitspan is the maximum tolerable latency between a user-level interrupt
        // request and the thread's conclusion
        private TimeSpan waitSpan = new TimeSpan(0, 0, 0, 0, 100); // 100 millis;
        private int message_index = 1;

        private NetMQSocket socket;

        private string socket_number;

        // intentionally public, 
        public ConcurrentQueue<RPCMessage> request_queue;
        public ConcurrentQueue<RPCMessage> response_queue;
        
        
        public NetMqRpcIO(string socket_number)
        {
            this.socket_number = socket_number;

            this.response_queue = new ConcurrentQueue<RPCMessage>();
            this.request_queue = new ConcurrentQueue<RPCMessage>();

            worker = new Thread(Work);
            worker.IsBackground = true; // hopefully this keeps thread from crashing unity
        }

        

        private void checkForInterrupt() {
            if (should_quit) {
                NetMQConfig.Cleanup();
                Thread.CurrentThread.Abort();
            }
        }

        // actual blocking recvs exist, but don't check for thread interrupt requests
        private byte[] blockingRecv(NetMQSocket socket) {
            byte[] bytes;
            //UnityEngine.Debug.Log("Try Recieve");
            while (!socket.TryReceiveFrameBytes(waitSpan, out bytes)) {
                checkForInterrupt();
                //UnityEngine.Debug.Log("Try Recieve");
            }
            return bytes;
        }
        private void blockingSend(NetMQSocket socket, byte[] data) {
            while (!OutgoingSocketExtensions.TrySendFrame(socket, waitSpan, data))
            {
                checkForInterrupt();
            }
        }

//        // TODO: what to do with return value??
//        private string interaction (RPCMessage rpc) {
//
//            // get the request from frontend:
//            RPCMessage req = this.request_queue.;
//            // make sure it has the right id
//            req.id = i;
//            i += 1;
//            // send it to backend:
//            blockingSend(socket, MyConvert.rpcobj(req));
//            // now wait for response
//            while (true) {
//                byte[] recvBuf = blockingRecv(socket);
//                RPCMessage response = MyConvert.rpcobj(recvBuf);
//                UnityEngine.Debug.Log("Response:\n" + response.ToString());
//                if (response.id == req.id && response.method == req.method) {
//                    if (response.error != null)
//                    {
//                        UnityEngine.Debug.LogError("Response error for method " + req.method + " : " + response.error);
//                    }
//                    else
//                    {
//                        UnityEngine.Debug.Log("...Response is the desired return value: " + response.result);
//                        responseQueue.Enqueue(new InteractionResult(req.method, response.result));
//                        return response.result;
//                    }
//                }
//                else if (response.result == null) {
//                    UnityEngine.Debug.Log("...Response is a request");
//                    // request... should be processed now
//                    processMessage(recvBuf);
//                }
//                else if (response.id != req.id) {
//                    UnityEngine.Debug.LogError("Response had id " + response.id + ", but was expecting " + req.id);
//                }
//                else {
//                    UnityEngine.Debug.LogError("Unknown State!");
//                }
//            }
//        }

        private void processResult(byte[] message)
        {
            RPCMessage rpc;
            try
            {
                rpc = MyConvert.rpcobj(message);
            }
            catch (Exception e)
            {
                UnityEngine.Debug.LogError(e.Message + "\n" + e.StackTrace);
                return;
            }
            response_queue.Enqueue(rpc);
        }

        private void Work()
        {
            UnityEngine.Debug.Log("ZeroMQ Starting");
            AsyncIO.ForceDotNet.Force();
            socket = new PairSocket();
            socket.Bind("tcp://*:" + socket_number);
            UnityEngine.Debug.Log("ZeroMQ Bind");

            try {
                // initial handshake
                UnityEngine.Debug.Log("ZeroMQ Starting Handshake");
                blockingRecv(socket);
                UnityEngine.Debug.Log("ZeroMQ Handshake Recv");
                blockingSend(socket, MyConvert.stringToBytes("handshake"));
                UnityEngine.Debug.Log("Handshake Complete");

                // main loop
                while (!should_quit)
                {
                    // send
                    RPCMessage message = null;
                    bool success = false;
                    while (!success && !should_quit)
                    {
                        //UnityEngine.Debug.Log("Try Deque...");
                        message = request_queue.TryDeque(ref success, waitSpan);
                        checkForInterrupt();
                    }
                    checkForInterrupt();
                    UnityEngine.Debug.Log("NetMqRpcIO: Request successfully sent");
                    byte[] request = MyConvert.stringToBytes(MyConvert.tojson(message));
                    socket.SendFrame(request);
                    
                    // recv
                    byte[] newMsg = blockingRecv(socket);
                    UnityEngine.Debug.Log("NetMqRpcIO: Response successfully recieved");
                    processResult(newMsg);
                }
            }
            catch (Exception e) // catch all for exception reporting outside of builtin unity functions
            {
                UnityEngine.Debug.LogError(e);
            }

            NetMQConfig.Cleanup();
        }

        public void Start()
        {
            should_quit = false;
            worker.Start();
        }

        public void Stop()
        {
            UnityEngine.Debug.Log("STOP SIGNAL");
            should_quit = true;
            //worker.Interrupt(); // just added... might be a problem
            //worker.Join();
            if (socket != null)
                socket.Close();

            //  Note: both of the following cause the unity editor to hang/crash
            //NetMQConfig.Cleanup();
            //_listenerWorker.Join();
        }
    }
}

// an example of what a subclass's implementation of processResult() might look like:

// {
//            UnityEngine.Debug.Log(rpc);
//
//            byte[] ret = null;
//
//            // incoming message denotes an error that was thrown by the debugger's code
//            if (rpc.error != null)
//            {
//                debuggerErrorQueue.Enqueue(
//                    new DebuggerError(
//                        rpc.args[0],
//                        rpc.args[1]
//                    )
//                );
//            }
//            // ordinary and expected messages, handle accordingly
//            else
//            {
//                switch (rpc.method)
//                {
//                    case "dbquit":
//                        quitFlag = true;
//                        ret = MyConvert.stringToBytes("closing handshake");
//                        break;
//                    case "interaction":
//                        // tell the frontend to send a command
//                        interaction(rpc);
//                        break;
//                    case "startup":
//                        // TODO: store args given
//                        RPCMessage request = RPCMessage.Request("run", new List<string>(), i);
//                        i += 1;
//                        ret = MyConvert.rpcobj(request);
//                        break;
//                    case "exception":
//                        programErrorQueue.Enqueue(
//                            new ProgramError(
//                                rpc.args[0], // title
//                                rpc.args[1], // type
//                                rpc.args[2], // value
//                                rpc.args[3], // trace
//                                rpc.args[4]  // message
//                            )
//                        );
//                        break;
//                    case "readline":
//                        readFlag = true;
//                        // will block until user inserts into queue
//                        string line = stdinQueue.Dequeue();
//                        rpc.result = MyConvert.tojson(line);
//                        ret = MyConvert.rpcobj(rpc);
//                        break;
//                    case "write":
//                        foreach (string s in rpc.args)
//                        {
//                            stdoutQueue.Enqueue(s);
//                        }
//                        break;
//                    case "ping":
//                        //TODO: test
//                        rpc.result = rpc.args[0];
//                        ret = MyConvert.rpcobj(rpc);
//                        break;
//                    default:
//                        UnityEngine.Debug.LogError("Unknown method name '" + rpc.method + "'");
//                        break;
//                }
//            }
//
//            // send the result
//            if (ret != null)
//            {
//                blockingSend(server, ret);
//            }
//        }
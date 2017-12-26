﻿using System.Diagnostics;
using System.Threading;
using NetMQ;
using NetMQ.Sockets;
using UnityEngine;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System;
using DBNotif;

public class Server : MonoBehaviour
{
    public bool Connected;
    public GuiDBFrontend Frontend;
    private NetMQPublisher _netMqPublisher;
    private string _response;

    private void Start()
    {
        _netMqPublisher = new NetMQPublisher();
        _netMqPublisher.Start();
    }

    private void Update()
    {
        // helpful indicator visible from the unity editor's inspector
        Connected = _netMqPublisher.Connected;

        //check for stdout
        bool isStdout = false;
        string stdout = _netMqPublisher.stdoutQueue.TryDequeue(ref isStdout);
        if (isStdout) {
            Frontend.Stdout(stdout);
        }
        //check for errors
        if (_netMqPublisher.programErrorQueue.Count > 0) {
            bool success = true;
            while (success)
            {
                ProgramError err = _netMqPublisher.programErrorQueue.TryDequeue(ref success);
                if (success)
                {
                    Frontend.ProgramErrorNotif(err);
                }
            }
        }
        if (_netMqPublisher.debuggerErrorQueue.Count > 0) {
            bool success = false;
            while (success) {
                DebuggerError err = _netMqPublisher.debuggerErrorQueue.TryDequeue(ref success);
                if (success)
                {
                    Frontend.DebuggerErrorNotif(err);
                }
            }
        }

        // check flags
        if (_netMqPublisher.readFlag) {
            _netMqPublisher.readFlag = false;
            Frontend.ReadReady(_netMqPublisher.stdinQueue);
        }
        if (_netMqPublisher.interactionFlag) {
            _netMqPublisher.interactionFlag = false;
            Frontend.InteractionReady(_netMqPublisher.interactionArgs, _netMqPublisher.interactionQueue);
        }
        if (_netMqPublisher.quitFlag) {
            // don't reset quit flag, it indicates that NetMQ communication should cease
            Frontend.DBQuit();
        }
    }

    private void OnDestroy()
    {
        _netMqPublisher.Stop();
    }
}

public class NetMQPublisher
{
    private readonly Thread _listenerWorker;

    private bool _listenerCancelled;

    private readonly Stopwatch _contactWatch;

    private const long ContactThreshold = 1000;

    public bool Connected;

    bool sending = false;

    private TimeSpan waitSpan;
    private int i = 1;

    private NetMQSocket server;

    public ConcurrentQueue<byte[]> sendQueue = new ConcurrentQueue<byte[]>();
    public ConcurrentQueue<byte[]> recvQueue = new ConcurrentQueue<byte[]>();

    public ConcurrentQueue<RPCObject> interactionQueue = new ConcurrentQueue<RPCObject>();

    public ConcurrentQueue<string> stdoutQueue = new ConcurrentQueue<string>();
    public ConcurrentQueue<string> stdinQueue = new ConcurrentQueue<string>();

    public ConcurrentQueue<ProgramError> programErrorQueue
                                    = new ConcurrentQueue<ProgramError>();
    public ConcurrentQueue<DebuggerError> debuggerErrorQueue
                                    = new ConcurrentQueue<DebuggerError>();

    public InteractionArgs interactionArgs = null;

    public bool interactionFlag = false;
    public bool quitFlag = false;
    public bool readFlag = false;

    private Queue<byte[]> notifications = new Queue<byte[]>();
    private string nextOutgoingMessage = null;

    private void checkForInterrupt() {
        if (_listenerCancelled) {
            NetMQConfig.Cleanup();
            Thread.CurrentThread.Abort();
        }
    }

    // actual blocking recvs exist, but don't check for thread interrupt requests
    private byte[] blockingRecv(NetMQSocket socket) {
        byte[] bytes;
        while (!socket.TryReceiveFrameBytes(waitSpan, out bytes)) {
            checkForInterrupt();
        }
        Connected = _contactWatch.ElapsedMilliseconds < ContactThreshold;
        return bytes;
    }
    private void blockingSend(NetMQSocket socket, byte[] data) {
        while (!OutgoingSocketExtensions.TrySendFrame(socket, waitSpan, data))
        {
            checkForInterrupt();
        }
        Connected = _contactWatch.ElapsedMilliseconds < ContactThreshold;
    }

    // TODO: what to do with return value??
    private string interaction (RPCObject rpc) {
        UnityEngine.Debug.Log("Server Interaction args: " + rpc.args);
        // signal to frontend that it should send a command to backend:
        this.interactionArgs = new InteractionArgs(
                        rpc.args[0],
                        rpc.args[1],
                        rpc.args[2]
                    );
        this.interactionFlag = true;

        // get the request from frontend:
        RPCObject req = this.interactionQueue.Dequeue();
        // make sure it has the right id
        req.id = i;
        i += 1;
        // send it to backend:
        blockingSend(server, MyConvert.rpcobj(req));
        // now wait for response
        while (true) {
            byte[] recvBuf = blockingRecv(server);
            RPCObject response = MyConvert.rpcobj(recvBuf);
            if (response.id == -1) {
                // notification... should be processed later (TODO: why?)
                this.notifications.Enqueue(recvBuf);
            }
            else if (response.result != null) {
                // request... should be processed now
                processMessage(recvBuf);
            }
            else if (response.id != req.id) {
                UnityEngine.Debug.LogError("Response had id " + response.id + ", but was expecting " + req.id);
            }
            else if (response.error != null) {
                UnityEngine.Debug.LogError("Response error for method " + req.method + " : " + response.error);
            }
            else {
                return response.result;
            }
        }
    }

    private void processMessage(byte[] message)
    {
        RPCObject rpc;
        try
        {
            rpc = MyConvert.rpcobj(message);
        }
        catch (Exception e)
        {
            UnityEngine.Debug.LogError(e.Message + "\n" + e.StackTrace);
            return;
        }
        processMessage(rpc);
    }
    private void processMessage(RPCObject rpc) {
        byte[] ret = null;

        // incoming message denotes an error that was thrown by the debugger's code
        if (rpc.error != null)
        {
            debuggerErrorQueue.Enqueue(
                new DebuggerError(
                    rpc.args[0],
                    rpc.args[1]
                )
            );
        }
        // ordinary and expected messages, handle accordingly
        else
        {
            switch (rpc.method)
            {
                case "dbquit":
                    quitFlag = true;
                    ret = MyConvert.stringToBytes("closing handshake");
                    break;
                case "interaction":
                    // tell the frontend to send a command
                    interaction(rpc);
                    break;
                case "startup":
                    // TODO: store args given
                    RPCObject request = RPCObject.Request("run", new List<string>(), i);
                    i += 1;
                    ret = MyConvert.rpcobj(request);
                    break;
                case "exception":
                    programErrorQueue.Enqueue(
                        new ProgramError(
                            rpc.args[0], // title
                            rpc.args[1], // type
                            rpc.args[2], // value
                            rpc.args[3], // trace
                            rpc.args[4]  // message
                        )
                    );
                    break;
                case "readline":
                    readFlag = true;
                    // will block until user inserts into queue
                    string line = stdinQueue.Dequeue();
                    rpc.result = line;
                    ret = MyConvert.rpcobj(rpc);
                    break;
                case "write":
                    foreach (string s in rpc.args)
                    {
                        stdoutQueue.Enqueue(s);
                    }
                    break;
                case "ping":
                    //TODO: test
                    rpc.result = rpc.args[0];
                    ret = MyConvert.rpcobj(rpc);
                    break;
                default:
                    UnityEngine.Debug.LogError("Unknown method name '" + rpc.method + "'");
                    break;
            }
        }

        // send the result
        if (ret != null)
        {
            blockingSend(server, ret);
        }
    }


    private void ListenerWork()
    {
        UnityEngine.Debug.Log("ZeroMQ Starting");
        AsyncIO.ForceDotNet.Force();
        server = new PairSocket();
        server.Bind("tcp://*:6000");
        UnityEngine.Debug.Log("ZeroMQ Bind");

        try {
            // initial handshake
            UnityEngine.Debug.Log("ZeroMQ Starting Handshake");
            blockingRecv(server);
            UnityEngine.Debug.Log("ZeroMQ Handshake Recv");
            blockingSend(server, MyConvert.stringToBytes("handshake"));
            UnityEngine.Debug.Log("Handshake Complete");

            // main loop
            while (!_listenerCancelled && !quitFlag)
            {
                // process notifications, if any
                if (notifications.Count > 0) {
                    processMessage(notifications.Dequeue());
                }
                // otherwise read new message
                else {
                    byte[] newMsg = blockingRecv(server);
                    processMessage(newMsg);
                }
            }
        }
        catch (Exception e) // catch all for exception reporting outside of builtin unity functions
        {
            UnityEngine.Debug.LogError(e);
        }

        NetMQConfig.Cleanup();
    }

    public NetMQPublisher()
    {
        _contactWatch = new Stopwatch();
        _contactWatch.Start();
        _listenerWorker = new Thread(ListenerWork);
        _listenerWorker.IsBackground = true; // hopefully this keeps thread from crashing unity

        // waitspan is the maximum tolerable latency between a user-level interrupt
        // request and the thread's conclusion
        waitSpan = new TimeSpan(0, 0, 0, 0, 100); // 100 millis
    }

    public void Start()
    {
        _listenerCancelled = false;
        _listenerWorker.Start();
    }

    public void Stop()
    {
        _listenerCancelled = true;
        server.Close();

        //  Note: both of the following cause the unity editor to hang/crash
        //NetMQConfig.Cleanup();
        //_listenerWorker.Join();
    }
}

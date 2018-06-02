using UnityEngine;
using System.Threading;
using System.Collections;
using System.Timers;
using NetMQ; // for NetMQConfig
using NetMQ.Sockets;

public class NetMQServer : MonoBehaviour
{

    private ResponseSocket responseSocket;

    Thread server_thread;
    private Object thisLock_ = new Object();
    bool stop_thread_ = false;

    void Start()
    {
        Debug.Log("Start a Response thread.");
        server_thread = new Thread(server);
        server_thread.Start();
    }

    // Client thread which does not block Update()
    void server()
    {
        AsyncIO.ForceDotNet.Force();
        NetMQConfig.ManualTerminationTakeOver();
        NetMQConfig.ContextCreate(true);

        string msg;
        var timeout = new System.TimeSpan(0, 0, 1); //1sec

        Debug.Log("Connect to the client.");
        responseSocket = new ResponseSocket("@tcp://*:50021");
        bool is_connected = responseSocket.TryReceiveFrameString(timeout, out msg);

        Debug.Log("is connected: " + is_connected);
        while (is_connected && stop_thread_ == false)
        {
            Debug.Log("Request a message.");
            responseSocket.TrySendFrame(timeout, "msg");
            is_connected = responseSocket.TryReceiveFrameString(timeout, out msg);
            Debug.Log("Sleep");
            Thread.Sleep(1000);
        }
        Debug.Log("Closing the socket");
        responseSocket.Close();
    }

    void Update()
    {
        /// Do normal Unity stuff
    }

    void OnDestroy()
    {
        
    }
    void OnDisable()
    {
        
    }
    void OnApplicationQuit()
    {
        cleanup();
    }

    void cleanup()
    {
        Debug.Log("cleanup");
        lock (thisLock_) stop_thread_ = true;
        server_thread.Join();
        Debug.Log("Server thread joined");
        NetMQConfig.Cleanup();
        Debug.Log("Server cleanup");
        NetMQConfig.ContextTerminate();
        Debug.Log("Server Context Terminated");
    }
    

}
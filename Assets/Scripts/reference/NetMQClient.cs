using UnityEngine;
using System.Threading;
using System.Collections;
using System.Timers;
using NetMQ; // for NetMQConfig
using NetMQ.Sockets;

public class NetMQClient : MonoBehaviour
{

    Thread client_thread_;
    private Object thisLock_ = new Object();
    bool stop_thread_ = false;

    void Start()
    {
        Debug.Log("Start a request thread.");
        client_thread_ = new Thread(client);
        client_thread_.Start();
    }

    // Client thread which does not block Update()
    void client()
    {
        AsyncIO.ForceDotNet.Force();
        NetMQConfig.ManualTerminationTakeOver();
        NetMQConfig.ContextCreate(true);

        string msg;
        var timeout = new System.TimeSpan(0, 0, 20); //20sec

        Debug.Log("Connect to the server.");
        var requestSocket = new RequestSocket(">tcp://0.0.0.0:50020");
        requestSocket.TrySendFrame(timeout, "SUB_PORT");
        bool is_connected = requestSocket.TryReceiveFrameString(timeout, out msg);

        while (is_connected && stop_thread_ == false)
        {
            Debug.Log("Request a message.");
            requestSocket.SendFrame("msg");
            is_connected = requestSocket.TryReceiveFrameString(timeout, out msg);
            Debug.Log("Sleep");
            Thread.Sleep(1000);
        }
        /*
        requestSocket.Close();
        Debug.Log("ContextTerminate.");
        NetMQConfig.ContextTerminate();
        */
    }

    void Update()
    {
        /// Do normal Unity stuff
    }

    /*
    void OnApplicationQuit()
    {
        lock (thisLock_) stop_thread_ = true;
        client_thread_.Join();
        Debug.Log("Quit the thread.");
    }
    */

}

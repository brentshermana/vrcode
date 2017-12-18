using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using NetMQ;
using NetMQ.Sockets;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using UnityEngine;


public class RealNetMqServer : MonoBehaviour {

    private ConcurrentQueue<ActionableJsonMessage> inqueue = new ConcurrentQueue<ActionableJsonMessage>();
    private ConcurrentQueue<ActionableJsonMessage> outqueue = new ConcurrentQueue<ActionableJsonMessage>();

    private NetMQThread actor;

	// Use this for initialization
	void Start () {
        actor = new NetMQThread(inqueue, outqueue);
        actor.Start();

        //assign an arbitrary working directory:
        ActionableJsonMessage setwd = new ActionableJsonMessage(
            "Editor",
            "Directory",
            new string[] { @"D:\dev\python_workspace"  }
        );
        SendToBackend(setwd);
    }

    private void OnDestroy()
    {
        actor.Stop();
    }

    public void SendToBackend(ActionableJsonMessage msg)
    {
        UnityEngine.Debug.Log("server sending to backend: " + msg.ToString());
        outqueue.Enqueue(msg);
    }

    //TODO: in the future, netmqserver should have the responsibility
    // of sorting incoming messages into separate queues by recipient?
    public ActionableJsonMessage AttemptDequeue()
    {
        bool success = true;
        ActionableJsonMessage msg = inqueue.TryDequeue(ref success);
        if (success) return msg;
        else return null;
    }

    // Update is called once per frame
    void Update () {
        //TODO: act on messages!
        bool success = true;
        while (success)
        {
            ActionableJsonMessage msg = inqueue.TryDequeue(ref success);
            if (success && msg.Type != "NOP")
            {
                UnityEngine.Debug.Log("Server Received a Message -> " + msg.ToString());
            }
        }
	}
}

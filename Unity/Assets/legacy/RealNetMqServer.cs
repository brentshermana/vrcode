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
using vrcode.networking.netmq;


public class RealNetMqServer : MonoBehaviour {
    /*
    private ConcurrentQueue<ActionableJsonMessage> inqueue = new ConcurrentQueue<ActionableJsonMessage>();
    private ConcurrentQueue<ActionableJsonMessage> outqueue = new ConcurrentQueue<ActionableJsonMessage>();
    */

    private MyNetMQThread thread;

	// Use this for initialization
	void Start () {
        thread = new MyNetMQThread();
        thread.Start();
    }

    private void OnDestroy()
    {
        thread.Stop();
    }

    public void SendToBackend(ActionableJsonMessage msg)
    {
        //UnityEngine.Debug.Log("server sending to backend: " + msg.ToString());
        //outqueue.Enqueue(msg);
    }

    //TODO: in the future, netmqserver should have the responsibility
    // of sorting incoming messages into separate queues by recipient?
    //public ActionableJsonMessage AttemptDequeue()
    //{
        //bool success = true;
        //ActionableJsonMessage msg = inqueue.TryDequeue(ref success);
        //if (success) return msg;
        //else return null;
    //}

    // Update is called once per frame
    void Update () {
        
	}
}

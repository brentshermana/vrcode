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
	}

    private void OnDestroy()
    {
        actor.Stop();
    }

    // Update is called once per frame
    void Update () {
        //TODO: act on messages!
        bool success = true;
        while (success)
        {
            ActionableJsonMessage msg = inqueue.TryDequeue(ref success);
            if (success)
            {
                UnityEngine.Debug.Log("Server Received a Message -> " + msg.ToString());
            }
        }
	}
}

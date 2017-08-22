using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class JsonClientTest : MonoBehaviour {

    private JsonClient client;
    private QueueWithSem<ActionableJsonMessage> q;
    private ActionableJsonMessage msg;
    private bool success = false;

	// Use this for initialization
	void Start () {
        msg = new ActionableJsonMessage();
        q = new QueueWithSem<ActionableJsonMessage>();
        client = new JsonClient(process, q);
        success = q.putNoWait(msg);
	}

    void process(ActionableJsonMessage message)
    {
        Debug.Log("Client Processing Json Message: " + message);
    }
	
	// Update is called once per frame
	void Update () {
		if (!success)
        {
            success = q.putNoWait(msg);
        }
	}
}

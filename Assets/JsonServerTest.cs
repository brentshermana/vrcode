using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class JsonServerTest : MonoBehaviour {

    private JsonServer server;

    private bool success = false;

    private QueueWithSem<ActionableJsonMessage> q;

	// Use this for initialization
	void Start () {
        Debug.Log("Server Test Init");
        q = new QueueWithSem<ActionableJsonMessage>();
        server = new JsonServer(nothing, q);
        success = q.putNoWait(new ActionableJsonMessage());
	}

    void nothing(ActionableJsonMessage message)
    {
        Debug.Log("Success Read!");
    }
	
	// Update is called once per frame
	void Update () {
		if (! success)
        {
            success = q.putNoWait(new ActionableJsonMessage());
        }
	}
}

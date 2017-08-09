using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using System;
using System.Net.Sockets;
using System.Net;

public class NetworkingClientDemo : MonoBehaviour {

	private IPHostEntry localhost;
	private TcpClient client;

	// Use this for initialization
	void Start () {
		//localhost = Dns.GetHostEntry (Dns.GetHostName());
		//IPAddress[] addressList = localhost.AddressList;
		TcpClient _client = new TcpClient ();
		_client.BeginConnect (IPAddress.Loopback, 5555, new AsyncCallback(acceptCallback), _client);
	}

	private void acceptCallback(IAsyncResult result) {
		if (result.IsCompleted) {
			Debug.Log("Client Completed!");
			TcpClient _client = (TcpClient)result.AsyncState;
			_client.EndConnect (result);
			client = _client; //this is where client is initialized
			Debug.Log("Client connected");
		}
		else {
			Debug.Log("Error!");
		}
	}
	
	// Update is called once per frame
	void Update () {
		
	}
}

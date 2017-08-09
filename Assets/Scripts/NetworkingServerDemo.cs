using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;

public class NetworkingServerDemo : MonoBehaviour {


	private string buf2str (byte[] buf) {
		return System.Text.Encoding.UTF8.GetString (buf);
	}
	private byte[] str2buf (string str) {
		return System.Text.Encoding.UTF8.GetBytes (str);
	}

	private IPHostEntry localhost;

	// Use this for initialization
	void Start () {
		//localhost = Dns.GetHostEntry (Dns.GetHostName());
		//IPAddress[] addressList = localhost.AddressList;
		//Debug.Log ("Length " + addressList.Length);
		TcpListener listener = new TcpListener(IPAddress.Loopback, 5555);
		listener.Start ();

		IAsyncResult task = listener.BeginAcceptTcpClient (new AsyncCallback(acceptCallback), listener);
	}

	private void acceptCallback(IAsyncResult result) {
		if (result.IsCompleted) {
			Debug.Log("ServerCompleted!");
			TcpListener listener = (TcpListener)result.AsyncState;
			listener.EndAcceptTcpClient (result);
			Debug.Log ("Server Connect");
		}
		else {
			Debug.Log("Error!");
		}
	}

	
	// Update is called once per frame
	void Update () {
		
	}
}

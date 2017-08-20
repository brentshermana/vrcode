using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;


using Newtonsoft.Json;

public class NetworkingServerDemo : MonoBehaviour {

    private static int MIN_BUF = 2000;

    private Socket sock;


	private string buf2str (byte[] buf) {
		return System.Text.Encoding.UTF8.GetString (buf);
	}
	private byte[] str2buf (string str) {
		return System.Text.Encoding.UTF8.GetBytes (str);
	}

    private byte[] buffer;

	private IPHostEntry localhost;

	// Use this for initialization
	void Start () {
        //localhost = Dns.GetHostEntry (Dns.GetHostName());
        //IPAddress[] addressList = localhost.AddressList;
        //Debug.Log ("Length " + addressList.Length);

        buffer = new byte[4];

		TcpListener listener = new TcpListener(IPAddress.Loopback, 5555);
		listener.Start ();

		IAsyncResult task = listener.BeginAcceptTcpClient (new AsyncCallback(acceptCallback), listener);
	}

	private void acceptCallback(IAsyncResult result) {
		if (result.IsCompleted) {
			Debug.Log("Server Completed!");
			TcpListener listener = (TcpListener)result.AsyncState;
			TcpClient client = listener.EndAcceptTcpClient (result);

            sock = client.Client;
            
			Debug.Log ("Server Connect");
            
            sock.BeginReceive(buffer, 0, 4, SocketFlags.None, new AsyncCallback(jsonsizecallback), new object());
		}
		else {
			Debug.Log("Error!");
		}
	}

    private void jsonsizecallback(IAsyncResult result)
    {
        sock.EndReceive(result);

        //at this point the size of the buffer is 4
        if (BitConverter.IsLittleEndian) Array.Reverse(buffer);
        int size = BitConverter.ToInt32(buffer, 0);
        Debug.Log("Server: Size is " + size);
        buffer = new byte[size];

        sock.BeginReceive(buffer, 0, size, SocketFlags.None, new AsyncCallback(jsoncallback), size);
    }

    private void jsoncallback(IAsyncResult result)
    {
        int size = (int)result.AsyncState;
        Debug.Log("Server: parsing json of size " + size);
        sock.EndReceive(result);
        string jsonstr = buf2str(buffer);
        ExampleJsonClass obj = JsonConvert.DeserializeObject<ExampleJsonClass>(jsonstr);

        Debug.Log("Server: Json: " + obj.ToString());
    }

	
	// Update is called once per frame
	void Update () {
		
	}
}

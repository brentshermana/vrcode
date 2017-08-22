using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using System;
using System.Net.Sockets;
using System.Net;

using Newtonsoft.Json;

public class NetworkingClientDemo : MonoBehaviour {

	private IPHostEntry localhost;
	private TcpClient client;
    private Socket sock;
    private byte[] buffer;

    private string buf2str(byte[] buf)
    {
        return System.Text.Encoding.UTF8.GetString(buf);
    }
    private byte[] str2buf(string str)
    {
        return System.Text.Encoding.UTF8.GetBytes(str);
    }

    // Use this for initialization
    void Start () {
        //localhost = Dns.GetHostEntry (Dns.GetHostName());
        //IPAddress[] addressList = localhost.AddressList;

        //prep the json message to send
        ExampleJsonClass obj = new ExampleJsonClass(4, "Hello World!");
        string jsonstr = JsonConvert.SerializeObject(obj);
        byte[] json_buf = str2buf(jsonstr);
        byte[] sizebytes = BitConverter.GetBytes(json_buf.Length);
        int size_len = sizebytes.Length; //presumably 4
        if (BitConverter.IsLittleEndian) Array.Reverse(sizebytes); //big endian
        Array.Resize<byte>(ref sizebytes, size_len + json_buf.Length);
        Array.Copy(json_buf, 0, sizebytes, size_len, json_buf.Length);
        buffer = sizebytes;

		TcpClient _client = new TcpClient ();
		_client.BeginConnect (IPAddress.Loopback, 5555, new AsyncCallback(acceptCallback), _client);
	}

	private void acceptCallback(IAsyncResult result) {
		if (result.IsCompleted) {
			Debug.Log("Client Completed!");
			TcpClient _client = (TcpClient)result.AsyncState;
			_client.EndConnect (result);
			client = _client; //this is where client is initialized

            sock = client.Client;
            
            Debug.Log("Client connected");

            Debug.Log("Client Begin Send");

            sock.BeginReceive(new byte[10], 0, 10, SocketFlags.None, new AsyncCallback(receivecallback), new object());

            sock.BeginSend(buffer, 0, buffer.Length, SocketFlags.None, new AsyncCallback(sendcomplete), new object());
        }
		else {
			Debug.Log("Error!");
		}
	}

    private void receivecallback(IAsyncResult result)
    {
        sock.EndReceive(result);
        Debug.Log("Client receive callback (Whoa)");
    }

    private void sendcomplete(IAsyncResult result)
    {
        Debug.Log("Client Finished Sending");
        sock.EndSend(result);
    }
	
	// Update is called once per frame
	void Update () {
		
	}
}

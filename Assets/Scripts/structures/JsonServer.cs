using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Net.Sockets;
using System.Net;

using Newtonsoft.Json;
using System.Threading;

public class JsonServer {

    private static int PORT = 5555;

    private Action<ActionableJsonMessage> messageProcessor;

    private Socket sock;

    private QueueWithSem<ActionableJsonMessage> outgoingQueue;

    private Semaphore queueSemaphore;

    //outgoing queue must be synchronized!
	public JsonServer(Action<ActionableJsonMessage> messageProcessor_, QueueWithSem<ActionableJsonMessage> outgoingQueue_)
    {
        messageProcessor = messageProcessor_;
        outgoingQueue = outgoingQueue_;

        TcpListener listener = new TcpListener(IPAddress.Loopback, PORT);
        listener.Start();

        listener.BeginAcceptTcpClient(new AsyncCallback(acceptCallback), listener);
    }

    private void acceptCallback(IAsyncResult result)
    {
        TcpListener listener = (TcpListener)result.AsyncState;
        TcpClient client = listener.EndAcceptTcpClient(result);
        sock = client.Client;
        listener.EndAcceptTcpClient(result);

        startRead();

        Thread sendThread = new Thread(new ThreadStart(sendingThread));
        sendThread.Start();
    }

    #region reading
    private void startRead()
    {
        byte[] buffer = new byte[4];
        sock.BeginReceive(buffer, 0, 4, SocketFlags.None, new AsyncCallback(jsonsizecallback), buffer);
    }

    private void jsonsizecallback(IAsyncResult result)
    {

        byte[] sizeBuffer = (byte[])result.AsyncState;
        sock.EndReceive(result);

        //at this point the size of the buffer is 4
        if (BitConverter.IsLittleEndian) Array.Reverse(sizeBuffer);
        int size = BitConverter.ToInt32(sizeBuffer, 0);
        Debug.Log("Server: Size of json object is " + size);
        byte[] jsonBuffer = new byte[size];

        sock.BeginReceive(jsonBuffer, 0, size, SocketFlags.None, new AsyncCallback(jsoncallback), jsonBuffer);
    }
    private void jsoncallback(IAsyncResult result)
    {
        byte[] jsonBuffer = (byte[])result.AsyncState;
        sock.EndReceive(result);

        Debug.Log("Server: parsing json of size " + jsonBuffer.Length);
        string jsonstr = buf2str(jsonBuffer);
        ActionableJsonMessage obj = JsonConvert.DeserializeObject<ActionableJsonMessage>(jsonstr);

        messageProcessor.Invoke(obj); // process the message

        Debug.Log("Server: Read Json Message" + obj.ToString());

        startRead(); //read the next message
    }
    #endregion

    #region writing

    private void sendingThread()
    {
        while (true)
        {
            ActionableJsonMessage json = outgoingQueue.getWait(); //semaphore will block here
            string jsonstr = JsonConvert.SerializeObject(json, Formatting.None);
            byte[] buffer = str2buf(jsonstr);
            sock.BeginSend(buffer, 0, buffer.Length, SocketFlags.None, new AsyncCallback(sendHandler), 0);
        }
    }

    private void sendHandler(IAsyncResult result)
    {
        Debug.Log("Json server sent message");
        sock.EndSend(result);
    }

    #endregion

    #region encoding
    private string buf2str(byte[] buf)
    {
        return System.Text.Encoding.UTF8.GetString(buf);
    }
    private byte[] str2buf(string str)
    {
        return System.Text.Encoding.UTF8.GetBytes(str);
    }
#endregion


}

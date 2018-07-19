using System;
using System.Collections;
using UnityEngine;
using vrcode.networking.message;
using System.Collections.Generic;

namespace vrcode.networking.netmq
{
    /*
     * MonoBehaviour which wraps NetMqRpcIO functions to ensure that Responses are acted upon
     * in a threadsafe manner (during Update())
     */
    public class NetMqRpcIOAdapter
    {
        private string port_number;
        
        private NetMqRpcIO io;
        
        // sometimes messages we want to send through io will not work
        // as fast as we might want, so we should store them locally rather
        // than holding up Unity's Update(), which would impact frame rate
        private Queue<RPCMessage> outgoing_queue;
        
        public Queue<RPCMessage> incoming_queue;

        public NetMqRpcIOAdapter(string port_number)
        {
            this.port_number = port_number;
        }

        // should be called by the containing MonoBehaviour
        public void Start()
        {
            io = new NetMqRpcIO(port_number);
            io.Start();
            
            outgoing_queue = new Queue<RPCMessage>();
            incoming_queue = new Queue<RPCMessage>();
        }
        public void Update()
        {
            // Write requests to IO
            bool success = true;
            while (success && outgoing_queue.Count > 0)
            {
                success = io.request_queue.TryEnqueue(outgoing_queue.Peek());
                if (success)
                {
                    UnityEngine.Debug.Log("NetMqAdapter: request successfully sent!");
                    outgoing_queue.Dequeue();
                }
                    
            }

            // read results from queue
            success = true;
            while (success)
            {
                RPCMessage result = io.response_queue.TryDequeue(ref success);
                if (success)
                {
                    incoming_queue.Enqueue(result);
                    UnityEngine.Debug.Log("NetMqAdapter: Response successfully read!");
                }
                    
            }
            
        }
        public void Stop()
        {
            io.Stop();
        }

        public void SendMessage(RPCMessage message)
        {
            UnityEngine.Debug.Log("NetMqAdapter: request enqueued!");
            outgoing_queue.Enqueue(message);
        }


        
    }
}
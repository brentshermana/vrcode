using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;
using vrcode.datastructures.concurrent;
using vrcode.ide.debugger.frontend;
using vrcode.networking.message;
using Debug = UnityEngine.Debug;

namespace vrcode.networking.netmq
{
    /**
     * NewServer interacts with an IDBFrontend instance by performing necessary reads, writes,
     * and event raises. The IDBFrontend should have no need to operate on or even know about
     * the NewServer instance
     */
    public class NewServer : MonoBehaviour
    {
        [SerializeField] private string python_path;
        [SerializeField] private string backend_script_path;
        
        
        public bool Connected; // only exists to be visible in the editor
        private DBFrontend Frontend;
        private NewNetMQPublisher _netMqPublisher;
        private ConcurrentQueue<RPCMessage> ResultQueue;

        private bool active = false;

        private Process backend_process;

        public void StartDebugging(string debugged_script)
        {
            if (!active)
            {
                // start debugger backend
                backend_process = new Process();
                backend_process.StartInfo.FileName = python_path; // technically, the program we're launching is python
                //backend_process.StartInfo.UseShellExecute = false; //necessary for getting stdout,err
                //backend_process.StartInfo.RedirectStandardError = true;
                //backend_process.StartInfo.RedirectStandardOutput = true;
                backend_process.StartInfo.Arguments = backend_script_path + " " + debugged_script;
                backend_process.Start();
                
                //start frontend io
                _netMqPublisher = new NewNetMQPublisher();
                _netMqPublisher.Start();
                ResultQueue = _netMqPublisher.ResultQueue;

                active = true;
            }
            else
            {
                Debug.LogError("Invalid Request");
            }
        }

        public void StopDebugging()
        {
            if (active)
            {
                if (!backend_process.HasExited)
                {
                    backend_process.Kill(); // will this block?
                }
                active = false;
                _netMqPublisher.Stop();
            }
            else
            {
                Debug.LogError("Invalid Request");
            }
        }

        private void Start()
        {
            Frontend = GetComponent<DBFrontend>();
        }

        private void NotifyFrontend(RPCMessage result)
        {
            DebuggerError error = null;
            if (result.error != null)
            {
                error = new DebuggerError((string)result.args[0], (string)result.args[1]);
            }
            Frontend.OnResult(result, error);
            
        }

        private void Update()
        {
            if (active)
            {
                // helpful indicator visible from the unity editor's inspector
                Connected = _netMqPublisher.Connected;
                
                // read requests from the Frontend and pass them along
                bool success = true;
                while (Frontend.PeekRequest() != null && success)
                {
                    success = _netMqPublisher.RequestQueue.TryEnqueue(Frontend.PeekRequest());
                    if (success) Frontend.DequeueRequest();
                }
                // same thing for stdin
                success = true;
                while (Frontend.PeekStdin() != null && success)
                {
                    success = _netMqPublisher.stdinQueue.TryEnqueue(Frontend.PeekStdin());
                    if (success) Frontend.DequeueStdin();
                }
        
                //TODO: code for reading from queues is inelegant. We should be able to:
                //  1) combine queues so that we have as few queues as possible, and a
                //      unified way of dealing with them (as in frontend.Result())
                //  2) abstract out control flow for reading from queues into a func
                if (ResultQueue.Count > 0) {
                    success = true;
                    while (success) {
                        RPCMessage result = ResultQueue.TryDequeue(ref success);
                        if (success) {
                            NotifyFrontend(result);
                        }
                    }
                }
                //check for stdout
                bool isStdout = false;
                string stdout = _netMqPublisher.stdoutQueue.TryDequeue(ref isStdout);
                if (isStdout) {
                    Frontend.WriteStdout(stdout);
                }
                //check for errors
                if (_netMqPublisher.programErrorQueue.Count > 0) {
                    success = true;
                    while (success)
                    {
                        ProgramError err = _netMqPublisher.programErrorQueue.TryDequeue(ref success);
                        if (success)
                        {
                            Frontend.ProgramErrorNotif(err);
                        }
                    }
                }
                
                // check flags
                if (_netMqPublisher.readFlag) {
                    _netMqPublisher.readFlag = false;
                    Frontend.ReadReady();
                }
                if (_netMqPublisher.interactionFlag) {
                    _netMqPublisher.interactionFlag = false;
                    Frontend.InteractionReady(_netMqPublisher.interactionArgs);
                }
                if (_netMqPublisher.quitFlag) {
                    // don't reset quit flag, it indicates that NetMQ communication should cease
                    StopDebugging();
                    Frontend.DBQuit();
                }
                
                
        //            if (_netMqPublisher.debuggerErrorQueue.Count > 0) {
        //                bool success = false;
        //                while (success) {
        //                    DebuggerError err = _netMqPublisher.debuggerErrorQueue.TryDequeue(ref success);
        //                    if (success)
        //                    {
        //                        Frontend.DebuggerErrorNotif(err);
        //                    }
        //                }
        //            }
        
                
            }
            
        }

        private void OnDestroy()
        {
            StopDebugging();
        }
    }
}
using UnityEngine;
using System.Collections.Generic;
// using System;
using vrcode.datastructures.concurrent;
using vrcode.ide.debugger.frontend;
using vrcode.networking.message;

namespace vrcode.networking.netmq
{
    public class Server : MonoBehaviour
    {
        public bool Connected;
        public GuiDBFrontend Frontend;
        private NetMQPublisher _netMqPublisher;
        private string _response;
        private ConcurrentQueue<InteractionResult> ResultQueue = new ConcurrentQueue<InteractionResult>();

        private void Start()
        {
            _netMqPublisher = new NetMQPublisher(ResultQueue);
            _netMqPublisher.Start();
        }

        private void NotifyFrontend(InteractionResult result) {
            switch (result.Method)
            {
                case ("do_environment"):
                    //Frontend.Result(MyConvert.fromjson<DBEnvironment>(result.Result));
                    break;
                case ("do_where"):
                    //Frontend.Result(MyConvert.fromjson<DBStackTrace>(result.Result));
                    break;
                case ("do_eval"):
                    //Frontend.Result(new DBEvalResult(result.Result));
                    break;
                case ("do_list_breakpoint"):
                    //Frontend.Result(MyConvert.fromjson<List<DBBreakpoint>>(result.Result));
                    break;
                case ("do_exec"):
                    //Frontend.Result(new DBExecResult(result.Result));
                    break;
            }
        }

        private void Update()
        {
            // helpful indicator visible from the unity editor's inspector
            Connected = _netMqPublisher.Connected;

            //TODO: code for reading from queues is inelegant. We should be able to:
            //  1) combine queues so that we have as few queues as possible, and a
            //      unified way of dealing with them (as in frontend.Result())
            //  2) abstract out control flow for reading from queues into a func
            if (ResultQueue.Count > 0) {
                bool success = true;
                while (success) {
                    InteractionResult result = ResultQueue.TryDequeue(ref success);
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
                bool success = true;
                while (success)
                {
                    ProgramError err = _netMqPublisher.programErrorQueue.TryDequeue(ref success);
                    if (success)
                    {
                        Frontend.ProgramErrorNotif(err);
                    }
                }
            }
            if (_netMqPublisher.debuggerErrorQueue.Count > 0) {
                bool success = false;
                while (success) {
                    DebuggerError err = _netMqPublisher.debuggerErrorQueue.TryDequeue(ref success);
                    if (success)
                    {
                        //Frontend.DebuggerErrorNotif(err);
                    }
                }
            }

            // check flags
            if (_netMqPublisher.readFlag) {
                _netMqPublisher.readFlag = false;
                //Frontend.ReadReady(_netMqPublisher.stdinQueue);
            }
            if (_netMqPublisher.interactionFlag) {
                _netMqPublisher.interactionFlag = false;
                //Frontend.InteractionReady(_netMqPublisher.interactionArgs,  _netMqPublisher.interactionQueue);
            }
            if (_netMqPublisher.quitFlag) {
                // don't reset quit flag, it indicates that NetMQ communication should cease
                Frontend.DBQuit();
            }
        }

        private void OnDestroy()
        {
            _netMqPublisher.Stop();
        }
    }
}
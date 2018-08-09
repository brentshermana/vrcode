using System;
using vrcode.datastructures.concurrent;
using vrcode.networking.message;

using System.Collections.Generic;
using System.Diagnostics;
using System.Linq.Expressions;
using NUnit.Framework.Constraints;
using UnityEngine;
using vrcode.networking;
using vrcode.networking.netmq;

namespace vrcode.ide.debugger.frontend
{
    /**
     * Handy IDBFrontend subclass which abstacts out
     * the details of managing RPCMessages,
     * tracking/calling callbacks,
     * and performing queue reads and writes
     */
    
    [RequireComponent(typeof(NewServer))] // the server does all the essential reads and writes
    public abstract class DBFrontend : MonoBehaviour
    {
        

        private NewServer server;

        private Process backend_process;
        
        private Dictionary<int, Action<RPCMessage, DebuggerError>> callbacks;
        private Queue<RPCMessage> outgoing_messages;
        private Queue<string> stdinQueue;
        private int i;

        /**
         * The number of Stdin writes performed minus the number of stdin read requests.
         * We only need to prompt the user for input when this value becomes negative.
         *
         * For example, if the user preemptively enters some input, then the program asks
         * for it, we would not want to prompt the user
         */
        private int stdinSurplus;
        private int requestSurplus; // same idea as stdinSurplus
        
        #region monobehaviour_events

        public void Start()
        {
            server = GetComponent<NewServer>();
            callbacks = new Dictionary<int, Action<RPCMessage, DebuggerError>>();
            outgoing_messages = new Queue<RPCMessage>();
            i = 1;
            stdinSurplus = 0;
            requestSurplus = 0;
            stdinQueue = new Queue<string>();
        }

        public void Update()
        {
            if (backend_process != null)
            {
//                string stdout = backend_process.StandardOutput.ReadToEnd();
//                if (stdout.Length > 0) UnityEngine.Debug.Log(stdout);
//
//                string stderr = backend_process.StandardError.ReadToEnd();
//                if (stderr.Length > 0) UnityEngine.Debug.LogError(stderr);
            }
        }

        #endregion
        
        

        #region callback_management

        private void RegisterCallback(int id, Action<RPCMessage, DebuggerError> callback)
        {
            //UnityEngine.Debug.Log("Registering callback for request id " + id);
            callbacks[id] = callback;
        }
        private void InvokeCallback(int id, RPCMessage response, DebuggerError error)
        {
            UnityEngine.Debug.Log("Invoking callback for request id " + id);
            if (id < 1) return; // backend will send messages with id -1

            if (!callbacks.ContainsKey(id))
            {
                UnityEngine.Debug.LogError("ID Key " + id + " with method " + response.method + " not in dict!");
            }
            else
            {
                if (callbacks[id] != null) // this is acceptable, null just means 'don't do anything'
                    callbacks[id].Invoke(response, error);
                callbacks.Remove(id); // callbacks are only used once
            }
        }

        #endregion
        
        
        #region IDBFrontend_Implementation

        public void StartDebugging(string debugged_script)
        {
            server.StartDebugging(debugged_script);
        }
        
        private void SendRequest(string method, Action<RPCMessage, DebuggerError> callback)
        {
            SendRequest(method, new object[]{}, callback);
        }

        // because this method maintains the correct value of requestSurplus,
        // it MUST be the entry point into appending to outgoing_messages
        private void SendRequest(string method, object[] args, Action<RPCMessage, DebuggerError> callback)
        {
            RegisterCallback(i, callback);
            requestSurplus++;
            outgoing_messages.Enqueue(
                RPCMessage.Request(
                    method,
                    new List<object>(args),
                    i
                ));
            i++;
        }

        public RPCMessage PeekRequest()
        {
            if (outgoing_messages.Count == 0) return null;
            return outgoing_messages.Peek();
        }
        
        public RPCMessage DequeueRequest()
        {
            if (outgoing_messages.Count == 0) return null;
            return outgoing_messages.Dequeue();
        }

        public string PeekStdin()
        {
            if (stdinQueue.Count == 0) return null;
            return stdinQueue.Peek();
        }

        public string DequeueStdin()
        {
            if (stdinQueue.Count == 0) return null;
            return stdinQueue.Dequeue();
        }

        public abstract void ProgramErrorNotif(ProgramError err);

        public void ReadReady()
        {
            stdinSurplus--;
            if (stdinSurplus < 0)
                OnNeedStdin();
        }

        // only called when the backend requests std input *and* it isn't already
        // available
        public abstract void OnNeedStdin();

        public abstract void DisplayDebuggerState(InteractionArgs args);

        public void InteractionReady(InteractionArgs args)
        {
            DisplayDebuggerState(args);
            requestSurplus--;
            if (requestSurplus < 0)
            {
                OnNeedDebuggerCommand();
            }
        }

        // similar to OnNeedStdin
        public abstract void OnNeedDebuggerCommand();

        public abstract void DBQuit();

        public abstract void WriteStdout(string output);

        public void OnResult(RPCMessage message, DebuggerError error)
        {
            InvokeCallback(message.id, message, error);
        }

        public void SetBreakpoint(string file, int line, Action<RPCMessage, DebuggerError> callback)
        {
            SendRequest(
                "do_set_breakpoint",
                new object[]
                {
                    file,
                    line
                },
                callback
            );
        }

        public void ClearBreakpoint(string file, int line, Action<RPCMessage, DebuggerError> callback)
        {
            SendRequest(
                "do_clear_breakpoint",
                new object[]
                {
                    file,
                    line
                },
                callback
            );
        }

        public void ClearFileBreakpoints(string file, Action<RPCMessage, DebuggerError> callback)
        {
            SendRequest(
                "do_clear_file_breakpoints",
                new object[]
                {
                    file
                },
                callback
            );
        }

        public void GetBreakpoints(Action<RPCMessage, DebuggerError> callback)
        {
            SendRequest(
                "do_list_breakpoint",
                callback
            );
        }

        public void EnterStdin(string line)
        {
            stdinSurplus++;
            stdinQueue.Enqueue(line);
        }

        public void ContinueExecution()
        {
            SendRequest(
                "do_continue",
                null
            );
        }

        public void Step()
        {
            SendRequest(
                "do_step",
                null
            );
        }

        public void Next()
        {
            SendRequest(
                "do_next",
                null
            );
        }

        public void Return()
        {
            SendRequest(
                "do_return",
                null
            );
        }

        public void Jump(int line_number)
        {
            SendRequest(
                "do_jump",
                new object[]{line_number},
                null
            );
        }

        public void Quit()
        {
            SendRequest(
                "do_quit",
                null
            );
        }

        public void Where(Action<DBStackTrace, DebuggerError> callback)
        {
            SendRequest(
                "do_where",
                // lambda as a hacky way to get a more specific data type from json
                (RPCMessage message, DebuggerError error) =>
                {
                    DBStackTrace stackTrace = null;
                    if (error == null) stackTrace = MyConvert.fromjson<DBStackTrace>(message.result);
                    callback.Invoke(stackTrace, error);
                }    
            );
        }

        public void GetEnvironment(Action<DBEnvironment, DebuggerError> callback)
        {
            SendRequest(
                "do_environment",
                (RPCMessage message, DebuggerError error) =>
                {
                    DBEnvironment environment = null;
                    if (error == null) environment = MyConvert.fromjson<DBEnvironment>(message.result);
                    callback.Invoke(environment, error);
                }    
            );
        }

        public void Evaluate(string expression, Action<DBEvalResult, DebuggerError> callback)
        {
            SendRequest(
                "do_eval",
                new object[]{expression},
                (RPCMessage message, DebuggerError error) =>
                {
                    DBEvalResult result = null;
                    if (error == null) result = MyConvert.fromjson<DBEvalResult>(message.result);
                    callback.Invoke(result, error);
                }    
            );
        }

        public void Execute(string statement, Action<DBExecResult, DebuggerError> callback)
        {
            SendRequest(
                "do_exec",
                new object[]{statement},
                (RPCMessage message, DebuggerError error) =>
                {
                    DBExecResult result = null;
                    if (error == null) result = MyConvert.fromjson<DBExecResult>(message.result);
                    callback.Invoke(result, error);
                }    
            );
        }
        
        #endregion
    }
}
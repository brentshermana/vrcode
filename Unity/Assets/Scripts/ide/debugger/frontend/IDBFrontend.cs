using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using vrcode.datastructures.concurrent;
using vrcode.networking.message;

namespace vrcode.ide.debugger.frontend
{
    // WE DON'T NEED IDBFRONTEND
    // because DBFrontend itself is also a supertype of anything that we'll be working with
    public interface IDBFrontend {
        void ProgramErrorNotif(ProgramError err);
        // void DebuggerErrorNotif(DebuggerError err);
        void ReadReady(); // prompts user to input a string (readline)
        void InteractionReady(InteractionArgs args); // signals readiness for inputs
        void DBQuit(); // backend signals that it's done
        void WriteStdout(string output); // for printing program output

        RPCMessage PeekRequest();
        RPCMessage DequeueRequest(); // NewServer should be able to read requests using this,
                                   // so the DBFrontend has no need to perform operations on
                                   // the NewServer

        string PeekStdin();
        string DequeueStdin();
        

        // overloaded function for providing returned values for requests
        // sent from frontend
        void OnResult(RPCMessage message, DebuggerError error);

        void SetBreakpoint(string file, int line, Action<RPCMessage, DebuggerError> callback);
        void ClearBreakpoint(string file, int line, Action<RPCMessage, DebuggerError> callback);
        void ClearFileBreakpoints(string file, Action<RPCMessage, DebuggerError> callback);
        void GetBreakpoints(Action<RPCMessage, DebuggerError> callback);

        void EnterStdin(string line);

        void ContinueExecution();
        void Step(); // TODO: is this step in or step over? change name to reflect that
        void Next(); // TODO: is this step in or step over? change name to reflect that
        void Return(); // continue until current debugged function ends
        void Jump(int line_number);

        void Quit(); // TODO: does this end the debugging session, or totally close connection with the backend?
        
        void Where(Action<DBStackTrace, DebuggerError> callback);
        void GetEnvironment(Action<DBEnvironment, DebuggerError> callback);
        void Evaluate(string expression, Action<DBEvalResult, DebuggerError> callback);
        void Execute(string statement, Action<DBExecResult, DebuggerError> callback);



// obsolete messages that we used to use

//        void Result(DBEnvironment env);
//        void Result(DBStackTrace trace);
//        void Result(DBEvalResult eval_result);
//        void Result(List<DBBreakpoint> breakpoints);
//        void Result(DBExecResult exec_result);
    }
}
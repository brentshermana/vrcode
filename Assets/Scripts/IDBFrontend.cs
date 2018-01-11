using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using DBNotif;

public interface IDBFrontend {
    void ProgramErrorNotif(ProgramError err);
    void DebuggerErrorNotif(DebuggerError err);
    void ReadReady(ConcurrentQueue<string> stdin); // prompts user to input a string (readline)
    void InteractionReady(InteractionArgs args, ConcurrentQueue<RPCMessage> commandFeed); // signals readiness for inputs
    void DBQuit(); // backend signals that it's done
    void Stdout(string output); // for printing program output

    // overloaded function for providing returned values for requests
    // sent from frontend
    void Result(DBEnvironment env);
    void Result(DBStackTrace trace);
    void Result(DBEvalResult eval_result);
    void Result(List<DBBreakpoint> breakpoints);
    void Result(DBExecResult exec_result);
}

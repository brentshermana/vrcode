using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using DBNotif;

public interface IDBFrontend {
    void ProgramErrorNotif(ProgramError err);
    void DebuggerErrorNotif(DebuggerError err);
    void ReadReady(ConcurrentQueue<string> stdin); // prompts user to input a string (readline)
    void InteractionReady(InteractionArgs args, ConcurrentQueue<RPCObject> commandFeed); // signals readiness for inputs
    void DBQuit(); // backend signals that it's done
}

using System;
using UnityEngine;
using vrcode.networking.message;
using TMPro;

namespace vrcode.ide.debugger.frontend
{
    public class CurvedUiDbFrontend : DBFrontend
    {
        [SerializeField] private string debugged_script; // TODO: REMOVE eventually...
        
        [SerializeField] private TMP_InputField log;
        [SerializeField] private TMP_InputField stdout;
        private int nlines = 6;


        void Start()
        {
            base.Start();
            
            StartDebugging(debugged_script);
            //ContinueExecution();
        }

        void Update()
        {
            base.Update();
        }
        
        

        private void truncate(TMP_InputField field)
        {
            string[] lines = field.text.Split(new char[] {'\n'});
            if (lines.Length > nlines)
            {
                // take only the last nlines of text
                string[] new_lines = new string[nlines];
                for (int i = 0; i < nlines; i++)
                {
                    new_lines[i] = lines[i + lines.Length - new_lines.Length];
                }

                field.text = String.Join("\n", new_lines);
            }
        }
        
        public override void ProgramErrorNotif(ProgramError err)
        {
            Debug.Log("DBFrontend: Program Error Notification");
            log.text += "Error: " + err.message + "\n";
            truncate(log);
        }

        public override void OnNeedStdin()
        {
            Debug.Log("DBFrontend: Need Stdin Notification");
            log.text += "Program waiting for input\n";
            truncate(log);
        }

        public override void DisplayDebuggerState(InteractionArgs args)
        {
            Debug.Log("DBFrontend: Updating Debugger Status");
            log.text += "Status: line " + args.lineno + "\n";
            truncate(log);
        }

        public override void OnNeedDebuggerCommand()
        {
            Debug.Log("DBFrontend: Waiting for a command");
            log.text += "Paused: waiting for a command\n";
            truncate(log);
        }

        public override void DBQuit()
        {
            Debug.Log("DBFrontend: Debugger Session Quit");
            log.text += "Debugger Session Quit\n";
            truncate(log);
        }

        public override void WriteStdout(string output)
        {
            Debug.Log("DBFrontend: Writing Stdout");
            stdout.text += output;
            stdout.text += "/n"; // TODO: does the stdout already contain a newline character?
            truncate(stdout);
        }
    }
}
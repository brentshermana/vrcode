using System;
using UnityEngine;
using vrcode.networking.message;
using TMPro;
using Valve.VR.InteractionSystem;

namespace vrcode.ide.debugger.frontend
{
    public class CurvedUiDbFrontend : DBFrontend
    {
        [SerializeField] private GameObject ArrowPrefab;
        private GameObject lastArrow;
        
        [SerializeField] private EnvironmentDisplayer envDisplay;
        
        //[SerializeField] private string debugged_script; // TODO: REMOVE eventually...
        
        [SerializeField] private TMP_InputField log;
        [SerializeField] private TMP_InputField stdout;
        private int nlines = 6;

        private bool gotEnvLast = false;

        void Start()
        {
            base.Start();
            //StartDebugging(TheEnvironment.GetSourceFilePath());
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
            
            //if (args.filename == debugged_script)
                PlaceArrow(int.Parse(args.lineno));
            //else ClearArrow();
        }

        public override void OnNeedDebuggerCommand()
        {
            // we need to ask for the environment every other command, because the only
            // commands exposed to the user are 'move' commands, which can possibly change the
            // state of one or more variables
            if (gotEnvLast)
            {
                gotEnvLast = false;
                
                Debug.Log("DBFrontend: Waiting for a command");
                log.text += "Paused: waiting for a command\n";
                truncate(log);
            }
            else
            {
                gotEnvLast = true;
                
                GetEnvironment((DBEnvironment environment, DebuggerError error) =>
                {
                    if (error != null)
                    {
                        Debug.LogError(error);
                    }
                    else
                    {
                        envDisplay.DisplayEnvironment(environment);
                    }
                });
            }
        }

        public override void DBQuit()
        {
            gotEnvLast = false;
            Debug.Log("DBFrontend: Debugger Session Quit");
            log.text += "Debugger Session Quit\n";
            truncate(log);

            ClearArrow();
        }

        private void ClearArrow()
        {
            if (lastArrow != null)
            {
                Destroy(lastArrow);
                lastArrow = null;
            }
        }

        private void PlaceArrow(int line)
        {
            ClearArrow();

            lastArrow = Instantiate(ArrowPrefab);
            lastArrow.GetComponent<LinePositioner>().GoToLine(line);
        }

        public override void WriteStdout(string output)
        {
            Debug.Log("DBFrontend: Writing Stdout");
            stdout.text += output;
            //stdout.text += "\n"; // TODO: does the stdout already contain a newline character?
            truncate(stdout);
        }
    }
}
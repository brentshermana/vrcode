using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using System.Text;

using System.Diagnostics;

namespace vrcode
{
    public class LaunchPython : MonoBehaviour {
        public string PythonPath;
        public string FileToRun;

        private Process process;

        // Use this for initialization
        void Start () {
            process = new Process();
            process.StartInfo.FileName = PythonPath;
            process.StartInfo.UseShellExecute = false; //necessary for getting stdout,err
            process.StartInfo.RedirectStandardError = true;
            process.StartInfo.RedirectStandardOutput = true;
            process.StartInfo.Arguments = FileToRun;
            process.Start();
        }
	
        // Update is called once per frame
        void Update () {
            if (process.HasExited)
            {
                Stdio(process);
                this.enabled = false;
            }
        }
        void Stdio(Process p)
        {
            PrintStream(p.StandardOutput, "Python Stdout");
            PrintStream(p.StandardError, "Python Stderr");
        }
        void PrintStream(System.IO.StreamReader reader, string streamName)
        {
            StringBuilder sb = new StringBuilder("Stream " + streamName + ":\n");
            while (!reader.EndOfStream)
            {
                sb.Append((char)reader.Read());
            }
            UnityEngine.Debug.Log(sb.ToString());
        }
    }
}
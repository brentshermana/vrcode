using System;
using System.IO;
using System.Reflection;
using System.CodeDom.Compiler;
using Microsoft.CSharp;
using UnityEngine;
using UnityEngine.UI;

public class CodeTextEditor : MonoBehaviour
{

    [SerializeField]
    private string DefaultText;

    [SerializeField]
    private InputField OutputField;

    private InputField _inputField;
    private Color _defaultTextColour;

    public InputField ExecInput;
    public InputField BreakInput;

    private RealNetMqServer server;

    public void Start()
    {
        _inputField = gameObject.GetComponent<InputField>();
        _inputField.text = "";
        _defaultTextColour = OutputField.textComponent.color;

        server = GameObject.Find("NetMQ").GetComponent<RealNetMqServer>();
    }

    public void SetBreakpoint()
    {
        string line = BreakInput.text;
        Debug.Log("Breakpoint at " + line);
        ActionableJsonMessage bpmsg = new ActionableJsonMessage(
            "Runtime",
            "BpSet",
            new string[] { line }
        );
        server.SendToBackend(bpmsg);
    }

    public void Continue()
    {
        Debug.Log("Continue");
        ActionableJsonMessage continuemsg = new ActionableJsonMessage(
            "Runtime",
            "Continue",
            new string[] { }
        );
        server.SendToBackend(continuemsg);
    }

    public void Exec ()
    {
        string s = ExecInput.text;
        Debug.Log("Executing String " + s);
        ActionableJsonMessage execMessage = new ActionableJsonMessage(
            "Runtime",
            "Exec",
            new string[] { s }
        );
        server.SendToBackend(execMessage);
    }

    public void Next()
    {
        Debug.Log("Next Line");
        ActionableJsonMessage nextmsg = new ActionableJsonMessage(
            "Runtime",
            "Next",
            new string[] { }
        );
        server.SendToBackend(nextmsg);
    }

    public void Quit()
    {
        Debug.Log("Quitting");
        ActionableJsonMessage quitmsg = new ActionableJsonMessage(
            "Runtime",
            "Quit",
            new string[] {  }
        );
        server.SendToBackend(quitmsg);
    }

    public void RunCode()
    {
        Debug.Log("Sending Code to Backend...");
        string code = _inputField.text;
        string defaultfilename = "deleteme.py";
        ActionableJsonMessage codeMessage = new ActionableJsonMessage(
            "Editor",
            "File",
            new string[] { defaultfilename, code }
        );
        ActionableJsonMessage runMessage = new ActionableJsonMessage(
            "Editor",
            "Run",
            new string[] { defaultfilename }
        );
        ActionableJsonMessage[] messages = new ActionableJsonMessage[] { codeMessage, runMessage };

        
        foreach (ActionableJsonMessage msg in messages)
        {
            server.SendToBackend(msg);
        }
        Debug.Log("...Complete");
    }

    public void Update()
    {
        ActionableJsonMessage msg = server.AttemptDequeue();
        while (msg != null)
        {
            Debug.Log("Editor processing message " + msg.ToString());
            processMessage(msg);
            msg = server.AttemptDequeue();
        }
    }

    private void processMessage(ActionableJsonMessage message)
    {
        switch (message.Type)
        {
            case "Runtime":
                processRuntimeMessage(message);
                break;
            case "Output":
                processOutputMessage(message);
                break;
        }
    }

    private void console_log(string str)
    {
        Debug.Log("console_logging : " + str);
        OutputField.text = OutputField.text + (Environment.NewLine + str);
    }

    private void processOutputMessage(ActionableJsonMessage msg)
    {
        if (msg.SubType == "Stdout")
        {
            console_log("Stdout: " + msg.Args[0]);
        }
        else if (msg.SubType == "Stderr")
        {
            console_log("Stderr: " + msg.Args[0]);
        }
    }

    private void processRuntimeMessage(ActionableJsonMessage message)
    {
        string subtype = message.SubType;
        string[] args = message.Args;
        
        switch (subtype)
        {
            case "Exit":
                console_log("Program Exited with status " + args[0]);
                break;
            case "Error":
                console_log("Program Error: " + args[0]);
                break;
            case "Pause":
                console_log("Program waiting on line " + args[0]);
                break;
            
        }
    }

}

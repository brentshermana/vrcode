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

    public void Start()
    {
        _inputField = gameObject.GetComponent<InputField>();
        _inputField.text = DefaultText.Replace("\\n", "\n").Replace("\\t", "\t");
        _defaultTextColour = OutputField.textComponent.color;
    }

    public void SendToBackend()
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

        RealNetMqServer server = GameObject.Find("NetMQ").GetComponent<RealNetMqServer>();
        foreach (ActionableJsonMessage msg in messages)
        {
            server.SendToBackend(msg);
        }
        Debug.Log("...Complete");
    }

    public void Compile()
    {
        OutputField.textComponent.color = _defaultTextColour;
        CompilerResults compile = CompileHelper.CompileCodeFromString(_inputField.text);

        if (compile.Errors.HasErrors)
        {
            string text = "Compile error: ";
            foreach (CompilerError error in compile.Errors)
            {
                text += '\n' + error.ToString();
            }

            var defaultColour = OutputField.textComponent.color;
            OutputField.textComponent.color = Color.red;
            OutputField.text = text;
            return;
        }

        Module module = compile.CompiledAssembly.GetModules()[0];
        Type typeInfo = null;
        MethodInfo methodInfo = null;

        if (module != null)
        {
            typeInfo = module.GetType("TestClass");
        }

        if (typeInfo != null)
        {
            methodInfo = typeInfo.GetMethod("EchoStuff");
        }

        if (methodInfo != null)
        {
            OutputField.text = (string)methodInfo.Invoke(null, new object[] { "Here's some stuff!" });
        }

    }
}

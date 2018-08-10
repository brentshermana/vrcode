using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using TMPro;

// only for use with a single-file demo...
public class TheEnvironment : MonoBehaviour
{
	private static TheEnvironment singleton;
	private static string the_python_source_file;
	private static string the_python_path;
	private static string the_debugger_backend_path;
	private static TMP_InputField the_field;

	[SerializeField] private string python_source_file;
	[SerializeField] private string python_path;
	[SerializeField] private string debugger_backend_path;
	[SerializeField] private TMP_InputField text_editor;
	

	// Use this for initialization
	void Start () {
		if (singleton == null)
		{
			singleton = this;
			the_python_source_file = python_source_file;
			the_field = text_editor;
			the_python_path = python_path;
			the_debugger_backend_path = debugger_backend_path;
		}
		else
		{
			Debug.LogError("Singleton instance already exists! This is upsetting!");
		}
	}

	public static string GetSourceFilePath()
	{
		EnsureInstance();
		return the_python_source_file;
	}

	public static string GetPythonPath()
	{
		EnsureInstance();
		return the_python_path;
	}

	public static string GetDebuggerScriptPath()
	{
		EnsureInstance();
		return the_debugger_backend_path;
	}

	public static void Rewrite(string new_content)
	{
		EnsureInstance();
		System.IO.File.WriteAllText(the_python_source_file, new_content);
	}

	public static void Rewrite()
	{
		Rewrite(the_field.text);
	}

	private static void EnsureInstance()
	{
		if (singleton == null)
		{
			throw new Exception("TheSourceFile Singleton used, but is not set!");
		}
	}
}

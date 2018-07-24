using System.Collections;
using System.Collections.Generic;
using System.Text;
using TMPro;
using UnityEngine;
using vrcode.networking;
using vrcode.networking.message;
using vrcode.networking.netmq;

/*
 * do NOT use, only here for reference.
 * because TMP returns markup as a part of the text object,
 * we can't easily send the 'pure' source to the backend easily.
 *
 * Adding rtf tags into the source would similarly complicate all
 * other components (autocomplete) which require reading from the source
 */
public class SyntaxHighlighterManager : MonoBehaviour
{

	private NetMqRpcIOAdapter io;
	[SerializeField] private string port;
	
	[SerializeField] private TMP_InputField editor;
	private int last_caret_pos = -10;

	private int message_id = 1;

	// Use this for initialization
	void Start () {
		io = new NetMqRpcIOAdapter(port);
		io.Start();
	}

	private void OnDisable()
	{
		io.Stop();
	}

	// Update is called once per frame
	void Update () {
		io.Update();
		
		if (last_caret_pos != editor.caretPosition)
		{
			last_caret_pos = editor.caretPosition;

			
			
			// construct request:
			RPCMessage message = RPCMessage.Request(
				"do_highlight",
				new List<object>(new object[] {editor.text}),
				message_id++
			);
			Debug.Log(editor.textComponent.GetParsedText());
			Debug.Log("Syntax Highlighter Sending Request");
			Debug.Log(message.ToString());
			io.SendMessage(message);
		}
		
		// recv responses
		string result = null;
		while (io.incoming_queue.Count > 0)
		{
			Debug.Log("Syntax Highlighter Reading Reply");
			Debug.Log(io.incoming_queue.Peek());

			RPCMessage response = io.incoming_queue.Dequeue();
			if (response.error != null)
				Debug.LogError(response.error);
			else
				// we only want the latest result
				result = MyConvert.fromjson<string>(response.result);
		}

		if (result != null)
		{
			editor.text = result;
			editor.caretPosition = last_caret_pos; // just in case
		}
	}
}

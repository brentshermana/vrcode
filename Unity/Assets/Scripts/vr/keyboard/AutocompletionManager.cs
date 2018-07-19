using System.Collections.Generic;
using TMPro;
using UnityEngine;
using vrcode.networking;
using vrcode.networking.message;
using vrcode.networking.netmq;

namespace vrcode.vr.keyboard
{
    [RequireComponent(typeof(ButtonArranger))]
    public class AutocompletionManager : MonoBehaviour
    {
        [SerializeField] private Transform buttonPrefab;
        [SerializeField] private float charSize;

        private NetMqRpcIOAdapter io;
        [SerializeField] private string port;
        
        [SerializeField] private TMP_InputField editor;
        private int last_caret_pos = -10;

        private int message_id = 1;

        private ButtonArranger buttons;

        void Start()
        {
            io = new NetMqRpcIOAdapter(port);
            io.Start();

            buttons = GetComponent<ButtonArranger>();
        }

        void Update()
        {
            io.Update();
            
            // send requests
            if (last_caret_pos != editor.caretPosition && editor.caretPosition >= 0)
            {
                last_caret_pos = editor.caretPosition;
                
                // calculate the line, col positions
                int line = 1;
                int col = 0;
                for (int i = 0; i < last_caret_pos; i++)
                {
                    if (editor.text[i] == '\n')
                    {
                        col = 0;
                        line++;
                    }
                    else
                    {
                        col++;
                    }
                }
                
                // construct the message
                RPCMessage message = RPCMessage.Request(
                    "do_set_source",
                    new List<object>(new object[] {editor.text, line, col}),
                    message_id++
                );

                Debug.Log("AutocompletionManager Sending Request");
                io.SendMessage(message);
            }
            
            
            // read replies
            List<AutocompletionOption> completions = null;
            while (io.incoming_queue.Count > 0)
            {
                Debug.Log("AutocompletionManager Reading Reply");
                Debug.Log(io.incoming_queue.Peek());

                RPCMessage response = io.incoming_queue.Dequeue();
                if (response.method == "do_set_source")
                {
                    if (response.error != null)
                    {
                        Debug.LogError(response.error.message);
                    }
                    else
                    {
                        // we only care about the most recent reply, so overwriting behavior is intentional
                        completions = MyConvert.fromjson<List<AutocompletionOption>>(response.result);   
                    }
                }
            }

            if (completions != null)
            {
                AddButtons(completions);
            }
        }

        void OnDisable()
        {
            Debug.Log("ON DISABLE");
            io.Stop();
        }

        
        

        void AddButtons(List<AutocompletionOption> options)
        {
            buttons.Clear(); // remove current buttons, if any
            
            foreach (AutocompletionOption option in options)
            {
                Debug.Log("Autocompletion Option: " + option.name);
                
                GameObject button = Instantiate(buttonPrefab).gameObject;

                KeyUtils key = button.GetComponent<KeyUtils>();
                key.SetSpecialKeyInfo(option.name, option.complete);
                key.SetSize(charSize * option.name.Length, charSize, charSize);
                key.SetTravel(.1f * charSize);
			
                buttons.AddButton(button);
            }
        }
    }
}
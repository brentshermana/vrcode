using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

using DBNotif;

public class GuiDBFrontend : MonoBehaviour, IDBFrontend {

    public Text DisplayText;
    public Transform buttonsParent;

    public Text InputText; // used for adding arguments to buttons

    private ConcurrentQueue<RPCObject> commandFeed;
    private ConcurrentQueue<string> stdin;

    private Dictionary<string, Button> buttons = new Dictionary<string, Button>();


	// Use this for initialization
	void Start () {
        // initialize buttons using parent:
        foreach (Transform child in buttonsParent) {
            Button button = child.gameObject.GetComponent<Button>();
            if (button != null) buttons.Add(button.gameObject.name, button);
            // Debug.Log("Button Name " + button.gameObject.name);
        }

        // add events
        buttons["Continue"].onClick.AddListener(Continue);
        buttons["ReadLine"].onClick.AddListener(ReadLine);
        buttons["Step"].onClick.AddListener(Step);
        buttons["Next"].onClick.AddListener(Next);
        buttons["Return"].onClick.AddListener(Return);
        buttons["Where"].onClick.AddListener(Where);
        buttons["Quit"].onClick.AddListener(Quit);
        buttons["Environment"].onClick.AddListener(Environment);

        // disable buttons until interaction
        SetActive(false);
	}
	// Update is called once per frame
	void Update () {
		
	}



    // Button Events:
    private void Continue() {
        display("Continue Registered");
        this.commandFeed.Enqueue(RPCObject.Request("do_continue", null, 0)); // id will be reset
        this.commandFeed = null;
        SetActive(false);
    }
    private void ReadLine() {
        string line = InputText.text;
        InputText.text = "";
        display(("Line Read: " + line));
        stdin.Enqueue(line);
        stdin = null;
        SetActive(false);
    }
    private void Step() {
        display("Step Registered");
        this.commandFeed.Enqueue(RPCObject.Request("do_step", null, 0)); // id will be reset
        this.commandFeed = null;
        SetActive(false);
    }
    private void Next()
    {
        display("Next Registered");
        this.commandFeed.Enqueue(RPCObject.Request("do_next", null, 0)); // id will be reset
        this.commandFeed = null;
        SetActive(false);
    }
    private void Return()
    {
        display("Return Registered");
        this.commandFeed.Enqueue(RPCObject.Request("do_return", null, 0)); // id will be reset
        this.commandFeed = null;
        SetActive(false);
    }
    private void Where()
    {
        display("Where Registered");
        this.commandFeed.Enqueue(RPCObject.Request("do_where", null, 0)); // id will be reset
        this.commandFeed = null;
        SetActive(false);
    }
    private void Quit()
    {
        display("Debugging Session Ended");
        this.commandFeed.Enqueue(RPCObject.Request("do_quit", null, 0)); // id will be reset
        this.commandFeed = null;
        SetActive(false);
    }
    private void Environment()
    {
        display("Environment Registered");
        this.commandFeed.Enqueue(RPCObject.Request("do_environment", null, 0)); // id will be reset
        this.commandFeed = null;
        SetActive(false);
    }


    // Interface functions
    public void ProgramErrorNotif(ProgramError err) {
        display(err.ToString());
    }
    public void DebuggerErrorNotif(DebuggerError err) {
        display(err.ToString());
    }
    public void ReadReady(ConcurrentQueue<string> stdin_) {
        this.stdin = stdin_;
        display("Read Line"); 
    } 
    //TODO: add file, line number as arguments
    public void InteractionReady(InteractionArgs args, ConcurrentQueue<RPCObject> commandFeed_) {
        display("Interaction");
        this.commandFeed = commandFeed_;
        SetActive(true);
    }
    public void DBQuit() {
        display("Debugger Quit");
        SetActive(false);
    }



    //helper functions
    private void display(string s) {
        Debug.Log("Frontend: " + s);
        DisplayText.text = s;
    }
    private void SetActive(bool active) {
        foreach (Button b in buttons.Values) {
            b.interactable = active;
        }
    }
}

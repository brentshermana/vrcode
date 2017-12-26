using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

using DBNotif;

public class GuiDBFrontend : MonoBehaviour, IDBFrontend {

    public Text PromptText;
    public Text CurrentCodeLineText;

    public Text StdinText;
    public Text StdoutText;

    public Transform buttonsParent;



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
        DisplayPrompt("Continue Registered");
        this.commandFeed.Enqueue(RPCObject.Request("do_continue", null, 0)); // id will be reset
        this.commandFeed = null;
        SetActive(false);
    }
    private void ReadLine() {
        UnityEngine.Debug.Log("Std is null " + (stdin == null));
        string line = StdinText.text;
        StdinText.text = "";
        DisplayPrompt(("Line Read: " + line));
        stdin.Enqueue(line);
        stdin = null;
        SetActive(false);
    }
    private void Step() {
        DisplayPrompt("Step Registered");
        this.commandFeed.Enqueue(RPCObject.Request("do_step", null, 0)); // id will be reset
        this.commandFeed = null;
        SetActive(false);
    }
    private void Next()
    {
        DisplayPrompt("Next Registered");
        this.commandFeed.Enqueue(RPCObject.Request("do_next", new List<string>(), 0)); // id will be reset
        this.commandFeed = null;
        SetActive(false);
    }
    private void Return()
    {
        DisplayPrompt("Return Registered");
        this.commandFeed.Enqueue(RPCObject.Request("do_return", null, 0)); // id will be reset
        this.commandFeed = null;
        SetActive(false);
    }
    private void Where()
    {
        DisplayPrompt("Where Registered");
        this.commandFeed.Enqueue(RPCObject.Request("do_where", null, 0)); // id will be reset
        this.commandFeed = null;
        SetActive(false);
    }
    private void Quit()
    {
        DisplayPrompt("Debugging Session Ended");
        this.commandFeed.Enqueue(RPCObject.Request("do_quit", null, 0)); // id will be reset
        this.commandFeed = null;
        SetActive(false);
    }
    private void Environment()
    {
        DisplayPrompt("Environment Registered");
        this.commandFeed.Enqueue(RPCObject.Request("do_environment", null, 0)); // id will be reset
        this.commandFeed = null;
        SetActive(false);
    }


    // Interface functions, called by debugger interface
    public void ProgramErrorNotif(ProgramError err) {
        DisplayPrompt(err.ToString());
    }
    public void DebuggerErrorNotif(DebuggerError err) {
        DisplayPrompt(err.ToString());
    }
    public void ReadReady(ConcurrentQueue<string> stdin_) {
        //just want the one button to be active
        SetActive(false);
        buttons["ReadLine"].interactable = true;
        this.stdin = stdin_;
        UnityEngine.Debug.Log("Std is null " + (stdin == null));
        DisplayPrompt("Read Line"); 
    } 
    //TODO: add file, line number as arguments
    public void InteractionReady(InteractionArgs args, ConcurrentQueue<RPCObject> commandFeed_) {
        DisplayCodeLine(args);
        DisplayPrompt("Interaction");
        this.commandFeed = commandFeed_;
        SetActive(true);
    }
    public void DBQuit() {
        DisplayPrompt("Debugger Quit");
        SetActive(false);
    }
    public void Stdout(string output) {
        UnityEngine.Debug.Log("Stdout " + output);
        StdoutText.text += output;
    }



    //helper functions
    private void DisplayPrompt(string s) {
        Debug.Log("Frontend: " + s);
        PromptText.text = s;
    }
    private void DisplayCodeLine(InteractionArgs args) {
        CurrentCodeLineText.text = args.ToString();
    }
    private void SetActive(bool active) {
        foreach (Button b in buttons.Values) {
            b.interactable = active;
        }
    }
}

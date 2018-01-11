using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

using DBNotif;

public class GuiDBFrontend : MonoBehaviour, IDBFrontend {

    public Text PromptText;
    public Text CurrentCodeLineText;

    public Text[] Args;
    public Text StdinText;
    public Text StdoutText;

    public Transform buttonsParent;



    private ConcurrentQueue<RPCMessage> commandFeed;
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
        buttons["Jump"].onClick.AddListener(Jump);
        buttons["Eval"].onClick.AddListener(Eval);
        buttons["Set BP"].onClick.AddListener(SetBP);
        buttons["Clear BP"].onClick.AddListener(ClearBP);
        buttons["Clear File BPs"].onClick.AddListener(ClearFileBPs);
        buttons["List BPs"].onClick.AddListener(ListBPs);
        buttons["Exec"].onClick.AddListener(Exec);

        // disable buttons until interaction
        SetActive(false);
	}
	// Update is called once per frame
	void Update () {
		
	}



    // Button Events:
    private void Continue() {
        DisplayPrompt("Continue Registered");
        this.commandFeed.Enqueue(RPCMessage.Request("do_continue", new List<string>(), 0)); // id will be reset
        this.commandFeed = null;
        SetActive(false);
    }
    private void ReadLine() {
        //UnityEngine.Debug.Log("Std is null " + (stdin == null));
        string line = StdinText.text;
        StdinText.text = "";
        DisplayPrompt(("Line Read: " + line));
        stdin.Enqueue(line);
        stdin = null;
        SetActive(false);
    }
    private void Step() {
        DisplayPrompt("Step Registered");
        this.commandFeed.Enqueue(RPCMessage.Request("do_step", new List<string>(), 0)); // id will be reset
        this.commandFeed = null;
        SetActive(false);
    }
    private void Next()
    {
        DisplayPrompt("Next Registered");
        this.commandFeed.Enqueue(RPCMessage.Request("do_next", new List<string>(), 0)); // id will be reset
        this.commandFeed = null;
        SetActive(false);
    }
    private void Return()
    {
        DisplayPrompt("Return Registered");
        this.commandFeed.Enqueue(RPCMessage.Request("do_return", new List<string>(), 0)); // id will be reset
        this.commandFeed = null;
        SetActive(false);
    }
    private void Where()
    {
        DisplayPrompt("Where Registered");
        this.commandFeed.Enqueue(RPCMessage.Request("do_where", new List<string>(), 0)); // id will be reset
        this.commandFeed = null;
        SetActive(false);
    }
    private void Quit()
    {
        DisplayPrompt("Debugging Session Ended");
        this.commandFeed.Enqueue(RPCMessage.Request("do_quit", new List<string>(), 0)); // id will be reset
        this.commandFeed = null;
        SetActive(false);
    }
    private void Environment()
    {
        DisplayPrompt("Environment Registered");
        this.commandFeed.Enqueue(RPCMessage.Request("do_environment", new List<string>(), 0)); // id will be reset
        this.commandFeed = null;
        SetActive(false);
    }
    private void Jump() {
        string line = GetArg(0);
        List<string> args = new List<string>();
        args.Add(line);
        this.commandFeed.Enqueue(RPCMessage.Request("do_jump", args, 0)); // id will be reset
        this.commandFeed = null;
        SetActive(false);
    }
    private void Eval() {
        string expression = GetArg(0);
        List<string> args = new List<string>();
        args.Add(expression);
        this.commandFeed.Enqueue(RPCMessage.Request("do_eval", args, 0)); // id will be reset
        this.commandFeed = null;
        SetActive(false);
    }
    private void SetBP() {
        string fname = "/Users/brentshermana/dev/repos/vrcode_backend/test.py";//GetArg(0);
        string lineno = GetArg(1);
        Debug.Log("Set BP : " + fname + " : " + lineno);
        // the backend only wants one arg, which is the filename and lineno concatenated with a colon
        List<string> args = new List<string>(new string[] { fname, lineno });
        this.commandFeed.Enqueue(RPCMessage.Request("do_set_breakpoint", args, 0)); // id will be reset
        this.commandFeed = null;
        SetActive(false);
    }
    private void ClearBP() {
        string fname = GetArg(0);
        string lineno = GetArg(1);
        List<string> args = new List<string>(new string[] { fname, lineno });
        this.commandFeed.Enqueue(RPCMessage.Request("do_clear_breakpoint", args, 0)); // id will be reset
        this.commandFeed = null;
        SetActive(false);
    }
    private void ClearFileBPs() {
        string fname = GetArg(0);
        List<string> args = new List<string>(new string[] { fname });
        this.commandFeed.Enqueue(RPCMessage.Request("do_clear_file_breakpoints", args, 0)); // id will be reset
        this.commandFeed = null;
        SetActive(false);
    }
    private void ListBPs() {
        List<string> args = new List<string>();
        this.commandFeed.Enqueue(RPCMessage.Request("do_list_breakpoint", args, 0)); // id will be reset
        this.commandFeed = null;
        SetActive(false);
    }
    private void Exec() {
        List<string> args = new List<string>(new string[]{GetArg(0)});
        this.commandFeed.Enqueue(RPCMessage.Request("do_exec", args, 0)); // id will be reset
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
        //UnityEngine.Debug.Log("Std is null " + (stdin == null));
        DisplayPrompt("Read Line"); 
    } 
    //TODO: add file, line number as arguments
    public void InteractionReady(InteractionArgs args, ConcurrentQueue<RPCMessage> commandFeed_) {
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
        //UnityEngine.Debug.Log("Stdout " + output);
        StdoutText.text += output;
    }



    // TODO: implement with better GUI functions... if you really want to
    public void Result(DBEnvironment env) {
        Debug.Log(env);
    }
    public void Result(DBStackTrace trace) {
        Debug.Log(trace);
    }
    public void Result(DBEvalResult eval_result) {
        Debug.Log("Eval: " + eval_result.Result);
    }
    public void Result(List<DBBreakpoint> breakpoints) {
        string s = "Breakpoints:\n";
        foreach (DBBreakpoint b in breakpoints) {
            s += b.ToString();
            s += "\n===============================\n";
        }
        Debug.Log(s);
    }
    public void Result(DBExecResult exec_result) {
        Debug.Log("Exec Result: " + exec_result.Result);
    }



    //helper functions
    private string GetArg(int i) {
        string ret = Args[i].text.Trim();
        Args[i].text = "";
        return ret;
    }
    private void DisplayPrompt(string s) {
        //Debug.Log("Frontend: " + s);
        PromptText.text = s;
    }
    private void DisplayCodeLine(InteractionArgs args) {
        CurrentCodeLineText.text = args.ToString();
    }
    private void SetActive(bool active) {
        foreach (Button b in buttons.Values)
            b.interactable = active;
    }
}

namespace vrcode.networking.message
{
    /**
     * An error exhibited by the debugger itself, rather than
     * be the debugged program
     */
    public class DebuggerError {
        public DebuggerError(string code_, string message_) {
            this.code = code;
            this.message = message_;
        }

        public override string ToString() // autogenerated by Visual Studio :D
        {
            return string.Format("[DebuggerError: code={0}, message={1}]", code, message);
        }

        public string code {get;set;}
        public string message {get;set;}
    }
}
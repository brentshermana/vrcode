namespace vrcode.networking.message
{
    /**
     * An error occurring in the debugged program. Does not indicate that the debugger
     * itself experienced any problems
     */
    public class ProgramError {
        public ProgramError(string title_, string type_, string val_, string trace_, string message_) {
            this.title = title_;
            this.type = type_;
            this.val = val_;
            this.trace = trace_;
            this.message = message_;
        }

        public override string ToString()
        {
            return string.Format("[ProgramError: title={0}, type={1}, val={2}, trace={3}, message={4}]", title, type, val, trace, message);
        }

        public string title {get;set;}
        public string type {get;set;}
        public string val {get;set;}
        public string trace {get;set;}
        public string message {get;set;}
    }
}
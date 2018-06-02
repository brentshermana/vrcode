namespace vrcode.networking.message
{
    public class DBExecResult {
        public DBExecResult(string result) {
            Result = result;
        }

        public string Result { get; set; }
    }
}
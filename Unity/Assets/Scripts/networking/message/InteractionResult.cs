namespace vrcode.networking.message
{
    // this is a stupid class. No need to use it in the future.
    // RPCMessage does all the exact same things, and more!
    public class InteractionResult {
        public InteractionResult(string method_, string result_) {
            Result = result_;
            Method = method_;
        }

        public string Result { get; set; }
        public string Method { get; set; }
    }
}
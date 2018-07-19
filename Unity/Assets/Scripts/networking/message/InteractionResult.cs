namespace vrcode.networking.message
{
    // this is a stupid class. No need to use it in the future.
    public class InteractionResult {
        public InteractionResult(string method_, string result_) {
            Result = result_;
            Method = method_;
        }

        public string Result { get; set; }
        public string Method { get; set; }
    }
}
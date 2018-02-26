namespace vrcode.networking.message
{
    public class InteractionResult {
        public InteractionResult(string method_, string result_) {
            Result = result_;
            Method = method_;
        }

        public string Result { get; set; }
        public string Method { get; set; }
    }
}
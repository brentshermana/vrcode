namespace vrcode.networking.message
{
    public class InteractionArgs {
        public InteractionArgs(string filename_, string lineno_, string line_) {
            this.filename = filename_;
            this.lineno = lineno_;
            this.line = line_;
        }

        public override string ToString()
        {
            return string.Format("{0} ({1}) => {2}", filename, lineno, line);
        }

        public string filename { get; set; }
        public string lineno { get; set; }
        public string line { get; set; }
    }
}
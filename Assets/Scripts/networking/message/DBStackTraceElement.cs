namespace vrcode.networking.message
{
    public class DBStackTraceElement {
        public override string ToString()
        {
            return string.Format("[{0}:{1}    {2}]", fname, lineno, line.Trim());
        }

        public string fname { get; set; }
        public string lineno { get; set; }
        public string line { get; set; }
    }
}
namespace vrcode.networking.message
{
    public class DBBreakpoint {

        public override string ToString()
        {
            return string.Format("[DBBreakpoint\nnumber={0}\nfile={1}\nlineno={2}\ntemporary={3}\nenabled={4}\nhits={5}\ncondition={6}]", number, file, lineno, temporary, enabled, hits, condition);
        }

        public int number { get; set; }
        public string file { get; set; }
        public int lineno { get; set; }
        public bool temporary { get; set; }
        public bool enabled { get; set; }
        public int hits { get; set; }
        public string condition { get; set; }
    }
}
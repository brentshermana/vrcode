namespace vrcode.networking.message
{
    public class Variable {

        public override string ToString()
        {
            return string.Format("[name={0}, value={1}, type={2}]", name, val, type);
        }

        public string name { get; set; }
        public string val { get; set; }
        public string type { get; set; }
    }
}
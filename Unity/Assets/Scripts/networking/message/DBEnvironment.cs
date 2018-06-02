using System.Collections.Generic;

namespace vrcode.networking.message
{
    public class DBEnvironment {
        public override string ToString()
        {
            string locals_str = "===LOCALS===";
            foreach (var variable in locals)
                locals_str += "\n" + variable.ToString();
            string globals_str = "\n===GLOBALS===";
            foreach (var global_ in globals)
                globals_str += "\n" + global_.ToString();
            return "Environment:\n" + locals_str + globals_str;
        }

        public List<Variable> locals { get; set; }
        public List<Variable> globals { get; set; }
    }
}
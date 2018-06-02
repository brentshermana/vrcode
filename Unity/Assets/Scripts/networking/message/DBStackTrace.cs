using System.Collections.Generic;

namespace vrcode.networking.message
{
    public class DBStackTrace {
        public override string ToString()
        {
            string str = "Stack Trace:";
            foreach (DBStackTraceElement element in trace) {
                str += "\n";
                str += element.ToString();   
            }
            return str;
        }

        public List<DBStackTraceElement> trace { get; set; }
    }
}
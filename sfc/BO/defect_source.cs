using System;

namespace sfc.BO
{
    public class defect_source
    {
        public defect_source(string code) { id = code; }
        public defect_source(string code, string desc) { id = code; description = desc; }
        public string id { get; set; }
        public string description { get; set; }
    }
}

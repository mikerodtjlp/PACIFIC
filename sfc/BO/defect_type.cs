using System;

namespace sfc.BO
{
    public class defect_type
    {
        public defect_type(string code) { id = code; }
        public string id { get; set; }
        public string description { get; set; }
    }
}

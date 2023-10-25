using System;

namespace sfc.BO
{
    public class defect
    {
        public defect(string code) { id = code; }
        public defect(int code) { id = code.ToString(); }

        public string group { get; set; }
        public string id { get; set; }
        public string description_s { get; set; }
        public string description { get; set; }
        public int type { get; set; }
        public string type_desc { get; set; }
        public string category { get; set; }
    }
}

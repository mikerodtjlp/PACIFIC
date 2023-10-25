using System;

namespace sfc.BO
{
    public class qc_inspection_type
    {
        public qc_inspection_type(string code) { type = code; }
        public string type { get; set; }
        public string description { get; set; }
    }
}

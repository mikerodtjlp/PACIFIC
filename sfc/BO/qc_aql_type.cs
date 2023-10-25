using System;

namespace sfc.BO
{
    public class qc_aql_type
    {
        public qc_aql_type(string code) { type = code; }
        public string type { get; set; }
        public string description { get; set; }
    }
}

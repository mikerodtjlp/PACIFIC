using System;

namespace sfc.BO
{
    public class qc_aql
    {
        public qc_aql_type aql_type { get; set; }
        public qc_inspection_type insp_type { get; set; }
        public defect_type def_type { get; set; }
        public int low { get; set; }
        public int high { get; set; }
        public int sample { get; set; }
        public int accepted_with { get; set; }
        public int rejected_with { get; set; }
    }
}

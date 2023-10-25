using System;

namespace sfc.BO
{
    public class qc_history
    {
        public qc_history(line l, qc_inspection_level c, qc_inspection_level ml, qc_inspection_level mp)
        {
            line_id = l;
            aql = new qc_aql_evaluation(c, ml, mp);
        }
        public line line_id { get; set; }
        public qc_aql_evaluation aql { get; set; }
    }
}

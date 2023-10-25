using System;

namespace sfc.BO
{
    public class qc_aql_evaluation
    {
        public qc_aql_evaluation(string ctr, string mln, string mem) 
        { 
            critics = new qc_inspection_level(ctr, ""); 
            major_line = new qc_inspection_level(mln, ""); 
            major_pack = new qc_inspection_level(mem, ""); 
        }

        public qc_aql_evaluation(qc_inspection_level ctr, qc_inspection_level mln, qc_inspection_level mem)
        {
            critics = ctr;
            major_line = mln;
            major_pack = mem;
        }

        public qc_inspection_level critics { get; set; }
        public qc_inspection_level major_line { get; set; }
        public qc_inspection_level major_pack { get; set; }
    }
}

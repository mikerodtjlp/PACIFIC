using System;

namespace sfc.BO
{
    public class qc_inspection_result
    {
        public qc_inspection_result(string ctr, string mln, string mem) { critics = ctr; major_line = mln; major_pack = mem; }
        public string critics { get; set; }
        public string major_line { get; set; }
        public string major_pack { get; set; }
    }
}

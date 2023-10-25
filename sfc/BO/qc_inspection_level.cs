using System;

namespace sfc.BO
{
    public class qc_inspection_level
    {
        public qc_inspection_level(string value, string sta) { level = value; status = sta;  }
        public string level { get; set; }
        public string status { get; set; }
    }
}

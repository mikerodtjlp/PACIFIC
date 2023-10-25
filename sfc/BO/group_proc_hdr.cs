using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace sfc.BO
{
    public class group_proc_hdr
    {
        public group_proc_hdr()
        {
            group_id = "";
            name = "";
        }
        private string group_id { get; set; }
        private string name { get; set; }
    }
}

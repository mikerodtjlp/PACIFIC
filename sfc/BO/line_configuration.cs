using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace sfc.BO
{
    public class line_configuration
    {
        public line_configuration()
        {
        }
        public string line { get; set; }
        public int check_status { get; set; }
        public int full_lean { get; set; }
        public string planning_type { get; set; }
        public string cast_coat { get; set; }
        public string block_insp_type { get; set; }
    }
}

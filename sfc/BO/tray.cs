using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace sfc.BO
{
    public class tray
    {
        public tray()
        {
        }
        public tray(string id, bool used)
        {
            this.id = id;
            this.used = used ? "Y" : "N";
        }
        public string id { get; set; }
        public string used { get; set; }
    }
}

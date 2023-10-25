using System;
using System.Collections.Generic;

namespace sfc.BO
{
    public class family
    {
     
        public string description { get; set; }
        public string id { get; set; }

        public List<product> products { get; set; }

        public family()
        {
            products = new List<product>();
        }
        public family(string id)
        {
            this.id = id;
            products = new List<product>();
        }

    }
}

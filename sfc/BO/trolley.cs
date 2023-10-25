using System;
using System.Collections.Generic;

namespace sfc.BO
{
    public class trolley
    {
        public trolley(string number)   { id = number; }
        public string id                { get; set; }
        public List<product_bulk> products = new List<product_bulk>();
    }
}

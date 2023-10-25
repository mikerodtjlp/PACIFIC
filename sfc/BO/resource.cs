using System;
using mro;

namespace sfc.BO
{
    public class resource
    {
        public resource()
        {
            isvalidated = false;
        }
        public resource(string sku) 
        { 
            id = sku;
            isvalidated = false;
        }

        public string opc_bar_code { get; set;}
        public string prod_code { get; set; }
        public string base_ { get; set;}
        public string addition { get; set; }
        public string eye { get; set; }
        public string id { get; set; }
        public string description { get; set; }
        public string cost_code { get; set; }
        public string std_code { get; set; }

//        public string ppc { get; set; }

        private bool isvalidated { get; set; }
        public void validate()
        {
            if (isvalidated) return;
            err.require(id.Length == 0, mse.INC_DAT_SKU);
            isvalidated = true;
        }
        public bool isactive { get; set; }

        //encapsulamiento del producto  
        public string product
        {
            get
            {
                if (string.IsNullOrEmpty(id)) return string.Empty;
                return id.Substring(0, 3);
            }
        }
    }
}

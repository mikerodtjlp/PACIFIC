using System;

namespace sfc.BO
{
    public class qc_product_test
    {
        public qc_product_test(string prodid, string insptype) 
        { 
            prod = new product(prodid); 
            insp_type = insptype; 
        }

        public product prod {get; set;}
        public string insp_type {get; set;}
        public string crear_insp { get; set; }
    }
}

using System;

namespace sfc.BO
{
    public class product_bulk
    {
        public product_bulk()
        {
            prod = null;
            baseno = null;
            qty = -1;
        }
        public product_bulk(product code, basenum basen, int quantity) 
        {
            prod = code;
            baseno = basen;
            qty = quantity;
        }
        public product_bulk(string code, string basen, int quantity)
        {
            prod = new product(code);
            baseno = new basenum(basen);
            qty = quantity;
        }
        public product prod     { get; set; }
        public basenum baseno   { get; set; }
        public int qty          { get; set; }
    }
}

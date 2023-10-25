using System;

namespace sfc.BO
{
    public class product_base_eye
    {
        public product_base_eye()
        {
            prod = null;
            baseno = null;
            eye_ = null;
            qty = -1;
        }
        public product_base_eye(product code, basenum basen, eye eye_, int quantity)
        {
            prod = code;
            baseno = basen;
            this.eye_ = eye_;
            qty = quantity;
        }
        public product_base_eye(string code, string basen, string eye_, int quantity)
        {
            prod = new product(code);
            baseno = new basenum(basen);
            this.eye_ = new eye(eye_);
            qty = quantity;
        }
        public product prod { get; set; }
        public basenum baseno { get; set; }
        public eye eye_ { get; set; }
        public int qty { get; set; }
    }
}

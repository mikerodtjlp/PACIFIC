using System;

namespace sfc.BO
{
    public class product_mold_relation
    {
        public product_mold_relation(string prod, string moldcode, string skuid)
        {
            prod_code = new product(prod);
            moldid = new mold(moldcode);
            sku = new resource(skuid);
            isvalidated = false;
        }
        public product_mold_relation(product prod, mold moldcode, resource skuid)
        {
            prod_code = prod;
            moldid = moldcode;
            sku = skuid;
            isvalidated = false;
        }

        public product prod_code { get; set; }
        public mold moldid { get; set; }
        public resource sku { get; set; }

        private bool isvalidated { get; set; }
        public void validate()
        {
            if (isvalidated) return;
            prod_code.validate();
            moldid.validate();
            sku.validate();
            isvalidated = true;
        }
    }
}

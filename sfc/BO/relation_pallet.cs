using System;
using mro;

namespace sfc.BO
{
    public class relation_pallet
    {
        public relation_pallet()
        {
            lote = new lot();
            module = string.Empty;
            palletid = new pallet(string.Empty);

            frontmold = new mold();
            backmold = new mold();
            sku = new resource();

            moldindex = string.Empty;
        }

        public lot lote { get; set; }
        public string module { get; set; }
        public pallet palletid { get; set; }
        public mold frontmold { get; set; }
        public mold backmold { get; set; }
        public resource sku { get; set; }
        public string moldindex { get; set; }
    }
}

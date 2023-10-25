using System;

namespace sfc.BO
{
    public class qc_batch_detail
    {
        public qc_batch_detail() { }

        public qc_batch_detail(batch bat, resource res, defect dft, zone zne)
        {
            lote = bat;
            sku = res;
            def = dft;
            zone_ = zne;
        }
        public qc_batch_detail(batch bat, resource res, defect dft, zone zne, int howmany)
        {
            lote = bat;
            sku = res;
            def = dft;
            zone_ = zne;
            qty = howmany;
        }

        public batch lote { get; set; }
        public resource sku { get; set; }
        public defect def { get; set; }
        public zone zone_ { get; set; }
        public int qty { get; set; }
    }
}

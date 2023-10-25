using System;

namespace sfc.BO
{
	public class qc_block_detail
	{
		public qc_block_detail() {}

		public qc_block_detail(batch bat, qc_block blk, int noinsp, location lct, resource res, defect dft, zone zne)
		{
			lote = bat;
			block = blk;
            this.noinsp = noinsp;
            loc = lct;
			sku = res;
			def = dft;
			zone_ = zne;
		}
		public qc_block_detail(batch bat, qc_block blk, int noinsp, location lct, resource res, defect dft, zone zne, int howmany)
		{
			lote = bat;
			block = blk;
            this.noinsp = noinsp;
            loc = lct;
			sku = res;
			def = dft;
			zone_ = zne;
			qty = howmany;
		}

		public batch lote {get; set;}
		public qc_block block {get; set;}
		public int noinsp { get; set; }
		public location loc {get; set;}
		public resource sku { get; set; }
		public defect def { get; set; }
		public zone zone_ { get; set; }
		public int qty { get; set; }
	}
}

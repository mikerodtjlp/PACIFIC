using System;

namespace sfc.BO
{
	public class qc_block_sample
	{
		public qc_block_sample() {}

		public qc_block_sample(batch bat, qc_block blk, int noinsp, resource res, int howmany)
		{
			lote = bat;
			block = blk;
			this.noinsp = noinsp;
			_sku = res;
			qty = howmany;
		}
		public qc_block_sample(string lot_no, string line_id, string blck, int noinsp, string sku, int howmany)
		{
			lote = new batch(new lot(lot_no), null, new line(line_id), "");
			block = new qc_block(blck);
			this.noinsp = noinsp;
			_sku = new resource(sku);
			qty = howmany;
		}
		/*public qc_block_sample(string lot_no, string line_id, string base_get, int howmany, int sample, string _part)
		{
			lote = new batch(new lot(lot_no), null, new line(line_id), "");
			qty = howmany;
			qty_sample = sample;
			part = _part;
			_base = base_get;
		}*/
		public batch lote {get; set;}
		public qc_block block {get; set;}
		public int noinsp { get; set; }
		public resource _sku { get; set; }
		public int qty { get; set; }
		public int qty_sample { get; set; }
		public string part { get; set; }
		public string _base { get; set; }
	}
}

using System;

namespace sfc.BO
{
	public class qc_block_header
	{
		public qc_block_header() { }
		/*public qc_block_header(batch l, qc_block b, int noinsp)
		{
			lot = l;
			block = b;
			this.noinsp = noinsp;
		}*/
		public qc_block_header(batch l, qc_block b, int noinsp, location loc)
		{
			lot = l;
			block = b;
			this.noinsp = noinsp;
			this.location = loc;
		}
        public qc_block_header(batch l, qc_block b, location loc)
        {
            lot = l;
            block = b;
            this.noinsp = -1;
            this.location = loc;
        }
        private DateTime? _finishdate = null; 

		public batch lot {get;set;}
		public qc_block block{get;set;}
		public int noinsp { get; set; }
		public location location { get; set; }
		public string status{get;set;}
		public DateTime creation_date{get;set;}
		public DateTime? finish_date
		{
			get
			{
				if (_finishdate == DateTime.MinValue)
				{
					return null;
				}
				return _finishdate;
			}
			set
			{
				_finishdate = value;
			}
		}
		public int total { get; set; }
		public int sample{get;set;}
		public operador oper{get;set;}
		public string disposition{get;set;}
		public defect reason_code { get; set; }
		public string res_ctr{get;set;}
		public string res_mln{get;set;}
		public string res_mem{get;set;}
		public string sta_ctr{get;set;}
		public string sta_mln{get;set;}
		public string sta_mem { get; set; }
		public string inspected { get; set; }
		public string comments { get; set; }
		public qc_aql_type aql { get; set; }
        public string part { get; set; }
    }
}

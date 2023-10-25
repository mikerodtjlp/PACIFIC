using System;

namespace sfc.BO
{
    public class production_order
    {
        private  DateTime? _finishdate = null; 

        public string year {get; set;}
        public string week {get; set;}
        public string line_id {get; set;}

        public string docid { get; set;}
        public string sku { get; set;}
        public int qty { get; set;}
        public int acum { get; set;}

        public DateTime startdate;
        public DateTime duedate;
        public DateTime? finishdate
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
        public string status { get; set;}
        public string mold_id { get; set;}
        public string size_type { get; set;}
        public int qty_molds { get; set;}

        public int mon { get; set;}
        public int tue { get; set;}
        public int wed { get; set;}
        public int thu { get; set;}
        public int fri { get; set;}
        public int days_need { get; set;}

        public int priority{ get; set;}
        public bool processed { get; set;}
    }
}

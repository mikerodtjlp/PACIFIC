using System;

namespace sfc.BO
{
    public class production_order_m2o
    {
        private  DateTime? _finishdate = null; 
        public string docid { get; set;}
        public string sku { get; set;}
        public int qty { get; set;}
        public int waited { get; set;}
        public int acum { get; set;}
        public int excess { get; set;}
        public string status { get; set;}
        public DateTime creationdate { get; set;}
        public DateTime duedate { get; set;}
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
        public int priority{ get; set;}
    }
}

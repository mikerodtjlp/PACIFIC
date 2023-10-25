using System;
using mro;

namespace sfc.BO
{
    public class item_urgent
    {
        public item_urgent()
        {
        }

        private DateTime? _date_time = null; 

        public resource sku { get; set; }

        public DateTime? date_time
        {
            get
            {
                if (_date_time == DateTime.MinValue)
                {
                    return null;
                }
                return _date_time;
            }
            set
            {
                _date_time = value;
            }
        }
    }
}

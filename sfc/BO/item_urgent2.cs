using System;
using mro;

namespace sfc.BO
{
    public class item_urgent2
    {
        public item_urgent2()
        {
        }

        public family fam { get; set; }
        private DateTime? _creation_date= null;

        public resource sku { get; set; }

        public DateTime? creation_date
        {
            get
            {
                if (_creation_date == DateTime.MinValue)
                {
                    return null;
                }
                return _creation_date;
            }
            set
            {
                _creation_date = value;
            }
        }
        public string planner { get; set; }
    }
}

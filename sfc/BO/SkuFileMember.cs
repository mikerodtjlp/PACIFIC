using System;

namespace sfc.BO
{
    public class SkuFileMember
    {
        public SkuFileMember()
        {
            sku = new resource();
        }
        public resource sku { get; set; }
        public DateTime creation_date { get; set; }        
        public string planner { get; set; }      
    }
}
 
using System;
using mro;

namespace sfc.BO
{
    public class product
    {
        public product() { isvalidated = false; }
        public product(string code) 
        { 
            id = code; 
            isvalidated = false; 
        }

        public string id            { get; set; }
        public int attribute        { get; set; }
        public string desc_long     { get; set; }
        public string desc_short    { get; set; }
        public string handed        { get; set; }
        public string material      { get; set; }
        public string group_        { get; set; }
        public string type          { get; set; }
        public int tiempo_standar   { get; set; }
        public int yield_nivel      { get; set; }
        public int inmold           { get; set; }
        public qc_aql_type aql_type { get; set; }
        public string mix_type      { get; set; }

        private bool isvalidated { get; set; }
        public void validate()
        {
            if (isvalidated) return;
            err.require(id == "", mse.INC_DAT_PROD);
            isvalidated = true;
        }

    }
}

using System;
using mro;

namespace sfc.BO
{
    public class line
    {
        public line() { isvalidated = false; }
        public line(string number)  
        { 
            id = number;
            isvalidated = false;
        }

        public string id            { get; set; }
        public string description   { get; set; }
        public string val_type      { get; set; }
        public string create_type   { get; set; }
        public string islean        { get; set; }

        private bool isvalidated { get; set; }
        public void validate()
        {
            if (isvalidated) return;
            err.require(id == "", mse.INC_DAT_LINE);
            isvalidated = true;
        }
    }
}

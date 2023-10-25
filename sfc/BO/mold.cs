using System;
using mro;

namespace sfc.BO
{
    public class mold
    {
        public mold() { isvalidated = false; }
        public mold(string code) 
        { 
            id = code;
            isvalidated = false;
        }
        public string id { get; set; }

        private bool isvalidated { get; set; }
        public void validate()
        {
            if (isvalidated) return; 
            err.require(id == "", mse.INC_DAT_MOLD);
            isvalidated = true;
        }

        public string name { get; set; }
        public string FB { get; set; }
        public string BasePwr { get; set; }
        public string AddPwr { get; set; }
        public string Eye { get; set; }
        public string Diameter { get; set; }

        public string AddPwr4 { get; set; }
        public string RangePwr { get; set; }
        public string sap { get; set; }
        public string msource { get; set; }
        public string mchange { get; set; }
        public string dmat { get; set; }
        public string dgrp { get; set; }
        public string ddep { get; set; }
        public string defect { get; set; }
        public string mold2 { get; set; }
        public DateTime date_time { get; set; }

        //public lot batch { get; set; }

        //public Boolean validation { get; set; }

        public int qty { get; set; }
        public String statusA { get; set; }
        public String statusN { get; set; }
        public operador oper { get; set; }
    }
}

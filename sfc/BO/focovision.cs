using System;
using mro;

namespace sfc.BO
{
    public class focovision
    {
        public focovision()
        {
            prodname = "";
            pcc = "";
            moldid = "";
            batch =  "";
            shift = "";
            module ="";
            part ="";
            oper = "";
            station ="";
            style= "";
            lensstatus = "";

            basesph = 0.0;
            basecyl=0.0;
            addsph=0.0;
            addition=0.0;

            sku= "";

            axis = 0.0;
            insplevel ="";
        }

            public string prodname  {get; set; }
            public string pcc  {get; set; }
            public string moldid  {get; set; }
            public string  batch {get; set; }
            public string shift  {get; set; }
            public string module {get; set; }
            public string part {get; set; }
            public string oper  {get; set; }
            public string station {get; set; }
            public string style {get; set; }
            public string lensstatus  {get; set; }

            public double basesph {get; set; }
            public double basecyl { get; set; }
            public double addsph { get; set; }
            public double addition { get; set; }

            public string sku {get; set; }

            public double axis { get; set; }
            public string insplevel {get; set; }

    }
}

using System;

namespace sfc.BO
{
    public class qc_batch_header
    {
        public qc_batch_header() { }
        public qc_batch_header(batch l)
        {
            lot = l;
        }
        public qc_batch_header(string bat, string prd, string lin, string prt,
                                int noinsp, DateTime date, string action, string comments,
                                int reason_code, int size, int sample,
                                string inspector, string oper, int time,
                                string resctr, string resmln, string resmem, string ressct,
                                string stactr, string stamln, string stamem, 
                                string inspected, string reinspection, string aql)
        {
            this.lot = new batch(bat, prd, lin, prt);
            this.noinsp = noinsp;
            this.date = date;
            this.action = action;
            this.comments = comments;
            this.reason_code = reason_code;
            this.size = size;
            this.sample = sample;
            this.inspector = inspector;
            this.oper = oper;
            this.time = time;
            this.resctr = resctr;
            this.resmln = resmln;
            this.resmem = resmem;
            this.ressct = ressct;
            this.stactr = stactr;
            this.stamln = stamln;
            this.stamem = stamem;
            this.inspected = inspected;
            this.reinspection = reinspection;
            this.aql = new qc_aql_type(aql);
        }

        public batch lot { get; set; }
        public int noinsp { get; set; }
        public DateTime date { get; set; }
        public string action { get; set; }
        public string comments { get; set; }
        public int reason_code { get; set; }
        public int size { get; set; }
        public int sample { get; set; }
        public string inspector { get; set; }
        public string oper {get; set; }
        public int time { get; set; }
        public string resctr { get; set; }
        public string resmln { get; set; }
        public string resmem { get; set; }
        public string ressct { get; set; }
        public string stactr { get; set; }
        public string stamln { get; set; }
        public string stamem { get; set; }
        public string inspected { get; set; }
        public string reinspection { get; set; }
        public qc_aql_type aql { get; set; }
    }
}

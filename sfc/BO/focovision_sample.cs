using System;
using mro;

namespace sfc.BO
{
    public class focovision_sample
    {
        private DateTime? _date_time = null; 

        public focovision_sample()
        {
            batch =  "";
            shift = "";
            module ="";
            pcc = "";
            base_= "";
            part ="";

            batch_size =0;
            batch_sample="";
        }
        public focovision_sample(string batch, string module, string base_, int batchsize, int batchsample)
        {
            this.batch =  batch;
            this.shift = "";
            this.module = module;
            this.pcc = "";
            this.base_= base_;
            this.part ="";

            this.batch_size = batchsize;
            this.batch_sample = batchsample.ToString();
        }

        public string  batch {get; set; }
        public string shift  {get; set; }
        public string module {get; set; }
        public string pcc  {get; set; }

        public string base_ {get; set; }

        public string part {get; set; }
        public int batch_size {get; set; }
        public string batch_sample {get; set; }

        public DateTime? creation_date
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
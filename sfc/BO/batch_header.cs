using System;
using System.Collections.Generic;
using mro;

namespace sfc.BO
{
    public class batch_header
    {
        public batch_header() { isvalidated = false; }
        private  DateTime? _date_time = null; 

        private lot _lotno;
        private product _product;
        private line _line;
        private location _location;
        public bool validation { get; set; }

        public void init(batch bat, location loc, int cyc)
        {
            _lotno = bat.lotno;
            _product = bat.product;
            _line = bat.line;
            part = bat.part;
            _location = loc;
            cycle = cyc;
           isvalidated = false;
        }

        public batch_header(batch bat, location loc)            { init(bat, loc, -1);  }
        public batch_header(batch bat, location loc, int cyc)   { init(bat, loc, cyc); }
        public batch_header(lot lot_no, product prod, line line_id, string parte, location loc, int cyc)
        {
            init(new batch(lot_no, prod, line_id, parte), loc, cyc);
        }
        public batch_header(lot lot_no, product prod, line line_id, string parte, location loc)
        {
            init(new batch(lot_no, prod, line_id, parte), loc, -1);
        }
        public batch_header(string lot_no, string prod, string line_id, string parte, string loc, int cyc)
        {
            init(new batch(new lot(lot_no), new product(prod), new line(line_id), parte), new location(loc), cyc);
        }
        public batch_header(string lot_no, string prod, string line_id, string parte, string loc)
        {
            init(new batch(new lot(lot_no), new product(prod), new line(line_id), parte), new location(loc),-1);
        }

        public lot lotno { get { return _lotno; } set { _lotno = value; } }
        public product product { get { return _product; } set { _product = value; } }
        public line line { get { return _line; } set { _line = value; } }
        public string part { get; set; }
        public location location { get { return _location; } set { _location = value; } }
        public int cycle {get; set;}

        public string status {get; set;}
        public string finishontime {get; set;}
        public string as_400 {get; set;}
        public string new_pkg_count { get; set; }
        public string qty_in { get; set; }
        public string qc_audit { get; set; }
        public string comments {get; set;}
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
        public int creation_type { get; set; }
        public int boxes { get; set; }

        public List<batch_detail> detail = new List<batch_detail>();

        public bool isinwip() 
        {
            return status.Length == 3 &&
            status[0] == 'W' && status[1] == 'I' && status[2] == 'P';
        }

        private bool isvalidated { get; set; }
        public void validate()
        {
            if (isvalidated) return;
            err.require(lotno.id == "", mse.INC_DAT_BATCH);
            err.require(product.id == "", mse.INC_DAT_PROD);
            err.require(line.id == "", mse.INC_DAT_LINE);
            err.require(part == "", mse.INC_DAT_PART);
            err.require(location.id == "", mse.INC_DAT_LOC);
            isvalidated = true;
        }
    }
}

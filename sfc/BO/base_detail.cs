using System;
using mro;

namespace sfc.BO
{
    public class base_detail
    {
        private lot _lotno;
        private product _product;
        private line _line;
        private string _part;

        public base_detail()
        {
            _lotno = new lot();
            _product = new product();
            _line = new line();
            _part = "";

            basedtl = new basetype("");

            _def = 0;
            total = 0;
            oper = "";

            _mat = 1;
            _grp = 2;
            _dep = 1;

            isvalidated = false;
        }

        public base_detail(lot lot_no, product prod, line line_id, string parte)
        {
            _lotno = lot_no;
            _product = prod;
            _line = line_id;
            _part = parte;

            basedtl = new basetype("");

            _def = 0;
            total = 0;
            oper = "";

            _mat = 1;
            _grp = 2;
            _dep = 1;

            isvalidated = false;
        }
        public base_detail(string lot_no, string prod, string line_id, string parte)
        {
            lot lt = new lot(lot_no);
            product p = new product(prod);
            line ln = new line(line_id);

            _lotno = lt;
            _product = p;
            _line = ln;
            _part = parte;

            basedtl = new basetype("");

            _def = 0;
            total = 0;
            oper = "";

            _mat = 1;
            _grp = 2;
            _dep = 1;

            isvalidated = false;
        }

        public lot lotno        { get { return _lotno; } }
        public product product  { get { return _product; } }
        public line line        { get { return _line; } }
        public string part      { get { return _part; } }

        public basetype basedtl { get; set; }

        public int _mat { get; set; }
        public int _grp { get; set; }
        public int _dep { get; set; }


        public int _def       { get; set; }
        public int total        { get; set; }
        public string oper      { get; set; }

        private bool isvalidated { get; set; }
        public void validate()
        {
            if (isvalidated) return;
            err.require(lotno.id == "", mse.INC_DAT_BATCH);
            err.require(product.id == "", mse.INC_DAT_PROD);
            err.require(line.id == "", mse.INC_DAT_LINE);
            err.require(part == "", mse.INC_DAT_PART);
            isvalidated = true;
        }
    }
}

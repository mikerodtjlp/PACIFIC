using System;
using mro;

namespace sfc.BO
{
    public class production_plan
    {
        private lot _lotno;
        private product _product;
        private line _line;

        public production_plan(batch bat)
        {
            _lotno = bat.lotno;
            _product = bat.product;
            _line = bat.line;
            isvalidated = false;
        }
        public production_plan(lot lot_no, product prod, line line_id)
        {
            _lotno = lot_no;
            _product = prod;
            _line = line_id;
            isvalidated = false;
        }
        public production_plan(lot lot_no, product prod, line line_id, resource res)
        {
            _lotno = lot_no;
            _product = prod;
            _line = line_id;
            resource_ = res;
            isvalidated = false;
        }
        public production_plan(string lot_no, string prod, string line_id)
        {
            var lt = new lot(lot_no);
            var p = new product(prod);
            var ln = new line(line_id);

            _lotno = lt;
            _product = p;
            _line = ln;
            isvalidated = false;
        }
        public production_plan(string lot_no, string prod, string line_id, string res)
        {
            var lt = new lot(lot_no);
            var p = new product(prod);
            var ln = new line(line_id);
            var rs = new resource(res);

            _lotno = lt;
            _product = p;
            _line = ln;
            resource_ = rs;
            isvalidated = false;
        }

        public lot lotno { get { return _lotno; } set { _lotno = value; } }
        public product product { get { return _product; } set { _product = value; } }
        public line line { get { return _line; } set { _line = value; } }

        public resource resource_ { get; set; }
        public int qty { get; set; }
        public int plan_cst { get; set; }
        public int acum { get; set; }
        public int band { get; set; }

        private bool isvalidated { get; set; }
        public void validate()
        {
            if (isvalidated) return;
            err.require(lotno.id == "", mse.INC_DAT_BATCH);
            err.require(product.id == "", mse.INC_DAT_PROD);
            err.require(line.id == "", mse.INC_DAT_LINE);
            isvalidated = true;
        }

    }
}

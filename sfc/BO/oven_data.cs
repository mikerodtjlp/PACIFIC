using System;
using mro;

namespace sfc.BO
{
    public class oven_data
    {
        private lot _lotno;
        private product _product;
        private line _line;

        public oven_data(batch bat)
        {
            _lotno = bat.lotno;
            _product = bat.product;
            _line = bat.line;
            part = bat.part;
            isvalidated = false;
        }
        public oven_data(lot lot_no, product prod, line line_id, string parte)
        {
            _lotno = lot_no;
            _product = prod;
            _line = line_id;
            part = parte;
            isvalidated = false;
        }
        public oven_data(string lot_no, string prod, string line_id, string parte)
        {
            lot lt = new lot(lot_no);
            product p = new product(prod);
            line ln = new line(line_id);

            _lotno = lt;
            _product = p;
            _line = ln;
            part = parte;
            isvalidated = false;
        }

        public lot lotno { get { return _lotno; } set { _lotno = value; } }
        public product product { get { return _product; } set { _product = value; } }
        public line line { get { return _line; } set { _line = value; } }
        public string part { get; set; }

        public oven oven { get; set; }

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

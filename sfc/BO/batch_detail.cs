using System;
using mro;

namespace sfc.BO
{
    public class batch_detail
    {
        private lot _lotno;
        private product _product;
        private line _line;
        private string _part;

        public batch_detail()
        {
            sku = new resource();
            isvalidated = false;
        }

        public batch_detail(lot lot_no, product prod, line line_id, string parte)
        {
            _lotno = lot_no;
            _product = prod;
            _line = line_id;
            _part = parte;
            sku = new resource();
            isvalidated = false;
        }
        public batch_detail(lot lot_no, product prod, line line_id, string parte, resource sku)
        {
            _lotno = lot_no;
            _product = prod;
            _line = line_id;
            _part = parte;
            this.sku = sku;
            isvalidated = false;
        }
        public batch_detail(string lot_no, string prod, string line_id, string parte)
        {
            lot lt = new lot(lot_no);
            product p = new product(prod);
            line ln = new line(line_id);

            _lotno = lt;
            _product = p;
            _line = ln;
            _part = parte;

            sku = new resource();
            isvalidated = false;
        }
        public batch_detail(string lot_no, string prod, string line_id, string parte, resource sku)
        {
            lot lt = new lot(lot_no);
            product p = new product(prod);
            line ln = new line(line_id);

            _lotno = lt;
            _product = p;
            _line = ln;
            _part = parte;

            this.sku = sku;
            isvalidated = false;
        }

        public lot lotno        { get { return _lotno; } }
        public product product  { get { return _product; } }
        public line line        { get { return _line; } }
        public string part      { get { return _part; } }

        public location location {get; set;}
        public int cycle {get; set;}

        public string detail_type { get; set; }
        public resource sku { get; set; }
        public int reason_code { get; set; }
        public int qty { get; set; }

        public batch_header get_header() { return new batch_header(lotno, product, line, part, location); } 

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

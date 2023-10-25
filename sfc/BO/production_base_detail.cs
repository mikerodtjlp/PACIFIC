using System;

namespace sfc.BO
{
    public class production_base_detail
    {
        public production_base_detail(batch b)
        {
            lotno = b.lotno;
            product = b.product;
            line = b.line;
            part = b.part;

            baseno = new basenum("");
            dep = new depto(0);
        }
        public production_base_detail(lot lot_no, product prod, line line_id, string parte)
        {
            lotno = lot_no;
            product = prod;
            line = line_id;
            part = parte;

            baseno = new basenum("");
            dep = new depto(0);
        }
        public production_base_detail (string lot_no, string prod, string line_id, string parte)
        {
            lot lt = new lot(lot_no);
            product p = new product(prod);
            line ln = new line(line_id);

            lotno = lt;
            product = p;
            line = ln;
            part = parte;

            baseno = new basenum("");
            dep = new depto(0);
        }
        public lot lotno        { get; set; }
        public product product  { get; set; }
        public line line        { get; set; }
        public string part      { get; set; }

        public basenum baseno   { get; set; }
        public depto  dep       { get; set; }
        public int defect       { get; set; }
        public int qty          { get; set; }
    }
}

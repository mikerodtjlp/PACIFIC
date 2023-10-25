using System;
using mro;

namespace sfc.BO
{
    public class batch_coat_cast_relation
    {
        private lot _cot_lotno;
        private product _cot_product;
        private line _cot_line;

        private lot _cst_lotno;
        private product _cst_product;
        private line _cst_line;

        public batch_coat_cast_relation(batch coat)
        {
            _cot_lotno = coat.lotno;
            _cot_product = coat.product;
            _cot_line = coat.line;
            cot_part = coat.part;

            isvalidated = false;
        }
        public batch_coat_cast_relation(lot cot_lot_no, product cot_prod, line cot_line_id, string cot_parte)
        {
            _cot_lotno = cot_lot_no;
            _cot_product = cot_prod;
            _cot_line = cot_line_id;
            cot_part = cot_parte;

            isvalidated = false;
        }
        public batch_coat_cast_relation(string cot_lot_no, string cot_prod, string cot_line_id, string cot_parte)                                        
        {
            _cot_lotno = new lot(cot_lot_no);
            _cot_product = new product(cot_prod); ;
            _cot_line = new line(cot_line_id);
            cot_part = cot_parte;

            isvalidated = false;
        }

        public batch_coat_cast_relation(batch coat, batch cast)
        {
            _cot_lotno = coat.lotno;
            _cot_product = coat.product;
            _cot_line = coat.line;
            cot_part = coat.part;

            _cst_lotno = cast.lotno;
            _cst_product = cast.product;
            _cst_line = cast.line;
            cst_part = cast.part;

            isvalidated = false;
        }

        public batch_coat_cast_relation(lot cot_lot_no, product cot_prod, line cot_line_id, string cot_parte,
                                        lot cst_lot_no, product cst_prod, line cst_line_id, string cst_parte)
        {
            _cot_lotno = cot_lot_no;
            _cot_product = cot_prod;
            _cot_line = cot_line_id;
            cot_part = cot_parte;

            _cst_lotno = cst_lot_no;
            _cst_product = cst_prod;
            _cst_line = cst_line_id;
            cst_part = cst_parte;

            isvalidated = false;
        }
        public batch_coat_cast_relation(string cot_lot_no, string cot_prod, string cot_line_id, string cot_parte,
                                        string cst_lot_no, string cst_prod, string cst_line_id, string cst_parte)
        {
            _cot_lotno = new lot(cot_lot_no);
            _cot_product = new product(cot_prod); ;
            _cot_line = new line(cot_line_id);
            cot_part = cot_parte;

            _cst_lotno = new lot(cst_lot_no);
            _cst_product = new product(cst_prod); ;
            _cst_line = new line(cst_line_id);
            cst_part = cst_parte;

            isvalidated = false;
        }

        public lot cot_lotno { get { return _cot_lotno; } set { _cot_lotno = value; } }
        public product cot_product { get { return _cot_product; } set { _cot_product = value; } }
        public line cot_line { get { return _cot_line; } set { _cot_line = value; } }
        public string cot_part { get; set; }

        public lot cst_lotno { get { return _cst_lotno; } set { _cst_lotno = value; } }
        public product cst_product { get { return _cst_product; } set { _cst_product = value; } }
        public line cst_line { get { return _cst_line; } set { _cst_line = value; } }
        public string cst_part { get; set; }

        public basenum baseno { get; set; }
        public resource resource_ { get; set; }
        public int qty {get; set;}

        private bool isvalidated { get; set; }
        public void validate()
        {
            if (isvalidated) return;

            err.require(cot_lotno.id == "", mse.INC_DAT_BATCH);
            err.require(cot_product.id == "", mse.INC_DAT_PROD);
            err.require(cot_line.id == "", mse.INC_DAT_LINE);
            err.require(cot_part == "", mse.INC_DAT_PART);

            err.require(cst_lotno.id == "", mse.INC_DAT_BATCH);
            err.require(cst_product.id == "", mse.INC_DAT_PROD);
            err.require(cst_line.id == "", mse.INC_DAT_LINE);
            err.require(cst_part == "", mse.INC_DAT_PART);

            isvalidated = true;
        }
    }
}

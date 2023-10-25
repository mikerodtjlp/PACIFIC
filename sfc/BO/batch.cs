using System;
using System.Collections.Generic;
using System.Text;
using mro;

namespace sfc.BO
{
    public class batch
    {
        private lot _lotno;
        private product _product;
        private line _line;

        private DateTime? _coatdate = null;
        private DateTime? _qcdate = null;
        private DateTime? _packdate = null; 

        public batch() { isvalidated = false; }
        public batch(batch b)
        {
            this.lotno = new lot(b.lotno.id);
            this.product = new product(b.product.id);
            this.line = new line(b.line.id);
            this.part = b.part;
            this.date = b.date;
            this.date_coat = b.date_coat;
            this.date_qc = b.date_qc;
            this.date_pack = b.date_pack;
            this.comentario = b.comentario;
            this.status = b.status;
            this.variacion = b.variacion;
        }
        public batch(lot lot_no, product prod, line line_id, string parte)
        {
            _lotno = lot_no;
            _product = prod;
            _line = line_id;
            part = parte;
            isvalidated = false;
            status = -1;
        }
        public batch(string lot_no, string line_id)
        {
            lot lt = new lot(lot_no);
            product p = null;
            line ln = new line(line_id);

            _lotno = lt;
            _product = p;
            _line = ln;
            part = string.Empty;
            isvalidated = false;
            status = -1;
        }
        public batch(string lot_no, string prod, string line_id, string parte)
        {
            lot lt = new lot(lot_no);
            product p = new product(prod);
            line ln = new line(line_id);

            _lotno = lt;
            _product = p;
            _line = ln;
            part = parte;
            isvalidated = false;
            status = -1;
        }       

        public lot lotno { get { return _lotno; } set { _lotno = value; } }
        public product product { get { return _product; } set { _product = value; } }
        public line line { get { return _line; } set { _line = value; } }
        public string part { get; set; }

        public DateTime date    { get; set; }
        public DateTime? date_coat
        {
            get
            {
                if (_coatdate == DateTime.MinValue)
                {
                    return null;
                }
                return _coatdate;
            }
            set
            {
                _coatdate = value;
            }
        }
        public DateTime? date_qc
        {
            get
            {
                if (_qcdate == DateTime.MinValue)
                {
                    return null;
                }
                return _qcdate;
            }
            set
            {
                _qcdate = value;
            }
        }
        public DateTime? date_pack
        {
            get
            {
                if (_packdate == DateTime.MinValue)
                {
                    return null;
                }
                return _packdate;
            }
            set
            {
                _packdate = value;
            }
        }
        public int status       { get; set; }
        public string comentario{ get; set; }
        public int variacion    { get; set; }

        public List<production_base_detail> detail = new List<production_base_detail>();
        public List<batch_header> locs = null;

        private bool isvalidated { get; set; }
        public void validate()
        {
            if (isvalidated) return;
            err.require(lotno.id.Length == 0, mse.INC_DAT_BATCH);
            err.require(lotno.id.Length != 4, mse.WRONG_FMT_BATCH);
            err.require(product.id.Length == 0, mse.INC_DAT_PROD);
            err.require(product.id.Length != 3, mse.WRONG_FMT_PROD);
            err.require(line.id.Length == 0, mse.INC_DAT_LINE);
            err.require(line.id.Length != 2, mse.WRONG_FMT_LINE);
            err.require(part.Length == 0, mse.INC_DAT_PART);
            isvalidated = true;
        }

        public string getfullbatch()
        {
            StringBuilder fullbatch = new StringBuilder();
            if (lotno   != null)    fullbatch.Append(lotno.id);
            if (product != null)    fullbatch.Append(product.id);
            if (line    != null)    fullbatch.Append(line.id);
            fullbatch.Append(part);
            return fullbatch.ToString();
        }
    }
}

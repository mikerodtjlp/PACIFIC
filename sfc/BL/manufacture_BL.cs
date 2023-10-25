using System;
using System.Collections.Generic;
using sfc.BO;
using sfc.DAL;
using mro;
using mro.BO;

namespace sfc.BL {
   /**
    * bussines logic for manufacturing
    */
   public class manufacture_BL {
      public manufacture_BL(CParameters conns) {
         conns.get(defs.ZDFAULT, ref dbcode);
      }
      public readonly string dbcode = string.Empty;

      private void validate_heijunka_key(constraint c, basetype b, diammeter di,
                                          diammeter df, basenum s, basenum f) {
         err.require(c.id == "", mse.INC_DAT_CONSTRAINT);
         err.require(b.id == "", mse.INC_DAT_BASE_TYPE);
         err.require(di.id == "", mse.INC_DAT_DIAMMETER_INI);
         err.require(df.id == "", mse.INC_DAT_DIAMMETER_FIN);
         err.require(s.graduation == "", mse.INC_DAT_BASE_INI);
         err.require(f.graduation == "", mse.INC_DAT_BASE_FIN);
      }

      private void validate_heijunka_record(constraint c, basetype b, diammeter di,
                                              diammeter df, basenum s, basenum f, string w) {
         validate_heijunka_key(c, b, di, df, s, f);
         err.require(string.IsNullOrEmpty(w), mse.INC_DAT_WEIGHT);
      }

      public void insert_heijunka_data(constraint c, basetype b, diammeter di, diammeter df,
                                          basenum s, basenum f, string w) {
         validate_heijunka_record(c, b, di, df, s, f, w);
         using (var dal = manufacture_DAL.instance(dbcode)) {
            dal.insert_heijunka_data(c, b, di, df, s, f, w);
         }
      }

      public void update_heijunka_data(constraint c, basetype b, diammeter di, diammeter df,
                                          basenum s, basenum f, string w) {
         validate_heijunka_record(c, b, di, df, s, f, w);
         using (var dal = manufacture_DAL.instance(dbcode)) {
            dal.update_heijunka_data(c, b, di, df, s, f, w);
         }
      }

      public void delete_heijunka_data(constraint c, basetype b, diammeter di, diammeter df,
                                          basenum s, basenum f) {
         validate_heijunka_key(c, b, di, df, s, f);
         using (var dal = manufacture_DAL.instance(dbcode)) {
            dal.delete_heijunka_data(c, b, di, df, s, f);
         }
      }

      /****** old code *****/
      public product get_product(string prod_code) {
         err.require(string.IsNullOrEmpty(prod_code), mse.INC_DAT_PROD);
         using (var dal = manufacture_DAL.instance(dbcode)) {
            return dal.get_product(prod_code);
         }
      }
      #region trolley
      public List<product_bulk> get_full_trolley_batch(lot lt, line l, string part, trolley t, int insp) {
         err.require(string.IsNullOrEmpty(lt.id), mse.INC_DAT_BATCH);
         err.require(string.IsNullOrEmpty(l.id), mse.INC_DAT_LINE);
         err.require(string.IsNullOrEmpty(t.id), mse.INC_DAT_TROLLEY);
         err.require(insp < 1 || insp > 999, mse.WRONG_NO_INSPECTION);
         using (var dal = manufacture_DAL.instance(dbcode)) {
            return dal.trolley_batch_get_full(lt, l, part, t, insp);
         }
      }
      public List<product_base_eye> get_trolley_batch(lot lt, line l, string p, trolley t, int insp) {
         err.require(string.IsNullOrEmpty(lt.id), mse.INC_DAT_BATCH);
         err.require(string.IsNullOrEmpty(l.id), mse.INC_DAT_LINE);
         err.require(string.IsNullOrEmpty(p), mse.INC_DAT_PART);
         err.require(string.IsNullOrEmpty(t.id), mse.INC_DAT_TROLLEY);
         err.require(insp < 1 || insp > 999, mse.WRONG_NO_INSPECTION);
         using (var dal = manufacture_DAL.instance(dbcode)) {
            return dal.trolley_batch_get_all(lt, l, p, t, insp);
         }
      }

      public void insert_trolley_batch(lot lt, line l, string prt, trolley t, int insp,
                                                 product_bulk o, string eye) {
         validate_trolley_batch_relation(lt, l, prt, t, insp, o);

         err.require(o.qty <= 0, mse.CANNOT_CAPTURE_ZERO);
         err.require(o.qty > 4000, mse.QTY_TOO_HIGH);
         err.require(eye.Length == 0, mse.INC_DAT_EYE);
         err.require(eye != "L" && eye != "R" && eye != "C", mse.WRONG_EYE);

         var p = new product(o.prod.id);
         var b = new batch(lt, p, l, prt);
         var exists = exist_batch(b);
         err.require(!exists, mse.BATCH_NOT_EXIST);

         using (var dal = manufacture_DAL.instance(dbcode)) {
            var bulk = dal.trolley_batch_get(lt, l, prt, t, insp, o, eye);
            err.require(bulk != null, mse.REG_ALREADY_EXIST);

            var bases = dal.get_bases(o.prod);
            err.require(bases == null, mse.BASE_NOT_EXIST);

            var basefound = false;
            foreach (var bn in bases) {
               if (bn.graduation == o.baseno.graduation) { basefound = true; break; }
            }
            if (!basefound) {
               var errdcs = o.prod.id + ":" + o.baseno.graduation;
               err.require(!basefound, mse.BASE_NOT_EXIST, errdcs);
            }

            if (exists) {
               var pbase = dal.get_specific_production_base(b, o.baseno);
            }

            // we check that the products are from the same group
            bool good = dal.trolley_check_prod_group(lt, l, prt, t, insp, o);
            err.require(!good, mse.PROD_NOT_BELONG_GROUP);

            dal.trolley_batch_insert(lt, l, prt, t, insp, o, eye);
         }
      }

      public void update_trolley_batch(lot lt, line l, string prt, trolley t, int insp,
                                                  product_bulk o, string eye) {
         validate_trolley_batch_relation(lt, l, prt, t, insp, o);

         err.require(o.qty <= 0, mse.CANNOT_CAPTURE_ZERO);
         err.require(o.qty > 4000, mse.QTY_TOO_HIGH);
         err.require(eye.Length == 0, mse.INC_DAT_EYE);
         err.require(eye != "L" && eye != "R" && eye != "C", mse.WRONG_EYE);

         var p = new product(o.prod.id);
         var b = new batch(lt, p, l, prt);
         var exists = exist_batch(b);
         err.require(!exists, mse.BATCH_NOT_EXIST);

         using (var dal = manufacture_DAL.instance(dbcode)) {
            dal.trolley_batch_update(lt, l, prt, t, insp, o, eye);
         }
      }

      public void delete_trolley_batch(lot lt, line l, string prt, trolley t, int insp,
                                                  product_bulk o, string eye) {
         validate_trolley_batch_relation(lt, l, prt, t, insp, o);

         err.require(eye.Length == 0, mse.INC_DAT_EYE);
         err.require(eye != "L" && eye != "R" && eye != "C", mse.WRONG_EYE);

         using (var dal = manufacture_DAL.instance(dbcode)) {
            dal.trolley_batch_delete(lt, l, prt, t, insp, o, eye);
         }
      }
      #endregion

      #region qc blocks

      public int trolley_get_last(lot lt, line l) {
         err.require(string.IsNullOrEmpty(lt.id), mse.INC_DAT_BATCH);
         err.require(string.IsNullOrEmpty(l.id), mse.INC_DAT_LINE);
         using (var dal = manufacture_DAL.instance(dbcode)) {
            return dal.trolley_get_last(lt, l);
         }
      }

      public List<product_bulk> get_full_trolley_batch_relation(lot lt, line l, string pt, trolley t, int insp) {
         err.require(string.IsNullOrEmpty(lt.id), mse.INC_DAT_BATCH);
         err.require(string.IsNullOrEmpty(l.id), mse.INC_DAT_LINE);
         err.require(string.IsNullOrEmpty(t.id), mse.INC_DAT_TROLLEY);
         err.require(insp < 1 || insp > 999, mse.WRONG_NO_INSPECTION);
         using (var dal = manufacture_DAL.instance(dbcode)) {
            return dal.trolley_batch_relation_get_full(lt, l, pt, t, insp);
         }
      }
      public List<product_bulk> get_trolley_batch_relation(lot lt, line l, string p, trolley t, int insp) {
         err.require(string.IsNullOrEmpty(lt.id), mse.INC_DAT_BATCH);
         err.require(string.IsNullOrEmpty(l.id), mse.INC_DAT_LINE);
         err.require(string.IsNullOrEmpty(p), mse.INC_DAT_PART);
         err.require(string.IsNullOrEmpty(t.id), mse.INC_DAT_TROLLEY);
         err.require(insp < 1 || insp > 999, mse.WRONG_NO_INSPECTION);
         using (var dal = manufacture_DAL.instance(dbcode)) {
            return dal.trolley_batch_relation_get_all(lt, l, p, t, insp);
         }
      }

      private void validate_trolley_batch_relation(lot lt, line l, string p, trolley t, int insp,
                                                  product_bulk o) {
         err.require(string.IsNullOrEmpty(lt.id), mse.INC_DAT_BATCH);
         err.require(string.IsNullOrEmpty(l.id), mse.INC_DAT_LINE);
         err.require(string.IsNullOrEmpty(p), mse.INC_DAT_PART);
         err.require(string.IsNullOrEmpty(t.id), mse.INC_DAT_TROLLEY);
         err.require(insp < 1 || insp > 999, mse.WRONG_NO_INSPECTION);
         err.require(string.IsNullOrEmpty(o.prod.id), mse.INC_DAT_PROD);
         err.require(string.IsNullOrEmpty(o.baseno.graduation), mse.INC_DAT_BASE);
      }

      public void insert_trolley_batch_relation(lot lt, line l, string prt, trolley t, int insp,
                                                 product_bulk o) {
         validate_trolley_batch_relation(lt, l, prt, t, insp, o);

         err.require(o.qty <= 0, mse.CANNOT_CAPTURE_ZERO);
         err.require(o.qty > 4000, mse.QTY_TOO_HIGH);

         var p = new product(o.prod.id);
         var b = new batch(lt, p, l, prt);
         var exists = exist_batch(b);
         err.require(!exists, mse.BATCH_NOT_EXIST);

         using (var dal = manufacture_DAL.instance(dbcode)) {
            var bulk = dal.trolley_batch_relation_get(lt, l, prt, t, insp, o);
            err.require(bulk != null, mse.REG_ALREADY_EXIST);

            var bases = dal.get_bases(o.prod);
            if (bases == null) {
               dal.trolley_batch_relation_delete_block(lt, l, prt, t, insp);
               err.require(true, mse.BASE_NOT_EXIST);
            }

            var basefound = false;
            foreach (var bn in bases) {
               if (bn.graduation == o.baseno.graduation) { basefound = true; break; }
            }
            if (!basefound) {
               var errdcs = o.prod.id + ":" + o.baseno.graduation;
               dal.trolley_batch_relation_delete_block(lt, l, prt, t, insp);
               err.require(true, mse.BASE_NOT_EXIST, errdcs);
            }

            if (exists) {
               var pbase = dal.get_specific_production_base(b, o.baseno);
            }

            dal.trolley_batch_relation_insert(lt, l, prt, t, insp, o);
         }
      }

      public void update_trolley_batch_relation(lot lt, line l, string prt, trolley t, int insp,
                                                  product_bulk o) {
         validate_trolley_batch_relation(lt, l, prt, t, insp, o);

         err.require(o.qty <= 0, mse.CANNOT_CAPTURE_ZERO);
         err.require(o.qty > 4000, mse.QTY_TOO_HIGH);

         var p = new product(o.prod.id);
         var b = new batch(lt, p, l, prt);
         var exists = exist_batch(b);
         err.require(!exists, mse.BATCH_NOT_EXIST);

         using (var dal = manufacture_DAL.instance(dbcode)) {
            dal.trolley_batch_relation_update(lt, l, prt, t, insp, o);
         }
      }

      public void delete_trolley_batch_relation(lot lt, line l, string prt, trolley t, int insp,
                                                  product_bulk o) {
         validate_trolley_batch_relation(lt, l, prt, t, insp, o);

         using (var dal = manufacture_DAL.instance(dbcode)) {
            dal.trolley_batch_relation_delete(lt, l, prt, t, insp, o);
         }
      }
      #endregion

      public bool exist_batch(batch b) {
         b.validate();

         using (var dal = manufacture_DAL.instance(dbcode)) {
            var lote = dal.get_batch(b);
            return lote != null;
         }
      }

      public void force_holded_batch_2_release_qc(batch b) {
         b.validate();
         using (var dal = manufacture_DAL.instance(dbcode)) {
            var lote = dal.get_batch(b);
            err.require(lote == null, mse.BATCH_NOT_EXIST);
            err.require(lote.status != batchstatus.HOLDED, mse.BATCH_MUSTBE_HOLD);

            lote.status = batchstatus.RELQCT;

            dal.update_batch(lote);
         }
      }

      public resource get_resource_by_barcode(resource res_) {
         using (var dal = manufacture_DAL.instance(dbcode)) {
            return dal.get_resource_by_barcode(res_);
         }
      }

      public void create_casting_batch(batch b) {
         b.validate();
         using (var dal = manufacture_DAL.instance(dbcode)) {
            dal.create_casting_batch(b);
         }
      }
      public void delete_casting_batch(batch b) {
         b.validate();
         using (var dal = manufacture_DAL.instance(dbcode)) {
            var bt = dal.get_batch(b);
            err.require(bt == null, mse.BATCH_NOT_EXIST);
            err.require(bt.status != batchstatus.WIPCST, mse.BATCH_MUSTBE_WIPCST);
            var cst = dal.get_batch_detail_all(new batch_header(b, consts.LOCCST, 1));
            err.require(cst.Count > 0, mse.BATCH_HAS_CAST_INFO);
            var cot = dal.get_batch_detail_all(new batch_header(b, consts.LOCCOT, 1));
            err.require(cot.Count > 0, mse.BATCH_HAS_COAT_INFO);
            var qct = dal.get_batch_detail_all(new batch_header(b, consts.LOCQCT, 1));
            err.require(qct.Count > 0, mse.BATCH_HAS_QC_INFO);
            var pkg = dal.get_batch_detail_all(new batch_header(b, consts.LOCPKG, 1));
            err.require(pkg.Count > 0, mse.BATCH_HAS_PKG_INFO);

            dal.delete_batch(b);
            dal.delete_batch_header(new batch_header(b, consts.LOCCST, 1));
         }
      }

      public void update_batch(batch b) {
         b.validate();
         using (var dal = manufacture_DAL.instance(dbcode)) {
            dal.update_batch(b);
         }
      }

      public batch get_batch(batch b) {
         b.validate();
         using (var dal = manufacture_DAL.instance(dbcode)) {
            var lote = dal.get_batch(b);
            err.require(lote == null, mse.BATCH_NOT_EXIST);
            return lote;
         }
      }

      /*public List<batch> get_batches_like(batch b)
      {
          if (b.lotno.id == string.Empty) return new List<batch>();
          using (var dal = manufacture_DAL.instance(dbcode))
          {
              return dal.get_batches_like(b);
          }
      }*/

      public batch_lot get_batch_lot(batch b, location l) {
         b.validate();
         using (var dal = manufacture_DAL.instance(dbcode)) {
            return dal.get_batch_lot(b, l);
         }
      }

      public batch_header get_batch_header(batch_header b) {
         b.validate();
         using (var dal = manufacture_DAL.instance(dbcode)) {
            var header = dal.get_batch_header(b);
            return header;
         }
      }
      public batch_header get_batch_header(batch b, location l, int cycle) {
         b.validate();
         using (var dal = manufacture_DAL.instance(dbcode)) {
            var header = dal.get_batch_header(b, l, cycle);
            return header;
         }
      }
      public void insert_batch_header(batch_header b) {
         b.validate();
         using (var dal = manufacture_DAL.instance(dbcode)) {
            dal.insert_batch_header(b);
         }
      }
      public void update_batch_header(batch_header b) {
         b.validate();
         using (var dal = manufacture_DAL.instance(dbcode)) {
            dal.update_batch_header(b);
         }
      }
      public void delete_batch_header(batch_header b) {
         b.validate();
         using (var dal = manufacture_DAL.instance(dbcode)) {
            dal.delete_batch_header(b);
         }
      }

      #region production detail

      public void accumulate_production_reject(lot l, product p, line li, string part,
                                                  resource r, int q, location loc, int cycle,
                                                  defect def, bool discountqty) {
         // we first acumulate the defect
         accumulate_production_detail(l, p, li, part, r, q, loc, cycle,
                                         consts.DEFTYPRJ, def);

         // then we must discount the quality if it is need, not all the process discounts
         if (discountqty) {
            using (var dal = manufacture_DAL.instance(dbcode)) {
               var detail = new batch_detail(l, p, li, part);
               detail.location = loc;
               detail.cycle = cycle;
               detail.detail_type = typeq.FQ;
               detail.reason_code = typeq.DEFFQ;
               detail.sku = r;

               var currdetail = get_batch_detail(detail);
               if (currdetail != null) // only if we find something we do something
               {
                  currdetail.qty -= q;
                  // never could exist zero or negatives quantities
                  if (currdetail.qty <= 0)
                     delete_batch_detail(currdetail);
                  else
                     update_batch_detail(currdetail);
               }
            }
         }
      }

      public void accumulate_production_detail(lot l, product p, line li, string part,
                                                  resource r, int q, location loc, int cycle,
                                                  defect_type dtltype, defect def) {
         using (var dal = manufacture_DAL.instance(dbcode)) {
            var detail = new batch_detail(l, p, li, part);
            detail.location = loc;
            detail.cycle = cycle;
            detail.detail_type = dtltype.id;
            detail.reason_code = int.Parse(def.id);
            detail.sku = r;

            var currdetail = get_batch_detail(detail);
            if (currdetail == null) {
               detail.qty = q;
               insert_batch_detail(detail);
            }
            else {
               currdetail.qty += q;
               update_batch_detail(currdetail);
            }
         }
      }
      public batch_detail get_batch_detail(batch_detail b) {
         b.validate();
         using (var dal = manufacture_DAL.instance(dbcode)) {
            var detail = dal.get_batch_detail(b);
            return detail;
         }
      }
      public void insert_batch_detail(batch_detail b) {
         b.validate();
         using (var dal = manufacture_DAL.instance(dbcode)) {
            dal.insert_batch_detail(b);
         }
      }
      public void update_batch_detail(batch_detail b) {
         b.validate();
         using (var dal = manufacture_DAL.instance(dbcode)) {
            dal.update_batch_detail(b);
         }
      }
      public void delete_batch_detail(batch_detail b) {
         b.validate();
         using (var dal = manufacture_DAL.instance(dbcode)) {
            dal.delete_batch_detail(b);
         }
      }
      public void delete_batch_detail_all(batch_header b) {
         b.validate();
         using (var dal = manufacture_DAL.instance(dbcode)) {
            dal.delete_batch_detail_all(b);
         }
      }
      #endregion

      public void daily_batch_must_be_in_wip(lot l, line li) {
         using (var dal = manufacture_DAL.instance(dbcode)) {
            int totalbatches = 0;
            int totalfinished = 0;
            var batches = dal.get_all_product_batches(l, li);
            foreach (var bat in batches) {
               //URI02 if (bat.status != batchstatus.RELEASE && bat.status != batchstatus.RELCSTCOT) totalfinished++;
               if (bat.status == batchstatus.RELEASE) totalfinished++; //URI02
               totalbatches++;
            }

            //URI02 err.require(totalbatches != totalfinished, mse.BATCH_MUSTBE_WIP);
            err.require(totalbatches == totalfinished, mse.BATCH_MUSTBE_WIP); //URI02
            err.require(totalbatches == 0, mse.BATCH_NOT_EXIST);
         }
      }

      public void batch_must_be_in_wip(batch b) {
         b.validate();
         using (var dal = manufacture_DAL.instance(dbcode)) {
            var bat = dal.get_batch(b);
            err.require(bat == null, mse.BATCH_NOT_EXIST);

            //err.require(bat.status == 9, "batch_released_by_qc");
            //err.require(bat.status == 4, "batch_in_wip_scanning");
            //err.require(bat.status == 5, "batch_in_rel_scanning");

            err.require(bat.status == batchstatus.RELEASE, mse.BATCH_IS_RELEASED);
            err.require(bat.status == batchstatus.RELCSTCOT, mse.BATCH_TAKEN_BY_COAT);
         }
      }

      public postcured_data get_postcured_data(postcured_data b) {
         b.validate();
         using (var dal = manufacture_DAL.instance(dbcode)) {
            return dal.get_postcured_data(b);
         }
      }
      public void save_postcured_data(postcured_data b) {
         b.validate();
         using (var dal = manufacture_DAL.instance(dbcode)) {
            var curr = dal.get_postcured_data(b);
            if (curr != null) {
               curr.oven = b.oven;
               curr.oper = b.oper;
               dal.update_postcured_data(curr);
            }
            else dal.insert_postcured_data(b);
         }
      }
      public resine_data get_resine_data(resine_data b) {
         b.validate();
         using (var dal = manufacture_DAL.instance(dbcode)) {
            return dal.get_resine_data(b);
         }
      }
      public void save_resine_data(resine_data b) {
         b.validate();
         err.require(b.date_time == null, mse.INC_DAT_DATE);
         using (var dal = manufacture_DAL.instance(dbcode)) {
            var curr = dal.get_resine_data(b);
            if (curr != null) {
               curr.lot_res = b.lot_res;
               curr.date_time = b.date_time;
               dal.update_resine_data(curr);
            }
            else dal.insert_resine_data(b);
         }
      }
      public oven_data get_oven_data(oven_data b) {
         b.validate();
         using (var dal = manufacture_DAL.instance(dbcode)) {
            return dal.get_oven_data(b);
         }
      }
      public void save_oven_data(oven_data b) {
         b.validate();
         using (var dal = manufacture_DAL.instance(dbcode)) {
            var curr = dal.get_oven_data(b);
            if (curr != null) {
               curr.oven = b.oven;
               dal.update_oven_data(curr);
            }
            else dal.insert_oven_data(b);
         }
      }


      public bool is_lean_line(line l) {
         l.validate();
         using (var dal = manufacture_DAL.instance(dbcode)) {
            var lin = dal.get_line(l);
            if (lin == null) return false;
            if (lin.islean == "Y") return true;
         }
         return false;
      }

      public void delete_coat_batch(batch hdr) {
         hdr.validate();

         err.require(is_lean_line(hdr.line), mse.MUST_NOT_BE_LEAN_LINE);

         using (var dal = manufacture_DAL.instance(dbcode)) {
            // first check that the batch is in reality on wip coating
            hdr = get_batch(hdr);
            err.require(hdr == null, mse.BATCH_NOT_EXIST);
            err.require(hdr.status != batchstatus.WIPCOT, mse.BATCH_MUSTBE_WIPCOT);

            // next we check that the capture is on wip
            var coat = new batch_header(hdr, consts.LOCCOT);
            coat = get_batch_header(coat);
            err.require(coat == null, mse.LOC_NOT_EXIST);
            coat.status = coat.status.Trim();
            err.require(coat.status != status.WIP, mse.CAP_MUSTBE_WIPCOT);

            // next we check that the coated batch has no link with any casting batch
            var rel = dal.get_batch_coat_cast_relation_all(hdr);
            err.require(rel.Count != 0, mse.BATCH_HAS_COAT_CAST_REL);

            // we check for extra location information (qc)
            var loc = new batch_header(hdr, consts.LOCQCT);
            loc = get_batch_header(loc);
            if (loc != null) {
               var s = dal.get_batch_detail_all(loc);
               err.require(s.Count != 0, mse.BATCH_HAS_QC_INFO);
            }

            // we check for extra location information (packaging)
            loc = new batch_header(hdr, consts.LOCPKG);
            loc = get_batch_header(loc);
            if (loc != null) {
               var s = dal.get_batch_detail_all(loc);
               err.require(s.Count != 0, mse.BATCH_HAS_PKG_INFO);
            }

            // next we check that the coating batch does not have any SKU info at all
            var skus = dal.get_batch_detail_all(coat);
            err.require(skus.Count != 0, mse.BATCH_HAS_COAT_INFO);

            // next we check that the coating batch does not have any reinspection SKU info at all
            var resinspcoat = new batch_header(hdr, new location(locs.COR));
            resinspcoat = get_batch_header(resinspcoat);
            err.require(resinspcoat != null, mse.BATCH_HAS_REINSP_COAT_INFO);

            // next we check that the coating batch does not have any base info at all
            var bases = dal.get_production_base_detail_all(hdr, new depto(2));
            err.require(bases.Count != 0, mse.BATCH_HAS_COAT_INFO);

            // next we check that the coating batch does not have any base reinspection info at all
            bases = dal.get_production_base_detail_all(hdr, new depto(9));
            err.require(bases.Count != 0, mse.BATCH_HAS_REINSP_COAT_INFO);

            // we delete the casting data generated if any, not guarratie that exists
            var cast = new batch_header(hdr, consts.LOCCST);
            cast = get_batch_header(coat);
            if (cast != null) dal.delete_batch_detail_all(cast);

            dal.delete_production_base_all(hdr, new depto(1));

            // at last we detele the batch header and status info of the batch
            dal.delete_batch_header(coat);
            dal.delete_batch_header(resinspcoat);
            dal.delete_batch(hdr);
         }

      }

      public string get_status_desc(int s, string l) {
         using (var dal = manufacture_DAL.instance(dbcode)) {
            return dal.get_status_desc(s, l);
         }
      }

      public mroJSON _get_production_data(string b, string p, string l) {
         using (var dal = manufacture_DAL.instance(dbcode)) {
            return dal._get_production_data(b, p, l);
         }
      }
      public void _loadinfoplanlens(string b, string p, string l) {
         using (var dal = manufacture_DAL.instance(dbcode)) {
            dal._loadinfoplanlens(b, p, l);
         }
      }
      public void _loadinfoplan(string b, string p, string l) {
         using (var dal = manufacture_DAL.instance(dbcode)) {
            dal._loadinfoplan(b, p, l);
         }
      }

      public string get_twin(product p) {
         using (var dal = manufacture_DAL.instance(dbcode)) {
            return dal.get_twin(p);
         }
      }
      public batch EXTRACT_BATCH_FROM_PARAMS(mroJSON values) {
         var b = new batch(values.get("cbatini"), values.get("cprdini"),
                             values.get("clinini"), values.get("cprtini"));
         b.validate();
         return b;
      }

      public void _break_long_batch(mroJSON values, mroJSON newval) {
         var lt = string.Empty;
         if (values.get("cbtlini", ref lt) > 0) {
            err.require(lt.Length != 10, mse.BATCH_BAD_FORMAT, lt);

            var b = lt.Substring(0, 4);
            var p = lt.Substring(4, 3);
            var l = lt.Substring(7, 2);
            var r = lt.Substring(9, 1);

            values.set("$batch$", b);
            values.set("$_prod$", p);
            values.set("$_line$", l);
            values.set("$_part$", r);

            newval.set("$batch$", b);
            newval.set("$_prod$", p);
            newval.set("$_line$", l);
            newval.set("$_part$", r);
         }
      }
      public void get_batch_basic_data(mroJSON basics, mroJSON values, mroJSON result) {
         var b = get_batch(EXTRACT_BATCH_FROM_PARAMS(values));
         err.require(b == null, mse.BATCH_NOT_EXIST);

         result.set("_$cast$", string.Format("fecha casting: {0}", b.date.ToString()));
         result.set("_$coat$", string.Format("fecha coating: {0}", b.date_coat.ToString()));
         result.set("_$qc__$", string.Format("fecha qc: {0}", b.date_qc.ToString()));
         result.set("_$pack$", string.Format("fecha packaging: {0}", b.date_pack.ToString()));
         result.set("_$wrh_$", string.Format("fecha warehouse: pending"));
         result.set("$comms$", string.Format("comentarios: {0}", b.comentario));
         result.set("$statushdr$", string.Format("{0}", b.status));
         result.set("_$sta_$", string.Format("status: |{0}| {1}", b.status, get_status_desc(b.status, basics.get(defs.ZLANGUA))));
         result.set("$wipdy$", string.Format("wip days(s): {0}", b.status == 11 ? (DateTime.Now - b.date).Days : 0));
      }

      public void get_location_basic_data_max_cycle(mroJSON basics, mroJSON values,
                                                      mroJSON result) {
         result.del("_$stal$");
         var b = EXTRACT_BATCH_FROM_PARAMS(values);
         var loc = values.get("clocini");

         err.require(loc.Length == 0, mse.INC_DAT_LOC);

         var bh = get_batch_header(b, new location(loc), -1);
         err.require(bh == null, mse.BATCH_NOT_EXIST);

         result.set("$fcrea$", string.Format("fecha creacion: {0}", bh.date_time.ToString()));
         result.set("_$loc_$", string.Format("location: {0}", bh.location.id));
         result.set("_$lsta$", string.Format("status loc: {0}", bh.status));
         result.set("_$stal$", bh.status);
         result.set("$commsb$", bh.comments);
         result.set("$boxes$", bh.boxes);
         result.set("$as400$", bh.as_400);
         result.set("$cycle$", bh.cycle);

         get_batch_basic_data(basics, values, result);
      }

      public void get_batch_basic_data_insp(mroJSON basics,
                                             mroJSON values,
                                             mroJSON result,
                                             mroJSON newval) {
         get_batch_basic_data(basics, values, result);
         var qh = bls.get_qcbl().get_qc_batch_last_inspection(EXTRACT_BATCH_FROM_PARAMS(values));
         result.set("$_insp$", qh.noinsp);
         newval.set("$_insp$", qh.noinsp);
      }

      public bool is_transition(product p) {
         using (var dal = manufacture_DAL.instance(dbcode)) {
            return dal.is_transition(p);
         }
      }
      public defect_source get_defect_source(string s) {
         using (var dal = manufacture_DAL.instance(dbcode)) {
            return dal.get_defect_source(s);
         }
      }
      public List<product_bulk> get_batch_product_base(  lot lt, 
                                                         line l, 
                                                         string part, 
                                                         location loc) {
         err.require(string.IsNullOrEmpty(lt.id), mse.INC_DAT_BATCH);
         err.require(string.IsNullOrEmpty(l.id), mse.INC_DAT_LINE);
         using (var dal = manufacture_DAL.instance(dbcode)) {
            return dal.get_batch_product_base(lt, l, part, loc);
         }
      }

      #region palletes
      public List<relation_pallet> get_relation_pallet(lot lt, line ln) {
         using (var dal = manufacture_DAL.instance(dbcode)) {
            return dal.get_relation_pallet(lt, ln);
         }
      }
      public relation_pallet get_relation_pallet_dtl(lot lt, line ln, pallet pl) {
         using (var dal = manufacture_DAL.instance(dbcode)) {
            return dal.get_relation_pallet_dtl(lt, ln, pl);
         }
      }
      public void insert_relation_pallet(relation_pallet p) {
         using (var dal = manufacture_DAL.instance(dbcode)) {
            dal.insert_relation_pallet(p);
         }
      }
      public void delete_relation_pallet(lot lt, line ln, pallet pl) {
         using (var dal = manufacture_DAL.instance(dbcode)) {
            dal.delete_relation_pallet(lt, ln, pl);
         }
      }
      public void delete_whole_relation_pallet(lot lt, line ln) {
         using (var dal = manufacture_DAL.instance(dbcode)) {
            dal.delete_whole_relation_pallet(lt, ln);
         }
      }
      public void change_relation_pallet_batch(lot src, lot dst, line ln) {
         using (var dal = manufacture_DAL.instance(dbcode)) {
            dal.change_relation_pallet_batch(src, dst, ln);
         }
      }

      public bool is_mold_for_validate(lot lt, product pr, line ln, mold ml) {
         using (var dal = manufacture_DAL.instance(dbcode)) {
            return dal.is_mold_for_validate(lt, pr, ln, ml);
         }
      }

      public bool do_match_front_back(product pr, string bfront, string bback) {
         using (var dal = manufacture_DAL.instance(dbcode)) {
            return dal.do_match_front_back(pr, bfront, bback);
         }
      }

      public bool is_mold_on_production(mold ml) {
         using (var dal = manufacture_DAL.instance(dbcode)) {
            return dal.is_mold_on_production(ml);
         }
      }

      public relation_pallet get_mold_in_another_line(lot lt, mold ml, string shift, bool isfront) {
         using (var dal = manufacture_DAL.instance(dbcode)) {
            return dal.get_mold_in_another_line(lt, ml, shift, isfront);
         }
      }

      public Tuple<string, string> get_pallete_index(product pr, resource sku) {
         using (var dal = manufacture_DAL.instance(dbcode)) {
            return dal.get_pallete_index(pr, sku);
         }
      }
      public Tuple<string, string> get_pallete_index(product pr, mold ml) {
         using (var dal = manufacture_DAL.instance(dbcode)) {
            return dal.get_pallete_index(pr, ml);
         }
      }
      #endregion

      #region obsolete
      /*public production_base_detail get_production_base(batch b)
            {
                  if (b.lotno.id == "") throw new Exception(mse.INC_DAT_BATCH);
                  if (b.product.id == "") throw new Exception(mse.INC_DAT_PROD);
                  if (b.line.id == "") throw new Exception(mse.INC_DAT_LINE);
                  if (b.part == "") throw new Exception(mse.INC_DAT_PART);

                      var pdetail = manufacture_DAL.instance.get_production_base(b);
                      if (pdetail == null) throw new Exception(mse.BATCH_NOT_EXIST);

                  return pdetail;
            }*/

      /*public void sku_2_bases(batch b, location l, depto d)
              {
               b.validate();
               err.require(l == null || l.id == "", mse.INC_DAT_LOC);
               err.require(d == null, mse.INC_DAT_DEP);

               using (var dal = manufacture_DAL.instance(dbcode))
               {
                   dal.delete_production_base_all(b, d);

                   var hdr = new batch_header(b, l, 1);
                   //                hdr.location = l;
                   //                hdr.cycle = 1;
                   var skus = dal.get_batch_detail_all(hdr);
                   foreach (var sku in skus)
                   {
                       dal.insert_production_base(b, d,
                                                           new basenum(sku.sku.id.Substring(3, 4)),
                                                           new defect(sku.reason_code),
                                                           sku.qty,
                                                           consts.EMPTYOPER);
                   }
               }
              }*/
      /* public void set_wip_qc_2_release_qc(batch b)
              {
                  b.validate();
                  using (var dal = manufacture_DAL.instance(dbcode))
                  {
                      var lote = dal.get_batch(b);
                      if (lote == null) throw new Exception(mse.BATCH_NOT_EXIST);
                      if (lote.status != 0) throw new Exception("batch_must_be_wip");

                      lote.status = 9;

                      dal.update_batch(lote);
                  }
              }*/
      public void update_all_moldloss(batch b, int status) {
         b.validate();
         using (var dal = manufacture_DAL.instance(dbcode)) {
            var lote = dal.get_batch(b);
            if (lote == null) throw new Exception(mse.BATCH_NOT_EXIST);
            if (lote.status != 0) throw new Exception("batch_must_be_wip");

            dal.update_all_moldloss(lote, status);
         }
      }
      public void update_moldloss(batch b, int order, int status) {
         b.validate();
         using (var dal = manufacture_DAL.instance(dbcode)) {
            var lote = dal.get_batch(b);
            if (lote == null) throw new Exception(mse.BATCH_NOT_EXIST);
            if (lote.status != 0) throw new Exception("batch_must_be_wip");

            dal.update_moldloss(lote, order, status);
         }
      }
      /*public void update_moldloss(batch b, int order, int status)
      {
          b.validate();
          using (var dal = manufacture_DAL.instance(dbcode))
          {
              var lote = dal.get_batch(b);
              if (lote == null) throw new Exception(mse.BATCH_NOT_EXIST);
              if (lote.status != 0) throw new Exception("batch_must_be_wip");

              dal.update_moldloss(lote, order, status);
          }
      }*/
      public int how_many_pending_moldloss(batch b) {
         b.validate();
         using (var dal = manufacture_DAL.instance(dbcode)) {
            var lote = dal.get_batch(b);
            if (lote == null) throw new Exception(mse.BATCH_NOT_EXIST);
            if (lote.status != 0) throw new Exception("batch_must_be_wip");

            return dal.how_many_pending_moldloss(lote);
         }
      }
      public moldloss_detail get_moldloss_dtl(string m) {
         using (var dal = manufacture_DAL.instance(dbcode)) {
            return dal.get_moldloss_dtl(m);
         }
      }

      public void insert_moldloss_dtl(moldloss_detail p) {
         using (var dal = manufacture_DAL.instance(dbcode)) {
            dal.insert_moldloss_dtl(p);
         }
      }

      public void update_moldloss_dtl(moldloss_detail p) {
         using (var dal = manufacture_DAL.instance(dbcode)) {
            dal.update_moldloss_dtl(p);
         }
      }

      public void delete_moldloss_dtl(string m) {
         using (var dal = manufacture_DAL.instance(dbcode)) {
            dal.delete_moldloss_dtl(m);
         }
      }
      #endregion

      #region coat
      public void check_product_is_coated(product prod) {
         using (var dal = manufacture_DAL.instance(dbcode)) {
            var iscoated = dal.check_product_is_coated(prod.id);
            err.require(!iscoated, "prod_not_coated");
         }
      }
      public void update_coat_rejects_and_cast_fq(batch b) {
         /*var coatloc = new location("COT");

      using (var dal = qc_DAL.instance(dbcode))
      {
            var detail = new batch_detail(b.lotno, b.product, b.line, b.part);
            detail.location coatloc;
            detail.cycle = 1;
            detail.detail_type = typeq.RJ;
            detail.reason_code = int.Parse(d.def.id);
            detail.sku = d.sku;

            var currdetail = manbl.get_batch_detail(detail);
            if (currdetail == null)
            {
               detail.qty = d.qty;
               manbl.insert_batch_detail(detail);
            }
            else
            {
               currdetail.qty += d.qty;
               manbl.update_batch_detail(currdetail);
            }
         }
      }*/
      }
      #endregion
   }
}

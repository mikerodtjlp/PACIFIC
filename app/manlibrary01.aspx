<%@ Import Namespace="sfc" %>
<%@ Import Namespace="sfc.BL" %>
<%@ Import Namespace="sfc.BO" %>

<!-- #include file="~/core.aspx" -->

<script runat="server" language="C#">

   protected void break_long_batch() {
       bls.get_manbl()._break_long_batch(values, newval);
   }
   protected void get_batch_basic_data() {
       bls.get_manbl().get_batch_basic_data(basics, values, result);
   }
   protected void get_location_basic_data_max_cycle() {
       bls.get_manbl().get_location_basic_data_max_cycle(basics, values, result);
   }
   protected void get_batch_basic_data_insp() {
       bls.get_manbl().get_batch_basic_data_insp(basics, values, result, newval);
   }
   protected void save_comments() {
       var btc = values.get("cbatini");
       var prd = values.get("cprdini");
       var lin = values.get("clinini");
       var prt = values.get("cprtini");
       var cms = values.get("ccommens");
       var loc = values.get("clocini");
       var cyc = values.getint("ccycini");
       var msb = values.get("ccapmustbe");

       var man = bls.get_manbl();
       var bat = man.get_batch_header(new batch_header(btc, prd, lin, prt, loc, cyc));
       err.require(bat == null, mse.LOC_NOT_EXIST);

       // pueder ser que necesite estar en un status specifico
       if (msb.Length == 0)
           err.require(bat.status != msb, mse.WRONG_STATUS);

       bat.comments = cms;
       man.update_batch_header(bat);
   }
   protected void get_trolley_batch() {
       var btc = values.get("$batch$");
       var lin = values.get("$_line$");
       var prt = values.get("$_part$");
       var trl = values.get("$trolley$");
       var insp = values.getint("$noinsp$", 1);

       if (string.IsNullOrEmpty(prt)) prt = "1";

       var lt = new lot(btc);
       var l = new line(lin);
       var t = new trolley(trl);

       var relation = bls.get_manbl().get_trolley_batch(lt, l, prt, t, insp);

       int listid = values.getint(defs.ZLISTAF);
       var lr = new ListResponse(listid, 4, lnk.clie);
       var row = 0;

       foreach (var o in relation) {
           lr.set_data(row, 'A', o.prod.id, 'B', o.baseno.graduation,
               'C', o.eye_.type, 'D', o.qty.ToString(), '*', "1");
           ++row;
       }

       lr.set_rows(row);
       lr.pass_to_obj(result);
   }

   protected void update_trolley_batch() {
       var btc = values.get("$batch$");
       var lin = values.get("$_line$");
       var prt = values.get("$_part$");
       var trl = values.get("$trolley$");
       var ins = values.getint("$noinsp$", 1);
       var prd = values.get("$prod$");
       var bas = values.get("$base$");
       var eye = values.get("$eye$");
       var qty = values.get("$qty$");

       if (string.IsNullOrEmpty(prt)) prt = "1";

       var lt = new lot(btc);
       var l = new line(lin);
       var t = new trolley(trl);

       err.require(qty.Length == 0, mse.INC_DAT_QTY);
       err.require(qty == "0", mse.QTY_ZERO);
       var o = new product_bulk(prd, bas, int.Parse(qty));

       var man = bls.get_manbl();
       var hdr = man.get_batch(new batch(btc, prd, lin, prt));
       err.require(hdr.status == batchstatus.RELEASE, mse.BATCH_MUSTBE_WIP);

       man.update_trolley_batch(lt, l, prt, t, ins, o, eye);
   }

   protected void insert_trolley_batch() {
       var btc = values.get("$batch$");
       var lin = values.get("$_line$");
       var prt = values.get("$_part$");
       var trl = values.get("$trolley$");
       var ins = values.getint("$noinsp$", 1);
       var prd = values.get("$prod$");
       var bas = values.get("$base$");
       var eye = values.get("$eye$");
       var qty = values.get("$qty$");

       if (string.IsNullOrEmpty(prt)) prt = "1";

       var lt = new lot(btc);
       var l = new line(lin);
       var t = new trolley(trl);

       err.require(qty.Length == 0, mse.INC_DAT_QTY);
       err.require(qty == "0", mse.QTY_ZERO);
       var o = new product_bulk(prd, bas, int.Parse(qty));

       var man = bls.get_manbl();
       var hdr = man.get_batch(new batch(btc, prd, lin, prt));
       err.require(hdr.status == batchstatus.RELEASE, mse.BATCH_MUSTBE_WIP);

       man.insert_trolley_batch(lt, l, prt, t, ins, o, eye);
   }

   protected void delete_trolley_batch() {
       var btc = values.get("$batch$");
       var lin = values.get("$_line$");
       var prt = values.get("$_part$");
       var trl = values.get("$trolley$");
       var ins = values.getint("$noinsp$", 1);
       var prd = values.get("$prod$");
       var bas = values.get("$base$");
       var eye = values.get("$eye$");

       if (string.IsNullOrEmpty(prt)) prt = "1";

       var lt = new lot(btc);
       var l = new line(lin);
       var t = new trolley(trl);
       var o = new product_bulk(prd, bas, 0);

       var man = bls.get_manbl();
       var hdr = man.get_batch(new batch(btc, prd, lin, prt));
       err.require(hdr.status == batchstatus.RELEASE, mse.BATCH_MUSTBE_WIP);

       man.delete_trolley_batch(lt, l, prt, t, ins, o, eye);
   }
   // qc blocks capture
   protected void get_trolley_batch_relation() {
       var btc = values.get("$batch$");
       var lin = values.get("$_line$");
       var prt = values.get("$_part$");
       var trl = values.get("$qcblock$");
       var insp = values.getint("$noinsp$", 1);

       if (string.IsNullOrEmpty(prt)) prt = "1";

       var lt = new lot(btc);
       var l = new line(lin);
       var t = new trolley(trl);

       var relation = bls.get_manbl().get_trolley_batch_relation(lt, l, prt, t, insp);

       int listid = values.getint(defs.ZLISTAF);
       var lr = new ListResponse(listid, 3, lnk.clie);
       var row = 0;

       foreach (var o in relation) {
           lr.set_data(row, 'A', o.prod.id, 'B', o.baseno.graduation, 'C', o.qty.ToString(), '*', "1");
           ++row;
       }

       lr.set_rows(row);
       lr.pass_to_obj(result);
   }

   protected void update_trolley_batch_relation() {
       var btc = values.get("$batch$");
       var lin = values.get("$_line$");
       var prt = values.get("$_part$");
       var trl = values.get("$qcblock$");
       var insp = values.getint("$noinsp$", 1);
       var prd = values.get("$prod$");
       var bas = values.get("$base$");
       var qty = values.get("$qty$");

       if (string.IsNullOrEmpty(prt)) prt = "1";

       err.require(insp == -1, mse.INC_DAT_NOINSP);
       err.require(insp == 1, mse.QC_ONLY_FOR_INSP_2_HIGHER);

       var lt = new lot(btc);
       var l = new line(lin);
       var t = new trolley(trl);

       err.require(qty.Length == 0, mse.INC_DAT_QTY);
       err.require(qty == "0", mse.QTY_ZERO);
       var o = new product_bulk(prd, bas, int.Parse(qty));

       /*var qcinsp = bls.get_qcbl(maindb).get_qc_inspection(
                                           new batch(lt, new product("XXX"), l, "X"),
                                           new qc_block(t.id), insp, consts.LOCQCT);
       // could be that the qc insp existed, and if it is so, it must be WIP
       if (qcinsp != null)
           err.require(qcinsp.status != qcinspstatus.WIP, mse.QCBLOCK_INSP_MUSTBE_WIP);
       */
       var man = bls.get_manbl();
       var hdr = man.get_batch(new batch(btc, prd, lin, prt));
       err.require(hdr.status == batchstatus.RELEASE, mse.BATCH_MUSTBE_WIP);

       man.update_trolley_batch_relation(lt, l, prt, t, insp, o);
   }

   protected void insert_trolley_batch_relation() {
       var btc = values.get("$batch$");
       var lin = values.get("$_line$");
       var prt = values.get("$_part$");
       var trl = values.get("$qcblock$");
       var insp = values.getint("$noinsp$", 1);
       var prd = values.get("$prod$");
       var bas = values.get("$base$");
       var qty = values.get("$qty$");

       if (string.IsNullOrEmpty(prt)) prt = "1";

       err.require(insp == -1, mse.INC_DAT_NOINSP);
       err.require(insp == 1, mse.QC_ONLY_FOR_INSP_2_HIGHER);

       var lt = new lot(btc);
       var l = new line(lin);
       var t = new trolley(trl);

       err.require(qty.Length == 0, mse.INC_DAT_QTY);
       err.require(qty == "0", mse.QTY_ZERO);
       var o = new product_bulk(prd, bas, int.Parse(qty));

       /*var qcinsp = bls.get_qcbl(maindb).get_qc_inspection(
                                           new batch(lt, new product("XXX"), l, "X"),
                                           new qc_block(t.id), insp, consts.LOCQCT);
       // could be that the qc insp existed, and if it is so, it must be WIP
       if (qcinsp != null)
           err.require(qcinsp.status != qcinspstatus.WIP, mse.QCBLOCK_INSP_MUSTBE_WIP);
       */
       var man = bls.get_manbl();
       var hdr = man.get_batch(new batch(btc, prd, lin, prt));
       err.require(hdr.status == batchstatus.RELEASE, mse.BATCH_MUSTBE_WIP);

       man.insert_trolley_batch_relation(lt, l, prt, t, insp, o);
   }

   protected void delete_trolley_batch_relation() {
       var btc = values.get("$batch$");
       var lin = values.get("$_line$");
       var prt = values.get("$_part$");
       var trl = values.get("$qcblock$");
       var insp = values.getint("$noinsp$", 1);
       var prd = values.get("$prod$");
       var bas = values.get("$base$");

       if (string.IsNullOrEmpty(prt)) prt = "1";

       err.require(insp == -1, mse.INC_DAT_NOINSP);
       err.require(insp == 1, mse.QC_ONLY_FOR_INSP_2_HIGHER);

       var lt = new lot(btc);
       var l = new line(lin);
       var t = new trolley(trl);
       var o = new product_bulk(prd, bas, 0);

       /*var qcinsp = bls.get_qcbl(maindb).get_qc_inspection(
                                           new batch(lt, new product("XXX"), l, "X"),
                                           new qc_block(t.id), insp, consts.LOCQCT);
       // could be that the qc insp existed, and if it is so, it must be WIP
       if (qcinsp != null)
           err.require(qcinsp.status != qcinspstatus.WIP, mse.QCBLOCK_INSP_MUSTBE_WIP);
      */
       var man = bls.get_manbl();
       var hdr = man.get_batch(new batch(btc, prd, lin, prt));
       err.require(hdr.status == batchstatus.RELEASE, mse.BATCH_MUSTBE_WIP);

       man.delete_trolley_batch_relation(lt, l, prt, t, insp, o);
   }

   protected void move_skus_same_batch() {
       var batch = values.get("cbatini");
       var prod = values.get("cprdini");
       var line = values.get("clinini");
       var partsource = values.get("cprtini");
       var partdestiny = values.get("cprtfin");
       var cycle = values.get("ccycle");

       err.require(batch.Length == 0, mse.INC_DAT_BATCH);
       err.require(prod.Length == 0, mse.INC_DAT_PROD);
       err.require(line.Length == 0, mse.INC_DAT_LINE);
       err.require(partsource.Length == 0, mse.INC_DAT_PART_SRC);
       err.require(partdestiny.Length == 0, mse.INC_DAT_PART_DST);
       err.require(partsource == partdestiny, mse.SAME_BATCH);

       var mbl = bls.get_manbl();

       var prd = mbl.get_product(prod);
       err.require(prd == null, mse.PROD_NOT_EXISTS);
       //err.require(prd.type == ?, "prod_mustbe_transition");

       var bs = mbl.get_batch(new batch(batch, prod, line, partsource));
       // checamos que los batches existan y que esten en el status correcto
       err.require(bs == null, mse.BATCH_SOURCE_NOT_EXIST);
       err.require(bs.status == batchstatus.RELEASE, mse.BAT_SRC_MUSTBE_WIPCST);

       var bd = mbl.get_batch(new batch(batch, prod, line, partdestiny));
       // checamos que los batches existan y que esten en el status correcto
       err.require(bd == null, mse.BATCH_DESTINY_NOT_EXIST);
       err.require(bd.status == batchstatus.RELEASE, mse.BAT_DES_MUSTBE_WIPCST);

       var bhs = mbl.get_batch_header(new batch_header(batch, prod, line, partsource, locs.CST));
       // checamos que las locaciones existan y que esten en el status correcto
       err.require(bhs == null, mse.CAP_SOURCE_NOT_EXISTS);
       err.require(bhs.status.Length < 3, mse.CAP_SRC_MUSTBE_WIPCST);
       err.require(bhs.status.Substring(0, 3) != status.WIP, mse.CAP_SRC_MUSTBE_WIPCST);

       var bhd = mbl.get_batch_header(new batch_header(batch, prod, line, partdestiny, locs.CST));
       // checamos que las locaciones existan y que esten en el status correcto
       err.require(bhd == null, mse.CAP_DESTINY_NOT_EXISTS);
       err.require(bhd.status.Length < 3, mse.CAP_DES_MUSTBE_WIPCST);
       err.require(bhd.status.Substring(0, 3) != status.WIP, mse.CAP_DES_MUSTBE_WIPCST);

       var sku = string.Empty;
       var defect = string.Empty;
       var qty = string.Empty;
       var detailtype = string.Empty;

       var lstdata = new mroJSON();
       values.get(defs.ZLSTDAT, lstdata);
       var total = lstdata.getint(defs.ZLSTTOT);
       var cols = lstdata.getint(defs.ZLSTCLS);

       var helper = string.Empty;
       for (int i = 0; i < total; ++i) {
           var letter = 'A';
           sku = defect = qty = detailtype = string.Empty;
           for (int j = 0; j < cols; ++j, ++letter) {
               helper = string.Format("{0}{1}", letter, i);
               switch (j) {   // buscamos solamente las columnas que son necesarias
                   case 0/*1*/: lstdata.get(helper, ref sku); break; //6
                   case 1/*2*/: lstdata.get(helper, ref defect); break; //7
                   case 4/*5*/: lstdata.get(helper, ref detailtype); break; //10
                   default: continue; // las demas son irrelevantes
               }
           }

           err.require(sku.Length != 13 || defect.Length == 0 || detailtype.Length != 2, mse.WRONG_CAPTURE_SOURCE);
           bool validdef = defect[0] == '0' ? (detailtype[0] == 'F' && detailtype[1] == 'Q') :
                                      (detailtype[0] == 'R' && detailtype[1] == 'J');
           err.require(!validdef, mse.WRONG_CAPTURE_SOURCE);

           var ds = new batch_detail(batch, prod, line, partsource, new resource(sku));
           ds.location = consts.LOCCST;
           ds.cycle = 1;
           ds.detail_type = detailtype;
           ds.reason_code = int.Parse(defect);

           ds = mbl.get_batch_detail(ds);
           if (ds != null) {
               var dd = new batch_detail(batch, prod, line, partdestiny, new resource(sku));
               dd.location = consts.LOCCST;
               dd.cycle = 1;
               dd.detail_type = detailtype;
               dd.reason_code = int.Parse(defect);
               dd.qty = ds.qty;
               mbl.insert_batch_detail(dd);
               mbl.delete_batch_detail(ds);
           }
       }
   }

   protected void checkloadinfoplan() {
       var batch = values.get("cbatini");
       var prod = values.get("cprdini");
       var line = values.get("clinini");
       err.require(!bls.get_plnbl().exist_production_plan_any(
          new production_plan(batch, prod, line)), mse.PLAN_NOT_EXISTS);
   }

   protected void loadinfoplan() {
       var batch = values.get("cbatini");
       var prod = values.get("cprdini");
       var line = values.get("clinini");
       var bylens = values.getint("cbylens");

       // checamos que no haya informacion anteriormente en thdr,
       // batch_header, batch_detail, tdtml, tqchdr2, moldinv
       var obj = bls.get_manbl()._get_production_data(batch, prod, line);
       err.require(obj.getint("thdr") > 0, mse.PROD_INFO_ALREADY_EXISTS, "hdr");
       err.require(obj.getint("bhdr") > 0 || obj.getint("bdtl") > 0, mse.PROD_INFO_ALREADY_EXISTS, "batch");
       err.require(obj.getint("qhdr") > 0 || obj.getint("qdtl") > 0, mse.PROD_INFO_ALREADY_EXISTS, "qc");
       err.require(obj.getint("inv") > 0, mse.PROD_INFO_ALREADY_EXISTS, "inv");

       if (bylens == 1) bls.get_manbl()._loadinfoplanlens(batch, prod, line);
       else bls.get_manbl()._loadinfoplan(batch, prod, line);
   }

   protected void upload_plan_production() {
       int bycycles = values.getint("bycycles");
       int must = bycycles == 1 ? 6 : 5;

       var exedata = new mroJSON();
       values.get(defs.ZEXEDAT, exedata);
       int total = exedata.getint(defs.ZEXETOT);
       int cols = exedata.getint(defs.ZEXECLS);

       err.require(cols != must, mse.WRONG_FORMAT_COLS_NEEDED, string.Format("cols({0}) must({1}) type({2})", cols, must, bycycles));
       err.require(total <= 0, cme.FILE_EMPTY);

       var pln = bls.get_plnbl();

       for (int i = 1; i <= total; ++i) {
           var batch = exedata.get(string.Format("A{0}", i));
           if (string.CompareOrdinal(batch, "fin") == 0 ||
              string.CompareOrdinal(batch, "end") == 0) break;

           var prod = exedata.get(string.Format("B{0}", i));
           var line = exedata.get(string.Format("C{0}", i));
           var sku = exedata.get(string.Format("D{0}", i));
           var qty = exedata.getint(string.Format("E{0}", i));
           int qty2 = 0;

           if (bycycles == 1) qty2 = exedata.getint(string.Format("F{0}", i));
           else qty2 = qty;

           var pd = new production_plan(batch, prod, line, sku);
           err.require(pln.exist_production_plan_detail(pd), mse.SKU_ALREADY_EXIST, string.Format("{0}:{1}", sku, qty));

           pd.qty = qty;
           pd.plan_cst = qty2;
           pd.acum = 0;
           pd.band = 0;
           pln.insert_production_plan(pd);

           var k = batch + prod + line + "1";
           set_log("insert plan", k, logtype.HDR);
       }
   }

   protected void get_batch() {
       var btc = values.get(key.BATCH);
       var prd = values.get(key.PROD);
       var lin = values.get(key.LINE);
       var prt = values.get(key.PART);

       var b = new batch(btc, prd, lin, prt);
       var man = bls.get_manbl();
       b = man.get_batch(b);

       result.set(key.BATCH, b.lotno.id);
       result.set(key.PROD, b.product.id);
       result.set(key.LINE, b.line.id);
       result.set(key.PART, b.part);
       result.set(key.DATE, b.date);
       result.set(key.STATUS, b.status);
       result.set(key.COMMENTS, b.comentario);
       result.set(key.VARIATION, b.variacion);
   }

   /*protected void get_batches_like()
   {
      var btc = values.get(key.BATCH);
      var prd = values.get(key.PROD);
      var lin = values.get(key.LINE);
      var prt = values.get(key.PART);
      var sta = values.get(key.STATUS);

      var b = new batch(btc, prd, lin, prt);
      if(sta.Length > 0) b.status = int.Parse(sta);

      var man = bls.get_manbl(maindb);  
      var batches = man.get_batches_like(b);

      int listid = values.getint(defs.ZLISTAF);
      var lr = new ListResponse(listid, 7, lnk.clie);
      var row = 0;

      foreach (var o in batches)
      {
         lr.set_data(row, 'A', o.lotno.id, 'B', o.product.id, 'C', o.line.id, 'D', o.part);
         lr.set_data(row, 'E', o.status.ToString(), 'F', o.comentario, 'G', o.date.ToString(), '*', "0");
         ++row;
      }

      lr.set_rows(row);
      lr.pass_to_response(result);
   }*/

   protected void get_postcured_data() {
       var btc = values.get(key.BATCH);
       var prd = values.get(key.PROD);
       var lin = values.get(key.LINE);
       var prt = values.get(key.PART);

       var b = new postcured_data(btc, prd, lin, prt);
       var man = bls.get_manbl();
       var ovn = man.get_postcured_data(b);

       err.require(ovn == null, mse.REG_NOT_EXIST);

       result.set(key.OVEN, ovn.oven.id);
       result.set(key.OPERATOR, ovn.oper.id);
   }

   protected void save_postcured_data() {
       var btc = values.get(key.BATCH);
       var prd = values.get(key.PROD);
       var lin = values.get(key.LINE);
       var prt = values.get(key.PART);
       var ovn = values.get(key.OVEN);
       var opr = values.get(key.OPERATOR);

       var b = new postcured_data(btc, prd, lin, prt);
       b.oven = new oven(ovn);
       b.oper = new operador(opr);
       var man = bls.get_manbl();
       man.save_postcured_data(b);
   }

   protected void get_resine_data() {
       var btc = values.get(key.BATCH);
       var prd = values.get(key.PROD);
       var lin = values.get(key.LINE);
       var prt = values.get(key.PART);

       var b = new resine_data(btc, prd, lin, prt);
       var man = bls.get_manbl();
       var res = man.get_resine_data(b);

       err.require(res == null, mse.REG_NOT_EXIST);

       result.set(key.LOT_RESINE, res.lot_res);
       if (res.date_time != null) {
           DateTime f = (DateTime)res.date_time;
           result.set(key.DATE, utils.to_std_date(f));
       }
   }

   protected void save_resine_data() {
       var btc = values.get(key.BATCH);
       var prd = values.get(key.PROD);
       var lin = values.get(key.LINE);
       var prt = values.get(key.PART);
       var res = values.get(key.LOT_RESINE);
       var dat = values.get(key.DATE);

       err.require(dat.Length == 0, mse.INC_DAT_DATE);

       var b = new resine_data(btc, prd, lin, prt);
       b.lot_res = res;
       b.date_time = new DateTime();
       b.date_time = DateTime.Parse(dat);
       var man = bls.get_manbl();
       man.save_resine_data(b);
   }

   protected void get_oven_data() {
       var btc = values.get(key.BATCH);
       var prd = values.get(key.PROD);
       var lin = values.get(key.LINE);
       var prt = values.get(key.PART);

       var b = new oven_data(btc, prd, lin, prt);
       var man = bls.get_manbl();
       var ovn = man.get_oven_data(b);

       err.require(ovn == null, mse.REG_NOT_EXIST);

       result.set(key.OVEN, ovn.oven.id);
   }

   protected void save_oven_data() {
       var btc = values.get(key.BATCH);
       var prd = values.get(key.PROD);
       var lin = values.get(key.LINE);
       var prt = values.get(key.PART);
       var ovn = values.get(key.OVEN);

       var b = new oven_data(btc, prd, lin, prt);
       b.oven = new oven(ovn);
       var man = bls.get_manbl();
       man.save_oven_data(b);
   }

   protected void is_lean_line() {
       var ln = values.get(key.LINE);
       var man = bls.get_manbl();
       if (man.is_lean_line(new line(ln)))
           result.on(key.ISLEANLINE);
   }

   protected void set_rel_coat_2_wip_cast() {
       var btc = values.get(key.BATCH);
       var prd = values.get(key.PROD);
       var lin = values.get(key.LINE);
       var prt = values.get(key.PART);

       var man = bls.get_manbl();

       err.require(!man.is_lean_line(new line(lin)), mse.MUST_BE_LEAN_LINE);

       var bat = new batch(btc, prd, lin, prt);
       bat = man.get_batch(bat);
       err.require(bat == null, mse.BATCH_NOT_EXIST);
       err.require(bat.status != batchstatus.RELCOT, mse.BATCH_MUSTBE_RELCOT);

       var bath = new batch_header(bat, consts.LOCCST, 1);
       bath = man.get_batch_header(bath);
       err.require(bath == null, mse.LOC_NOT_EXIST);
       bath.status = status.WIP;
       man.update_batch_header(bath);

       bath = new batch_header(bat, consts.LOCCOT, 1);
       bath = man.get_batch_header(bath);
       err.require(bath == null, mse.LOC_NOT_EXIST);
       bath.status = status.WIP;
       man.update_batch_header(bath);

       bat.status = batchstatus.WIPCST;
       man.update_batch(bat);
   }

   protected void delete_coat_batch() {
       var btc = values.get(key.BATCH);
       var prd = values.get(key.PROD);
       var lin = values.get(key.LINE);
       var prt = values.get(key.PART);

       var man = bls.get_manbl();
       man.delete_coat_batch(new batch(btc, prd, lin, prt));
   }

   protected void create_casting_batch() {
       var btc = values.get(key.BATCH);
       var prd = values.get(key.PROD);
       var lin = values.get(key.LINE);
       var prt = "1";

       var man = bls.get_manbl();
       man.create_casting_batch(new batch(btc, prd, lin, prt));
   }

   protected void delete_casting_batch() {
       var btc = values.get(key.BATCH);
       var prd = values.get(key.PROD);
       var lin = values.get(key.LINE);
       var prt = "1";

       var man = bls.get_manbl();
       man.delete_casting_batch(new batch(btc, prd, lin, prt));
   }

   protected void create_batches_as_plan() {
       var btc = values.get(key.BATCH);
       var lin = values.get(key.LINE);

       var man = bls.get_manbl();
       var pln = bls.get_plnbl();

       lot lt = new lot(btc);
       line l = new line(lin);

       var products = pln.get_products_planned_by_line(lt, l);
       foreach (var prod in products) {
           var b = new batch(lt, prod, l, "1");
           if (man.exist_batch(b) == false) {
               man.create_casting_batch(b);
               set_log(logacts.CREATE_BATCH, b.getfullbatch(), logtype.HDR);
           }
       }
   }
</script>

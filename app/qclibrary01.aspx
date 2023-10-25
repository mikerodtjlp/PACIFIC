<%@ Import Namespace="sfc" %>
<%@ Import Namespace="sfc.BL" %>
<%@ Import Namespace="sfc.BO" %>

<!-- #include file="~/core.aspx" -->
<script runat="server" language="C#">

   protected void qc_inspection_update_comments() {
       var lotid = values.get(key.BATCH);
       var lineid = values.get(key.LINE);
       var blockid = values.get(key.BLOCK);
       var noinsp = values.getint(key.NOINSP);
       var locid = values.get(key.LOCATION);
       var comments = values.get(key.COMMENTS);

       if (locid.Length == 0) locid = locs.QCT;

       var block = new qc_block(blockid);
       var loc = new location(locid);
       var bat = new batch(lotid, lineid);

       bls.get_qcbl().
           update_qc_inspection_comments(bat, block, noinsp, loc, comments);
   }

   protected void force_holded_batch_2_release_qc() {
       var btc = values.get("$batch$");
       var prd = values.get("$_prod$");
       var lin = values.get("$_line$");
       var prt = values.get("$_part$");
       bls.get_manbl().
           force_holded_batch_2_release_qc(new batch(btc, prd, lin, prt));
   }

   protected void get_qc_inspection_detail() {
       var lotid = values.get(key.BATCH);
       var lineid = values.get(key.LINE);
       var blockid = values.get(key.BLOCK);
       var noinsp = values.getint(key.NOINSP, 1);
       var locid = values.get(key.LOCATION);

       if (locid.Length == 0) locid = locs.QCT;

       var block = new qc_block(blockid);
       var loc = new location(locid);
       var bat = new batch(lotid, lineid);

       var qc = bls.get_qcbl();
       var defects = qc.get_qc_inspection_detail(bat, block, noinsp, loc);

       int listid = values.getint(defs.ZLISTAF);
       var lr = new ListResponse(listid, 7, lnk.clie);
       var row = 0;

       foreach (var o in defects) {
           lr.set_data(row, 'A', o.sku.id, 'B', o.def.id,
                            'C', o.def.description, 'D', o.zone_.zone_);
           lr.set_data(row, 'E', o.def.category, 'F', o.qty.ToString(),
                            'G', o.def.type_desc, '*', "1");
           ++row;
       }

       lr.set_rows(row);
       lr.pass_to_obj(result);
   }

   protected void get_qc_inspection_header() {
       var lotid = values.get(key.BATCH);
       var lineid = values.get(key.LINE);
       var blockid = values.get(key.BLOCK);
       var noinsp = values.getint(key.NOINSP, 1);
       var locid = values.get(key.LOCATION);

       if (locid.Length == 0) locid = locs.QCT;

       var block = new qc_block(blockid);
       var loc = new location(locid);
       var bat = new batch(lotid, lineid);

       var qc = bls.get_qcbl();
       var insp = qc.get_qc_inspection(bat, block, noinsp, loc);
       err.require(insp == null, mse.INSP_NOT_EXIST);

       result.set(key.BATCH, insp.lot.lotno.id);
       result.set(key.LINE, insp.lot.line.id);
       result.set(key.BLOCK, insp.block.id);
       result.set(key.NOINSP, insp.noinsp);
       result.set(key.LOCATION, insp.location.id);
       result.set(key.STATUS, insp.status);
       result.set(key.PART, insp.part);
       result.set("creation_date", utils.to_std_date(insp.creation_date));
       result.set("finish_date", insp.finish_date == null ? "" :
                               utils.to_std_date(insp.finish_date));
       result.set(key.TOTAL, insp.total.ToString());
       result.set(key.SAMPLE, insp.sample.ToString());
       result.set(key.OPERATOR, insp.oper.id);
       result.set(key.DISPOSITION, insp.disposition);
       result.set("res_ctr", insp.res_ctr);
       result.set("res_mln", insp.res_mln);
       result.set("res_mem", insp.res_mln);
       result.set("sta_ctr", insp.sta_ctr);
       result.set("sta_mln", insp.sta_mln);
       result.set("sta_mem", insp.sta_mem);
       result.set("needinspection", insp.inspected);
       result.set(key.COMMENTS, insp.comments);
       result.set(key.AQL, insp.aql.type);
       result.set(key.PART, insp.part);
       result.set("rejreason", insp.reason_code.id);
   }

   protected void create_qc_pkg_inspection() {
       var operid = values.get(key.OPERATOR);
       var lotid = values.get(key.BATCH);
       var lineid = values.get(key.LINE);
       var blockid = values.get(key.BLOCK);
       var noinsp = values.getint(key.NOINSP);
       var locid = values.get(key.LOCATION);

       var block = new qc_block(blockid);
       var loc = new location(locid);
       var oper = new operador(operid);
       var bat = new batch(lotid, lineid);

       bls.get_qcbl().
           create_qc_pkg_inspection(bat, block, noinsp, loc, oper);

   }
   protected void release_qc_pkg_inspection() {
       var lotid = values.get(key.BATCH);
       var lineid = values.get(key.LINE);
       var prt = values.get(key.PART);
       var blockid = values.get(key.BLOCK);
       var noinsp = values.getint(key.NOINSP);
       var locid = values.get(key.LOCATION);

       var block = new qc_block(blockid);
       var loc = new location(locid);
       var bat = new batch(lotid, lineid);

       bls.get_qcbl().
           release_qc_pkg_inspection(bat, block, noinsp, loc);
   }

   protected void automatic_create_qcinsp() {
       var operid = values.get(key.OPERATOR);
       var lotid = values.get(key.BATCH);
       var lineid = values.get(key.LINE);
       var part = values.get(key.PART);
       var partial = values.getint(key.BLOCKSZ, -1);
       var noinsp = values.getint(key.NOINSP, -1);
       var istrans = values.getbool("istrans");
       var size = istrans ? 114 : 120;

       err.require(lotid.Length == 0, mse.INC_DAT_BATCH);
       err.require(lineid.Length == 0, mse.INC_DAT_LINE);
       err.require(part.Length == 0, mse.INC_DAT_PART);
       err.require(noinsp == -1, mse.INC_DAT_NOINSP);
       err.require(operid.Length == 0, mse.INC_DAT_OPER);
       err.require(partial != -1 && partial > size, mse.QCBLOCK_SIZE_GREATER_LIMIT);

       var llt = new lot(lotid);
       var lin = new line(lineid);

       var man = bls.get_manbl();
       var qc = bls.get_qcbl();

       // our last trolley generated
       var last = man.trolley_get_last(llt, lin);
       var curblock = new qc_block(last.ToString());
       var nueblock = new qc_block((last + 1).ToString());
       var newblock = new trolley((last + 1).ToString());
       var bt = new batch(lotid, "XXX", lineid, part);

       var pt = qc.qc_all_prods_have_tests(bt);
       if (pt != null) {
           var er = string.Concat("batch:", pt.lotno.id, ", product:", pt.product.id,
                                   ", line:", pt.line.id, ", part:", pt.part);
           err.require(true, mse.QC_TEST_NOT_COMPLETED, er);
       }

       // if current insp is in WIP no cannot create a new one
       if (last > 0) {
           //var insp = qc.get_qc_inspection(bt, curblock, noinsp, new location(locs.QCT));
           //err.require(insp == null, mse.INSP_NOT_EXIST);
           //err.require(insp.status == qcinspstatus.WIP, mse.QCBLOCK_LAST_MUSTBE_REL, curblock.id);
       }
       if (noinsp > 1) // if we have a inspection > 0 we check the previous of course
       {
           //var lastinsp = qc.get_qc_inspection(bt, nueblock, noinsp -1, new location(locs.QCT));
           //err.require(lastinsp == null, mse.WRONG_NO_INSPECTION);
           //err.require(lastinsp.noinsp != (noinsp - 1), mse.WRONG_NO_INSPECTION);
           //err.require(lastinsp.status != qcdisp.REL, mse.QCBLOCK_LAST_NOT_RELEASED);
       }

       if (noinsp == 1) // only inspection 1 create the trolley
       {
           var data = man.get_batch_product_base(llt, lin, part, new location(locs.CST));
           int t = data.Count;
           if (t > 20) t /= 20; // make shorter the posibilities
           err.require(t == 0, mse.SKU_NOT_PRODUCED);

           int a = 0;
           int b = 0;
           if (partial == -1) // fullbox
           {
               a = size / t;
               b = size % t;
           }
           else // pedacera
           {
               if (partial <= t) {
                   t = partial;
                   a = 1;
                   b = 0;
               }
               else {
                   a = partial / t;
                   b = partial % t;
               }
           }

           var pcd = new product_bulk();
           int row = 0;
           for (int i = 0; i < t; ++i) {
               var d = data[i];
               pcd.prod = new product(d.prod.id);
               pcd.baseno = new basenum(d.baseno.graduation);
               if (row == t - 1) a += b;
               pcd.qty = a;
               if (pcd.qty > 0)
                   man.insert_trolley_batch_relation(llt, lin, part, newblock, noinsp, pcd);
               ++row;
           }
       }

       values.set(key.BLOCK, newblock.id);

       create_qc_block_inspection();

       lnk.clie.newval.set("$qcblock$", newblock.id);
   }
   protected void create_qcinsp_autoblockfull() {
       var noinsp = values.getint(key.NOINSP, -1);
       err.require(noinsp == -1, mse.INC_DAT_NOINSP);
       err.require(noinsp != 1, mse.ONLY_FOR_INSPECTION_1);
       automatic_create_qcinsp();
   }
   protected void create_qcinsp_autoblockpartial() {
       var noinsp = values.getint(key.NOINSP, -1);
       err.require(noinsp == -1, mse.INC_DAT_NOINSP);
       err.require(noinsp != 1, mse.ONLY_FOR_INSPECTION_1);
       err.require(!values.has_val(key.BLOCKSZ), mse.QCBLOCK_SIZE_EMPTY);
       automatic_create_qcinsp();
   }
   protected void create_qc_inspection() {
       var noinsp = values.getint(key.NOINSP, -1);
       err.require(noinsp == -1, mse.INC_DAT_NOINSP);
       err.require(noinsp == 1, mse.QC_ONLY_FOR_INSP_2_HIGHER);
       create_qc_block_inspection();
   }
   protected void create_qc_block_inspection() {
       var operid = values.get(key.OPERATOR);
       var lotid = values.get(key.BATCH);
       var lineid = values.get(key.LINE);
       var part = values.get(key.PART);
       var blockid = values.get(key.BLOCK);
       var noinsp = values.getint(key.NOINSP);
       var locid = values.get(key.LOCATION);

       if (locid.Length == 0) locid = locs.QCT;

       var lin = new line(lineid);

       err.require(operid.Length == 0, mse.INC_DAT_OPER);
       err.require(!bls.get_manbl().is_lean_line(lin),
                   mse.MUST_BE_LEAN_LINE);

       var block = new qc_block(blockid);
       var loc = new location(locid);
       var oper = new operador(operid);
       var bat = new batch(lotid, "XXX", lineid, part);

       bls.get_qcbl().
           create_qc_inspection(bat, block, noinsp, loc, oper);

       // save into the log this important step
       //var s = bls.get_qcbl(maindb).get_qc_inspection_prod_disp
       //             (bat, block, noinsp, loc);
       //foreach (var o in s)
       //{
       //    set_log("create_qc_inspection",
       //        string.Concat("blk:", blockid),
       //        string.Concat(lotid, o.Item1, lineid, part), "H");
       //}
   }

   protected void set_wip_qc_inspection() {
       var operid = values.get(key.OPERATOR);
       var lotid = values.get(key.BATCH);
       var lineid = values.get(key.LINE);
       var part = values.get(key.PART);
       var blockid = values.get(key.BLOCK);
       var noinsp = values.getint(key.NOINSP);
       var locid = values.get(key.LOCATION);

       if (locid.Length == 0) locid = locs.QCT;

       err.require(lotid.Length == 0, mse.INC_DAT_BATCH);
       err.require(lineid.Length == 0, mse.INC_DAT_LINE);
       err.require(blockid.Length == 0, mse.INC_DAT_BLOCK);
       err.require(noinsp == -1, mse.INC_DAT_NOINSP);

       var block = new qc_block(blockid);
       var loc = new location(locid);
       var bat = new batch(lotid, "XXX", lineid, part);
       var lin = new line(lineid);

       bls.get_qcbl().
           set_wip_qc_inspection(bat, block, noinsp, loc);

       // save into the log this important step
       //var s = bls.get_qcbl(maindb).get_qc_inspection_prod_disp
       //             (bat, block, noinsp, loc);
       //foreach (var o in s)
       //{
       //    set_log("set_wip_qc_inspection",
       //        string.Concat("blk:", blockid),
       //        string.Concat(lotid, o.Item1, lineid, part), "H");
       //}      
   }

   protected void release_qc_inspection() {
       var lotid = values.get(key.BATCH);
       var lineid = values.get(key.LINE);
       var part = values.get(key.PART);
       var blockid = values.get(key.BLOCK);
       var noinsp = values.getint(key.NOINSP);
       var locid = values.get(key.LOCATION);
       var defect = values.get(key.DEFECT);

       if (locid.Length == 0) locid = locs.QCT;

       err.require(lotid.Length == 0, mse.INC_DAT_BATCH);
       err.require(lineid.Length == 0, mse.INC_DAT_LINE);
       err.require(blockid.Length == 0, mse.INC_DAT_BLOCK);
       err.require(noinsp == -1, mse.INC_DAT_NOINSP);

       var block = new qc_block(blockid);
       var loc = new location(locid);
       var bat = new batch(lotid, "XXX", lineid, part);
       var def = new defect(defect);

       bls.get_qcbl().
           release_qc_inspection(bat, block, noinsp, loc, def);

       // save into the log this important step    
       //var s = bls.get_qcbl(maindb).get_qc_inspection_prod_disp
       //             (bat, block, noinsp, loc);
       //foreach (var o in s)
       //{
       //    bool rel = string.CompareOrdinal(o.Item2, "REL") == 0;
       //    set_log(rel ? "release_qc_inspection" : 
       //                  "reject_qc_inspection", 
       //                  string.Concat("blk:",blockid),
       //        string.Concat(lotid, o.Item1, lineid, part), "H");
       //}       
   }

   protected void pass_qc_blocks_into_batch() {
       var lotid = values.get(key.BATCH);
       var lineid = values.get(key.LINE);
       var part = values.get(key.PART);
       var blockid = values.get(key.BLOCK);
       //var noinsp = values.getint(key.NOINSP);

       bls.get_qcbl().
           pass_qc_blocks_into_batch(new lot(lotid),
                                       new line(lineid),
                                       part,
                                       new trolley(blockid), //noinsp, 
                                       consts.LOCQCT);
   }

   protected void get_batch_qc_tests_and_batch_data() {
       get_batch_qc_tests();

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
       result.set(key.DATE, b.date.ToString());
       result.set(key.STATUS, b.status.ToString());
       result.set(key.COMMENTS, b.comentario);
       result.set(key.VARIATION, b.variacion.ToString());
   }

   protected void get_batch_qc_tests() {
       var btc = values.get(key.BATCH);
       var prd = values.get(key.PROD);
       var lin = values.get(key.LINE);
       var prt = values.get(key.PART);

       var b = new batch(btc, prd, lin, prt);
       var qc = bls.get_qcbl();
       var tests = qc.get_batches_qc_tests(b);

       int listid = values.getint(defs.ZLISTAF);
       var lr = new ListResponse(listid, 7, lnk.clie);
       var row = 0;

       foreach (var o in tests) {
           lr.set_data(row, 'A', o.id.id, 'B', o.id.description,
                            'C', o.status, '*', o.image);
           ++row;
       }

       lr.set_rows(row);
       lr.pass_to_obj(result);
   }

   protected void create_liberar_gen_capture() {
       var btc = values.get(key.BATCH);
       var prd = values.get(key.PROD);
       var lin = values.get(key.LINE);
       var prt = values.get(key.PART);

       var b = new batch(btc, prd, lin, prt);
       var qc = bls.get_qcbl();

       qc.create_liberar_gen_capture(b, values);
   }
   protected void create_liberar_gen_capture_all() {
       var btc = values.get(key.BATCH);
       var prd = values.get(key.PROD);
       var lin = values.get(key.LINE);
       var prt = values.get(key.PART);

       var b = new batch(btc, prd, lin, prt);
       var qc = bls.get_qcbl();

       qc.create_liberar_gen_capture_all(b);
   }

   protected void update_qc_inspection_info() {
       var lotid = values.get(key.BATCH);
       var lineid = values.get(key.LINE);
       var part = values.get(key.PART);
       var blockid = values.get(key.BLOCK);
       var noinsp = values.getint(key.NOINSP);
       var locid = values.get(key.LOCATION);

       if (locid.Length == 0) locid = locs.QCT;

       var block = new qc_block(blockid);
       var loc = new location(locid);
       var bat = new batch(lotid, "XXX", lineid, part);

       var qcbl = bls.get_qcbl();
       var insp = qcbl.get_qc_inspection(new batch(lotid, "XXX", lineid, part),
                                           new qc_block(blockid), noinsp,
                                           new location(locid));
       err.require(insp == null, mse.INSP_NOT_EXIST);
       err.require(insp.status != qcinspstatus.WIP, mse.CAP_MUSTBE_WIP);

       qcbl.update_qc_inspection_info(bat, block, noinsp, loc, 0);
   }

   protected void get_qc_inspection_sample() {
       var lotid = values.get(key.BATCH);
       var lineid = values.get(key.LINE);
       var blockid = values.get(key.BLOCK);
       var noinsp = values.getint(key.NOINSP, 1);
       var locid = values.get(key.LOCATION);

       if (locid.Length == 0) locid = locs.QCT;

       var block = new qc_block(blockid);
       var loc = new location(locid);
       var bat = new batch(lotid, lineid);

       var qc = bls.get_qcbl();
       var sample = qc.get_qc_inspection_sample(bat, block, noinsp, loc);

       int listid = values.getint(defs.ZLISTAF);
       var lr = new ListResponse(listid, 7, lnk.clie);
       var row = 0;
       int samplecount = 0;

       foreach (var o in sample) {
           lr.set_data(row, 'A', o._sku.id, 'B', o.qty.ToString(), '*', "1");
           ++row;
           samplecount += o.qty;
       }

       result.set(key.SAMPLECOUNT, samplecount);

       lr.set_rows(row);
       lr.pass_to_obj(result);
   }

   /*protected void release_batch_from_qc()
   {
       var lotid = values.get(key.BATCH);
       var prod = values.get(key.PROD);
       var lineid = values.get(key.LINE);
       var part = values.get(key.PART);

       var locid = values.get(key.LOCATION);
       var depid = values.get("dept");

       if (locid.Length == 0) locid = locs.QCT;
       if (depid.Length == 0) depid = "6";

       var b = new batch(lotid, prod, lineid, part);
       var qc = bls.get_qcbl(maindb);//new qc_BL(maindb);

       qc.release_batch_from_qc(b, new location(locid));
   }*/

   protected void insert_qc_pkg_defect() {
       var lotid = values.get(key.BATCH);
       var lineid = values.get(key.LINE);
       var part = values.get(key.PART);
       var blockid = values.get(key.BLOCK);
       var noinsp = values.getint(key.NOINSP);
       var locid = values.get(key.LOCATION);

       var code = values.get(key.SKU);
       var defectid = values.get(key.DEFECT);
       var zon = values.get(key.ZONE);
       var qty = values.get(key.QTY);

       var sku = code;

       if (code.Length == 10) // seems barcode
       {
           var md = bls.get_manbl();
           var tofind = new resource();
           tofind.opc_bar_code = code;
           var res = md.get_resource_by_barcode(tofind);
           sku = res.id;
       }

       err.require(sku.Length != 13, mse.INC_DAT_SKU);
       values.set(key.SKU, sku);

       var prd = sku.Substring(0, 3);
       var man = bls.get_manbl();
       var hdr = man.get_batch(new batch(lotid, prd, lineid, part));
       err.require(hdr.status == batchstatus.RELEASE, mse.BATCH_MUSTBE_WIP);

       insert_inspection_defect();
   }

   protected void insert_inspection_defect() {
       var lotid = values.get(key.BATCH);
       var lineid = values.get(key.LINE);
       var part = values.get(key.PART);
       var blockid = values.get(key.BLOCK);
       var noinsp = values.getint(key.NOINSP);
       var locid = values.get(key.LOCATION);

       var skuid = values.get(key.SKU);
       var defectid = values.get(key.DEFECT);
       var zon = values.get(key.ZONE);
       var qty = values.get(key.QTY);

       int dummy = 0;
       err.require(defectid.IndexOf(' ') != -1, mse.WRONG_DEFECT); // has garbage?
       var defisnum = int.TryParse(defectid, out dummy);
       err.require(!defisnum, mse.WRONG_DEFECT); // is really a number?
       var qcbl = bls.get_qcbl();
       var deff = qcbl.get_defect_by_location(consts.LOCQCT, new defect(defectid));
       err.require(deff == null, mse.DEFECT_NOT_BELONG_TO_LOC); // belogns to QC defects?

       if (locid.Length == 0) locid = locs.QCT;

       err.require(qty.Length == 0, mse.INC_DAT_QTY);
       var q = int.Parse(qty);
       err.require(q < 0, mse.INVALID_QTY);
       err.require(q > 120, mse.QTY_TOO_HIGH);

       if (skuid.Length == 10) // seems barcode
       {
           var md = bls.get_manbl();
           var tofind = new resource();
           tofind.opc_bar_code = skuid;
           var res = md.get_resource_by_barcode(tofind);
           skuid = res.id;
       }
       err.require(skuid.Length != 13, mse.SKU_WRONG_FORMAT);
       err.require(skuid[7] != '.', mse.SKU_WRONG_FORMAT);

       var blk = new qc_block(blockid);
       var loc = new location(locid);
       var bat = new batch(lotid, lineid);
       var sku = new resource(skuid);
       var def = new defect(defectid);
       var zne = new zone(zon);

       //* URI01 06/15/2012 -Revisa si el SKU de rechazo a insertar existe en el trolley
       var mbl = bls.get_manbl();
       product sku2 = new product { id = skuid };

       // is a change, there is a twin 
       string prodtwin = mbl.get_twin(sku2);

       //var trlprds = bls.get_manbl(maindb).
       //	get_full_trolley_batch_relation(new lot(lotid),
       //                                    new line(lineid),
       //									new trolley(blockid), noinsp);
       //int founds = 0;
       //foreach (var prodbase in trlprds)
       //{
       //    founds += prodbase.prod.id == sku.id.Substring(0, 3) ? 1 : 0;
       //    founds += prodbase.prod.id == prodtwin ? 1 : 0;
       //}
       //err.require(founds == 0, mse.SKU_NOT_EXIST_ON_BLOCK);

       var prd = sku.id.Substring(0, 3);
       var b2c = new batch(lotid, lineid);
       b2c.product = new product(prd);
       var exist_cst = bls.get_cstbl().exist_sku_produced(b2c, sku);
       err.require(!exist_cst, mse.SKU_NOT_PRODUCED);
       //var ds = new batch_detail(lotid, sku.id.Substring(0,3), lineid, "1", sku);
       //ds.location = consts.LOCCST;
       //ds.cycle = 1;
       //ds.detail_type = "FQ";
       //ds.reason_code = 0;
       //ds = bls.get_manbl(maindb).get_batch_detail(ds);
       //err.require(ds == null, mse.SKU_NOT_PRODUCED);

       //*  

       var fullbat = new batch(lotid, prd, lineid, part);

       // check capture no more rejects than a block contains
       int act_defs = 0;
       var dtl = /*bls.get_qcbl(maindb)*/qcbl.get_qc_inspection_detail_allX(fullbat, loc);
       foreach (var d in dtl) { act_defs += d.qty; }
       err.require((act_defs + q) > 120, mse.QTY_TOO_HIGH);


       var man = bls.get_manbl();
       var hdr = man.get_batch(fullbat);
       err.require(hdr.status == batchstatus.RELEASE, mse.BATCH_MUSTBE_WIP);

       bls.get_qcbl().
           insert_inspection_defect(bat, blk, noinsp, loc, sku,
                                   def, zne, q);
   }

   protected void update_inspection_defect() {
       var lotid = values.get(key.BATCH);
       var lineid = values.get(key.LINE);
       var part = values.get(key.PART);
       var blockid = values.get(key.BLOCK);
       var noinsp = values.getint(key.NOINSP);
       var locid = values.get(key.LOCATION);

       var skuid = values.get(key.SKU);
       var defectid = values.get(key.DEFECT);
       var zon = values.get(key.ZONE);
       var qty = values.get(key.QTY);

       if (locid.Length == 0) locid = locs.QCT;

       err.require(qty.Length == 0, mse.INC_DAT_QTY);
       var q = int.Parse(qty);
       err.require(q < 0, mse.INVALID_QTY);
       err.require(q > 120, mse.QTY_TOO_HIGH);

       var blk = new qc_block(blockid);
       var loc = new location(locid);
       var bat = new batch(lotid, lineid);
       var sku = new resource(skuid);
       var def = new defect(defectid);
       var zne = new zone(zon);

       var prd = skuid.Substring(0, 3);

       var fullbat = new batch(lotid, prd, lineid, part);

       // check capture no more rejects than a block contains
       int act_defs = 0;
       var dtl = bls.get_qcbl().get_qc_inspection_detail_allX(fullbat, loc);
       foreach (var d in dtl) { act_defs += d.qty; }
       err.require((act_defs + q) > 120, mse.QTY_TOO_HIGH);

       var man = bls.get_manbl();
       var hdr = man.get_batch(fullbat);
       err.require(hdr.status == batchstatus.RELEASE, mse.BATCH_MUSTBE_WIP);

       bls.get_qcbl().
           update_inspection_defect(bat, blk, noinsp, loc, sku,
                                   def, zne, q);
   }

   protected void delete_inspection_defect() {
       var lotid = values.get(key.BATCH);
       var lineid = values.get(key.LINE);
       var part = values.get(key.PART);
       var blockid = values.get(key.BLOCK);
       var noinsp = values.getint(key.NOINSP);
       var locid = values.get(key.LOCATION);

       var skuid = values.get(key.SKU);
       var defectid = values.get(key.DEFECT);
       var zon = values.get(key.ZONE);

       if (locid.Length == 0) locid = locs.QCT;

       var blk = new qc_block(blockid);
       var loc = new location(locid);
       var bat = new batch(lotid, lineid);
       var sku = new resource(skuid);
       var def = new defect(defectid);
       var zne = new zone(zon);

       var prd = skuid.Substring(0, 3);
       var man = bls.get_manbl();
       var hdr = man.get_batch(new batch(lotid, prd, lineid, part));
       err.require(hdr.status == batchstatus.RELEASE, mse.BATCH_MUSTBE_WIP);

       bls.get_qcbl().
           delete_inspection_defect(bat, blk, noinsp, loc, sku, def, zne);
   }

   protected void insert_inspection_sample() {
       var lotid = values.get(key.BATCH);
       var lineid = values.get(key.LINE);
       var part = values.get(key.PART);
       var blockid = values.get(key.BLOCK);
       var noinsp = values.getint(key.NOINSP);
       var locid = values.get(key.LOCATION);
       var skuid = values.get(key.SKU);
       var qty = values.get(key.QTY);

       if (locid.Length == 0) locid = locs.QCT;

       err.require(qty.Length == 0, mse.INC_DAT_QTY);
       err.require(skuid.Length == 0, mse.INC_DAT_SKU);

       //URI05 Si es barcode busca el sku

       if (skuid.Length == 10) // seems barcode
       {
           var md = bls.get_manbl();
           var tofind = new resource();
           tofind.opc_bar_code = skuid;
           var res = md.get_resource_by_barcode(tofind);
           skuid = res.id;
       }

       //URI05

       err.require(skuid.Length != 13, mse.SKU_WRONG_FORMAT);
       err.require(skuid[7] != '.', mse.SKU_WRONG_FORMAT);

       var blk = new qc_block(blockid);
       var loc = new location(locid);
       var bat = new batch(lotid, lineid);
       var sku = new resource(skuid);

       var prd = skuid.Substring(0, 3);
       var man = bls.get_manbl();
       var hdr = man.get_batch(new batch(lotid, prd, lineid, part));
       err.require(hdr.status == batchstatus.RELEASE, mse.BATCH_MUSTBE_WIP);

       var qc = bls.get_qcbl();

       if (!qc.is_enough_inspection_sample(bat, blk, noinsp, loc)) {
           // check if the sku to be inserted really exist on the trolley sended by manufacture
           //var trlprds = bls.get_manbl(maindb).
           //	get_full_trolley_batch_relation(new lot(lotid), 
           //                                                                new line(lineid), 
           //									new trolley(blockid), noinsp);
           //int founds = 0;
           //foreach (var prodbase in trlprds)
           //{
           //    founds += prodbase.prod.id == sku.id.Substring(0, 3) ? 1 : 0;
           //}
           //err.require(founds == 0, mse.SKU_NOT_EXIST_ON_BLOCK);

           //var prd = sku.id.Substring(0, 3);
           var b2c = new batch(lotid, lineid);
           b2c.product = new product(prd);
           var exist_cst = bls.get_cstbl().exist_sku_produced(b2c, sku);
           err.require(!exist_cst, mse.SKU_NOT_PRODUCED);
           //var ds = new batch_detail(lotid, sku.id.Substring(0, 3), lineid, "1", sku);
           //ds.location = consts.LOCCST;
           //ds.cycle = 1;
           //ds.detail_type = "FQ";
           //ds.reason_code = 0;
           //ds = bls.get_manbl(maindb).get_batch_detail(ds);
           //err.require(ds == null, mse.SKU_NOT_PRODUCED);

           qc.insert_inspection_sample(bat, blk, noinsp, loc, sku, int.Parse(qty));

           var sample = qc.get_qc_inspection_sample(bat, blk, noinsp, loc);

           var samplecount = 0;
           foreach (var s in sample)
               samplecount += s.qty;

           result.off("stopsample");
           result.set(key.SAMPLECOUNT, samplecount);
       }
       else {
           result.on("stopsample");

           var desc = new mroJSON("$errdesc$", "enough sample");
           var res = new mroJSON(new string[] {
                                    "matchcode", "S016",
                                    "send", desc.get_json(),
                                    "dlgtype", "1",
                                    "isdialog", "1"
                                });
           /*var res = new mroJSON("matchcode", "S016");
           res.set("ctrls2match", desc.get_json());
           res.set("dlgtype", "1");
           res.on("isdialog";*/

           result.set(defs.ZGOTOBC, res);
       }
       result.set("samplesku", "");
   }

</script>

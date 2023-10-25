<%@ Import Namespace="sfc" %>
<%@ Import Namespace="sfc.BL" %>
<%@ Import Namespace="sfc.BO" %>

<!-- #include file="~/core.aspx" -->
<script runat="server" language="C#">

   protected void consult_moldloss() {
       var mbl = bls.get_manbl();
       var b = mbl.get_batch(mbl.EXTRACT_BATCH_FROM_PARAMS(values));

       var bhdr = mbl.get_batch_header(b, consts.LOCMOL, 1);
       err.require(bhdr == null, mse.MOLDLOSS_NOT_EXISTS);

       result.set("$fcrea$", string.Concat("fecha creacion : ", utils.to_std_date(bhdr.date_time)));
       result.set("_$loc_$", string.Concat("location : ", locs.MOL));
       result.set("_$lsta$", string.Concat("status loc : ", bhdr.status));
       result.set("_$stal$", bhdr.status);
       result.set("$commsb$", bhdr.comments);
       result.set("$boxes$", bhdr.boxes);
       result.set("$as400$", bhdr.as_400);
       newval.set("$cycle$", bhdr.cycle);

       // is a change, there is a twin 
       string prodtocheck = mbl.get_twin(b.product);
       if (prodtocheck.Length == 0) prodtocheck = b.product.id;
       result.set("cprdini", prodtocheck);

       mbl.get_batch_basic_data(basics, values, result);
   }

   protected void create_moldloss() {
       var mbl = bls.get_manbl();
       var b = mbl.get_batch(mbl.EXTRACT_BATCH_FROM_PARAMS(values));

       // is a change, there is a twin 
       string prodtocheck = mbl.get_twin(b.product);
       if (prodtocheck.Length == 0) prodtocheck = b.product.id;

       // + checamos que el lote este en wip casting solamente 
       // note: we dont check thdr(especially in status=0) because is not guarrantee that
       // thdr.status = 0 means wip-casting(due the reinspection put the status in 0 too) 
       // so the only way to know is to check it's capture
       var bs = mbl.get_batch_header(b, consts.LOCCST, 1);
       err.require(bs == null, mse.BATCH_NOT_EXIST);
       err.require(!bs.isinwip(), mse.BATCH_MUSTBE_WIPCST, bs.status);

       // preparamos los parametros para crear la locacion
       var hdr = mbl.get_batch_header(b, consts.LOCMOL, 1);
       err.require(hdr != null, mse.MOLDLOSS_ALREADY_EXISTS);

       hdr = new batch_header(b, consts.LOCMOL, 1);
       hdr.as_400 = "0";
       hdr.boxes = 0;
       hdr.comments = "modloss";
       hdr.creation_type = 0;
       hdr.date_time = DateTime.Now;
       hdr.finishontime = "0";
       hdr.status = status.WIP;
       mbl.insert_batch_header(hdr);
   }

   protected void liberar_moldloss() {
       var mbl = bls.get_manbl();
       var b = mbl.get_batch(mbl.EXTRACT_BATCH_FROM_PARAMS(values));

       // is a change, there is a twin 
       string prodtocheck = mbl.get_twin(b.product);
       if (prodtocheck.Length == 0) prodtocheck = b.product.id;

       // + checamos que el lote este en wip casting solamente 
       // note: we dont check thdr(especially in status=0) because is not guarrantee that
       // thdr.status = 0 means wip-casting(due the reinspection put the status in 0 too) 
       // so the only way to know is to check it's capture
       var bs = mbl.get_batch_header(b, consts.LOCCST, 1);
       err.require(bs == null, mse.BATCH_NOT_EXIST);
       err.require(!bs.isinwip(), mse.BATCH_MUSTBE_WIPCST);

       // preparamos los parametros para crear la locacion
       var hdr = mbl.get_batch_header(b, consts.LOCMOL, 1);
       err.require(hdr == null, mse.MOLDLOSS_NOT_EXISTS);
       err.require(!hdr.isinwip(), mse.CAP_MUSTBE_WIP);

       // + we put as authorized all the molds that were left as temporary
       mbl.update_all_moldloss(b, 0);

       // we mark it as a release
       hdr.status = status.REL;
       mbl.update_batch_header(hdr);

       // + we check that there is something to upload
       // note: if theare is nothing left to upload, it is pointless to add overhead to the upload process
       // for something that will not generate any mold to the sap/r3, so if has nothing to upload: we put as400 to 1
       int c = mbl.how_many_pending_moldloss(b);
       if (c == 0) // there is nothing to upload, so we put as400 = 1
       {
           hdr.as_400 = "1";
           mbl.update_batch_header(hdr);
       }
   }

   protected void authorize_moldloss() {
       var mbl = bls.get_manbl();
       var b = mbl.get_batch(mbl.EXTRACT_BATCH_FROM_PARAMS(values));

       // - checamos que la captura este en wip
       var bhdr = mbl.get_batch_header(b, consts.LOCMOL, 1);
       err.require(bhdr == null, mse.MOLDLOSS_NOT_EXISTS);

       var sta = bhdr.status.TrimEnd();
       err.require(sta != status.WIP, mse.CAP_MUSTBE_WIP);

       var lstdata = new mroJSON();
       values.get(defs.ZLSTDAT, lstdata);
       var total = lstdata.getint(defs.ZLSTTOT);
       var cols = lstdata.getint(defs.ZLSTCLS);
       var helper = string.Empty;

       for (var i = 0; i < total && i < 1024; ++i) // 1024 safety break;
       {
           var letter = 'A';
           var moldid = string.Empty;
           for (var j = 0; j < cols; ++j, ++letter) {
               helper = string.Format("{0}{1}", letter, i);
               switch (j) { // buscamos solamente las columnas que son necesarias
                   case 12: moldid = lstdata.get(helper); break; //17
                                                                 //case 13: moldid = lstdata.get(helper); break; //17
                   default: continue; // las demas son irrelevantes
               }
               if (moldid.Length == 0) continue;
               // checamos que los batches existan y que esten en el status correcto
               var m = mbl.get_moldloss_dtl(moldid);
               err.require(m == null, cme.INTERNAL_ERROR);
               if (m._dep == 1) continue; // ya esta en sap
                                          // here we really say that this mold is authorized/unauthorized
               m._dep = m._dep == 2 ? 0 : 2;
               mbl.update_moldloss_dtl(m);
           }
       }
   }

   protected void validate_moldloss_action() {
       var mbl = bls.get_manbl();
       var b = mbl.get_batch(mbl.EXTRACT_BATCH_FROM_PARAMS(values));

       // + is a change, there is a twin 
       string prodtocheck = mbl.get_twin(b.product);
       if (prodtocheck.Length == 0) prodtocheck = b.product.id;

       // we must check the casting batch
       var btwin = new batch(b);
       btwin.product = new product(prodtocheck);
       var bhdr = mbl.get_batch_header(btwin, consts.LOCCST, 1);
       err.require(bhdr == null, mse.BATCH_NOT_EXIST);

       // checamos el moldloss
       bhdr = mbl.get_batch_header(b, consts.LOCMOL, 1);
       err.require(bhdr == null, mse.MOLDLOSS_NOT_EXISTS);
       var sta = bhdr.status.TrimEnd();
       err.require(sta != status.WIP, mse.MOLDLOSS_NOT_IN_WIP);
       err.require(bhdr.as_400 != "0", mse.MOLDLOSS_ALREADY_UPLOADED);
   }

   protected void edit_ml_reg() {
       validate_moldloss_action();

       var moldid = values.get("moldini");
       int qty = values.getint("qty");

       //err.require(qty < 1, mse.INVALID_QTY);
       err.require(qty != 1, mse.QTY_ONLY_ONE);

       var mbl = bls.get_manbl();
       var m = mbl.get_moldloss_dtl(moldid);
       err.require(m == null, cme.LOGIC_ERROR);
       err.require(m._dep == 1, mse.MOLD_ALREADY_IN_SAP);

       m.total = qty;
       mbl.update_moldloss_dtl(m);
   }

   protected void add_ml_reg() {
       validate_moldloss_action();

       var mbl = bls.get_manbl();
       var b = mbl.get_batch(mbl.EXTRACT_BATCH_FROM_PARAMS(values));

       var bas = values.get("base");
       var addition = values.get("addition");
       var mouldCLR = values.get("clr");
       var mouldFB = values.get("fb");
       var defect = values.get("defect");
       var source = values.get("source");
       var qty = values.getint("qty");
       var oper = values.get("operator");
       var mold = values.get("moldcod");

       mouldCLR = mouldCLR.ToUpper();
       mouldFB = mouldFB.ToUpper();

       err.require(bas.Length != 4, mse.INC_DAT_BASE);
       err.require(addition.Length != 4, mse.INC_DAT_ADD);
       err.require(mouldCLR != "C" && mouldCLR != "L" && mouldCLR != "R", mse.WRONG_EYE);
       err.require(mouldFB != "F" && mouldFB != "B", mse.WRONG_FB);
       err.require(defect.Length == 0, mse.INC_DAT_DEFECT);
       err.require(source.Length == 0, mse.INC_DAT_SOURCE);
       //err.require(qty <= 0, mse.CANNOT_CAPTURE_ZERO);
       err.require(qty != 1, mse.QTY_ONLY_ONE);
       //err.require(qty > 99, "cannot_capture_99");
       err.require(oper.Length == 0, mse.INC_DAT_OPER);
       //err.require(mold.Length == 0, mse.INC_DAT_MOLD);

       // check that if the product is transition the mold have to be captured
       var istransition = mbl.is_transition(b.product);
       err.require(istransition && mold.Length == 0, "moldloss_transition_needs_moldid");

       // validate the source of the error
       var dfs = mbl.get_defect_source(source);
       err.require(dfs == null, mse.WRONG_DEFECT_SOURCE);
       result.set("$fuentedesc$", dfs.description);

       // validate the defect that cause the moldloss
       var qc = bls.get_qcbl();
       var def = qc.get_defect_by_location(consts.LOCMOL, new defect(defect));
       err.require(def == null, mse.WRONG_DEFECT);
       result.set("$defdesc$", def.description_s);

       var eye = "0";
       if (mouldCLR == "L") eye = "1";
       else if (mouldCLR == "R") eye = "2";

       var sku = string.Concat(b.product.id, bas, ".", addition, eye);
       var bpl = bls.get_plnbl();
       var pln = bpl.get_production_plan(b, new resource(sku));
       err.require(pln == null || pln.qty == 0, mse.MOLD_NOT_PLANNED);

       var m = new moldloss_detail(b.lotno.id, b.product.id, b.line.id, "1");

       m.basedtl = new basetype(bas);
       m.addition = addition;
       m.mouldlr = mouldCLR;
       m.mouldfb = mouldFB;
       m.mouldsrc = int.Parse(source);
       m.mouldchg = 1;

       m._mat = 0;
       m._grp = 1;
       m._dep = 2;
       m._def = int.Parse(defect);

       m.total = qty;
       m.oper = oper;
       m.mol = new mold(mold);

       mbl.insert_moldloss_dtl(m);
   }

   protected void delete_ml_reg() {
       validate_moldloss_action();

       var moldid = values.get("moldini");

       var mbl = bls.get_manbl();
       var m = mbl.get_moldloss_dtl(moldid);
       err.require(m == null, cme.LOGIC_ERROR);
       err.require(m._dep == 1, mse.MOLD_ALREADY_IN_SAP);

       mbl.delete_moldloss_dtl(moldid);
   }
</script>

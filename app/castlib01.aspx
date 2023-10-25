<%@ Import Namespace="sfc" %>
<%@ Import Namespace="sfc.BL" %>
<%@ Import Namespace="sfc.BO" %>

<!-- #include file="~/core.aspx" -->
<script runat="server" language="C#">

   protected void get_production_plan_by_line() {
       var btc = values.get(key.BATCH);
       var lin = values.get(key.LINE);

       var bt = new lot(btc);
       var li = new line(lin);

       var plan = bls.get_plnbl().get_production_plan_by_line(bt, li);

       int listid = values.getint(defs.ZLISTAF);
       var lr = new ListResponse(listid, 8, lnk.clie);
       var row = 0;

       foreach (var o in plan) {
           lr.set_data(row, 'A', o.lotno.id, 'B', o.product.id, 'C', o.line.id,
                            'D', o.resource_.id, 'E', o.qty.ToString());
           lr.set_data(row, 'F', o.plan_cst.ToString(), 'G', o.acum.ToString(),
                            'H', o.band.ToString(), '*', "0");
           ++row;
       }

       lr.set_rows(row);
       lr.pass_to_obj(result);
   }

   protected void accumulate_defect() {
       var btc = values.get(key.BATCH);
       var prd = values.get(key.PROD);
       var lin = values.get(key.LINE);
       var prt = values.get(key.PART);

       var lct = values.get(key.LOCATION);
       var cyc = values.getint(key.CYCLE);
       var res = values.get(key.SKU);
       var def = values.getint(key.DEFECT);
       var qty = values.getint(key.QTY);
       var dis = values.getint(key.DISCOUNTFQ);

       var bat = new batch(btc, prd, lin, prt);
       var loc = new location(lct);
       var sku = new resource(res);

       var man = bls.get_manbl();

       var bth = man.get_batch_header(new batch_header(bat, loc));
       err.require(bth == null, mse.BATCH_NOT_EXIST);

       bth.status = bth.status.Trim();
       err.require(bth.status != status.WIP, mse.CAP_MUSTBE_WIPCST);

       var hdr = man.get_batch(bat);
       err.require(hdr == null, mse.BATCH_NOT_EXIST);
       err.require(hdr.status == batchstatus.RELEASE, mse.BATCH_MUSTBE_WIP);

       var qc = bls.get_qcbl();
       var rej = new defect(def);
       var rec = qc.get_defect(rej);
       err.require(rec == null, mse.DEFECT_NOT_EXIST);
       man.accumulate_production_reject(new lot(btc), new product(prd),
                                           new line(lin), prt, sku, qty, loc, cyc,
                                           new defect(def), dis == 1);
   }

   protected void no_check_batch_accumulate_defect() {
       var btc = values.get(key.BATCH);
       var prd = values.get(key.PROD);
       var lin = values.get(key.LINE);
       var prt = values.get(key.PART);

       var lct = values.get(key.LOCATION);
       var cyc = values.getint(key.CYCLE);
       var res = values.get(key.SKU);
       var def = values.getint(key.DEFECT);
       var qty = values.getint(key.QTY);
       var dis = values.getint(key.DISCOUNTFQ);

       var bat = new batch(btc, prd, lin, prt);
       var loc = new location(lct);
       var sku = new resource(res);

       var man = bls.get_manbl();
       var hdr = man.get_batch(bat);
       err.require(hdr == null, mse.BATCH_NOT_EXIST);
       err.require(hdr.status == batchstatus.RELEASE, mse.BATCH_MUSTBE_WIP);

       var qc = bls.get_qcbl();
       var rej = new defect(def);
       var rec = qc.get_defect(rej);
       err.require(rec == null, mse.DEFECT_NOT_EXIST);
       man.accumulate_production_reject(new lot(btc), new product(prd),
                                           new line(lin), prt, sku, qty, loc, cyc,
                                           new defect(def), dis == 1);
   }

   protected void accumulate_sku() {
       var btc = values.get(key.BATCH);
       var prd = values.get(key.PROD);
       var lin = values.get(key.LINE);
       var prt = values.get(key.PART);
       var res = values.get(key.SKU);

       var bat = new batch(btc, prd, lin, prt);
       var sku = new resource(res);

       // we hit the production
       var man = bls.get_manbl();
       man.accumulate_production_detail(bat.lotno, bat.product, bat.line, prt,
           //sku, 1, consts.LOCCST, 1, new defect_type(typeq.FQ), consts.DEFFQ);
           sku, 1, consts.LOCCST, 1, consts.DEFTYPFG, consts.DEFFQ);

       // we hit the inventory
       var cst = bls.get_cstbl();
       cst.accumulate_production_inventory(bat.lotno, bat.product, bat.line, sku, 1);

       // we hit the plan
       var pln = bls.get_plnbl();
       pln.accumulate_production_plan(bat.lotno, bat.product, bat.line, sku, 1);

       // always check is enough prodution made for the batch
       var ispull = pln.ispull(bat.lotno, bat.product, bat.line, sku);

       if (ispull) result.on(key.ISPULL);
   }

   protected void accumulate_sku_and_defect() {
       accumulate_sku();
       accumulate_defect();
       // at this moment we will not take into acount whether is pull or not
       // maybe on the future this function will take it into acount
       result.del(key.ISPULL);
   }

   protected void pass_cast_to_coat() {
       /*EXTRACT_BATCH_FROM_PARAMS();

       check_product_is_coated(prod);

       // checamos si es una linea lean
       _is_lean_line();
       require(!_params.getbool(_T("$isleanline$"),12), _T("must_be_lean_line"));

       batch_must_be_in_wip();

       _send_to_coating();

       // once we have passed all the quality we discount the coating rejects
       cCommand command(_basics);
       command.Format(	_T("select resource, qty from batch_detail with (nolock) ")
                   _T("where batch='%s' and prod_code='%s' and line_id='%s' and part='%s' ")
                   _T("and location='COT' and cycle='1' and detail_type='RJ'"), 
                   batch, prod, line, part);						
       getconnectionx(con, obj);
       con.execute(command, obj);
       TCHAR resource[16];

       getconnection(rec);
       for(;!obj.IsEOF();obj.MoveNext())
       {
          obj.get(0, resource);
          int qty = obj.getint(1);
          command.Format(	_T("exec dcs_ins_batch_detail ")
                      _T("'%s','%s','%s','%s','COT',1,'FQ','%s',0,%d;"),
                      batch, prod, line, part, resource, qty * -1);
          rec.execute(command);
       }

       // FIXGUB delete inconsistent data, qty in negatives (BUG)
       command.Format(	_T("delete batch_detail ")
                   _T("where batch='%s' and prod_code='%s' and line_id='%s' and part='%s' and ")
                   _T("location='COT' and cycle='1' and detail_type='FQ' and reason_code=0 and ")
                   _T("qty<=0 "),
                   batch, prod, line, part);
       con.execute(command, obj);*/

       var man = bls.get_manbl();
       var b = man.get_batch(man.EXTRACT_BATCH_FROM_PARAMS(values));
       man.check_product_is_coated(b.product);
       err.require(!man.is_lean_line(new line(b.line.id)), mse.MUST_BE_LEAN_LINE);
       man.batch_must_be_in_wip(b);
       //man.send_to_coating(b);
       man.update_coat_rejects_and_cast_fq(b);
   }

</script>

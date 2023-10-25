<%@ Import Namespace="sfc" %>
<%@ Import Namespace="sfc.BL" %>
<%@ Import Namespace="sfc.BO" %>

<!-- #include file="~/core.aspx" -->
<script runat="server" language="C#">

   protected void create_packaging() {
       var btc = values.get(key.BATCH);
       var prd = values.get(key.PROD);
       var lin = values.get(key.LINE);
       var prt = values.get(key.PART);
       var loc = values.get(key.LOCATION);
       var ltn = values.get(key.LOTNUMBER);

       var lt = new batch(btc, prd, lin, prt);
       var lc = new location(loc);

       var pkg = bls.get_pkgbl();
       var blot = pkg.generate_shipment(lt, lc, ltn);
       result.set(key.LOTNUMBER, blot.id);
   }

   protected void close_packaging() {
       var btc = values.get(key.BATCH);
       var prd = values.get(key.PROD);
       var lin = values.get(key.LINE);
       var prt = values.get(key.PART);
       var loc = values.get(key.LOCATION);

       var lt = new batch(btc, prd, lin, prt);
       var lc = new location(loc);

       var pkg = bls.get_pkgbl();
       var isinwip = pkg.close_trans_batch(lt, lc);
       result.set("isinwip", isinwip);
   }

   protected void load_product_structure() {
       var btc = values.get(key.BATCH);
       var lin = values.get(key.LINE);

       var bt = new lot(btc);
       var li = new line(lin);


       var pln = bls.get_plnbl();
       var plan = pln.get_products_planned_by_line(bt, li);

       int listid = values.getint(defs.ZLISTAF);
       var lr = new ListResponse(listid, 8, lnk.clie);
       var row = 0;

       foreach (var o in plan) {
           lr.set_data(row, 'A', o.id, '*', "0");
           ++row;
       }

       lr.set_rows(row);
       lr.pass_to_obj(result);
   }

   protected void return_bulk_pack() {
       var btc = values.get(key.BATCH);
       var prd = values.get(key.PROD);
       var lin = values.get(key.LINE);
       var prt = values.get(key.PART);
       var sku = values.get(key.SKU);
       var qty = values.get(key.QTY);
       var mfg = values.get("mfg");

       err.require(qty.Length == 0, mse.INC_DAT_QTY);
       var q = int.Parse(qty);

       err.require(mfg.Length == 0, mse.INC_DAT_QTY, "mfg");
       var m = int.Parse(mfg);

       var bat = new batch(btc, prd, lin, prt);
       var res = new resource(sku);

       var pkg = bls.get_pkgbl();
       var bpdtl = pkg.insert_and_return_bulk_pack(bat, res, q, m);
       result.set(key.BULKPACK, bpdtl);
   }

   protected void verify_batch_header() {
       var btc = values.get(key.BATCH);
       var prd = values.get(key.PROD);
       var lin = values.get(key.LINE);
       var prt = values.get(key.PART);
       var loc = values.get(key.LOCATION);

       var lt = new batch_header(btc, prd, lin, prt, loc, 0);
       var man = bls.get_manbl();
       var bat = man.get_batch_header(lt);

       if (bat != null) {
           bat.status = bat.status.Trim();
           result.set(key.STATUS, bat.status);
       }
   }

   protected void verify_batch_lot() {
       var btc = values.get(key.BATCH);
       var prd = values.get(key.PROD);
       var lin = values.get(key.LINE);
       var prt = values.get(key.PART);
       var loc = values.get(key.LOCATION);

       var lt = new batch(btc, prd, lin, prt);
       var lc = new location(loc);

       var man = bls.get_manbl();
       var blot = man.get_batch_lot(lt, lc);

       if (blot != null) result.set(key.LOTNUMBER, blot.lot_number);
   }

</script>

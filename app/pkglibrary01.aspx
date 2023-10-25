<%@ Import Namespace="sfc" %>
<%@ Import Namespace="sfc.BL" %>
<%@ Import Namespace="sfc.BO" %>

<!-- #include file="~/core.aspx" -->
<script runat="server" language="C#">

   protected void authorize_box_2_upload() {
       var btc = values.get(key.BATCH);
       var prd = values.get(key.PROD);
       var lin = values.get(key.LINE);
       var prt = values.get(key.PART);
       var blk = values.get(key.BULKPACK);

       err.require(blk == string.Empty, mse.INC_DAT_BULKPACK);

       var bat = new batch(btc, prd, lin, prt);
       bat.validate();

       var fullbat = bat.getfullbatch();

       var pkg = bls.get_pkgbl();

       var bulk = new bulk_pack_upload(btc, prd, lin, prt);
       bulk.bulk_pack_id = blk;
       box_status res = pkg.authorize_box(bulk, true, false);
       switch (res.status) {
           case box_status.AUTHORIZED: set_log(logacts.AUTHORIZE_BOX + ":" + blk, fullbat, logtype.DTL); break;
           case box_status.PENDING: set_log(logacts.UNAUTHORIZE_BOX + ":" + blk, fullbat, logtype.DTL); break;
           case box_status.UPLOADED: set_log(logacts.UNUPLOAD_BOX + ":" + blk, fullbat, logtype.DTL); break;
       }
   }

   protected void authorize_boxes_2_upload() {
       var btc = values.get(key.BATCH);
       var prd = values.get(key.PROD);
       var lin = values.get(key.LINE);
       var prt = values.get(key.PART);
       var typ = values.getint(key.TYPE, -1);

       bool autorization = true;
       bool unupload = false;
       if (typ != -1) {
           if (typ == 0) { autorization = true; unupload = false; }
           if (typ == 1) { autorization = false; unupload = true; }
       }

       var bat = new batch(btc, prd, lin, prt);
       bat.validate();
       var bulk = new bulk_pack_upload(btc, prd, lin, prt);

       var fullbat = bat.getfullbatch();

       var lstdata = new mroJSON();
       values.get(defs.ZLSTDAT, lstdata);
       var total = lstdata.getint(defs.ZLSTTOT);
       var cols = lstdata.getint(defs.ZLSTCLS);
       var helper = string.Empty;

       var pkg = bls.get_pkgbl();

       for (var i = 0; i < total && i < 1024; ++i) // 1024 safety break;
       {
           var letter = 'A';
           var bulkpackid = string.Empty;
           for (var j = 0; j < cols; ++j, ++letter) {
               helper = string.Format("{0}{1}", letter, i);
               switch (j) { // buscamos solamente las columnas que son necesarias
                   case 4/*5*/: lstdata.get(helper, ref bulkpackid); break; //17
                   default: continue; // las demas son irrelevantes
               }
           }
           if (bulkpackid.Length > 0) {
               bulk.bulk_pack_id = bulkpackid;
               box_status res = pkg.authorize_box(bulk, autorization, unupload);
               switch (res.status) {
                   case box_status.AUTHORIZED: set_log(logacts.AUTHORIZE_BOX + ":" + bulkpackid, fullbat, logtype.DTL); break;
                   case box_status.PENDING: set_log(logacts.UNAUTHORIZE_BOX + ":" + bulkpackid, fullbat, logtype.DTL); break;
                   case box_status.UPLOADED: set_log(logacts.UNUPLOAD_BOX + ":" + bulkpackid, fullbat, logtype.DTL); break;
               }
           }
       }
   }

   // Package Sort: ordenamiento en base a imagenes (loteria) utilizado en Poly-->

   /**
    * WebService que se encarga de recibir parametros
    */
   protected void get_sku_to_sort() {
       result.set(key.IMAGE, string.Format("http://{0}:{1}/{2}/{3}",
                                    ApplicationInstance.Request.Url.DnsSafeHost,
                                    ApplicationInstance.Request.Url.Port,
                                    "images", bls.get_pkgbl().getImage(new lot(values.get("$batch$")),
                                                                               new line(values.get("$module$")),
                                                                               new resource(values.get("$samplesku$")))
                                                                               ));
       result.set("$sku$", values.get("$samplesku$"));
   }


   /* URI 05/30/2012 Muestre Imagen en Blanco en la Loteria antes de la imagen normal*/
   protected void get_image_blank() {
       result.set("uimage", string.Format("http://{0}:{1}/{2}/{3}",
                                    ApplicationInstance.Request.Url.DnsSafeHost,
                                    ApplicationInstance.Request.Url.Port,
                                    "images", "blank.jpg"));

       result.set("$sku$", "0000000.00000");

   }

   /**
    * WebService que consulta toda la info de imagenes 
    */
   protected void getRelationInformation() {
       var batch = values.get("$batch$");
       var module = values.get("$module$");

       packaging_BL pkgBL = bls.get_pkgbl();

       var ima_info = pkgBL.getRelationInformation(new lot(batch), new line(module));
       int listid = values.getint(defs.ZLISTAF);
       var lr = new ListResponse(listid, 3, lnk.clie);
       int row = 0;

       //lista uno (relacion image-sku actuales)
       string sku = string.Empty;
       var data = ima_info.Item1;
       foreach (var p in data) {
           lr.set_data(row, 'A', batch, 'B', module, 'C', p, '*', "0");
           sku = p;
           ++row;
       }
       lr.set_rows(row);
       lr.pass_to_obj(result);

       listid = values.getint(defs.ZLISTAF);
       lr = new ListResponse(listid + 1, 5, lnk.clie);
       row = 0;
       //lista cero (nuevos sku)
       var info = ima_info.Item2;
       foreach (var p in info) {
           lr.set_data(row, 'A', batch, 'B', module, 'C', sku, 'D', p, 'E', string.Empty, '*', "1");
           ++row;
       }
       lr.set_rows(row);
       lr.pass_to_obj(result);
   }

   /**
    * WebService que se encarga actualizar un sku por otro
    */
   protected void updateSku() {
       var batch = values.get("$batch$");
       var module = values.get("$module$");
       var sku = values.get("$sku$");
       var sku_new = values.get("$newsku$");

       packaging_BL pkg = bls.get_pkgbl();
       pkg.updateSku(new lot(batch), new line(module), new resource(sku), sku_new);
   }

</script>

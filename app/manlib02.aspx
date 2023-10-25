<%@ Import Namespace="sfc" %>
<%@ Import Namespace="sfc.BL" %>
<%@ Import Namespace="sfc.BO" %>

<!-- #include file="~/core.aspx" -->
<script runat="server" language="C#">

   /**
    * Metodos para la captura de moldes pulidos en CR-39, utilizados en la transaccion: 
    * tpmcap (Transaction Polished Moulds Capture)-->
    * Metodo comun encargado de recibir los parametros defaul de la transaccion y 
    * guardarlos en un objeto batch header 
    */
   private batch_header get_polishedMoulds_params(ref operador op, ref mold ml) {
       batch_header paramss = new batch_header();
       paramss.lotno = new lot { id = values.get("$batch$") };
       paramss.line = new line { id = values.get("$_line$") };
       paramss.product = new product { id = values.get("$_prod$") };
       paramss.location = new location { id = values.get("$__loc$") };
       paramss.part = values.get("$_part$");
       paramss.cycle = 1;
       op = new operador { id = values.get("$operator$") };
       paramss.status = values.get("$statushdr$");
       //        paramss.audit = new audit_bo { created_by = basics.get(defs.ZUSERID) };
       ml = new mold { msource = "0" };
       return paramss;
   }

   /**
    * Metodo comun encargado de recibir los parametros defaul de la transaccion y 
    * guardarlos en un objeto mold para insertar un nuevo molde
    */
   private mold get_polishedMouldsDtl_params() {
       mold par = new mold();

       par.BasePwr = values.get("$cbase$");
       par.AddPwr = values.get("$cadd$");
       par.Eye = values.get("$ceye$");
       par.FB = values.get("$ctmold$");
       par.msource = "0";
       par.mchange = "0";
       par.dmat = "0";
       par.dgrp = "0";
       par.ddep = "0";
       par.defect = "0";
       par.id = values.get("$cmold$");

       par.qty = 1;
       par.statusA = values.get("$statushdr$");
       par.statusN = status.REL;

       return par;
   }

   /**
    * Metodo comun encargado de recibir los parametros seleccionados de la tabla
    */
   private mold get_selected_params() {
       mold par = new mold();

       par.BasePwr = values.get(key.LCELLA);
       par.AddPwr = values.get(key.LCELLB);
       par.Eye = values.get(key.LCELLC);
       par.FB = values.get(key.LCELLD);
       par.msource = "0";
       par.mchange = "0";
       par.dmat = "0";
       par.dgrp = "0";
       par.ddep = "0";
       par.defect = "0";
       par.id = values.get(key.LCELLE);

       if (values.get(key.LCELLF) != "") {
           int q;
           err.require(!int.TryParse(values.get(key.LCELLF), out q), mse.INVALID_QTY);
           par.qty = q;
       }
       else {
           par.qty = 1;
       }
       par.statusN = status.REL;

       return par;
   }

   /**       
    * Obtiene la lista de moldes que se han capturado para un lote, linea y producto 
    * especifico
    */
   protected void getPolishedMoulds() {
       //Obtiene los parametros de la transaccion
       operador op = null;
       mold ml = null;
       batch_header paramss = get_polishedMoulds_params(ref op, ref ml);
       string batch_status = string.Empty;

       //lee el detalle para desplegarlo en la tabla de la transaccion
       casting_BL cst = bls.get_cstbl();
       List<mold> x = cst.getPolishedMoulds(paramss, ml);

       int listid = values.getint(defs.ZLISTAF);
       var lr = new ListResponse(listid, 8, lnk.clie);
       var row = 0;

       foreach (var o in x) {
           lr.set_data(row, 'A', o.BasePwr.ToString(), 'B', o.AddPwr.ToString(),
                        'C', o.Eye.ToString(), 'D', o.FB.ToString(), 'E', o.id.ToString());
           lr.set_data(row, 'F', o.qty.ToString(), 'G', o.oper.id.ToString(), 'H',
                         string.Format("{0:dd/MM/yyyy}", o.date_time), '*', "1");
           batch_status = o.statusA;
           ++row;
       }

       lr.set_rows(row);
       lr.pass_to_obj(result);
       result.set("$statushdr$", batch_status);
   }

   /**
    * Se encarga de insertar un molde con cantidad 1
    */
   protected void insertPolishedMoulds() {
       //Obtiene los parametros de la transaccion
       operador op = null;
       mold ml = null;
       batch_header paramss = get_polishedMoulds_params(ref op, ref ml);
       mold par = get_polishedMouldsDtl_params();

       // llama al metodo encargado de insertar el header
       casting_BL cbl = bls.get_cstbl();
       cbl.insertPolishedMoulds(paramss, op, par);

       // llama al metodo encargado de consultar los detalles para hacer un refresh 
       // automatico en la transaccion
       getPolishedMoulds();
   }

   /**
    * Elimina todo el registro de un molde en especifico sin importar la cantidad
    */
   protected void deletePolishedMoulds() {
       //Obtiene los parametros de la transaccion
       operador op = null;
       mold ml = null;
       batch_header paramss = get_polishedMoulds_params(ref op, ref ml);
       mold m = get_selected_params();

       // llama al metodo encargado de insertar el header
       casting_BL cbl = bls.get_cstbl();
       cbl.deletePolishedMoulds(paramss, m);

       // llama al metodo encargado de consultar los detalles para hacer un refresh
       // automatico en la transaccion
       getPolishedMoulds();
   }

   /**
    * Crea el registro en el batch header para el location recibido
    */
   protected void insertBatchHeader() {
       //Obtiene los parametros de la transaccion
       operador op = null;
       mold ml = null;
       batch_header paramss = get_polishedMoulds_params(ref op, ref ml);

       // llama al metodo encargado de insertar el header
       casting_BL cbl = bls.get_cstbl();
       cbl.insertBatchHeader(paramss);

       // llama al metodo encargado de consultar los detalles para hacer un refresh
       // automatico en la transaccion
       getPolishedMoulds();
   }

   /**
    * Cambia de estatus el batch a RELEASE
    */
   protected void releasePolishedMoulds() {
       //Obtiene los parametros de la transaccion
       operador op = null;
       mold ml = null;
       batch_header paramss = get_polishedMoulds_params(ref op, ref ml);
       mold m = get_polishedMouldsDtl_params();

       // llama al metodo encargado de insertar el header
       casting_BL cbl = bls.get_cstbl();
       cbl.releasePolishedMoulds(paramss, m);

       // llama al metodo encargado de consultar los detalles para hacer un refresh
       // automatico en la transaccion
       getPolishedMoulds();
   }

   /**
    * Actualiza la cantidad de moldes leidos, aumentandolo en 1.
    */
   protected void updatePolishedMould() {
       //Obtiene los parametros de la transaccion
       operador op = null;
       mold ml = null;
       batch_header paramss = get_polishedMoulds_params(ref op, ref ml);
       mold m = get_selected_params();

       // llama al metodo encargado de insertar el header
       casting_BL cbl = bls.get_cstbl();
       cbl.updatePolishedMould(paramss, m, "descontar");

       // llama al metodo encargado de consultar los detalles para hacer un refresh
       // automatico en la transaccion
       getPolishedMoulds();
   }

   /**
    * Metodos para validar y cachar los malos apareos de poly, utilizados en la 
    * transaccion: 
    * tcmatchmoldlent (Transaction apareo entre el molde y el lente)-->
    * WebService que obtiene las revisiones realizadas y se encarga de retornalos 
    * al cliente
    */
   protected void getMoldLentMatches() {
       var batch_get = values.get("$batch$");
       var module_get = values.get("$_line$");

       batch ba = new batch(batch_get, module_get);
       var cst = bls.get_cstbl();
       var data = cst.getMoldLentMatches(ba);

       int listid = values.getint(defs.ZLISTAF);
       var lr = new ListResponse(listid, 3, lnk.clie);
       var row = 0;
       foreach (var o in data) {
           lr.set_data(row, 'A', o.pallet.id.ToString(), 'B', o.molde.id, 'C', o.campo1, 'D', o.campo2, 'E', o.campo3);
           lr.set_data(row, 'F', o.campo4, '*', o._img.ToString());
           ++row;
       }

       lr.set_rows(row);
       lr.pass_to_obj(result);
   }

   /**
    * Se encarga de validar: que el campo a actualizar no este validado OK, 
    * valida que el campo aun se pueda modificar, valida que el indice introducido 
    * corresponda  al indice del sku e inserta el resultado.
    */
   protected void validEditField_MoldLent() {
       var batch_get = values.get("$batch$");
       var module_get = values.get("$_line$");
       var molde = values.get("$Molde$");
       var resource = values.get("$Resource$");
       var pallet = values.get("$Pallet$");
       var _1 = values.get("$1$");
       var _2 = values.get("$2$");
       var _3 = values.get("$3$");
       var _4 = values.get("$4$");
       var d = values.get(key.LCELLC);
       var e = values.get(key.LCELLD);
       var f = values.get(key.LCELLE);
       var g = values.get(key.LCELLF);
       var resultImage = string.Empty;
       var validInsert = string.Empty;

       int listid = values.getint(defs.ZLISTAF);
       var lr = new ListResponse(listid, 3, lnk.clie);
       var row = 0;

       var cst = bls.get_cstbl();
       batch ba = new batch(batch_get, module_get);

       /*consulta el sku*/
       var sku = cst.getMoldSku(new mold_lent_match(batch_get, molde, module_get,
                                  resource, "", Convert.ToInt32(pallet), 0));

       /*Verifica si el valor ya esta ok*/
       var dato_anterior = cst.validaDatoAnterior(new mold_lent_match(batch_get, molde,
                                  module_get, sku, _1, _2, _3, _4, d, e, f, g));

       if (dato_anterior != "OK") {
           /*Verifica que el campo que se edito y si aun tiene oportunidades para 
            * validacion, retorna NO u OK segun sea el caso*/
           var editField = cst.validEditField_MoldLent(new mold_lent_match(batch_get,
                             molde, module_get, sku, _1, _2, _3, _4, d, e, f, g));

           foreach (var j in editField) {
               if (j.validacion == "NO") {
                   var data = cst.getMoldLentMatches(ba);
                   foreach (var o in data) {
                       lr.set_data(row, 'A', o.pallet.id.ToString(), 'B', o.molde.id,
                                    'C', o.campo1, 'D', o.campo2, 'E', o.campo3);
                       lr.set_data(row, 'F', o.campo4, '*', o._img.ToString());
                       ++row;
                   }

                   lr.set_rows(row);
                   lr.pass_to_obj(result);

                   err.require(j.validacion == "NO", "only_two_validations");
               }
               else {
                   /*Si es editable aun, se valida que el numero introducido haga match con el molde-sku, inserta el registro y retorna la info de validaciones*/
                   var resultValida = cst.validateMoldLent(new mold_lent_match(batch_get,
                                  molde, module_get, sku, j.valor_introducido,
                                  Convert.ToInt32(pallet), j.campo));
                   foreach (var va in resultValida) {
                       resultImage = va.showmsg;
                       validInsert = va.resultado;
                   }

                   var data = cst.getMoldLentMatches(ba);
                   foreach (var o in data) {
                       lr.set_data(row, 'A', o.pallet.id.ToString(), 'B', o.molde.id,
                                    'C', o.campo1, 'D', o.campo2, 'E', o.campo3);
                       lr.set_data(row, 'F', o.campo4, '*', o._img.ToString());
                       ++row;
                   }

                   lr.set_rows(row);
                   lr.pass_to_obj(result);

                   err.require(resultImage == "2", "Error_Not_Match");
                   err.require(validInsert != "OK", "error_on_insert_revision");
               }
           }
       }
       else {
           var data = cst.getMoldLentMatches(ba);
           foreach (var o in data) {
               lr.set_data(row, 'A', o.pallet.id.ToString(), 'B', o.molde.id,
                             'C', o.campo1, 'D', o.campo2, 'E', o.campo3);
               lr.set_data(row, 'F', o.campo4, '*', o._img.ToString());
               ++row;
               resultImage = o._img.ToString();
           }

           lr.set_rows(row);
           lr.pass_to_obj(result);

           err.require(dato_anterior == "OK", "Completed_revision");
       }
   }

   /**
    * Metodos para el AQL de Focovision en Poly-->
    * Obtiene el AQL de focovision que se va a muestrear
    */
   protected void getAQLFoco() {
       string batch = values.get("$batch$");
       string line_id = values.get("$_line$");
       string prod_code = values.get("$_prod$");
       string _base = values.get("$base$");
       string part = values.get("$_part$");
       string usr = basics.get(defs.ZUSERID);

       var bat = new batch(batch, prod_code, line_id, part);
       bat = bls.get_manbl().get_batch(bat);
       err.require(bat == null, mse.BATCH_NOT_EXIST);

       List<focovision_sample> x = bls.get_cstbl().getAQLFoco(bat, new basenum(_base), new user(usr));

       int listid = values.getint(defs.ZLISTAF);
       var lr = new ListResponse(listid, 5, lnk.clie);
       var row = 0;

       foreach (var o in x) {
           lr.set_data(row, 'A', o.batch, 'B', o.module, 'C', o.base_, 'D', o.batch_size.ToString(), 'E', o.batch_sample, '*', "1");
           ++row;
       }

       lr.set_rows(row);
       lr.pass_to_obj(result);
   }

   /**
    * consulta la cantidad de lentes que se han medido en focovision
    */
   protected void getFocoReads() {
       string batch = values.get("$batch$");
       string line_id = values.get("$_line$");
       string prod_code = values.get("$_prod$");
       string _base = values.get("$base$");
       string part = values.get("$_part$");
       string _user = basics.get(defs.ZUSERID);

       batch_detail bh = new batch_detail(batch, prod_code, line_id, part);
       bh.sku.base_ = _base;

       casting_BL cst = bls.get_cstbl();
       result.set("$lectura$", cst.getFocoReads(bh));
   }

   /**
    * Copia de pallets de una linea a otra-->
    * WebService que se encarga de copiar los pallets de una linea a otra
    */
   protected void copyPalletsBetweenLines() {
       var cst = bls.get_cstbl();
       string resultado = cst.copyPallets(new lot(values.get("$batch$")),
                                  new line(values.get("$_line$")), new line(values.get("$line2$")));
   }

   // deprecated by release 2.0 and the client uses execute_query direct
   /*protected void cast_sku_and_accumulate_defect()
   {
      var btc = values.get(key.BATCH);
      var prd = values.get(key.PROD);
      var lin = values.get(key.LINE);
      var prt = values.get(key.PART);
      var res = values.get(key.SKU);

      var bat = new batch(btc, prd, lin, prt);
      var sku = new resource(res);

      var cst = bls.get_cstbl(maindb);
      cst.cast_collection(bat, sku);

      accumulate_defect();
      // at this moment we will not take into acount whether is pull or not
      // maybe on the future this function will take it into acount
      // result.DeleteParameter(key.ISPULL);
   }*/

   protected void cap_edit_pallete() {
       var batch = values.get(key.CBATINI);
       var line = values.get(key.CLININI);
       var pallete = values.get(key.CPLTINI);

       err.require(batch.Length == 0, mse.INC_DAT_BATCH);
       err.require(line.Length == 0, mse.INC_DAT_LINE);
       err.require(line.Length != 2, mse.INV_LINE);
       err.require(pallete.Length == 0, mse.INC_DAT_PALLET);

       var man = bls.get_manbl();
       var oldpal = man.get_relation_pallet_dtl(new lot(batch),
                                        new line(line),
                                        new pallet(pallete));
       err.require(oldpal == null, mse.REG_NOT_EXIST, pallete);

       bool wasokey = false;
       var someerror = string.Empty;

       try {
           cap_del_pallete();
           cap_add_pallete();
           wasokey = true;
       }
       catch (mroerr e) {
           someerror = e.description;
       }
       catch (Exception e) {
           someerror = e.Message;
       }

       if (!wasokey) {
           man.insert_relation_pallet(oldpal);
           throw new Exception(someerror);
       }
   }

   protected void cap_add_pallete() {
       var batch = values.get(key.CBATINI);
       var prod = values.get(key.CPRDINI);
       var line = values.get(key.CLININI);
       var pallete = values.get(key.CPLTINI);
       var front = values.get("cmfront");
       var back = values.get("cmback");

       // for avoid bugs/misunderstoods
       pallete = pallete.Trim();
       front = front.Trim();
       back = back.Trim();

       err.require(batch.Length == 0, mse.INC_DAT_BATCH);
       err.require(prod.Length == 0, mse.INC_DAT_PROD);
       err.require(line.Length == 0, mse.INC_DAT_LINE);
       err.require(line.Length != 2, mse.INV_LINE);
       err.require(pallete.Length == 0, mse.INC_DAT_PALLET);
       err.require(pallete.Length != 4, "inv_fld_pallete4d");
       err.require(front.Length == 0, mse.INC_DAT_FRONT);
       err.require(back.Length == 0, mse.INC_DAT_BACK);

       var lt = new lot(batch);
       var pr = new product(prod);
       var ln = new line(line);
       var pl = new pallet(pallete);
       var fr = new mold(front);
       var bk = new mold(back);

       var modulo = line.Substring(0, 1);
       var shift = line.Substring(1, 1);

       Tuple<string, string> sku_index = null;

       var man = bls.get_manbl();
       var validation = man.is_mold_for_validate(lt, pr, ln, fr);
       // buscamos el sku y el index
       if (validation) {
           var f = bls.get_cstbl().get_mold(fr);
           err.require(f == null || f.id.Length == 0, mse.SKU_NOT_FOUND);
           string sku4val = string.Concat(prod, f.BasePwr, ".0", f.AddPwr, f.Eye);
           sku_index = man.get_pallete_index(pr, new resource(sku4val));
       }
       else sku_index = man.get_pallete_index(pr, fr);

       err.require(sku_index == null, mse.FRONT_MOLD_NOT_CONFIGURED);
       string sku = sku_index.Item1;
       string index = sku_index.Item2;

       var sk = new resource(sku);

       // si no es validacion checamos que este planeado
       bool planned = false;
       if (!validation) {
           var pln = bls.get_plnbl();
           planned = pln.exist_production_plan_detail(new production_plan(lt, pr, ln, sk));
       }

       err.require(!planned && !validation, mse.SKU_NOT_PLANNED_NOT_VALIDATION, sku);

       // we must check that this mold is not on other line from the same batch
       var ap = man.get_mold_in_another_line(lt, fr, shift, true);
       if (ap != null) {
           var d = string.Concat(ap.module, ",", ap.palletid.id, ",", ap.sku.id);
           err.require(mse.FRONT_MOLD_PLANNED_IN_OTHER_LINE, d);
       }

       // check that the "BACK" mold must be in production
       bool inprod = man.is_mold_on_production(bk);
       err.require(!inprod, mse.BACK_MOLD_NOT_IN_PRODUCTION, back);

       // we must check that the BACK mold is not on other line from the same batch
       ap = man.get_mold_in_another_line(lt, bk, shift, false);
       if (ap != null) {
           var d = string.Concat(ap.module, ",", ap.palletid.id, ",", ap.sku.id);
           err.require(mse.BACK_MOLD_PLANNED_IN_OTHER_LINE, d);
       }

       var cst = bls.get_cstbl();
       // we get the front base
       var bmfront = cst.get_mold(fr, "F");
       err.require(bmfront == null || bmfront.id.Length == 0, mse.BASE_4_FRONT_NOT_FOUND);
       string basefront = bmfront.BasePwr;

       // we get the back base
       var bmback = cst.get_mold(bk, "B");
       err.require(bmback == null || bmback.id.Length == 0, mse.BASE_4_BACK_NOT_FOUND);
       string baseback = bmback.BasePwr;

       // once we have the two bases we must check on polybacks if the pair is valid
       bool domatch = man.do_match_front_back(pr, basefront, baseback);
       err.require(!domatch, mse.FRONT_NOT_MATCH_BACK);

       // we check that the register is not already ont the table
       var pal = man.get_relation_pallet_dtl(lt, ln, pl);
       err.require(pal != null, mse.REG_ALREADY_EXIST, pl.id);

       // at last we just insert the pallet
       pal = new relation_pallet();
       pal.lote = lt;
       pal.module = line;
       pal.palletid = pl;
       pal.frontmold = fr;
       pal.backmold = bk;
       pal.sku = sk;
       pal.moldindex = index;

       man.insert_relation_pallet(pal);
   }

   protected void cap_del_pallete() {
       var batch = values.get(key.CBATINI);
       var line = values.get(key.CLININI);
       var pallete = values.get(key.CPLTINI);

       err.require(batch.Length == 0, mse.INC_DAT_BATCH);
       err.require(line.Length == 0, mse.INC_DAT_LINE);
       err.require(line.Length != 2, mse.INV_LINE);
       err.require(pallete.Length == 0, mse.INC_DAT_PALLET);

       bls.get_manbl().delete_relation_pallet(new lot(batch),
                                           new line(line),
                                           new pallet(pallete));
   }

   protected void poly_copy_palletes() {
       var batchsrc = values.get(key.CBATINI);
       var linesrc = values.get(key.CLININI);
       var batchdst = values.get(key.CBATFIN);
       var linedst = values.get(key.CLINFIN);

       err.require(linesrc.Length != 2 || linesrc[1] != '1', mse.WRONG_LINE_SOURCE);
       err.require(linedst.Length != 2 || linedst[1] != '1', mse.WRONG_LINE_TARGET);

       // check if there is not already the destiny palletes plan
       var data = bls.get_manbl().get_relation_pallet(new lot(batchdst),
                                              new line(linedst));
       err.require(data != null && data.Count > 0, mse.PLAN_PALLETES_ALREADY_EXISTS);

       // tomamos todos los moldes del plan original y lo pasamos uno por uno al destino
       data = bls.get_manbl().get_relation_pallet(new lot(batchsrc),
                                              new line(linesrc));
       err.require(data == null || data.Count == 0, mse.PLAN_PALLETES_SOURCE_NOT_EXIST);

       foreach (var d in data) {
           values.set(key.CBATINI, batchdst);
           values.set(key.CPRDINI, d.sku.id);
           values.set(key.CLININI, linedst);
           values.set(key.CPLTINI, d.palletid.id);
           values.set("cmfront", d.frontmold.id);
           values.set("cmback", d.backmold.id);

           cap_add_pallete();
       }
   }

   protected void poly_move_palletes() {
       var batchsrc = values.get(key.CBATINI);
       var linesrc = values.get(key.CLININI);
       var batchdst = values.get(key.CBATFIN);
       var linedst = values.get(key.CLINFIN);

       err.require(linesrc.Length != 2 || linesrc[1] != '1', mse.WRONG_LINE_SOURCE);
       err.require(linedst.Length != 2 || linedst[1] != '1', mse.WRONG_LINE_TARGET);

       // renombrar source como temporal, para evitar errores de validacion
       var original = batchsrc;
       var temporal = string.Concat("X", batchsrc.Substring(1, 3));

       bls.get_manbl().delete_whole_relation_pallet(new lot(temporal),
                                              new line(linesrc));
       bls.get_manbl().change_relation_pallet_batch(new lot(batchsrc),
                                              new lot(temporal),
                                              new line(linesrc));
       batchsrc = temporal;

       bool wasokey = false;
       var someerror = string.Empty;

       try {
           // check if there is not already the destiny palletes plan
           var data = bls.get_manbl().get_relation_pallet(new lot(batchdst),
                                                  new line(linedst));
           err.require(data != null && data.Count > 0, mse.PLAN_PALLETES_ALREADY_EXISTS);

           // tomamos todos los moldes del plan original y lo pasamos uno por uno al destino
           data = bls.get_manbl().get_relation_pallet(new lot(batchsrc),
                                                  new line(linesrc));
           err.require(data == null || data.Count == 0, mse.PLAN_PALLETES_SOURCE_NOT_EXIST);

           foreach (var d in data) {
               values.set(key.CBATINI, batchdst);
               values.set(key.CPRDINI, d.sku.id);
               values.set(key.CLININI, linedst);
               values.set(key.CPLTINI, d.palletid.id);
               values.set("cmfront", d.frontmold.id);
               values.set("cmback", d.backmold.id);

               cap_add_pallete();
           }

           // borramos el fuente(que es el temporal) porque es un move no un copy
           bls.get_manbl().delete_whole_relation_pallet(new lot(temporal),
                                                  new line(linesrc));
           wasokey = true;
       }
       catch (Exception e) {
           someerror = e.Message;
       }

       if (!wasokey) {
           bls.get_manbl().delete_whole_relation_pallet(new lot(original),
                                                  new line(linesrc));
           bls.get_manbl().change_relation_pallet_batch(new lot(temporal),
                                                  new lot(original),
                                                  new line(linesrc));
           err.require(someerror);
       }
   }

   protected void move_plan_between_batches() {
       var bti = new lot(values.get(key.CBATINI));
       var pri = new product(values.get(key.CPRDINI));
       var lni = new line(values.get(key.CLININI));
       var btf = new lot(values.get(key.CBATFIN));
       var prf = new product(values.get(key.CPRDFIN));
       var lnf = new line(values.get(key.CLINFIN));

       err.require(string.IsNullOrEmpty(bti.id), mse.INC_DAT_BATCH_INI);
       err.require(string.IsNullOrEmpty(pri.id), mse.INC_DAT_PROD_INI);
       err.require(string.IsNullOrEmpty(lni.id), mse.INC_DAT_LINE_INI);
       err.require(string.IsNullOrEmpty(btf.id), mse.INC_DAT_BATCH_FIN);
       err.require(string.IsNullOrEmpty(prf.id), mse.INC_DAT_PROD_FIN);
       err.require(string.IsNullOrEmpty(lnf.id), mse.INC_DAT_LINE_FIN);

       err.require(string.CompareOrdinal(bti.id, btf.id) == 0, mse.SAME_BATCH);
       err.require(string.CompareOrdinal(lni.id, lnf.id) == 0, mse.SAME_LINE);

       var pln = bls.get_plnbl();

       // we must check that there is a module source
       var srcplan = pln.exist_production_plan_any(new production_plan(bti, pri, lni));
       err.require(!srcplan, mse.PLAN_SOURCE_NOT_EXISTS);

       // we must check that the target module is empty
       var tarplan = pln.exist_production_plan_any(new production_plan(btf, prf, lnf));
       err.require(tarplan, mse.PLAN_TARGET_ALREADY_EXISTS);

       // get the source plan
       var plan = pln.get_production_plan_by_prod_line(bti, pri, lni);

       // set the target plan
       var reg = new production_plan(btf, prf, lnf);
       foreach (var p in plan) {
           reg.resource_ = p.resource_;
           reg.qty = p.qty;
           reg.plan_cst = p.plan_cst;
           reg.acum = p.acum;
           reg.band = p.band;
           pln.insert_production_plan(reg);
       }
   }

   /*protected void download_mold_inventory()
   {
      var l_strFilter="";
      var helper="";

      var batch =		values.get(key.CBATINI);
      var prodini =	values.get(key.CPRDINI);
      var prodfin =	values.get(key.CPRDFIN);
      var line =		values.get(key.CLININI);
      var filename =	values.get("cfileini");

      int l_iCont = 0;
      int value = 0;

      Dictionary<string, int> l_mapQty;
      Dictionary<int,string> l_mapCont;

      var curpath = basics.get(key.CURPATH);

      // generate a temporary file 
      COleDateTime now = COleDateTime::GetCurrentTime();
      var file ="";
      var target_file = "";
      file.Format(		_T("%d%d%d%d%.0f.txt"), 
                     now.GetDayOfYear(), now.GetHour(), now.GetMinute(), 
                     now.GetSecond(), ((double)clock()));
      target_file.Format(	_T("%s\\temp\\%s"), curpath, file);
      if(mro::exist_file(target_file)) _tremove(target_file);
      CUTF16File obj_file;
      obj_file.Open(target_file, CFile::modeWrite | CFile::modeCreate);

      CString key;
      // sumanos lo extra capturado en el plan molds 
      cCommand command(_basics);
      command.Format(	_T("select line_id, resource, acum from t_plan_molds with (nolock) ")
                  _T("where batch='X%s' and (prod_code between '%s' and '%s') and ")
                  _T("line_id='%s' order by resource"), 
                  batch.Mid(1), prodini, prodfin, line);
      getconnectionx(con, obj);
      con.execute(command, obj);
      ensure(obj.IsEOF(), _T("floor_inventory_not_exist"));

      for(;!obj.IsEOF(); obj.MoveNext())
      {
         key.Format(_T("%s%s"), obj.get(_T("line_id")), obj.get(_T("resource")));
         l_mapQty[key] = obj.getint(_T("acum"));
         l_mapCont[l_iCont++] = key;
      }

      // tambien sumanos el dia normal de trabajo 
      command.Format(	_T("select line_id, resource, qty from batch_detail with (nolock) ")
                  _T("where batch='%s' and (prod_code between '%s' and '%s') and ")
                  _T("line_id='%s' and location='%s'"), 
                  batch,prodini,prodfin,line,_T("CST"));
      con.execute(command, obj);
      for(;!obj.IsEOF(); obj.MoveNext())
      {
         key.Format(_T("%s%s"), obj.get(_T("line_id")), obj.get(_T("resource")));
         value = l_mapQty[key];
         l_mapQty[key] = value == 0 ? obj.getint(_T("qty")) : obj.getint(_T("qty")) + value;
      }
      ensure(l_iCont == 0, _T("floor_inventory_empty"));

      map<CString, bool> cache;
      map<CString, CString> plants;
      map<CString, CString> code_uncs;
      map<CString, CString> line_molds;
      map<CString, CString> loc_molds;

      CString l_strResource;
      int l_iQty;
      for(int f_iCont = 0; f_iCont < l_iCont;f_iCont++)
      {
         l_strResource = l_mapCont[f_iCont];
         l_iQty = l_mapQty[l_strResource];

         key.Format(_T("%s%s"), l_strResource.Mid(2,3),l_strResource.Mid(0,2));
         if(cache[key] == false)
         {
            // primero buscamos producto y linea
            command.Format(	_T("select prod_code_unc, line_mold, location_mold ")
                        _T("from trel_lineas_cst_mold with (nolock) ")
                        _T("where prod_code='%s' and line_cst='%s'"),
                        l_strResource.Mid(2,3),l_strResource.Mid(0,2));
            con.execute(command, obj);
            if(!obj.IsEOF()) 
            {
               plants[key] = _T("0200");
               code_uncs[key] = obj.get(_T("prod_code_unc"));
               line_molds[key] = obj.get(_T("line_mold"));
               loc_molds[key] = obj.get(_T("location_mold"));
            }
            else
            {
               command.Format(	_T("select prod_code_unc, location_mold ")
                           _T("from trel_lineas_cst_mold with (nolock) ")
                           _T("where prod_code='%s'"),
                           l_strResource.Mid(2,3));
               con.execute(command, obj);
               if(!obj.IsEOF()) 
               {
                  plants[key] = _T("0200");
                  code_uncs[key] = obj.get(_T("prod_code_unc"));
                  line_molds[key] = l_strResource.Mid(0,2);
                  loc_molds[key] = obj.get(_T("location_mold"));
               }
               else
               {
                  plants[key] = _T("XXXX");
                  code_uncs[key] = _T(" ") + l_strResource.Mid(2,3);
                  line_molds[key] = l_strResource.Mid(0,2);
                  loc_molds[key] = _T("  ");
               }
            }
            cache[key] = true;
         }

         helper.Format(_T("%s\t%s%s\t%s%s\t1\t%ld\r\n"), 
                     plants[key], code_uncs[key], l_strResource.Mid(5), 
                     line_molds[key], loc_molds[key], l_iQty);
   //		obj_file.write(l_strAux);
         obj_file.WriteString(helper);
      }
      obj_file.Close();


      //CString servername = synservice::GetMachineNumber();
      //int webport = _basics.getint(_T("webport"), 7);
      //if(webport == 0) webport = 80;
      //TCHAR server[1024];
      //mikefmt(server, _T("%s:%d"), servername, webport);

      var download			= new filedownload();
       download.server			= //server;
       download.folder			= "temp";
       download.file			= file;
       download.topath			= filename;
      download.to_result(result);

      var shl	= new shell();
      shl.shellpath = filename;
      shl.to_result(result);
   }*/

</script>

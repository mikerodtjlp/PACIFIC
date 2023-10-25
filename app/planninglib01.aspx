<%@ Import Namespace="sfc" %>
<%@ Import Namespace="sfc.BL" %>
<%@ Import Namespace="sfc.BO" %>

<!-- #include file="~/core.aspx" -->

<script runat="server" language="C#">

   //Contiene los metodos para la funcionalidad de seleccionar SKU's urgentes, utilizada por planeacion

   /**
    * Consulta los sku cargados desde el archivo para una familia y fecha seleccionados
    */
   protected void getFileLoaded() {
       DateTime pDIni = DateTime.MinValue;
       DateTime pDFin = DateTime.MinValue;

       //err.require(!validate.isvalidDate(pDIni), mse.WRONG_DATE_INI);
       //err.require(!validate.isvalidDate(pDFin), mse.WRONG_DATE_FIN);

       var pFini = values.get("$perfamini$");
       var pFfin = values.get("$perfamfin$");
       pDIni = DateTime.Parse(values.get("$dateini$"));
       pDFin = DateTime.Parse(values.get("$datefin$"));
       var pPlannner = values.get("$planner$");

       planning_BL pbl = bls.get_plnbl();
       List<item_urgent2> x = pbl.getFileLoaded(pFini, pFfin, pPlannner, pDIni, pDFin);

       int listid = values.getint(defs.ZLISTAF);
       var lr = new ListResponse(listid, 4, lnk.clie);
       var row = 0;
       foreach (var o in x) {
           lr.set_data(row, 'A', o.fam.id, 'B', o.sku.id, 'C', o.planner, 'D', o.creation_date.ToString(), '*', "1");
           ++row;
       }

       lr.set_rows(row);
       lr.pass_to_obj(result);
   }

   /**    
    * Inserta los sku que vienen en el archivo de excel
    */
   protected void insertSKU() {
       var selection = new mroJSON();
       values.get(defs.ZEXEDAT, selection);
       int total = selection.getint(defs.ZEXETOT);
       int cols = selection.getint(defs.ZEXECLS);
       string _user = basics.get(defs.ZUSERID);
       List<item_urgent2> file = new List<item_urgent2>();

       err.require(total == 0 || cols == 0, cme.FILE_EMPTY);
       try {
           for (int i = 2; i <= total; ++i) {
               char letter = 'A';
               item_urgent2 member = new item_urgent2();
               for (int j = 0; j < cols; ++j, ++letter) {
                   string helper = string.Format("{0}{1}", letter, i);
                   string value = selection.get(helper);

                   switch (j) {
                       case 0: member.fam.id = value; break;
                       case 1: member.sku.id = value; break;
                       default: break;
                   }
               }
               member.planner = _user;
               file.Add(member);
           }
       }
       catch (Exception e) {
           err.require(cme.FILE_INVALID);
       }
       finally {
       }

       planning_BL pBl = bls.get_plnbl();
       pBl.insertSKU(file);
   }

   /**    
    * Elimina los sku que pertenecen a la familia indicada
    */
   protected void deleteSkuFam() {
       var pFam = values.get("$family$");
       planning_BL pbl = bls.get_plnbl();
       pbl.deleteSkuFam(new family(pFam));
   }

   /**    
    * Elimina un sku en especifico
    */
   protected void deleteSkuSelected() {
       DateTime pCreation_date = DateTime.MinValue;

       var pFam = values.get(key.LCELLA);
       var pResource = values.get(key.LCELLB);
       var pPlanner = values.get(key.LCELLC);
       pCreation_date = DateTime.Parse(values.get(key.LCELLD));

       item_urgent2 sf = new item_urgent2();

       sf.fam.id = pFam;
       sf.sku.id = pResource;
       sf.planner = pPlanner;
       sf.creation_date = pCreation_date;

       planning_BL pbl = bls.get_plnbl();
       pbl.deleteSkuSelected(sf);
   }

   /**    
    * Consulta todas las familias que estan asociados a un usuario en particular
    */
   protected void getListFamilies() {
       var pPlannner = basics.get(defs.ZUSERID);
       planning_BL pbl = bls.get_plnbl();
       List<family> x = pbl.getListFamilies(pPlannner);

       int listid = values.getint(defs.ZLISTAF);
       var lr = new ListResponse(listid, 2, lnk.clie);
       var row = 0;

       foreach (var o in x) {
           lr.set_data(row, 'A', o.id, 'B', o.description, '*', "1");
           ++row;
       }

       lr.set_rows(row);
       lr.pass_to_obj(result);
   }
</script>

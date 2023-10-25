using System;
using System.Collections.Generic;
using System.Text;
using sfc.BO;
using sfc.DAL;
using mro;

namespace sfc.BL {
   /**
    * bussines logic for planning
    */
   public class planning_BL {
      public planning_BL(CParameters conns) {
         conns.get(defs.ZDFAULT, ref dbcode);
      }
      public readonly string dbcode = string.Empty;

      public production_order_m2o insert_production_order_m2o(production_order_m2o po) {
         throw new System.NotImplementedException();
      }
      public void delete_production_oder_m2o(production_order_m2o po) {
         throw new System.NotImplementedException();
      }
      public void update_production_order_m2o(production_order_m2o po) {
         throw new System.NotImplementedException();
      }
      public production_order_m2o get_production_order_m2o(string po) {
         using (var dal = planning_DAL.instance(dbcode)) {
            return dal.get_production_order_m2o(po);
         }
      }
      public production_order_m2o insert_production_order_m2o_detail(production_order_m2o pod) {
         throw new System.NotImplementedException();
      }
      public void update_production_order_m2o_detail(production_order_m2o pod) {
         throw new System.NotImplementedException();
      }
      public void delete_production_order_m2o_detail(production_order_m2o pod) {
         throw new System.NotImplementedException();
      }
      public void accumulate_production_order_m2o(string res_, int howmany) {
         err.require(res_ == string.Empty, mse.INC_DAT_BARCODE);

         using (var dal = manufacture_DAL.instance(dbcode)) {
            // we look for its sku if we have the barcode (10 digits == barcode)
            if (res_.Length == 10) {
               resource tofind = new resource();
               tofind.opc_bar_code = res_;
               resource res = dal.get_resource_by_barcode(tofind);
               res_ = res.id;
            }
         }
         using (var dal = planning_DAL.instance(dbcode)) {
            resource r = new resource();
            r.id = res_;
            production_order_m2o opo = dal.get_one_opened_po_m2o(r);
            if (opo != null) {
               opo.acum += howmany;
               dal.update_production_order_m2o(opo);
               if (opo.acum >= opo.qty) {
                  opo.finishdate = DateTime.Now;
                  opo.status = "R";
                  dal.update_production_order_m2o(opo);
               }
            }
            else {
               production_order_m2o npo = dal.get_one_new_po_m2o(r);
               if (npo != null) {
                  npo.acum += howmany;
                  npo.status = "W";
                  dal.update_production_order_m2o(npo);
                  if (npo.acum >= npo.qty) {
                     npo.finishdate = DateTime.Now;
                     npo.status = "R";
                     dal.update_production_order_m2o(npo);
                  }
               }
               else {
                  production_order_m2o rpo = dal.get_one_released_po_m2o(r);
                  if (rpo != null) {
                     rpo.excess += 1;
                     dal.update_production_order_m2o(rpo);
                  }
               }
            }
         }
      }

      public bool check_sku_for_pull(string res_) {
         err.require(res_ == string.Empty, mse.INC_DAT_BARCODE);

         using (var dal = manufacture_DAL.instance(dbcode)) {
            if (res_.Length == 10) {
               resource tofind = new resource();
               tofind.opc_bar_code = res_;
               resource res = dal.get_resource_by_barcode(tofind);
               res_ = res.id;
            }
         }
         using (var dal = planning_DAL.instance(dbcode)) {
            resource r = new resource();
            r.id = res_;

            int opens = dal.get_open_orders_m2o_count(r);
            int news = dal.get_new_orders_m2o_count(r);

            return opens == 0 && news == 0;
         }
      }

      public List<product> get_products_planned_by_line(lot lt, line ln) {
         err.require(lt.id == null || lt.id == "", mse.INC_DAT_BATCH);
         err.require(ln.id == null || ln.id == "", mse.INC_DAT_LINE);

         using (var dal = planning_DAL.instance(dbcode)) {
            return dal.get_products_planned_by_line(lt, ln);
         }
      }
      public List<production_plan> get_production_plan_by_line(lot lt, line ln) {
         err.require(lt.id == null || lt.id == "", mse.INC_DAT_BATCH);
         err.require(ln.id == null || ln.id == "", mse.INC_DAT_LINE);

         using (var dal = planning_DAL.instance(dbcode)) {
            return dal.get_production_plan_by_line(lt, ln);
         }
      }
      public List<production_plan> get_production_plan_by_prod_line
                                      (lot lt, product pr, line ln) {
         err.require(lt.id == null || lt.id == "", mse.INC_DAT_BATCH);
         err.require(pr.id == null || pr.id == "", mse.INC_DAT_PROD);
         err.require(ln.id == null || ln.id == "", mse.INC_DAT_LINE);

         using (var dal = planning_DAL.instance(dbcode)) {
            return dal.get_production_plan_by_prod_line(lt, pr, ln);
         }
      }

      public production_order get_one_po_opened(resource res_) {
         using (var dal = planning_DAL.instance(dbcode)) {
            return dal.get_one_po_opened(res_);
         }
      }
      public void update_production_order(production_order po) {
         using (var dal = planning_DAL.instance(dbcode)) {
            dal.update_production_order(po);
         }
      }

      #region plan molds
      public void accumulate_production_plan(lot l, product p, line li, resource r, int q) {
         using (var dal = planning_DAL.instance(dbcode)) {
            var pln = get_production_plan(new production_plan(l, p, li, r));
            if (pln != null && pln.band == 0) {
               pln.acum += q;
               if (pln.acum >= pln.qty) pln.band = 1;
               dal.update_production_plan(pln);
            }
         }
      }

      public bool ispull(lot l, product p, line i, resource r) {
         err.require(l == null || l.id == "", mse.INC_DAT_BATCH);
         err.require(p == null || p.id == "", mse.INC_DAT_PROD);
         err.require(i == null || i.id == "", mse.INC_DAT_LINE);
         err.require(r == null || r.id == "", mse.INC_DAT_SKU);
         using (var dal = planning_DAL.instance(dbcode)) {
            var pln = dal.get_production_plan(new production_plan(l, p, i, r));
            if (pln != null) {
               return pln.band == 1;
            }
            return true;
         }
      }
      public bool exist_production_plan_any(production_plan p) {
         p.validate();
         using (var dal = planning_DAL.instance(dbcode)) {
            return dal.exist_production_plan_any(p);
         }
      }
      public bool exist_production_plan_detail(production_plan p) {
         p.validate();
         err.require(p.resource_ == null || p.resource_.id == "", mse.INC_DAT_SKU);
         using (var dal = planning_DAL.instance(dbcode)) {
            return dal.exist_production_plan_detail(p);
         }
      }
      public production_plan get_production_plan(batch b, resource r) {
         return get_production_plan(new production_plan(b.lotno, b.product, b.line, r));
      }
      public production_plan get_production_plan(lot l, product p, line li, resource r) {
         return get_production_plan(new production_plan(l, p, li, r));
      }
      public production_plan get_production_plan(production_plan p) {
         p.validate();
         err.require(p.resource_ == null || p.resource_.id == "", mse.INC_DAT_SKU);
         using (var dal = planning_DAL.instance(dbcode)) {
            return dal.get_production_plan(p);
         }
      }
      public void insert_production_plan(production_plan p) {
         p.validate();
         err.require(p.resource_ == null || p.resource_.id == "", mse.INC_DAT_SKU);
         using (var dal = planning_DAL.instance(dbcode)) {
            dal.insert_production_plan(p);
         }
      }
      public void update_production_plan(production_plan p) {
         p.validate();
         err.require(p.resource_ == null || p.resource_.id == "", mse.INC_DAT_SKU);
         using (var dal = planning_DAL.instance(dbcode)) {
            dal.update_production_plan(p);
         }
      }
      public void delete_production_plan(production_plan p) {
         p.validate();
         err.require(p.resource_ == null || p.resource_.id == "", mse.INC_DAT_SKU);
         using (var dal = planning_DAL.instance(dbcode)) {
            dal.delete_production_plan(p);
         }
      }
      #endregion

      #region urgentSKU
      /**
       * Consulta los sku cargados desde el archivo para una familia y fecha seleccionados
       */
      public List<item_urgent2> getFileLoaded(string famIni, string famFin, string planner,
                                              DateTime dateIni, DateTime dateFin) {
         if (famIni == null || string.IsNullOrEmpty(famIni)) famIni = "A";
         if (famFin == null || string.IsNullOrEmpty(famFin)) famFin = "Z";
         if (planner == null || string.IsNullOrEmpty(planner)) planner = "%";

         dateIni = dateIni.AddHours(00).AddMinutes(00).AddSeconds(01);
         dateFin = dateFin.AddHours(23).AddMinutes(59).AddSeconds(59);

         using (planning_DAL pdal = planning_DAL.instance(dbcode)) {
            List<item_urgent2> result = pdal.getSkuFileMembersLoaded(famIni, famFin, planner, dateIni, dateFin);
            return result;
         }
      }

      /**
       * Inserta los sku que vienen en el archivo de excel.
       */
      public void insertSKU(List<item_urgent2> fileMembers) {

         err.require(fileMembers == null || fileMembers.Count == 0, cme.FILE_NOT_EXIST);
         //if (fileMembers == null || fileMembers.Count == 0)
         //  throw new Exception("There is no file to process");

         using (planning_DAL pDal = planning_DAL.instance(dbcode)) {
            try {
               bool iFlag = false;
               pDal.begin_transaction();
               foreach (item_urgent2 x in fileMembers) {
                  //valida si el SKU y las Familia son Correctos                      
                  this.validSkuFamily(x, pDal);

                  //Valida que el SKU No EXISTA en Tabla1.
                  if (!this.existSKU(x.sku, pDal)) {
                     iFlag = true; // se marca true para indicar que almenos 1 registro de inserto
                     pDal.insertSKU(x.sku);
                  }
                  //Valida que el SKU No EXISTA en Tabla2.
                  if (!this.existSKUFam(x, pDal)) {
                     iFlag = true; // se marca true para indicar que almenos 1 registro de inserto
                     pDal.insertSKUFam(x);
                  }
               }
               //si no se inserto nada se manda un error de que ya todos existian.
               err.require(!iFlag, mse.SKU_ALREADY_EXIST);
               //if (!iFlag)
               //  throw new Exception("Ya existen los Skus Cargados");

               pDal.commit_transaction();
            }
            catch (Exception ex) {
               pDal.rollback_transaction();
               throw ex;
            }
         }
      }

      /// <summary>
      /// Elimina un sku en especifico
      /// </summary>
      /// <param name="file"></param>
      /// <returns></returns>
      public void deleteSkuSelected(item_urgent2 file) {
         err.require(file == null, "param_not_passed");
         err.require(file.sku.id == null, "param_not_passed");
         //if (file == null) throw new Exception("Parameter not passed");
         //if (file.sku.resource_ == null) throw new Exception("Parameter not passed");

         using (planning_DAL pdal = planning_DAL.instance(dbcode)) {
            err.require(pdal.deleteSkuSelected(file) == 0, mse.SKU_NOT_FOUND);
            //if (pdal.deleteSkuSelected(file) == 0)
            //  throw new Exception("Selected SKU no found in database");
         }
      }

      /// <summary>        
      /// Elimina los sku que pertenecen a la familia indicada
      /// </summary>
      /// <param name="file"></param>
      /// <returns></returns>
      public void deleteSkuFam(family sFam) {
         int re = -1;
         int re2 = -1;

         err.require(string.IsNullOrEmpty(sFam.id), mse.INC_DAT_FAM);
         //if (string.IsNullOrEmpty(sFam))
         //  throw new Exception("please provide a family");

         using (planning_DAL pdal = planning_DAL.instance(dbcode)) {
            try {
               pdal.begin_transaction();

               re = pdal.deleteSku(sFam); //Borra los SKUs. Crear este metodo en el DAL
               re2 = pdal.deleteSkuFam(sFam); //Borra los SKU con Familia asociada
               err.require(re == 0 && re2 == 0, "not_sku_fam");
               /* if (re == 0 && re2 == 0)
                {
                    throw new Exception("There aren't sku to delete from family: " + sFam);
                }*/
               pdal.commit_transaction();
            }
            catch (Exception ex) {
               pdal.rollback_transaction();
               throw ex;
            }
         }

      }

      /// <summary>
      /// Valida que el sku pertenezca a la familia que tiene asignado el sku
      /// </summary>
      /// <param name="file"></param>
      /// <returns></returns>
      private bool validSkuFamily(item_urgent2 file, planning_DAL pDal) {
         family fam = pDal.getProdFamily(file.fam, new product(file.sku.prod_code));
         err.require(fam.id.Equals(string.Empty), "not_prod_fam");
         return true;
      }

      /// <summary>
      /// Valida si existe o no el sku en las tablas
      /// </summary>
      /// <param name="file"></param>
      /// <returns></returns>
      private bool existSKU(resource sku, planning_DAL pDal) {
         var x = pDal.getSKU(sku);
         return (x != null && x.Count > 0);
      }

      private bool existSKUFam(item_urgent2 file, planning_DAL pDal) {
         var x = pDal.getSKUFam(file);
         return (x != null && x.Count > 0);
      }
      /// <summary>
      /// Valida si la familia a eliminar contiene por lo menos un sku 
      /// </summary>
      /// <param name="file"></param>
      /// <returns>Lista de SKU que pertenecen a la familia</returns>
      public List<SkuFileMember> getFamDtl(String sFam) {
         throw new NotImplementedException();
      }

      /// <summary>
      /// Consulta las familias de un usuario en especifico
      /// </summary>
      /// <param name="file">userid</param>
      /// <returns>Lista de familias</returns>
      public List<family> getListFamilies(string userId) {
         err.require(userId.Equals("") || userId.Equals(null), mse.INC_DAT_USER);
         using (planning_DAL pdal = planning_DAL.instance(dbcode)) {
            List<family> result = pdal.getListFamilies(userId);
            return result;
         }
      }

      #endregion
   }
}

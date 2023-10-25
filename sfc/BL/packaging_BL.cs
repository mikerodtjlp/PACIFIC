using System;
using System.Collections.Generic;
using System.Text;
using sfc.BO;
using sfc.DAL;
using mro;

namespace sfc.BL {
   /**
    * bussines logic for packaging process in particular
    */
   public class packaging_BL {
      public packaging_BL(CParameters conns) {
         conns.get(defs.ZDFAULT, ref dbcode);
      }
      public readonly string dbcode = string.Empty;

      public string insert_and_return_bulk_pack(batch b, resource res, int q, int m) {
         b.validate();
         err.require(res == null || res.id.Length == 0, mse.INC_DAT_SKU);
         using (var dal = packaging_DAL.instance(dbcode)) {
            return dal.insert_and_return_bulk_pack(b, res, q, m);
         }
      }

      public lot_number generate_shipment(batch b, location l, string lnum) {
         b.validate();
         err.require(l == null || l.id == "", mse.INC_DAT_LOC);
         using (var dal = packaging_DAL.instance(dbcode)) {
            return dal.generate_shipment(b, l, lnum);
         }
      }

      public string close_trans_batch(batch b, location l) {
         b.validate();
         err.require(l == null || l.id == "", mse.INC_DAT_LOC);
         using (var dal = packaging_DAL.instance(dbcode)) {
            return dal.close_trans_batch(b, l);
         }
      }

      public box_status authorize_box(bulk_pack_upload box2find, bool autorization, bool unupload) {
         var box = get_box_to_upload(box2find);
         if (box == null && autorization) {
            box = new bulk_pack_upload(box2find.lotno, box2find.product, box2find.line, box2find.part);
            box.bulk_pack_id = box2find.bulk_pack_id;
            box.status = box_status.AUTHORIZED;
            insert_box_to_upload(box);
            return new box_status(box_status.AUTHORIZED);
         }
         else {
            // only authorize and temporary are the status that can be changed
            // the cancel and, delete, and upload status cannot be changed
            if (autorization && (box.status == box_status.AUTHORIZED || box.status == box_status.PENDING)) {
               var newstatus = string.Empty;
               if (box.status == box_status.AUTHORIZED) newstatus = box_status.PENDING;
               else if (box.status == box_status.PENDING) newstatus = box_status.AUTHORIZED;
               box.status = newstatus;
               update_box_to_upload(box);
               return new box_status(newstatus == box_status.PENDING ? box_status.PENDING :
                                                                       box_status.AUTHORIZED);
            }
            if (unupload && (box.status == box_status.UPLOADED || box.status == box_status.PENDING)) {
               var newstatus = string.Empty;
               if (box.status == box_status.UPLOADED) newstatus = box_status.PENDING;
               else if (box.status == box_status.PENDING) newstatus = box_status.UPLOADED;
               box.status = newstatus;
               update_box_to_upload(box);
               return new box_status(newstatus == box_status.PENDING ? box_status.PENDING :
                                                                       box_status.UPLOADED);
            }
            return new box_status(string.Empty);
         }
      }
      public bulk_pack_upload get_box_to_upload(bulk_pack_upload b) {
         b.validate();
         using (var dal = packaging_DAL.instance(dbcode)) {
            return dal.get_box_to_upload(b);
         }
      }
      public void insert_box_to_upload(bulk_pack_upload b) {
         b.validate();
         using (var dal = packaging_DAL.instance(dbcode)) {
            dal.insert_box_to_upload(b);
         }
      }
      public void update_box_to_upload(bulk_pack_upload b) {
         b.validate();
         using (var dal = packaging_DAL.instance(dbcode)) {
            dal.update_box_to_upload(b);
         }
      }

      public bulk_pack_dtl get_bulk_pack_dtl_by_id(string b) {
         using (var dal = packaging_DAL.instance(dbcode)) {
            return dal.get_bulk_pack_dtl_by_id(b);
         }
      }
      public void insert_bulk_pack_dtl(bulk_pack_dtl b) {
         b.validate();
         using (var dal = packaging_DAL.instance(dbcode)) {
            dal.insert_bulk_pack_dtl(b);
         }
      }
      public void update_bulk_pack_dtl(bulk_pack_dtl b) {
         b.validate();
         using (var dal = packaging_DAL.instance(dbcode)) {
            dal.update_bulk_pack_dtl(b);
         }
      }

      #region package image

      /**
       * Se encarga de consultar la imagen que le corresponde al sku
       * <param name="ba">contiene el batch, linea y sku que se debe buscar</param>
       * <returns>Retorna el nombre del archivo o NO en caso de que se encuentre o no</returns>
       */
      public String getImage(lot lt, line ln, resource sku) {
         string resultado = string.Empty;
         int countRel = 0;

         //if el batch esta nulo enviar error al web service
         err.require(lt.id == string.Empty, mse.INC_DAT_BATCH);
         // si la linea origen esta vacia enviar error al web service
         err.require(ln.id == string.Empty, mse.INC_DAT_LINE);
         // si la linea destino esta vacia enviar error al web service
         err.require(sku.id == string.Empty, mse.INC_DAT_SKU);

         if (sku.id.Contains(".")) {
            using (packaging_DAL pdal = packaging_DAL.instance(dbcode)) {
               //getPlanned
               //if not planned 
               if (pdal.getPlanned(lt, ln, sku).Equals("NO")) {
                  //return error
                  resultado = "noplanned.jpg"; //"El sku: " + ba.sku.resource_ + " no esta planeado";
               }
               else //else
               {
                  //verifica si ya se creo la relacion con todo el plan
                  countRel = Convert.ToInt32(pdal.getSkuRelCount(lt, ln));

                  if (countRel == 0) {
                     //insert sku image todo el plan
                     pdal.insertSkuImgRel(lt, ln, sku, "all");
                     //getImage
                     resultado = pdal.getImage(lt, ln, sku);
                  }//end if
                   //verifica si existe el sku en la relacion
                   //if not existSku
                  if (pdal.existSkuOnRel(lt, ln, sku).Equals("NO")) {

                     if (countRel >= 30) {
                        //retorna error
                        resultado = "reemplazo.jpg"; //"Debe reemplazar el sku: " + ba.sku.resource_;
                     }
                     else {
                        //inserta sku en la ultima posicion de la relacion
                        pdal.insertSkuImgRel(lt, ln, sku, "last");
                        //getImage
                        resultado = pdal.getImage(lt, ln, sku);
                     }//end >=30
                  }
                  else {
                     //getImage
                     resultado = pdal.getImage(lt, ln, sku);
                  }//end else if not existSku

               }//end else if not planned                                                
            }
         }//end sku valido

         if (resultado.Equals("")) resultado = "error.jpg"; //throw new Exception("No existe imagen para el resource: " + ba.sku.resource_);                    


         return resultado;

      }

      /**
       * Se encarga de consultar la informacion inicial para llenar las tablas de datos
       * <param name="img">contiene el lote y linea</param>
       * <returns>Retorna la informacion de nuevos sku y sus imagenes relacionadas</returns>
       */
      public Tuple<List<string>, List<string>> getRelationInformation(lot lt, line ln) {
         Tuple<List<string>, List<string>> image = Tuple.Create(new List<string>(), new List<string>());
         //if el batch esta nulo enviar error al web service
         err.require(lt.id == string.Empty, mse.INC_DAT_BATCH);
         // si la linea origen esta vacia enviar error al web service
         err.require(ln.id == string.Empty, mse.INC_DAT_LINE);
         using (packaging_DAL pdal = packaging_DAL.instance(dbcode)) {
            image.Item1.Add(pdal.getNewSku(lt, ln).Item3.id);
            image.Item2.Add(pdal.getRelImgSku(lt, ln).Item4.img);
         }
         return image;
      }

      /**
       * Actualiza un sku por otro en la tabla de relacion de image-sku
       * <param name="img">contiene el lote,linea, sku nuevo y anterior</param>
       * <returns>No retorna informacion</returns> 
       */
      public void updateSku(lot lt, line ln, resource sku, string sku_new) {
         //if el batch esta nulo enviar error al web service
         err.require(lt.id == string.Empty, mse.INC_DAT_BATCH);
         // si la linea origen esta vacia enviar error al web service
         err.require(ln.id == string.Empty, mse.INC_DAT_LINE);
         // si la linea destino esta vacia enviar error al web service
         err.require(sku.id == string.Empty, mse.INC_DAT_SKU);
         err.require(sku_new == string.Empty, mse.NEW_SKU_CANNOT_BE_NULL);

         string sku_ant = sku.id;

         //getPlanned                     

         using (packaging_DAL pdal = packaging_DAL.instance(dbcode)) {
            sku.id = sku_new;
            err.require(pdal.getPlanned(lt, ln, sku).Equals("NO"), mse.SKU_NOT_PLANNED);
            sku.id = sku_ant;
            pdal.updateSku(lt, ln, sku, sku_new);
         }
      }
      #endregion
   }
}

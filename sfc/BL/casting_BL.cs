using System;
using System.Collections.Generic;
using System.Linq;
using sfc.BO;
using sfc.DAL;
using mro;
using mro.BO;

namespace sfc.BL {
   /**
    * bussines logic for casting process in particular
    */
   public class casting_BL {
      public casting_BL(CParameters conns) {
         conns.get(defs.ZDFAULT, ref dbcode);
      }
      public readonly string dbcode = string.Empty;

      public bool exist_sku_produced(batch b, resource sku) {
         using (var dal = casting_DAL.instance(dbcode)) {
            return dal.exist_sku_produced(b, sku);
         }
      }

      #region inventory
      public void accumulate_production_inventory(lot l, product p, line li, resource r, int q) {
         using (var dal = casting_DAL.instance(dbcode)) {
            var inv = get_production_inventory(new production_inventory(l, p, li, r));
            if (inv != null) {
               inv.qty += q;
               dal.update_production_inventory(inv);
            }
            else {
               var newinv = new production_inventory(l, p, li, r);
               newinv.date = DateTime.Now;
               dal.insert_production_inventory(newinv);
            }
         }
      }

      public production_inventory get_production_inventory(lot l, product p, line li, resource r) {
         return get_production_inventory(new production_inventory(l, p, li, r));
      }
      public production_inventory get_production_inventory(production_inventory p) {
         p.validate();
         err.require(p.resource_ == null || p.resource_.id.Length == 0, mse.INC_DAT_SKU);
         using (var dal = casting_DAL.instance(dbcode)) {
            return dal.get_production_inventory(p);
         }
      }
      public void insert_production_inventory(production_inventory p) {
         p.validate();
         err.require(p.resource_ == null || p.resource_.id.Length == 0, mse.INC_DAT_SKU);
         using (var dal = casting_DAL.instance(dbcode)) {
            dal.insert_production_inventory(p);
         }
      }
      public void update_production_inventory(production_inventory p) {
         p.validate();
         err.require(p.resource_ == null || p.resource_.id.Length == 0, mse.INC_DAT_SKU);
         using (var dal = casting_DAL.instance(dbcode)) {
            dal.update_production_inventory(p);
         }
      }
      public void delete_production_inventory(production_inventory p) {
         p.validate();
         err.require(p.resource_ == null || p.resource_.id.Length == 0, mse.INC_DAT_SKU);
         using (var dal = casting_DAL.instance(dbcode)) {
            dal.delete_production_inventory(p);
         }
      }
      #endregion

      #region product molds relation
      public List<product_mold_relation> get_product_mold_relation_all(product p) {
         p.validate();
         using (var dal = casting_DAL.instance(dbcode)) {
            return dal.get_product_mold_relation_all(p);
         }
      }
      public product_mold_relation get_product_mold_relation(product_mold_relation p) {
         using (var dal = casting_DAL.instance(dbcode)) {
            return dal.get_product_mold_relation(p);
         }
      }
      public void insert_product_mold_relation(product_mold_relation p) {
         p.validate();
         using (var dal = casting_DAL.instance(dbcode)) {
            dal.insert_product_mold_relation(p);
         }
      }
      public void update_product_mold_relation(product_mold_relation p) {
         p.validate();
         using (var dal = casting_DAL.instance(dbcode)) {
            dal.update_product_mold_relation(p);
         }
      }
      public void delete_product_mold_relation(product_mold_relation p) {
         p.validate();
         using (var dal = casting_DAL.instance(dbcode)) {
            dal.delete_product_mold_relation(p);
         }
      }
      #endregion

      #region match mold lent
      /**
       * Obtiene las revisiones generadas y las ordena para mostrarlas sin repetir.
       * <param name="ba">contiene el batch y la linea en el objeto batch</param>
       * <returns>Retorna las revisiones ordenadas en el objeto mold_lent_match</returns>
       */
      public List<mold_lent_match> getMoldLentMatches(batch ba) {
         //var datosReturn = "";
         List<mold_lent_match> data = new List<mold_lent_match>();

         //if el batch esta nulo enviar error al web service
         err.require(ba == null, mse.INC_DAT_BATCH);

         // si la linea esta vacia enviar error al web service
         err.require(ba.line == null, mse.INC_DAT_LINE);
         bool existeReg = false;

         using (casting_DAL cdal = casting_DAL.instance(dbcode)) {
            List<mold_lent_match> datosMatch = cdal.getMoldLentMatches(ba);
            existeReg = false;
            string[] pallet = new string[1];

            foreach (var dato in datosMatch) {

               string[] dat = new string[11];
               string pall = Convert.ToInt32(dato.pallet.id).ToString();

               if (dato.campo != 0) {
                  if (!pallet.Contains(pall)) {
                     dat[0] = Convert.ToString(dato.sku.id);
                     dat[1] = Convert.ToInt32(dato.pallet.id).ToString();
                     dat[2] = Convert.ToString(dato.modulo);
                     dat[3] = Convert.ToString(dato.molde.id);
                     dat[4] = Convert.ToInt32(dato.moldIdx).ToString();
                     existeReg = true;
                     pallet[0] = pall;
                  }
                  else {
                     existeReg = false;
                  }

                  if (existeReg == true) {
                     foreach (var d in datosMatch) {
                        if (d.modulo == dato.modulo && d.molde.id == dato.molde.id
                            && d.moldIdx == dato.moldIdx && d.pallet.id == dato.pallet.id && d.sku.id == dato.sku.id) {
                           if (d.campo == 1) {
                              dat[5] = Convert.ToString(d.validacion);
                              dat[9] = d._img.ToString();
                              dat[10] = d.barcode_number.ToString();
                           }
                           if (d.campo == 2) {
                              dat[6] = Convert.ToString(d.validacion);
                              dat[9] = d._img.ToString();
                              dat[10] = d.barcode_number.ToString();
                           }
                           if (d.campo == 3) {
                              dat[7] = Convert.ToString(d.validacion);
                              dat[9] = d._img.ToString();
                              dat[10] = d.barcode_number.ToString();
                           }
                           if (d.campo == 4) {
                              dat[8] = Convert.ToString(d.validacion);
                              dat[9] = d._img.ToString();
                              dat[10] = d.barcode_number.ToString();
                           }

                        }

                     }

                     data.Add(populateMold_lent_matchFromArray(dat));
                  }
               }
               else {
                  if (dato.campo == 0) {
                     //Aqui lee los registros que no se les ha echo ninguna validacion
                     dat[0] = Convert.ToString(dato.sku.id);
                     dat[1] = Convert.ToInt32(dato.pallet.id).ToString();
                     dat[2] = Convert.ToString(dato.modulo);
                     dat[3] = Convert.ToString(dato.molde.id);
                     dat[4] = Convert.ToInt32(dato.moldIdx).ToString();
                     dat[5] = Convert.ToString("");
                     dat[6] = Convert.ToString("");
                     dat[7] = Convert.ToString("");
                     dat[8] = Convert.ToString("");
                     dat[9] = dato._img.ToString();
                     dat[10] = Convert.ToString(0);
                     data.Add(populateMold_lent_matchFromArray(dat));
                  }
               }
            }
         }
         return data;
      }

      /**
       * Se encarga de asignar la informacion del arreglo de revisiones al objeto mold_lent_match
       * <param name="dat">Arreglo de revisiones</param>
       * <returns>Retorna las revisiones en el objeto mold_lent_match</returns>
       */
      protected virtual mold_lent_match populateMold_lent_matchFromArray(string[] dat) {
         mold_lent_match m = new mold_lent_match();
         m.sku.id = dat[0];
         m.pallet.id = dat[1];
         m.modulo = dat[2];
         m.molde.id = dat[3];
         m.moldIdx = Convert.ToInt32(dat[4]);
         m.campo1 = dat[5];
         m.campo2 = dat[6];
         m.campo3 = dat[7];
         m.campo4 = dat[8];
         m._img = Convert.ToInt32(dat[9]);
         m.barcode_number = dat[10];

         return m;
      }

      /**
       * Valida si el indice introducido corresponde a la relacion molde-sku y llama el metodo que inserta el resultado de la revision
       * </summary>
       * <param name="datos">objeto mold_lent_match donde se encuentra la informacion seleccionada</param>
       * <returns>retorna el resultado de la revision y el resultado de la insercion</returns>
       */
      public List<mold_lent_match> validateMoldLent(mold_lent_match datos) {
         string resultValidacion = string.Empty;
         List<mold_lent_match> data = new List<mold_lent_match>();
         using (casting_DAL cdal = casting_DAL.instance(dbcode)) {
            resultValidacion = cdal.validateMoldLent(datos);
            List<mold_lent_match> resultInsert = cdal.insertMoldLentRelation(datos, resultValidacion);
            mold_lent_match show = new mold_lent_match();

            foreach (var val in resultInsert) {
               show.resultado = val.resultado.Trim();
               show.showmsg = val.showmsg;

            }
            data.Add(show);
         }

         return data;
      }

      /**
       * Valida que el campo aun se pueda modificar ya que solo se permiten dos intentos.
       * <param name="datos">objeto mold_lent_match donde se encuentra la informacion seleccionada</param>
       * <returns>Retorna el valor introducido y el resultado de la validacion en el objeto mold_lent_match</returns>
       */
      public List<mold_lent_match> validEditField_MoldLent(mold_lent_match datos) {

         int campo = 0;
         string valor_introducido = string.Empty;

         if (datos._1 != datos.d) {
            campo = 1;
            valor_introducido = datos._1;
         }
         else {
            if (datos._2 != datos.e) {
               campo = 2;
               valor_introducido = datos._2;
            }
            else {
               if (datos._3 != datos.f) {
                  campo = 3;
                  valor_introducido = datos._3;
               }
               else {
                  campo = 4;
                  valor_introducido = datos._4;
               }
            }
         }

         datos.campo = campo;
         datos.valor_introducido = valor_introducido;

         using (casting_DAL cdal = casting_DAL.instance(dbcode)) {
            var validacion = cdal.validEditField_MoldLent(datos);
            return validacion;
         }
      }

      /**
       * Inserta el resultado de la revision
       * <param name="datos">recibe los datos en el objeto mold_lent_match y la validacion</param>
       * <param name="validacion"></param>
       * <returns></returns>
       */
      public List<mold_lent_match> insertMoldLentRelation(mold_lent_match datos, string validacion) {
         using (casting_DAL cdal = casting_DAL.instance(dbcode)) {
            var resultado = cdal.insertMoldLentRelation(datos, validacion);

            return resultado;
         }
      }

      /**
       * Verifica que el dato modificado no haya sido ya validado como OK
       * <param name="datos">recibe los datos seleccionados en el objeto mold_lent_match</param>
       * <returns>retorna el valor validado anteriormente</returns>
       */
      public string validaDatoAnterior(mold_lent_match datos) {
         int campo = 0;
         string valor_introducido = string.Empty;
         string valor_anterior = string.Empty;

         if (datos._1 != datos.d) {
            campo = 1;
            valor_introducido = datos._1;
            valor_anterior = datos.d;
         }
         else {
            if (datos._2 != datos.e) {
               campo = 2;
               valor_introducido = datos._2;
               valor_anterior = datos.e;
            }
            else {
               if (datos._3 != datos.f) {
                  campo = 3;
                  valor_introducido = datos._3;
                  valor_anterior = datos.f;
               }
               else {
                  campo = 4;
                  valor_introducido = datos._4;
                  valor_anterior = datos.g;
               }
            }
         }
         return valor_anterior;
      }

      /**
       * consulta el sku de un molde
       * <param name="datos">recibe los datos seleccionados en el objeto mold_lent_match</param>
       * <returns>retorna el sku</returns>
       */
      public string getMoldSku(mold_lent_match datos) {
         using (casting_DAL cdal = casting_DAL.instance(dbcode)) {
            var resultado = cdal.getMoldSku(datos);

            return resultado;
         }
      }


      #endregion

      #region captura de moldes pulidos
      /**
       * Obtiene la lista de moldes que se han capturado para un lote, linea y producto especifico
       * <param name="bh"></param>
       */
      public List<mold> getPolishedMoulds(batch_header bh, mold ml) {

         validDefaultRequiredFields(bh);

         List<mold> data = new List<mold>();

         using (casting_DAL cdal = casting_DAL.instance(dbcode)) {
            err.require(getBatchHeader(bh) == 0, mse.BATCH_NOT_EXIST);

            data = cdal.getPolishedMoulds(bh, ml);

         }

         return data;

      }
      /**
       * Se encarga de insertar un molde con cantidad 1
       * <param name="bh"></param>
       */
      public void insertPolishedMoulds(batch_header bh, operador op, mold m) {
         int exist_mould = 0;

         validDefaultRequiredFields(bh);
         validDefaultRequiredFieldsDtl(m);
         validOtherRequiredFields(op);



         using (casting_DAL cdal = casting_DAL.instance(dbcode)) {
            err.require(getBatchHeader(bh) == 0, mse.BATCH_NOT_EXIST);
            err.require(getBatchStatus(bh).Equals(status.REL), mse.BATCH_NOT_STATUS);

            exist_mould = getExistMould(bh, m);

            if (exist_mould == 0) {

               cdal.insertPolishedMould(bh, op, m);
            }
            else {
               m.qty = 1;
               cdal.updatePolishedMould(bh, m, "agregar");
            }


         }
      }

      /**
       * Elimina todo el registro de un molde en especifico sin importar la cantidad
       */
      public void deletePolishedMoulds(batch_header bh, mold m) {
         validDefaultRequiredFields(bh);
         validDefaultRequiredFieldsDtl(m);

         err.require(getBatchStatus(bh).Equals(status.REL), mse.BATCH_NOT_STATUS);

         using (casting_DAL cdal = casting_DAL.instance(dbcode)) {
            cdal.deletePolishedMoulds(bh, m);
         }
      }

      /**
       * Crea el registro en el batch header para el location recibido
       */
      public void insertBatchHeader(batch_header bh) {

         validDefaultRequiredFields(bh);

         using (manufacture_DAL mdal = manufacture_DAL.instance(dbcode)) {
            if (getBatchHeader(bh) == 0) {
               bh.status = status.WIP;
               bh.new_pkg_count = "0";
               bh.as_400 = "0";
               bh.qty_in = "0";
               bh.qc_audit = "0";
               bh.comments = "molde pulido";

               mdal.insert_batch_header(bh);
            }

         }

      }

      /**
       * Cambia de estatus el batch a RELEASE
       * <param name="bh"></param>
       * <param name="bd"></param>
       */
      public void releasePolishedMoulds(batch_header bh, mold m) {

         validDefaultRequiredFields(bh);

         // if (getBatchStatus(bh).Equals(val.RELEASE)) throw new Exception("capture already RELEASE");
         err.require(getBatchStatus(bh).Equals(status.REL), mse.BATCH_IS_RELEASED);

         using (casting_DAL cdal = casting_DAL.instance(dbcode)) {
            if (getBatchHeader(bh) != 0) {
               cdal.updateBatchStatus(bh, m);
            }

         }
      }

      /**
       * Actualiza la cantidad de moldes leidos, aumentandolo en 1.
       * <param name="bh"></param>
       * <param name="bd"></param>
       * <param name="action"></param>
       */
      public void updatePolishedMould(batch_header bh, mold m, string action) {

         validDefaultRequiredFields(bh);
         validDefaultRequiredFieldsDtl(m);

         err.require(getBatchStatus(bh).Equals(status.REL), mse.BATCH_NOT_STATUS);

         using (casting_DAL cdal = casting_DAL.instance(dbcode)) {

            if (m.qty == 1) {
               cdal.deletePolishedMoulds(bh, m);
            }
            else {
               m.qty = 1;
               cdal.updatePolishedMould(bh, m, action);
            }
         }
      }

      /**
       * valida los campos requeridos
       */
      public void validDefaultRequiredFields(batch_header bh) {
         err.require(bh.lotno.id == string.Empty, mse.INC_DAT_BATCH);
         err.require(bh.line.id == string.Empty, mse.INC_DAT_LINE);
         err.require(bh.product.id == string.Empty, mse.INC_DAT_PROD);
         err.require(bh.part == string.Empty, mse.INC_DAT_PART);
         err.require(bh.location.id == string.Empty, mse.INC_DAT_LOC);
         err.require(bh.cycle.Equals(0), mse.INC_DAT_CYCLE);
      }

      /**
       * valida los campos requeridos del detalle
       */
      public void validDefaultRequiredFieldsDtl(mold m) {
         err.require(m.BasePwr == string.Empty, mse.INC_DAT_BASE);
         err.require(m.AddPwr == string.Empty, mse.INC_DAT_ADD);
         err.require(m.Eye == string.Empty, mse.INC_DAT_EYE);
         err.require(m.FB == string.Empty, mse.INC_DAT_FB);
         err.require(m.id == string.Empty, mse.INC_DAT_MOLD);
      }

      /**
       * valida los campos requeridos pero que no son default
       */
      public void validOtherRequiredFields(operador op) {
         err.require(op.id == string.Empty, mse.INC_DAT_OPER);
      }

      /**
       * Retorna cero si no existe el lote y si existe retorna un numero mayor de cero
       */
      public int getBatchHeader(batch_header bh) {
         int resul = 0;
         validDefaultRequiredFields(bh);
         batch_header b = new batch_header();
         using (manufacture_DAL mdal = manufacture_DAL.instance(dbcode)) {
            b = mdal.get_batch_header(bh);
         }
         if (b != null) {
            resul = 1;
         }
         return resul;
      }

      /**
       * obtiene el estatus actual de un lote
       */
      public String getBatchStatus(batch_header bh) {
         using (manufacture_DAL mdal = manufacture_DAL.instance(dbcode)) {
            return mdal.get_batch_header(bh).status.Trim();
         }
      }

      /**
       * verifica si ya existe el molde dado de alta
       */
      public int getExistMould(batch_header bh, mold m) {
         using (casting_DAL cdal = casting_DAL.instance(dbcode)) {
            return cdal.getExistMould(bh, m);
         }
      }
      #endregion

      #region AQLFocovision

      /**
       * Consulta el tamaño del lote(solo la calidad buena de casting) y la muestra que se debe tomar
       * <param name="datos">objeto qc_block_sample donde se encuentra la informacion seleccionada</param>
       * <returns>retorna el resultado de la revision y el resultado de la insercion</returns>
       */
      public List<focovision_sample> getAQLFoco(batch bh, basenum base_, user usr) {
         var existHeader = string.Empty;
         var existDetail = string.Empty;
         var existAQL = string.Empty;

         List<focovision_sample> data = new List<focovision_sample>();

         // si la linea esta vacia enviar error al web service
         err.require(bh.lotno.id == null, mse.INC_DAT_BATCH);
         // si la linea origen esta vacia enviar error al web service
         err.require(bh.line.id == null, mse.INC_DAT_LINE);
         // si la linea destino esta vacia enviar error al web service             
         err.require(bh.product.id == null, mse.INC_DAT_PROD);
         err.require(base_ == null, mse.INC_DAT_BASE);
         err.require(bh.part == null, mse.INC_DAT_PART);

         using (casting_DAL cdal = casting_DAL.instance(dbcode)) {
            //existHeader = cdal.getBatchHeader(bh.get_header());
            //err.require(existHeader.Equals("0"), mse.BATCH_NOT_EXIST);  
            //if (existHeader.Equals("0")) throw new Exception("batch header does not exist");

            existDetail = cdal.getBatchDetail(bh);
            err.require(existDetail.Equals("0"), mse.BAT_DTL_NOT_EXISTS);

            if (base_.graduation.Equals("ALL")) {
               List<focovision_sample> bases = cdal.getAllBases(bh);

               foreach (var bas in bases) {
                  base_.graduation = bas.base_;
                  existAQL = cdal.getAQLCount(bh, base_);
                  if (existAQL.Equals("0")) {
                     cdal.insertAQLFoco(bh, base_, usr);
                  }
                  else //URI01
                  {
                     cdal.deleteAQLFoco(bh); //URI01
                     cdal.insertAQLFoco(bh, base_, usr); //URI01
                  } //URI01
               }
               base_.graduation = "ALL";
            }
            else {

               existAQL = cdal.getAQLCount(bh, base_);
               if (existAQL.Equals("0")) {
                  cdal.insertAQLFoco(bh, base_, usr);
               }
               else //URI01
               {
                  cdal.deleteAQLFoco(bh); //URI01
                  cdal.insertAQLFoco(bh, base_, usr); //URI01
               } //URI01
            }
            data = cdal.getAQLFoco(bh, base_);
         }
         return data;
      }

      /**
       * Consulta el tamaño del lote(solo la calidad buena de casting) y la muestra que se debe tomar
       * <param name="datos"> objeto qc_block_sample donde se encuentra la informacion seleccionada</param>
       * <returns> retorna el resultado de la revision y el resultado de la insercion</returns>
       */
      public String getFocoReads(batch_detail bh) {

         // si la linea esta vacia enviar error al web service
         err.require(bh.lotno.id == null, mse.INC_DAT_BATCH);
         // si la linea origen esta vacia enviar error al web service
         err.require(bh.line.id == null, mse.INC_DAT_LINE);
         // si la linea destino esta vacia enviar error al web service             
         err.require(bh.product.id == null, mse.INC_DAT_PROD);
         err.require(bh.sku.base_ == null, mse.INC_DAT_BASE);
         err.require(bh.part == null, mse.INC_DAT_PART);

         using (casting_DAL cdal = casting_DAL.instance(dbcode)) {
            return cdal.getFocoReads(bh);
         }
      }

      #endregion

      #region copy pallets between lines

      /**
       * Se encarga de copiar los pallets de una linea a otra
       * <param name="ba">contiene el batch, linea de origen y destino donde se copiaran</param>
       * <returns>Retorna OK o NO en caso de no se haya podido copiar</returns>
       */
      public String copyPallets(lot ba, line linsrc, line lintar) {
         string line_origen = string.Empty;
         string line_des = string.Empty;
         string existe_pallet = string.Empty; ;
         string palletsfaltantes = string.Empty;
         string insertpallet = string.Empty;
         List<mold_lent_match> data = new List<mold_lent_match>();

         //if el batch esta nulo enviar error al web service
         //if (ba.lotno.id == string.Empty) throw new Exception("batch cannot be null");
         err.require(ba.id == string.Empty, mse.INC_DAT_BATCH);

         // si la linea origen esta vacia enviar error al web service
         err.require(linsrc.id == string.Empty, mse.INC_DAT_LINE);
         //if (ba.line.id == string.Empty) throw new Exception("source line cannot be null");
         // si la linea destino esta vacia enviar error al web service
         //if (ba.line_destino == string.Empty) throw new Exception("destiny line cannot be null");
         err.require(lintar.id == string.Empty, mse.INC_DAT_LINE_DST);

         //if (ba.line.id == ba.line_destino) throw new Exception("cannot copy the same line");
         err.require(linsrc.id == lintar.id, mse.SAME_LINE);
         err.require(lintar.id.Substring(0, 1) != linsrc.id.Substring(0, 1), mse.WRONG_TARGET_LINE);
         //if (ba.line_destino.Substring(0, 1) != ba.line.id.Substring(0, 1)) throw new Exception("wrong destiny line");


         using (casting_DAL cdal = casting_DAL.instance(dbcode)) {
            line_origen = cdal.validateInfoLines(ba, linsrc, lintar, "origen");
            line_des = cdal.validateInfoLines(ba, linsrc, lintar, "des");
         }

         if (line_origen.Trim() == "OK") {
            if (line_des.Trim() == "OK") {
               using (casting_DAL cdal4 = casting_DAL.instance(dbcode)) {
                  List<mold_lent_match> copyPallets = cdal4.getPalletsOrigen(ba, linsrc);

                  foreach (var pal in copyPallets) {

                     //string pall = Convert.ToInt32(dato.pallet.id).ToString();
                     using (casting_DAL cdal2 = casting_DAL.instance(dbcode)) {
                        existe_pallet = cdal2.validExistPallet(ba, lintar, pal.pallet);
                     }
                     if (existe_pallet.Trim() == "NO") {
                        //inserta
                        using (casting_DAL cdal3 = casting_DAL.instance(dbcode)) {
                           insertpallet = cdal3.insertPalletbetweenlines(ba, linsrc, lintar, pal.pallet);
                        }

                        if (insertpallet.Trim() == "NO") {
                           return insertpallet;
                        }
                     }
                     else {
                        palletsfaltantes = string.Concat(palletsfaltantes, ",", existe_pallet);
                     }
                  }
               }
            }
            else {
               //if (line_des.Trim() == "NO") throw new Exception("La linea:" + ba.line_destino + " ya cuenta con pallets asignados");
               err.require(line_des.Trim() == "NO", mse.DEST_LINE_HAS_PALLETES);
            }
         }
         else {
            err.require(line_origen.Trim() == "NO", mse.SRC_LINE_NOT_HAS_PALLETES);
            //if (line_origen.Trim() == "NO") throw new Exception("La linea:" + ba.line.id + " no cuenta con pallets asignados");
         }

         return string.Concat(insertpallet, ":", palletsfaltantes);
      }

      public mold get_mold(mold ml) {
         using (var dal = casting_DAL.instance(dbcode)) {
            return dal.get_mold(ml);
         }
      }
      public mold get_mold(mold ml, string fb) {
         using (var dal = casting_DAL.instance(dbcode)) {
            return dal.get_mold(ml, fb);
         }
      }
      #endregion
   }
}

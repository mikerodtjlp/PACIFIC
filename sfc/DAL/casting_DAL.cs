using System;
using System.Collections.Generic;
using System.Text;
using mro;
using mro.BO;
using mro.db;
using sfc.BO;
using sfc.BL;
using System.Data;

namespace sfc.DAL
{
    public class casting_DAL : DataWorker, IDisposable
    {
        #region database setup

        //private validate validate = validate.getInstance();
        private IDbConnection conn = null;

        public static casting_DAL instance(string name = "main")
        {
            return new casting_DAL(name);
        }
        public casting_DAL(string name): base(name)
        {
            conn = database.CreateOpenConnection();
        }
        void IDisposable.Dispose()
        {
            conn.Close();
            conn.Dispose();
        }
        #endregion

        // deprecated by release 2.0 and the client uses execute_query direct
        /*public void cast_collection(batch btc, resource sku)
        {
            var qry = new StringBuilder("execute cast_collection ");
            qry.AppendFormat(" '{0}','{1}','{2}','{3}','{4}', 0; ", 
                                btc.lotno.id, btc.product.id, btc.line.id, btc.part, sku.id);
            using (var cmd = database.CreateCommand(qry.ToString(), conn))
            {
                cmd.ExecuteNonQuery();
            }
        }*/

        public void insert_sku(batch b, resource res, int q, int m)
        {
            /*            var qry = new StringBuilder("execute insert_bulk_pack_3 ");
                        qry.AppendFormat(" '{0}','{1}','{2}','{3}','{4}',{5},{6},'' ",
                                            b.lotno.id, b.line.id, b.part, b.product.id, res.resource_, q, m);
                        string bpack_id = "";
                        using (var cmd = database.CreateCommand(qry.ToString(), conn))
                        {
                            for (var r = cmd.ExecuteReader(); r.Read(); )
                            {
                                bpack_id = r["bpack_id"].ToString();
                                break;
                            }
                        }
                        return bpack_id;*/
        }
        public bool exist_sku_produced(batch b, resource sku)
        {
            using (IDbCommand cmd = database.CreateStoredProcCommand("cst_exist_sku_produced", conn))
            {
                var p = cmd.Parameters;
                p.Add(database.CreateParameter("@batch", b.lotno.id));
                p.Add(database.CreateParameter("@prod", b.product.id));
                p.Add(database.CreateParameter("@line", b.line.id));
                p.Add(database.CreateParameter("@sku", sku.id));

                IDataReader reader = cmd.ExecuteReader();
                int r = 0;
                while (reader.Read())
                {
                    r = reader.GetInt32(0);
                    break;
                }
                reader.Dispose();
                reader.Close();
                return r == 1;
            }
        }

        public bool ispull(batch b, resource res, int q, int m)
        {
            /*            var qry = new StringBuilder("execute insert_bulk_pack_3 ");
                        qry.AppendFormat(" '{0}','{1}','{2}','{3}','{4}',{5},{6},'' ",
                                            b.lotno.id, b.line.id, b.part, b.product.id, res.resource_, q, m);
                        string bpack_id = "";
                        using (var cmd = database.CreateCommand(qry.ToString(), conn))
                        {
                            for (var r = cmd.ExecuteReader(); r.Read(); )
                            {
                                bpack_id = r["bpack_id"].ToString();
                                break;
                            }
                        }
                        return bpack_id;*/
            return true;
        }

        #region inventory
        public production_inventory populate_production_inventory(IDataReader r)
        {
            production_inventory obj = null;

            while (r.Read())
            {
                obj = new production_inventory(r["batch"].ToString(), r["prod_code"].ToString(),
                                            r["line_id"].ToString(), r["resource"].ToString());
                obj.qty = int.Parse(r["qty"].ToString());
                obj.date = (DateTime)r["date"];
            }
            return obj;
        }
        public production_inventory get_production_inventory(production_inventory p)
        {
            production_inventory pi = null;
            var qry = new StringBuilder("select * from mold_inv with (nolock) ");
            qry.AppendFormat(" where batch='{0}' and prod_code='{1}' and line_id='{2}' and resource='{3}'",
                            p.lotno.id, p.product.id, p.line.id, p.resource_.id);
            using (var cmd = database.CreateCommand(qry.ToString(), conn))
            {
                var reader = cmd.ExecuteReader();
                pi = populate_production_inventory(reader);
            }
            return pi;
        }
        public void insert_production_inventory(production_inventory p)
        {
            var qry = new StringBuilder("insert into mold_inv");
            qry.AppendFormat(" values('{0}', '{1}', '{2}', '{3}', {4}, getdate()) ",
                                p.lotno.id, p.line.id, p.product.id, p.resource_.id, p.qty);
            using (var cmd = database.CreateCommand(qry.ToString(), conn))
            {
                cmd.ExecuteNonQuery();
            }
        }
        public void update_production_inventory(production_inventory p)
        {
            var qry = new StringBuilder("update mold_inv");
            qry.AppendFormat(" set qty={0} " +
                            " where batch='{1}' and prod_code='{2}' and line_id='{3}' and resource='{4}'",
                                p.qty, p.lotno.id, p.product.id, p.line.id, p.resource_.id);
            using (var cmd = database.CreateCommand(qry.ToString(), conn))
            {
                cmd.ExecuteNonQuery();
            }
        }
        public void delete_production_inventory(production_inventory p)
        {
            var qry = new StringBuilder("delete mold_inv");
            qry.AppendFormat(" where batch='{0}' and prod_code='{1}' and line_id='{2}' and resource='{3}'",
                                p.lotno.id, p.product.id, p.line.id, p.resource_.id);
        }
        #endregion

        #region product molds relation
        public List<product_mold_relation> get_product_mold_relation_all(product p)
        {
            var data = new List<product_mold_relation>();
            var qry = new StringBuilder("select * from t_molds_rel with (nolock) ");
            qry.AppendFormat(" where prod_code='{0}' ", p.id);
            using (var cmd = database.CreateCommand(qry.ToString(), conn))
            {
                for (var r = cmd.ExecuteReader(); r.Read(); )
                {
                    var obj = new product_mold_relation(r["prod_code"].ToString(),
                                            r["moldid"].ToString(),
                                            r["resource"].ToString());
                    data.Add(obj);
                }
            }
            return data;
        }
        public product_mold_relation get_product_mold_relation(product_mold_relation p)
        {
            var qry = new StringBuilder("select * from t_molds_rel with (nolock) ");
            qry.AppendFormat(" where prod_code='{0}' and moldid='{1}'",
                                p.prod_code.id, p.moldid.id);
            product_mold_relation obj = null;
            using (var cmd = database.CreateCommand(qry.ToString(), conn))
            {
                for (var r = cmd.ExecuteReader(); r.Read(); )
                {
                    obj = new product_mold_relation(r["prod_code"].ToString(),
                                            r["moldid"].ToString(),
                                            r["resource"].ToString());
                    break;
                }
            }
            return obj;
        }
        public void insert_product_mold_relation(product_mold_relation p)
        {
            var qry = new StringBuilder("insert into t_molds_rel ");
            qry.AppendFormat(" values('{0}', '{1}', '{2}')",
                                p.prod_code.id, p.moldid.id, p.sku.id);
            using (var cmd = database.CreateCommand(qry.ToString(), conn))
            {
                cmd.ExecuteNonQuery();
            }
        }
        public void update_product_mold_relation(product_mold_relation p)
        {
            var qry = new StringBuilder("update t_molds_rel ");
            qry.AppendFormat(" set resource='{0}'  " +
                                " where prod_code='{1}' and moldid='{2}' ",
                                p.sku.id,
                                p.prod_code.id, p.moldid.id);
            using (var cmd = database.CreateCommand(qry.ToString(), conn))
            {
                cmd.ExecuteNonQuery();
            }
        }
        public void delete_product_mold_relation(product_mold_relation p)
        {
            var qry = new StringBuilder("delete t_molds_rel ");
            qry.AppendFormat(" where prod_code='{0}' and moldid='{1}'",
                                p.prod_code.id, p.moldid.id);
            using (var cmd = database.CreateCommand(qry.ToString(), conn))
            {
                cmd.ExecuteNonQuery();
            }
        }
        #endregion

        #region Match Mold Lent
        /// <summary>
        /// Hace el llamado al procedimiento getMoldLentMatches, el cual retorna los datos validados.
        /// </summary>
        /// <param name="ba">recibe batch y linea en el objeto batch</param>
        /// <returns>Retorna los datos validados en el objeto mold_lent_match</returns>
        public List<mold_lent_match> getMoldLentMatches(batch ba)
        {

            using (IDbCommand cmd = database.CreateStoredProcCommand("getMoldLentMatches", conn))
            {
                cmd.Parameters.Add(database.CreateParameter("@batch", ba.lotno.id));
                cmd.Parameters.Add(database.CreateParameter("@module", ba.line.id));
                IDataReader reader = cmd.ExecuteReader();
                List<mold_lent_match> data = new List<mold_lent_match>();
                while (reader.Read())
                {
                    data.Add(populateMold_lent_matchFromReader(reader));

                }
                reader.Dispose();
                reader.Close();
                return data;
            }

        }
        /// <summary>
        /// Se ecarga de leer el resultado del procedure
        /// </summary>
        /// <param name="reader">Recibe los datos en un reader</param>
        /// <returns>Retorna los datos de las revisiones en el objeto mold_lent_match</returns>
        protected virtual mold_lent_match populateMold_lent_matchFromReader(IDataReader reader)
        {
            mold_lent_match m = new mold_lent_match();
            m.sku.id = Convert.ToString((validate.getDefaultIfDBNull(reader["resource"], TypeCode.String)));
            m.pallet.id = Convert.ToString((validate.getDefaultIfDBNull(reader["palletid"], TypeCode.String)));            
            m.modulo = Convert.ToString((validate.getDefaultIfDBNull(reader["module"], TypeCode.String)));
            m.molde.id = Convert.ToString((validate.getDefaultIfDBNull(reader["moldid"], TypeCode.String)));
            m.moldIdx = Convert.ToInt32((validate.getDefaultIfDBNull(reader["moldindex"], TypeCode.String)));            
            m.barcode_number = Convert.ToString((validate.getDefaultIfDBNull(reader["barcode_number"], TypeCode.String)));
            m.moldDate = Convert.ToDateTime((validate.getDefaultIfDBNull(reader["revision_date"], TypeCode.DateTime)));
            m.validacion = Convert.ToString((validate.getDefaultIfDBNull(reader["validacion"], TypeCode.String)));
            m.campo = Convert.ToInt32((validate.getDefaultIfDBNull(reader["campo"], TypeCode.Int32)));
            m._img = Convert.ToInt32((validate.getDefaultIfDBNull(reader["_img"], TypeCode.Int32)));

            return m;
        }

        /**
         * Hace el llamado al procedimiento validateMoldLent, el cual valida que el indice introducido corresponda con el del sku.
         * </summary>
         * <param name="datos">recibe los datos seleccionados en el objeto mold_lent_match</param>
         * <returns>Retorna un String NO u OK segun el resultado de la validacion</returns>
         **/
        public String validateMoldLent(mold_lent_match datos)
        {

            using (IDbCommand cmd = database.CreateStoredProcCommand("validateMoldLent", conn))
            {
                var p = cmd.Parameters;
                p.Add(database.CreateParameter("@batch", datos.batch.lotno.id));
                p.Add(database.CreateParameter("@palletid", datos.pal));
                p.Add(database.CreateParameter("@module", datos.modulo));
                p.Add(database.CreateParameter("@resource", datos.sku.id));
                p.Add(database.CreateParameter("@barcode_number", datos.valor_introducido));
                IDataReader r = cmd.ExecuteReader();
                String resultado = String.Empty;
                while (r.Read())
                {
                    resultado = r["validacion"].ToString();

                }
                r.Dispose();
                r.Close();
                return resultado.Trim() ;
            }

        }
        /**
         *  Hace el llamado al procedure validEditField_MoldLent, el cual se encarga de validar si el campo que se modifico aun no completa las dos oportunidades de validacion
         * </summary>
         * <param name="datos">recibe los datos seleccionados en el objeto mold_lent_match</param>
         * <returns>Retorna si se puede modificar o no, el campo modificado y el valor introducido en el objeto mold_lent_match</returns>
         **/
        public List<mold_lent_match> validEditField_MoldLent(mold_lent_match datos)
        {
            using (IDbCommand cmd = database.CreateStoredProcCommand("validEditField_MoldLent", conn))
            {
                var p = cmd.Parameters;
                p.Add(database.CreateParameter("@batch", datos.batch.lotno.id));
                p.Add(database.CreateParameter("@module", datos.modulo));
                p.Add(database.CreateParameter("@moldid", datos.molde.id));
                p.Add(database.CreateParameter("@resource", datos.sku.id));
                p.Add(database.CreateParameter("@campo", datos.campo));
                p.Add(database.CreateParameter("@valor", datos.valor_introducido));
                IDataReader r = cmd.ExecuteReader();
                List<mold_lent_match> data = new List<mold_lent_match>();
                while (r.Read())
                {
                    data.Add(populatevalidEditField_MoldLentFromReader(r));

                }
                r.Dispose();
                r.Close();
                return data;
            }
        }
        /// <summary>
        /// Se encarga de leer el resultado del procedure validEditField_MoldLent
        /// </summary>
        /// <param name="reader">contiene los datos retornados de la base de datos</param>
        /// <returns>Retorna un objeto de tipo mold_lent_match con el resultado de la validacion</returns>
        protected virtual mold_lent_match populatevalidEditField_MoldLentFromReader(IDataReader reader)
        {
            mold_lent_match m = new mold_lent_match();
            m.validacion = Convert.ToString((validate.getDefaultIfDBNull(reader["modificar"], TypeCode.String)));
            m.valor_introducido = Convert.ToString((validate.getDefaultIfDBNull(reader["valor_introducido"], TypeCode.String)));
            m.campo = Convert.ToInt32((validate.getDefaultIfDBNull(reader["campo"], TypeCode.Int32)));

            return m;
        }
        /// <summary>
        /// Hace el llamado al procedure insertMoldLentRelation
        /// </summary>
        /// <param name="datos">recibe los datos seleccionados en el objeto mold_lent_match y la validacion</param>
        /// <param name="validacion">Indica es un indice valido o no.</param>
        /// <returns>Retorna el resultado de la insercion</returns>
        public List<mold_lent_match> insertMoldLentRelation(mold_lent_match datos, String validacion)
        {            
            using (IDbCommand cmd = database.CreateStoredProcCommand("insertMoldLentRelation", conn))
            {
                var p = cmd.Parameters;
                p.Add(database.CreateParameter("@batch", datos.batch.lotno.id));
                p.Add(database.CreateParameter("@module", datos.modulo));
                p.Add(database.CreateParameter("@moldid", datos.molde.id));
                p.Add(database.CreateParameter("@resource", datos.sku.id));
                p.Add(database.CreateParameter("@revision", validacion));
                p.Add(database.CreateParameter("@valor", datos.valor_introducido));
                p.Add(database.CreateParameter("@campo", datos.campo));
                p.Add(database.CreateParameter("@pallet", datos.pal));
                IDataReader reader = cmd.ExecuteReader();
                List<mold_lent_match> data = new List<mold_lent_match>();
                while (reader.Read())
                {
                    data.Add(populateinsertMoldLentRelationFromReader(reader));

                }
                reader.Dispose();
                reader.Close();
                return data;
            }
        }

        /// <summary>
        /// Se encarga de leer el resultado del llamado al procedure insertMoldLentRelation
        /// </summary>
        /// <param name="reader">contiene el resultado directo del procedure</param>
        /// <returns>Retorna el resultado de la insercion y el mensaje a mostrar en un objeto mold_lent_match</returns>
        protected virtual mold_lent_match populateinsertMoldLentRelationFromReader(IDataReader reader)
        {
            mold_lent_match m = new mold_lent_match();
            m.resultado = Convert.ToString((validate.getDefaultIfDBNull(reader["resultado"], TypeCode.String)));
            m.showmsg = Convert.ToString((validate.getDefaultIfDBNull(reader["showmsg"], TypeCode.String)));

            return m;
        }
        /// <summary>
        /// Consulta el sku de un molde
        /// </summary>
        /// <param name="datos">recibe los datos seleccionados en el objeto mold_lent_match</param>
        /// <returns>Retorna un String con el sku</returns>
        public String getMoldSku(mold_lent_match datos)
        {
            using (IDbCommand cmd = database.CreateStoredProcCommand("getMoldSku", conn))
            {
                var p = cmd.Parameters;
                p.Add(database.CreateParameter("@batch", datos.batch.lotno.id));
                p.Add(database.CreateParameter("@palletid", datos.pal));
                p.Add(database.CreateParameter("@module", datos.modulo));

                IDataReader reader = cmd.ExecuteReader();
                String resultado = String.Empty;
                while (reader.Read())
                {
                    resultado = reader["resource"].ToString();

                }
                reader.Dispose();
                reader.Close();
                return resultado.Trim();
            }

        }    
        #endregion

        #region captura de moldes pulidos
        /// <summar>y
        /// Obtiene la lista de moldes que se han capturado para un lote, linea y producto especifico
        /// </summary>
        /// <param name="bh"></param>
        public List<mold> getPolishedMoulds(batch_header bh, mold ml)
        {
            using (IDbCommand cmd = database.CreateStoredProcCommand("getPolishedMoulds", conn))
            {
                var p = cmd.Parameters;
                p.Add(database.CreateParameter("@Batch", bh.lotno.id));
                p.Add(database.CreateParameter("@line_id", bh.line.id));
                p.Add(database.CreateParameter("@prod_code", bh.product.id));
                p.Add(database.CreateParameter("@location", bh.location.id));
                p.Add(database.CreateParameter("@part", bh.part));
                p.Add(database.CreateParameter("@mouldsource", ml.msource));

                IDataReader reader = cmd.ExecuteReader();
                List<mold> data = new List<mold>();
                while (reader.Read())
                {
                    data.Add(populategetPolishedMouldsFromReader(reader));

                }
                reader.Dispose();
                reader.Close();
                return data;
            }
        }
        protected virtual mold populatetblmolds(IDataReader reader)
        {
            mold m = new mold();
            m.id = Convert.ToString((validate.getDefaultIfDBNull(reader["moldid"], TypeCode.String))).Trim();
            m.name = Convert.ToString((validate.getDefaultIfDBNull(reader["moldname"], TypeCode.String))).Trim();
            m.FB = Convert.ToString((validate.getDefaultIfDBNull(reader["FB"], TypeCode.String)));
            m.BasePwr = Convert.ToString((validate.getDefaultIfDBNull(reader["basepwr"], TypeCode.String)));
            m.AddPwr = Convert.ToString((validate.getDefaultIfDBNull(reader["addpwr"], TypeCode.String)));
            m.Eye = Convert.ToString((validate.getDefaultIfDBNull(reader["eye"], TypeCode.String)));
            m.Diameter = Convert.ToString((validate.getDefaultIfDBNull(reader["diameter"], TypeCode.String)));
            m.AddPwr = Convert.ToString((validate.getDefaultIfDBNull(reader["addpwr4"], TypeCode.String)));
            m.RangePwr = Convert.ToString((validate.getDefaultIfDBNull(reader["rangepwr"], TypeCode.String)));
            m.sap = Convert.ToString((validate.getDefaultIfDBNull(reader["sap"], TypeCode.String)));
            return m;
        }
        public mold get_mold(mold ml)
        {
            var qry = string.Format("select * from tblmolds with (nolock) " +
                                    "where moldid='{1}'",
                                    ml.id);
            using (var cmd = database.CreateCommand(qry, conn))
            {
                for (var r = cmd.ExecuteReader(); r.Read(); )
                {
                    return populatetblmolds(r);
                }
            }
            return null;
        }
        public mold get_mold(mold ml, string fb)
        {
            var qry = string.Format("select * from tblmolds with (nolock) " +
                                    "where FB='{0}' and moldid='{1}'",
                                    fb, ml.id);
            using (var cmd = database.CreateCommand(qry, conn))
            {
                for (var r = cmd.ExecuteReader(); r.Read(); )
                {
                    return populatetblmolds(r);
                }
            }
            return null;
        }
        /// <summary>
        /// agrega a un objeto lo consultado de la base de datos
        /// </summary>
        /// <param name="reader"></param>
        /// <returns>Lista de moldes</returns>
        protected virtual mold populategetPolishedMouldsFromReader(IDataReader reader)
        {

            mold m = new mold();
           
            m.BasePwr = Convert.ToString((validate.getDefaultIfDBNull(reader["base"], TypeCode.String)));
            m.AddPwr = Convert.ToString((validate.getDefaultIfDBNull(reader["addition"], TypeCode.String)));
            m.Eye = Convert.ToString((validate.getDefaultIfDBNull(reader["mouldLR"], TypeCode.String)));
            m.FB = Convert.ToString((validate.getDefaultIfDBNull(reader["mouldFB"], TypeCode.String)));
            m.id = Convert.ToString((validate.getDefaultIfDBNull(reader["mold"], TypeCode.String))).Trim();
            m.date_time = Convert.ToDateTime((validate.getDefaultIfDBNull(reader["date_time"], TypeCode.DateTime)));
            
            m.qty = Convert.ToInt32(validate.getDefaultIfDBNull(reader["total"], TypeCode.Int32));
            m.oper = new operador(Convert.ToString((validate.getDefaultIfDBNull(reader["operator"], TypeCode.String))));  
            m.statusA = Convert.ToString((validate.getDefaultIfDBNull(reader["status"], TypeCode.String))).Trim();

            return m;

        }
      /*  /// <summary>
        /// Retorna cero si no existe el lote y si existe retorna un numero mayor de cero
        /// </summary>
        /// <param name="bh"></param>
        /// <returns></returns>
        public int getBatchHeaderByLoc(batch_header bh)
        {

            int existe = 0;

            using (IDbCommand cmd = database.CreateStoredProcCommand("getBatchHeaderByLoc", conn))
            {
                cmd.Parameters.Add(database.CreateParameter("@Batch", bh.lotno.id));
                cmd.Parameters.Add(database.CreateParameter("@line_id", bh.line.id));
                cmd.Parameters.Add(database.CreateParameter("@prod_code", bh.product.id));
                cmd.Parameters.Add(database.CreateParameter("@location", bh.location.id));
                cmd.Parameters.Add(database.CreateParameter("@part", bh.part));
                cmd.Parameters.Add(database.CreateParameter("@cycle", bh.cycle));

                IDataReader reader = cmd.ExecuteReader();

                while (reader.Read())
                {
                    existe = Convert.ToInt32(validate.getDefaultIfDBNull(reader["exist"], TypeCode.Int32));

                }
                reader.Dispose();
                reader.Close();
                return existe;
            }

        }*/
        /// <summary>
        /// verifica si ya existe el molde dado de alta
        /// </summary>
        /// <param name="bh"></param>
        /// <param name="bd"></param>
        /// <returns></returns>
        public int getExistMould(batch_header bh, mold m)
        {
            int existe = 0;

            using (IDbCommand cmd = database.CreateStoredProcCommand("getExistMould", conn))
            {
                var p = cmd.Parameters;
                p.Add(database.CreateParameter("@Batch", bh.lotno.id));
                p.Add(database.CreateParameter("@line_id", bh.line.id));
                p.Add(database.CreateParameter("@prod_code", bh.product.id));
                p.Add(database.CreateParameter("@part", bh.part));
                p.Add(database.CreateParameter("@mold_id", m.id));
                p.Add(database.CreateParameter("@mouldsource", m.msource));

                IDataReader reader = cmd.ExecuteReader();

                while (reader.Read())
                {
                    existe = Convert.ToInt32(validate.getDefaultIfDBNull(reader["exist"], TypeCode.Int32));

                }
                reader.Dispose();
                reader.Close();
            }

            return existe;
        }
      /*  /// <summary>
        /// obtiene el estatus actual de un lote
        /// </summary>
        /// <param name="bh"></param>
        /// <returns></returns>
        public String getBatchStatus(batch_header bh)
        {

            string status = "";

            using (IDbCommand cmd = database.CreateStoredProcCommand("getBatchStatus", conn))
            {
                cmd.Parameters.Add(database.CreateParameter("@Batch", bh.lotno.id));
                cmd.Parameters.Add(database.CreateParameter("@line_id", bh.line.id));
                cmd.Parameters.Add(database.CreateParameter("@prod_code", bh.product.id));
                cmd.Parameters.Add(database.CreateParameter("@part", bh.part));
                cmd.Parameters.Add(database.CreateParameter("@location", bh.location.id));
                cmd.Parameters.Add(database.CreateParameter("@cycle", bh.cycle));

                IDataReader reader = cmd.ExecuteReader();

                while (reader.Read())
                {
                    status = reader["status"].ToString();

                }
                reader.Dispose();
                reader.Close();
            }

            return status.Trim();

        }*/
        /// <summary>
        /// Se encarga de insertar un molde con cantidad 1
        /// </summary>
        /// <param name="bd"></param>
        public void insertPolishedMould(batch_header bh, operador op, mold m)
        {

            using (IDbCommand cmd = database.CreateStoredProcCommand("insertPolishedMould", conn))
            {
                var p = cmd.Parameters;
                p.Add(database.CreateParameter("@DtlID", bh.lotno.id));
                p.Add(database.CreateParameter("@Part", bh.part));
                p.Add(database.CreateParameter("@Line", bh.line.id));
                p.Add(database.CreateParameter("@Pcc", bh.product.id));
                p.Add(database.CreateParameter("@Base", m.BasePwr));
                p.Add(database.CreateParameter("@Addition", m.AddPwr));
                p.Add(database.CreateParameter("@MouldLR", m.Eye));
                p.Add(database.CreateParameter("@MouldFB", m.FB));
                p.Add(database.CreateParameter("@MouldSource", m.msource));
                p.Add(database.CreateParameter("@MouldChange", m.mchange));
                p.Add(database.CreateParameter("@DefectMat", m.dmat));
                p.Add(database.CreateParameter("@DefectGrp", m.dgrp));
                p.Add(database.CreateParameter("@DefectDep", m.ddep));
                p.Add(database.CreateParameter("@Defect", m.defect));
                p.Add(database.CreateParameter("@Total", m.qty));
                p.Add(database.CreateParameter("@Operator", op.id));
                p.Add(database.CreateParameter("@mold", m.id));
                cmd.ExecuteNonQuery();
            }
        }
        /// <summary>
        /// Elimina todo el registro de un molde en especifico sin importar la cantidad
        /// </summary>
        /// <param name="bh"></param>
        /// <param name="bd"></param>
        public void deletePolishedMoulds(batch_header bh, mold m)
        {
            using (IDbCommand cmd = database.CreateStoredProcCommand("deletePolishedMoulds", conn))
            {
                var p = cmd.Parameters;
                p.Add(database.CreateParameter("@batch", bh.lotno.id));
                p.Add(database.CreateParameter("@part", bh.part));
                p.Add(database.CreateParameter("@line_id", bh.line.id));
                p.Add(database.CreateParameter("@prod_code", bh.product.id));
                p.Add(database.CreateParameter("@mouldrl", m.Eye));
                p.Add(database.CreateParameter("@mouldfb", m.FB));
                p.Add(database.CreateParameter("@base", m.BasePwr));
                p.Add(database.CreateParameter("@adicion", m.AddPwr));
                p.Add(database.CreateParameter("@mold_id", m.id));
                p.Add(database.CreateParameter("@mouldsource", m.msource));
                cmd.ExecuteNonQuery();
            }
        }
       /* /// <summary>
        /// Crea el registro en el batch header para el location recibido
        /// </summary>
        /// <param name="bh"></param>
        public void insertBatchHeader(batch_header bh)
        {

            using (IDbCommand cmd = database.CreateStoredProcCommand("insertBatchHeader", conn))
            {
                cmd.Parameters.Add(database.CreateParameter("@batch", bh.lotno.id));
                cmd.Parameters.Add(database.CreateParameter("@line_id", bh.line.id));
                cmd.Parameters.Add(database.CreateParameter("@prod_code", bh.product.id));
                cmd.Parameters.Add(database.CreateParameter("@part", bh.part));
                cmd.Parameters.Add(database.CreateParameter("@location", bh.location.id));
                cmd.Parameters.Add(database.CreateParameter("@cycle", bh.cycle));
                cmd.Parameters.Add(database.CreateParameter("@status", bh.status));
                cmd.Parameters.Add(database.CreateParameter("@new_pkg_count", bh.new_pkg_count));
                cmd.Parameters.Add(database.CreateParameter("@as_400", bh.as_400));
                cmd.Parameters.Add(database.CreateParameter("@qty_in", bh.qty_in));
                cmd.Parameters.Add(database.CreateParameter("@qc_audited", bh.qc_audit));
                cmd.Parameters.Add(database.CreateParameter("@comment", bh.comments));

                cmd.ExecuteNonQuery();
            }
        }*/
        /// <summary>
        /// Cambia de estatus el batch a RELEASE
        /// </summary>
        /// <param name="bh"></param>
        /// <param name="bd"></param>
        public void updateBatchStatus(batch_header bh, mold m)
        {
            using (IDbCommand cmd = database.CreateStoredProcCommand("updateBatchStatus", conn))
            {
                var p = cmd.Parameters;
                p.Add(database.CreateParameter("@batch", bh.lotno.id));
                p.Add(database.CreateParameter("@line_id", bh.line.id));
                p.Add(database.CreateParameter("@prod_code", bh.product.id));
                p.Add(database.CreateParameter("@part", bh.part));
                p.Add(database.CreateParameter("@location", bh.location.id));
                p.Add(database.CreateParameter("@cycle", bh.cycle));
                p.Add(database.CreateParameter("@act_status", m.statusA));
                p.Add(database.CreateParameter("@new_status", m.statusN));
                cmd.ExecuteNonQuery();
            }
        }
        /// <summary>
        /// Actualiza la cantidad de moldes leidos, aumentandolo en 1.
        /// </summary>
        /// <param name="bh"></param>
        /// <param name="bd"></param>
        /// <param name="action"></param>
        public void updatePolishedMould(batch_header bh, mold m, string action)
        {
            using (IDbCommand cmd = database.CreateStoredProcCommand("updatePolishedMould", conn))
            {
                var p = cmd.Parameters;
                p.Add(database.CreateParameter("@batch", bh.lotno.id));
                p.Add(database.CreateParameter("@part", bh.part));
                p.Add(database.CreateParameter("@line_id", bh.line.id));
                p.Add(database.CreateParameter("@prod_code", bh.product.id));
                p.Add(database.CreateParameter("@mouldrl", m.Eye));
                p.Add(database.CreateParameter("@mouldfb", m.FB));
                p.Add(database.CreateParameter("@base", m.BasePwr));
                p.Add(database.CreateParameter("@adicion", m.AddPwr));
                p.Add(database.CreateParameter("@mold_id", m.id));
                p.Add(database.CreateParameter("@qty", m.qty));
                p.Add(database.CreateParameter("@action", action));
                p.Add(database.CreateParameter("@mouldsource", m.msource));
                cmd.ExecuteNonQuery();
            }
        }

        #endregion

        #region AQLFocovision
        /// <summary>
        /// verifica si existe el lote creado.
        /// </summary>
        /// <param name="datos"></param>
        /// <returns>Retorna la cantidad en el header</returns>
        public String getBatchHeader(batch_header ba)
        {
            using (IDbCommand cmd = database.CreateStoredProcCommand("getBatchHeader", conn))
            {
                var p = cmd.Parameters;
                p.Add(database.CreateParameter("@batch", ba.lotno.id));
                p.Add(database.CreateParameter("@module", ba.line.id));
                p.Add(database.CreateParameter("@prod_code", ba.product.id));
                p.Add(database.CreateParameter("@part", ba.part));

                IDataReader reader = cmd.ExecuteReader();
                String resultado = String.Empty;
                while (reader.Read())
                {
                    resultado = reader["header"].ToString();

                }
                reader.Dispose();
                reader.Close();
                return resultado.Trim();
            }
        }

        /// <summary>
        /// verifica si existen detalles para poder sacar la muestra.
        /// </summary>
        /// <param name="datos"></param>
        /// <returns>Retorna la cantidad de registros en el detail</returns>
        public String getBatchDetail(batch ba)
        {
            using (IDbCommand cmd = database.CreateStoredProcCommand("getBatchDetail", conn))
            {
                var p = cmd.Parameters;
                p.Add(database.CreateParameter("@batch", ba.lotno.id));
                p.Add(database.CreateParameter("@module", ba.line.id));
                p.Add(database.CreateParameter("@prod_code", ba.product.id));
                p.Add(database.CreateParameter("@part", ba.part));

                IDataReader reader = cmd.ExecuteReader();
                String resultado = String.Empty;
                while (reader.Read())
                {
                    resultado = reader["detail"].ToString();

                }
                reader.Dispose();
                reader.Close();
                return resultado.Trim();
            }
        }

        /// <summary>
        /// verifica si existe ya creado el AQL.
        /// </summary>
        /// <param name="datos"></param>
        /// <returns>Retorna si existe o no el aql creado</returns>
        public String getAQLCount(batch ba, basenum base_)
        {
            using (IDbCommand cmd = database.CreateStoredProcCommand("getAQLCount", conn))
            {
                var p = cmd.Parameters;
                p.Add(database.CreateParameter("@batch", ba.lotno.id));
                p.Add(database.CreateParameter("@module", ba.line.id));
                p.Add(database.CreateParameter("@prod_code", ba.product.id));
                p.Add(database.CreateParameter("@part", ba.part));
                p.Add(database.CreateParameter("@base", base_.graduation));

                IDataReader reader = cmd.ExecuteReader();
                String resultado = String.Empty;
                while (reader.Read())
                {
                    resultado = reader["existAQL"].ToString();

                }
                reader.Dispose();
                reader.Close();
                return resultado.Trim();
            }
        }

        /// <summary>
        /// se encarga de consultar e insertar el tamaño del lote y la muestra.
        /// </summary>
        /// <param name="datos"></param>
        /// <returns>no retorna nada</returns>
        public void insertAQLFoco(batch bh, basenum base_, user usr)
        {
            using (IDbCommand cmd = database.CreateStoredProcCommand("insertAQLFoco", conn))
            {
                var p = cmd.Parameters;
                p.Add(database.CreateParameter("@batch", bh.lotno.id));
                p.Add(database.CreateParameter("@module", bh.line.id));
                p.Add(database.CreateParameter("@prod_code", bh.product.id));
                p.Add(database.CreateParameter("@part", bh.part));
                p.Add(database.CreateParameter("@user", usr.id));
                p.Add(database.CreateParameter("@base", base_.graduation));
                p.Add(database.CreateParameter("@adicion", "ALL"));
                cmd.ExecuteNonQuery();
            }
        }

        //URI01 Borra la tabla tfocovision_sample para despues actualizarlo
        public void deleteAQLFoco(batch bh)
        {
            using (IDbCommand cmd = database.CreateStoredProcCommand("deleteAQLFoco", conn))
            {
                var p = cmd.Parameters;
                p.Add(database.CreateParameter("@batch", bh.lotno.id));
                p.Add(database.CreateParameter("@module", bh.line.id));
                p.Add(database.CreateParameter("@prod_code", bh.product.id));
                p.Add(database.CreateParameter("@part", bh.part));
                cmd.ExecuteNonQuery();
            }
        }



        /// <summary>
        /// obtiene los datos de aql para el lote introducido
        /// </summary>
        /// <param name="ba">recibe batch, linea,prod y parte en el objeto batch_detail</param>
        /// <returns>Retorna los datos en el objeto qc_block_sample</returns>
        public List<focovision_sample> getAQLFoco(batch bh, basenum base_)
        {
            using (IDbCommand cmd = database.CreateStoredProcCommand("getAQLFoco", conn))
            {
                var p = cmd.Parameters;
                p.Add(database.CreateParameter("@batch", bh.lotno.id));
                p.Add(database.CreateParameter("@module", bh.line.id));
                p.Add(database.CreateParameter("@prod_code", bh.product.id));
                p.Add(database.CreateParameter("@part", bh.part));
                p.Add(database.CreateParameter("@base", base_));
                IDataReader reader = cmd.ExecuteReader();
                List<focovision_sample> data = new List<focovision_sample>();
                while (reader.Read())
                {
                    data.Add(populategetAQLFocoFromReader(reader));
                }
                reader.Dispose();
                reader.Close();
                return data;
            }

        }
        /// <summary>
        /// Se ecarga de leer el resultado del procedure
        /// </summary>
        /// <param name="reader">Recibe los datos en un reader</param>
        /// <returns>Retorna los datos de las revisiones en el objeto qc_block_sample</returns>
        protected virtual focovision_sample populategetAQLFocoFromReader(IDataReader reader)
        {
            focovision_sample s = new focovision_sample(
                Convert.ToString((validate.getDefaultIfDBNull(reader["batch"], TypeCode.String))),
                Convert.ToString((validate.getDefaultIfDBNull(reader["module"], TypeCode.String))),
                Convert.ToString((validate.getDefaultIfDBNull(reader["base"], TypeCode.String))),
                Convert.ToInt32((validate.getDefaultIfDBNull(reader["batch_size"], TypeCode.Int32))),
                Convert.ToInt32((validate.getDefaultIfDBNull(reader["batch_sample"], TypeCode.Int32))));

            return s;
        }

        /// <summary>
        /// consulta la cantidad leida en focovision.
        /// </summary>
        /// <param name="datos"></param>
        /// <returns>Retorna la cantidad leida en foco</returns>
        public String getFocoReads(batch_detail ba)
        {
            using (IDbCommand cmd = database.CreateStoredProcCommand("getFocoReads", conn))
            {
                var p = cmd.Parameters;
                p.Add(database.CreateParameter("@batch", ba.lotno.id));
                p.Add(database.CreateParameter("@module", ba.line.id));
                p.Add(database.CreateParameter("@prod_code", ba.product.id));
                p.Add(database.CreateParameter("@part", ba.part));
                p.Add(database.CreateParameter("@base", ba.sku.base_));

                IDataReader reader = cmd.ExecuteReader();
                String resultado = String.Empty;
                while (reader.Read())
                {
                    resultado = reader["reads"].ToString();

                }
                reader.Dispose();
                reader.Close();
                return resultado.Trim();
            }
        }

        /// <summary>
        /// consulta todas las bases de un producto dado
        /// </summary>
        /// <param name="ba">recibe batch, linea,prod y parte en el objeto batch_detail</param>
        /// <returns>Retorna los datos en el objeto qc_block_sample</returns>
        public List<focovision_sample> getAllBases(batch bh)
        {
            using (IDbCommand cmd = database.CreateStoredProcCommand("getAllBases", conn))
            {
                var p = cmd.Parameters;
                p.Add(database.CreateParameter("@batch", bh.lotno.id));
                p.Add(database.CreateParameter("@module", bh.line.id));
                p.Add(database.CreateParameter("@prod_code", bh.product.id));
                p.Add(database.CreateParameter("@part", bh.part));
                
                IDataReader reader = cmd.ExecuteReader();

                List<focovision_sample> data = new List<focovision_sample>();
                while (reader.Read())
                {
                    data.Add(populategetAllBasesFromReader(reader));
                }
                reader.Dispose();
                reader.Close();
                return data;
            }

        }
        /// <summary>
        /// Se ecarga de leer el resultado del procedure
        /// </summary>
        /// <param name="reader">Recibe los datos en un reader</param>
        /// <returns>Retorna los datos de las revisiones en el objeto qc_block_sample</returns>
        protected virtual focovision_sample populategetAllBasesFromReader(IDataReader reader)
        {
            focovision_sample f = new focovision_sample();
            f.base_ = Convert.ToString((validate.getDefaultIfDBNull(reader["base"], TypeCode.String)));           

            return f;
        }


        #endregion

        #region Copy pallets
        /// <summary>
        /// Valida que la linea origen tenga datos y que la linea destino no tenga datos.
        /// </summary>
        /// <param name="datos">recibe los datos seleccionados en el objeto mold_lent_match</param>
        /// <returns>Retorna un String NO u OK segun el resultado de la validacion</returns>
        public String validateInfoLines(lot ba, line src, line tar, string tipo)
        {
            String resultado = String.Empty;
            using (IDbCommand cmd = database.CreateStoredProcCommand("validateInfoLines", conn))
            {
                cmd.Parameters.Add(database.CreateParameter("@batch", ba.id));
                if (tipo == "origen")
                {
                    cmd.Parameters.Add(database.CreateParameter("@line", src.id));
                }
                else
                {
                    cmd.Parameters.Add(database.CreateParameter("@line", tar.id));
                }

                cmd.Parameters.Add(database.CreateParameter("@tipo", tipo));

                IDataReader reader = cmd.ExecuteReader();

                while (reader.Read())
                {
                    resultado = reader["validacion"].ToString();

                }
                reader.Dispose();
                reader.Close();
                return resultado.Trim();
            }
        }
        /// <summary>
        /// Hace el llamado al procedimiento getPalletsOrigen, el cual retorna los los pallets a insertar.
        /// </summary>
        /// <param name="ba">recibe batch </param>
        /// <returns>Retorna los pallets en el objeto mold_lent_match</returns>
        public List<mold_lent_match> getPalletsOrigen(lot lt, line ln)
        {

            using (IDbCommand cmd = database.CreateStoredProcCommand("getPalletsOrigen", conn))
            {
                cmd.Parameters.Add(database.CreateParameter("@batch", lt.id));
                cmd.Parameters.Add(database.CreateParameter("@module", ln.id));
                IDataReader reader = cmd.ExecuteReader();
                List<mold_lent_match> data = new List<mold_lent_match>();
                while (reader.Read())
                {
                    data.Add(populategetPalletsOrigenFromReader(reader));

                }
                reader.Dispose();
                reader.Close();
                return data;
            }

        }
        /// <summary>
        /// Se ecarga de leer el resultado del procedure
        /// </summary>
        /// <param name="reader">Recibe los datos en un reader</param>
        /// <returns>Retorna los pallets origen en el objeto mold_lent_match</returns>
        protected virtual mold_lent_match populategetPalletsOrigenFromReader(IDataReader reader)
        {
            mold_lent_match m = new mold_lent_match();

            m.pallet.id = Convert.ToString((validate.getDefaultIfDBNull(reader["palletid"], TypeCode.String)));

            return m;
        }

        /// <summary>
        /// Valida que la linea destino no tenga insertado el pallet.
        /// </summary>
        /// <param name="datos">recibe los datos seleccionados en el objeto mold_lent_match</param>
        /// <returns>Retorna un String SI o NO si existe o no</returns>
        public String validExistPallet(lot ba, line ln, pallet pallet)
        {
            using (IDbCommand cmd = database.CreateStoredProcCommand("validExistPallet", conn))
            {
                var p = cmd.Parameters;
                p.Add(database.CreateParameter("@batch", ba.id));
                p.Add(database.CreateParameter("@module", ln.id));
                p.Add(database.CreateParameter("@pallet", int.Parse(pallet.id)));

                IDataReader reader = cmd.ExecuteReader();
                String resultado = String.Empty;
                while (reader.Read())
                {
                    resultado = reader["existe"].ToString();

                }
                reader.Dispose();
                reader.Close();
                return resultado.Trim();
            }
        }
        /// <summary>
        /// Inserta el pallet en la linea destino.
        /// </summary>
        /// <param name="datos">recibe los datos seleccionados en el objeto mold_lent_match</param>
        /// <returns>Retorna un String SI o NO si existe o no</returns>
        public String insertPalletbetweenlines(lot ba, line src, line tar, pallet pallet)
        {
            using (IDbCommand cmd = database.CreateStoredProcCommand("insertPallets", conn))
            {
                var p = cmd.Parameters;
                p.Add(database.CreateParameter("@batch", ba.id));
                p.Add(database.CreateParameter("@line_origen", src.id));
                p.Add(database.CreateParameter("@line_des", tar.id));
                p.Add(database.CreateParameter("@pallet", int.Parse(pallet.id)));

                IDataReader reader = cmd.ExecuteReader();
                String resultado = String.Empty;
                while (reader.Read())
                {
                    resultado = reader["inserto"].ToString();

                }
                reader.Dispose();
                reader.Close();
                return resultado.Trim();
            }
        }


        #endregion
    }
}

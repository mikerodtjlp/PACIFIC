using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using mro;
using mro.db;
using sfc.BO;
using sfc.BL;
using System.Data;

namespace sfc.DAL
{
    public class packaging_DAL : DataWorker, IDisposable
    {
        #region database setup

        //private validate validate = validate.getInstance();
        private IDbConnection conn = null;

        public static packaging_DAL instance(string name = "main")
        {
            return new packaging_DAL(name);
        }
        public packaging_DAL(string name): base(name)
        {
            conn = database.CreateOpenConnection();
        }
        void IDisposable.Dispose()
        {
            conn.Close();
            conn.Dispose();
        }
        #endregion


        public string insert_and_return_bulk_pack(batch b, resource res, int q, int m)
        {
            var qry = new StringBuilder("execute insert_bulk_pack_3 ");
            qry.AppendFormat(" '{0}','{1}','{2}','{3}','{4}',{5},{6},'' ",
                                b.lotno.id, b.line.id, b.part, b.product.id, res.id, q, m);
            string bpack_id = "";
            using (var cmd = database.CreateCommand(qry.ToString(), conn))
            {
                for (var r = cmd.ExecuteReader(); r.Read(); )
                {
                    bpack_id = r["bpack_id"].ToString();
                    break;
                }
            }
            return bpack_id;
        }

        public lot_number generate_shipment(batch b, location l, string lnum)
        {
            var qry = new StringBuilder("execute generashipment_lean ");
            qry.AppendFormat(" '{0}{1}{2}{3}','{4}','{5}', 0 ",
                                b.lotno.id, b.line.id, b.part, b.product.id, l.id, lnum);
            lot_number obj = null;
            using (var cmd = database.CreateCommand(qry.ToString(), conn))
            {
                for (var r = cmd.ExecuteReader(); r.Read(); )
                {
                    obj = new lot_number(r["lotnumber"].ToString());
                    break;
                }
            }
            return obj;
        }

        public string close_trans_batch(batch b, location l)
        {
            var qry = new StringBuilder("execute sp_batch_close_lean ");
            qry.AppendFormat(" '{0}{1}{2}{3}','{4}', 0 ",
                                b.lotno.id, b.line.id, b.part, b.product.id, l.id);
            var isinwip = "";
            using (var cmd = database.CreateCommand(qry.ToString(), conn))
            {
                for (var r = cmd.ExecuteReader(); r.Read(); )
                {
                    isinwip = r["isinwip"].ToString();
                    break;
                }
            }
            return isinwip;
        }


        public bulk_pack_upload get_box_to_upload(bulk_pack_upload b)
        {
            var qry = new StringBuilder("select * from bulk_pack_upload with (nolock) ");
            qry.AppendFormat(" where batch='{0}' and prod_code='{1}' and line_id='{2}' and part='{3}' and bulk_pack_id='{4}' ",
                                b.lotno.id, b.product.id, b.line.id, b.part, b.bulk_pack_id);
            bulk_pack_upload obj = null;
            using (var cmd = database.CreateCommand(qry.ToString(), conn))
            {
                for (var r = cmd.ExecuteReader(); r.Read(); )
                {
                    obj = new bulk_pack_upload(r["batch"].ToString(),
                                            r["prod_code"].ToString(),
                                            r["line_id"].ToString(),
                                            r["part"].ToString());
                    obj.bulk_pack_id = r["bulk_pack_id"].ToString();
                    obj.status = r["status"].ToString();
                    obj.fecha = (DateTime)r["fecha"];
                    break;
                }
            }
            return obj;
        }
        public void insert_box_to_upload(bulk_pack_upload b)
        {
            var qry = new StringBuilder("insert into bulk_pack_upload ");
            qry.AppendFormat(" values('{0}', '{1}', '{2}', '{3}', '{4}', '{5}', getdate()) ",
                                b.lotno.id, b.product.id, b.line.id, b.part, b.bulk_pack_id, b.status);
            using (var cmd = database.CreateCommand(qry.ToString(), conn))
            {
                cmd.ExecuteNonQuery();
            }
        }
        public void update_box_to_upload(bulk_pack_upload b)
        {
            var qry = new StringBuilder("update bulk_pack_upload ");
            qry.AppendFormat(" set status='{0}' " +
                                " where batch='{1}' and prod_code='{2}' and line_id='{3}' and part='{4}' and bulk_pack_id='{5}' ",
                                b.status, 
                                b.lotno.id, b.product.id, b.line.id, b.part, b.bulk_pack_id);
            using (var cmd = database.CreateCommand(qry.ToString(), conn))
            {
                cmd.ExecuteNonQuery();
            }
        }

        public bulk_pack_dtl get_bulk_pack_dtl_by_id(string bulk_pack_id)
        {
            var qry = new StringBuilder("select batch, prod_code, line_id, part, cycle," +
                                        "RTRIM(bulk_pack_id) as bulk_pack_id, RTRIM(resource) as resource,"+
                                        "quantity, prod_order from bulk_pack_dtl with (nolock) ");
            qry.AppendFormat(" where bulk_pack_id='{0}' ",
                                bulk_pack_id);
            bulk_pack_dtl obj = null;
            using (var cmd = database.CreateCommand(qry.ToString(), conn))
            {
                for (var r = cmd.ExecuteReader(); r.Read(); )
                {
                    obj = new bulk_pack_dtl(r["batch"].ToString(),
                                            r["prod_code"].ToString(),
                                            r["line_id"].ToString(),
                                            r["part"].ToString());
                    obj.cycle = int.Parse(r["cycle"].ToString());
                    obj.bulk_pack_id = r["bulk_pack_id"].ToString();
                    obj.resource_ = new resource(r["resource"].ToString());
                    obj.qty = int.Parse(r["quantity"].ToString());
                    obj.prod_order = r["prod_order"].ToString();
                    break;
                }
            }
            return obj;
        }
        public void insert_bulk_pack_dtl(bulk_pack_dtl b)
        {
            var qry = new StringBuilder("insert into bulk_pack_dtl ");
            qry.AppendFormat(" (batch, prod_code, line_id, part, cycle, bulk_pack_id, resource, quantity, prod_order) " +
                                " values('{0}','{1}','{2}','{3}',{4},'{5}','{6}',{7},'{8}') ",
                                b.lotno.id, b.product.id, b.line.id, b.part, b.cycle, b.bulk_pack_id, b.resource_.id, b.qty, b.prod_order);
            using (var cmd = database.CreateCommand(qry.ToString(), conn))
            {
                cmd.ExecuteNonQuery();
            }
        }
        public void update_bulk_pack_dtl(bulk_pack_dtl b)
        {
            var qry = new StringBuilder("update bulk_pack_dtl ");
            qry.AppendFormat(" set resource='{0}', quantity={1}, prod_order='{2}' " +
                                " where batch='{3}' and prod_code='{4}' and line_id='{5}' and part='{6}' and bulk_pack_id='{7}' ",
                                b.resource_.id, b.qty, b.prod_order,
                                b.lotno.id, b.product.id, b.line.id, b.part, b.bulk_pack_id);
            using (var cmd = database.CreateCommand(qry.ToString(), conn))
            {
                cmd.ExecuteNonQuery();
            }
        }

        #region package image

        /// <summary>
        /// verifica si el sku esta planeado en la tabla t_relationtable.
        /// </summary>
        /// <param name="datos">batch, modulo y sku</param>
        /// <returns>Retorna un String, SI si existe o NO si no existe</returns>
        public String getPlanned(lot lt, line ln, resource sku)
        {
            using (IDbCommand cmd = database.CreateStoredProcCommand("getPlanned", conn))
            {
                var p = cmd.Parameters;

                p.Add(database.CreateParameter("@batch", lt.id));
                p.Add(database.CreateParameter("@module", ln.id));
                p.Add(database.CreateParameter("@sku", sku.id));

                IDataReader reader = cmd.ExecuteReader();
                String resultado = String.Empty;
                while (reader.Read())
                {
                    resultado = reader["planned"].ToString();

                }
                reader.Dispose();
                reader.Close();
                return resultado.Trim();
            }
        }

        /// <summary>
        /// Consulta si existe creada la relacion para el lote y modulo seleccionado.
        /// </summary>
        /// <param name="datos">batch y modulo</param>
        /// <returns>Retorna la cantidad de registros de la nueva tabla t_image_sku_relation</returns>
        public String getSkuRelCount(lot lt, line ln)
        {
            using (IDbCommand cmd = database.CreateStoredProcCommand("getSkuRelCount", conn))
            {
                cmd.Parameters.Add(database.CreateParameter("@batch", lt.id));
                cmd.Parameters.Add(database.CreateParameter("@module", ln.id));

                IDataReader reader = cmd.ExecuteReader();
                String resultado = String.Empty;
                while (reader.Read())
                {
                    resultado = reader["countrel"].ToString();

                }
                reader.Dispose();
                reader.Close();
                return resultado.Trim();
            }
        }

        /// <summary>
        /// Consulta el sku en la nueva tabla t_image_sku_relation.
        /// </summary>
        /// <param name="datos">batch, modulo y sku</param>
        /// <returns>si existe retorna SI de lo contrario retorna NO</returns>
        public String existSkuOnRel(lot lt, line ln, resource sku)
        {
            using (IDbCommand cmd = database.CreateStoredProcCommand("existSkuOnRel", conn))
            {
                cmd.Parameters.Add(database.CreateParameter("@batch", lt.id));
                cmd.Parameters.Add(database.CreateParameter("@module", ln.id));
                cmd.Parameters.Add(database.CreateParameter("@sku", sku.id));

                IDataReader reader = cmd.ExecuteReader();
                String resultado = String.Empty;
                while (reader.Read())
                {
                    resultado = reader["existsku"].ToString();

                }
                reader.Dispose();
                reader.Close();
                return resultado.Trim();
            }
        }

        /// <summary>
        /// Inserta los sku del plan de pallet cuando la bandera action=new o inserta el sku que recibe de parametro al final de la lista cuando la bandera action=last.
        /// </summary>
        /// <param name="datos">batch,modulo,sku y action</param>
        /// <returns>Retorna un String, nombre de la imagen si existe o NO si no existe</returns>
        public void insertSkuImgRel(lot lt, line ln, resource sku, string action)
        {
            using (IDbCommand cmd = database.CreateStoredProcCommand("insertSkuImgRel", conn))
            {
                var p = cmd.Parameters;
                p.Add(database.CreateParameter("@batch", lt.id));
                p.Add(database.CreateParameter("@module", ln.id));
                p.Add(database.CreateParameter("@sku", sku.id));
                p.Add(database.CreateParameter("@action", action));
                cmd.ExecuteNonQuery();
            }
        }

        /// <summary>
        /// obtiene la imagen de un sku.
        /// </summary>
        /// <param name="datos">recibe el sku leido</param>
        /// <returns>Retorna un String, nombre de la imagen si existe o NO si no existe</returns>
        public String getImage(lot lt, line ln, resource sku)
        {
            using (IDbCommand cmd = database.CreateStoredProcCommand("getImage", conn))
            {
                cmd.Parameters.Add(database.CreateParameter("@batch", lt.id));
                cmd.Parameters.Add(database.CreateParameter("@module", ln.id));
                cmd.Parameters.Add(database.CreateParameter("@sku", sku.id));

                IDataReader reader = cmd.ExecuteReader();
                String resultado = String.Empty;
                while (reader.Read())
                {
                    resultado = reader["imagen"].ToString();

                }
                reader.Dispose();
                reader.Close();
                return resultado.Trim();
            }
        }

        /// <summary>
        /// retorna los nuevos sku planeados por reemplazo de moldes que no existen en la relacion image-sku.
        /// </summary> 
        /// <param name="datos">recibe: lote y linea</param>
        /// <returns>Retorna batch,linea y sku nuevo</returns>
        public Tuple<lot, line, resource> getNewSku(lot lt, line ln)
        {
            Tuple<lot, line, resource> l_img = null;
            using (IDbCommand cmd = database.CreateStoredProcCommand("getNewSku", conn))
            {
                cmd.Parameters.Add(database.CreateParameter("@batch", lt.id));
                cmd.Parameters.Add(database.CreateParameter("@module", ln.id));
                IDataReader reader = cmd.ExecuteReader();
                l_img = populateGetNewSkuFromReader(reader);
                reader.Dispose();
                reader.Close();
            }
            return l_img;
        }

        /// <summary>
        /// Guarda en el objeto imagenes todo lo consultado
        /// </summary> 
        /// <param name="datos">recibe: reader</param>
        /// <returns>Retorna el objeto lista</returns>
        private Tuple<lot, line, resource> populateGetNewSkuFromReader(IDataReader reader)
        {
            return Tuple.Create(new lot(Convert.ToString(validate.getDefaultIfDBNull(reader["batch"], TypeCode.String))),
                                new line(Convert.ToString(validate.getDefaultIfDBNull(reader["module"], TypeCode.String))),
                                new resource(Convert.ToString(validate.getDefaultIfDBNull(reader["resource"], TypeCode.String))));
        }

        /// <summary>
        /// Retorna la lista de los sku que tienen su imagen relacionada
        /// </summary> 
        /// <param name="datos">recibe: lote y linea</param>
        /// <returns>Retorna batch,linea e imagen</returns>
        public Tuple<lot, line, resource, imagen> getRelImgSku(lot lt, line ln)
        {
            Tuple<lot, line, resource,imagen> l_img = null;
            using (IDbCommand cmd = database.CreateStoredProcCommand("getRelImgSku", conn))
            {
                cmd.Parameters.Add(database.CreateParameter("@batch", lt.id));
                cmd.Parameters.Add(database.CreateParameter("@module", ln.id));
                IDataReader reader = cmd.ExecuteReader();
                l_img = populateGetRelImgSkuFromReader(reader);
                reader.Dispose();
                reader.Close();
            }
            return l_img;
        }

        /// <summary>
        /// Guarda en el objeto imagenes todo lo consultado
        /// </summary> 
        /// <param name="datos">recibe: reader</param>
        /// <returns>Retorna el objeto lista</returns>
        private Tuple<lot,line,resource,imagen> populateGetRelImgSkuFromReader(IDataReader reader)
        {
            return Tuple.Create(new lot(Convert.ToString(validate.getDefaultIfDBNull(reader["batch"], TypeCode.String))),
                                new line(Convert.ToString(validate.getDefaultIfDBNull(reader["module"], TypeCode.String))),
                                new resource(Convert.ToString(validate.getDefaultIfDBNull(reader["resource"], TypeCode.String))),
                                new imagen(Convert.ToString(validate.getDefaultIfDBNull(reader["imagen"], TypeCode.String))));
        }

        /// <summary>
        /// actualiza el sku que indica el usuario
        /// </summary> 
        /// <param name="datos">recibe: lote, linea, sku anterior y sku nuevo</param>
        /// <returns>Retorna nada</returns>
        public void updateSku(lot lt, line ln, resource sku, string sku_new)
        {
            using (IDbCommand cmd = database.CreateStoredProcCommand("updateSkuImage", conn))
            {
                var p = cmd.Parameters;
                p.Add(database.CreateParameter("@batch", lt.id));
                p.Add(database.CreateParameter("@module", ln.id));
                p.Add(database.CreateParameter("@sku", sku.id));
                p.Add(database.CreateParameter("@sku_new", sku_new));
                cmd.ExecuteNonQuery();
            }
        }

        #endregion

    }
}

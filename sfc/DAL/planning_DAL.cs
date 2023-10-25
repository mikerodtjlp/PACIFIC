using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using mro;
using mro.db;
using mro.BO;
using mro.BL;
using sfc.BO;
using sfc.BL;
using System.Data;

namespace sfc.DAL
{
    public class planning_DAL : DataWorker, IDisposable
    {
        #region database setup

        //private validate validate = validate.getInstance();
        private IDbConnection conn = null;
        //private IDbTransaction txn = null;

        public static planning_DAL instance(string name = "main")
        {
            return new planning_DAL(name);
        }
        public planning_DAL(string name): base(name)
        {
            conn = database.CreateOpenConnection();
        }

        public void begin_transaction()
        {
            //txn = conn.BeginTransaction();
        }
        public void commit_transaction()
        {
            //txn.Commit();
        }
        public void rollback_transaction()
        {
            //txn.Rollback();
        }
        void IDisposable.Dispose()
        {
            conn.Close();
            conn.Dispose();
            //if (txn != null) { txn.Dispose(); }

        }
        #endregion

        public production_order_m2o get_production_order_m2o(string po)
        {
            using (IDbCommand cmd = database.CreateStoredProcCommand("get_production_order", conn))
            {
                cmd.Parameters.Add(database.CreateParameter("@docid", po));

                var obj = new production_order_m2o();

                for (var r = cmd.ExecuteReader(); r.Read(); )
                {
                    obj.docid = r["docid"].ToString();
                    obj.status = r["status"].ToString();
                    obj.creationdate = (DateTime)r["creationdate"];
                    obj.duedate = (DateTime)r["duedate"];
                    obj.finishdate = (DateTime)r["finishdate"];
                    obj.priority = int.Parse(r["priority"].ToString());
                }

                return obj;
            }
        }
        public production_order_m2o insert_production_order_m2o_detail(production_order_m2o pod)
        {
            throw new System.NotImplementedException();
        }
        public void update_production_order_m2o_detail(production_order_m2o pod)
        {
            throw new System.NotImplementedException();
        }
        public void delete_production_order_m2o_detail(production_order_m2o pod)
        {
            throw new System.NotImplementedException();
        }
        public production_order_m2o populate_production_order_m2o(IDataReader reader)
        {
            production_order_m2o po = null;

            while (reader.Read())
            {
                po = new production_order_m2o();

                po.docid = reader["docid"].ToString();
                po.sku = reader["sku"].ToString();
                po.qty = int.Parse(reader["qty"].ToString());
                po.waited = int.Parse(reader["waited"].ToString());
                po.acum = int.Parse(reader["acum"].ToString());
                po.excess = int.Parse(reader["excess"].ToString());
                po.status = reader["status"].ToString();
                po.creationdate = (DateTime)reader["creationdate"];
                po.duedate = (DateTime)reader["duedate"];
                po.finishdate = (DateTime)validate.getDefaultIfDBNull(reader["finishdate"], TypeCode.DateTime);
                po.priority = int.Parse(reader["priority"].ToString());
            }
            return po;
        }
        public production_order_m2o get_one_opened_po_m2o(resource res_)
        {
            production_order_m2o po = null;  
            using (IDbCommand cmd = database.CreateStoredProcCommand("pkg_get_one_opened_po", conn))
            {
                cmd.Parameters.Add(database.CreateParameter("@sku", res_.id));
                var reader = cmd.ExecuteReader();
                po = populate_production_order_m2o(reader);
            }
            return po;
        }
        public production_order_m2o get_one_new_po_m2o(resource res_)
        {
            production_order_m2o po = null;
            using (IDbCommand cmd = database.CreateStoredProcCommand("pkg_get_one_new_po", conn))
            {
                cmd.Parameters.Add(database.CreateParameter("@sku", res_.id));
                var reader = cmd.ExecuteReader();
                po = populate_production_order_m2o(reader);
            }
            return po;
        }
        public void update_production_order_m2o(production_order_m2o po)
        {
            using (IDbCommand cmd = database.CreateStoredProcCommand("update_production_order", conn))
            {
                var p = cmd.Parameters;

                p.Add(database.CreateParameter("@docid", po.docid));
                p.Add(database.CreateParameter("@sku", po.sku));
                p.Add(database.CreateParameter("@qty", po.qty));
                p.Add(database.CreateParameter("@waited", po.waited));
                p.Add(database.CreateParameter("@acum", po.acum));
                p.Add(database.CreateParameter("@excess", po.excess));
                p.Add(database.CreateParameter("@status", po.status));
                p.Add(database.CreateParameter("@creationdate", po.creationdate));
                p.Add(database.CreateParameter("@duedate", po.duedate));
                p.Add(database.CreateParameter("@finishdate", po.finishdate));
                p.Add(database.CreateParameter("@priority", po.priority));

                validate.evaluateParameters(p, false);

                cmd.ExecuteNonQuery();
            }
        }
        public production_order_m2o get_one_released_po_m2o(resource res_)
        {
            production_order_m2o po = null;  
            using (IDbCommand cmd = database.CreateStoredProcCommand("pkg_get_one_released_po", conn))
            {
                cmd.Parameters.Add(database.CreateParameter("@sku", res_.id));
                var reader = cmd.ExecuteReader();
                po = populate_production_order_m2o(reader);
            }
            return po;
        }
        public int get_open_orders_m2o_count(resource res_)
        {
            var howmanyopened = 0;

            using (IDbCommand cmd = database.CreateStoredProcCommand("pkg_get_open_orders_count", conn))
            {
                cmd.Parameters.Add(database.CreateParameter("@sku", res_.id));

                for (var r = cmd.ExecuteReader(); r.Read(); )
                {
                    howmanyopened = int.Parse(r["res"].ToString());
                }
            }
            return howmanyopened;
        }
        public int get_new_orders_m2o_count(resource res_)
        {
            var howmanyopened = 0;

            using (IDbCommand cmd = database.CreateStoredProcCommand("pkg_get_new_orders_count", conn))
            {
                cmd.Parameters.Add(database.CreateParameter("@sku", res_.id));

                for (var r = cmd.ExecuteReader(); r.Read(); )
                {
                    howmanyopened = int.Parse(r["res"].ToString());
                }
            }
            return howmanyopened;
        }


        public production_order get_one_po_opened(resource res_)
        {
            var qry = new StringBuilder("select * from t_orders_arrived with (nolock) ");
            qry.AppendFormat(" where item='{0}' and status='W' ", res_.id);
            production_order obj = null;
            using (var cmd = database.CreateCommand(qry.ToString(), conn))
            {
                for (var r = cmd.ExecuteReader(); r.Read(); )
                {
                    obj = new production_order();

                    obj.year = r["year"].ToString();
                    obj.week = r["week"].ToString();
                    obj.line_id = r["line_id"].ToString();

                    obj.docid = r["order_number"].ToString();
                    obj.sku = r["item"].ToString();
                    obj.qty = int.Parse(r["line_id"].ToString());
                    obj.acum = int.Parse(r["acum"].ToString());

                    obj.startdate =(DateTime)r["start_date"];
                    obj.duedate = (DateTime)r["due_date"];
                    obj.finishdate = (DateTime)r["finish_date"];

                    obj.status = r["status"].ToString();
                    obj.mold_id = r["mold_id"].ToString();
                    obj.size_type = r["size_type"].ToString();
                    obj.qty_molds = int.Parse(r["qty_molds"].ToString());

                    obj.mon = int.Parse(r["mon"].ToString());
                    obj.tue = int.Parse(r["tue"].ToString());
                    obj.wed = int.Parse(r["wed"].ToString());
                    obj.thu = int.Parse(r["thu"].ToString());
                    obj.fri = int.Parse(r["fri"].ToString());
                    obj.days_need = int.Parse(r["days_need"].ToString());

                    obj.priority = int.Parse(r["priority"].ToString());
                    obj.processed = int.Parse(r["priority"].ToString()) == 1;

                    break;
                }
            }
            return obj;
        }
        public void update_production_order(production_order po)
        {
            var qry = new StringBuilder("update t_orders_arrived");
            qry.AppendFormat("set acum={0}, status='{1}' where order_number='{2}' ",
                             po.acum, po.status, po.docid);
            using (var cmd = database.CreateCommand(qry.ToString(), conn))
            {
                cmd.ExecuteNonQuery();
            }
        }


        public List<product> get_products_planned_by_line(lot lt, line ln)
        {
            var data = new List<product>();
            var qry = new StringBuilder("select distinct prod_code from t_plan_molds with (nolock) ");
            qry.AppendFormat(" where batch= '{0}' and line_id='{1}' order by prod_code ASC ", lt.id, ln.id);
            using (var cmd = database.CreateCommand(qry.ToString(), conn))
            {
                for (var r = cmd.ExecuteReader(); r.Read(); )
                {
                    var obj = new product(r["prod_code"].ToString());
                    data.Add(obj);
                }
            }
            return data;
        }
        public List<production_plan> get_production_plan_by_line(lot lt, line ln)
        {
            var data = new List<production_plan>();
            var qry = new StringBuilder("select * from t_plan_molds with (nolock) ");
            qry.AppendFormat(" where batch= '{0}' and line_id='{1}' order by prod_code ASC ", 
                            lt.id, ln.id);
            using (var cmd = database.CreateCommand(qry.ToString(), conn))
            {
                for (var r = cmd.ExecuteReader(); r.Read(); )
                {
                    var obj = new production_plan(r["batch"].ToString(), r["prod_code"].ToString(),
                                                r["line_id"].ToString(), r["resource"].ToString());
                    obj.qty = int.Parse(r["qty"].ToString());
                    obj.plan_cst = int.Parse(r["plan_cst"].ToString());
                    obj.acum = int.Parse(r["acum"].ToString());
                    obj.band = int.Parse(r["band"].ToString());
                    data.Add(obj);
                }
            }
            return data;
        }
        public List<production_plan> get_production_plan_by_prod_line
                                        (lot lt, product pr, line ln)
        {
            var data = new List<production_plan>();
            var qry = new StringBuilder("select * from t_plan_molds with (nolock) ");
            qry.AppendFormat(   "where batch='{0}' and prod_code='{1}' and line_id='{2}' ",
                                lt.id, pr.id, ln.id);
            using (var cmd = database.CreateCommand(qry.ToString(), conn))
            {
                for (var r = cmd.ExecuteReader(); r.Read(); )
                {
                    var obj = new production_plan(r["batch"].ToString(), r["prod_code"].ToString(),
                                                r["line_id"].ToString(), r["resource"].ToString());
                    obj.qty = int.Parse(r["qty"].ToString());
                    obj.plan_cst = int.Parse(r["plan_cst"].ToString());
                    obj.acum = int.Parse(r["acum"].ToString());
                    obj.band = int.Parse(r["band"].ToString());
                    data.Add(obj);
                }
            }
            return data;
        }

        #region plan molds
        public production_plan populate_production_plan(IDataReader reader)
        {
            production_plan obj = null;

            while (reader.Read())
            {
                obj = new production_plan(  reader["batch"].ToString(), reader["prod_code"].ToString(),
                                            reader["line_id"].ToString(), reader["resource"].ToString());
                obj.qty = int.Parse(reader["qty"].ToString());
                obj.plan_cst = int.Parse(reader["plan_cst"].ToString());
                obj.acum = int.Parse(reader["acum"].ToString());
                obj.band = int.Parse(reader["band"].ToString());
            }
            return obj;
        }

        public bool exist_production_plan_any(production_plan p)
        {
            var qry = new StringBuilder("if(exists(select top 1 resource from t_plan_molds with (nolock) ");
            qry.AppendFormat("where batch='{0}' and prod_code='{1}' and line_id='{2}')) " + 
                            "select 1 as res else select 0 as res ", 
                            p.lotno.id, p.product.id, p.line.id);
            using (var cmd = database.CreateCommand(qry.ToString(), conn))
            {
                var r = cmd.ExecuteReader();
                while (r.Read())
                {
                    return r.GetInt32(0) == 1;
                }
                return false;
            }
        }
        public bool exist_production_plan_detail(production_plan p)
        {
            var qry = new StringBuilder("if(exists(select resource as res from t_plan_molds with (nolock) ");
            qry.AppendFormat("where batch='{0}' and prod_code='{1}' and line_id='{2}' and resource='{3}')) " +
                            "select 1 as res else select 0 as res ",
                            p.lotno.id, p.product.id, p.line.id, p.resource_.id);
            using (var cmd = database.CreateCommand(qry.ToString(), conn))
            {
                var r = cmd.ExecuteReader();
                while (r.Read())
                {
                    return r.GetInt32(0) == 1;
                }
                return false;
            }
        }

        public production_plan get_production_plan(production_plan p)
        {
            production_plan pp = null;
            var qry = new StringBuilder("select * from t_plan_molds with (nolock) ");
            qry.AppendFormat(" where batch='{0}' and prod_code='{1}' and line_id='{2}' and resource='{3}'",
                            p.lotno.id, p.product.id, p.line.id, p.resource_.id);
            using (var cmd = database.CreateCommand(qry.ToString(), conn))
            {
                var reader = cmd.ExecuteReader();
                pp = populate_production_plan(reader);
            }
            return pp;
        }
        public void insert_production_plan(production_plan p)
        {
            var qry = new StringBuilder("insert into t_plan_molds");
            qry.AppendFormat(" values('{0}', '{1}', '{2}', '{3}', {4}, {5}, {6}, {7}) ",
                                p.lotno.id, p.line.id, p.product.id, p.resource_.id,
                                p.qty, p.plan_cst, p.acum, p.band);
            using (var cmd = database.CreateCommand(qry.ToString(), conn))
            {
                cmd.ExecuteNonQuery();
            }
        }
        public void update_production_plan(production_plan p)
        {
            var qry = new StringBuilder("update t_plan_molds");
            qry.AppendFormat(" set qty={0}, plan_cst={1}, acum={2}, band={3} " +
                            " where batch='{4}' and prod_code='{5}' and line_id='{6}' and resource='{7}'",
                                p.qty, p.plan_cst, p.acum, p.band,
                                p.lotno.id, p.product.id, p.line.id, p.resource_.id);
            using (var cmd = database.CreateCommand(qry.ToString(), conn))
            {
                cmd.ExecuteNonQuery();
            }
        }
        public void delete_production_plan(production_plan p)
        {
            var qry = new StringBuilder("delete t_plan_molds");
            qry.AppendFormat(" where batch='{0}' and prod_code='{1}' and line_id='{2}' and resource='{3}'",
                                p.lotno.id, p.product.id, p.line.id, p.resource_.id);
        }

        #endregion

        #region urgentSKU
        /// <summary>
        /// Consulta los sku cargados desde el archivo para una familia y fecha seleccionados
        /// </summary>
        /// <param name="file"></param>
        /// <returns>El listado de sku cargados</returns>
        public List<item_urgent2> getSkuFileMembersLoaded(string famIni, string famFin, string planner, DateTime dateIni, DateTime dateFin)
        {
            using (IDbCommand cmd = database.CreateStoredProcCommand("getSkuFileMembersLoaded", conn))
            {
                var p = cmd.Parameters;

                p.Add(database.CreateParameter("@family_ini", famIni));
                p.Add(database.CreateParameter("@family_fin", famFin));
                p.Add(database.CreateParameter("@created_by", planner));
                p.Add(database.CreateParameter("@creation_date_ini", dateIni));
                p.Add(database.CreateParameter("@creation_date_fin", dateFin));
                IDataReader reader = cmd.ExecuteReader();
                List<item_urgent2> data = new List<item_urgent2>();
                while (reader.Read())
                {
                    data.Add(populategetFileLoadedFromReader(reader));
                }
                reader.Dispose();
                reader.Close();

                return data;

            }
        }


        /// <summary>
        /// Lee lo retornado en el objeto file
        /// </summary>
        /// <param name="reader"></param>
        /// <returns></returns>
        protected virtual item_urgent2 populategetFileLoadedFromReader(IDataReader reader)
        {
            item_urgent2 sf = new item_urgent2();

            sf.fam.id = Convert.ToString((validate.getDefaultIfDBNull(reader["family"], TypeCode.String)));
            sf.sku.id = Convert.ToString((validate.getDefaultIfDBNull(reader["resource"], TypeCode.String)));
            sf.planner = Convert.ToString((validate.getDefaultIfDBNull(reader["created_by"], TypeCode.String)));
            sf.creation_date = Convert.ToDateTime((validate.getDefaultIfDBNull(reader["creation_date"], TypeCode.DateTime)));

            return sf;

        }

        /// <summary>
        /// Inserta la relacion sku-familia que vienen en el archivo de excel
        /// </summary>
        /// <param name="file"></param>
        /// <returns></returns>
        public void insertSKUFam(item_urgent2 file)
        {
            using (IDbCommand cmd = database.CreateStoredProcCommand("insertSKUFam", conn))
            {
                var p = cmd.Parameters;

                p.Add(database.CreateParameter("@family", file.fam.id));
                p.Add(database.CreateParameter("@resource", file.sku.id));
                p.Add(database.CreateParameter("@created_by", file.planner));

                cmd.ExecuteNonQuery();
            }
        }


        /// <summary>
        /// Inserta el sku
        /// </summary>
        /// <param name="file"></param>
        /// <returns></returns>
        public void insertSKU(resource sku)
        {
            using (IDbCommand cmd = database.CreateStoredProcCommand("insertSKU", conn))
            {
                cmd.Parameters.Add(database.CreateParameter("@resource", sku.id));
                cmd.ExecuteNonQuery();
            }
        }


        /// <summary>
        /// Elimina los sku que pertenecen a la familia indicada
        /// </summary>
        /// <param name="file"></param>
        /// <returns></returns>
        public int deleteSkuFam(family fam)
        {
            using (IDbCommand cmd = database.CreateStoredProcCommand("deleteSkuFam", conn))
            {
                cmd.Parameters.Add(database.CreateParameter("@family", fam.id));

                return cmd.ExecuteNonQuery();
            }
        }

        /// <summary>
        /// Elimina un sku en especifico desde la tabla t_items_urg
        /// </summary>
        /// <param name="file"></param>
        /// <returns></returns>
        public int deleteSku(family fam)
        {
            using (IDbCommand cmd = database.CreateStoredProcCommand("deleteSku", conn))
            {
                cmd.Parameters.Add(database.CreateParameter("@family", fam.id));

                return cmd.ExecuteNonQuery();
            }
        }

        /// <summary>
        /// Elimina un sku en especifico por familia y sku
        /// </summary>
        /// <param name="file"></param>
        /// <returns></returns>
        public int deleteSkuSelected(item_urgent2 file)
        {
            using (IDbCommand cmd = database.CreateStoredProcCommand("deleteSkuSelected", conn))
            {
                cmd.Parameters.Add(database.CreateParameter("@family", file.fam.id));
                cmd.Parameters.Add(database.CreateParameter("@resource", file.sku.id));
                cmd.Parameters.Add(database.CreateParameter("@created_by", file.planner));
                //cmd.Parameters.Add(database.CreateParameter("@creation_date", file.creation_date));                

                return cmd.ExecuteNonQuery();
            }
        }

        /// <summary>
        /// Obtiene la familia del producto del sku
        /// </summary>
        /// <param name="file"></param>
        /// <returns></returns>
        public family getProdFamily(family f, product p)
        {
            family fam = null;
            using (IDbCommand cmd = database.CreateStoredProcCommand("getProdFamily", conn))
            {
                cmd.Parameters.Add(database.CreateParameter("@fam", f.id));
                cmd.Parameters.Add(database.CreateParameter("@prod_code", p.id));                
                IDataReader reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    fam = new family(reader["family"].ToString().Trim());
                }
                reader.Dispose();
                reader.Close();

                return fam;
            }
        }


        /// <summary>
        /// Valida si existe o no el sku en las tablas
        /// </summary>
        /// <param name="file"></param>
        /// <returns></returns>
        public List<resource> getSKU(resource sku)
        {
            using (IDbCommand cmd = database.CreateStoredProcCommand("getSKU", conn))
            {
                cmd.Parameters.Add(database.CreateParameter("@resource", sku.id));
                List<resource> data = new List<resource>();
                IDataReader reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    data.Add(populategetSKUFromReader(reader));
                }
                reader.Dispose();
                reader.Close();

                return data;

            }
        }
        /// <summary>
        /// Lee lo retornado en el objeto file
        /// </summary>
        /// <param name="reader"></param>
        /// <returns></returns>
        protected virtual resource populategetSKUFromReader(IDataReader reader)
        {
            return new resource(Convert.ToString((validate.getDefaultIfDBNull(reader["resource"], TypeCode.String))));
        }

        /// <summary>
        /// Valida si existe o no la relacion sku-familia en las tablas
        /// </summary>
        /// <param name="file"></param>
        /// <returns></returns>
        public List<item_urgent2> getSKUFam(item_urgent2 file)
        {
            using (IDbCommand cmd = database.CreateStoredProcCommand("getSKUFam", conn))
            {
                cmd.Parameters.Add(database.CreateParameter("@family", file.fam.id));
                cmd.Parameters.Add(database.CreateParameter("@resource", file.sku.id));
                IDataReader reader = cmd.ExecuteReader();
                List<item_urgent2> data = new List<item_urgent2>();
                while (reader.Read())
                {
                    data.Add(populategetSKUFamFromReader(reader));
                }
                reader.Dispose();
                reader.Close();
                return data;
            }
        }

        /// <summary>
        /// Lee lo retornado en el objeto file
        /// </summary>
        /// <param name="reader"></param>
        /// <returns></returns>
        protected virtual item_urgent2 populategetSKUFamFromReader(IDataReader reader)
        {
            item_urgent2 sf = new item_urgent2();

            sf.fam.id = Convert.ToString((validate.getDefaultIfDBNull(reader["family"], TypeCode.String)));
            sf.sku.id = Convert.ToString((validate.getDefaultIfDBNull(reader["resource"], TypeCode.String)));

            return sf;
        }

        /// <summary>
        /// Consulta las familias de un usuario en especifico
        /// </summary>
        /// <param name="file">userid</param>
        /// <returns>Lista de familias</returns>
        public List<family> getListFamilies(string userId)
        {
            using (IDbCommand cmd = database.CreateStoredProcCommand("getListFamilies", conn))
            {
                cmd.Parameters.Add(database.CreateParameter("@userid", userId));

                IDataReader reader = cmd.ExecuteReader();
                List<family> data = new List<family>();
                while (reader.Read())
                {
                    data.Add(populategetListFamiliesFromReader(reader));
                }
                reader.Dispose();
                reader.Close();
                return data;
            }
        }

        /// <summary>
        /// Lee lo retornado en el objeto file
        /// </summary>
        /// <param name="reader"></param>
        /// <returns></returns>
        protected virtual family populategetListFamiliesFromReader(IDataReader reader)
        {
            family sf = new family();

            sf.id = Convert.ToString((validate.getDefaultIfDBNull(reader["objectid"], TypeCode.String)));
            sf.description = Convert.ToString((validate.getDefaultIfDBNull(reader["description"], TypeCode.String)));

            return sf;

        }

        #endregion

    }
}

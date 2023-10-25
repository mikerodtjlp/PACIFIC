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
    public class qc_DAL : DataWorker, IDisposable
    {
        #region database setup

        //private validate validate = validate.getInstance();
        private IDbConnection conn = null;

        public static qc_DAL instance(string name = "main")
        {
            return new qc_DAL(name);
        }
        public qc_DAL(string name): base(name)
        {
            conn = database.CreateOpenConnection();
        }
        void IDisposable.Dispose()
        {
            conn.Close();
            conn.Dispose();
        }
        #endregion

        public List<product> qc_get_all_prods_inspected(lot lt, line ln, qc_block bk)
        {
            var qry = new StringBuilder("execute qc_get_all_prods_inspected ");
            qry.AppendFormat("'{0}','{1}','{2}';", lt.id, ln.id, bk.id);
            using (var cmd = database.CreateCommand(qry.ToString(), conn))
            {

                var r = cmd.ExecuteReader();
                var data = new List<product>();

                while (r.Read())
                {
                    var obj = new product(r.GetString(0));
                    data.Add(obj);
                }

                return data;
            }
        }
        public List<qc_block_detail> get_qc_inspection_detail_allX(batch b, location l)
        {
            var qry = new StringBuilder("execute qc_get_inspection_detail_all ");
            qry.AppendFormat("'{0}','{1}','{2}','{3}';", b.lotno.id, b.line.id, b.part, l.id);
            using (var cmd = database.CreateCommand(qry.ToString(), conn))
            {

                var r = cmd.ExecuteReader();
                var data = new List<qc_block_detail>();

                while (r.Read())
                {
                    var obj = new qc_block_detail();

                    obj.lote = new batch(r["batch"].ToString(), "", r["line_id"].ToString(), "");
                    obj.block = new qc_block(r["block"].ToString());
                    obj.noinsp = int.Parse(r["insp"].ToString());
                    obj.loc = new location(r["location"].ToString());
                    obj.sku = new resource(r["resource"].ToString());
                    obj.def = new defect(r["def"].ToString());
                    obj.def.description = r["long_description"].ToString();
                    obj.zone_ = new zone(r["zone"].ToString());
                    obj.def.category = r["category"].ToString();
                    obj.qty = int.Parse(r["qty"].ToString());
                    obj.def.type_desc = r["type"].ToString();
                    data.Add(obj);
                }

                return data;
            }
        }

        public List<qc_block_detail> get_qc_inspection_detail(qc_block_header b)
        {
            var qry = new StringBuilder("execute qc_get_inspection_detail ");
            qry.AppendFormat("'{0}','{1}','{2}',{3},'{4}';", 
                            b.lot.lotno.id, b.lot.line.id, b.block.id, b.noinsp, b.location.id);
            using (var cmd = database.CreateCommand(qry.ToString(), conn))
            {

                var r = cmd.ExecuteReader();
                var data = new List<qc_block_detail>();

                while (r.Read())
                {
                    var obj = new qc_block_detail();
                    obj.lote = new batch(r["batch"].ToString(), "", r["line_id"].ToString(), "");
                    obj.block = new qc_block(r["block"].ToString());
                    obj.noinsp = int.Parse(r["insp"].ToString());
                    obj.loc = new location(r["location"].ToString());
                    obj.sku = new resource(r["resource"].ToString());
                    obj.def = new defect(r["def"].ToString());
                    obj.def.description = r["long_description"].ToString();
                    obj.zone_ = new zone(r["zone"].ToString());
                    obj.def.category = r["category"].ToString();
                    obj.qty = int.Parse(r["qty"].ToString());
                    obj.def.type_desc = r["type"].ToString();
                    data.Add(obj);
                }

                return data;
            }
        }

        public List<qc_block_header> populate_block_inspection(IDataReader r)
        {
            var data = new List<qc_block_header>();
            for (; r.Read(); )
            {
                var obj = new qc_block_header();
                obj.lot = new batch(r["batch"].ToString(), "", r["line_id"].ToString(), "");
                obj.block = new qc_block(r["block"].ToString());
                obj.noinsp = int.Parse(r["insp"].ToString());
                obj.location = new location(r["location"].ToString());
                obj.status = r["status"].ToString();
                obj.creation_date = (DateTime)r["creation_date"];
                obj.finish_date = (DateTime)validate.getDefaultIfDBNull(r["finish_date"], TypeCode.DateTime);
                obj.total = int.Parse(r["total"].ToString());
                obj.sample = int.Parse(r["sample"].ToString());
                obj.oper = new operador(r["operator"].ToString());
                obj.disposition = r["disposition"].ToString();
                obj.reason_code = new defect(r["reason_code"].ToString());
                obj.res_ctr = r["res_ctr"].ToString();
                obj.res_mln = r["res_mln"].ToString();
                obj.res_mem = r["res_mem"].ToString();
                obj.sta_ctr = r["sta_ctr"].ToString();
                obj.sta_mln = r["sta_mln"].ToString();
                obj.sta_mem = r["sta_mem"].ToString();
                obj.inspected = r["inspected"].ToString();
                obj.comments = r["comments"].ToString();
                obj.aql = new qc_aql_type(r["aql"].ToString());
                obj.part = r["part"].ToString();

                data.Add(obj);
            }
            return data;
        }

        /*public List<qc_block_header> get_block_inspection_all_for_insp(line l, DateTime when)
        {
            var qry = new StringBuilder("select top 6 * from t_qc_block_header with (nolock) ");
            qry.AppendFormat(   " where finish_date < '{0}.5' and line_id='{1}' and disposition != 'WIP' " + 
                                " order by finish_date DESC  ",
                                when.ToString("yyyy/MM/dd HH:mm:ss"), l.id);
            using (var cmd = database.CreateCommand(qry.ToString(), conn))
            {
                return populate_block_inspection(cmd.ExecuteReader());
            }
        }*/

        public List<qc_block_header> get_block_inspection_all_for_aql(lot lt, line l, qc_block b)
        {
            var qry = new StringBuilder();
            qry.AppendFormat("exec qc_batch_get_previous_inspections '{0}','{1}','{2}';",lt.id,l.id,b.id); 
            using (var cmd = database.CreateCommand(qry.ToString(), conn))
            {
                return populate_block_inspection(cmd.ExecuteReader());
            }
        }

        public List<qc_block_header> get_block_inspection_all(batch b)
        {
            var qry = new StringBuilder("select * from t_qc_block_header with (nolock) ");
            qry.AppendFormat(" where batch='{0}' and line_id='{1}' ", b.lotno.id, b.line.id);
            using (var cmd = database.CreateCommand(qry.ToString(), conn))
            {
                return populate_block_inspection(cmd.ExecuteReader());
            }
        }

        public qc_block_header get_block_inspection(qc_block_header b)
        {
            qc_block_header obj = null;
            var qry = new StringBuilder("execute qc_get_block_header ");
            qry.AppendFormat("'{0}','{1}','{2}',{3},'{4}';",
                            b.lot.lotno.id, b.lot.line.id, b.block.id, b.noinsp, b.location.id);

            using (var cmd = database.CreateCommand(qry.ToString(), conn))
            {
                for (var r = cmd.ExecuteReader(); r.Read(); )
                {
                    obj = new qc_block_header();
                    obj.lot = new batch(r["batch"].ToString(), "", r["line_id"].ToString(), "");
                    obj.block = new qc_block(r["block"].ToString());
                    obj.noinsp = int.Parse(r["insp"].ToString());
                    obj.location = new location(r["location"].ToString());
                    obj.status = r["status"].ToString();
                    obj.creation_date = (DateTime)r["creation_date"];
                    obj.finish_date = (DateTime)validate.getDefaultIfDBNull(r["finish_date"], TypeCode.DateTime);
                    obj.total = int.Parse(r["total"].ToString());
                    obj.sample = int.Parse(r["sample"].ToString());
                    obj.oper = new operador(r["operator"].ToString());
                    obj.disposition = r["disposition"].ToString();
                    obj.reason_code = new defect(r["reason_code"].ToString());
                    obj.res_ctr = r["res_ctr"].ToString();
                    obj.res_mln = r["res_mln"].ToString();
                    obj.res_mem = r["res_mem"].ToString();
                    obj.sta_ctr = r["sta_ctr"].ToString();
                    obj.sta_mln = r["sta_mln"].ToString();
                    obj.sta_mem = r["sta_mem"].ToString();
                    obj.inspected = r["inspected"].ToString();
                    obj.comments = r["comments"].ToString();
                    obj.aql = new qc_aql_type(r["aql"].ToString());
                    obj.part = r["part"].ToString();
                    break;
                }
                return obj;
            }
        }
        public qc_block_header get_last_block_inspection(qc_block_header b)
        {
            qc_block_header obj = null;
            var qry = new StringBuilder("execute qc_get_last_block_header ");
            qry.AppendFormat("'{0}','{1}','{2}','{3}';",
                            b.lot.lotno.id, b.lot.line.id, b.block.id, b.location.id);

            using (var cmd = database.CreateCommand(qry.ToString(), conn))
            {
                for (var r = cmd.ExecuteReader(); r.Read(); )
                {
                    obj = new qc_block_header();
                    obj.lot = new batch(r["batch"].ToString(), "", r["line_id"].ToString(), "");
                    obj.block = new qc_block(r["block"].ToString());
                    obj.noinsp = int.Parse(r["insp"].ToString());
                    obj.location = new location(r["location"].ToString());
                    obj.status = r["status"].ToString();
                    obj.creation_date = (DateTime)r["creation_date"];
                    obj.finish_date = (DateTime)validate.getDefaultIfDBNull(r["finish_date"], TypeCode.DateTime);
                    obj.total = int.Parse(r["total"].ToString());
                    obj.sample = int.Parse(r["sample"].ToString());
                    obj.oper = new operador(r["operator"].ToString());
                    obj.disposition = r["disposition"].ToString();
                    obj.reason_code = new defect(r["reason_code"].ToString());
                    obj.res_ctr = r["res_ctr"].ToString();
                    obj.res_mln = r["res_mln"].ToString();
                    obj.res_mem = r["res_mem"].ToString();
                    obj.sta_ctr = r["sta_ctr"].ToString();
                    obj.sta_mln = r["sta_mln"].ToString();
                    obj.sta_mem = r["sta_mem"].ToString();
                    obj.inspected = r["inspected"].ToString();
                    obj.comments = r["comments"].ToString();
                    obj.aql = new qc_aql_type(r["aql"].ToString());
                    obj.part = r["part"].ToString();
                    break;
                }
                return obj;
            }
        }

        public void insert_block_inspection(qc_block_header b)
        {
            var qry = new StringBuilder("execute qc_insert_block_header ");
            qry.AppendFormat("'{0}','{1}','{2}',{3},'{4}', " +
                            "'{5}','{6}', {7}, {8}, '{9}', '{10}', {11}, " +
                            "'{12}','{13}','{14}','{15}', '{16}', '{17}', '{18}', '{19}','{20}','{21}';",
                            b.lot.lotno.id, b.lot.line.id, b.block.id, b.noinsp, b.location.id,
                            b.status, b.creation_date.ToString("yyyy-MM-dd HH:mm:ss"), 
                            b.total, b.sample, b.oper.id, b.disposition, b.reason_code.id, 
                            b.res_ctr, b.res_mln, b.res_mem, b.sta_ctr, b.sta_mln, b.sta_mem, b.inspected, b.comments, b.aql.type,b.part);
            using (var cmd = database.CreateCommand(qry.ToString(), conn))
            {
                cmd.ExecuteNonQuery();
            }
        }

        public void update_block_inspection(qc_block_header b)
        {
            var qry = new StringBuilder("execute qc_update_block_header ");
            qry.AppendFormat("'{0}','{1}','{2}',{3},'{4}', " +
                            "'{5}','{6}','{7}',{8}, {9}, '{10}', '{11}', {12}, " +
                            "'{13}','{14}','{15}','{16}', '{17}', '{18}', '{19}', '{20}','{21}','{22}';",
                            b.lot.lotno.id, b.lot.line.id, b.block.id, b.noinsp, b.location.id,
                            b.status, b.creation_date.ToString("yyyy-MM-dd HH:mm:ss"), 
                            b.finish_date, b.total, b.sample, 
                            b.oper.id, b.disposition, b.reason_code.id,
                            b.res_ctr, b.res_mln, b.res_mem, b.sta_ctr, b.sta_mln, b.sta_mem, b.inspected, b.comments, b.aql.type, b.part);
            using (var cmd = database.CreateCommand(qry.ToString(), conn))
            {
                cmd.ExecuteNonQuery();
            }
        }
        public void delete_block_inspection(qc_block_header b)
        {
            var qry = new StringBuilder("execute qc_delete_block_header ");
            qry.AppendFormat("'{0}','{1}','{2}',{3},'{4}';",
                            b.lot.lotno.id, b.lot.line.id, b.block.id, b.noinsp, b.location.id);
            using (var cmd = database.CreateCommand(qry.ToString(), conn))
            {
                cmd.ExecuteNonQuery();
            }
        }

        public void insert_inspection_defect(qc_block_detail d)
        {
            var qry = new StringBuilder("execute qc_insert_block_detail ");
            qry.AppendFormat("'{0}','{1}','{2}',{3},'{4}','{5}','{6}','{7}',{8};",
                            d.lote.lotno.id, d.lote.line.id, d.block.id, d.noinsp, d.loc.id,
                            d.sku.id, d.zone_.zone_, d.def.id, d.qty);
            using (var cmd = database.CreateCommand(qry.ToString(), conn))
            {
                cmd.ExecuteNonQuery();
            }

/*            using (IDbCommand cmd = database.CreateStoredProcCommand("[qc_block_detail_insert]", conn))
            {
                cmd.Parameters.Add(database.CreateParameter("@batch", d.lote.lotno.id));
                cmd.Parameters.Add(database.CreateParameter("@line", d.lote.line.id));
                cmd.Parameters.Add(database.CreateParameter("@block", d.block.id));
                cmd.Parameters.Add(database.CreateParameter("@location", d.loc.id));
                cmd.Parameters.Add(database.CreateParameter("@sku", d.sku.resource_));
                cmd.Parameters.Add(database.CreateParameter("@defect", d.def.id));
                cmd.Parameters.Add(database.CreateParameter("@zone", d.zone_.zone_));
                cmd.Parameters.Add(database.CreateParameter("@qty", d.qty));

                validate.evaluateParameters(cmd.Parameters, false);

                cmd.ExecuteNonQuery();
            }*/
        }
        public void update_inspection_defect(qc_block_detail d)
        {
            var qry = new StringBuilder("execute qc_update_block_detail ");
            qry.AppendFormat("'{0}','{1}','{2}',{3},'{4}','{5}','{6}','{7}',{8};",
                            d.lote.lotno.id, d.lote.line.id, d.block.id, d.noinsp, d.loc.id,
                            d.sku.id, d.zone_.zone_, d.def.id, d.qty);
//            var qry = new StringBuilder("update t_qc_block_detail ");
//            qry.AppendFormat(   "set qty={0} " +
//                                "where batch='{1}' and line_id='{2}' and block='{3}' and location='{4}' and " +
//	                            "resource='{5}' and zone='{6}' and defect='{7}'", 
//                                d.qty, d.lote.lotno.id, d.lote.line.id, d.block.id, d.loc.id,
//                                d.sku.resource_, d.zone_.zone_, d.def.id);

            using (var cmd = database.CreateCommand(qry.ToString(), conn))
            {
                cmd.ExecuteNonQuery();
            }

/*            using (IDbCommand cmd = database.CreateStoredProcCommand("[qc_block_detail_update]", conn))
            {
                cmd.Parameters.Add(database.CreateParameter("@batch", d.lote.lotno.id));
                cmd.Parameters.Add(database.CreateParameter("@line", d.lote.line.id));
                cmd.Parameters.Add(database.CreateParameter("@block", d.block.id));
                cmd.Parameters.Add(database.CreateParameter("@location", d.loc.id));
                cmd.Parameters.Add(database.CreateParameter("@sku", d.sku.resource_));
                cmd.Parameters.Add(database.CreateParameter("@defect", d.def.id));
                cmd.Parameters.Add(database.CreateParameter("@zone", d.zone_.zone_));
                cmd.Parameters.Add(database.CreateParameter("@qty", d.qty));

                validate.evaluateParameters(cmd.Parameters, false);

                cmd.ExecuteNonQuery();
            }*/
        }
        public void delete_inspection_defect(qc_block_detail d)
        {
            var qry = new StringBuilder("execute qc_delete_block_detail ");
            qry.AppendFormat("'{0}','{1}','{2}',{3},'{4}','{5}','{6}','{7}';",
                            d.lote.lotno.id, d.lote.line.id, d.block.id, d.noinsp, d.loc.id,
                            d.sku.id, d.zone_.zone_, d.def.id);
//            var qry = new StringBuilder("delete t_qc_block_detail ");
//            qry.AppendFormat(   "where batch='{0}' and line_id='{1}' and block='{2}' and location='{3}' and " +
//                                "resource='{4}' and zone='{5}' and defect='{6}'",
//                                d.lote.lotno.id, d.lote.line.id, d.block.id, d.loc.id,
//                                d.sku.resource_, d.zone_.zone_, d.def.id);

            using (var cmd = database.CreateCommand(qry.ToString(), conn))
            {
                cmd.ExecuteNonQuery();
            }

/*            using (IDbCommand cmd = database.CreateStoredProcCommand("[qc_block_detail_delete]", conn))
            {
                cmd.Parameters.Add(database.CreateParameter("@batch", d.lote.lotno.id));
                cmd.Parameters.Add(database.CreateParameter("@line", d.lote.line.id));
                cmd.Parameters.Add(database.CreateParameter("@block", d.block.id));
                cmd.Parameters.Add(database.CreateParameter("@location", d.loc.id));
                cmd.Parameters.Add(database.CreateParameter("@sku", d.sku.resource_));
                cmd.Parameters.Add(database.CreateParameter("@defect", d.def.id));
                cmd.Parameters.Add(database.CreateParameter("@zone", d.zone_.zone_));

                validate.evaluateParameters(cmd.Parameters, false);

                cmd.ExecuteNonQuery();
            }*/
        }

        public qc_block_detail get_inspection_defect(qc_block_detail d)
        {
            qc_block_detail obj = null;

/*            using (IDbCommand cmd = database.CreateStoredProcCommand("[qc_block_detail_get]", conn))
            {
                cmd.Parameters.Add(database.CreateParameter("@batch", d.lote.lotno.id));
                cmd.Parameters.Add(database.CreateParameter("@line", d.lote.line.id));
                cmd.Parameters.Add(database.CreateParameter("@block", d.block.id));
                cmd.Parameters.Add(database.CreateParameter("@location", d.loc.id));
                cmd.Parameters.Add(database.CreateParameter("@sku", d.sku.resource_));
                cmd.Parameters.Add(database.CreateParameter("@defect", d.def.id));
                cmd.Parameters.Add(database.CreateParameter("@zone", d.zone_.zone_));

                validate.evaluateParameters(cmd.Parameters, false);
            */

            var qry = new StringBuilder("execute qc_get_block_detail ");
            qry.AppendFormat("'{0}','{1}','{2}',{3},'{4}','{5}','{6}','{7}';",
                            d.lote.lotno.id, d.lote.line.id, d.block.id, d.noinsp, d.loc.id,
                            d.sku.id, d.zone_.zone_, d.def.id);
//            var qry = new StringBuilder("select * from t_qc_block_detail ");
//            qry.AppendFormat("where batch='{0}' and line_id='{1}' and block='{2}' and location='{3}' and " +
//                            "resource='{4}' and zone='{5}' and defect='{6}'", 
//                            d.lote.lotno.id, d.lote.line.id, d.block.id, d.loc.id,
//                            d.sku.resource_, d.zone_.zone_, d.def.id);

            using (var cmd = database.CreateCommand(qry.ToString(), conn))
            {
                for (var r = cmd.ExecuteReader(); r.Read(); )
                {
                    obj = new qc_block_detail();

                    obj.lote = new batch(r["batch"].ToString(), "", r["line_id"].ToString(), "");
                    obj.block = new qc_block(r["block"].ToString());
                    obj.noinsp = int.Parse(r["insp"].ToString());
                    obj.loc = new location(r["location"].ToString());

                    obj.sku = new resource(r["resource"].ToString());
                    obj.def = new defect(r["defect"].ToString());
                    obj.zone_ = new zone(r["zone"].ToString());
                    obj.qty = int.Parse(r["qty"].ToString());

                    break;
                }

                return obj;
            }
        }

        public void update_inspection_level(line l, qc_inspection_level t)
        {
            var qry = new StringBuilder("update tqchis ");
            qry.AppendFormat(" set status = '{0}' where line='{1}' and pcc='{2}' and location='{3}'; ",
                               t.status, l.id, locs.QCT, t.level);
            using (var cmd = database.CreateCommand(qry.ToString(), conn))
            {
                cmd.ExecuteNonQuery();
            }
        }


        public qc_inspection_level get_permanent_inspection_level(line l, qc_inspection_level t, product p)
        {
            qc_inspection_level obj = null;

            var qry = new StringBuilder("select * from t_qs_perexp_insp with (nolock) ");
            qry.AppendFormat(" where prod_code='{0}' and line_id='{1}' and loc='{2}'; ",
                               p.id, l.id, t.level);
            using (var cmd = database.CreateCommand(qry.ToString(), conn))
            {
                for (var r = cmd.ExecuteReader(); r.Read(); )
                {
                    obj = new qc_inspection_level(r["loc"].ToString(), r["status"].ToString());
                    break;
                }
            }

            return obj;
        }

        public qc_inspection_level get_inspection_level(line l, qc_inspection_level t)
        {
            qc_inspection_level obj = null;

            var qry = new StringBuilder("select * from tqchis with (nolock) ");
            qry.AppendFormat(" where line='{0}' and pcc='{1}' and location='{2}'; ",
                               l.id, locs.QCT, t.level);
            using (var cmd = database.CreateCommand(qry.ToString(), conn))
            {
                for (var r = cmd.ExecuteReader(); r.Read(); )
                {
                    obj = new qc_inspection_level(r["location"].ToString(), r["status"].ToString());
                    break;
                }
            }

            return obj;
        }

        public qc_aql get_aql(qc_aql_type aql, qc_inspection_level lvl, defect_type def, int size)
        {
            qc_aql obj = null;
            var qry = new StringBuilder("execute qc_aql_get ");
            qry.AppendFormat(" '{0}','{1}','{2}',{3};", aql.type, lvl.status, def.id, size);
            using (var cmd = database.CreateCommand(qry.ToString(), conn))
            {
                for (var r = cmd.ExecuteReader(); r.Read(); )
                {
                    obj = new qc_aql();

                    obj.aql_type = new qc_aql_type(r.GetString(0));
                    obj.insp_type = new qc_inspection_type(r.GetString(1));
                    obj.def_type = new defect_type(r.GetString(2));

                    obj.low = r.GetInt32(3);
                    obj.high = r.GetInt32(4);
                    obj.sample = r.GetInt32(5);
                    obj.rejected_with = r.GetInt32(6);
                    obj.accepted_with = r.GetInt32(7);

                    break;
                }

                return obj;
            }
        }

        public defect get_defect(defect d)
        {
            defect obj = null;
/*            using (IDbCommand cmd = database.CreateStoredProcCommand("qc_defect_get", conn))
            {
                cmd.Parameters.Add(database.CreateParameter("@defect", int.Parse(d.id)));*/
            var qry = new StringBuilder("select * from t_qs_defects with (nolock) ");
            qry.AppendFormat("where defect_id = {0}", d.id);
            using (var cmd = database.CreateCommand(qry.ToString(), conn))
            {
                for (var r = cmd.ExecuteReader(); r.Read(); )
                {
                    obj = new defect(r["defect_id"].ToString());
                    obj.description = r["long_description"].ToString();
                    obj.description_s = r["short_description"].ToString();
                    obj.type = int.Parse(r["type"].ToString());
                    obj.category = r["category"].ToString();
                    break;
                }
            }
            return obj;
        }
        public defect get_defect_by_location(location l, defect d)
        {
            defect obj = null;
            var qry = new StringBuilder("select d.* from t_qs_def_grp_dtl h with (nolock) ");
            qry.AppendFormat("right join t_qs_defects d with (nolock) on d.defect_id = h.defect_id " +
                            "where h.group_id='{0}' and d.defect_id={1}", 
                            l.id, d.id);
            using (var cmd = database.CreateCommand(qry.ToString(), conn))
            {
                for (var r = cmd.ExecuteReader(); r.Read(); )
                {
                    obj = new defect(r["defect_id"].ToString());
                    obj.description = r["long_description"].ToString();
                    obj.description_s = r["short_description"].ToString();
                    obj.type = int.Parse(r["type"].ToString());
                    obj.category = r["category"].ToString();
                    break;
                }
            }
            return obj;
        }

        public List<qc_block_sample> get_qc_inspection_sample_all(batch b)
        {
            var qry = new StringBuilder("select * from t_qc_block_sample with (nolock) ");
            qry.AppendFormat(" where batch='{0}' and line_id='{1}' ",
                                b.lotno.id, b.line.id);
            var data = new List<qc_block_sample>();

            using (var cmd = database.CreateCommand(qry.ToString(), conn))
            {
                for (var r = cmd.ExecuteReader(); r.Read(); )
                {
                    data.Add(new qc_block_sample(r["batch"].ToString(),
                                                    r["line_id"].ToString(),
                                                    r["block"].ToString(),
                                                    int.Parse(r["insp"].ToString()),
                                                    r["resource"].ToString(),
                                                    int.Parse(r["qty"].ToString())));
                }
            }
            return data;
        }


        public List<qc_block_sample> get_inspection_sample_all(qc_block_header b)
        {
            var qry = new StringBuilder("select * from t_qc_block_sample with (nolock) ");
            qry.AppendFormat(" where batch='{0}' and line_id='{1}' and block='{2}' and insp={3} ",
                                b.lot.lotno.id, b.lot.line.id, b.block.id, b.noinsp );
            var data = new List<qc_block_sample>();
            using (var cmd = database.CreateCommand(qry.ToString(), conn))
            {
                for (var r = cmd.ExecuteReader(); r.Read(); )
                {
                    data.Add(new qc_block_sample(   r["batch"].ToString(),
                                                    r["line_id"].ToString(),
                                                    r["block"].ToString(),
                                                    int.Parse(r["insp"].ToString()), 
                                                    r["resource"].ToString(),
                                                    int.Parse(r["qty"].ToString())));
                }
            }
            return data;
        }
        /*public List<Tuple<string,string>> get_inspection_prod_disp(qc_block_header b)
        {
            var qry = new StringBuilder("select distinct SUBSTRING(d.resource,1,3) as prod, h.status " +
                                        "from t_qc_block_sample d with (nolock) " +
                                        "inner join t_qc_block_header h with(nolock) on  " +
			                            "h.batch=d.batch and h.line_id=d.line_id and h.block=d.block and h.insp=d.insp ");
            qry.AppendFormat(" where d.batch='{0}' and d.line_id='{1}' and h.part='{2}' and d.block='{3}' and d.insp={4} ",
                                b.lot.lotno.id, b.lot.line.id, b.lot.part, b.block.id, b.noinsp);
            var data = new List<Tuple<string, string>>();
            using (var cmd = database.CreateCommand(qry.ToString(), conn))
            {
                for (var r = cmd.ExecuteReader(); r.Read(); )
                {
                    data.Add(new Tuple<string,string>
                        (r.GetString(0),r.GetString(1)));
                }
            }
            return data;
        }*/

        public qc_block_sample get_inspection_sample(qc_block_sample b)
        {
            var qry = new StringBuilder("execute qc_get_block_sample ");
            qry.AppendFormat("'{0}','{1}','{2}',{3},'{4}','{5}';",
                b.lote.lotno.id, b.lote.line.id, b.block.id, b.noinsp, locs.QCT, b._sku.id);
            qc_block_sample obj = null;
            using (var cmd = database.CreateCommand(qry.ToString(), conn))
            {
                for (var r = cmd.ExecuteReader(); r.Read(); )
                {
                    obj = new qc_block_sample(r["batch"].ToString(),
                                                r["line_id"].ToString(),
                                                r["block"].ToString(),
                                                int.Parse(r["insp"].ToString()), 
                                                r["resource"].ToString(),
                                                int.Parse(r["qty"].ToString()));
                    break;
                }
            }
            return obj;
        }
        public void insert_inspection_sample(qc_block_sample b)
        {
            var qry = new StringBuilder("execute qc_insert_block_sample ");
            qry.AppendFormat("'{0}','{1}','{2}',{3},'{4}','{5}',{6};",
                b.lote.lotno.id, b.lote.line.id, b.block.id, b.noinsp, locs.QCT, b._sku.id, b.qty);
            using (var cmd = database.CreateCommand(qry.ToString(), conn))
            {
                cmd.ExecuteNonQuery();
            }
        }
        public void update_inspection_sample(qc_block_sample b)
        {
            var qry = new StringBuilder("execute qc_update_block_sample ");
            qry.AppendFormat("'{0}','{1}','{2}',{3},'{4}','{5}',{6};",
                b.lote.lotno.id, b.lote.line.id, b.block.id, b.noinsp, locs.QCT, b._sku.id, b.qty);
            using (var cmd = database.CreateCommand(qry.ToString(), conn))
            {
                cmd.ExecuteNonQuery();
            }
        }
        public void delete_inspection_sample(qc_block_sample b)
        {
            var qry = new StringBuilder("execute qc_delete_block_sample ");
            qry.AppendFormat("'{0}','{1}','{2}',{3},'{4}','{5}';",
                b.lote.lotno.id, b.lote.line.id, b.block.id, b.noinsp, locs.QCT, b._sku.id);
            using (var cmd = database.CreateCommand(qry.ToString(), conn))
            {
                cmd.ExecuteNonQuery();
            }
        }

        public List<qc_product_test> get_product_tests(product p)
        {
            var qry = new StringBuilder("select prod, type_insp from t_qc_prod_type_insp with (nolock) ");
            qry.AppendFormat(" where prod='{0}' ", p.id);
            using (var cmd = database.CreateCommand(qry.ToString(), conn))
            {
                var data = new List<qc_product_test>();
                for (var r = cmd.ExecuteReader(); r.Read(); )
                {
                    data.Add(new qc_product_test(r["prod"].ToString(), r["type_insp"].ToString()));
                }

                return data;
            }
        }

        public bool exist_transmitance_capture(batch b)
        {
            var exist = false;
            var qry = new StringBuilder(" if(exists(select lote from t_qc_trans with (nolock)  ");
            qry.AppendFormat(   " where lote='{0}' and prod='{1}' and linea='{2}')) " +
                                " select exist='1' else select exist='0' ",
                                b.lotno.id, b.product.id, b.line.id);
            using (var cmd = database.CreateCommand(qry.ToString(), conn))
            {
                for (var r = cmd.ExecuteReader(); r.Read(); )
                {
                    exist = bool.Parse(r["exist"].ToString());
                }
                return exist;
            }
        }

        public List<qc_test> get_batches_qc_tests(batch b)
        {
            var qry = new StringBuilder("");

            qry.AppendFormat("exec qc_get_batches_qc_tests '{0}','{1}','{2}','{3}'; ",
                              b.lotno.id, b.product.id, b.line.id, b.part);
            using (var cmd = database.CreateCommand(qry.ToString(), conn))
            {
                var data = new List<qc_test>();
                for (var r = cmd.ExecuteReader(); r.Read(); )
                {
                    var obj = new qc_test();
                    obj.id = new location(r["location"].ToString());
                    obj.id.description = r["description"].ToString();
                    obj.status = r["status"].ToString();
                    obj.image = r["*"].ToString(); //temporary
                    data.Add(obj);
                }

                return data;
            }
        }

        public List<batch_header> get_qc_all_pending_products(lot l, line lin)
        {
            var qry = new StringBuilder("select * from batch_header with (nolock) ");
            qry.AppendFormat(" where batch='{0}' and line_id='{1}' and location='QCT' and prod_code != 'QCT' ", l.id, lin.id);
            using (var cmd = database.CreateCommand(qry.ToString(), conn))
            {
                var data = new List<batch_header>();
                for (var r = cmd.ExecuteReader(); r.Read(); )
                {
                    var obj = new batch_header(r["batch"].ToString(),
                                            r["prod_code"].ToString(),
                                            r["line_id"].ToString(),
                                            r["part"].ToString(),
                                            r["location"].ToString(),
                                            int.Parse(r["cycle"].ToString()));
//                    obj.location = new location(reader["location"].ToString());
//                    obj.cycle = int.Parse(reader["cycle"].ToString());
                    obj.status = r["status"].ToString().TrimEnd();
                    obj.finishontime = r["new_pkg_count"].ToString();
                    obj.as_400 = r["as_400"].ToString();
                    obj.comments = r["comment"].ToString();
                    obj.date_time = (DateTime)r["date_time"];
                    obj.creation_type = int.Parse(r["reason_code_1"].ToString());
                    obj.boxes = int.Parse(r["reason_code_3"].ToString());
                    data.Add(obj);
                }
                return data;
            }
        }

        /*        public qc_test_full get_qc_all_pending_products(product p, location l)
                {
                    var qry = new StringBuilder("select *, t_qc_insp_types.description, t_qc_insp_types.depto_id from t_scheme_route with (nolock) ");
                    qry.AppendFormat(   "join t_qc_insp_types  with (nolock) on (t_scheme_route.location = t_qc_insp_types.object_id) " +
                                        "join vw_prodxfam  with (nolock) on (vw_prodxfam.object_id=t_scheme_route.familia and vw_prodxfam.group_ =t_scheme_route.grupo) " +
                                        "where location='{0}' and vw_prodxfam.prod_code='{1}' ", l.id, p.id);
                    qc_test_full obj = null;
                    using (var cmd = database.CreateCommand(qry.ToString(), conn))
                    {
                        for (var r = cmd.ExecuteReader(); r.Read(); )
                        {
                            obj = new qc_test_full();

                            obj.id = r["id"].ToString();
                            obj.familia = r["familia"].ToString();
                            obj.grupo = r["grupo"].ToString();
                            obj.sequence = r["sequence"].ToString();
                            obj.location = r["location"].ToString();
                            obj.funs = r["funs"].ToString();
                            obj.RFB = r["RFB"].ToString();
                            obj.gencap = r["gencap"].ToString();
                            obj.object_id = r["object_id"].ToString();
                            obj.description = r["description"].ToString();
                            obj.process_id = r["process_id"].ToString();
                            obj.depto_id = r["depto_id"].ToString();

                            obj.this_id = r["this_id"].ToString();
                            obj.prod_code = r["prod_code"].ToString();
                            obj.object_id1  = r["object_id1"].ToString();
                            obj.group_  = r["group_"].ToString();
                            obj.description1 = r["description1"].ToString();
                            obj.depto_id1  = r["depto_id1"].ToString();

                            break;
                        }
                    }
                    return obj;
                }*/

        public int trolley_batch_exist_tests(batch b, qc_block t)
        {
            using (IDbCommand cmd = database.CreateStoredProcCommand("trolley_batch_exist_tests", conn))
            {
                cmd.Parameters.Add(database.CreateParameter("@batch", b.lotno.id));
                cmd.Parameters.Add(database.CreateParameter("@line", b.line.id));
                cmd.Parameters.Add(database.CreateParameter("@trolley", t.id));
                int resultado = 0;
                using (var r = cmd.ExecuteReader())
                {
                    while (r.Read())
                    {
                        resultado = int.Parse(r["valid"].ToString());
                    }
                }
                return resultado;
            }
        }

        /**
         * Se encarga llamar al procedure getqcvalidation el cual retorna si se 
         * puede crear o no la inspeccion
         * <param name="d">objeto que contiene lote, linea y producto</param>
         * <param name="t">objeto que contiene el trolley</param>
         * <returns>Retorna si se puede crear o no la inspeccion</returns>
         */ 
        public int getValidInspec(batch b, qc_block t)
        {
            using (IDbCommand cmd = database.CreateStoredProcCommand("getqcvalidation", conn))
            {
                cmd.Parameters.Add(database.CreateParameter("@batch", b.lotno.id));
                cmd.Parameters.Add(database.CreateParameter("@line", b.line.id));
                cmd.Parameters.Add(database.CreateParameter("@trolley", t.id));
                int resultado = 0; 
                using (var r = cmd.ExecuteReader())
                {
                    while (r.Read())
                    {
                        resultado = int.Parse(r["valid"].ToString());
                    }
                }
                return resultado;
            }
        }
        public void create_liberar_gen_capture_all(batch b)
        {
            var qry = new StringBuilder("execute qc_create_all_qc_tests ");
            qry.AppendFormat("'{0}','{1}','{2}';",b.lotno.id, b.line.id, b.part);
            using (var cmd = database.CreateCommand(qry.ToString(), conn))
            {
                cmd.ExecuteNonQuery();
            }
        }
        public batch qc_all_prods_have_tests(batch b)
        {
            batch bat = null;
            using (IDbCommand cmd = database.CreateStoredProcCommand("qc_all_prods_have_tests", conn))
            {
                cmd.Parameters.Add(database.CreateParameter("@batch", b.lotno.id));
                cmd.Parameters.Add(database.CreateParameter("@line", b.line.id));
                string p = string.Empty; 
                using (var r = cmd.ExecuteReader())
                {
                    while (r.Read())
                    {
                        bat = new BO.batch( r.GetString(0), r.GetString(1),
                                            r.GetString(2), r.GetString(3));
                        break;
                    }
                }
            }
            return bat;
        }

        public qc_batch_header get_qc_batch_last_inspection(batch b)
        {
            var qry = new StringBuilder("select * from tqchdr2 with (nolock) ");
            qry.AppendFormat(" where batch='{0}' and prod='{1}' and line='{2}' and part='{3}' order by noinsp DESC",
                            b.lotno.id, b.product.id, b.line.id, b.part);
            using (var cmd = database.CreateCommand(qry.ToString(), conn))
            {
                for (var r = cmd.ExecuteReader(); r.Read(); )
                {
                    return new qc_batch_header(r[0].ToString(),r[1].ToString(),r[2].ToString(),r[3].ToString(),
                        int.Parse(r[4].ToString()),(DateTime)r[5],r[6].ToString(),r[7].ToString(),
                        int.Parse(r[8].ToString()),int.Parse(r[9].ToString()),int.Parse(r[10].ToString()),
                        r[11].ToString(),r[12].ToString(),int.Parse(r[13].ToString()),
                        r[14].ToString(),r[15].ToString(),r[16].ToString(),r[17].ToString(),
                        r[18].ToString(),r[19].ToString(),r[20].ToString(),
                        r[21].ToString(),r[22].ToString(),r[23].ToString());
                }
            }
            return null;
        }
        public qc_batch_header get_qc_batch_first_inspection(batch b)
        {
            var qry = new StringBuilder("select * from tqchdr2 with (nolock) ");
            qry.AppendFormat(" where batch='{0}' and prod='{1}' and line='{2}' and part='{3}' order by noinsp ASC",
                            b.lotno.id, b.product.id, b.line.id, b.part);
            using (var cmd = database.CreateCommand(qry.ToString(), conn))
            {
                for (var r = cmd.ExecuteReader(); r.Read(); )
                {
                    return new qc_batch_header(r[0].ToString(), r[1].ToString(), r[2].ToString(), r[3].ToString(),
                        int.Parse(r[4].ToString()), (DateTime)r[5], r[6].ToString(), r[7].ToString(),
                        int.Parse(r[8].ToString()), int.Parse(r[9].ToString()), int.Parse(r[10].ToString()),
                        r[11].ToString(), r[12].ToString(), int.Parse(r[13].ToString()),
                        r[14].ToString(), r[15].ToString(), r[16].ToString(), r[17].ToString(),
                        r[18].ToString(), r[19].ToString(), r[20].ToString(),
                        r[21].ToString(), r[22].ToString(), r[23].ToString());
                }
            }
            return null;
        }
    }
}

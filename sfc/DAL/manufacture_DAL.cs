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
	public class manufacture_DAL : DataWorker, IDisposable
	{
		#region database setup

		//private validate validate = validate.getInstance();
		private IDbConnection conn = null;

		public static manufacture_DAL instance(string name = "main")
		{
			return new manufacture_DAL(name);
		}
		public manufacture_DAL(string name) : base(name) { conn = database.CreateOpenConnection(); }
		void IDisposable.Dispose()
		{
			conn.Close();
			conn.Dispose();
		}
		#endregion

		public resource get_resource_by_barcode(resource res_)
		{
			using (var cmd = database.CreateStoredProcCommand("get_resource_by_barcode", conn))
			{
				cmd.Parameters.Add(database.CreateParameter("@barcode", res_.opc_bar_code));

				var reader = cmd.ExecuteReader();
				var obj = new resource();

				while (reader.Read())
				{
					obj.opc_bar_code = reader["opc_bar_code"].ToString();
					obj.prod_code = reader["prod_code"].ToString();
					obj.base_ = reader["base"].ToString();
					obj.addition = reader["addition"].ToString();
					obj.eye = reader["eye"].ToString();
					obj.id = reader["resource"].ToString();
					obj.description = reader["description"].ToString();
					obj.cost_code = reader["cost_code"].ToString();
					obj.std_code = reader["std_code"].ToString();
				}
				return obj;
			}
		}

		public void insert_heijunka_data(constraint c, basetype b, diammeter di, diammeter df, basenum s, basenum f, string w)
		{
			var qry = new StringBuilder("insert t_routes ");
			qry.AppendFormat("values('{0}','{1}','{2}','{3}','{4}', '{5}', {6}) ",
							 c.id, b.id, di.id, df.id, s.graduation, f.graduation, w);
			using (var cmd = database.CreateCommand(qry.ToString(), conn))
			{
				cmd.ExecuteNonQuery();
			}
		}

		public void update_heijunka_data(constraint c, basetype b, diammeter di, diammeter df, basenum s, basenum f, string w)
		{
			var qry = new StringBuilder("update t_routes ");
			qry.AppendFormat("set weight={0} " +
							 "where constraintid='{1}' and typeid='{2}' and diamm_begin='{3}' and diamm_end='{4}' and base_begin='{5}' and base_end='{6}' ",
							 w, c.id, b.id, di.id, df.id, s.graduation, f.graduation);
			using (var cmd = database.CreateCommand(qry.ToString(), conn))
			{
				cmd.ExecuteNonQuery();
			}
		}

		public void delete_heijunka_data(constraint c, basetype b, diammeter di, diammeter df, basenum s, basenum f)
		{
			var qry = new StringBuilder("delete t_routes ");
			qry.AppendFormat("where constraintid='{0}' and typeid='{1}' and diamm_begin='{2}' and diamm_end='{3}' and base_begin='{4}' and base_end='{5}' ",
							 c.id, b.id, di.id, df.id, s.graduation, f.graduation);
			using (var cmd = database.CreateCommand(qry.ToString(), conn))
			{
				cmd.ExecuteNonQuery();
			}
		}
        #region trolley
        public List<product_bulk> trolley_batch_get_full(lot l, line li, string part, trolley t, int insp)
        {
            using (IDbCommand cmd = database.CreateStoredProcCommand("trolley_batch_get_full", conn))
            {
                var p = cmd.Parameters;
                p.Add(database.CreateParameter("@batch", l.id));
                p.Add(database.CreateParameter("@line", li.id));
                p.Add(database.CreateParameter("@part", part));
                p.Add(database.CreateParameter("@trolley", t.id));
                p.Add(database.CreateParameter("@insp", insp));

                var data = new List<product_bulk>();
                for (var r = cmd.ExecuteReader(); r.Read(); )
                {
                    data.Add(new product_bulk(r["prod"].ToString(),
                                                r["base"].ToString(),
                                                int.Parse(r["qty"].ToString())));
                }

                return data;
            }
        }
        public List<product_base_eye> trolley_batch_get_all(lot l, line li, string pt, trolley t, int insp)
        {
            using (IDbCommand cmd = database.CreateStoredProcCommand("trolley_batch_get_all", conn))
            {
                var p = cmd.Parameters;
                p.Add(database.CreateParameter("@batch", l.id));
                p.Add(database.CreateParameter("@line", li.id));
                p.Add(database.CreateParameter("@part", pt));
                p.Add(database.CreateParameter("@trolley", t.id));
                p.Add(database.CreateParameter("@insp", insp));

                var data = new List<product_base_eye>();
                for (var r = cmd.ExecuteReader(); r.Read(); )
                {
                    data.Add(new product_base_eye(r["prod"].ToString(),
                                                r["base"].ToString(),
                                                r["eye"].ToString(),
                                                int.Parse(r["qty"].ToString())));
                }

                return data;
            }
        }

        public product_base_eye trolley_batch_get(lot l, line li, string pt, trolley t, int insp, product_bulk o, string eye)
        {
            using (IDbCommand cmd = database.CreateStoredProcCommand("trolley_batch_get", conn))
            {
                var p = cmd.Parameters;
                p.Add(database.CreateParameter("@batch", l.id));
                p.Add(database.CreateParameter("@line", li.id));
                p.Add(database.CreateParameter("@part", pt));
                p.Add(database.CreateParameter("@trolley", t.id));
                p.Add(database.CreateParameter("@insp", insp));
                p.Add(database.CreateParameter("@prod", o.prod.id));
                p.Add(database.CreateParameter("@base", o.baseno.graduation));
                p.Add(database.CreateParameter("@eye", eye));

                product_base_eye obj = null;

                for (var r = cmd.ExecuteReader(); r.Read(); )
                {
                    obj = new product_base_eye(r["prod"].ToString(),
                                            r["base"].ToString(),
                                            r["eye"].ToString(),
                                            int.Parse(r["qty"].ToString()));
                    break;
                }

                return obj;
            }
        }

        public bool trolley_check_prod_group(lot l, line li, string pt, trolley t, int insp, product_bulk o)
        {
            using (IDbCommand cmd = database.CreateStoredProcCommand("trolley_check_prod_group", conn))
            {
                var p = cmd.Parameters;
                p.Add(database.CreateParameter("@batch", l.id));
                p.Add(database.CreateParameter("@line", li.id));
                p.Add(database.CreateParameter("@part", pt));
                p.Add(database.CreateParameter("@trolley", t.id));
                p.Add(database.CreateParameter("@insp", insp));
                p.Add(database.CreateParameter("@prod", o.prod.id));
                return ((int)Convert.ToInt32(cmd.ExecuteScalar())) == 1;
            }
        }

        public void trolley_batch_insert(lot l, line li, string pt, trolley t, int insp, product_bulk o,string eye)
        {
            using (IDbCommand cmd = database.CreateStoredProcCommand("trolley_batch_insert", conn))
            {
                var p = cmd.Parameters;
                p.Add(database.CreateParameter("@batch", l.id));
                p.Add(database.CreateParameter("@line", li.id));
                p.Add(database.CreateParameter("@part", pt));
                p.Add(database.CreateParameter("@trolley", t.id));
                p.Add(database.CreateParameter("@insp", insp));
                p.Add(database.CreateParameter("@prod", o.prod.id));
                p.Add(database.CreateParameter("@base", o.baseno.graduation));
                p.Add(database.CreateParameter("@eye", eye));
                p.Add(database.CreateParameter("@qty", o.qty));

                validate.evaluateParameters(p, false);

                cmd.ExecuteNonQuery();
            }
        }

        public void trolley_batch_update(lot l, line li, string pt, trolley t, int insp, product_bulk o, string eye)
        {
            using (IDbCommand cmd = database.CreateStoredProcCommand("trolley_batch_update", conn))
            {
                var p = cmd.Parameters;
                p.Add(database.CreateParameter("@batch", l.id));
                p.Add(database.CreateParameter("@line", li.id));
                p.Add(database.CreateParameter("@part", pt));
                p.Add(database.CreateParameter("@trolley", t.id));
                p.Add(database.CreateParameter("@insp", insp));
                p.Add(database.CreateParameter("@prod", o.prod.id));
                p.Add(database.CreateParameter("@base", o.baseno.graduation));
                p.Add(database.CreateParameter("@eye", eye));
                p.Add(database.CreateParameter("@qty", o.qty));
                validate.evaluateParameters(p, false);
                cmd.ExecuteNonQuery();
            }
        }

        public void trolley_batch_delete(lot l, line li, string pt, trolley t, int insp, product_bulk o,string eye)
        {
            using (IDbCommand cmd = database.CreateStoredProcCommand("trolley_batch_delete", conn))
            {
                var p = cmd.Parameters;
                p.Add(database.CreateParameter("@batch", l.id));
                p.Add(database.CreateParameter("@line", li.id));
                p.Add(database.CreateParameter("@part", pt));
                p.Add(database.CreateParameter("@trolley", t.id));
                p.Add(database.CreateParameter("@insp", insp));
                p.Add(database.CreateParameter("@prod", o.prod.id));
                p.Add(database.CreateParameter("@base", o.baseno.graduation));
                p.Add(database.CreateParameter("@eye", eye));
                validate.evaluateParameters(p, false);
                cmd.ExecuteNonQuery();
            }
        }
        #endregion

        #region qc block
        public int trolley_get_last(lot lt, line l)
        {
            using (IDbCommand cmd = database.CreateStoredProcCommand("trolley_get_last", conn))
            {
                var p = cmd.Parameters;
                p.Add(database.CreateParameter("@batch", lt.id));
                p.Add(database.CreateParameter("@line", l.id));
                return (int)Convert.ToInt32(cmd.ExecuteScalar());
            }
        }

		public List<product_bulk> trolley_batch_relation_get_full(lot l, line li, string pt, trolley t, int insp)
		{
			using (IDbCommand cmd = database.CreateStoredProcCommand("trolley_batch_relation_get_full", conn))
			{
				var p = cmd.Parameters;
				p.Add(database.CreateParameter("@batch", l.id));
				p.Add(database.CreateParameter("@line", li.id));
                p.Add(database.CreateParameter("@part", pt));
                p.Add(database.CreateParameter("@trolley", t.id));
				p.Add(database.CreateParameter("@insp", insp));

				var data = new List<product_bulk>();
				for (var r = cmd.ExecuteReader(); r.Read(); )
				{
					data.Add(new product_bulk(r["prod"].ToString(),
												r["base"].ToString(),
												int.Parse(r["qty"].ToString())));
				}

				return data;
			}
		}
		public List<product_bulk> trolley_batch_relation_get_all(lot l, line li, string pt, trolley t, int insp)
		{
			using (IDbCommand cmd = database.CreateStoredProcCommand("trolley_batch_relation_get_all", conn))
			{
				var p = cmd.Parameters;
				p.Add(database.CreateParameter("@batch", l.id));
				p.Add(database.CreateParameter("@line", li.id));
				p.Add(database.CreateParameter("@part", pt));
				p.Add(database.CreateParameter("@trolley", t.id));
				p.Add(database.CreateParameter("@insp", insp));

				var data = new List<product_bulk>();
				for (var r = cmd.ExecuteReader(); r.Read(); )
				{
					data.Add(new product_bulk(r["prod"].ToString(),
												r["base"].ToString(),
												int.Parse(r["qty"].ToString())));
				}

				return data;
			}
		}

		public product_bulk trolley_batch_relation_get(lot l, line li, string pt, trolley t, int insp, product_bulk o)
		{
			using (IDbCommand cmd = database.CreateStoredProcCommand("trolley_batch_relation_get", conn))
			{
				var p = cmd.Parameters;
				p.Add(database.CreateParameter("@batch", l.id));
				p.Add(database.CreateParameter("@line", li.id));
				p.Add(database.CreateParameter("@part", pt));
				p.Add(database.CreateParameter("@trolley", t.id));
				p.Add(database.CreateParameter("@insp", insp));
				p.Add(database.CreateParameter("@prod", o.prod.id));
				p.Add(database.CreateParameter("@base", o.baseno.graduation));

				product_bulk obj = null;

				for (var r = cmd.ExecuteReader(); r.Read(); )
				{
					obj = new product_bulk(r["prod"].ToString(),
											r["base"].ToString(),
											int.Parse(r["qty"].ToString()));
					break;
				}

				return obj;
			}
		}

		public void trolley_batch_relation_insert(lot l, line li, string pt, trolley t, int insp, product_bulk o)
		{
			using (IDbCommand cmd = database.CreateStoredProcCommand("trolley_batch_relation_insert", conn))
			{
				var p = cmd.Parameters;
				p.Add(database.CreateParameter("@batch", l.id));
				p.Add(database.CreateParameter("@line", li.id));
				p.Add(database.CreateParameter("@part", pt)); 
				p.Add(database.CreateParameter("@trolley", t.id));
				p.Add(database.CreateParameter("@insp", insp));
				p.Add(database.CreateParameter("@prod", o.prod.id));
				p.Add(database.CreateParameter("@base", o.baseno.graduation));
				p.Add(database.CreateParameter("@qty", o.qty));

				validate.evaluateParameters(p, false);

				cmd.ExecuteNonQuery();
			}
		}

		public void trolley_batch_relation_update(lot l, line li, string pt, trolley t, int insp, product_bulk o)
		{
			using (IDbCommand cmd = database.CreateStoredProcCommand("trolley_batch_relation_update", conn))
			{
				var p = cmd.Parameters;
				p.Add(database.CreateParameter("@batch", l.id));
				p.Add(database.CreateParameter("@line", li.id));
				p.Add(database.CreateParameter("@part", pt)); 
				p.Add(database.CreateParameter("@trolley", t.id));
				p.Add(database.CreateParameter("@insp", insp));
				p.Add(database.CreateParameter("@prod", o.prod.id));
				p.Add(database.CreateParameter("@base", o.baseno.graduation));
				p.Add(database.CreateParameter("@qty", o.qty));
				validate.evaluateParameters(p, false);
				cmd.ExecuteNonQuery();
			}
		}

		public void trolley_batch_relation_delete(lot l, line li, string pt, trolley t, int insp, product_bulk o)
		{
			using (IDbCommand cmd = database.CreateStoredProcCommand("trolley_batch_relation_delete", conn))
			{
				var p = cmd.Parameters;
				p.Add(database.CreateParameter("@batch", l.id));
				p.Add(database.CreateParameter("@line", li.id));
				p.Add(database.CreateParameter("@part", pt)); 
				p.Add(database.CreateParameter("@trolley", t.id));
				p.Add(database.CreateParameter("@insp", insp));
				p.Add(database.CreateParameter("@prod", o.prod.id));
				p.Add(database.CreateParameter("@base", o.baseno.graduation));
				validate.evaluateParameters(p, false);
				cmd.ExecuteNonQuery();
			}
		}
		public void trolley_batch_relation_delete_block(lot l, line li, string pt, trolley t, int insp)
		{
			using (IDbCommand cmd = database.CreateStoredProcCommand("trolley_batch_relation_delete_block", conn))
			{
				var p = cmd.Parameters;
				p.Add(database.CreateParameter("@batch", l.id));
				p.Add(database.CreateParameter("@line", li.id));
				p.Add(database.CreateParameter("@part", pt)); 
				p.Add(database.CreateParameter("@trolley", t.id));
				p.Add(database.CreateParameter("@insp", insp));
				validate.evaluateParameters(p, false);
				cmd.ExecuteNonQuery();
			}
		}
        #endregion

        public List<product_bulk> get_batch_product_base(lot l, line li, string part, location loc)
        {
            var qry = new StringBuilder("select prod_code,substring(resource,4,4),sum(qty) ");
            qry.AppendFormat("from batch_detail with(nolock) " +
                            "where batch='{0}' and line_id ='{1}' and  part='{2}' and location='{3}' " +
                            "group by prod_code,substring(resource,4,4) ",
                            l.id, li.id,part,loc.id);
            using (var cmd = database.CreateCommand(qry.ToString(), conn))
            {
                var data = new List<product_bulk>();
                for (var r = cmd.ExecuteReader(); r.Read(); )
                {
                    data.Add(new product_bulk(r.GetString(0),
                                              r.GetString(1),
                                              r.GetInt32(2)));
                }
                return data;
            }
        }

		public batch populate_batch(IDataReader r)
		{
			batch lote = null;

			//while (r.Read()) URI02
			if (r.Read()) //URI02
			{
				lote = new batch(r.GetString(0), r.GetString(3), r.GetString(2), r.GetString(1));

				lote.date = r.GetDateTime(4);
				lote.date_coat = (DateTime)validate.getDefaultIfDBNull(r[5], TypeCode.DateTime);
				lote.date_qc = (DateTime)validate.getDefaultIfDBNull(r[6], TypeCode.DateTime);
				lote.date_pack = (DateTime)validate.getDefaultIfDBNull(r[7], TypeCode.DateTime);

				lote.status = r.GetInt16(8);
				lote.comentario = r.GetString(9);
				lote.variacion = r.GetInt16(10);
			}
			return lote;
		}

		public List<batch> get_all_product_batches(lot l, line li)
		{
			var data = new List<batch>();
			var qry = new StringBuilder("select * from thdr with (nolock) ");
			qry.AppendFormat("where hdrid='{0}' and line='{1}' ", l.id, li.id);

			batch lote = null;
			using (var cmd = database.CreateCommand(qry.ToString(), conn))
			{
				for (var r = cmd.ExecuteReader(); r.Read(); ) //URI02
				{
					//data.Add(populate_batch(r)); URI02
					// URI02
					lote = null;
					lote = new batch(r.GetString(0), r.GetString(3), r.GetString(2), r.GetString(1));

						lote.date = r.GetDateTime(4);
						lote.date_coat = (DateTime)validate.getDefaultIfDBNull(r[5], TypeCode.DateTime);
						lote.date_qc = (DateTime)validate.getDefaultIfDBNull(r[6], TypeCode.DateTime);
						lote.date_pack = (DateTime)validate.getDefaultIfDBNull(r[7], TypeCode.DateTime);

						lote.status = r.GetInt16(8);
						lote.comentario = r.GetString(9);
						lote.variacion = r.GetInt16(10);
					data.Add(lote);
					//
				}
				
			}
			return data;
		}

		public void create_casting_batch(batch b)
		{
			using (IDbCommand cmd = database.CreateStoredProcCommand("insert_header_osm3", conn))
			{
				var p = cmd.Parameters;
				p.Add(database.CreateParameter("@batch_sn", b.lotno.id));
				p.Add(database.CreateParameter("@prod_code", b.product.id));
				p.Add(database.CreateParameter("@line_id", b.line.id));
				p.Add(database.CreateParameter("@part", b.part));
				p.Add(database.CreateParameter("@err", 0));
				validate.evaluateParameters(p, false);
				cmd.ExecuteNonQuery();
			}
		}

		public batch get_batch(batch b)
		{
			batch lote = null;
			var qry = new StringBuilder("select * from thdr with (nolock) ");
			qry.AppendFormat("where hdrid='{0}' and pcc='{1}' and line='{2}' and part='{3}'",
							b.lotno.id, b.product.id, b.line.id, b.part);
			using (var cmd = database.CreateCommand(qry.ToString(), conn))
			{
				lote = populate_batch(cmd.ExecuteReader());
			}
			return lote;
		}

		public void insert_batch(batch b)
		{
		}

		public void update_batch(batch b)
		{
			var qry = new StringBuilder("update thdr ");
			qry.AppendFormat("set date='{0}', status={1}, comentario='{2}', variacion={3} " +
							 "where hdrid='{4}' and pcc='{5}' and line='{6}' and part='{7}' ",
							 b.date, b.status, b.comentario, b.variacion,
							 b.lotno.id, b.product.id, b.line.id, b.part);
			using (var cmd = database.CreateCommand(qry.ToString(), conn))
			{
				cmd.ExecuteNonQuery();
			}
		}
		public void delete_batch(batch b)
		{
			var qry = new StringBuilder("delete thdr ");
			qry.AppendFormat("where hdrid='{0}' and pcc='{1}' and line='{2}' and part='{3}' ",
							 b.lotno.id, b.product.id, b.line.id, b.part);
			using (var cmd = database.CreateCommand(qry.ToString(), conn))
			{
				cmd.ExecuteNonQuery();
			}
		}

		public production_base_detail populate_production_base_detail(IDataReader reader)
		{
			production_base_detail pdetail = null;

			/*var l = new lot(reader["dtlid"].ToString());
			var p = new product(reader["pcc"].ToString());
			var ln = new line(reader["line"].ToString());
			pdetail = new production_base_detail(l, p, ln, reader["part"].ToString());

			pdetail.baseno = new basenum(reader["base"].ToString());
			pdetail.dep = new depto(int.Parse(reader["defectdep"].ToString()));
			pdetail.defect = int.Parse(reader["defect"].ToString());
			pdetail.qty = int.Parse(reader["total"].ToString());
			*/
			return pdetail;
		}

		public List<production_base_detail> get_production_base_detail_all(batch b, depto dep)
		{
			//var qry = new StringBuilder("select * from tdtl with (nolock) ");
			//qry.AppendFormat("where dtlid='{0}' and pcc='{1}' and line='{2}' and part='{3}' and defectdep={4}",
			//                b.lotno.id, b.product.id, b.line.id, b.part, dep.id);

			var data = new List<production_base_detail>();
			/*using (var cmd = database.CreateCommand(qry.ToString(), conn))
			{
				for (var r = cmd.ExecuteReader(); r.Read(); )
				{
					data.Add(populate_production_base_detail(r));
				}
			}*/
			return data;
		}

		public production_base_detail get_specific_production_base(batch b, basenum bas)
		{
			production_base_detail pdetail = null;
			/*var qry = new StringBuilder("select * from tdtl with (nolock) ");
			qry.AppendFormat("where dtlid='{0}' and pcc='{1}' and line='{2}' and part='{3}' and base='{4}'",
							b.lotno.id, b.product.id, b.line.id, b.part, bas.graduation);
			using (var cmd = database.CreateCommand(qry.ToString(), conn))
			{
				for (var r = cmd.ExecuteReader(); r.Read(); )
				{
					pdetail = populate_production_base_detail(r);
					break;
				}
			}*/
			return pdetail;
		}

		public void delete_production_base_all(batch b, depto d)
		{
			/*var qry = new StringBuilder("delete tdtl with (rowlock) ");
			qry.AppendFormat(" where dtlid='{0}' and pcc='{1}' and line='{2}' and part='{3}' and defectdep={4} ",
								b.lotno.id, b.product.id, b.line.id, b.part, d.id);
			using (var cmd = database.CreateCommand(qry.ToString(), conn))
			{
				cmd.ExecuteNonQuery();
			}*/
		}

		public void insert_production_base(batch b, depto d, basenum bas, defect def, int qty, operador oper)
		{
			/*var qry = new StringBuilder("");
			qry.AppendFormat(" execute dcs_ins_tdtl '{0}', '{1}', '{2}', '{3}', '{4}', {5}, {6}, {7}, '{8}'; ",
								b.lotno.id, b.product.id, b.line.id, b.part, bas.graduation, d.id, def.id, qty, oper.id);
			using (var cmd = database.CreateCommand(qry.ToString(), conn))
			{
				cmd.ExecuteNonQuery();
			}*/
		}

		public List<basenum> get_bases(product p)
		{
			var data = new List<basenum>();
			/*            using (IDbCommand cmd = database.CreateStoredProcCommand("get_available_bases_from_product", conn))
						{
							cmd.Parameters.Add(database.CreateParameter("@prod", p.id));*/
			var qry = new StringBuilder("select distinct base from tresource with (nolock) ");
			qry.AppendFormat("where prod_code='{0}'", p.id);
			using (var cmd = database.CreateCommand(qry.ToString(), conn))
			{
				for (var r = cmd.ExecuteReader(); r.Read(); )
				{
					data.Add(new basenum(r.GetString(0)));
				}
			}
			return data;
		}
		/*public List<batch> get_batches_like(batch b)
		{
			var data = new List<batch>();
			var status = "";
			if (b.status != -1) status = b.status.ToString();


			var qry = new StringBuilder("declare @statusini varchar(2) ");
			qry.AppendFormat("declare @statusfin varchar(2) " +
							"set @statusini = '{0}' " +
							"set @statusfin = '{1}' " +

							"if('{2}' = '' and '{3}' = '' and '{4}' ='' and '{5}' = '') return " +
							"if(@statusini = '') set @statusini = '0' " +
							"if(@statusfin = '') set @statusfin = '99' " +

							"select hdrid, pcc, line, part, status, comentario, Date as fecha, variacion, " +
								"cast(0 as smallint) as _img   " +
							"from thdr with (nolock)  " +
							"where hdrid like '{6}'+'%' and pcc like '{7}'+'%' and line like '{8}'+'%' and part like '{9}'+'%' " +
								"and status between @statusini and @statusfin " +
							"order by hdrid, pcc, line, part ",
							status, status,
							b.lotno.id, b.product.id, b.line.id, b.part,
							b.lotno.id, b.product.id, b.line.id, b.part);
			using (var cmd = database.CreateCommand(qry.ToString(), conn))
			{
				//           using (IDbCommand cmd = database.CreateStoredProcCommand("get_batches_like", conn))
				//			{
				//				var status = "";
				//				if (b.status != -1) status = b.status.ToString();
                //
				//				cmd.Parameters.Add(database.CreateParameter("@batch", b.lotno.id));
				//				cmd.Parameters.Add(database.CreateParameter("@prod", b.product.id));
				//				cmd.Parameters.Add(database.CreateParameter("@line", b.line.id));
				//				cmd.Parameters.Add(database.CreateParameter("@part", b.part));
				//				cmd.Parameters.Add(database.CreateParameter("@status", status));
                //
				for (var r = cmd.ExecuteReader(); r.Read(); )
				{
					var bt = new batch(r["hdrid"].ToString(),
										r["pcc"].ToString(),
										r["line"].ToString(),
										r["part"].ToString());

					bt.date = (DateTime)r["fecha"];
					bt.status = int.Parse(r["status"].ToString());
					bt.comentario = r["comentario"].ToString();
					bt.variacion = int.Parse(r["variacion"].ToString());

					data.Add(bt);
				}
			}
			return data;
		}*/

		public batch_lot get_batch_lot(batch b, location l)
		{
			var qry = new StringBuilder("select * from batch_lot with (nolock) ");
			qry.AppendFormat("where batch='{0}' and prod_code='{1}' and line_id='{2}' and part='{3}' and location='{4}' ",
								b.lotno.id, b.product.id, b.line.id, b.part, l.id);
			batch_lot obj = null;
			using (var cmd = database.CreateCommand(qry.ToString(), conn))
			{
				for (var r = cmd.ExecuteReader(); r.Read(); )
				{
					obj = new batch_lot(r["batch"].ToString(),
											r["prod_code"].ToString(),
											r["line_id"].ToString(),
											r["part"].ToString());

					obj.location_ = new location(r["location"].ToString());
					obj.lot_number = r["lot_number"].ToString();
					obj.as_400 = r["as_400"].ToString();
					obj.number_of_bulk_packs = int.Parse(r["number_of_bulk_packs"].ToString());
					break;
				}
			}
			return obj;
		}

		public batch_header get_batch_header(batch_header b)
		{
			var qry = new StringBuilder("select * from batch_header with (nolock) ");
			if (b.cycle == -1)
				qry.AppendFormat(" where batch='{0}' and prod_code='{1}' and line_id='{2}' and part='{3}' and location='{4}' ",
									b.lotno.id, b.product.id, b.line.id, b.part, b.location.id);
			else
				qry.AppendFormat(" where batch='{0}' and prod_code='{1}' and line_id='{2}' and part='{3}' and location='{4}' and cycle={5} ",
									b.lotno.id, b.product.id, b.line.id, b.part, b.location.id, b.cycle);
			batch_header obj = null;
			using (var cmd = database.CreateCommand(qry.ToString(), conn))
			{
				for (var r = cmd.ExecuteReader(); r.Read(); )
				{
					obj = new batch_header(r["batch"].ToString(),
											r["prod_code"].ToString(),
											r["line_id"].ToString(),
											r["part"].ToString(),
											r["location"].ToString(),
											int.Parse(r["cycle"].ToString()));
					//                    obj.location = new location(r["location"].ToString());
					//                    obj.cycle = int.Parse(r["cycle"].ToString());
					obj.status = r["status"].ToString().TrimEnd();
					obj.finishontime = r["new_pkg_count"].ToString();
					obj.as_400 = r["as_400"].ToString();
					obj.comments = r["comment"].ToString();
					//                    obj.comments = obj.comments.Trim();
					obj.date_time = (DateTime)r["date_time"];
					obj.creation_type = int.Parse(r["reason_code_1"].ToString());
					obj.boxes = int.Parse(r["reason_code_3"].ToString());
					break;
				}
			}
			return obj;
		}
		public batch_header get_batch_header(batch b, location l, int cycle)
		{
			var qry = new StringBuilder("select * from batch_header with (nolock) ");
			if (cycle == -1)
				qry.AppendFormat(" where batch='{0}' and prod_code='{1}' and line_id='{2}' and part='{3}' and location='{4}' ",
									b.lotno.id, b.product.id, b.line.id, b.part, l.id);
			else
				qry.AppendFormat(" where batch='{0}' and prod_code='{1}' and line_id='{2}' and part='{3}' and location='{4}' and cycle={5} ",
									b.lotno.id, b.product.id, b.line.id, b.part, l.id, cycle);
			batch_header obj = null;
			using (var cmd = database.CreateCommand(qry.ToString(), conn))
			{
				for (var r = cmd.ExecuteReader(); r.Read(); )
				{
					obj = new batch_header(r["batch"].ToString(),
											r["prod_code"].ToString(),
											r["line_id"].ToString(),
											r["part"].ToString(),
											r["location"].ToString(),
											int.Parse(r["cycle"].ToString()));
					//                    obj.location = new location(r["location"].ToString());
					//                    obj.cycle = int.Parse(r["cycle"].ToString());
                    obj.status = r["status"].ToString().TrimEnd();
					obj.finishontime = r["new_pkg_count"].ToString();
					obj.as_400 = r["as_400"].ToString();
					obj.comments = r["comment"].ToString();
					//                    obj.comments = obj.comments.Trim();
					obj.date_time = (DateTime)r["date_time"];
					obj.creation_type = int.Parse(r["reason_code_1"].ToString());
					obj.boxes = int.Parse(r["reason_code_3"].ToString());
					break;
				}
			}
			return obj;
		}
		public void insert_batch_header(batch_header b)
		{
            DateTime dt = (DateTime)b.date_time;
			var qry = new StringBuilder("insert into batch_header ");
			qry.AppendFormat(" values('{0}', '{1}', '{2}', '{3}', '{4}', {5}, " +
								" '{6}', '{7}', '{8}', 0,0, '{9}', '{10}', {11}, 0, {12},0)",
								b.lotno.id, b.line.id, b.part, b.product.id, b.location.id, b.cycle,
								b.status, b.finishontime, b.as_400, b.comments, 
                                dt.ToString("yyyy/MM/dd HH:mm:ss"), 
                                b.creation_type, b.boxes);
			using (var cmd = database.CreateCommand(qry.ToString(), conn))
			{
				cmd.ExecuteNonQuery();
			}
		}
		public void update_batch_header(batch_header b)
		{
			var qry = new StringBuilder("update batch_header ");
			qry.AppendFormat(" set status='{0}', new_pkg_count='{1}', as_400='{2}', comment='{3}', date_time='{4}', reason_code_1={5}, reason_code_3={6} " +
								" where batch='{7}' and prod_code='{8}' and line_id='{9}' and part='{10}' and location='{11}' and cycle={12} ",
								b.status, b.finishontime, b.as_400, b.comments, b.date_time, b.creation_type, b.boxes,
								b.lotno.id, b.product.id, b.line.id, b.part, b.location.id, b.cycle);
			using (var cmd = database.CreateCommand(qry.ToString(), conn))
			{
				cmd.ExecuteNonQuery();
			}
		}
		public void delete_batch_header(batch_header b)
		{
			var qry = new StringBuilder("delete batch_header ");
			qry.AppendFormat(" where batch='{0}' and prod_code='{1}' and line_id='{2}' and part='{3}' and location='{4}' and cycle={5} ",
								b.lotno.id, b.product.id, b.line.id, b.part, b.location.id, b.cycle);
			using (var cmd = database.CreateCommand(qry.ToString(), conn))
			{
				cmd.ExecuteNonQuery();
			}
		}

		public void insert_batch_detail(batch_detail b)
		{
			var qry = new StringBuilder("insert into batch_detail ");
			qry.AppendFormat(" values('{0}', '{1}', '{2}', '{3}', '{4}', {5}, " +
								" '{6}', '{7}', {8}, {9}, null) ",
								b.lotno.id, b.line.id, b.part, b.product.id, b.location.id, b.cycle,
								b.detail_type, b.sku.id, b.reason_code, b.qty);
			using (var cmd = database.CreateCommand(qry.ToString(), conn))
			{
				cmd.ExecuteNonQuery();
			}
		}
		public void update_batch_detail(batch_detail b)
		{
			var qry = new StringBuilder("update batch_detail ");
			qry.AppendFormat(" set qty={0} " +
								" where batch='{1}' and prod_code='{2}' and line_id='{3}' and part='{4}' and location='{5}' and cycle={6} and " +
								" detail_type='{7}' and resource='{8}' and reason_code='{9}' ",
								b.qty,
								b.lotno.id, b.product.id, b.line.id, b.part, b.location.id, b.cycle, b.detail_type, b.sku.id, b.reason_code);
			using (var cmd = database.CreateCommand(qry.ToString(), conn))
			{
				cmd.ExecuteNonQuery();
			}
		}
		public void delete_batch_detail(batch_detail b)
		{
			var qry = new StringBuilder("delete batch_detail with (rowlock) ");
			qry.AppendFormat(" where batch='{0}' and prod_code='{1}' and line_id='{2}' and part='{3}' and location='{4}' and cycle={5} and " +
								" detail_type='{6}' and resource='{7}' and reason_code='{8}' ",
							   b.lotno.id, b.product.id, b.line.id, b.part, b.location.id, b.cycle, b.detail_type, b.sku.id, b.reason_code);
			using (var cmd = database.CreateCommand(qry.ToString(), conn))
			{
				cmd.ExecuteNonQuery();
			}
		}
		public void delete_batch_detail_all(batch_header b)
		{
			var qry = new StringBuilder("delete batch_detail with (rowlock) ");
			qry.AppendFormat(" where batch='{0}' and prod_code='{1}' and line_id='{2}' and part='{3}' and location='{4}' and cycle={5} ",
								b.lotno.id, b.product.id, b.line.id, b.part, b.location.id, b.cycle);
			using (var cmd = database.CreateCommand(qry.ToString(), conn))
			{
				cmd.ExecuteNonQuery();
			}
		}

		public List<batch_detail> get_batch_detail_all(batch_header b)
		{
			var data = new List<batch_detail>();
			var qry = new StringBuilder("select * from batch_detail with (nolock) ");
			qry.AppendFormat(" where batch='{0}' and prod_code='{1}' and line_id='{2}' and part='{3}' and location='{4}' and cycle={5} ",
								b.lotno.id, b.product.id, b.line.id, b.part, b.location.id, b.cycle);
			using (var cmd = database.CreateCommand(qry.ToString(), conn))
			{
				for (var r = cmd.ExecuteReader(); r.Read(); )
				{
					var obj = new batch_detail(r["batch"].ToString(),
											r["prod_code"].ToString(),
											r["line_id"].ToString(),
											r["part"].ToString());
					obj.location = new location(r["location"].ToString());
					obj.cycle = int.Parse(r["cycle"].ToString());

					obj.detail_type = r["detail_type"].ToString();
					obj.sku = new resource(r["resource"].ToString());
					obj.reason_code = int.Parse(r["reason_code"].ToString());
					obj.qty = int.Parse(r["qty"].ToString());
					data.Add(obj);
				}
			}
			return data;
		}

		public batch_detail get_batch_detail(batch_detail b)
		{
			var qry = new StringBuilder("select * from batch_detail with (nolock) ");
			qry.AppendFormat(" where batch='{0}' and prod_code='{1}' and line_id='{2}' and part='{3}' and location='{4}' and cycle={5} and " +
								"resource='{6}' and detail_type='{7}' and reason_code={8} ",
								b.lotno.id, b.product.id, b.line.id, b.part, b.location.id, b.cycle,
								b.sku.id, b.detail_type, b.reason_code);
			batch_detail obj = null;
			using (var cmd = database.CreateCommand(qry.ToString(), conn))
			{
				for (var r = cmd.ExecuteReader(); r.Read(); )
				{
					obj = new batch_detail(r["batch"].ToString(),
											r["prod_code"].ToString(),
											r["line_id"].ToString(),
											r["part"].ToString());
					obj.location = new location(r["location"].ToString());
					obj.cycle = int.Parse(r["cycle"].ToString());

					obj.detail_type = r["detail_type"].ToString();
					obj.sku = new resource(r["resource"].ToString());
					obj.reason_code = int.Parse(r["reason_code"].ToString());
					obj.qty = int.Parse(r["qty"].ToString());
					break;
				}
			}
			return obj;
		}

		public postcured_data get_postcured_data(postcured_data b)
		{
			var qry = new StringBuilder("select * from t_postcure_data with (nolock) ");
			qry.AppendFormat(" where batch='{0}' and prod_code='{1}' and line_id='{2}' and part='{3}' ",
								b.lotno.id, b.product.id, b.line.id, b.part);
			postcured_data obj = null;
			using (var cmd = database.CreateCommand(qry.ToString(), conn))
			{
				for (var r = cmd.ExecuteReader(); r.Read(); )
				{
					obj = new postcured_data(r["batch"].ToString(),
											r["prod_code"].ToString(),
											r["line_id"].ToString(),
											r["part"].ToString());
					obj.oven = new oven(r["oven"].ToString());
					obj.oper = new operador(r["operator"].ToString());
					break;
				}
			}
			return obj;
		}
		public void insert_postcured_data(postcured_data b)
		{
			var qry = new StringBuilder("insert into t_postcure_data ");
			qry.AppendFormat(" values('{0}', '{1}', '{2}', '{3}', '{4}', '{5}')",
								b.lotno.id, b.product.id, b.line.id, b.part, b.oven.id, b.oper.id);
			using (var cmd = database.CreateCommand(qry.ToString(), conn))
			{
				cmd.ExecuteNonQuery();
			}
		}
		public void update_postcured_data(postcured_data b)
		{
			var qry = new StringBuilder("update t_postcure_data ");
			qry.AppendFormat(" set oven='{0}', operator='{1}' " +
								" where batch='{2}' and prod_code='{3}' and line_id='{4}' and part='{5}'",
								b.oven.id, b.oper.id,
								b.lotno.id, b.product.id, b.line.id, b.part);
			using (var cmd = database.CreateCommand(qry.ToString(), conn))
			{
				cmd.ExecuteNonQuery();
			}
		}

		public resine_data get_resine_data(resine_data b)
		{
			var qry = new StringBuilder("select * from t_resine_data with (nolock) ");
			qry.AppendFormat(" where batch='{0}' and prod_code='{1}' and line_id='{2}' and part='{3}' ",
								b.lotno.id, b.product.id, b.line.id, b.part);
			resine_data obj = null;
			using (var cmd = database.CreateCommand(qry.ToString(), conn))
			{
				for (var r = cmd.ExecuteReader(); r.Read(); )
				{
					obj = new resine_data(r["batch"].ToString(),
											r["prod_code"].ToString(),
											r["line_id"].ToString(),
											r["part"].ToString());
					obj.lot_res = r["lote_res"].ToString();
					obj.date_time = (DateTime)r["fecha"];
					break;
				}
			}
			return obj;
		}
		public void insert_resine_data(resine_data b)
		{
			var qry = new StringBuilder("insert into t_resine_data ");
			qry.AppendFormat(" values('{0}', '{1}', '{2}', '{3}', '{4}', '{5}')",
								b.lotno.id, b.product.id, b.line.id, b.part, b.lot_res, b.date_time.ToString());
			using (var cmd = database.CreateCommand(qry.ToString(), conn))
			{
				cmd.ExecuteNonQuery();
			}
		}
		public void update_resine_data(resine_data b)
		{
			var qry = new StringBuilder("update t_resine_data ");
			qry.AppendFormat(" set lote_res='{0}', fecha='{1}' " +
								" where batch='{2}' and prod_code='{3}' and line_id='{4}' and part='{5}'",
								b.lot_res, b.date_time.ToString(),
								b.lotno.id, b.product.id, b.line.id, b.part);
			using (var cmd = database.CreateCommand(qry.ToString(), conn))
			{
				cmd.ExecuteNonQuery();
			}
		}

		public oven_data get_oven_data(oven_data b)
		{
			var qry = new StringBuilder("select * from t_oven_data with (nolock) ");
			qry.AppendFormat(" where batch='{0}' and prod_code='{1}' and line_id='{2}' and part='{3}' ",
								b.lotno.id, b.product.id, b.line.id, b.part);
			oven_data obj = null;
			using (var cmd = database.CreateCommand(qry.ToString(), conn))
			{
				for (var r = cmd.ExecuteReader(); r.Read(); )
				{
					obj = new oven_data(r["batch"].ToString(),
										r["prod_code"].ToString(),
										r["line_id"].ToString(),
										r["part"].ToString());
					obj.oven = new oven(r["oven"].ToString());
					break;
				}
			}
			return obj;
		}
		public void insert_oven_data(oven_data b)
		{
			var qry = new StringBuilder("insert into t_oven_data ");
			qry.AppendFormat(" values('{0}', '{1}', '{2}', '{3}', '{4}')",
								b.lotno.id, b.product.id, b.line.id, b.part, b.oven.id);
			using (var cmd = database.CreateCommand(qry.ToString(), conn))
			{
				cmd.ExecuteNonQuery();
			}
		}
		public void update_oven_data(oven_data b)
		{
			var qry = new StringBuilder("update t_oven_data ");
			qry.AppendFormat(" set oven='{0}' " +
								" where batch='{1}' and prod_code='{2}' and line_id='{3}' and part='{4}'",
								b.oven.id,
								b.lotno.id, b.product.id, b.line.id, b.part);
			using (var cmd = database.CreateCommand(qry.ToString(), conn))
			{
				cmd.ExecuteNonQuery();
			}
		}

		public line get_line(line l)
		{
			var qry = new StringBuilder("select * from tlines with (nolock) ");
			qry.AppendFormat(" where lineid='{0}' ", l.id);
			line obj = null;
			using (var cmd = database.CreateCommand(qry.ToString(), conn))
			{
				for (var r = cmd.ExecuteReader(); r.Read(); )
				{
					obj = new line(r["lineid"].ToString());

					obj.description = r["description"].ToString();
					obj.val_type = r["val_type"].ToString();
					obj.create_type = r["create_type"].ToString();
					obj.islean = r["islean"].ToString();

					break;
				}
			}
			return obj;
		}

		public product get_product(string prod_code)
		{
			var qry = new StringBuilder("select attribute, desc_long, desc_short, handed, " +
										"material, group_, type, tiempo_standar, yield_nivel, " +
										"inmold, aql_type, mix_type " +
										"from product_ext with (nolock) ");
			qry.AppendFormat(" where prod_code='{0}' ", prod_code);
			product obj = null;
			using (var cmd = database.CreateCommand(qry.ToString(), conn))
			{
				for (var r = cmd.ExecuteReader(); r.Read(); )
				{
					obj = new product(prod_code);

					obj.attribute = int.Parse(r[0].ToString());
					obj.desc_long = r.GetString(1);
					obj.desc_short = r.GetString(2);
					obj.handed = r.GetString(3);
					obj.material = r.GetString(4);
					obj.group_ = r.GetString(5);
					obj.type = r.GetString(6);
					obj.tiempo_standar = int.Parse(r[7].ToString());
					obj.yield_nivel = int.Parse(r[8].ToString());
					obj.inmold = int.Parse(r[9].ToString());
					obj.aql_type = new qc_aql_type(r.GetString(10));
					obj.mix_type = r.GetString(11);

					break;
				}
			}
			err.require(obj == null, mse.PROD_NOT_EXISTS);  //URI04 - SI no existe prod marca error
			return obj;
		}

		public List<batch_coat_cast_relation> get_batch_coat_cast_relation_all(batch b)
		{
			var data = new List<batch_coat_cast_relation>();

			var qry = new StringBuilder("select * from tpermagard with (nolock) ");
			qry.AppendFormat(" where batch='{0}' and prod='{1}' and line='{2}' and part='{3}' ",
								b.lotno.id, b.product.id, b.line.id, b.part);
			batch_coat_cast_relation obj = null;
			using (var cmd = database.CreateCommand(qry.ToString(), conn))
			{
				for (var r = cmd.ExecuteReader(); r.Read(); )
				{
					obj = new batch_coat_cast_relation(r["batch"].ToString(),
														r["prod"].ToString(),
														r["line"].ToString(),
														r["part"].ToString(),
														r["prodbatch"].ToString(),
														r["prodprod"].ToString(),
														r["prodline"].ToString(),
														r["prodpart"].ToString());
					obj.baseno = new basenum(r["prodbase"].ToString());
					obj.resource_ = new resource(r["prodresource"].ToString());
					obj.qty = int.Parse(r["prodqty"].ToString());

					data.Add(obj);
				}
			}
			return data;
		}

		public string get_status_desc(int s, string l)
		{
			var qry = string.Format("execute get_status_desc {0},'{1}';", s,l);
			using (var cmd = database.CreateCommand(qry, conn))
			{
				for (var r = cmd.ExecuteReader(); r.Read(); )
				{
					return r.GetString(0);
				}
			}
			return string.Empty; 
		}

		public mroJSON _get_production_data(string b, string p, string l)
		{
			var data = new mroJSON();
			var qry = string.Format(
				"select sum(thdr) as thdr, sum(bhdr) as bhdr, sum(bdtl) as bdtl, " +
					"sum(qhdr) as qhdr, sum(qdtl) as qdtl, sum(inv) as inv " +
					"from ( " +
						"select count(*) as thdr, bhdr=0, bdtl=0, qhdr=0, qdtl=0, inv=0 from thdr with (nolock) where hdrid='{0}' and pcc='{1}' and line='{2}' " +
						"union select 0, count(*) as bhdr, 0 ,0 ,0, 0 from batch_header with (nolock) where batch ='{3}' and prod_code='{4}' and line_id='{5}' " +
						"union select 0, 0, count(*) as bdtl, 0 ,0, 0 from batch_detail with (nolock) where batch='{6}' and prod_code='{7}' and line_id='{8}' " +
						"union select 0, 0, 0, count(*) as qhdr, 0, 0 from tqchdr2 with (nolock) where batch='{9}' and prod='{10}' and line='{11}' " +
						"union select 0, 0, 0, 0, count(*) as qdtl, 0 from tqcdtlhis with (nolock) where batch='{12}' and pcc='{13}' and line='{14}' " +
						"union select 0, 0, 0, 0, 0, count(*) as inv from mold_inv with (nolock) where batch='{15}' and prod_code='{16}' and line_id='{17}' " +
					") data ",
							b, p, l,
							b, p, l,
							b, p, l,
							b, p, l,
							b, p, l,
							b, p, l);
			using (var cmd = database.CreateCommand(qry, conn))
			{
				for (var r = cmd.ExecuteReader(); r.Read(); )
				{
					data.set("thdr", r.GetInt32(0));
					data.set("bhdr", r.GetInt32(1));
					data.set("bdtl", r.GetInt32(2));
					data.set("qhdr", r.GetInt32(3));
					data.set("qdtl", r.GetInt32(4));
					data.set("inv", r.GetInt32(5));
				}
			}
			return data;
		}
		public void _loadinfoplanlens(string b, string p, string l)
		{
			var qry = string.Format("execute LoadInfoPlanlens '{0}','{1}','{2}',0;", b,l,p);
			using (var cmd = database.CreateCommand(qry, conn))
			{
				cmd.ExecuteNonQuery();
			}
		}
		public void _loadinfoplan(string b, string p, string l)
		{
			var qry = string.Format("execute LoadInfoPlan '{0}','{1}','{2}',0;", b,l,p);
			using (var cmd = database.CreateCommand(qry, conn))
			{
				cmd.ExecuteNonQuery();
			}
		}

		public string get_twin(product p)
		{
			var qry = string.Format("execute get_twin '{0}';", p.id);
			using (var cmd = database.CreateCommand(qry, conn))
			{
				for (var r = cmd.ExecuteReader(); r.Read(); )
				{
					return r.GetString(0);
				}
			}
			return string.Empty;
		}

		#region palletes
		public List<relation_pallet> get_relation_pallet(lot lt, line ln)
		{
			var data = new List<relation_pallet>();
			var qry = string.Format("select * from t_relationtable with (nolock) " +
									"where batch='{0}' and module='{1}'; ",
									lt.id, ln.id);
			using (var cmd = database.CreateCommand(qry, conn))
			{
				for (var r = cmd.ExecuteReader(); r.Read(); )
				{
					relation_pallet rp = new relation_pallet();

					rp.lote = new lot(r.GetString(0));
					rp.module = r.GetString(1);
					rp.palletid = new pallet(r.GetString(2));

					rp.frontmold = new mold(r.GetString(3));
					rp.backmold = new mold(r.GetString(4));
					rp.sku = new resource(r.GetString(5));

					rp.moldindex = r.GetString(6);

					data.Add(rp);
				}
			}
			return data;
		}
		public relation_pallet get_relation_pallet_dtl(lot lt, line ln, pallet pl)
		{
			var qry = string.Format("select * from t_relationtable with (nolock) " +
									"where batch='{0}' and module='{1}' " + 
									" and palletid='{2}';", 
									lt.id,ln.id, pl.id);
			using (var cmd = database.CreateCommand(qry, conn))
			{
				for (var r = cmd.ExecuteReader(); r.Read(); )
				{
					relation_pallet rp = new relation_pallet();

					rp.lote = new lot(r.GetString(0));
					rp.module = r.GetString(1);
					rp.palletid = new pallet(r.GetString(2));

					rp.frontmold = new mold(r.GetString(3));
					rp.backmold = new mold(r.GetString(4));
					rp.sku = new resource(r.GetString(5));

					rp.moldindex = r.GetString(6);

					return rp;
				}
			}
			return null;
		}

		public void insert_relation_pallet(relation_pallet p)
		{
			var qry = new StringBuilder("insert into t_relationtable ");
			qry.AppendFormat(" values('{0}', '{1}', '{2}', '{3}', '{4}', " +
							"'{5}', '{6}', getdate()); ",
							p.lote.id, p.module, p.palletid.id, 
							p.frontmold.id, p.backmold.id, p.sku.id, 
							p.moldindex);
			using (var cmd = database.CreateCommand(qry.ToString(), conn))
			{
				cmd.ExecuteNonQuery();
			}
		}
		public void delete_relation_pallet(lot lt, line ln, pallet pl)
		{
			var qry = new StringBuilder("delete t_relationtable ");
			qry.AppendFormat("where batch='{0}' and module='{1}' and palletid='{2}'; ",
							lt.id, ln.id, pl.id);
			using (var cmd = database.CreateCommand(qry.ToString(), conn))
			{
				cmd.ExecuteNonQuery();
			}
		}
		public void delete_whole_relation_pallet(lot lt, line ln)
		{
			var qry = new StringBuilder("delete t_relationtable ");
			qry.AppendFormat("where batch='{0}' and module='{1}'; ",
							lt.id, ln.id);
			using (var cmd = database.CreateCommand(qry.ToString(), conn))
			{
				cmd.ExecuteNonQuery();
			}
		}
		public void change_relation_pallet_batch(lot src, lot dst, line ln)
		{
			var qry = new StringBuilder("update t_relationtable set batch='{0}' ");
			qry.AppendFormat("where batch='{1}' and module='{2}'; ",
							dst.id, src.id, ln.id);
			using (var cmd = database.CreateCommand(qry.ToString(), conn))
			{
				cmd.ExecuteNonQuery();
			}
		}

		public bool is_mold_for_validate(lot lt, product pr, line ln, mold ml)
		{
			var qry = string.Format("if exists (select moldid " + 
									"from t_p_molds_4_val with (nolock) " +
									"where batch='{0}' and prod_code='{1}' and line_id='{2}1' and moldid='{3}') " +
									"select 1 as res else select 0 ",
									lt.id, pr.id, ln.id.Substring(0,1), ml.id);
			using (var cmd = database.CreateCommand(qry, conn))
			{
				for (var r = cmd.ExecuteReader(); r.Read(); )
				{
					return r.GetInt32(0) == 1;
				}
			}
			return false;
		}

		public bool do_match_front_back(product pr, string bfront, string bback)
		{
			var qry = string.Format("if exists (select base from tpolybacks with (nolock) " +
								"where prod_code='{0}' and base='{1}' and back='{2}')" +
								"select 1 as res else select 0 ",
								pr.id, bfront, bback);
			using (var cmd = database.CreateCommand(qry, conn))
			{
				for (var r = cmd.ExecuteReader(); r.Read(); )
				{
					return r.GetInt32(0) == 1;
				}
			}
			return false;
		}

		public bool is_mold_on_production(mold ml)
		{
			var qry = string.Format("if exists(select moldid from VW_polyValidMoldsbacks " +
									"where moldid='{0}')" +
									"select 1 as res else select 0 ",
									ml.id);
			using (var cmd = database.CreateCommand(qry, conn))
			{
				for (var r = cmd.ExecuteReader(); r.Read(); )
				{
					return r.GetInt32(0) == 1;
				}
			}
			return false;
		}

		public relation_pallet get_mold_in_another_line(lot lt, mold ml, string shift, bool isfront)
		{
			var qry = string.Format("select top 1 * " +
									"from t_relationtable with (nolock) " +
				//URI 05/24/2012 - Que valide el turno ( el molde se puede repetir pero en turnos diferentes )                        
									"where batch='{0}' and {1}='{2}' and Substring(module,2,1) ='{3}'",
									lt.id, isfront ? "moldid" : "backmoldid", ml.id, shift);
			using (var cmd = database.CreateCommand(qry, conn))
			{
				for (var r = cmd.ExecuteReader(); r.Read(); )
				{
					relation_pallet rp = new relation_pallet();

					rp.lote = new lot(r.GetString(0));
					rp.module = r.GetString(1);
					rp.palletid = new pallet(r.GetString(2));

					rp.frontmold = new mold(r.GetString(3));
					rp.backmold = new mold(r.GetString(4));
					rp.sku = new resource(r.GetString(5));

					rp.moldindex = r.GetString(6);

					return rp;
				}
			}
			return null;
		}

		public Tuple<string, string> get_pallete_index(product pr, resource sku)
		{
			var qry = string.Format("select res, mindex from VW_polyMoldsIndex with (nolock) " +
							"where prod_code='{0}' and res='{1}' ", pr.id, sku.id);
			using (var cmd = database.CreateCommand(qry, conn))
			{
				for (var r = cmd.ExecuteReader(); r.Read(); )
				{
					return new Tuple<string, string>(r.GetString(0),r.GetString(1));
				}
			}
			return null;
		}
		public Tuple<string, string> get_pallete_index(product pr, mold ml)
		{
			var qry = string.Format("select res, mindex from VW_polyMoldsIndex with (nolock) " +
							"where prod_code='{0}' and moldid='{1}' ", pr.id, ml.id);
			using (var cmd = database.CreateCommand(qry, conn))
			{
				for (var r = cmd.ExecuteReader(); r.Read(); )
				{
					return new Tuple<string, string>(r.GetString(0), r.GetString(1));
				}
			}
			return null;
		}
		#endregion

        public bool is_transition(product p)
        {
            var qry = string.Format("select count(*) as res from vw_prodxfam with (nolock) " +
					                "where prod_code='{0}' and object_id='TR'",
                                    p.id);
            using (var cmd = database.CreateCommand(qry, conn))
            {
                for (var r = cmd.ExecuteReader(); r.Read(); )
                {
                    return r.GetInt32(0) > 0;
                }
            }
            return false;
        }
        public defect_source get_defect_source (string s)
        {
            var qry = string.Format(
	                "select defectsourcedescription from tdefectsource with (nolock) " +
					"where defectsourceid = {0}", s);
            using (var cmd = database.CreateCommand(qry, conn))
            {
                for (var r = cmd.ExecuteReader(); r.Read(); )
                {
                    return new defect_source(s,r.GetString(0));
                }
            }
            return null;
        }

        #region moldloss
        public void update_all_moldloss(batch b, int status)
        {
            var qry = string.Format("update tdtlml set defectdep={0} " +
                                    "where dtlid='{1}' and pcc='{2}' and line='{3}' and part='{4}' and defectdep=2",
                                    status, b.lotno.id, b.product.id, b.line.id, b.part);
            using (var cmd = database.CreateCommand(qry, conn))
            {
                cmd.ExecuteNonQuery();
            }
        }
        public void update_moldloss(batch b, int order, int status)
        {
            var qry = string.Format("update tdtlml set defectdep={0} " +
                                    "where dtlid='{1}' and pcc='{2}' and line='{3}' and part='{4}' and defectdep=2 and mold_id={5}",
                                    status, b.lotno.id, b.product.id, b.line.id, b.part, order);
            using (var cmd = database.CreateCommand(qry, conn))
            {
                cmd.ExecuteNonQuery();
            }
        }
        public int how_many_pending_moldloss(batch b)
        {
			var qry = string.Format("select count(*) as res from tdtlml with (nolock) " +
                                    "where dtlid='{0}' and pcc='{1}' and line='{2}' and part='{3}' and defectdep=0", 
                                    b.lotno.id, b.product.id, b.line.id, b.part);
			using (var cmd = database.CreateCommand(qry, conn))
			{
				for (var r = cmd.ExecuteReader(); r.Read(); )
				{
					return r.GetInt32(0);
				}
			}
			return 0;
        }

        public List<moldloss_detail> get_moldloss_dtl(batch p)
        {
            var data = new List<moldloss_detail>();
            var qry = string.Format("select * from tdtlml with (nolock) " +
                                    "where dtlid='{0}' and pcc='{1}' and line='{2}' and part='{3}'",
                                    p.lotno.id, p.product.id, p.line.id, p.part);
            using (var cmd = database.CreateCommand(qry, conn))
            {
                for (var r = cmd.ExecuteReader(); r.Read();)
                {
                    moldloss_detail rp = new moldloss_detail(
                        r.GetString(0),
                        r.GetString(3),
                        r.GetString(2),
                        r.GetString(1));

                    rp.basedtl = new basetype(r.GetString(4));
                    rp.addition = r.GetString(5);
                    rp.mouldlr = r.GetString(6);
                    rp.mouldfb = r.GetString(7);
                    rp.mouldsrc = r.GetInt32(8);
                    rp.mouldchg = r.GetInt32(9);

                    rp._mat = r.GetInt16(10);
                    rp._grp = r.GetInt16(11);
                    rp._dep = r.GetInt16(12);
                    rp._def = r.GetInt32(13);

                    rp.total = r.GetInt32(14);
                    rp.oper = r.GetString(15);
                    rp.mold_id = r.GetInt32(16);
                    rp.mol = new mold(r.GetString(17));

                    data.Add(rp);
                }
            }
            return data;
        }
        public moldloss_detail get_moldloss_dtl(string m)
        {
            var qry = string.Format("select * from tdtlml with (nolock) " +
                                    "where mold_id={0}",
                                    m);
            using (var cmd = database.CreateCommand(qry, conn))
            {
                for (var r = cmd.ExecuteReader(); r.Read(); )
                {
                    moldloss_detail rp = new moldloss_detail(
                        r.GetString(0),
                        r.GetString(3),
                        r.GetString(2),
                        r.GetString(1));
                    
                    rp.basedtl = new basetype(r.GetString(4));
                    rp.addition = r.GetString(5);
                    rp.mouldlr = r.GetString(6);
                    rp.mouldfb = r.GetString(7);
                    rp.mouldsrc = r.GetInt16(8);
                    rp.mouldchg = r.GetInt16(9);

                    rp._mat = r.GetByte(10);
                    rp._grp = r.GetByte(11);
                    rp._dep = r.GetByte(12);
                    rp._def = r.GetInt16(13);

                    rp.total = r.GetInt16(14);
                    rp.oper = r.GetString(15);
                    rp.mold_id = r.GetInt32(16);
                    rp.mol = new mold(r.GetString(17));

                    return rp;
                }
            }
            return null;
        }

        public void insert_moldloss_dtl(moldloss_detail p)
        {
            var qry = new StringBuilder("insert into tdtlml ");
			qry.AppendFormat(
                    "(dtlid, pcc, line, part, base, addition, " +
					"mouldLR, mouldFB, mouldsource, mouldchange, " +
					"defectmat, defectgrp, defectdep, defect, " +
					"total, operator, mold) " +
					"values ('{0}','{1}','{2}','{3}','{4}','{5}','{6}','{7}',{8},0,1,2,2,{9},{10},'{11}','{12}')",
					p.lotno.id, p.product.id, p.line.id, p.part,
                    p.basedtl.id, p.addition, p.mouldlr, p.mouldfb, p.mouldsrc, 
                    p._def, p.total, p.oper, p.mol.id);
            using (var cmd = database.CreateCommand(qry.ToString(), conn))
            {
                cmd.ExecuteNonQuery();
            }
        }

        public void update_moldloss_dtl(moldloss_detail p)
        {
            var qry = new StringBuilder("update tdtlml ");
            qry.AppendFormat("set total={0},defectdep={1} where mold_id={2}",
                            p.total, p._dep, p.mold_id);
            using (var cmd = database.CreateCommand(qry.ToString(), conn))
            {
                cmd.ExecuteNonQuery();
            }
        }

        public void delete_moldloss_dtl(string m)
        {
            var qry = new StringBuilder("delete tdtlml ");
            qry.AppendFormat("where mold_id={0}; ", m);
            using (var cmd = database.CreateCommand(qry.ToString(), conn))
            {
                cmd.ExecuteNonQuery();
            }
        }
        #endregion

        public bool check_product_is_coated(string p)
        {
            var qry = string.Format("exec prod_is_coated '{0}';",p);
            using (var cmd = database.CreateCommand(qry, conn))
            {
                for (var r = cmd.ExecuteReader(); r.Read();)
                {
                    return r.GetInt32(0)==1;
                }
            }
            return false;
        }

    }
}

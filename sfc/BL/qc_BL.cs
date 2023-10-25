using System;
using System.Collections.Generic;
using System.Text;
using sfc.BO;
using sfc.DAL;
using mro;

namespace sfc.BL {
   /**
	 * bussines logic for quality control 
	 */
   public class qc_BL {
      public qc_BL(CParameters conns) {
         conns.get(defs.ZDFAULT, ref dbcode);
         manbl = new manufacture_BL(conns);
      }
      public readonly string dbcode = string.Empty;
      public manufacture_BL manbl = null;

      public void validate_qc_block_key(batch b, qc_block t, int noinsp, location l) {
         err.require(b.lotno.id.Length == 0, mse.INC_DAT_BATCH);
         err.require(b.line.id.Length == 0, mse.INC_DAT_LINE);
         err.require(t.id.Length == 0, mse.INC_DAT_BLOCK);
         err.require(noinsp < 1 || noinsp > 999, mse.WRONG_NO_INSPECTION);
         err.require(l.id.Length == 0, mse.INC_DAT_LOC);
      }
      public void validate_qc_block_key(batch b, qc_block t, location l) {
         err.require(b.lotno.id.Length == 0, mse.INC_DAT_BATCH);
         err.require(b.line.id.Length == 0, mse.INC_DAT_LINE);
         err.require(t.id.Length == 0, mse.INC_DAT_BLOCK);
         err.require(l.id.Length == 0, mse.INC_DAT_LOC);
      }
      public List<qc_block_detail> get_qc_inspection_detail(batch b, qc_block t, int noinsp, location l) {
         validate_qc_block_key(b, t, noinsp, l);
         var insp = new qc_block_header(b, t, noinsp, l);
         using (var dal = qc_DAL.instance(dbcode)) {
            return dal.get_qc_inspection_detail(insp);
         }
      }

      public qc_block_header get_qc_inspection(batch b, qc_block t, int noinsp, location l) {
         validate_qc_block_key(b, t, noinsp, l);
         using (var dal = qc_DAL.instance(dbcode)) {
            return dal.get_block_inspection(new qc_block_header(b, t, noinsp, l));
         }
      }
      public qc_block_header get_last_qc_inspection(batch b, qc_block t, location l) {
         validate_qc_block_key(b, t, l);
         using (var dal = qc_DAL.instance(dbcode)) {
            return dal.get_last_block_inspection(new qc_block_header(b, t, l));
         }
      }
      public qc_history get_qc_history(lot lt, line l, string pt, qc_block t, int noinsp) {
         using (var dal = qc_DAL.instance(dbcode)) {
            qc_inspection_level critics = null;
            qc_inspection_level major_l = null;
            qc_inspection_level major_p = null;

            // we check if the product to be inspected are in a permanent disposition
            var prods = manbl.get_full_trolley_batch_relation(lt, l, pt, new trolley(t.id), noinsp);
            foreach (var p in prods) {
               var cr = dal.get_permanent_inspection_level(l, consts.QCINSPCRT, p.prod);
               var ml = dal.get_permanent_inspection_level(l, consts.QCINSPMLN, p.prod);
               var me = dal.get_permanent_inspection_level(l, consts.QCINSPMEM, p.prod);

               if (cr != null) critics = cr;
               if (ml != null) major_l = ml;
               if (me != null) major_p = me;
            }

            if (critics == null) critics = dal.get_inspection_level(l, consts.QCINSPCRT);
            if (major_l == null) major_l = dal.get_inspection_level(l, consts.QCINSPMLN);
            if (major_p == null) major_p = dal.get_inspection_level(l, consts.QCINSPMEM);

            err.require(critics == null, mse.QC_HIS_FOR_LINE_NOT_EXISTS, qcdeftype.CRT);
            err.require(major_l == null, mse.QC_HIS_FOR_LINE_NOT_EXISTS, qcdeftype.MLN);
            err.require(major_p == null, mse.QC_HIS_FOR_LINE_NOT_EXISTS, qcdeftype.MEM);

            return new qc_history(l, critics, major_l, major_p);
         }
      }
      public int get_size_from_trolley(batch b, qc_block t, int noinsp, bool bybox) {
         int size = 0;
         var trol = new trolley(t.id);
         var relation = bybox ? manbl.get_full_trolley_batch_relation(b.lotno, b.line, b.part, trol, noinsp) :
                manbl.get_full_trolley_batch(b.lotno, b.line, b.part, trol, noinsp);
         foreach (var rel in relation)
            size += rel.qty;
         return size;
      }

      public int get_sample_from_aql(qc_aql_type aql, qc_inspection_level lvl,
                              defect_type deftype, int size) {
         using (var dal = qc_DAL.instance(dbcode)) {
            var a = dal.get_aql(aql, lvl, deftype, size);
            err.require(a == null, mse.AQL_RANGE_NOT_REGISTERED, size);
            return a.sample == -1 ? size : a.sample;
         }
      }

      public qc_aql get_aql(qc_aql_type aql, qc_inspection_level lvl,
                        defect_type deftype, int size) {
         using (var dal = qc_DAL.instance(dbcode)) {
            return dal.get_aql(aql, lvl, deftype, size);
         }
      }

      // the idea for this function is to create the inspection level only is there 
      // only one product or all the products that are on the trolley have the same AQL
      public qc_aql_type get_aql_from_trolley(batch b, trolley t, int noinsp, bool bybox) {
         var prods = bybox ? manbl.get_full_trolley_batch_relation(b.lotno, b.line, b.part, t, noinsp) :
             manbl.get_full_trolley_batch(b.lotno, b.line, b.part, t, noinsp);
         var pro = string.Empty;
         var aql = string.Empty;
         foreach (var prd in prods) {
            var p = manbl.get_product(prd.prod.id);
            if (pro.Length == 0) {
               pro = prd.prod.id;
               aql = p.aql_type.type;
               continue;
            }
            err.require(string.Equals(pro, p.id) == false &&
                     string.Equals(aql, p.aql_type.type) == false,
                     mse.DIFFERENT_AQLS_IN_TROLLEY);
            // different product and aql impossible to know real aql type
         }
         err.require(aql.Length == 0, mse.NO_PRODUCTS_FOUND);
         return new qc_aql_type(aql);
      }

      public void update_qc_inspection_info(batch b, qc_block t, int noinsp, location l, int realsize = 0) {
         validate_qc_block_key(b, t, noinsp, l);

         var insp = get_qc_inspection(b, t, noinsp, l);
         err.require(insp == null, mse.INSP_NOT_EXIST);

         bool bybox = is_block_by_box(b.lotno, b.line, b.part, new trolley(t.id), noinsp);

         insp.total = realsize == 0 ? get_size_from_trolley(b, t, noinsp, bybox) : realsize;
         err.require(insp.total <= 0, mse.QCBLOCK_NOT_EXIST);

         // we need to find out the real sample size according with the aql
         var lvl = new qc_inspection_level("", "N");

         lvl.status = insp.sta_ctr; // only manage one history type (critics) (120 proj)
                                    //if (insp.sta_ctr == "S" && insp.sta_mln == "S") lvl.status = "S"; //&& insp.sta_mem == "S") lvl.status = "S";

         qc_aql_type aqltype = get_aql_from_trolley(b, new trolley(t.id), noinsp, bybox);
         err.require(aqltype == null, mse.CANNOT_GET_AQL_TYPE);

         insp.sample = get_sample_from_aql(aqltype, lvl, consts.DEFTYPQCCRT, insp.total);
         //insp.sample = get_sample_from_aql(aqltype, lvl, consts.DEFTYPQCMAJ, insp.total); (120 proj)

         using (var dal = qc_DAL.instance(dbcode)) {
            // update the sample, size values
            dal.update_block_inspection(insp);
         }

         //calculate_if_need_inspection(insp); (120 proj not need skip lot
      }

      /*public void calculate_if_need_inspection(qc_block_header insp)
		{
				using (var dal = qc_DAL.instance(dbcode))
			{
				var insps = dal.get_block_inspection_all_for_insp(insp.lot.line, insp.creation_date);
				if(insps.Count != 0)
				{
					var skiped_ours = false;
					var howmany = 0;
					var inspected = "";
					foreach(var i in insps)
					{
						// remeber that the first one will be ours
						if(!skiped_ours)
						{
							skiped_ours = true;
							inspected = i.inspected;
							continue;
						}
						if (i.sta_ctr == "S" && i.sta_mln == "S") // && i.sta_mem == "S")
							howmany++;
					}
					if(howmany == 5 && inspected == "Y") 
					{
						insp.inspected = "N";
						dal.update_block_inspection(insp);
					}
				}
			}
		}*/

      public qc_inspection_result generate_inspection_result(batch b, qc_block t, int noinsp, location l) {
         var defects = 0;
         //var majorslin = 0;
         //var majorspkg = 0;

         var insp = get_qc_inspection(b, t, noinsp, l);
         err.require(insp == null, mse.INSP_ALREADY_EXIST);

         var dtl = get_qc_inspection_detail(b, t, noinsp, l);
         foreach (var d in dtl) {
            var def = get_defect(d.def);
            // the QC defects because they are destroy by qc and dont are 
            // generated by produccion dont impact the result
            var category = def.category.Trim();
            if (category != qcdefcat.QC) {
               defects += d.qty; // (120 proj) only one history level
                                 //if (def.type == 0) critics += d.qty;
                                 //if (def.type == 1)
                                 //{
                                 //	if (category == "LINEA") majorslin += d.qty;
                                 //	else majorspkg += d.qty;
                                 //}
            }
         }

         bool bybox = is_block_by_box(b.lotno, b.line, b.part, new trolley(t.id), noinsp);

         qc_aql_type aqltype = get_aql_from_trolley(b, new trolley(t.id), noinsp, bybox);
         err.require(aqltype == null, mse.CANNOT_GET_AQL_TYPE);

         bool smallsample = insp.sta_ctr == "S";//(120 proj) && insp.sta_mln == "S";

         var aqlcritics = get_aql(aqltype, new qc_inspection_level("",
            smallsample ? "S" : insp.sta_ctr), consts.DEFTYPQCCRT, insp.total);

         var rescritics = "P"; // pass by default
         if (aqlcritics.sample != -1) {
            if (defects >= aqlcritics.rejected_with) rescritics = "F";
            if (defects > aqlcritics.accepted_with &&
                defects < aqlcritics.rejected_with) rescritics = "?"; // wrong AQL
         }

         var remajorln = "P";
         var remajorpk = "P";
         return new qc_inspection_result(rescritics, remajorln, remajorpk);
      }
      public bool is_block_by_box(lot lt, line l, string pt, trolley t, int insp) {
         var relation = manbl.get_full_trolley_batch_relation(lt, l, pt, t, insp);
         err.require(relation == null || relation.Count == 0, mse.QCBLOCK_NOT_EXIST);
         return true;
         //if (relation != null && relation.Count > 0) return true;
         //else return false;
      }
      public void create_qc_inspection(batch b, qc_block t, int noinsp, location l, operador o) {
         manbl.daily_batch_must_be_in_wip(b.lotno, b.line);
         validate_qc_block_key(b, t, noinsp, l);

         qc_block_header lastinsp = null;
         if (noinsp > 1) {
            //lastinsp = get_last_qc_inspection(b, t, l);
            //err.require(lastinsp == null, mse.WRONG_NO_INSPECTION);
            //err.require(lastinsp.noinsp != (noinsp - 1), mse.WRONG_NO_INSPECTION);
            //err.require(lastinsp.status != qcdisp.REL, mse.QCBLOCK_LAST_NOT_RELEASED);
         }

         // maybe the inspeccion already exists
         var insp = get_qc_inspection(b, t, noinsp, l);
         err.require(insp != null, mse.INSP_ALREADY_EXIST);

         // we validate the block to be inspected
         bool bybox = is_block_by_box(b.lotno, b.line, b.part, new trolley(t.id), noinsp);
         var relation = manbl.get_full_trolley_batch_relation(b.lotno, b.line, b.part, new trolley(t.id), noinsp);
         if (relation == null || relation.Count == 0) {
            bybox = false;
            relation = manbl.get_full_trolley_batch(b.lotno, b.line, b.part, new trolley(t.id), noinsp);
         }

         //var allreleased = true;
         // we check that the trolley comes with valid data and at least one product is in WIP
         foreach (var dato in relation) {
            err.require(dato.qty == 0, mse.TROLLEY_WRONG_CAPTURE,
               string.Concat(dato.prod.id, "-", dato.baseno.graduation));
            //var bt = manbl.get_batch(new batch(b.lotno.id, dato.prod.id, b.line.id,"1"));
            //err.require(bt == null, mse.WRONG_BLOCK_INFORMATION);
            //if (bt.status != batchstatus.RELEASE) allreleased = false;
         }
         //err.require(allreleased, mse.ALL_PRODUCTS_ARE_RELEASED);

         //valida si se completaron las pruebas de qc            
         //using (qc_DAL qdal = qc_DAL.instance(dbcode))
         //{
         err.require(qc_incomplete_tests(b, t, bybox), mse.QC_TEST_NOT_COMPLETED);
         //}            

         err.require(relation == null || relation.Count == 0, mse.QCBLOCK_NOT_EXIST);

         // we create the block inspection
         insp = new qc_block_header(b, t, noinsp, l);
         insp.oper = o;
         insp.status = status.WIP;
         insp.creation_date = DateTime.Now;
         insp.finish_date = null;

         insp.total = 0;
         insp.sample = 0;
         insp.disposition = status.WIP;
         insp.reason_code = consts.DEFFQ;

         // at the moment of the creation the result is unknown
         var res = new qc_inspection_result("N", "N", "N");
         insp.res_ctr = res.critics;
         insp.res_mln = res.major_line;
         insp.res_mem = res.major_pack;

         // find out the inspection level which the block will be evaluated
         if (lastinsp != null) {
            insp.sta_ctr = lastinsp.sta_ctr;
            insp.sta_mln = lastinsp.sta_mln;
            insp.sta_mem = lastinsp.sta_mem;
         }
         else {

            var history = get_qc_history(b.lotno, b.line, b.part, t, noinsp);
            insp.sta_ctr = history.aql.critics.status;
            insp.sta_mln = history.aql.major_line.status;
            insp.sta_mem = history.aql.major_pack.status;
         }

         // pending: we must calculate when and when not the block needs a inspection
         insp.inspected = "Y";
         insp.aql = get_aql_from_trolley(b, new trolley(t.id), noinsp, bybox);
         insp.part = b.part;

         using (var dal = qc_DAL.instance(dbcode)) {
            dal.insert_block_inspection(insp);
         }

         update_qc_inspection_info(b, t, noinsp, l);
      }

      public bool qc_incomplete_tests(batch b, qc_block t, bool bybox) {
         using (qc_DAL qdal = qc_DAL.instance(dbcode)) {
            return bybox ? qdal.getValidInspec(b, t) == 0 :
                qdal.trolley_batch_exist_tests(b, t) == 0;
         }
      }
      public batch qc_all_prods_have_tests(batch b) {
         using (qc_DAL qdal = qc_DAL.instance(dbcode)) {
            return qdal.qc_all_prods_have_tests(b);
         }
      }

      public void create_qc_pkg_inspection(batch b, qc_block t, int noinsp, location l, operador o) {
         manbl.daily_batch_must_be_in_wip(b.lotno, b.line);
         validate_qc_block_key(b, t, noinsp, l);

         // validate operator
         err.require(o.id.Length == 0, mse.INC_DAT_OPER);

         // maybe the inspeccion already exists
         var insp = get_qc_inspection(b, t, noinsp, l);
         err.require(insp != null, mse.INSP_ALREADY_EXIST);

         insp = new qc_block_header(b, t, noinsp, l);
         insp.status = status.WIP;
         insp.creation_date = DateTime.Now;
         insp.disposition = status.WIP;

         insp.total = 0;
         insp.sample = 0;
         insp.oper = o;
         insp.reason_code = consts.DEFFQ;

         insp.inspected = "N";
         insp.comments = "NA";

         insp.res_ctr = "N";
         insp.res_mln = "N";
         insp.res_mem = "N";
         insp.sta_ctr = "N";
         insp.sta_mln = "N";
         insp.sta_mem = "N";

         insp.aql = new qc_aql_type("S");

         using (var dal = qc_DAL.instance(dbcode)) {
            dal.insert_block_inspection(insp);
         }
      }

      public void release_qc_pkg_inspection(batch b, qc_block t, int noinsp, location l) {
         manbl.daily_batch_must_be_in_wip(b.lotno, b.line);
         validate_qc_block_key(b, t, noinsp, l);

         var insp = get_qc_inspection(b, t, noinsp, l);
         err.require(insp == null, mse.INSP_NOT_EXIST);

         using (var dal = qc_DAL.instance(dbcode)) {
            int totaldefs = 0;
            var dtl = dal.get_qc_inspection_detail(insp);
            foreach (var d in dtl)
               totaldefs++;

            insp.finish_date = DateTime.Now;

            // if at least one defect is found we reject the box
            var status = totaldefs == 0 ? qcdisp.REL : qcdisp.REJ;

            insp.status = status;
            insp.disposition = status;

            dal.update_block_inspection(insp);
         }
      }

      public void set_wip_qc_inspection(batch b, qc_block t, int noinsp, location l) {
         manbl.daily_batch_must_be_in_wip(b.lotno, b.line);
         validate_qc_block_key(b, t, noinsp, l);

         var insp = get_qc_inspection(b, t, noinsp, l);
         err.require(insp == null, mse.INSP_NOT_EXIST);

         insp.status = status.WIP;
         insp.disposition = status.WIP;

         var res = new qc_inspection_result("N", "N", "N");

         insp.res_ctr = res.critics;
         insp.res_mln = res.major_line;
         insp.res_mem = res.major_pack;
         insp.reason_code = consts.DEFFQ;

         using (var dal = qc_DAL.instance(dbcode)) {
            dal.update_block_inspection(insp);
         }
      }

      public void recalculate_evaluation_level(batch b, qc_block t, int noinsp) {
         int lotes10 = 0;
         int lotes5 = 0;
         int rejected10 = 0;
         int rejected5 = 0;
         int snormal10 = 0;

         var normal = "N";
         var reinsp = "R";
         var fail = "F";

         var ctroutofsmall = false;
         var laststatus = "";

         using (var dal = qc_DAL.instance(dbcode)) {
            var blocks = dal.get_block_inspection_all_for_aql(b.lotno, b.line, t);

            foreach (var block in blocks) {
               var res = block.res_ctr;
               if (laststatus.Length == 0) laststatus = block.sta_ctr;

               if (block.status == qcinspstatus.WIP) continue;
               //{
               //    string er = string.Concat(block.lot.lotno.id, "-", block.lot.line.id, 
               //                              " block:", block.block.id);
               //    err.require(true, mse.CANNOT_CALC_INSP_LEVEL, er);
               //}

               if (res != fail && res != reinsp && res != "P" && res != "N") {
                  string er = string.Concat(block.lot.lotno.id, "-", block.lot.line.id,
                                              " block:", block.block.id);
                  err.require(true, mse.CANNOT_CALC_INSP_LEVEL, er);
               }

               if (lotes10 < 5) {
                  if (res == reinsp || res == fail) ++rejected5;
                  lotes5++;
               }
               if (res == reinsp || res == fail) ++rejected10;
               if (res == reinsp || res == fail) ctroutofsmall = true; //any
               if (block.sta_ctr == normal) ++snormal10;

               lotes10++;
            }

            bool tosmall = laststatus == "N" &&
                            lotes10 == 10 && // at least 10 blocks
                            snormal10 == 10 && // since normal has change since 10 blocks
                            !ctroutofsmall;
            bool tonormal = (laststatus == "R" &&
                            lotes5 == 5 &&
                            rejected5 == 0) ||
                            (laststatus == "S" &&
                            ctroutofsmall);
            bool torigurosa = laststatus == "N" &&
                            lotes5 == 5 &&
                            rejected5 >= 2;

            var qchistory = get_qc_history(b.lotno, b.line, b.part, t, noinsp);
            var curraql = qchistory.aql;

            if (tosmall) curraql.critics.status = "S";
            if (tonormal && !tosmall) curraql.critics.status = "N";
            if (torigurosa) curraql.critics.status = "R";
            if (ctroutofsmall && curraql.critics.status == "S") curraql.critics.status = "N";

            dal.update_inspection_level(b.line, curraql.critics);
         }
      }

      public void release_qc_inspection(batch b, qc_block t, int noinsp, location l, defect def) {
         manbl.daily_batch_must_be_in_wip(b.lotno, b.line);
         validate_qc_block_key(b, t, noinsp, l);

         var insp = get_qc_inspection(b, t, noinsp, l);
         err.require(insp == null, mse.INSP_NOT_EXIST);
         err.require(insp.status != status.WIP, mse.INSP_NOT_IN_WIP);
         err.require(!is_enough_inspection_sample(b, t, noinsp, l), mse.NOT_ENOUGH_SAMPLE_COLLECTED);

         var inspres = generate_inspection_result(b, t, noinsp, l);

         insp.status = qcinspstatus.REL;
         insp.disposition = qcdisp.REL;

         // fix the defect code
         if (def == null) def = consts.DEFFQ;
         if (def.id == "") def.id = "0";

         if (inspres.critics == "R" || inspres.critics == "F")//(120 proj) || inspres.major_line == "R" || inspres.major_pack == "R")
      {
            err.require(def.id == "0", mse.IF_HOLDED_NEED_REASON);
            var defect = get_defect(def);
            err.require(defect == null, mse.REJECT_REASON_NOT_EXIST);

            insp.disposition = qcdisp.REJ;
            insp.reason_code = def;
         }

         insp.finish_date = DateTime.Now;

         insp.res_ctr = inspres.critics;
         insp.res_mln = inspres.major_line;
         insp.res_mem = inspres.major_pack;

         using (var dal = qc_DAL.instance(dbcode)) {
            dal.update_block_inspection(insp);
            if (noinsp == 1) // only first inspectio can alter inspection level
            {
               try {
                  recalculate_evaluation_level(b, t, noinsp);
               }
               catch (Exception e) {
                  // in any error we must return to WIP the inspeccion, because 
                  // if we calc first then the not finished inspection will be 
                  // out of the history because it is not finished yet
                  insp.res_ctr = "W";
                  insp.status = "WIP";
                  insp.disposition = "WIP";
                  dal.update_block_inspection(insp);
                  throw;
               }
            }
         }
      }

      /*        public void can_liberar_qc_insp(batch b)
				{
					manbl.batch_must_be_in_wip(b);

					using (var dal = qc_DAL.instance(dbcode))
					{
						var tests = dal.get_product_tests(b.product);
						if (tests.Count == 0) throw new Exception("prod_not_configured");

						foreach (var test in tests)
						{
							var hdr2check = new batch_header(b, new location(test.insp_type), 1);
		//                    hdr2check.location = new location(test.insp_type);
		//                    hdr2check.cycle = 1;
							var bat = manbl.get_batch_header(hdr2check);

							if (bat == null)
							{
								if (test.insp_type != locs.QCT)
								{
									var error = string.Format("{0} {1}", test.insp_type, "at_least_test_not_captured");
									throw new Exception(error);
								}
							}
							else
							{
								var status = bat.status;
								status = status.Trim();
								status = status.ToUpper();
								if (status == status.WIP)
								{
									var error = string.Format("{0} {1}", test.insp_type, "at_least_test_not_finished");
									throw new Exception(error);
								}

								if (test.insp_type == "QCR") // if the product has the transmitance test, we looking for its detail
								{
									if (!dal.exist_transmitance_capture(b)) throw new Exception("cap_dtl_transmitance_not_exist");
								}
							}
						}
					}
				}*/

      public void update_qc_inspection_comments(batch b, qc_block t, int noinsp, location l,
                                       string comments) {
         manbl.daily_batch_must_be_in_wip(b.lotno, b.line);
         validate_qc_block_key(b, t, noinsp, l);
         var insp = get_qc_inspection(b, t, noinsp, l);
         err.require(insp == null, mse.INSP_NOT_EXIST);
         err.require(string.IsNullOrEmpty(comments), mse.INC_DAT_COMMENTS);
         insp.comments = comments;
         using (var dal = qc_DAL.instance(dbcode)) {
            dal.update_block_inspection(insp);
         }
      }
      public List<qc_block_detail> get_qc_inspection_detail_allX(batch b, location l) {
         using (var dal = qc_DAL.instance(dbcode)) {
            return dal.get_qc_inspection_detail_allX(b, l);
         }
      }

      private void create_account_info(batch b,
          //trolley t, int noinsp,
          location l) {
         // buscamos la locacion de control de calidad
         var hdr = new batch_header(b, l, 1);
         var header = manbl.get_batch_header(hdr);
         if (header == null) // first time
         {
            hdr.as_400 = "1";
            hdr.boxes = 0;
            hdr.comments = "";
            hdr.creation_type = 0;
            hdr.date_time = DateTime.Now;
            hdr.finishontime = "1";
            hdr.status = status.REL;
            manbl.insert_batch_header(hdr);
         }
         else {
            header.status = status.REL;
            manbl.update_batch_header(header);
         }

         // we get rid of the orginal data in order to avoid bugs
         var batch2del = new batch_header(b.lotno, b.product, b.line, b.part, l, 1);
         manbl.delete_batch_detail_all(batch2del);
         using (var dal = qc_DAL.instance(dbcode)) {
            // we copy all the reject from the inspection to the location (only rejects)
            //var dtl = dal.get_qc_inspection_detail(new qc_block_header(b,new qc_block(t.id),noinsp,l));
            var dtl = dal.get_qc_inspection_detail_allX(b, l);
            foreach (var d in dtl) {
               var prod = d.sku.id.Substring(0, 3);
               if (prod != b.product.id) continue; // only skus that are from the product

               var detail = new batch_detail(b.lotno, new product(prod), b.line, b.part);
               detail.location = batch2del.location;
               detail.cycle = batch2del.cycle;
               detail.detail_type = typeq.RJ;
               detail.reason_code = int.Parse(d.def.id);
               detail.sku = d.sku;

               var currdetail = manbl.get_batch_detail(detail);
               if (currdetail == null) {
                  detail.qty = d.qty;
                  manbl.insert_batch_detail(detail);
               }
               else {
                  currdetail.qty += d.qty;
                  manbl.update_batch_detail(currdetail);
               }
            }
         }
      }

      /**
       * check every batch that comes from the list of products that exists on every block 
       */
      public void check_batch_status_from_block(lot lt, List<string> prods, line ln, string part) {
         foreach (var prd in prods) {
            var b = new batch(lt, new product(prd), ln, part);
            var hdr = manbl.get_batch(b);
            err.require(hdr.status == batchstatus.RELEASE, mse.BATCH_MUSTBE_WIP);
         }
      }

      /**
		 * this functions takes all the rejects from the inspections and
		 * summarize the rejects a save them on the batch_detail for report
		 * purposes due most of the reports are base on batch_detail.
		 * NOTE: we need to create the QCT batch_header also, so the only source
		 * of products are the rejects by it self and the samples cause if we take
		 * teh rejects as a source of productos there are inspection with no 
		 * rejects but all the inspection must have sample
		 */
      public void pass_qc_blocks_into_batch(lot lt, line ln, string part,
            trolley t,// int noinsp, 
            location lc) {
         using (var dal = qc_DAL.instance(dbcode)) {
            // find out all the products inspected
            var prods = new List<string>();
            ////var dtls = dal.get_qc_inspection_detail(new qc_block_header(new batch(lt, null, ln, part), 
            ////                                                            new qc_block(t.id),
            ////                                                            noinsp,lc));
            //var dtls = dal.get_qc_inspection_detail_allX(new batch(lt, null, ln, part),lc);
            //foreach (var dtl in dtls)
            //{
            //	var prdid = dtl.sku.id.Substring(0, 3);
            //	if(prods.Contains(prdid) == false)
            //		prods.Add(prdid);
            //}

            //// although the rejects are the only saved we need to check the
            //// sample also only for the creation at least of the batch header, 
            //// because when no defects no prods are filled and no location is
            //// created at all
            //var samps = dal.get_qc_inspection_sample_all(new batch(lt, null, ln, ""));
            //foreach (var smp in samps)
            //{
            //	var prdid = smp._sku.id.Substring(0, 3);
            //	if (prods.Contains(prdid) == false)
            //		prods.Add(prdid);
            //}

            var dtls = dal.qc_get_all_prods_inspected(lt, ln, new qc_block(t.id));
            foreach (var dtl in dtls) {
               if (prods.Contains(dtl.id) == false)
                  prods.Add(dtl.id);
            }

            check_batch_status_from_block(lt, prods, ln, part);

            // pass the trolley info into every batch
            foreach (var prd in prods) {
               var b = new batch(lt, new product(prd), ln, part);
               b.validate();
               create_account_info(b,
                   //t, noinsp, 
                   lc);
            }
         }
      }

      /*public void release_batch_from_qc(batch b, location l)
      {
          b.validate();
          can_liberar_qc_insp(b);
          create_account_info(b, l);
          manbl.set_wip_qc_2_release_qc(b);
      }*/

      public void validate_qc_block_detail_key(batch b, qc_block t, int noinsp,
                                     resource sku,
                                     defect def, zone zne) {
         err.require(b.lotno.id.Length == 0, mse.INC_DAT_BATCH);
         err.require(b.line.id.Length == 0, mse.INC_DAT_LINE);
         err.require(t.id.Length == 0, mse.INC_DAT_BLOCK);
         err.require(noinsp < 1 || noinsp > 99, mse.WRONG_NO_INSPECTION);
         err.require(sku.id.Length == 0, mse.INC_DAT_SKU);
         err.require(def.id.Length == 0, mse.INC_DAT_DEFECT);
         err.require(zne.zone_.Length == 0, mse.INC_DAT_QCZONE);
      }

      public qc_block_detail get_inspection_defect(batch b, qc_block t, int noinsp,
                                          location l, resource sku,
                                          defect def, zone zne) {
         validate_qc_block_detail_key(b, t, noinsp, sku, def, zne);

         var dtl = new qc_block_detail(b, t, noinsp, l, sku, def, zne);

         using (var dal = qc_DAL.instance(dbcode)) {
            return dal.get_inspection_defect(dtl);
         }
      }

      public void insert_inspection_defect(batch b, qc_block t, int noinsp, location l,
                                    resource sku,
                                    defect def, zone zne, int qty) {
         validate_qc_block_detail_key(b, t, noinsp, sku, def, zne);

         var defect = get_defect(def);
         err.require(defect == null, mse.DEFECT_NOT_EXIST);

         var insp = get_qc_inspection(b, t, noinsp, l);
         err.require(insp == null, mse.INSP_NOT_EXIST);
         err.require(insp.status != status.WIP, mse.INSP_NOT_IN_WIP);

         var dtl = get_inspection_defect(b, t, noinsp, l, sku, def, zne);
         if (dtl != null) {
            dtl.qty += qty;
            update_inspection_defect(dtl);
            return;
         }

         dtl = new qc_block_detail(b, t, noinsp, l, sku, def, zne, qty);

         using (var dal = qc_DAL.instance(dbcode)) {
            dal.insert_inspection_defect(dtl);
         }
      }

      public void update_inspection_defect(qc_block_detail dtl) {
         using (var dal = qc_DAL.instance(dbcode)) {
            dal.update_inspection_defect(dtl);
         }
      }
      public void update_inspection_defect(batch b, qc_block t, int noinsp, location l,
                                    resource sku,
                                    defect def, zone zne, int qty) {
         validate_qc_block_detail_key(b, t, noinsp, sku, def, zne);

         var insp = get_qc_inspection(b, t, noinsp, l);
         err.require(insp == null, mse.INSP_NOT_EXIST);
         err.require(insp.status != status.WIP, mse.INSP_NOT_IN_WIP);

         var dtl = get_inspection_defect(b, t, noinsp, l, sku, def, zne);
         err.require(dtl == null, mse.REG_NOT_EXIST);

         dtl.qty = qty;

         using (var dal = qc_DAL.instance(dbcode)) {
            dal.update_inspection_defect(dtl);
         }
      }

      public void delete_inspection_defect(batch b, qc_block t, int noinsp, location l,
                                    resource sku,
                                    defect def, zone zne) {
         validate_qc_block_detail_key(b, t, noinsp, sku, def, zne);

         var insp = get_qc_inspection(b, t, noinsp, l);
         err.require(insp == null, mse.INSP_NOT_EXIST);
         err.require(insp.status != status.WIP, mse.INSP_NOT_IN_WIP);

         var dtl = get_inspection_defect(b, t, noinsp, l, sku, def, zne);
         err.require(dtl == null, mse.REG_NOT_EXIST);

         using (var dal = qc_DAL.instance(dbcode)) {
            dal.delete_inspection_defect(dtl);
         }
      }

      public defect get_defect(defect d) {
         using (var dal = qc_DAL.instance(dbcode)) {
            return dal.get_defect(d);
         }
      }
      public defect get_defect_by_location(location l, defect d) {
         using (var dal = qc_DAL.instance(dbcode)) {
            return dal.get_defect_by_location(l, d);
         }
      }


      public void insert_inspection_sample(batch b, qc_block t, int noinsp, location l,
                                 resource sku, int qty) {
         validate_qc_block_key(b, t, noinsp, l);

         err.require(sku.id.Length == 0, mse.INC_DAT_SKU);

         var insp = get_qc_inspection(b, t, noinsp, l);
         err.require(insp == null, mse.INSP_NOT_EXIST);
         err.require(insp.status != status.WIP, mse.INSP_NOT_IN_WIP);

         using (var dal = qc_DAL.instance(dbcode)) {
            var newsample = new qc_block_sample(b, t, noinsp, sku, qty);
            var sample = dal.get_inspection_sample(newsample);

            if (sample != null) {
               sample.qty += qty;
               dal.update_inspection_sample(sample);
            }
            else
               dal.insert_inspection_sample(newsample);
         }
      }

      public bool is_enough_inspection_sample(batch b, qc_block t, int noinsp, location l) {
         validate_qc_block_key(b, t, noinsp, l);

         var insp = get_qc_inspection(b, t, noinsp, l);
         err.require(insp == null, mse.INSP_NOT_EXIST);

         var samplecount = 0;
         using (var dal = qc_DAL.instance(dbcode)) {
            var sample = dal.get_inspection_sample_all(new qc_block_header(b, t, noinsp, consts.LOCQCT));
            if (sample.Count != 0) {
               foreach (var s in sample)
                  samplecount += s.qty;
            }
         }

         return samplecount >= insp.sample;
      }

      public List<qc_block_sample> get_qc_inspection_sample(batch b, qc_block t, int noinsp, location l) {
         validate_qc_block_key(b, t, noinsp, l);

         var insp = get_qc_inspection(b, t, noinsp, l);
         err.require(insp == null, mse.INSP_NOT_EXIST);

         using (var dal = qc_DAL.instance(dbcode)) {
            return dal.get_inspection_sample_all(new qc_block_header(b, t, noinsp, consts.LOCQCT));
         }
      }
      /*public List<Tuple<string, string>> get_qc_inspection_prod_disp(batch b, qc_block t, int noinsp, location l)
      {
          validate_qc_block_key(b, t, noinsp, l);

          var insp = get_qc_inspection(b, t, noinsp, l);
          err.require(insp == null, mse.INSP_NOT_EXIST);

          using (var dal = qc_DAL.instance(dbcode))
          {
              return dal.get_inspection_prod_disp(new qc_block_header(b, t, noinsp, consts.LOCQCT));
          }
      }*/
      public List<qc_test> get_batches_qc_tests(batch b) {
         err.require(!manbl.exist_batch(b), mse.BATCH_NOT_EXIST);

         using (var dal = qc_DAL.instance(dbcode)) {
            return dal.get_batches_qc_tests(b);
         }
      }

      //        public qc_test_full get_prod_loc_data(product p, location l)
      //        {
      //            using (var dal = qc_DAL.instance(dbcode))
      //            {
      //                return dal.get_prod_loc_data(b);
      //            }
      //        }

      public void create_liberar_gen_capture_all(batch b) {
         using (var dal = qc_DAL.instance(dbcode)) {
            dal.create_liberar_gen_capture_all(b);
         }
      }
      public void create_liberar_gen_capture(batch b, mroJSON values) {
         err.require(!manbl.exist_batch(b), mse.BATCH_NOT_EXIST);

         var lstdata = new mroJSON();
         values.get(defs.ZLSTDAT, lstdata);
         var total = lstdata.getint(defs.ZLSTTOT);
         var cols = lstdata.getint(defs.ZLSTCLS);

         var loc = string.Empty;
         for (var i = 0; i < total; ++i) {
            char col = 'A';
            for (var j = 0; j < cols; ++j) {
               switch (j) { // buscamos solamente las columnas que son necesarias
                  case 0:
                     var key = string.Format("{0}{1}", col.ToString(), i);
                     lstdata.get(key, ref loc);
                     break; //5
               }
               col++;
            }

            if (loc == locs.QCT) continue; // la prueba de cosmeticos se tiene que capturar manual;

            // we create the location info (for integrate the inspeccion into the standard DCS)
            var hdr = new batch_header(b, new location(loc), 1);
            // create the qc location, this is for compabilty of the whole system, 
            // the first block will create it and it will be finish when release the batch
            var header = manbl.get_batch_header(hdr);
            if (header == null) // first time
            {
               hdr.as_400 = "1";
               hdr.boxes = 0;
               hdr.comments = "";
               hdr.creation_type = 0;
               hdr.date_time = DateTime.Now;
               hdr.finishontime = "1";
               hdr.status = status.REL;
               manbl.insert_batch_header(hdr);
            }
         }
      }

      public qc_batch_header get_qc_batch_last_inspection(batch b) {
         using (var dal = qc_DAL.instance(dbcode)) {
            return dal.get_qc_batch_last_inspection(b);
         }
      }
      public qc_batch_header get_qc_batch_first_inspection(batch b) {
         using (var dal = qc_DAL.instance(dbcode)) {
            return dal.get_qc_batch_first_inspection(b);
         }
      }
   }
}

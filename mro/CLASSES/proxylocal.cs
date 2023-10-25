#if NETCOREAPP
using Microsoft.AspNetCore.Http;
#endif
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.SqlClient;

namespace mro {
  public partial class proxy {
    /**
     * executes local functions, the arrange is suppossed to be on the frecuancy of use
     */
    private void loc_backend(string f) {
      var l = f.Length;
      if (l == 3 && mem._tmemcmp(f, "RUN")) { runcallable(); return; }
      if (l == 8) {
        if (f[0] == 'a' && f[1] == 't' && f[2] == 'l' && f[3] == 'e' && // atlevent
            f[4] == 'v' && f[5] == 'e' && f[6] == 'n' && f[7] == 't') { atlevent(); return; }
        if (f[0] == 'g' && f[1] == 'e' && f[2] == 't' && f[3] == '_' && // get_file
            f[4] == 'f' && f[5] == 'i' && f[6] == 'l' && f[7] == 'e') { get_file(); return; }
        if (f[0] == 'g' && f[1] == 'i' && f[2] == 'v' && f[3] == 'e' && // giveback
            f[4] == 'b' && f[5] == 'a' && f[6] == 'c' && f[7] == 'k') { giveback(); return; }
        if (f[0] == 'c' && f[1] == 'o' && f[2] == 'n' && f[3] == 's' && // giveback
            f[4] == 'f' && f[5] == 'u' && f[6] == 'l' && f[7] == 'l') { consfull(); return; }
      }
      if (l == 6) {
        if (string.CompareOrdinal(f, "cpyrgt") == 0) { copyrights(); return; }
      }
      if (l == 7) {
        if (f[0] == 'a' && f[1] == 't' && f[2] == 'l' && f[3] == 'c' && // atlcall
            f[4] == 'a' && f[5] == 'l' && f[6] == 'l') { atlcall(); return; }
        if (f[0] == 'n' && f[1] == 'o' && f[2] == 'n' && f[3] == 'e' && // nonefun
            f[4] == 'f' && f[5] == 'u' && f[6] == 'n') { return; }
        if (f[0] == 'c' && f[1] == 'o' && f[2] == 'n' && f[3] == 's' && // nonefun
            f[4] == 'o' && f[5] == 'l' && f[6] == 'e') { console(); return; }
      }
      if (l == 9) {
        if (string.CompareOrdinal(f, "run_query") == 0) { run_query(); return; }
        if (string.CompareOrdinal(f, "chglibgrp") == 0) { chglibgrp(); return; }
      }
      if (l == 10) {
        if (string.CompareOrdinal(f, "notify_use") == 0) { notify_use(); return; }
        if (string.CompareOrdinal(f, "reset_site") == 0) { reset_site(); return; }
        if (string.CompareOrdinal(f, "btn_design") == 0) { btn_design(); return; }
        if (string.CompareOrdinal(f, "chglibcmpy") == 0) { chglibcmpy(); return; }
        if (string.CompareOrdinal(f, "sessetcmpy") == 0) { sessetcmpy(); return; }
      }
      if (l == 11) {
        if (string.CompareOrdinal(f, "addlibentry") == 0) { addlibentry(); return; }
        //if (string.CompareOrdinal(fn, "upload_file") == 0) { upload_file(); return; }
      }
      if (l == 12) {
        if (string.CompareOrdinal(f, "query_direct") == 0) { query_direct(); return; }
        //if (string.CompareOrdinal(f, "get_lastdata") == 0) { lastdata_get(); return; } // this function is obsolete the values are saved on localstorage 
        //if (string.CompareOrdinal(f, "set_lastdata") == 0) { lastdata_set(); return; } // this function is obsolete the values are saved on localstorage 
        //if (string.CompareOrdinal(f, "get_last_css") == 0) { get_last_css(); return; }
        if (string.CompareOrdinal(f, "copy_session") == 0) { copy_session(); return; }
        if (string.CompareOrdinal(f, "get_identity") == 0) { get_identity(); return; }
        if (string.CompareOrdinal(f, "get_cert_gen") == 0) { get_cert_gen(); return; }
      }
      if (l == 13) {
        if (string.CompareOrdinal(f, "execute_query") == 0) { execute_query(); return; }
        if (string.CompareOrdinal(f, "check_session") == 0) { check_session(); return; }
        if (string.CompareOrdinal(f, "gui_get_texts") == 0) { gui_get_texts(); return; }
        if (string.CompareOrdinal(f, "get_date_time") == 0) { get_date_time(); return; }
        if (string.CompareOrdinal(f, "download_file") == 0) { download_file(); return; }
        if (string.CompareOrdinal(f, "get_init_data") == 0) { get_init_data(); return; }
      }
      //if (l == 14) {
      //  if (string.CompareOrdinal(f, "get_last_state") == 0) { get_last_state(); return; }
      //}
      if (l == 15) {
        if (string.CompareOrdinal(f, "get_first_state") == 0) { get_first_state(); return; }
      }
      if (l == 16) {
        if (string.CompareOrdinal(f, "release_sessions") == 0) { release_sessions(); return; }
      }
      if (l == 17) {
        //if (string.CompareOrdinal(fn, "get_logon_options") == 0) { get_logon_options(); return; }
        if (string.CompareOrdinal(f, "gui_get_favorites") == 0) { gui_get_favorites(); return; }
        //if (string.CompareOrdinal(fn, "show_rights_cache") == 0) { show_rights_cache(); return; }
      }
      if (l == 18) {
        if (string.CompareOrdinal(f, "reset_rights_cache") == 0) { reset_rights_cache(); return; }
      }
      //if (l == 19) {
      //if (string.CompareOrdinal(fn, "show_webservice_use") == 0) { show_webservice_use(); return; }
      //}
      if (l == 0) {
        err.require(cme.FUN_MISSING);
      }
      err.require(cme.FUN_NOT_EXIST, f);
    }
    private void runcallable() {
      var kind = values.get("type");
      var name = values.get("name");
      if (name == "RUN") return;          // avoid recursion
      var prm = pools.get(1);
      for (var i = 0; i < 16; ++i) {
        prm.set('p');
        prm.Append(i);
        if (!values.has_val(prm)) break;
        string[] pair = values.extract(prm).Split('/');
        if (pair.Length != 2) break;
        values.set(pair[0], pair[1]);
      }
      loc_backend(name);
    }
    private void reset_site() {
      lock (addrlock) {
        addressesloaded = false;
        init_environment();
      }
    }
    private void reset_rights_cache() {
      corefuncs.reset_rights_cache(rightslock, rights, values.get("cuserini"));
    }
    private void get_init_data() {
      get_date_time();
      get_identity();
            // lastdata_get();  // we need to stop reading the lang and layout from the database and recevied the values from the browser
            result.set(defs.ZLANGUA, values.get(defs.ZLANGUA));
            result.set(defs.ZLAYOUT, values.get(defs.ZLAYOUT));
      get_last_css();
      result.set(defs.ZGUIVER, frm.GUIVER);
    }
    private void get_date_time() {
      response.send(resp, defs.ZDATTIM,
         DateTime.Now.ToString("yyyy/MM/dd hh:mm:ss tt"));
    }
    private void get_identity() {
      response.send(resp, defs.ZIPADDR, clie.ip);
      response.send(resp, defs.ZMACNAM, clie.macname);
      response.send(resp, defs.ZWINUSR, values.get(defs.ZWINUSR));
      response.send(resp, defs.ZWINDOM, values.get(defs.ZWINDOM));
    }
    private void get_cert_gen() {
      if (scripts[0] == null) scripts[0] = doc_get(defs.ZKERNEL, "JS00", "layout", "JSC");
      response.send(resp, "certgen", scripts[0]);
    }
    private void check_api_key(string api_key) {
      err.require(api_key.Length == 0, "certificate_missing");
      err.require(api_key.Length != 12, "certificate_invalid");
      err.require(string.CompareOrdinal(api_key, "030605280209") != 0, "certificate_invalid");
      /*if (string.CompareOrdinal(api_key, "030605280209") != 0) {
          DateTime cerdate = new DateTime(
               int.Parse(api_key.Substring(0, 4)),
               int.Parse(api_key.Substring(4, 2)), 
               int.Parse(api_key.Substring(6, 2)),
               int.Parse(api_key.Substring(8, 2)), 
               int.Parse(api_key.Substring(10, 2)),
               int.Parse(api_key.Substring(12, 2)));
          DateTime todaysDateTime = DateTime.UtcNow;
          TimeSpan span = todaysDateTime.Subtract(cerdate);
          double totalMins = span.TotalMinutes;
          if (totalMins > 5) err.require("certificate_expired");
      }*/
    }
    /*private void lastdata_get() {
      var sql = mem.join3("exec dbo.lastdata_get '", clie.macname, "';");
      var conn = new SqlConnection(dbcode);
      conn.Open();
      var obj = new SqlCommand(sql, conn).ExecuteReader();
      if (obj.HasRows && obj.Read()) {
        var tmp = obj.GetString(0);
        result.set(defs.ZLANGUA, tmp);
        values.set(defs.ZLANGUA, tmp);
        tmp = obj.GetString(1);
        result.set(defs.ZLAYOUT, tmp);
        values.set(defs.ZLAYOUT, tmp);
      }
      conn.Close();
    }*/
    /**
     *	obviously the set last data have to be triggered when the user enter 
     *	on the system isn't it?
     */
    /*private void lastdata_set() {
      var mac = clie.macname;
      if (mac.Length == 0) return;

      sql.sqlnores(dbcode, mem.join7("exec dbo.lastdata_set '", mac.ToString(), "','",
          values.get("clanguage"), "','",
          values.get("clayout"), "';"), true);
    }*/
    private void get_last_css() {
      var res = doc_get_res("", "css" + values.get(defs.ZLAYOUT), "layout", "CSS");
      err.check(res);
      res.del(defs.ZRESEND);
      response.send(resp, res);
    }
    private void gui_get_texts() {
      var a = poolj.getclean(0);
      var lng = values.get(defs.ZLANGUA);

      var conn = new SqlConnection(dbcode);
      conn.Open();
      var r = new SqlCommand(mem.join3("exec dbo.gui_get_texts '", lng, "';"), conn).ExecuteReader();
      if (r.HasRows) for (;r.Read();) a.set(r.GetString(0), r.GetString(1));
      result.set("text_params", a);
      a.clear();
      r = new SqlCommand(mem.join3("exec dbo.gui_get_errs '", lng, "';"), conn).ExecuteReader();
      if (r.HasRows) for (; r.Read();) a.set(r.GetString(0), r.GetString(1));
      result.set("error_params", a);
      conn.Close();
    }
    private void giveback() { values.get(defs.GIVEBACK, result); }
    private void copy_session() {
      var t = pools.getclean(0);
      atl.atlservice(t, null, basics, funs.COPY_SESSION, values);
      mrosocket.atlantic(clie.buffer, proc.bbytes, pools.get(2),
                          atl_svr, gatprt, t, result);
      err.check(result);
      ++savenv;

      // super parche
      if (result.has(defs.ZFILERS)) utils.handle_html(result);
    }
    private void release_sessions() {
      var t = pools.getclean(0);
      atl.atlservice(t, null, basics, funs.RELEASE_SESS);
      mrosocket.atlantic(clie.buffer, proc.bbytes, pools.get(2),
                          atl_svr, gatprt, t, result);
      err.check(result);
    }
    private void check_session() {
      var t = pools.getclean(0);
      atl.atlservice(t, null, basics, funs.CHECK_SESSION);
      mrosocket.atlantic(clie.buffer, proc.bbytes, pools.get(2),
                          atl_svr, gatprt, t, result);
      err.check(result);
    }
    public void copyrights() {
      var from = values.get("from");
      var to = values.get("to");
      err.require(from.Length == 0, cme.INC_DATA_SOURCE);
      err.require(to.Length == 0, cme.INC_DATA_TARGET);
      var qry = string.Format("exec dbo.rights_copy_cmpy '{0}',{1},{2};",
                clie.user, from, to);
      sql.sqlnores(dbcode, qry, true);
    }
    public void sessetcmpy() {
      var t = pools.getclean(0);
      atl.atlservice(t, null, basics, "session_set_company", values);
      mrosocket.atlantic(clie.buffer, proc.bbytes, pools.get(2),
                          atl_svr, gatprt, t, result);
      err.check(result);
    }
    public void chglibgrp() {
      var libgrp = values.get("libgrp");
      err.require(libgrp.Length == 0, cme.INC_DATA_LIBGRP);
      var qry = sql.sp6(pools.get(0), "exec dbo.user_set_libgrp",
                clie.zsesins, clie.zsesmac, clie.zsescli, clie.zsesses, libgrp);
      sql.sqlnores(dbcode, qry, true);
    }
    public void chglibcmpy() {
      var cmpy = values.getint("cmpy", -1);
      err.require(cmpy == -1, cme.INC_DATA_CMPY);
      var qry = string.Format("exec dbo.user_ins_liblist {0},{1},{2},{3},{4},'{5}';",
                clie.zsesins, clie.zsesmac, clie.zsescli, clie.zsesses,
                cmpy, clie.user);
      sql.sqlnores(dbcode, qry, true);
    }
    public void addlibentry() {
      var nam = values.get("name");
      var qry = sql.sp6(pools.get(0), "exec dbo.lib_addlistentry",
                clie.zsesins, clie.zsesmac, clie.zsescli, clie.zsesses, nam);
      sql.sqlnores(dbcode, qry, true);
    }
    public void delliblist() {
      var qry = sql.sp5(pools.get(0), "exec dbo.lib_clean_list",
                        clie.zsesins, clie.zsesmac, clie.zsescli, clie.zsesses);
      sql.sqlnores(dbcode, qry, true);
    }
    private void get_first_state() {
      var lng = poolj.getclean(2);
      lng.set(defs.ZLANGUA, result.get(defs.ZLANGUA));

      var ct = poolj.get(0);
      var t0 = pools.getclean(0);
      atl.atlservice(t0, null, lng, funs.GET_LAST_STATE, values);
      mrosocket.atlantic(clie.buffer, proc.bbytes, pools.get(2),
                          atl_svr, gatprt, t0, ct);
      err.check(ct);

      // super parche
      if (ct.has(defs.ZFILERS)) utils.handle_html(ct);

      result.set(defs.LIBRARY, ct.get(defs.LIBRARY));
      result.set(defs.ZFILE02, ct.get(defs.ZFILERS));
      result.set("fstrncd", ct.get(defs.ZRTRNCD));
      result.set("fhispos", ct.get(defs.ZHISPOS));
    }
    /*private void get_last_state() {
      var t = pools.getclean(0);
      atl.atlservice(t, null, basics, funs.GET_LAST_STATE);
      mrosocket.atlantic(clie.buffer, proc.bbytes, pools.get(2),
                          atl_svr, gatprt, t, result);
      err.check(result);

      // super parche
      if (result.has(defs.ZFILERS)) utils.handle_html(result);
    }*/

    // internal, no intended to be used by the clients
    private string doc_get(string library,
                            string document,
                            string typetran,
                            string type) {
      var t = pools.getclean(0);
      atl.atlservice(t, null, funs.GET_FILE,
                          defs.LIBRARY, library,
                          defs.ZTYPTRN, typetran,
                          defs.ZFILE01, document,
                          defs.PDOCTYP, type);
      var ct = poolj.get(0);
      mrosocket.atlantic(clie.buffer, proc.bbytes, pools.get(2),
                        atl_svr, gatprt, t, ct);
      return ct.get(defs.ZFILERS);
    }
    private mroJSON doc_get_res(string library,
                                    string document,
                                    string typetran,
                                    string type) {
      var t = pools.getclean(0);
      atl.atlservice(t, null, funs.GET_FILE,
                          defs.LIBRARY, library,
                          defs.ZTYPTRN, typetran,
                          defs.ZFILE01, document,
                          defs.PDOCTYP, type);
      var res = new mroJSON();
      mrosocket.atlantic(clie.buffer, proc.bbytes, pools.get(2),
                        atl_svr, gatprt, t, res);
      return res;
    }
    private void get_file() {
      var t = pools.getclean(0);
      var resource = string.Empty;
      atl.atlservice(t, basics, funs.GET_FILE,
                          defs.LIBRARY, values.get(defs.LIBRARY),
                          defs.ZTYPTRN, values.get(defs.ZTYPTRN),
                          defs.ZFILE01, values.get(defs.ZFILE01),
                          defs.PDOCTYP, values.get(defs.PDOCTYP),
                          defs.ZTYPRED, values.get(defs.ZTYPRED));
      mrosocket.atlantic(clie.buffer, proc.bbytes, pools.get(2),
                          atl_svr, gatprt, t, result, ref resource);
      err.check(result);

      // super parche
      if (resource.Length > 0) {
        var typ = values.get(defs.PDOCTYP);

        if (typ == defs.PDOCTRN) 
          langeng.apply_lang(desclock, desccache, dbcode, ref resource, basics);

        if (typ == defs.PDOCTRN || typ == defs.PDOCJSC)
          resource = utils.handle_html(resource);

        result.set(defs.ZFILERS, resource);
      }
      //var typ = values.get(defs.PDOCTYP);
      //var fix = typ == defs.PDOCTRN || typ == defs.PDOCJSC;
      //if (fix && result.has(defs.ZFILERS)) utils.handle_html(result);
    }
    private void notify_use() {
      var t = pools.getclean(0);
      atl.atlservice(t, frm.HDRNOTRETRES, basics, funs.NOTIFY_USE);
      mrosocket.atlantic(clie.buffer, proc.bbytes, pools.get(2),
                          atl_svr, gatprt, t, result);
    }
    private void query_direct() {
      var id = values.get(defs.PQRYCOD);
      err.require(id != defs.VEMBEDD, cme.ONLY_EMBEDDED_QUERIES, id);
      var qry = values.get(defs.PSQLTXT);
      err.require(qry.Length == 0, cme.QUERY_EMPTY);
      sql.sqlnores(dbcode, qry, true);
    }
    private void run_query() {
      var id = values.get(defs.PQRYCOD);
      err.require(id != defs.VEMBEDD, cme.ONLY_EMBEDDED_QUERIES, id);
      var qry = values.get(defs.PSQLTXT);
      err.require(qry.Length == 0, cme.QUERY_EMPTY);
      sql.sqlnores(dbcode, qry, true);
    }
    private void execute_query() {
      var id = values.get(defs.PQRYCOD);
      err.require(id != defs.VEMBEDD, cme.ONLY_EMBEDDED_QUERIES, id);
      var qry = values.get(defs.PSQLTXT);
      err.require(qry.Length == 0, cme.QUERY_EMPTY);
      using (var dal = mro.DAL.control_DAL.instance(dbcode)) {
        var res = dal.execute_query_resp(clie, new ListResponse(), true, 0,
            qry, null, -1, false, false);
        res.pass_to_obj(result);
      }
    }
    private void btn_design() {
      frontend.btn_design(clie, values.get(defs.ZTRNCOD), values.get("dsctrns"));
    }
    private void gui_get_favorites() {
      frontend.gui_get_favorites(clie, dbcode);
    }
    private void download_file() {
      var folder = values.get(defs.ZFOLDER);
      var file = values.get(defs.ZFILE);
      var type = values.get(defs.ZTYPE);
      //err.require(folder.Length == 0, cme.FOLDER_EMPTY);
      err.require(file.Length == 0, cme.FILE_EMPTY);
      var download = new mro.BO.filedownload(client.node, folder, file, type);
      download.pass_into(result);
    }
    private void console() {
      /*siteinfo.return_proxy_state(Request, Response, cpus, domains,
      awakesthread, false, functions, rights, atl_svr, gatprt,
      reqsrec, reqsacc, reqsnpr, reqsrej, reqserr, reqsexe);*/
    }
    private void consfull() {
      /*siteinfo.return_proxy_state(Request, Response, cpus, domains,
      awakesthread, true, functions, rights, atl_svr, gatprt,
      reqsrec, reqsacc, reqsnpr, reqsrej, reqserr, reqsexe);*/
    }
    //protected void gui_get_system_data() {
    //    result.set(defs.ZCMPYNM, sql.sqlscalar(dbcode, "exec dbo.sys_get_name;"));
    //}
  }
}

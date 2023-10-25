/**
 * Author: Miguel Rodriguez Ojeda
 * 
 * Purpose: The purpose of this implementation is that of link between the framework 
 * and some Net site that wants to be part of the system, it routes the dispatch to 
 * specific destination and carry on its values and returns the result to the proxy and
 * eventually the client
 * 
 * significant dates
 * creation: september 21 2009 
 * version 1: november 16 2009  (basic functionallity)
 * version 2: june 7 2010       (clear distinction between proxy and core)
 * version 3: march 14 2011     (use of caches and include support for log, rights, 
 *                                       and most basic core features)
 * version 4: december 5 2011   (increasse stability and high capacity processing)
 * incorporata JSON : august 10 2012    (process mro2json and json2mro converters)
 */
#if NETCOREAPP
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Hosting;
#endif

using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Web;
using System.Reflection;
using System.Threading;
using System.Data.SqlClient;
using mro.BO;
using System.Runtime.InteropServices;

namespace mro {
  /** 
  * this class have to read the framework's configuration in order to the 
  * framework work and specially know the address of the other components
  */
  public sealed class config {
    public static string locaddr = string.Empty;
    public static string locport = string.Empty;
    public static string admingr = string.Empty;
    public static string company = string.Empty;
    public static string dbcode = string.Empty;
    public static string home = string.Empty;
    public static string home_temp = string.Empty;
    public static bool done = false;
    public static readonly object cfglock = new object();

    public bool init(HttpContext Context,
                      object Env,
                      HttpRequest Request,
                      sdata data) {
      locaddr = utils.getlocalip();
#if NETCOREAPP
         locport = Context.GetServerVariable("SERVER_PORT");
         IWebHostEnvironment env = (IWebHostEnvironment)Env;
         home = env.WebRootPath;
#else
      locport = Request.ServerVariables["SERVER_PORT"];
      home = Request.PhysicalApplicationPath;
#endif
      home_temp = mem.join2(home, "temp");

      err.require(data.proxysvr.Length == 0, cme.SERVER_MISSING, "proxy");
      err.require(data.proxyprt.Length == 0, cme.PORT_MISSING, "proxy");
      return get_coredbstr(mem.join3(data.proxysvr, ':', data.proxyprt));
    }
    private bool get_coredbstr(string proxyaddr) {
      /*var addrs = new mroJSON(mrosocket.webservice(
                              new char[8192 + (8192 / 2)], method.GET,
                              mem.join3(dhtml.http,
                              proxyaddr, "/cfgs/addressesJSON.txt")));
      */
     var addrs = new mroJSON(mrosocket.webservice(
                              new char[8192 + (8192 / 2)], method.GET,
                              mem.join3(dhtml.http,
                              proxyaddr, "/cfgs/link.json")));
     var dbinfo = addrs.get(addrs.get("linkdb"));

     addrs.set_value(sql.sqlscalar(dbinfo, mem.join3("exec dbo.get_mro_addresses '", "", "';")));
     addrs.get("zcoredb", ref dbcode);
     addrs.get("admingr", ref admingr);
     addrs.get("cmpyname", ref company);

     using (var connpwd = new SqlConnection(dbinfo)) { 
        connpwd.Open();
        using (var cmd = new SqlCommand("exec dbo.get_mro_pass;", connpwd)) { 
            var r = cmd.ExecuteReader();
            err.require(!r.HasRows, cme.CANNOT_GET_PASSWORDS);
            for (;r.Read();) {
                dbcode = utils.ReplaceEx(null, dbcode, r.GetString(0), r.GetString(1));
            }
        }
     }

      /*var pdict = pdb.get_dict();
      foreach (DictionaryEntry p in pdict)
        dbcode = utils.ReplaceEx(null, dbcode, p.Key.ToString(),
                                               p.Value.ToString());
      */
      // load every database per config.sys list configuration
      //var specifcsitedb = string.Empty;
      //try
      //{
      //    var n = ConfigurationManager.ConnectionStrings.Count;
      //    if (n > 0)
      //    {
      //        for (int i = 0; i < n; ++i)
      //        {
      //            var name = ConfigurationManager.ConnectionStrings[i].Name;
      //            var cstr = ConfigurationManager.ConnectionStrings[i].ConnectionString;
      //            foreach (KeyValuePair<string, string> p in pdict)
      //                cstr = utils.ReplaceEx(null, cstr, p.Key, p.Value);
      //            appdbs.set(name, cstr);
      //            log.Write(string.Concat(name, ": ", cstr, "<br/>"));
      //        }
      //        specifcsitedb = ConfigurationManager.ConnectionStrings["main"].ConnectionString;
      //        if(!string.IsNullOrEmpty(specifcsitedb))
      //            foreach (KeyValuePair<string, string> p in pdict)
      //                specifcsitedb = utils.ReplaceEx(null, specifcsitedb, p.Key, p.Value);
      //    }
      //}
      //catch (Exception e) { }

      return true;
    }
  }

  /**
  * this class is the link between the target aspx page with the framework, this 
  * is the class tha the developer has to use in order to integrate its site with 
  * the framework
  */
  public partial class link {
    public static config cfg = new config();
    private Type pagetype = null;
    private static Type thistype = null;
    private const BindingFlags FLAGS = BindingFlags.Instance |
                               BindingFlags.InvokeMethod |
                               BindingFlags.NonPublic;
    // site statistics
    private static ulong reqsrec = 0;            // received requests
    private static ulong reqsacc = 0;            // accepted requests
    private static ulong reqsrej = 0;            // rejected requests
    private static ulong reqserr = 0;            // requests with error
    private static ulong reqsexe = 0;            // executed requests

    // caches
    static public Dictionary<string, string> querycache =
       new Dictionary<string, string>();
    public static readonly object doclock = new object();
    static public Dictionary<string, string> errorcache =
       new Dictionary<string, string>();
    public static readonly object errlock = new object();
    static public Dictionary<string, mroJSON> rightscache =
       new Dictionary<string, mroJSON>();
    public static readonly object rightslock = new object();
    static public Dictionary<string, MethodInfo> funscache =
       new Dictionary<string, MethodInfo>();
    public static readonly object funslock = new object();
    public static Dictionary<string, Dictionary<int, ListResponse>> dataset =
       new Dictionary<string, Dictionary<int, ListResponse>>();
    // bl caches
    public static mro.BL.control_BL appbl = new BL.control_BL();

    public object component = null;          // aspx's page reference
    public HttpContext Context = null;
    public object Env = null;
    public HttpRequest Request = null;              // main request (input)
    public HttpResponse Response = null;            // main response (output)

    public client clie = null;
    public micro micr = null;

    // shortcut for performance
    public sdata data = null;
    public mroJSON header = null;
    public mroJSON basics = null;
    public mroJSON values = null;
    public mroJSON result = null;
    public mroJSON websvr = null;
    public response resp = null;
    public strbpool pools = null;
    public JSONpool poolj = null;
    public JSONpool poole = null;

    public bool byproxy = false;
    public string database = string.Empty; // OUT to link

    public link(object component, HttpContext Context, object Env) {
      this.component = component;
      this.Context = Context;
      this.Env = Env;
      this.Request = Context.Request;
      this.Response = Context.Response;
      pagetype = component.GetType();
    }

    public void init_environment() {
      querycache.Clear();
      errorcache.Clear();
      rightscache.Clear();
      funscache.Clear();

      thistype = this.GetType(); // never changes
      client.node = mem.join3(config.locaddr, ':', config.locport);

#if NETCOREAPP
      IWebHostEnvironment env = (IWebHostEnvironment)Env;
      var site = env.ApplicationName;
      var rmtaddr = Context.GetServerVariable("REMOTE_ADDR");
      var rmthost = Context.GetServerVariable("REMOTE_HOST");
      var rmtport = Context.GetServerVariable("REMOTE_PORT");
      var rmtuser = Context.GetServerVariable("REMOTE_USER");
      var rmtagnt = Context.GetServerVariable("HTTP_USER_AGENT");
#else
      var site = System.Web.Hosting.HostingEnvironment.SiteName;
      var rmtaddr = Request.ServerVariables["REMOTE_ADDR"];
      var rmthost = Request.ServerVariables["REMOTE_HOST"];
      var rmtport = Request.ServerVariables["REMOTE_PORT"];
      var rmtuser = Request.ServerVariables["REMOTE_USER"];
      var rmtagnt = Request.ServerVariables["HTTP_USER_AGENT"];
#endif

      var info = string.Format("Addr:{0}<br>Host:{1}<br>Port:{2}<br>User:{3}<br>Agent:{4}",
        rmtaddr, rmthost, rmtport, rmtuser, rmtagnt);
      mail.start_email(config.company, mem.join3(client.node, ' ', site), info, config.admingr);
      background();
    }
    private static void delete_temp_data() {
      int ndels = 0;
      foreach (var f in Directory.GetFiles(config.home_temp)) {
        FileInfo fi = new FileInfo(f);
        if (fi != null) { // could be deleted externally at same time
          TimeSpan ts = DateTime.Now - fi.LastAccessTime;
          if (ts.Minutes > 15) {
            try {
              File.Delete(f);
              if (++ndels == 10) break;
            } 
            catch (Exception e) { }
          }
        }
      }
    }
    private static void background() {
      ThreadPool.QueueUserWorkItem(new WaitCallback((Object stateInfo) => {
        if (!Directory.Exists(config.home_temp)) return;
        for (int cycle = 0; ; Thread.Sleep(1000), ++cycle) {
          try {
            //if (cycle == 6) GC.Collect();
            if (cycle == 20) delete_temp_data();
          } 
          catch (Exception e) { }
          if (cycle == 21) cycle = 0;
        }
      }));
    }

    public void go(ref mroJSON envbas,
                   ref mroJSON envval,
                   ref mroJSON envres,
                   ref mroJSON envnew) {
      Response.ContentType = "text/plain";
      Response.Headers.Add("Access-Control-Allow-Origin", "*");

      data = corefuncs.extract_data(Request, false);
      var ip = corefuncs.getmac(Context);
      clie = client.find_slot(Response, ip);
      micr = micro.find_slot(ip);

      try {
        if (clie.status == cpu.states.BUSY) ++reqsexe;

        // bound variables for performance
        header = clie.header;
        basics = envbas = clie.basics;
        values = envval = clie.values;
        result = envres = clie.result;
        envnew = clie.newval;
        websvr = clie.webservice;
        resp = clie.resp;
        pools = clie.pools;
        poolj = clie.poolj;
        poole = clie.poole;

        ++reqsrec;
        clie.step = 10;

        resp.start();

        if (!config.done) {
          lock (config.cfglock) {
            if (!config.done)
              config.done = cfg.init(Context, Env, Request, data);
          }
          if (config.done) init_environment();
        }

        if (data.isempty) { online(); return; }

        ++reqsacc;
        clie.step = 20;
        validate_data();
        handle_header();
        handle_basics();
        gen_database();
        read_request();
        pre_execution();
        execution();
        post_execution();
        post_process();
        finish_process();
      } 
      catch (Exception e) {
        corefuncs.check_client(clie, values, result);

        ++reqserr;
        try {
          err.manage_exception(clie, e, poole);
          err.get_error_desc(errlock, errorcache, config.dbcode, clie);
          process_error(result);
        } 
        finally {
          if (!byproxy) response.send(resp, defs.ZWBSEND);
        }
      } 
      finally {
        resp.end();

        // what ever result maybe we free this cpu
        clie.status = cpu.states.FREE;
        micr.status = cpu.states.FREE;
        --reqsexe;
      }
    }
    private void online() {
      Response.ContentType = "text/html";
      response.Write(Response, mem.join3("</br>\"Server\":\"online\",</br>\"Version\":",
         frm.FRMVER, "\"</br>"));
    }

    /**
     * This function forces the service to read again the configuration
     */
    private void reset_site() {
      config.done = false;
      config.locaddr = string.Empty;
    }

    private void process_error(mroJSON result) {
      if (!byproxy) response.send(resp, defs.ZWBSEND);
      response.send(resp, result);
    }

    #region special functions
    private void console() {
      //siteinfo.return_site_state(Request, Response, clie, cfg, client._rep,
      //reqsrec, reqsacc, reqsnpr, reqsrej, reqserr, reqsexe, reqspng);
    }
    /**
     * it's not only a ping function but also to run the last query in order to
     * maintain the DB alive and not it gets sleep, keeping the system fast enough 
     * also we must check if last query is a select cause there's no guarantee 
     * of malicious query to use CRUD or alter information with dangerous queries
     */
    private void ping() {
      micr.pingWatch.Stop();
      if (micr.pingWatch.Elapsed.TotalSeconds > 30) {
        appbl.execute_query_no_resp(clie, "select db_name(); ", config.dbcode);
        micr.pingWatch.Reset();
      }
      result.on("pinged");
      micr.pingWatch.Start();
    }
    private void reset_document_cache() {
      var id = values.get(defs.PDOCMNT);
      var idempty = id.Length == 0;
      var dummy = string.Empty;
      lock (doclock) {
        if (idempty) querycache.Clear();
        else
            if (querycache.TryGetValue(id, out dummy))
          querycache.Remove(id);
      }
    }
    private void reset_error_cache() {
      var id = values.get(defs.PCODEID);
      var idempty = id.Length == 0;
      lock (errlock) {
        if (idempty) errorcache.Clear();
        else errorcache.Remove(id);
      }
    }
    private void reset_rights_cache() {
      var id = values.get("cuserini");
      if (id.Length > 0)
        corefuncs.reset_rights_cache(rightslock, rightscache, id);
    }
    private void reset_funs_cache() {
      var id = values.get("cfunini");
      if (id.Length > 0)
        corefuncs.reset_funs_cache(funslock, funscache, id);
    }
    #endregion

    private void has_right() {
      // first we check the desire right, or in its default is a free function
      var right2check = string.Empty;
      if (websvr.get(defs.ZRIGHT1, ref right2check) == 0) return;

      // when it comes from a gate, it'll be marked, so if it is marked 
      // but has no right is because is has no rights, period
      if (clie.rights.notempty() && clie.rights.ison("pre")) {
        if (!clie.rights.ison(right2check))
          err.require(cme.ENORGTS, right2check);
        return;
      }

      // so we call the gate service to check if the request has rights
      corefuncs.look_rights(clie, config.dbcode, basics, rightslock, rightscache, clie.rights);
      if (clie.rights.notempty()) {
        if (!clie.rights.ison(right2check))
          err.require(cme.ENORGTS, right2check);
      }

      //            // any kind of error is a sign of not appropriate rights
      //            err.require(rights.has(defs.ZSERROR), rights.get(defs.ZSERROR));

      //            var rrights = new mroJSON();
      //            rights.get(defs.ZURGTSZ, ref rrights);
      //            if (!rrights.isempty()) {
      //                err.require(!rrights.is_active(ref right2check), defs.ENORGTS);
      //            }
    }
    public void set_log(string action, string key, string type) {
      var log = poolj.get(0);
      result.get(defs.ZZTOLOG, log);
      var nlogs = 0;
      if (log.notempty() && log.has(defs.ZZNLOGS))
        nlogs = log.getint(defs.ZZNLOGS);
      var logsn = nlogs.ToString();

      log.set(defs.ZTXTLOG + logsn, action);
      log.set(defs.ZKEYLOG + logsn, key);
      log.set(defs.ZTYPLOG + logsn, type);
      log.set(defs.ZZNLOGS, nlogs + 1);

      result.set(defs.ZZTOLOG, log);
    }
    private void giveback() {
      values.get(defs.GIVEBACK, result);
    }
    // this function either extract it from the cache or the database
    private void get_document() {
      response.send(resp, defs.ZFILERS,
         get_query_cache(values.get(defs.LIBRARY), values.get(defs.PDOCMNT)).sql);
    }
    private query get_query_cache(string lib, string id) {
      var type = datatype.QRY; // temporary
      var library = qryseslib2("lib_get_top", id, type);
      if (library.Length == 0) library = defs.ZKERNEL;

      if (id.Length == 0) err.require(cme.QUERY_MISSING);
      string p = null;
      lock (doclock) {
        if (querycache.TryGetValue(id, out p)) {
          if (p == null || p.Length == 0) querycache.Remove(id);
          else goto end;
        }
        p = qry3("document_get_data", library, id, type);
        if (p == null) err.require(true, cme.QUERY_NOT_REGISTERED, id);
        if (p.Length == 0) err.require(true, cme.QUERY_EMPTY, id);
        querycache.Add(id, p);
        goto end;
      }
    end:
      return new query(p);
    }
    /**
     * this function get the query and decide if it is save or embedded query
     */
    private query extract_main_query() {
      var lib = values.get(defs.LIBRARY);
      var qry = values.get(defs.PQRYCOD);
      if (qry.Length == 0) err.require(cme.QUERY_MISSING);

      query qrydtl = null;
      if (qry.Length == defs.VEMBEDDLEN && mem._tmemcmp(qry, defs.VEMBEDD)) {
        var t = pools.get(2);
        values.get(defs.PSQLTXT, t);
        t.Replace("lthan", "<");
        t.Replace("gthan", ">");
        var s = t.ToString();
        int a = s.IndexOf('&');
        if (a != -1) {
          if (s.IndexOf("&lt;", a) != -1) s = utils.ReplaceEx(clie.buffer, s, "&lt;", "<");
          if (s.IndexOf("&gt;", a) != -1) s = utils.ReplaceEx(clie.buffer, s, "&gt;", ">");
          if (s.IndexOf("&#43;", a) != -1) s = utils.ReplaceEx(clie.buffer, s, "&#43;", "+");
        }
        qrydtl = new query(s);
      }
      else {
        qrydtl = get_query_cache(lib, qry);
        if (qrydtl == null) err.require(cme.QUERY_NOT_REGISTERED, qry);
      }

      if (qrydtl.sql.Length == 0) err.require(cme.QUERY_EMPTY, qry);

      return qrydtl;
    }
    /**
     * this functions process, prepare and cleans the query to be runned
     */
    private query prepare_query() {
      var qrydtl = extract_main_query();

      var notempty = poolj.get(0);
      var donoem = values.get(defs.PNOEMPT, notempty) > 0;
      var defaults = poolj.get(1);
      var dodef = values.get(defs.PDEFAUS, defaults) > 0;
      var lstdata = poolj.get(2);
      var dolist = values.get(defs.ZLSTDAT, lstdata) > 0;

      var nvars = values.getint(defs.PNREPVS);
      if (nvars > 0) {
        if (nvars >= utils.repvars.Length)
          err.require(cme.E2MVARS, nvars); // prevent finger bugs

        for (var i = nvars - 1; i >= 0; --i) {
          var var2chg = utils.repvars[i];
          var value = values.get(var2chg);

          // on empty check if it has a default value
          if (dodef && value.Length == 0 && defaults.has(var2chg))
            value = defaults.get(var2chg);

          // check if the field is needed not empty 
          if (donoem && value.Length == 0 && notempty.has(var2chg))
            err.require(notempty.get(var2chg));

          //if (!lstdata.isempty() && value.Length >= 5 && 
          if (dolist && value.Length >= 5 &&
             value.IndexOf("list", 0, 4) == 0) {
            var lstid = lstdata.get("lstid");
            if (lstid == value.Substring(4, 1))
              value = lstdata.get(value.Substring(5));
          }

          var v2c = utils.repvars_ext[i];

          var s = qrydtl.sql;
          var idx = s.IndexOf(v2c);
          if (idx != -1) { // try to change string type 'repvarx'
            var v2val = mem.join3("'", value, "'");
            s = qrydtl.sql = utils.ReplaceEx(clie.buffer, s, v2c, v2val);
          }

          idx = s.IndexOf(var2chg);
          if (idx != -1) // try to change not string type repvarx
            qrydtl.sql = utils.ReplaceEx(clie.buffer, s, var2chg, value);
        }
      }

      var qi = values.getint(defs.ZQRYIDX, -1);
      if (qi != -1) {
        //var k = string.Concat("qryret",(qi++).ToString());
        //result.set(k, mro.corefuncs.clean_qry(qrydtl.sql));
        corefuncs.ses_save_query(this, appbl, qi++, qrydtl.sql,
            mem.join3(clie.trans, ".", data.func));
        clie.newval.set(defs.ZQRYIDX, qi);
      }

      return qrydtl;
    }

    #region execution
    /**
     * this function execute the function on some component, this component
     * could be whatever callable object, most of the time pages or libraries
     */
    private void execcomp() {
      clie.step = 60;

      // find out if we have the component on the mini cache
      var idx = -1;
      var ava = -1;
      var nws = micr.lastws.Length;
      for (int i = 0; i < nws; ++i) {
        var ws = micr.lastws[i];
        if (mem._tmemcmp(ws, data.func)) {
          idx = i;
          break;
        }
        else if (ava == -1 && ws.Length == 0) ava = i;
      }

      if (idx == -1) {
        idx = ava == -1 ? 0 : ava;
        micr.mi[idx] = pagetype.GetMethod(data.func, FLAGS);
        if (micr.mi[idx] == null) { // whitin link.cs (internal)
          clie.step = 65;
          micr.mi[idx] = thistype.GetMethod(data.func, FLAGS);
          if (micr.mi[idx] == null) {
            micr.lastws[idx] = string.Empty;
            micr.wsinternal[idx] = false;
            err.require(cme.FUN_NOT_EXIST, data.func);
          }
          micr.lastws[idx] = data.func;
          micr.wsinternal[idx] = true;
          clie.step = 66;
        }
        else {
          micr.lastws[idx] = data.func;
          micr.wsinternal[idx] = false;
          clie.step = 68;
        }
      }

      clie.step = 69;
      micr.mi[idx].Invoke(micr.wsinternal[idx] ? (object)this : component, null);
      clie.step = 70;
    }
    public void validate_data() {
      if (data.func.Length == 0) err.require(cme.FUN_MISSING);
    }
    public void handle_header() {
      if (data.header.Length == 0) return;
      header.set_value(data.header);

      clie.retresult = header.getbool(defs.ZRETRES);
      byproxy = header.getbool(defs.BYPROXY);
    }
    public void handle_basics() {
      if (data.basics.Length == 0) return;
      basics.set_value(data.basics);

      basics.get(defs.ZCOMPNY, ref clie.cmpy);
      basics.get(defs.ZTRNCOD, ref clie.trans);
      basics.get(defs.ZUSERID, ref clie.user);

      clie.zsesins = basics.getint(defs.ZSESINS, -1);
      clie.zsesmac = basics.getint(defs.ZSESMAC, -1);
      clie.zsescli = basics.getint(defs.ZSESCLI, -1);
      clie.zsesses = basics.getint(defs.ZSESSES, -1);
    }
    /**
     * if transaction webservice the we find its woriking database
     */
    private void gen_database() {
      database = clie.trans.Length == 0 ? string.Empty :
                 qryseslib1("doc_get_by_ses", clie.trans);

      err.require(clie.trans.Length != 0 && database.Length == 0,
         cme.DBNAME_MISSING);
    }
    public void read_request() {
      clie.step = 30;
      clie.macname.set(data.mac.Length > 0 ? data.mac : mem.join2(clie.ip, 'i'));

      if (data.parms.Length == 0) return;
      websvr.set_value(data.parms);

      clie.workonvalues = websvr.has(defs.ZVALUES);
      if (clie.workonvalues) {
        websvr.extract(defs.ZVALUES, values);
        websvr.append(values);
      }
    }
    private void pre_execution() {
      clie.status = cpu.states.EXEC;
      if (websvr.has(defs.ZURGTSZ))
        websvr.get(defs.ZURGTSZ, clie.rights);
    }
    private void execution() {
      clie.step = 50;

      // some result come from basics specially the part added by the server
      if (websvr.notempty()) {
        has_right();
        if (websvr.ison(defs.ZUSEBAS))
          corefuncs.repl_values(clie, poolj.get(0), values, basics, null);
      }

      if (!mem._tmemcmp(data.func, defs.ZNONFUN)) execcomp();

      if (websvr.has(defs.RETPRMS)) {
        var retprms = websvr.getobj(defs.RETPRMS);
        corefuncs.repl_values(clie, poolj.get(0), retprms, clie.values, null);
        result.append(retprms);
      }

      // first check that if the transaction does not have a log to be saved
      if (websvr.has(defs.ZZTOLOG))
        micr.haslog = websvr.get(defs.ZZTOLOG, clie.log) != 0;

      if (clie.log.isempty()) {
        // if transaction doesn't have log, check if the function generated one
        if (result.has(defs.ZZTOLOG)) {
          result.extract(defs.ZZTOLOG, clie.log);
          micr.haslog = true;
        }
      }

      if (websvr.has_val(defs.ZSTATUS)) {
        var okstatus = websvr.get(defs.ZSTATUS);
        langeng.translate(errlock, errorcache, config.dbcode, basics, ref okstatus);
        result.set(defs.ZSTATUS, okstatus);
      }

      if (micr.haslog) {
        //corefuncs.replace_values(clie, poolp.get(0), clie.log, values, null);
        // we add helpfull tracking data
        websvr.copyto(defs.ZCOMPNM, clie.log, defs.ZCOMPNM);
        websvr.copyto(defs.ZFUNNAM, clie.log, defs.ZFUNNAM);
      }
    }
    private void post_execution() {
      clie.step = 80;
      if (clie.newval.notempty()) {
        if (clie.newval.has(defs.PFLDTXT)) clie.newval.del(defs.PFLDTXT); // ***FIX***
        result.set(defs.ZNEWVAL, clie.newval);
      }
      if (!byproxy) response.send(resp, defs.ZWBSEND);
      response.send(resp, result);
    }
    private void post_process() {
      clie.status = cpu.states.BUSY;
      clie.step = 100;
      if (micr.haslog) corefuncs.save_log(config.dbcode, clie.log, basics);
    }
    private void finish_process() {
      clie.step = 120;
      clie.stopWatch.Stop();
      response.send(resp, defs.ZSITTIM, clie.stopWatch.Elapsed.ToString());
    }
    #endregion

    public string qryseslib0(string sp) {
      return sql.sqlscalar(config.dbcode, mem.join5("exec core.dbo.", sp, " ",
          sessinf(), ";"));
    }
    public string qryseslib1(string sp, string a) {
      return sql.sqlscalar(config.dbcode, string.Concat("exec core.dbo.", sp, " ",
          sessinf(), ",'", a, "';"));
    }
    public string qryseslib2(string sp, string a, string b) {
      return sql.sqlscalar(config.dbcode, string.Concat("exec core.dbo.", sp, " ",
          sessinf(), ",'", a, "','", b, "';"));
    }
    public string qryseslib3(string sp, string a, string b, string c) {
      return sql.sqlscalar(config.dbcode, string.Concat("exec core.dbo.", sp, " ",
          sessinf(), ",'", a, "','", b, "','", c, "';"));
    }
    public string qry3(string sp, string a, string b, string c) {
      return sql.sqlscalar(config.dbcode, string.Concat("exec core.dbo.", sp, " '",
          a, "','", b, "','", c, "';"));
    }
    public string get_appdb() {
      return values.has_val(defs.PDATBAS) ? values.get(defs.PDATBAS) : database;
    }
    public string sessinf() {
      return string.Join(",", new int[] {
            clie.zsesins, clie.zsesmac, clie.zsescli, clie.zsesses });
    }
  }
}
/*private void sys_command() {
    var qrydtl = prepare_query();
    // Create the ProcessInfo object
    System.Diagnostics.ProcessStartInfo psi = new System.Diagnostics.ProcessStartInfo("cmd.exe");
    psi.UseShellExecute = false;
    psi.RedirectStandardOutput = true;
    psi.RedirectStandardInput = true;
    psi.RedirectStandardError = true;
    psi.WorkingDirectory = config.home;// _temp;
    // Start the process
    System.Diagnostics.Process proc = System.Diagnostics.Process.Start(psi);
    // Open the batch file for reading
    //System.IO.StreamReader strm = System.IO.File.OpenText(strFilePath);
    // Attach the output for reading
    System.IO.StreamReader sOut = proc.StandardOutput;
    // Attach the in for writing
    System.IO.StreamWriter sIn = proc.StandardInput;
    // Write each line of the batch file to standard input
    //while (strm.Peek() != -1)
    //{
    //    sIn.WriteLine(strm.ReadLine());
    //}
    //strm.Close();
    // Exit CMD.EXE
    //string stEchoFmt = "# {0} run successfully. Exiting";
    //sIn.WriteLine(String.Format(stEchoFmt, strFilePath));
    sIn.WriteLine(qrydtl.sql);
    sIn.WriteLine("EXIT");
    // Close the process
    proc.Close();
    // Read the sOut to a string.
    string results = sOut.ReadToEnd().Trim();
    // Close the io Streams;
    sIn.Close();
    sOut.Close();
    // Write out the results.
    //string fmtStdOut = “<font face=courier size=0>{0}</font>”;
    //this.Response.Write(String.Format(fmtStdOut,results.Replace(System.Environment.NewLine, “<br>”)));
    result.set("consres", results);
}*/
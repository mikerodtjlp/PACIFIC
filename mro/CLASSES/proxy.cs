/**
 * Author: Miguel Rodriguez Ojeda
 * 
 * Purpose: The objective of this class is that of link between the client and 
 * framework, it routes the client's webservices to specific destination/node and 
 * carry on its values and returns the result from the specific sites to the 
 * client, it is called proxy because as the client is concerned he execute sites 
 * through this site as a proxy model (resembling reverse proxy)
 * 
 * note: this version proxy and dictionary/control must be on the same machine
 * 
 * significant dates
 * creation: september 21 2009 
 * version 1: november 16 2009  (basic functionallity)
 * version 2: june 7 2010       (clear distinction between proxy and core)
 * version 3: march 14 2011     (use of caches and include support for 
 *                              log, rights, and most basic core features)
 * version 4: december 5 2011   (increasse stability and high capacity 
 *                              processing)
 */

#if NETCOREAPP
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Hosting;
#endif

using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Net.NetworkInformation;
using System.IO;
using System.Reflection;
using System.Web;
using System.Threading;
using System.Data.SqlClient;
using System.Diagnostics;

namespace mro {
  public sealed class nodeentry {
    public nodeentry(string address, string server, string awakeurl,
                      int port,
                      string exename, /*string exeparams,*/
                      int type, int priority, bool checksession) {
      this.address = address;
      this.server = server;
      this.awakeurl = awakeurl;
      this.port = port;
      this.type = type;
      this.exename = exename;
      //this.exeparams = exeparams;
      this.priority = priority;
      this.checksession = checksession;
    }
    public string address = null;
    public string server = null;
    public string awakeurl = null;
    public StringBuilder resawake = new StringBuilder();
    public int port = 0;
    public int type = 0;
    public int priority = 0;
    public string exename = null;
    //public string exeparams = null;
    public ulong execwebs = 0;
    public ulong execfuns = 0;
    public ulong fails = 0;
    public ulong pings = 0;
    public double time = 0.0;
    public bool checksession = false;
  }
  public sealed class node {
    public List<nodeentry> sites = null;
    public int seq = 0;
  }

  /**
  * the proxy is the gateway for the client and the framework, webservices
  * must be executed through the proxy.aspx which in turn uses this class
   */
  public partial class proxy {
    public proxy(HttpContext context, object Env) {
      this.Context = context;
      this.Env = Env;
      this.Request = context.Request;
      this.Response = context.Response;
    }
    public HttpContext Context = null;
    public object Env = null;
    public HttpRequest Request = null;
    public HttpResponse Response = null;

    // process tracking
    public client clie = null;
    public process proc = null;

    private static string[] scripts = new string[1];

    // site statistics
    private static ulong reqsrec = 0;
    private static ulong reqsacc = 0;
    private static ulong reqsrej = 0;
    private static ulong reqserr = 0;
    private static ulong reqsnpr = 0;
    private static ulong reqsexe = 0;
    private static ulong awakesthread = 0;

    public static readonly object addrlock = new object();
    public static readonly object funslock = new object();
    public static readonly object rightslock = new object();
    public static readonly object notiflock = new object();
    public static readonly object desclock = new object();
    public static readonly object errlock = new object();

    public static Dictionary<string, string> desccache =
       new Dictionary<string, string>();
    public static Dictionary<string, string> errorcache =
       new Dictionary<string, string>();
    public static Dictionary<string, node> addresses =
       new Dictionary<string, node>();
    public static Dictionary<string, funcdata> functions =
       new Dictionary<string, funcdata>();
    public static Dictionary<string, mroJSON> rights =
       new Dictionary<string, mroJSON>();
    public static Dictionary<string, bool> notifs =
       new Dictionary<string, bool>();

    public static bool addressesloaded = false;
    public static mroJSON netsites = new mroJSON();
    public static string atl_svr = string.Empty;
    public static int gatprt = 0;
    public static string dbcode = string.Empty;
    public static string proxysvr = string.Empty;
    public static string proxyprt = string.Empty;
    public static string proxylnk = string.Empty;
    public static string company = string.Empty;
    public static nodeentry proxyitem = null;
    public static string home = string.Empty;
    public static string home_temp = string.Empty;
    public static string gatdisp = string.Empty;
    public static int s_nsites = -1;
    public static int savenv = 0;

    // shortcut for performance
    public sdata data = null;
    public mroJSON header = null;
    public mroJSON basics = null;
    public mroJSON values = null;
    public mroJSON result = null;
    public mroJSON websvr = null;
    public response resp = null;
    public strbpool pools = null;
    public strbpool poolx = null;
    public JSONpool poolj = null;
    public JSONpool poole = null;

    public mroJSON request = new mroJSON();

    private void init_environment() {
      addresses.Clear();
      functions.Clear();

#if NETCOREAPP
      proxysvr = Context.GetServerVariable("SERVER_NAME");
      proxyprt = Context.GetServerVariable("SERVER_PORT");
      IWebHostEnvironment env = (IWebHostEnvironment)Env;
      home = env.WebRootPath;
      var site = env.ApplicationName;

      var rmtaddr = Context.GetServerVariable("REMOTE_ADDR");
      var rmthost = Context.GetServerVariable("REMOTE_HOST");
      var rmtport = Context.GetServerVariable("REMOTE_PORT");
      var rmtuser = Context.GetServerVariable("REMOTE_USER");
      var rmtagnt = Context.GetServerVariable("HTTP_USER_AGENT");
#else
      proxysvr = Request.ServerVariables["SERVER_NAME"];
      proxyprt = Request.ServerVariables["SERVER_PORT"];
      var site = System.Web.Hosting.HostingEnvironment.SiteName;
      home = Request.PhysicalApplicationPath;

      var rmtaddr = Request.ServerVariables["REMOTE_ADDR"];
      var rmthost = Request.ServerVariables["REMOTE_HOST"];
      var rmtport = Request.ServerVariables["REMOTE_PORT"];
      var rmtuser = Request.ServerVariables["REMOTE_USER"];
      var rmtagnt = Request.ServerVariables["HTTP_USER_AGENT"];
#endif

      proxylnk = mem.join4("proxysvr=", proxysvr, "&proxyprt=", proxyprt);

      if (home.Length > 0 && home[home.Length - 1] == '\\')
        home = home.Substring(0, home.Length - 1);
      home_temp = mem.join2(home, "\\temp");

      client.node = mem.join3(proxysvr, ':', proxyprt);

      var admingr = string.Empty;
      load_addresses(ref admingr);
      if (addressesloaded) {
        var info = string.Format("Addr:{0}<br>Host:{1}<br>Port:{2}<br>User:{3}<br>Agent:{4}",
          rmtaddr, rmthost, rmtport, rmtuser, rmtagnt);
        mail.start_email(company, mem.join3(client.node, ' ', site), info, admingr);
        background();
      }
    }

    private static void load_addresses(ref string admingr) {
      if (!addressesloaded) {

        var addrs = new mroJSON();
        setup.load_params(home, "\\cfgs\\link.json", ref addrs);
        var dbinfo = addrs.get(addrs.get("linkdb"));

        addrs.set_value(sql.sqlscalar(dbinfo, mem.join3("exec dbo.get_mro_addresses '", "", "';")));
        addrs.get("zcoredb", ref dbcode);
        addrs.get("admingr", ref admingr);
        addrs.get("cmpyname", ref company);

        using (var connpwd = new SqlConnection(dbinfo)){
          connpwd.Open();
          using (var cmd = new SqlCommand("exec dbo.get_mro_pass;", connpwd)) {
            var r = cmd.ExecuteReader();
            err.require(!r.HasRows, cme.CANNOT_GET_PASSWORDS);
            for (;r.Read();) {
              dbcode = utils.ReplaceEx(null, dbcode, r.GetString(0), r.GetString(1));
            }
          }
        }

        var domainJSON = sql.sqlscalar(dbcode/*dbinfo*/, "exec dbo.get_mro_nodes;");
        corefuncs.update_addresses(null, addrs, ref domainJSON);

        var servicesJSON = sql.sqlscalar(dbcode/*dbinfo*/, "exec dbo.get_mro_kernel;");
        corefuncs.update_addresses(null, addrs, ref servicesJSON);

        var svrJSON = new mroJSON(servicesJSON);
        var domJSON = svrJSON.getobj("domain0");

        netsites = new mroJSON();
        var parms = new mroJSON(domainJSON);
        parms.get("netsites", netsites);
        node nodelist = null;

        try {
          var site = new mroJSON();
          var nsites = netsites.getint("nsites");

          for (var j = 0; j < nsites; ++j) {
            if (netsites.get(utils.sites[j], site) == 0) continue;

            var name = site.get(defs.P_NAME_);
            var addr = site.get(defs.PADDRSS);
            var checkses = site.getbool("checkses");
            var dumplog = site.getbool("dumplog");
            var type = site.getint("model");
            var serv = site.get("service");

            var domsvr = domJSON.getobj(serv);
            var srvc = domsvr.get("service");
            var exen = domsvr.get("file");
            if (exen.Length > 0 && exen[0] == '\\')
              exen = exen.Substring(1, exen.Length - 1);
            var exep = domsvr.getobj("params").get_mro();
            var prior = domsvr.getint("priority");

            var i = addr.IndexOf(':');
            var server = addr.Substring(0, i);
            var port = utils.IntParseFast(addr.Substring(i + 1));

            var sitcall = name.ToLower();
            var isproxy = sitcall.Length >= 5 && sitcall == "proxy";

            nodeentry entry = new nodeentry(addr, server,
                                          type == 0 && sitcall.Length > 0 && !isproxy ?
                                          string.Concat(dhtml.http, addr,
                                          "/core.aspx?fun=ping&proxysvr=",
                                          proxysvr, "&proxyprt=", proxyprt) :
                                          string.Empty,
                                          port, exen,
                                          //exep, 
                                          type, prior, checkses);

            if (addresses.TryGetValue(sitcall, out nodelist))
              nodelist.sites.Add(entry);
            else {
              nodelist = new node();
              nodelist.sites = new List<nodeentry>();
              nodelist.sites.Add(entry);
              if (isproxy) proxyitem = entry;
              addresses.Add(sitcall, nodelist);
            }
          }

          // get dictionary location
          parms.get("servers", site);
          atl_svr = site.get(defs.ZGATSVR);
          gatprt = site.getint(defs.ZGATPRT);

          var t3 = new StringBuilder();
          gatdisp = mem.join3(
                CParameters.gen_pair(defs.ZBYGATE, "1", t3),
                CParameters.gen_pair(defs.ZGATSVR, atl_svr, t3),
                CParameters.gen_pair(defs.ZGATPRT, gatprt.ToString(), t3));
        } 
        catch (Exception ex) {
        } 
        addressesloaded = addresses.Count > 0;
      }
    }

    /**
     * function forms and marks a whatever action to notify the kernel the
     * that the client is active
     */
    private void notify_action() {
      if (clie.zsesins == -1) return;
      var action = new CParameters();
      action.set(defs.ZSESINS, clie.zsesins);
      action.set(defs.ZSESMAC, clie.zsesmac);
      action.set(defs.ZSESCLI, clie.zsescli);
      action.set(defs.ZSESSES, clie.zsesses);
      var sesid = action.get_data();
      lock (notiflock) {
        if (notifs.ContainsKey(sesid)) notifs[sesid] = true;
        else notifs.Add(sesid, true);
      }
    }
    /**
     * this function find any function belonging to some specific transaction 
     * and mark it as to be reload whe access the next time
     */
    private void reload_codebehind() {
      // we must invalidate our last function for tricky bugs
      proc.fundata.clear();
      proc.lastfun.Length = 0;
      var doc = clie.trans;
      var doclen = doc.Length;
      lock (funslock) {
        foreach (var f in functions) {
          if (f.Key.Length <= doclen) continue; // to short to be a candidate
          if (string.CompareOrdinal(f.Key, 0, doc, 0, doclen) == 0) {
            f.Value.reload = true;
            ++f.Value.reldfns;
          }
        }
      }
    }

    /**
     * loads a specific function, it looks first if it is not an internal 
     * reload function, or is the function as the last one, or already exist 
     * on the cache and finally on the dictionary, all of this is for 
     * perfomance reasons.
     */
    private void load_function() {
      // check for a very special function onreload
      var tr = clie.trans;
      var ev = proc.eventname;
      if (ev.Length == defs.ZONRELDLEN && mem._tmemcmp(ev, defs.ZONRELD)) {
        reload_codebehind();
        return;
      }

      if (tr.Length == 0) err.require(cme.PAGE_MISSING);
      if (ev.Length == 0) err.require(cme.FUN_MISSING);

      // form key to find, example SPO1oneter (kind of full namespace name)
      var t = pools.getclean(0);
      t.Append(tr);
      t.Append(ev);
      if (t.Length > 63) err.require(cme.FUN_NAME_2_LONG, t.ToString());

      // temporary validation has to be synchornized both
      var lstfun = proc.lastfun;
      var fundata = proc.fundata;
      if (fundata.isempty()) lstfun.Length = 0;
      if (lstfun.Length == 0) fundata.clear();

      // first of all we look for it on the last function(happens quite often)
      if (t.Length != lstfun.Length || !mem._tmemcmp(lstfun, t)) {
        var f2find = t.ToString();
        fundata.clear();
        lstfun.Length = 0;

        var exists = false;
        var lookindict = false;
        var byforce = false;
        // second of all we lookfor it on our local cache
        try {
          funcdata fdata = null;
          lock (funslock) {
            if (functions.TryGetValue(f2find, out fdata)) {
              if (fdata == null) {
                lookindict = true;
                functions.Remove(f2find);
              }
              else {
                if (fdata.info != null && fdata.info.isempty()) {
                  lookindict = true;
                  exists = true;
                }
                else {
                  byforce = fdata.reload;
                  if (byforce) {
                    lookindict = true;
                    exists = true;
                  }
                  else {
                    fundata.set_value(fdata.info);
                    ++fdata.accesses;
                  }
                }
              }
            }
            else lookindict = true;

            if (lookindict) {
              t.Length = 0;
              atl.atlservice(t, basics, funs.GET_FINAL_FUN,
                                  defs.PFN2FND, ev.ToString(),
                                  defs.PDOCMNT, tr,
                                  defs.ZTYPRED, byforce ? "force" : "");

              var ct = poolj.get(0);
              var t1 = pools.get(2);
              mrosocket.atlantic(clie.buffer, proc.bbytes, t1,
                                atl_svr, gatprt, t, ct);

              // special code treatment, right now we deal with: '+'
              //int n = t1.Length;
              //for (int i = n - 1; i >= 0; --i)
              //   if (t1[i] == '+') { t1.Replace("+", "%2B"); break; }
              //ct.set_value(t1);

              err.check(ct);

              // now we have the function code
              ct.get(defs.ZFINFUN, fundata);
              if (fundata.isempty())
                err.require(cme.FUN_NOT_LOADED, f2find);
              var nfuns = fundata.getint(defs.ZZNFUNS);

              // none/simple funs dont have to be fixed
              if (nfuns > 0) {
                var s = fundata.get_json();
                if (s.IndexOf(defs.PSQLTXT) != -1) {
                  s = utils.DeSerializeString(s, t);
                  fundata.set_value(s);
                }
              }

              if (exists) { // when we have the real function we store in the cache for latter use
                fdata.info.set_value(fundata);
                fdata.reload = false;
                ++fdata.accesses;
              }
              else functions.Add(f2find, new funcdata(fundata, false));
            }
          }
          // now we have our last function
          lstfun.Append(f2find);
        } 
        finally { }
      }
    }

    public sealed class RequestState {
      public HttpWebRequest request = null;
      public RequestState() {
        request = null;
      }
    }
    private void finishwebrequest(IAsyncResult asynchronousResult) {
      const int LENPACKAGE = 8192;

      RequestState myRequestState = (RequestState)asynchronousResult.AsyncState;
      HttpWebRequest myHttpWebRequest = myRequestState.request;
      HttpWebResponse resp = null;
      Stream streamResponse = null;
      var cr = proc.noderesp;
      try {
        resp = (HttpWebResponse)myHttpWebRequest.EndGetResponse(asynchronousResult);
        streamResponse = resp.GetResponseStream();
        using (StreamReader streamRead = new StreamReader(streamResponse)) {
          char[] c = clie.buffer;
          int index = 0;
          int page = 0;
          int safetybreak = LENPACKAGE * 16;
          int packs = 0;
          while (true) {
            if (++packs == safetybreak) break;
            int nc = streamRead.Read(c, index, c.Length - index);
            if (nc < 1) break;
            index += nc;
            if (index >= LENPACKAGE) {
              cr.Append(c, 0, index);
              index = 0;
              ++page;
            }
          }
          if (index > 0)
            cr.Append(c, 0, index);
          if (packs == safetybreak) err.require(cme.ECNBRKN);
        }
      } 
      finally {
        if (streamResponse != null) {
          streamResponse.Close();
          streamResponse.Dispose();
        }
        if (resp != null) {
          resp.Close();
        }
      }
    }
    /**
     * this function is who really execute the final function on some aspx 
     * and on some website
     * 
     * NOTE !!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
     * WE CHANGE THIS function to call mrosocket.WEBSERVICE 
     * !!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
     */
    private void backend() {
      clie.step = 60;

      Exception ee = null;
      var page = proc.defcomp ? proc.comp : proc.scmp;

      // bulding the real destination website
      var ws = pools.getclean(1);
      ws.Append(dhtml.http);
      ws.Append(proc.nodeentry.address);
      ws.Append('/');
      ws.Append(page);
      var websrv = ws.ToString();

      //byte[] buffer = mrosocket.utf8.GetBytes(Uri.EscapeUriString(clie.post.ToString()));
      var postdata = proc.post.ToString();//.Replace("+", "%2B").Replace(" ", "%20");
      byte[] buffer = mrosocket.utf8.GetBytes(postdata);

      try {
        // creating the request
        HttpWebRequest myRequest = (HttpWebRequest)WebRequest.Create(websrv);
        myRequest.Method = WebRequestMethods.Http.Post;
        myRequest.ContentType = "application/x-www-form-urlencoded";
        myRequest.ContentLength = buffer.Length;
        myRequest.Timeout = 1000 * (20 * 60);
        myRequest.ReadWriteTimeout = 1000 * (20 * 60);

proc.noderesp.Length = 0;
result.clear();

        // sending the request
        using (Stream postStream = myRequest.GetRequestStream()) {
          postStream.Write(buffer, 0, buffer.Length);
        }

        // proc.noderesp.Length = 0;

        clie.step = 65;
        // collecting the response

        //System.Net.HttpWebResponse httpResponse = (System.Net.HttpWebResponse)(myRequest.GetResponse());
        //System.IO.StreamReader SReader = new StreamReader(httpResponse.GetResponseStream());
        //t.Append(SReader.ReadToEnd());
        //httpResponse.Close();

        RequestState myRequestState = new RequestState();
        myRequestState.request = myRequest;
        proc.asyncres = myRequest.BeginGetResponse(
                        new AsyncCallback(finishwebrequest),
                        myRequestState);

        // testing: is obvious that the webservice call to one site is not 
        // instantenious some we can do things for this particular call 
        // before receiving the response
        // ************************ pending ******************************
        notify_action();
        //result.clear();
        clie.status = cpu.states.PROXY;
        clie.step = 70;

        if (!proc.asyncres.AsyncWaitHandle.WaitOne(1000 * (20 * 60), true))
          err.require(cme.SITE_TIMEOUT);
      } 
      catch (Exception ex) {
        ee = ex;
      }

      // here we take the backend's result as our result
if(proc.noderesp.Length > 0) 
  result.set_value(proc.noderesp);

      clie.status = cpu.states.BUSY;

      if (ee != null) {
        var er = pools.getclean(2);
        er.Append(ee.Message);
        er.Append(" ws:");
        er.Append(ws);
        er.Append('&');
        er.Append(proc.post);
        err.require(cme.SITE_NOT_AVAILABLE, er.ToString());
      }
    }

    private void pre_execution() {
      // get the function's execution data
      if (request.get(defs.ZEVENTN, proc.eventname) > 0)
        load_function();

      // get the variables's values, only (but not necesarly) the gui 
      // clients handle values, so direct clients like external devices 
      // dont need this kind of overhead
      clie.workonvalues = request.has(defs.ZVALUES);
      if (clie.workonvalues) request.extract(defs.ZVALUES, values);

      // check if this request needs to save the result and this node is 
      // allowed to do it
      int sr = proc.saveresult = proc.fundata.getint(defs.ZSAVSTA);
      if (sr > 2) err.require(cme.WRONG_SAVE_RESULT_PARAM, sr);
      resp.saveresult = sr;
    }
    /**
     * standar execution, execute function by function from the event's 
     * execution tree
     */
    private void execution() {
      clie.step = 50;
      var nfuns = proc.fundata.getint(defs.ZZNFUNS);
      for (int i = 0; i < nfuns; ++i) {
        if (proc.fundata.get(utils.fnums[i], websvr) == 0) continue;

        corefuncs.proc_params(clie);

        // apply the real values to the log if any
        if (websvr.extract(defs.ZZTOLOG, clie.log) > 0) {
          corefuncs.repl_values(clie, poolj.get(0), clie.log, clie.values, null);
          websvr.set(defs.ZZTOLOG, clie.log);
        }

        // process if any a specific site/node 
        var node = proc.site;
        if (websvr.has(defs.ZWEBSIT)) {
          var modlen = websvr.get(defs.ZWEBSIT, proc.ssit);
          if (modlen != proc.site.Length || !proc.site.Equals(proc.ssit)) node = proc.ssit;
        }

        // looking for the real destination site/node
        get_node(node.ToString());

        // process if any a specific component/page
        proc.defcomp = true;
        if (websvr.has(defs.ZCOMPNM)) {
          int len = websvr.get(defs.ZCOMPNM, proc.scmp);
          proc.defcomp = false;
          proc.loc_exec = len >= 5 && mem._tmemcmp(proc.scmp, len, pages.PROXY);
        }

        data.func = websvr.get(defs.ZFUNNAM);

        if (proc.loc_exec) {
          loc_pre_execution();
          loc_execution();
        }
        else {
          proc.nodeentry.execfuns++;

          var post = proc.post;
          post.set(proxylnk);
          post.Append("&fun=");
          post.Append(data.func);
          post.Append("&mac=");
          post.Append(clie.macname);
          post.Append("&hdr=");
          post.Append(header.get_json());
          post.Append("&bas=");
          post.Append(basics.get_json());
          post.Append("&prm=");
          if (proc.retqry) websvr.set(defs.ZQRYIDX, proc.qi);

          var tmp = poolj.getclean(0); // need optimize use cache/pool
          tmp.set(defs.ZVALUES, websvr);
          tmp.set(defs.ZURGTSZ, clie.rights);
          post.Append(HttpUtility.UrlEncode(tmp.get_json()));

          backend();
        }

        // redirect downfiles to the central proxy
        if (result.has(defs.ZDWNFSV)) redirect_downloads();

        if (clie.workonvalues) {
          if (result.has_val(defs.ZNEWVAL)) {
            if (result.extract(defs.ZNEWVAL, clie.newval) > 0) {
              if (proc.retqry)
                proc.qi = clie.newval.extractint(defs.ZQRYIDX);
              values.replace_from(clie.newval);
              result.append(clie.newval);
            }
          }
        }
        // responde to client
        response.send(resp, result);

        // stop executing on fisrt error
        if (result.has_val(defs.ZSERROR)) break;
      }
      // if no function was supplied, at least we add the reponse end message
      if (nfuns == 0) response.send(resp, result);
    }
    /**
     * should manage all this that are side effects of the execution
     * and anything at all of the execution, like logs, cleanhousing, etc..
     */
    private void post_execution() {
      clie.step = 80;

      // we must return the original trans code and function event if any
      if (clie.trans.Length > 0) response.send(resp, defs.ZTRNCOD, clie.trans);
      if (proc.eventname.Length > 0) response.send(resp, defs.ZEVENTN, proc.eventname);

      if (proc.retqry) response.send(resp, "nqryret", proc.qi.tostr());
    }
    /**
     * the client cannot load the direct resource(workbook/report/txt/etc...)
     * directly from the target site, the client only communicates through 
     * the proxy, so the proxy gets the resource and redirects the link to it
     */
    private void redirect_downloads() {
      var source = string.Empty;
      if (result.get(defs.ZDWNFSV, ref source) > 0) {
        var folder = result.get(defs.ZDWNFPA);
        var file = result.get(defs.ZDWNFFL);
        var item = folder.Length == 0 ? file : mem.join3(folder, "/", file);
        var from = mem.join4(dhtml.http, source, "/", item);
        var to = mem.join3(home_temp, "\\", file);

        try {
          // get the original site
          WebClient webClient = new WebClient();
          webClient.DownloadFile(from, to);
        } 
        catch (Exception e) {
          throw new Exception(cme.DWNLOD_COUDNT_REDIR);
        }

        // redirect the source
        result.set(defs.ZDWNFSV, client.node);
        result.set(defs.ZDWNFPA, defs.TMPFOLDER);
        result.set(defs.ZDWNFFL, file);
      }
    }

    private void save_result_to_db(string q) {
      ThreadPool.QueueUserWorkItem(new WaitCallback((Object stateInfo) => {
        sql.sqlnores(dbcode, q, false);
      }));
    }

    /** 
     * this function save the last state of the client, which is the same as 
     * its last result, it only works for the log service
     */
    private void save_result() {
      var hispos = basics.getint(defs.ZHISPOS, -1);
      if (hispos == -1) return; // not have a session

      var q = string.Empty;
      var id = string.Concat(clie.zsesins, ",", clie.zsesmac, ",",
                              clie.zsescli, ",", clie.zsesses, ",",
                              hispos.tostr());

      if (result.has(defs.ZERRORI) || result.has(defs.ZSERROR))
        goto error; // error result not taken

      var vals = string.Empty;
      if (proc.isatlevent) {
        var vls = poolj.get(0);
        values.get(defs.ZVALUES, vls);
        if (vls.nkeys() > 0)
          vals = utils.ReplaceEx(clie.buffer, vls.get_mro(), "'", "''");
      }
      else {
        if (values.nkeys() > 0)
          vals = utils.ReplaceEx(clie.buffer, values.get_mro(), "'", "''");
      }

      var data = string.Empty;
      if (proc.saveresult == 1 || proc.saveresult == 2) {
        var flushed = resp.flushed.ToString();
        // because it wanted as json is a json
        var f = pools.getclean(0);
        var r = pools.getclean(1);
        r.Append('{');
        r.Append(resp.flushed.ToString());
        r.Append('}');
        utils.json2mro(r, f);
        flushed = f.ToString();
        data = mem.join2(result.get_mro(), flushed);
        data = utils.ReplaceEx(clie.buffer, data, "'", "''");
      }

      if (data.IndexOf(defs.ZERRORI) != -1 || data.IndexOf(defs.ZSERROR) != -1)
        goto error; // error result not taken

      var size = vals.Length + data.Length;
      if (size == 0) goto error; // nothing to save 
      if (size > (1024 * 32)) goto error; // to big to be saved

      if (proc.saveresult == 1 || proc.saveresult == 2) { // full
        q = string.Concat("exec dbo.set_last_result ",
            id, ",'", clie.cmpy, "','", clie.trans, "','", vals, "','", data, "';");
      }
      else goto error;

      goto save;
    error:
      q = mem.join3("exec dbo.del_last_result ", id, ";");
    save:
      save_result_to_db(q);
    }

    private void handle_header() {
      if (header.notempty()) {
        resp.retresult = clie.retresult = header.getbool(defs.ZRETRES, true);
        proc.retqry = header.getbool(defs.ZRETQRY);
      }
      header.on(defs.BYPROXY);
    }
    private void handle_basics() {
      if (basics.notempty()) {
        basics.get(defs.ZCOMPNY, ref clie.cmpy);
        basics.get(defs.ZTRNCOD, ref clie.trans);
        basics.get(defs.ZUSERID, ref clie.user);

        clie.zsesins = basics.getint(defs.ZSESINS, -1);
        clie.zsesmac = basics.getint(defs.ZSESMAC, -1);
        clie.zsescli = basics.getint(defs.ZSESCLI, -1);
        clie.zsesses = basics.getint(defs.ZSESSES, -1);
      }
    }
    /**
     * this function inspect the url data and decide type of execution
     */
    private exectype read_request() {
      clie.step = 30;
      clie.macname.set(data.mac.Length > 0 ? data.mac : mem.join2(clie.ip, 'i'));

      if (data.isempty) return exectype.EMPTY; // startup?

      if (string.CompareOrdinal(data.function, "upload") == 0) {
        var folder = data.folder;
        if (folder.Length == 0) folder = "uploadedfiles";
        var cx = corefuncs.save_it(Context, folder, data.newname);
#if NETCOREAPP
        Response.WriteAsync(cx);
#else
        Response.Write(cx);
#endif
        return exectype.FILE;
      }

      // if have no node to execute
      proc.site.set(data.site);
      if (proc.site.Length == 0 && data.func.Length > 0) return exectype.LOCAL;

      // if have no page/componenet to execute
      proc.comp.set(data.page);
      if (proc.comp.Length == 0 && data.func.Length > 0) return exectype.LOCAL;

      proc.direct = data.func.Length > 0;

      ++reqsacc;
      return exectype.NODE;
    }

    /**
     * extract the main webservice components, header, basics and parameters
     */
    private void form_request() {
      clie.step = 20;
      if (data.header.Length > 0) header.set_value(data.header);
      if (data.basics.Length > 0) basics.set_value(data.basics);
      if (data.parms.Length > 0) request.set_value(data.parms);
    }
    private void look_rights() {
      corefuncs.look_rights(clie, dbcode, basics, rightslock, rights, clie.rights);
      clie.rights.on("pre");
    }
    private void apply_functions() {
      get_node(proc.site.ToString());
      look_rights();

      if (proc.direct) {
        proc.nodeentry.execfuns++;

        var post = proc.post;
        post.set(proxylnk);
        post.Append("&fun=");
        post.Append(data.func);
        post.Append("&mac=");
        post.Append(clie.macname);
        post.Append("&hdr=");
        post.Append(header.get_json());
        post.Append("&bas=");
        post.Append(basics.get_json());
        post.Append("&prm=");
        post.Append(HttpUtility.UrlEncode(request.get_json()));

        backend();

        // redirect downfiles to the central proxy
        if (result.has(defs.ZDWNFSV)) redirect_downloads();

        response.send(resp, result);
      }
      else {
        pre_execution();
        execution();
        post_execution();
      }
    }
    private void post_process() {
      clie.step = 100;
      if (clie.log.notempty())
        corefuncs.save_log(dbcode, clie.log, basics);
      if (proc.saveresult > 0 && clie.retresult)
        save_result();
    }
    private void finish_process() {
      clie.step = 120;
      clie.stopWatch.Stop();
      response.send(resp, defs.ZPRXTIM, clie.stopWatch.Elapsed.ToString());
      // end of execution, at this point execution was done forthe client
      response.send(resp, defs.ZWBSEND);
    }
    /**
     * to dispatch to the final webservice we need to know where (its address)
     * with the site we can inspect our dictionary and the the real address
     */
    private void get_node(string sitcall) {
      if (proc.lastsitelen != sitcall.Length || !mem._tmemcmp(proc.lastsite, sitcall)) {
        if (!addresses.TryGetValue(sitcall, out proc.nodes)) {
          proc.lastsite = string.Empty;
          proc.lastsitelen = 0;
          err.require(cme.SITE_NOT_CONFIGURED, sitcall);
        }
        proc.lastsite = sitcall;
        proc.lastsitelen = sitcall.Length;
        proc.nodeentry = proc.nodes.sites[proc.nodes.seq];
      }
      if (proc.nodeentry.checksession) check_api_key(basics.get(defs.ZAPIKEY));
      proc.nodeentry.execfuns++;
    }

    /**
     * entry point of execution 
     */
    public void go() {
      exectype exect = exectype.NOTDEF;
      Response.ContentType = "text/plain";
      Response.Headers.Add("Access-Control-Allow-Origin", "*");

      data = corefuncs.extract_data(Request, true);
      var ip = corefuncs.getmac(Context);
      clie = client.find_slot(Response, ip);
      proc = process.find_slot(ip);

      try {
        if (clie.status == cpu.states.BUSY) ++reqsexe;

        // bound variables for performance
        header = clie.header;
        basics = clie.basics;
        values = clie.values;
        result = clie.result;
        websvr = clie.webservice;
        resp = clie.resp;
        pools = clie.pools;
        poolj = clie.poolj;
        poole = clie.poole;

        ++reqsrec;
        clie.step = 10;

        resp.start();

        if (!addressesloaded)
          lock (addrlock)
            if (!addressesloaded)
              init_environment();

        exect = read_request();

        if (exect == exectype.EMPTY) { online(); return; }
        else if (exect == exectype.FILE) return;

        form_request();
        handle_header();
        handle_basics();

        if (exect == exectype.NODE) 
          apply_functions();
        else if (exect == exectype.LOCAL) 
          loc_apply_functions();

        post_process();
        finish_process();
      } 
      catch (Exception e) {
        corefuncs.check_client(clie, values, result);

        err.manage_exception(clie, e, poole);
        err.get_error_desc(errlock, errorcache, dbcode, clie);

        clie.stopWatch.Stop();
        response.send(resp, defs.ZPRXTIM, clie.stopWatch.Elapsed.ToString());
        response.send(resp, defs.ZWBSEND);
        response.send(resp, result);
        if (proc.nodeentry != null) ++proc.nodeentry.fails;

        // convinient code: FIX sequence for any BUG related to sequence of nodes
        if (proc.nodes != null && proc.nodes.seq >= proc.nodes.sites.Count)
          proc.nodes.seq = 0;
      } 
      finally {
        resp.end();

        var nodes = proc.nodes;
        if (exect == exectype.LOCAL && nodes == null) {
          if (proxyitem != null) ++proxyitem.execwebs;
        }
        else
            if (nodes != null) {
          ++proc.nodeentry.execwebs;
          if (++nodes.seq >= nodes.sites.Count) nodes.seq = 0;
        }

        // what ever result may would be we free this cpu
        clie.status = cpu.states.FREE;
        proc.status = cpu.states.FREE;
        --reqsexe;
      }
    }
    private void online() {
      Response.ContentType = "text/html";
      response.Write(Response, mem.join3("</br>\"Server\":\"online\",</br>\"Version\":",
         frm.FRMVER, "\"</br>"));
    }
#region local_execution
    private void loc_execution() {
      var nfuns = 0;
      int.TryParse(data.func, out nfuns);
      if (nfuns == 0) nfuns = 1;

      // we convert the values as the execution data
      var tmpvalues = poolj.get(1);
      values.clone_to(tmpvalues);
      values.clear();

      var pivot = poolj.get(2);
      for (int i = 0; i < nfuns; ++i) {
        // extracting the function/webservice data
        tmpvalues.get(utils.funs[i], ref data.func);
        var p = utils.prms[i];
        if (tmpvalues.has_val(p)) {
          tmpvalues.get(utils.prms[i], pivot);
          values.append(pivot);  // should append or set value?
        }
        loc_backend(data.func);
      }
    }
    private void loc_pre_execution() {
      request.extract(defs.ZVALUES, values);
    }
    private void loc_post_execution() {
      response.send(resp, result);
    }
    public void loc_apply_functions() {
      loc_pre_execution();
      loc_execution();
      loc_post_execution();
    }
#endregion

    /*protected void upload_file(){
        var folder = values.get("to");//"uploadedfiles";
        var cx = corefuncs.save_it(context.Request.Files, folder);
        context.Response.Write(cx);
    }*/
    /**
     * Direct calll to kernel c++ functions, very low leveel
     */
    private void atlcall() {
      notify_action();

      var sitcall = values.get(defs.ZSITCAL);
//////////// PATH //////////////////
if(sitcall=="atl") sitcall="gat";
////////////////////////////////////
      get_node(sitcall);
      var node = proc.nodeentry;

      var t2 = pools.getclean(1);
      var t0 = pools.getclean(2);
      atl.atlservice(t0, null, basics, values.get(defs.ZFUNCAL), values);
      mrosocket.atlantic(clie.buffer, proc.bbytes, t2, node.server,
                        node.port, t0, result);
      if (result.has(defs.ZFILERS)) utils.handle_html(result);
      err.check(result);
      ++savenv;
    }
    /**
     * Direct calll to kernel c++ old events, eventually will move to here
     */
    private void atlevent() {
      notify_action();

      var sitcall = values.get(defs.ZSITCAL);
//////////// PATH //////////////////
if(sitcall=="atl") sitcall="gat";
////////////////////////////////////
      get_node(sitcall);
      var node = proc.nodeentry;

      var t2 = pools.getclean(1);
      t2.Append(gatdisp);

      var t = pools.getclean(2);
      atl.atlevent(t, null, basics, values, t2);
      mrosocket.atlantic(clie.buffer, proc.bbytes, t2, node.server,
                        node.port, t, result);
      if (result.has(defs.ZFILERS)) utils.handle_html(result);
      err.check(result);
      ++savenv;
    }
  }
}
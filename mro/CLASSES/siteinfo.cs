#if NETCOREAPP
using Microsoft.AspNetCore.Http;
#endif
using System;
using System.IO;
using System.Reflection;
using System.Collections.Generic;
using System.Text;
using System.Web;

namespace mro {
  public class siteinfo {
    static private DateTime started = DateTime.Now;
    static private string iplocal = utils.getlocalip();
    static private Version vers = Assembly.GetExecutingAssembly().GetName().Version; //Version v = new Version();

    /*      static private void respond(HttpResponse Response, HttpRequest Request) {
             var appname = string.Empty;
             if (string.IsNullOrEmpty(appname)) {
                appname = Request.ServerVariables["APPL_PHYSICAL_PATH"];
                appname = appname.Substring(0, appname.Length - 1);
                appname = appname.Substring(appname.LastIndexOf('\\') + 1);
             }

             string strpath = System.Reflection.Assembly.GetExecutingAssembly().Location;
             var fi = new FileInfo(strpath);

             //Assembly assem = Assembly.GetExecutingAssembly();
             //Version vers = assem.GetName().Version;

             Response.Write(string.Format(
                                 "<title>{0}</title>" +
                                 "<body style=\"background-color:#000000;\">" +
                                 "<font style=\"font-size: 12px; color: #00ff00; font-family:consolas;\">" +
                                 "site name: {1}</br></br>" +
                                 "server: {2}</br>" +
                                 "port: {3}</br>" +
                                 "path: {4}</br>" +
                                 "remote_addr: {5}</br>" +
                                 "remote_host: {6}</br>" +
                                 "http_user_agent: {7}</br></br>" +
                                 "company: {8}</br></br>" +
                                 "site version: {9}</br>" +
                                 "framework version: {10}</br>" +
                                 "gui compatible version: {11}</br>" +
                                 "compilation date: {12}</br>" +
                                 "assembly version: {13}</br></br>" +
                                 "time online: {14}</br>" +
                                 "</body>",
                                 appname,
                                 appname,
                                 iplocal,
                                 Request.ServerVariables["SERVER_PORT"],
                                 Request.ServerVariables["PATH_TRANSLATED"],
                                 Request.ServerVariables[32],
                                 Request.UserHostAddress,
                                 Request.ServerVariables["http_user_agent"],

                                 "Coromuel",
                                 "1.022",
                                  frm.FRMVER, frm.GUIVER,
                                 fi.LastWriteTime, vers.ToString(),
                                 (DateTime.Now - started)));
          }
          private static string blanks19 = new string(utils.work(19));
          private static string blanks25 = new string(utils.work(25));
          public static void return_proxy_state(HttpRequest Request, HttpResponse Response,
                                                  Dictionary<string, List<client>> cpus,
                                                  //Dictionary<string, nodes> addresses,
                                                  Dictionary<string, Dictionary<string, node>> domains,
                                                  ulong awakes, bool showfull,
                                                  Dictionary<string, funcdata> functions,
                                                  Dictionary<string, CParameters> rights,
                                                  string gatsvr, int gatprt,
                                                  ulong reqsrec,
                                                  ulong reqsacc,
                                                  ulong reqsnpr,
                                                  ulong reqsrej,
                                                  ulong reqserr,
                                                  ulong reqsexe) {
             var br = dhtml.br;
             respond(Response, Request);
             Response.Write("</br>proxy build:1.03</br></br>");
             Response.Write("</br>gate(atlantic): address:");
             Response.Write(gatsvr);
             Response.Write(", port:");
             Response.Write(gatprt.ToString());
             Response.Write("</br></br>");

             int naddrs = 0;
             int ngen = GC.MaxGeneration;
             int ngcs = 0;
             for (int i = 0; i < ngen; ++i)
                ngcs += GC.CollectionCount(i);

             Response.Write("dom ");
             Response.Write("site");
             Response.Write(blanks19);
             Response.Write("address");
             Response.Write(blanks25);
             Response.Write("stats(ex-ef-fa-ti-pn)");
             Response.Write(br);
             foreach (var addresses in domains.Values) {
                foreach (var addr in addresses) {
                   foreach (var val in addr.Value.sites) {
                      // site
                      Response.Write(addr.Key);
                      char[] b0 = utils.work(27 - addr.Key.Length);
                      Response.Write(b0, 0, b0.Length);

                      // address
                      Response.Write("<a href=\"");
                      Response.Write(dhtml.http);
                      Response.Write(val.address);
                      Response.Write('"');
                      Response.Write('>');
                      Response.Write(val.address);
                      Response.Write("/core.aspx");
                      Response.Write("</a>");
                      char[] b1 = utils.work(22 - val.address.Length);
                      Response.Write(b1, 0, b1.Length);

                      // statistics
                      Response.Write(string.Format("{0}-{1}-{2}-{3}-{4}",
                          val.execwebs, val.execfuns, val.fails, val.time, val.pings));

                      if (showfull) {
                         Response.Write(" -> ");
                         Response.Write(val.awakeurl);
                         Response.Write(" -> ");
                         Response.Write(val.resawake.ToString());
                      }
                      Response.Write(br);
                      naddrs++;
                   }
                }
             }

             Response.Write("</br>cpus: ");
             Response.Write(cpus.Count.ToString());
             Response.Write(", addresses: ");
             Response.Write(naddrs.ToString());
             Response.Write(", awake threads: ");
             Response.Write(awakes.ToString());
             Response.Write(", functions: ");
             Response.Write(functions.Count.ToString());
             Response.Write(", rights: ");
             Response.Write(rights.Count.ToString());
             Response.Write("</br></br>");

             Response.Write("requests: received: ");
             Response.Write(reqsrec);
             Response.Write(", accepted: ");
             Response.Write(reqsacc);
             Response.Write(", not processed: ");
             Response.Write(reqsnpr);
             Response.Write(", rejected: ");
             Response.Write(reqsrej);
             Response.Write(", with error: ");
             Response.Write(reqserr);
             Response.Write(", executing: ");
             Response.Write(reqsexe);
             Response.Write("</br>");
             Response.Write("cpu cycles: ");
             Response.Write(Environment.TickCount & Int32.MaxValue);
             Response.Write(", gc memory: ");
             Response.Write(ngcs);
             Response.Write(':');
             Response.Write(GC.GetTotalMemory(false));
             Response.Write("</br></br>");
             Response.Write("functions: ");
             Response.Write("</br>");

             Response.Write("</br></br><table style=\"background-color:#000000;font-size: 12px; color: #00ff00; font-family:consolas;\">");
             Response.Write("<tr><th align=\"left\";>transaction</th>");
             Response.Write("<th align=\"left\";>accesses</th><th align=\"left\";>reloads</th><th align=\"left\";>time</th></tr></br>");
             foreach (KeyValuePair<string, funcdata> f in functions) {
                Response.Write("<tr valign=\"top\">");
                Response.Write("<td>");
                Response.Write(f.Key);
                Response.Write("</td>");

                var v = f.Value;

                Response.Write("<td>");
                Response.Write(v.accesses.ToString());
                Response.Write("</td>");

                Response.Write("<td>");
                Response.Write(v.reldfns.ToString());
                Response.Write("</td>");

                Response.Write("<td>");
                Response.Write(v.time.ToString());
                Response.Write("</td>");

                Response.Write("</tr>");
             }
             Response.Write("</table>");

             Response.Write("</br></br><table style=\"background-color:#000000;font-size: 12px; color: #00ff00; font-family:consolas;\">");
             Response.Write("<tr><th align=\"left\";>client</th>");
             Response.Write("<th align=\"left\";>status</th><th align=\"left\";>data</th><th align=\"left\";>error</th><th align=\"left\";>time</th><th align=\"left\";>step</th></tr></br>");
             foreach (var cp in cpus) {
                var cpvalue = cp.Value;
                foreach (var c in cpvalue) {
                   var d = c.data;

                   Response.Write("<tr valign=\"top\"><td>");
                   Response.Write(cp.Key);
                   Response.Write("</td><td>");
                   Response.Write(c.status.ToString());
                   Response.Write("</td><td>");
                   Response.Write("hdr:");
                   Response.Write(d.header != null ? d.header.ToString() : string.Empty);
                   Response.Write(br);
                   Response.Write("bas:");
                   Response.Write(d.basics != null ? d.basics.ToString() : string.Empty);
                   Response.Write(br);
                   Response.Write("fun:");
                   Response.Write(d.func != null ? d.func.ToString() : string.Empty);
                   Response.Write(br);
                   Response.Write("fnr:");
                   //Response.Write(c._fundata.get_data());
                   Response.Write(br);
                   Response.Write("prm:");
                   Response.Write(d.parms != null ? d.parms.ToString() : string.Empty);
                   Response.Write(br);
                   Response.Write("val:");
                   Response.Write(c.values.get_json());
                   Response.Write("</td><td>");
                   Response.Write(c.result.get(defs.ZSERROR));
                   Response.Write("</td><td>");
                   Response.Write(c.stopWatch.Elapsed.ToString());
                   Response.Write("</td><td>");
                   Response.Write(c.step.ToString());
                   Response.Write("</td></tr>");
                }
             }
             Response.Write("</table>");
          }

          public static void return_site_state(  HttpRequest Request, 
                                                 HttpResponse Response,
                                                 client clie,
                                                 config cfg,
                                                 Dictionary<string, List<client>> cpus,
                                                 ulong reqsrec,
                                                 ulong reqsacc,
                                                 ulong reqsnpr,
                                                 ulong reqsrej,
                                                 ulong reqserr,
                                                 ulong reqsexe,
                                                 ulong reqspng) {
             var br = dhtml.br;
             var t = clie.pools.getclean(0); 
             var cdb = clie.pools.getclean(1); 

             char[] blk5 = utils.work(5);
             if (cfg != null) {
                t.Append(br);
                //if (cfg.coredb != null)
                //{
                //    cdb.Append(br);
                //    var pairs = cfg.coredb.get_dict();
                //    foreach (var pair in pairs)
                //    {
                //        cdb.Append(blk5, 0, 5 * 5);
                //        cdb.Append(pair.Key);
                //        cdb.Append(':');
                //        cdb.Append(pair.Value);
                //        cdb.Append(br);
                //    }
                //}
             }

             respond(Response, Request);

             Response.Write(t);
             Response.Write(cdb);

             int ngen = GC.MaxGeneration;
             int ngcs = 0;
             for (int i = 0; i < ngen; ++i)
                ngcs += GC.CollectionCount(i);

             Response.Write("</br>requests: received: ");
             Response.Write(reqsrec);
             Response.Write(", accepted: ");
             Response.Write(reqsacc);
             Response.Write(", not processed : ");
             Response.Write(reqsnpr);
             Response.Write(", rejected: ");
             Response.Write(reqsrej);
             Response.Write(", with error: ");
             Response.Write(reqserr);
             Response.Write("</br>");
             Response.Write("executing: ");
             Response.Write(reqsexe);
             Response.Write(", pings: ");
             Response.Write(reqspng);
             Response.Write(", cpu cycles: ");
             Response.Write(Environment.TickCount & Int32.MaxValue);
             Response.Write(", gc memory: ");
             Response.Write(ngcs);
             Response.Write(':');
             Response.Write(GC.GetTotalMemory(false));

             Response.Write("</br></br><table style=\"background-color:#000000;font-size: 12px; color: #00ff00; font-family:consolas;\">");
             Response.Write("<tr><th align=\"left\";>client</th>");
             Response.Write("<th align=\"left\";>status</th><th align=\"left\";>data</th><th align=\"left\";>error</th><th align=\"left\";>time</th><th align=\"left\";>step</th></tr></br>");
             foreach (var cp in cpus) {
                var cpvalue = cp.Value;
                foreach (var c in cpvalue) {
                   var d = c.data;

                   Response.Write("<tr valign=\"top\"><td>");
                   Response.Write(cp.Key);
                   Response.Write("</td><td>");
                   Response.Write(c.status.ToString());
                   Response.Write("</td><td>");
                   Response.Write("hdr:");
                   Response.Write(d.header != null ? d.header.ToString() : string.Empty);
                   Response.Write(br);
                   Response.Write("bas:");
                   Response.Write(d.basics != null ? d.basics.ToString() : string.Empty);
                   Response.Write(br);
                   Response.Write("fun:");
                   Response.Write(d.func != null ? d.func.ToString() : string.Empty);
                   Response.Write(br);
                   Response.Write("fnr:");
                   //Response.Write(c._fundata.get_data());
                   Response.Write(br);
                   Response.Write("prm:");
                   Response.Write(d.parms != null ? d.parms.ToString() : string.Empty);
                   Response.Write(br);
                   Response.Write("val:");
                   Response.Write(c.values.get_json());
                   Response.Write("</td><td>");
                   Response.Write(c.result.get(defs.ZSERROR));
                   Response.Write("</td><td>");
                   Response.Write(c.stopWatch.Elapsed.ToString());
                   Response.Write("</td><td>");
                   Response.Write(c.step.ToString());
                   Response.Write("</td></tr>");
                }
             }
             Response.Write("</table>");
          }*/
  }
}

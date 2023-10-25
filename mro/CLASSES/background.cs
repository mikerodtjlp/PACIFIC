/**
 * Author: Miguel Rodriguez Ojeda
 * 
 * Purpose : Do background process for the proxy service as login, emailing, keep 
 * alive nodes, notify clients, etc... everthing related to services ran on threads
 * 
 */

#if NETCOREAPP
using Microsoft.AspNetCore.Http;
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
  public partial class proxy {
    /**
     * main background function for dispatching background activities
     **/
    private static void background() {
      ThreadPool.QueueUserWorkItem(new WaitCallback((Object stateInfo) => {
        var buffer = new char[8192 + (8192 / 2)];
        var bbytes = new byte[8192 + (8192 / 2)];

        int nsites = addresses.Count;
        List<nodeentry> urls = new List<nodeentry>();
        foreach (var addr in addresses) {
          var k = addr.Key;
          foreach (var val in addr.Value.sites)
            if (val.address.Length > 0 &&
                !(k.Length >= 5 && mem._tmemcmp(k, pages.PROXY)))
              urls.Add(val);
        }

        var j0 = new mroJSON();
        var j1 = new mroJSON();
        var p1 = new mroJSON();
        var t1 = new StringBuilder();
        var t2 = new StringBuilder();

        // first time wakeup sites which is an essential action
        wakeup_sites(t1, t2, buffer, bbytes, urls, p1, home);

        for (int cycle = 9; ; Thread.Sleep(1000), ++cycle) {
          try {
            // 5,10,15,20,25
            if (cycle % 5 == 0) notifyuse(t1, t2, buffer, bbytes, p1);
            if (cycle == 3) process_sessions(t1, t2, buffer, bbytes, p1);
            // 7,14,21
            if (cycle % 7 == 0) apply_emails(buffer, company, j0, j1, t1);
            //if (cycle == 6) GC.Collect();
            if (cycle == 9) wakeup_sites(t1, t2, buffer, bbytes, urls, p1, home);
            if (cycle == 16) process_sessions(t1, t2, buffer, bbytes, p1);
            if (cycle == 22) fish_emails();
            if (cycle == 23) delete_temp_data();
          } 
          catch (Exception e) { }
          if (cycle == 25) cycle = 0;
        }
      }));
    }
    /**
     * keep the nodes/sites/kernels alive, pinging them and wake up them
     **/
    private static void wakeup_sites(StringBuilder t,
                                     StringBuilder t3,
                                     char[] buffer,
                                     byte[] bbytes,
                                     List<nodeentry> urls,
                                     mroJSON r,
                                     string home) {
      foreach (var val in urls) {
        DateTime sta = DateTime.Now;
        val.resawake.Length = 0;
        val.resawake.Append("running");

        if (val.type == 1) {  // ATL KERNEL service
          try {
            ++awakesthread;

            var usr = new mroJSON(defs.ZUSERID, "mromain");

            t.Length = 0;
            atl.atlservice(t, frm.HDRNOTRETRES, usr.get_mro(),
                            funs.FLUSH_LOGS, defs.ZTYPCOM, typecomp.SYS);
            t.Append(gatdisp);
            mrosocket.atlantic(buffer, bbytes, t3, val.server, val.port, t, r);
            err.check(r);

            DateTime end = DateTime.Now;
            val.resawake.Append(r.get_mro());
            ++val.pings;
          } 
          catch (Exception e) {
            ProcessStartInfo psi = new ProcessStartInfo();
            psi.RedirectStandardOutput = true;
            psi.UseShellExecute = false;
            psi.CreateNoWindow = true;
            psi.FileName = mem.join3(home, "\\", val.exename);
            psi.WorkingDirectory = home;
            //psi.Arguments = val.exeparams;

            string logfile = mem.join2(home, "\\proxyLOG.html");
            using (var log = new StreamWriter(logfile, true)) {
              try {
                log.Write(mem.join5("launching: ", psi.FileName, ", args: ", psi.Arguments, "<br/>"));
                var p = Process.Start(psi);
                switch (val.priority) {
                  case 1: p.PriorityClass = ProcessPriorityClass.AboveNormal; break;
                  case 2: p.PriorityClass = ProcessPriorityClass.High; break;
                }
                log.Write(mem.join3("running: ", psi.FileName, "<br/>"));
              } 
              catch (Exception f) {
                val.resawake.Length = 0;
                val.resawake.Append(f.Message);
                log.Write(mem.join5("failed: ", psi.FileName, ", err: ", f.Message, "<br/>"));
              }
            }

            val.resawake.Length = 0;
            val.resawake.Append(e.Message);
          }
        }
        else {  // IIS/nodejs/aws/azure/etc... Sites
          try {
            ++awakesthread;
            mrosocket.webservice(buffer, method.GET, val.awakeurl, val.resawake);
            DateTime end = DateTime.Now;
            r.set_value(val.resawake);
            if (r.ison("pinged")) {
              ++val.pings;
              DateTime sit = DateTime.Parse(r.get("time"));
              int diff = (end - sit).Milliseconds;
            }
            else { /*TBD*/ }
          } 
          catch (Exception e) {
            val.resawake.Length = 0;
            val.resawake.Append(e.Message);
          }
        }
      }
    }
    /**
     * we can execution the calling inside the lock, otherwise it'll be
     * locked for too many time
     */
    private static void notifyuse(StringBuilder t1,
                                  StringBuilder t2,
                                  char[] buffer,
                                  byte[] bbytes,
                                  mroJSON dummy) {
      string[] founds = null;
      int howmany = 0;
      try {
        lock (notiflock) {
          foreach (KeyValuePair<string, bool> ntf in notifs) {
            if (ntf.Value) {
              if (founds == null) founds = new string[4];
              founds[howmany++] = ntf.Key;
              if (howmany == 4) break;
            }
          }
        }
        if (howmany == 0) return;
        for (int i = 0; i < howmany; ++i) {
          var found = founds[i];
          lock (notiflock) { notifs[found] = false; } // mark as proccessed
          t2.Length = 0;
          t2.Append(found);
          t1.Length = 0;
          atl.atlservice(t1, frm.HDRNOTRETRES, t2.ToString(), funs.NOTIFY_USE);
          mrosocket.atlantic(buffer, bbytes, t2, atl_svr, gatprt, t1, dummy);
          Thread.Sleep(128);
        }
      } 
      catch (Exception e) { }
    }

    /**
     * calls kernel to process the current sessions
     **/
    private static void process_sessions(StringBuilder t1,
                                           StringBuilder t2,
                                           char[] buffer,
                                           byte[] bbytes,
                                           mroJSON dummy) {
      try {
        var fun = savenv > 0 ? funs.SAVE_SESSIONS : funs.RESET_GHOST_SES;
        if (savenv > 0) savenv = 0;
        t1.Length = 0;
        atl.atlservice(t1, frm.HDRNOTRETRES, string.Empty, fun);
        mrosocket.atlantic(buffer, bbytes, t2, atl_svr, gatprt, t1, dummy);
      } 
      catch (Exception e) { }
    }
    private static void fish_emails() {
      try {
        mail.ReadEmail();
      } 
      catch (Exception e) { }
    }
    /**
     * extract emails from a specific FRAMEWORK user and according
     * to certain rules try to process actions from the system
     */
    private static void apply_emails(char[] buffer,
                                     string company,
                                     mroJSON data,
                                     mroJSON site,
                                     StringBuilder s_rs) {
      if (s_nsites == -1) s_nsites = netsites.getint("nsites", -1);
      try {
        mro.BO.email em = null;
        using (var dal = mro.DAL.control_DAL.instance(dbcode)) {
          em = dal.email_get_last();
        }
        if (em != null) {
          if (em.status == 10) {
            var info = em.body;
            var a = info.IndexOf("[PCF-ACTION:");
            if (a != -1) {
              a += 12;
              var b = info.IndexOf("];", a);
              if (b != -1 && b < (a + 128)) {
                var d = info.Substring(a, (b - a));
                data.set_value(d);
                var component = data.get("module");
                var page = data.get("page");
                var func = data.get("func");
                var parms = data.get("params");
                for (int j = 0; j < s_nsites; ++j) {
                  site.set_value(netsites.get(utils.sites[j]));
                  if (string.CompareOrdinal(site.get("model"), "1") == 0)
                    continue; // ATL sites are not handle here
                  var stnm = site.get("name");
                  if (stnm != component) continue;

                  string address = site.get("address");
                  string addaddr = string.Empty;
                  int addport = 0;
                  utils.separte_address(address, ref addaddr, ref addport);
                  if (addport == utils.IntParseFast(proxyprt))
                    page = "proxy.aspx";

                  string u = string.Concat(dhtml.http, address, "/", page,
                                           "?fun=", func,
                                           "&prm={\"", defs.ZVALUES,
                                           "\":{", parms, "}}");
                  try {
                    mrosocket.webservice(buffer, method.GET, u, s_rs);
                  } 
                  catch (Exception e) { }
                }
              }
              using (var dal = mro.DAL.control_DAL.instance(dbcode)) {
                dal.email_update(em.id, 11);
              }
            }
          }
          if (em.status == 0) {
            mail.SendEmail(company, em.to, em.cc, em.subject,
                                 em.body, em.attach);

            // at this moment whether the email was sent or not we mark as sent
            using (var dal = mro.DAL.control_DAL.instance(dbcode)) {
              dal.email_update(em.id, 1);
            }
          }
        }
      } 
      catch (Exception e) { }
    }
    private static void delete_temp_data() {
      int ndels = 0;
      foreach (var f in Directory.GetFiles(home_temp)) {
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
  }
}
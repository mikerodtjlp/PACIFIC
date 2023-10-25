/**
 * Author: Miguel Rodriguez Ojeda
 * 
 * Purpose : This class implements the basic structure for clients, the clients have an
 * entry on the pool of clients being this class the main objet for tha pool
 * 
 */

#if NETCOREAPP
using Microsoft.AspNetCore.Http;
#endif

using System;
using System.Collections.Generic;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Net.Mail;
//using System.Net.Mime;
using System.Reflection;
using System.IO;
using System.Data.OleDb;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Data;
using System.Threading;
using System.Web;

namespace mro {
  public sealed class client {
    public client(string ip) {
      status = cpu.states.BUSY;
      this.ip = ip;
    }
    public int step;

    public string ip = null;
    public StringBuilder macname = new StringBuilder();
    public string cmpy = string.Empty;
    public string user = string.Empty;
    public string trans = string.Empty;
    public static string node = string.Empty;

    public mroJSON header = new mroJSON();
    public mroJSON basics = new mroJSON();
    public mroJSON result = new mroJSON();
    public mroJSON values = new mroJSON();
    public mroJSON webservice = new mroJSON();
    public mroJSON log = new mroJSON();
    public mroJSON newval = new mroJSON();
    public mroJSON rights = new mroJSON();

    public char[] buffer = new char[8192 + (8192 / 2)];

    public response resp = new response();
    public cpu.states status = cpu.states.FREE;

    // helpers ----------------------------------
    public strbpool pools = new strbpool();
    public JSONpool poolj = new JSONpool(4);
    public JSONpool poole = new JSONpool(6);
    public memhelper mhelp = new memhelper(); // OUT to link

    public Stopwatch stopWatch = new Stopwatch();

    public int zsesins = -1;
    public int zsesmac = -1;
    public int zsescli = -1;
    public int zsesses = -1;
    public int unit = -1;
    public bool retresult = true;

    public bool hasretprms = false;
    public bool workonvalues = false;

    public void init(HttpResponse resp) {
      step = 0;

      header.clear();
      basics.clear();
      result.clear();
      values.clear();
      newval.clear();
      rights.clear();
      log.clear();
      webservice.clear();
      zsesins = -1;
      zsesmac = -1;
      zsescli = -1;
      zsesses = -1;
      unit = -1;
      cmpy = string.Empty;
      trans = string.Empty;
      user = string.Empty;
      hasretprms = false;
      workonvalues = false;

      // only for lists
      this.resp.init(resp);

      retresult = true;

      stopWatch.Reset();
      stopWatch.Start();
    }
    public static client find_slot(HttpResponse Response,
                                     string ip) {
      List<client> lcpus = null;
      client entry = null;
      var unit = -1;

      lock (_replock) {
        if (_rep.TryGetValue(ip, out lcpus) == false) {
          lcpus = new List<client>();
          lcpus.Add(entry = new client(ip));
          _rep.Add(string.Copy(ip), lcpus);
          unit = 0;
          goto found;
        }

        var n = lcpus.Count;
        for (int i = n - 1; i >= 0; --i) {
          entry = lcpus[i];
          if (entry.status == cpu.states.FREE) {
            entry.status = cpu.states.BUSY;
            unit = i;
            goto found;
          }
        }
        lcpus.Add(entry = new client(ip));
      }
    found:
      entry.init(Response);
      entry.unit = unit;
      return entry;
    }
    public static Dictionary<string, List<client>> _rep =
       new Dictionary<string, List<client>>();
    public static readonly object _replock = new object();
  }
}
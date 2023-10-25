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
  public sealed class micro {
    public micro(string ip) {
      status = cpu.states.BUSY;
      this.ip = ip;

      for (int i = 0; i < 8; ++i) {
        mi[i] = null;
        lastws[i] = string.Empty;
        wsinternal[i] = false;
      }
    }

    public int step;
    public string ip = null;
    public cpu.states status = cpu.states.FREE;

    public MethodInfo[] mi = new MethodInfo[8];
    public string[] lastws = new string[8];
    public bool[] wsinternal = new bool[8];

    public ListResponse lstres = new ListResponse();
    public Stopwatch pingWatch = new Stopwatch();

    public bool haslog = false;

    public void init() {
      haslog = false; 
    }

  public static micro find_slot(string ip) {
      List<micro> lcpus = null;
      micro entry = null;

      lock (_replock) {
        if (_rep.TryGetValue(ip, out lcpus) == false) {
          lcpus = new List<micro>();
          lcpus.Add(entry = new micro(ip));
          _rep.Add(string.Copy(ip), lcpus);
          goto found;
        }

        int n = lcpus.Count;
        for (int i = n - 1; i >= 0; --i) {
          entry = lcpus[i];
          if (entry.status == cpu.states.FREE) {
            entry.status = cpu.states.BUSY;
            goto found;
          }
        }
        lcpus.Add(entry = new micro(ip));
      }
    found:
      entry.init();
      return entry;
    }

    public static Dictionary<string, List<micro>> _rep =
       new Dictionary<string, List<micro>>();
    public static readonly object _replock = new object();
  }
}
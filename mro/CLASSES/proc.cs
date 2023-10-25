/**
 * Author: Miguel Rodriguez Ojeda
 * 
 * Purpose : This class implements the basic structure for procs/nodes, the proxy
 * gets an entry on the pool of procs (being this class) for trigger the real 
 * webservice to the specific target, Net Site, NodeJs, Aws, Azure, Reporting etc..
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
  public sealed class process {
    public process(string ip) {
      status = cpu.states.BUSY;
      this.ip = ip;
    }

    public int step;
    public string ip = null;
    public cpu.states status = cpu.states.FREE;

    public byte[] bbytes = new byte[8192 + (8192 / 2)];
    public IAsyncResult asyncres = null;
    public bool loc_exec = false;
    public string server = null;

    // component variables
    public bool defcomp = true;
    public StringBuilder comp = new StringBuilder();
    public StringBuilder scmp = new StringBuilder();

    // node variables
    public StringBuilder site = new StringBuilder();
    public StringBuilder ssit = new StringBuilder();

    // address variables
    public node nodes = null;
    public nodeentry nodeentry = null;
    public string lastsite = string.Empty; // no init cuase maintane value
    public int lastsitelen = 0;

    public bool isatlevent = false;
    public bool direct = false;               // means one direct function instead of event
    public int saveresult = 0;                            // does save result

    // debuggin
    public bool retqry = false;                           // does return query executed
    public int qi = 0;                                    // query ID

    // node execution
    public mroJSON fundata = new mroJSON();               // fun data to execute
    public StringBuilder lastfun = new StringBuilder();   // last fun data executed
    public StringBuilder eventname = new StringBuilder(); // event name
    public StringBuilder noderesp = new StringBuilder();  // node response
    public StringBuilder post = new StringBuilder();      // node request post

    public void init() {
      asyncres = null;
      loc_exec = false;
      defcomp = true;
      direct = false;
      isatlevent = false;
      qi = 0;

      eventname.Length = 0;

      saveresult = 0;
      post.Length = 0;

      comp.Length = 0;
      scmp.Length = 0;

      site.Length = 0;
      ssit.Length = 0;
    }
    public static process find_slot(string ip) {
      List<process> lcpus = null;
      process entry = null;

      lock (_replock) {
        if (_rep.TryGetValue(ip, out lcpus) == false) {
          lcpus = new List<process>();
          lcpus.Add(entry = new process(ip));
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
        lcpus.Add(entry = new process(ip));
      }
    found:
      entry.init();
      return entry;
    }

    public static Dictionary<string, List<process>> _rep =
       new Dictionary<string, List<process>>();
    public static readonly object _replock = new object();
  }
}
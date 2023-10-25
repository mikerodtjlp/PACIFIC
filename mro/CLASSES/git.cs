#if NETCOREAPP
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Hosting;
#endif

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Web;

namespace mro {
  public class git {
    public git(HttpContext context, object Env) {
      this.context = context;
      Request = context.Request;
      Response = context.Response;
#if NETCOREAPP
         IWebHostEnvironment env = (IWebHostEnvironment)Env;
         home = env.WebRootPath;
#else
      home = Request.PhysicalApplicationPath;
#endif
    }
    public HttpContext context = null;
    public HttpRequest Request = null;
    public HttpResponse Response = null;
    public string home = null;

    public void pull(string repo) {
      ProcessStartInfo psi = new ProcessStartInfo();
      psi.RedirectStandardOutput = true;
      psi.UseShellExecute = false;
      psi.CreateNoWindow = true;

      psi.FileName = "git"; // string.Concat(home, "bats\\", repo, ".bat");
      psi.Arguments = "pull";
      psi.WorkingDirectory = home + repo;

      var jrs = new mroJSON();
      jrs.set("cmd", psi.FileName);
      jrs.set("args", psi.Arguments);
      jrs.set("work", psi.WorkingDirectory);

      try {
        var p = Process.Start(psi);
        jrs.set("status", "ok");
        jrs.set("msg", p.StandardOutput.ReadToEnd());
      } 
      catch (Exception err) {
        jrs.set("status", "err");
        jrs.set("msg", err.Message);
      }

      response.Write(Response, jrs.get_json());
    }
  }
}

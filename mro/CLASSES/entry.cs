#if NETCOREAPP
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Hosting;
#endif

using System.Web;

namespace mro {
  public interface Ientry {
    void link(link lnk);
  }
  public class entry {
#if NETCOREAPP
      public void run(object component, HttpContext context, IWebHostEnvironment env) {
#else
    public void run(object component, HttpContext context, object env) {
#endif
      var link = new link(component, context, (object)env);
      ((Ientry)component).link(link);
      link.go(ref basics, ref values, ref result, ref newval);
    }
    mroJSON values = null;
    mroJSON basics = null;
    mroJSON result = null;
    mroJSON newval = null;
  }
}
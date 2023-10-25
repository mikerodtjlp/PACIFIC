#if NETCOREAPP
using Microsoft.AspNetCore.Http;
#endif
using System;
using System.Text;
using System.IO;

namespace mro {
  public class setup {
    static public void load_params(string home,
                                     string file,
                                     ref mroJSON target) {
      var f = mem.join2(home, file);
      var a = File.ReadAllText(f);
      target.set_value(new mroJSON(a));
    }
  }
}

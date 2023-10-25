using System;
using mro;

namespace mro.BO {
   public class shell {
      public shell() {
      }
      public shell(string path, string action, string prms) {
         this.path = path;
         this.action = action;
         this.prms = prms;
      }

      public string path { get; set; }
      public string action { get; set; }
      public string prms { get; set; }

      public void pass_into(mroJSON result) {
         result.on(defs.ZISSHEL);
         result.set(defs.ZNSHELS, "1");
         result.set(defs.ZSHELLP, path);
         result.set(defs.ZSHELLA, action);
         result.set(defs.ZSHELLR, prms);
      }
   }
}

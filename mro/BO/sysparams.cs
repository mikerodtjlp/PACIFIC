using System;

namespace mro.BO {
   public class sysparams {
      public sysparams() {
      }
      public sysparams(string name, string version) {
         this.name = name;
         this.version = version;
      }

      public string name { get; set; }
      public string version { get; set; }
   }
}

using System;
using mro;

namespace mro.BO {
   public struct filedownload {
      public filedownload(string server, string folder, string file, string type) {
         this.server = server;
         this.folder = folder;
         this.file = file;
         this.type = type;
         this.tofile = string.Empty;
         this.topath = string.Empty;
         this.direct = string.Empty;
      }

      public string server { get; set; }
      public string folder { get; set; }
      public string file { get; set; }
      public string type { get; set; }

      public string tofile { get; set; }
      public string topath { get; set; }
      public string direct { get; set; }

      public void pass_into(mroJSON result) {
         result.on(defs.ZDOWNLD);
         result.set(defs.ZNDOWNS, "1");
         result.set(defs.ZDWNFSV, server);
         result.set(defs.ZDWNFPA, folder);
         result.set(defs.ZDWNFFL, file);
         result.set(defs.ZDWNTYP, type);
         result.set(defs.ZDWNTFL, tofile);
         result.set(defs.ZDWNTPA, topath);
         result.set(defs.ZDWNDIR, direct);
      }
   }
}

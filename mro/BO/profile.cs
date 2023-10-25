using System;
using System.Collections.Generic;

namespace mro.BO {
   public class profile {
      public profile() {
      }
      public profile(string id, string name) {
         this.id = id;
         this.name = name;
      }

      public string id { get; set; }
      public string name { get; set; }

      public List<profile_detail> detail = null;
   }
}

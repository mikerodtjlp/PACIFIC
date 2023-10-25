using System;

namespace mro.BO {
   public class profile_detail {
      public profile_detail() {
      }

      public profile_detail(string id, string transaction) {
         this.id = id;
         this.transaction = transaction;
      }

      public string id { get; set; }
      public string transaction { get; set; }
      public CParameters rights = null;
   }
}

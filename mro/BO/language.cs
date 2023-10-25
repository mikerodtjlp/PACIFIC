using System;

namespace mro.BO {
   public class language {
      public language() {
      }
      public language(string id, string name) {
         this.id = id;
         this.name = name;
      }

      public string id { get; set; }
      public string name { get; set; }
   }
}

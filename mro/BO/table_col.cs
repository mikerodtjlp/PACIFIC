using System;

namespace mro.BO {
   public class table_col {
      public table_col(string id, string name, string field_type) {
         this.id = id;
         this.name = name;
         this.type = -1;
         this.field_type = field_type;
      }

      public string id { get; set; }
      public string name { get; set; }
      //public Type type { get; set; }
      public int type { get; set; }
      public string field_type { get; set; }
   }
}

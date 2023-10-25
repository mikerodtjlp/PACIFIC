using System;

namespace mro.BO {
   public class query {
      public query() { }
      public query(string sqltext) { sql = sqltext; }
      public string sql { get; set; }
   }
}

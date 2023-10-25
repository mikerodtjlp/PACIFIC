using System;

namespace mro.BO {
   public class company {
      public company(int id) {
         this.id = id;
      }
      public int id { get; set; }
      public string name { get; set; }
      public int owner { get; set; }
      public int sector { get; set; }
      public string legalID { get; set; }
      public string country { get; set; }
      public string state { get; set; }
      public string city { get; set; }
      public string address1 { get; set; }
      public string address2 { get; set; }
      public string zipcode { get; set; }
   }
}

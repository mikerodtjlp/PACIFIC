using System;

namespace mro.BO {
   public struct webaddress {
      //public webaddress() { }
      //public webaddress(string code) { id = code; }
      public webaddress(string code, string addresstext) { 
         id = code; 
         address = addresstext; 
      }
      public string id { get; set; }
      public string address { get; set; }
   }
}

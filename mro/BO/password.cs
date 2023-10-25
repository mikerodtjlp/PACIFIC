using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace mro.BO {
   public class password {
      public password(string value) {
         this.value = value;
      }
      public password(password value) {
         this.value = value.value;
      }
      public string value { get; set; }
   }
}

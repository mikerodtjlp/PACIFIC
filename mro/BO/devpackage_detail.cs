using System;

namespace mro.BO {
   public class devpackage_detail {
      public devpackage_detail() {
      }

      public devpackage_detail(string id) {
         this.id = id;
      }
      public devpackage_detail(string id, string type) {
         this.id = id;
         this.type = type;
      }
      public devpackage_detail(string id, string type, string user,
                                  string action, string component,
                                  string comms, string status) {
         this.id = id;
         this.type = type;
         this.user = user;
         this.action = action;
         this.comp = component;
         this.comms = comms;
         this.status = status;
      }

      public string id { get; set; }
      public string type { get; set; }
      public string user { get; set; }
      public string action { get; set; }
      public string comp { get; set; }
      public string comms { get; set; }
      public string status { get; set; }
   }
}

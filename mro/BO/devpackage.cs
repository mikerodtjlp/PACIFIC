using System;
using System.Collections.Generic;

namespace mro.BO {
   public class devpackage {
      private DateTime? _date_submitted = null;
      private DateTime? _date_required = null;
      private DateTime? _date_tentative = null;
      private DateTime? _date_start = null;
      private DateTime? _date_finish = null;
      public devpackage() {
      }
      public devpackage(string id) {
         this.id = id;
      }
      public devpackage(string id, string name) {
         this.id = id;
         this.name = name;
      }

      public string id { get; set; }
      public string name { get; set; }
      public string comments { get; set; }
      public string status { get; set; }
      public string type { get; set; }
      public string reason { get; set; }
      public string priority { get; set; }
      public string owner { get; set; }
      public string originator { get; set; }
      public string keyuser { get; set; }
      public DateTime? date_submitted {
         get {
            if (_date_submitted == DateTime.MinValue) return null;
            return _date_submitted;
         }
         set { _date_submitted = value; }
      }
      public DateTime? date_required {
         get {
            if (_date_required == DateTime.MinValue) return null;
            return _date_required;
         }
         set { _date_required = value; }
      }
      public DateTime? date_tentative {
         get {
            if (_date_tentative == DateTime.MinValue) return null;
            return _date_tentative;
         }
         set { _date_tentative = value; }
      }
      public DateTime? date_start {
         get {
            if (_date_start == DateTime.MinValue) return null;
            return _date_start;
         }
         set { _date_start = value; }
      }
      public DateTime? date_finish {
         get {
            if (_date_finish == DateTime.MinValue) return null;
            return _date_finish;
         }
         set { _date_finish = value; }
      }

      public List<devpackage_detail> detail = null;
   }
}

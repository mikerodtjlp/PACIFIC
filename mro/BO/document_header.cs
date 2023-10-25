using System;

namespace mro.BO {
   public class document_header {
      private DateTime? _date_create = null;
      private DateTime? _date_modify = null;

      public document_header() { isvalidated = false; }
      public document_header(string code) {
         id = code;
         isvalidated = false;
      }
      public document_header(string code, string typ) {
         id = code;
         type = typ;
      }
      public document_header(string code, string typ, int ver) {
         id = code;
         type = typ;
         version = ver;
      }
      public document_header(string code, string typ, int ver, string system) {
         id = code;
         type = typ;
         version = ver;
         this.system = system;
      }
      public string id { get; set; }
      public string type { get; set; }
      public int version { get; set; }
      public DateTime? date_create {
         get {
            if (_date_create == DateTime.MinValue) {
               return null;
            }
            return _date_create;
         }
         set {
            _date_create = value;
         }
      }
      public DateTime? date_modify {
         get {
            if (_date_modify == DateTime.MinValue) {
               return null;
            }
            return _date_modify;
         }
         set {
            _date_modify = value;
         }
      }
      public string system { get; set; }
      public string module { get; set; }

      private bool isvalidated { get; set; }
      public void validate() {
         if (isvalidated) return;
         err.require(id.Length == 0, cme.INC_DATA_DOCUMENT);
         //err.require(version.Length == 0, cme.INC_DATA_VERSION);
         err.require(type.Length == 0, cme.INC_DATA_TYPE);
         err.require(system.Length == 0, cme.INC_DATA_SYSTEM);
         err.require(module.Length == 0, cme.INC_DATA_MODULE);
         isvalidated = true;
      }
   }
}

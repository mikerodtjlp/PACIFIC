using System;

namespace mro.BO {
   public class document {
      private DateTime? _date_create = null;
      private DateTime? _date_modify = null;

      public document() { isvalidated = false; }
      public document(string lib, string code) {
         this.lib = lib;
         id = code;
         isvalidated = false;
      }
      public document(string lib, string code, string dat) {
         this.lib = lib;
         id = code;
         data = dat;
      }
      public document(string lib, string code, string dat, string typ) {
         this.lib = lib;
         id = code;
         data = dat;
         type = typ;
      }
      public document(string lib, string code, string dat, string typ, int ver) {
         this.lib = lib;
         id = code;
         data = dat;
         type = typ;
         version = ver;
      }
      public document(string lib, string code, string dat, string typ, int ver, string system) {
         this.lib = lib;
         id = code;
         data = dat;
         type = typ;
         version = ver;
      }
      public string lib { get; set; }
      public string id { get; set; }
      public string data { get; set; }
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

      private bool isvalidated { get; set; }
      public void validate() {
         if (isvalidated) return;
         err.require(lib.Length == 0, cme.INC_DATA_LIBRARY);
         err.require(id.Length == 0, cme.INC_DATA_DOCUMENT);
         err.require(data.Length == 0, cme.INC_DATA_CONTENT);
         //err.require(version.Length == 0, cme.INC_DATA_VERSION);
         err.require(type.Length == 0, cme.INC_DATA_TYPE);
         isvalidated = true;
      }
   }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace mro.BO {
   public class email {
      private DateTime? _creation_date = null;
      private DateTime? _send_date = null;

      public email() { }

      public email(long id, string from, string to, string cc, string subject, string body, string attach) {
         this.id = id;
         this.from = from;
         this.to = to;
         this.cc = cc;
         this.subject = subject;
         this.body = body;
         this.attach = attach;
      }
      public email(string from, string to, string cc, string subject, string body, string attach) {
         this.from = from;
         this.to = to;
         this.cc = cc;
         this.subject = subject;
         this.body = body;
         this.attach = attach;
      }
      public long id { get; set; }
      public string from { get; set; }
      public string to { get; set; }
      public string cc { get; set; }
      public string subject { get; set; }
      public string body { get; set; }
      public string attach { get; set; }
      public int status { get; set; }
      public DateTime? creation_date {
         get {
            if (_creation_date == DateTime.MinValue) return null;
            return _creation_date;
         }
         set { _creation_date = value; }
      }
      public DateTime? send_date {
         get {
            if (_send_date == DateTime.MinValue) return null;
            return _send_date;
         }
         set { _send_date = value; }
      }
      public int dir { get; set; }
   }
}

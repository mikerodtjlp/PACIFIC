using System;

namespace mro.BO {
   public class user {
      public user(string id) {
         this.id = id;
      }

      public string id { get; set; }
      public string description { get; set; }
      public string comments { get; set; }
      public DateTime date_start { get; set; }
      public DateTime date_end { get; set; }
      public string password { get; set; }
      public string type { get; set; }
      public DateTime time_start { get; set; }
      public DateTime time_end { get; set; }
      public string groupid { get; set; }
      public string email { get; set; }
      public string winuser { get; set; }
      public string phone { get; set; }
      public string image { get; set; }

      public void validate() {
         err.require(string.IsNullOrEmpty(id), cme.INC_DATA_USER);
      }

      public void to_json(mroJSON prms) {
         prms.set("user", id);
         prms.set("image", image);
         prms.set("description", description);
         prms.set("comments", comments);
         prms.set("date_start", utils.date_part(date_start));
         prms.set("date_end", utils.date_part(date_end));
         prms.set("password", password);
         prms.set("type", type);
         prms.set("time_start", utils.hour_part(time_start));
         prms.set("time_end", utils.hour_part(time_end));
         prms.set("group", groupid);
         prms.set("email", email);
         prms.set("winuser", winuser);
         prms.set("phone", phone);
      }
   }
}

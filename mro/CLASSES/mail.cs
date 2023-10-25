#if NETCOREAPP
using Microsoft.AspNetCore.Http;
#endif

using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Net.Mail;
using System.Reflection;
using System.IO;
using System.Data.OleDb;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Data;
using System.Threading;
using System.Web;

namespace mro {
  public class mail {
    public static void start_email(string company, string site, string info, string to) {
      var fr = company + " Kernel service";
      var cc = "";
      var sb = company + " Start node: " + site;
      var bo = company + " Service started...<br><br>" + info;
      var at = "";
      compose_email("", fr, to, cc, sb, bo, at, 0, true);
    }
    public static void compose_email(string dbcode,
                               string from,
                               string to,
                               string cc,
                               string subject,
                               string body,
                               string attach,
                               int direction,
                               bool direct) {
      if (direct) {
        SendEmail(from, to, cc, subject, body, attach);
      }
      else {
        var qry = string.Concat("exec dbo.email_ins '",
           from, "','", to, "','", subject, "','",
           body, "','", attach, "',", direction, ";");

        sql.sqlnores(dbcode, qry, false);
      }
    }
    public static bool SendEmail(//int requestId, string assignedByUserName)
                         string fromx,
                         string tox,
                         string cc,
                         string subject,
                         string body,
                         string attach) {
    /*
      MailMessage message = new MailMessage(new MailAddress("pacific@coromuel.mx"),
                                    new MailAddress(tox));
      message.Subject = subject;
      message.IsBodyHtml = true;
      message.Body = "<!DOCTYPE html><html><head>" +
                  //"<meta name=\"viewport\"content=\"width=device-width, initial-scale=1\">"+
                  "<style>body{font-family:Arial;}</style></head><body>" +
                  "<h1> " + fromx + " </h1><br>" + body + "</body></html>";

      if (attach.Length > 0) message.Attachments.Add(new Attachment(attach));

      SmtpClient client = new SmtpClient("smtp.ionos.mx", 587) {
        Credentials = new NetworkCredential("pacific@coromuel.mx", "Rushrush2@"),
        EnableSsl = true
      };
      try {
        client.Send(message);
      } 
      catch (SmtpException ex) { throw ex; }
    */

      return true;
    }
    public static void ReadEmail() {
      /*var client = new Pop3Client();
   client.Connect("smtp.ionos.mx", 587, false);
   client.Authenticate("pacific@coromuel.mx", "Rushrush2@");

   int messageCount = client.GetMessageCount();

   var allMessages = new List<string>(messageCount);

   for (int i = messageCount; i > 0; i--)
   {
     allMessages.Add(client.GetMessage(i).ToString());
   }*/

      //return allMessages;
    }
  }
}

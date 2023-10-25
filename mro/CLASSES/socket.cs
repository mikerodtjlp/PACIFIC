#if NETCOREAPP
using Microsoft.AspNetCore.Http;
#endif
using System;
using System.Collections.Generic;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Net.Mail;
//using System.Net.Mime;
using System.Reflection;
using System.IO;
using System.Data.OleDb;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Data;
using System.Threading;
using System.Web;

namespace mro {
  public sealed class mrosocket {
    //public static ASCIIEncoding ascii = new ASCIIEncoding();
    public static UTF8Encoding utf8 = new UTF8Encoding();
    public static UnicodeEncoding Unicode = new UnicodeEncoding();
    public const string LOCADDR = "127.0.0.1";

    static byte[] MRO_UNICODE_BOM = new byte[2] { 0xFF, 0xFE };
    static byte[] MRO_END_MSG_MRK = new byte[4] { 0xD, 0xA, 0xD, 0xA };

    static public string webservice(char[] buffer,
                                     string verb,
                                     string url,
                                     string postdata = "") {
      string result = string.Empty;
      webservice(buffer, verb, url, postdata, ref result);
      return result;
    }
    static public void webservice(char[] buffer,
                                     string verb,
                                     string url,
                                     StringBuilder r) {

      string result = string.Empty;
      webservice(buffer, verb, url, null, ref result);
      r.Length = 0;
      r.Append(result);
    }
    static public void webservice(char[] buffer,
                                     string verb,
                                     string url,
                                     string postdata,
                                     ref string resp) {
      const int LENPACKAGE = 8192;

      HttpWebResponse myHttpWebResponse = null;
      Stream streamResponse = null;
      try {
        bool ispost = verb == method.POST;
        byte[] data = null;
        if (ispost) data = mrosocket.utf8.GetBytes(postdata);

        // creating the request
        HttpWebRequest myRequest = (HttpWebRequest)WebRequest.Create(url);

        //myRequest.Credentials = CredentialCache.DefaultCredentials;
        //ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;

        myRequest.Method = verb;

        if (ispost) {
          myRequest.ContentType = "application/x-www-form-urlencoded";
          myRequest.ContentLength = data.Length;
          using (var stream = myRequest.GetRequestStream()) {
            stream.Write(data, 0, data.Length);
          }
        }

        myHttpWebResponse = (HttpWebResponse)myRequest.GetResponse();
        streamResponse = myHttpWebResponse.GetResponseStream();
        using (StreamReader streamRead = new StreamReader(streamResponse)) {
          char[] c = buffer;
          int index = 0;
          int page = 0;
          StringBuilder r = new StringBuilder();
          int safetybreak = LENPACKAGE * 16;
          int packs = 0;
          while (true) {
            if (++packs == safetybreak) break;
            int nc = streamRead.Read(c, index, c.Length - index);
            if (nc < 1) break;
            index += nc;
            if (index >= LENPACKAGE) {
              r.Append(c, 0, index);
              index = 0;
              ++page;
            }
          }
          if (page == 0) {
            if (index > 0) resp = new string(c, 0, index);
            else resp = string.Empty;
          }
          else {
            if (index > 0)
              r.Append(c, 0, index);
            resp = r.ToString();
          }
          if (packs == safetybreak) err.require(cme.ECNBRKN);
        }
        streamResponse.Close();
        streamResponse = null;
        myHttpWebResponse.Close();
        myHttpWebResponse = null;
      } 
      finally {
        if (streamResponse != null) { 
          streamResponse.Close(); 
          streamResponse.Dispose(); 
        }
        if (myHttpWebResponse != null) { 
          myHttpWebResponse.Close(); 
        }
      }
    }

    /*static public void webservice(char[] buffer,
                            string verb,
                            string url,
                            StringBuilder r,
                            StringBuilder postdata = null) {
       const int LENPACKAGE = 8192;

       HttpWebResponse myHttpWebResponse = null;
       Stream streamResponse = null;
       try {
          // creating the request
          HttpWebRequest myRequest = (HttpWebRequest)WebRequest.Create(url);
          myRequest.Method = verb;
          if (postdata != null) {
             ASCIIEncoding encoding = new ASCIIEncoding();
             byte[] bf = encoding.GetBytes(postdata.ToString());
             myRequest.ContentType = "application/x-www-form-urlencoded";
             myRequest.ContentLength = bf.Length;
             using (Stream postStream = myRequest.GetRequestStream()) {
                postStream.Write(bf, 0, bf.Length);
             }
          }
          myHttpWebResponse = (HttpWebResponse)myRequest.GetResponse();
          streamResponse = myHttpWebResponse.GetResponseStream();
          using (StreamReader streamRead = new StreamReader(streamResponse)) {
             char[] c = buffer;
             int index = 0;
             int page = 0;
             r.Length = 0;
             int safetybreak = LENPACKAGE * 16;
             int packs = 0;
             while (true) {
                if (++packs == safetybreak) break;
                int nc = streamRead.Read(c, index, c.Length - index);
                if (nc < 1) break;
                index += nc;
                if (index >= LENPACKAGE) {
                   r.Append(c, 0, index);
                   index = 0;
                   ++page;
                }
             }
             if (page == 0) {
                if (index > 0) r.Append(c, 0, index);
             }
             else {
                if (index > 0)
                   r.Append(c, 0, index);
             }
             if (packs == safetybreak) err.require(cme.ECNBRKN);
          }
          streamResponse.Close();
          streamResponse = null;
          myHttpWebResponse.Close();
          myHttpWebResponse = null;
       }
       finally {
          if (streamResponse != null) {
             streamResponse.Close();
             streamResponse.Dispose();
          }
          if (myHttpWebResponse != null) {
             myHttpWebResponse.Close();
          }
       }
    }*/

    public static void Send(Socket socket,
                      byte[] buffer,
                      int offset,
                      int size,
                      int timeout) {
      int startTickCount = Environment.TickCount;
      int sent = 0;  // how many bytes is already sent
      do {
        if (Environment.TickCount > startTickCount + timeout)
          throw new Exception("Timeout.");
        try {
          sent += socket.Send(buffer, offset + sent, size - sent, SocketFlags.None);
        } 
        catch (SocketException ex) {
          if (ex.SocketErrorCode == SocketError.WouldBlock ||
             ex.SocketErrorCode == SocketError.IOPending ||
             ex.SocketErrorCode == SocketError.NoBufferSpaceAvailable)
            continue;//Thread.Sleep(30); // socket buffer is probably full, wait and try again
          else throw ex;  // any serious error occurr
        }
      } while (sent < size);
    }

    //static public void execute(char[] buffer, byte[] bbytes, //StringBuilder str,
    //    string server, int port, StringBuilder command, StringBuilder resp) {
    //   execute(buffer, bbytes, /*str,*/ server, port, command.ToString(), resp);
    //}
    /*static public void execute(char[] buffer, byte[] bbytes, //StringBuilder str,
        string server, int port, string command, StringBuilder resp) {
       const int LENPACKAGE = 8192;

       byte[] send = Unicode.GetBytes(command);
       char[] chars = buffer;
       byte[] bytes = bbytes;
       resp.Length = 0;

       int index = 0;
       int page = 0;

       TcpClient tcpClient = null;
       try {
          tcpClient = new TcpClient(server, port);
          Socket s = tcpClient.Client;
          Send(s, MRO_UNICODE_BOM, 0, 2, 10000);
          Send(s, send, 0, send.Length, 10000);
          Send(s, MRO_END_MSG_MRK, 0, 4, 10000);

          int safetybreak = LENPACKAGE * 16;
          int packs = 0;
          while (true) {
             if (++packs == safetybreak) break;

             int i = 0;
             try {
                i = s.Receive(bytes, 0, LENPACKAGE, SocketFlags.None);
             }
             catch (SocketException ex) {
                if (ex.SocketErrorCode == SocketError.WouldBlock ||
                    ex.SocketErrorCode == SocketError.IOPending ||
                    ex.SocketErrorCode == SocketError.NoBufferSpaceAvailable) {
                   Thread.Sleep(16);
                   continue;
                }//Thread.Sleep(30); // socket buffer is probably full, wait and try again
                else throw ex;  // any serious error occurr
             }

             if (i < 1) break;//if (i == 0) break;
                              // find out the real length of the package
             int charCount = Unicode.GetCharCount(bytes, 0, i);
             // converting to unidcode the just read data
             Unicode.GetChars(bytes, 0, i, chars, index);
             // form the final string
             index += charCount;
             if (index >= LENPACKAGE) {
                resp.Append(chars, 0, index);
                index = 0;
                ++page;
             }
          }
          if (packs == safetybreak) err.require(cme.ECNBRKN);
          tcpClient.Close();
          tcpClient = null;
       }
       catch (Exception e) {
          throw new Exception(string.Concat("execute:", server, ":",
              port.ToString(), ":", e.Message));
       }
       finally {
          if (tcpClient != null) { tcpClient.Close(); tcpClient = null; }
       }

       if (page == 0) {
          if (index > 0) resp.Append(chars, 0, index);
          return;
       }
       else {
          if (index > 0) resp.Append(chars, 0, index);
       }
       //resp.Append(str);
    }*/

    /*
     * This function interfaces th C++ KERNEL for low level services
     */
    static public void atlantic(char[] buffer,
                                  byte[] bbytes,
                                  StringBuilder str,
                                  string server,
                                  int port,
                                  StringBuilder command,
                                  mroJSON res,
                                  ref string resource) {
      var atl = new CParameters();
      atlantic(buffer, bbytes, str, server, port, command.ToString(), atl);

      var fix = atl.has(defs.ZFILERS);
      resource = string.Empty;
      if (fix) atl.extract(defs.ZFILERS, ref resource);
      res.set_value(atl);
    }
    static public void atlantic(char[] buffer,
                                  byte[] bbytes,
                                  StringBuilder str,
                                  string server,
                                  int port,
                                  StringBuilder command,
                                  mroJSON res) {
      var atl = new CParameters();
      atlantic(buffer, bbytes, str, server, port, command.ToString(), atl);

      var fix = atl.has(defs.ZFILERS);
      var tmp = string.Empty;
      if (fix) atl.extract(defs.ZFILERS, ref tmp);

      res.set_value(atl);

      if (fix) res.set(defs.ZFILERS, tmp);
    }
    static public void atlantic(char[] buffer,
                                  byte[] bbytes,
                                  StringBuilder str,
                                  string server,
                                  int port,
                                  string command,
                                  CParameters resp) {
      const int LENPACKAGE = 8192;

      byte[] send = Unicode.GetBytes(command);
      char[] chars = buffer;
      byte[] bytes = bbytes;
      str.Length = 0;

      int index = 0;
      int page = 0;

      TcpClient tcpClient = null;
      try {
        tcpClient = new TcpClient(server, port);
        Socket s = tcpClient.Client;
        Send(s, MRO_UNICODE_BOM, 0, 2, 10000);
        Send(s, send, 0, send.Length, 10000);
        Send(s, MRO_END_MSG_MRK, 0, 4, 10000);

        int safetybreak = LENPACKAGE * 16;
        int packs = 0;
        while (true) {
          if (++packs == safetybreak) break;

          int i = 0;
          try {
            i = s.Receive(bytes, 0, LENPACKAGE, SocketFlags.None);
          } 
          catch (SocketException ex) {
            if (ex.SocketErrorCode == SocketError.WouldBlock ||
               ex.SocketErrorCode == SocketError.IOPending ||
               ex.SocketErrorCode == SocketError.NoBufferSpaceAvailable) {
              Thread.Sleep(16);
              continue;
            }//Thread.Sleep(30); // socket buffer is probably full, wait and try again
            else throw ex;  // any serious error occurr
          }

          if (i < 1) break;//if (i == 0) break;
                           // find out the real length of the package
          int charCount = Unicode.GetCharCount(bytes, 0, i);
          // converting to unidcode the just read data
          Unicode.GetChars(bytes, 0, i, chars, index);
          // form the final string
          index += charCount;
          if (index >= LENPACKAGE) {
            str.Append(chars, 0, index);
            index = 0;
            ++page;
          }
        }
        if (packs == safetybreak) err.require(cme.ECNBRKN);
        tcpClient.Close();
        tcpClient = null;
      } 
      catch (Exception e) {
        throw new Exception(string.Concat("execute:", server, ":",
                port.ToString(), ":", e.Message));
      } 
      finally {
        if (tcpClient != null) { 
          tcpClient.Close(); 
          tcpClient = null; 
        }
      }

      if (page == 0) {
        if (index > 0) resp.set_value(chars, index);
        else resp.clear();
        return;
      }
      else {
        if (index > 0) str.Append(chars, 0, index);
      }
      resp.set_value(str);
    }
    /*
     * This function interfaces th C++ KERNEL for low level services
     * but writes directly the result to the client
     */
    static public void atlantic(char[] buffer,
                                  byte[] bbytes,
                                  StringBuilder str,
                                  string server,
                                  int port,
                                  StringBuilder cmd,
                                  response resp,
                                  bool retjson) {
      var wrk = new CParameters();
      atlantic(buffer, bbytes, str, server, port, cmd.ToString(),
         resp, retjson, wrk);
    }
    static public void atlantic(char[] buffer,
                                  byte[] bbytes,
                                  StringBuilder str,
                                  string server,
                                  int port,
                                  string command,
                                  response resp,
                                  bool retjson,
                                  CParameters prms) {
      str.Length = 0;
      const int LENPACKAGE = 8192;

      byte[] send = Unicode.GetBytes(command);
      char[] chars = buffer;
      byte[] bytes = bbytes;

      int index = 0;
      int page = 0;

      TcpClient tcpClient = null;
      try {
        tcpClient = new TcpClient(server, port);
        Socket s = tcpClient.Client;
        Send(s, MRO_UNICODE_BOM, 0, 2, 10000);
        Send(s, send, 0, send.Length, 10000);
        Send(s, MRO_END_MSG_MRK, 0, 4, 10000);

        int safetybreak = LENPACKAGE * 16;
        int packs = 0;
        while (true) {
          if (++packs == safetybreak) break;

          int i = 0;
          int offset = 0;
          try {
            i = s.Receive(bytes, 0, LENPACKAGE, SocketFlags.None);
            if (packs == 1 && i >= 2 && bytes[0] == 255 && bytes[1] == 254)
              offset = 2;
          } 
          catch (SocketException ex) {
            if (ex.SocketErrorCode == SocketError.WouldBlock ||
               ex.SocketErrorCode == SocketError.IOPending ||
               ex.SocketErrorCode == SocketError.NoBufferSpaceAvailable) {
              Thread.Sleep(16);
              continue;
            }//Thread.Sleep(30); // socket buffer is probably full, wait and try again
            else throw ex;  // any serious error occurr
          }

          if (i < 1) {
            if (page == 0)
              if (index > 0)
                if (retjson) str.Append(chars, 0, index);
                else resp.Write(chars, 0, index);
            break;//if (i == 0) break;
          }
          // find out the real length of the package
          int charCount = Unicode.GetCharCount(bytes, offset, i - offset);
          // converting to unidcode the just read data
          Unicode.GetChars(bytes, offset, i - offset, chars, index);
          // form the final string
          index += charCount;
          if (index >= LENPACKAGE) {
            if (retjson) str.Append(chars, 0, index);
            else resp.Write(chars, 0, index);
            index = 0;
            ++page;
          }
        }
        if (packs == safetybreak) err.require(cme.ECNBRKN);
        tcpClient.Close();
        tcpClient = null;
      } 
      catch (Exception e) {
        throw new Exception(string.Concat("execute:", server, ":",
                port.ToString(), ":", e.Message));
      } 
      finally {
        if (tcpClient != null) {
          tcpClient.Close();
          tcpClient = null;
        }
      }

      if (page > 0) {
        if (index > 0) {
          if (retjson) str.Append(chars, 0, index);
          else resp.Write(chars, 0, index);
        }
      }
      if (retjson && str.Length > 0) {
        var s = str.ToString();
        // super parche
        if (s.IndexOf(defs.ZFILERS) != -1) {
          int pos = s.IndexOf(dhtml.html);
          if (pos != -1) s = utils.fix_html(s, pos);
        }
        prms.set_value(s);
        s = utils.mro2json(prms, str, retjson);

        if (s.Length > 0) {
          if (resp.nblocks > 0) resp.Write(',');
          resp.Write(s);
        }
      }
    }
  }
}
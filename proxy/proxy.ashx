<%@ WebHandler Language="C#" Class="proxy" %>
using System.Web;
public class proxy : IHttpHandler {
    public void ProcessRequest(HttpContext context) { new mro.proxy(context, null).go(); }
    public bool IsReusable { get { return false; } }
}
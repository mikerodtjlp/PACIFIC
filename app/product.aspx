<%@ Import Namespace="mro" %>
<%@ Import Namespace="mro.BL" %>

<!-- #include file="~/core.aspx" -->

<script runat="server" language="C#">
   protected void get_product_image() {
       var prodid = values.get("prodid");

       var image = string.Concat("http://", lnk.data.proxysvr, ":",
                   lnk.data.proxyprt, "/files/products/", prodid, ".jpg");
       result.set("image", image);
   }
</script>

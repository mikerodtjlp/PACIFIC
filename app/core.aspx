<script runat="server" language="C#">
   public link lnk = null;
   public mroJSON values = null;
   public mroJSON basics = null;
   public mroJSON result = null;
   public mroJSON newval = null;
   protected void set_log(string action, string key, string type) { lnk.set_log(action, key, type); }
</script>
<% (lnk = new link(this, Context, null)).go(ref basics, ref values, ref result, ref newval); %>
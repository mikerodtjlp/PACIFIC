<script runat="server" language="C#">
void pull(string repo) { new mro.git(this.Context, null).pull(repo); }
</script>
<% pull("ZTESTS"); %>
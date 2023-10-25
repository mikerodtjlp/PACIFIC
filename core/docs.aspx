<%@ Import Namespace="mro" %>
<%@ Import Namespace="mro.BL" %>

<!-- #include file="~/core.aspx" -->

<script runat="server" language="C#">
   //protected void read_doc_header() { bls.get_ctrbl().read_doc_header(lnk); }
   //protected void read_document() { bls.get_ctrbl().read_document(lnk); }
   protected void update_document() { bls.get_ctrbl().update_document(lnk); }
   protected void create_document() { bls.get_ctrbl().create_document(lnk); }
   protected void check_document() { bls.get_ctrbl().check_document(lnk); }
   //protected void check_document_exp() { bls.get_ctrbl().check_document_expanded(lnk); }
   //protected void expand_document() { bls.get_ctrbl().expand_document(lnk); }
</script>

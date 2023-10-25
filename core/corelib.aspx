<%@ Import Namespace="mro" %>
<%@ Import Namespace="mro.BL" %>

<!-- #include file="~/core.aspx" -->

<script runat="server" language="C#">
   protected void desencrypt_password() { bls.get_ctrbl().desencrypt_password(lnk); }
   protected void encrypt_password() { bls.get_ctrbl().encrypt_password(lnk); }
   protected void get_user() { bls.get_ctrbl().get_user(lnk); }
   protected void change_user() { bls.get_ctrbl().change_user(lnk); }
   protected void create_modify_user() { bls.get_ctrbl().create_modify_user(lnk); }
   protected void validate_pass() { bls.get_ctrbl().validate_pass(lnk); }
   protected void change_pass() { bls.get_ctrbl().change_pass(lnk); }
   protected void recover_pass() { bls.get_ctrbl().recover_pass(lnk); }
   protected void get_passwords() { bls.get_ctrbl().get_passwords(lnk); }

   protected void check_chat() { bls.get_ctrbl().check_chat(lnk); }
   protected void read_chat() { bls.get_ctrbl().read_chat(lnk); }
   protected void write_chat() { bls.get_ctrbl().write_chat(lnk); }
   protected void borrar_chat() { bls.get_ctrbl().clean_chat(lnk); }

   protected void deploy_create() { bls.get_ctrbl().deploy_create(lnk); }

   protected void exec_batch() { bls.get_ctrbl().exec_batch(lnk); }
   protected void ses_get_query() { bls.get_ctrbl().ses_get_query(lnk); }
   protected void create_company() { bls.get_ctrbl().create_company(lnk); }
   protected void create_account() { bls.get_ctrbl().create_account(lnk); }
   protected void join_company() { bls.get_ctrbl().join_company(lnk); }

   protected void form_shortcut() { bls.get_ctrbl().form_shortcut(lnk);   }
   protected void get_user_photos() {bls.get_ctrbl().get_user_photos(lnk);   }
</script>

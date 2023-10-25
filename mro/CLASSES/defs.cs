#if NETCOREAPP
using Microsoft.AspNetCore.Http;
#endif
using System;
using System.Globalization;

namespace mro {
  public enum exectype {
    NOTDEF = -1,
    NODE = 0,
    LOCAL = 1,
    /*INFO = 2,*/
    FILE = 3,
    EMPTY = 4
  };

  /**
   * core definitions
   * language of the framework, all most all actions and communication is triggered by
   * the use of some labels with some value on it, at this point they are strings but future
   * version will handle numeric ids for better performance
   */
  public struct defs {
    // header
    public static readonly string ZHEADER = "zhdr";           // header key
    public static readonly string ZPRIORI = "priority";       // priority of the request
    public static readonly string ZRETRES = "retresult";         // if a response if needed
  //public static readonly string ZSERVER = "server";            // what server/service we use
  //public static readonly string ZPKGNAM = "zpkgnam";        // what packaged for free functions
    public static readonly string RETJSON = "retjson";            // response as json format
    public static readonly string ZRETQRY = "retqry";             // response as json format

    // basics
    public static readonly string ZBASICS = "zbasics";        // basics key
    public static readonly string ZIPADDR = "ipaddre";            // client's ip address
    public static readonly string ZMACADR = "macaddr";
    public static readonly string ZMACNAM = "macname";
    public static readonly string ZUSERID = "zzzuser";            // user's id
    public static readonly string ZPASSWR = "zpasswr";            // user's password
    public static readonly string ZWINUSR = "zwinusr";            // windows user
    public static readonly string ZWINDOM = "zwindom";            // windows user domain
    public static readonly string ZSESINS = "zsesins";        // session instance
    public static readonly string ZSESMAC = "zsesmac";        // session machine
    public static readonly string ZSESCLI = "zsescli";        // session client
    public static readonly string ZSESSES = "zsesses";        // session session
    public static readonly string ZLANGUA = "p_langu";            // language id
    public static readonly string ZLAYOUT = "zlayout";            // layout id
                                                                  //public static readonly string ZDOMAIN = "aserver";            // domain
    public static readonly string ZCOMPNY = "zcompny";            // company
    public static readonly string ZAPIKEY = "ZAPIKEY";
    public static readonly string ZGUIVER = "ZGUIVER";           // gui version

    // extra-basics need to be worked out
    public static readonly string ZTRNCOD = "modcode";        // transaction code
    public static readonly string ZMODULE = "wmodule";            // module
    public static readonly string ZINSTAN = "instanc";        // client's session instance

    // debugging
    public static readonly string ZISDEBG = "isdebug";        // debug flag
    public static readonly string ZDFAULT = "default";         // default

    // execution
    public static readonly string ZZNFUNS = "zznfuns";        // how many function will be executed
    public static readonly string ZTYPCOM = "ztypcom";            // type of component
    public static readonly string ZWEBSIT = "zwebsit";        // zwebsit's name
    public static readonly string ZCOMPNM = "zcompnm";        // component's name
    public static readonly string ZFUNNAM = "dcsfunc";        // function's name
    public static readonly string ZVALUES = "zvalues";        // the values sended by the client(mostly gui client)
    public static readonly string ZNEWVAL = "znewval";            // new values to change from the functions to the whole
    public static readonly string ZFUN00Z = "zfun00z";        // for convinience functiion one's id
    public static readonly string ZFUN01Z = "zfun01z";        // for convinience functiion two's id
    public static readonly string ZBYGATE = "zbygate";            // function call through gate/proxy
    public static readonly string ZPARAMS = "zparams";            // parameters
    public static readonly string ZUSEBAS = "zusebas";            // we need to use basics values
    public static readonly string ZA2KILL = "za2kill";            // number of session's slice to kill
    public static readonly string ZPRXTIM = "prxtime";            // proxy's time
    public static readonly string ZSITTIM = "sittime";            // site's time
    public static readonly string ZQRYIDX = "zqryidx";            // FOR DEBUG ONLY

    // execution services
    public static readonly string ZSAVERR = "zsaverr";        // flag if we must save the error
    public static readonly string ZORIVAL = "zorival";        // original values
    public static readonly string ZLISTDT = "zlistdt";        // data related to the list data
    public static readonly string ZLISTAF = "listaff";            // list affected , 0..MAX LIST HANDLED
    public static readonly string ZSAVSTA = "zsavsta";        // save state ???
    public static readonly string ZDSPTCH = "ZDSPTCH";            // dispatch one webservice
    public static readonly string ZLSTRES = "lastres";            // last result
    public static readonly string ZUSRPRM = "zusrprm";            // user parameters

    public static readonly string ZDATA = "data";
    public static readonly string ZCOLS = "cols";
    public static readonly string ZTYPES = "types";
    public static readonly string ZNROWS = "nrows";
    public static readonly string ZNCOLS = "ncols";

    public static readonly string ZEXEDAT = "exedata";   // execel data
    public static readonly string ZEXETOT = "exetot";    // execel totals
    public static readonly string ZEXECLS = "execols";   // execel columns
    public static readonly string ZLSTDAT = "lstdata";   // text data
    public static readonly string ZLSTTOT = "lsttot";    // list totals
    public static readonly string ZLSTCLS = "lstcols";   // list columns
    public static readonly string ZTXTDAT = "txtdata";   // text data
    public static readonly string ZTXTTOT = "txttot";    // text totals

    // service internals
    public static readonly string ZSVRTYP = "svrtype";   // server's type
    public static readonly string ZSYSTEM = "zsystem";   // system
    public static readonly string CURPATH = "curpath";   // current server path
    public static readonly string ZURGTSZ = "zurgtsz";   // user rights

    public static readonly string ZCMPYNM = "zcmpynm";   // company' name
    public static readonly string ZCOREDB = "ZCOREDB";   // control dabatabase
    public static readonly string ZPSTMSG = "ZPSTMSG";   // on client post a windows message
    public static readonly string ZPSTMSI = "ZPSTMSI";   // on client post a windows message id
    public static readonly string ZKEYPRE = "ZKEYPRE";
    public static readonly string ZDSPPAG = "ZDSPPAG";
    public static readonly string ZRIGHT1 = "0right1";   // particular right
    public static readonly string ZNOTIFY = "znotify";
    public static readonly string ZINCFLD = "zincfld";   // incomplete fields

    // log
    public static readonly string ZZNLOGS = "nlogs";     // how many entries in log (none means 1)
    public static readonly string ZZTOLOG = "zztolog";   // log key ??
    public static readonly string ZTXTLOG = "ztxtlog";   // log text
    public static readonly string ZKEYLOG = "zkeylog";   // log key
    public static readonly string ZTYPLOG = "ztyplog";   // log type
    public static readonly string ZSAVLOG = "zsavlog";   // do we save the log

    // history session
    public static readonly string ZHISPOS = "zhispos";   // position on the history session
    public static readonly string ZHISTRN = "zhistrn";   // transaction code
    public static readonly string ZHISDSC = "zhisdsc";   // description
    public static readonly string ZHISTYP = "zhistyp";   // transaction's type
    public static readonly string ZHISSIZ = "zhissiz";

    // servers
    public static readonly string ZGATSVR = "gat_svr";   // gate server
    public static readonly string ZGATPRT = "gatport";   // gate port
    public static readonly string ZIISSVR = "iis_svr";   // IIS server
    public static readonly string ZIISPRT = "iisport";   // IIS port
    public static readonly string ZWEBSVR = "web_svr";   // web server

    public static readonly string ZSVRGAT = "gat";

    public static readonly string ZFUNCAL = "funcall";
    public static readonly string ZSITCAL = "sitcall";
    public static readonly string ATLEVNT = "atlevent";

    // errors
    public static readonly string ZERRORI = "zerrori";            // error information
    public static readonly string ZERRORM = "zierror";            // error message information
    public static readonly string ZERRORS = "zlerror";            // error stack trace information
    public static readonly string ZSERROR = "error01";        // error string
    public static readonly string ZCERROR = "errorc1";            // error string whitout translation
    public static readonly string ZNERROR = "errorn1";        // error code
    public static readonly string ZHERROR = "errorh1";        // error helper information
    public static readonly string ZNERRIN = "errorin";
    public static readonly string ZNERRLO = "errorln";
    public static readonly string ZIERROR = "errori1";        // error aditional information
    public static readonly string ZLERROR = "errorl1";            // error location

    public static readonly string ZSWARNG = "warning";            // warning description
    public static readonly string ZSTATUS = "zstatus";            // specific status response

    // core component
    //public static readonly string ZCTROBJ = "mroctrl.SessionMan";	// control component

    // client keys
    public static readonly string ZORIPAS = "oripass";        // original password
    public static readonly string ZDATTIM = "datetim";        // server date time 
    public static readonly string ZCURFLD = "zcurfld";            // current field focused
    public static readonly string ZEVENTN = "zeventn";        // event that dispatches the action
    public static readonly string ZGOTOBC = "zgotobc";
    public static readonly string ZNLSTAF = "nlistaf";            // id of list affected by the result
    public static readonly string ZNONFUN = "nonefun"; public static readonly int ZNONFUNLEN = 7;
    public static readonly string ZSESTIM = "zsestim";

    // framework transactions
    public static readonly string ZTRNPAS = "S000";           // password transaction
    public static readonly string ZCRDBNM = "CORE";

    // server keys
    public static readonly string REQCHKD = "reqchkd";        // mark as a requisition already checked
    public static readonly string RETPRMS = "retprms";            // parameters that we must return
    public static readonly string EVENTFN = "eventfn";            // event function
    public static readonly string BYPROXY = "zbproxy";
    public static readonly string ZRESEND = "zresend";            // end respond mark any complete webservice must have it
    public static readonly string ZWBSEND = "\"" + ZRESEND + "\":\"1\"";

    // download
    public static readonly string ZDOWNLD = "ZISDWLD";            // download key
    public static readonly string ZNDOWNS = "ZNDWLDS";            // how many downloads
    public static readonly string ZDWNFSV = "ZDWLFS0";            // from server
    public static readonly string ZDWNFPA = "ZDWLFP0";            // from path
    public static readonly string ZDWNFFL = "ZDWLFF0";            // specific file

    public static readonly string ZDWNTYP = "ZDWLTY0";            // file's type
    public static readonly string ZDWNTFL = "ZDWLTF0";            // to file
    public static readonly string ZDWNTPA = "ZDWLTP0";            // to path
    public static readonly string ZDWNDIR = "ZDWLDI0";            // to specific place
    public static readonly string ZDWNWIT = "ZDWLWI0";            // with what? default will be the client 

    public static readonly string ZISSHEL = "zisshell";           // shell key
    public static readonly string ZNSHELS = "zshellfiles";        // how many files ???
    public static readonly string ZSHELLP = "zshellpath0";        // path
    public static readonly string ZSHELLA = "zshellaction0";      // action
    public static readonly string ZSHELLR = "zshellprms0";        // parameters

    // gui
    public static readonly string ZUPDCLS = "updclis";            // client's params to be change
    public static readonly string ZLOCACT = "zlocact";
    public static readonly string ZSHWDLG = "ZSHWDLG";           // show dialog is need ?
    public static readonly string ZDLGTYP = "zdlgtyp";
    public static readonly string ZRTRNCD = "rmodule";
    public static readonly string ZRTRNPR = "rmodprm";
    public static readonly string ZTYPTRN = "typtran";            // transaction's type
    public static readonly string ZTYPRED = "typread";
    public static readonly string ZTYPRGT = "tyright";
    public static readonly string LIBRARY = "library";
    public static readonly string ZKERNEL = "KERNEL";
    public static readonly string ZFILE01 = "file_01";
    public static readonly string ZFILERS = "xfile01";
    public static readonly string ZFILE02 = "xfile02";
    public static readonly string ZSHRTCT = "shortcut";
    public static readonly string ZFINFUN = "finalfun";

    public static readonly string PDOCTYP = "doctype";
    public static readonly string PDOCTRN = "TRN";
    public static readonly string PDOCJSC = "JSC";


    public static readonly string ZDOEVNT = "doevent";            // event key
    public static readonly string ZONENTR = "onenter";            // when enter
    public static readonly string ZONLOAD = "onload"; public static readonly int ZONLOADLEN = 6; // when load the transaction first time
    public static readonly string ZONUNLD = "onunload";           // when leave the transaction
    public static readonly string ZONRELD = "onreload"; public static readonly int ZONRELDLEN = 8; //  reload it (not when reload it)
    public static readonly string ZONLINS = "oninsert";           // insert on list event
    public static readonly string ZONLUPD = "onedit";             // update on list event
    public static readonly string ZONLDEL = "ondelete";           // delete on list event

    // pacific only
    public static readonly string PDOCMNT = "document";
    public static readonly string PCODEID = "codeid";
    public static readonly string PQRYCOD = "query";
    public static readonly string PSQLTXT = "sqltext";
    public static readonly string PDATBAS = "database";
    public static readonly string PFLDTXT = "$text$";
    public static readonly string NORTREP = "noretrep";           // only generate it not download it
    public static readonly string PLINKVS = "link";
    public static readonly string PNLINKS = "nlinks";
    public static readonly string UPDCOLS = "updcols";
    public static readonly string UPDNCLS = "ncols";
    public static readonly string PNOEMPT = "notempty";
    public static readonly string PDEFAUS = "defaults";
    public static readonly string PNREPVS = "nrepvars";
    public static readonly string PFN2FND = "fun2find";
    public static readonly string PHEADER = "header";
    public static readonly string PDETAIL = "detail";
    public static readonly string PFOOTER = "footer";
    public static readonly string PADDRSS = "address";
    public static readonly string P_PORT_ = "port";
    public static readonly string P_NAME_ = "name";
    public static readonly string USER = "user"; //??????????

    public static readonly string VEMBEDD = "embedded"; public static readonly int VEMBEDDLEN = 8;

    public static readonly string TEMPLATE = "template";
    public static readonly string COLSNEEDED = "colsneeded";
    public static readonly string CHKISEMPTY = "checkisempty";
    public static readonly string RETCOLS = "retcols";
    public static readonly string RETCOLTYPE = "retcoltype";      // returns column's type
    public static readonly string RETINFO = "retinfo";
    public static readonly string USEDEFHDR = "usedefhdr";
    public static readonly string USEDEFFTR = "usedefftr";
    public static readonly string DTLBYCRT = "dtlbycrt";
    public static readonly string DTLBYRPL = "dtlbyrpl";
    public static readonly string FILENAME = "filename";
    public static readonly string QUERYHDR = "queryhdr";
    public static readonly string QUERYFTR = "queryftr";
    public static readonly string QUERYFT2 = "queryftr2";
    public static readonly string REPDESC = "repdesc";
    public static readonly string RNGDESC = "rngdesc";
    public static readonly string FIELDSW = "fieldsw";
    public static readonly string SUMFLDS = "sumflds";
    public static readonly string GIVEBACK = "giveback";
    public static readonly string TXTLINE = "txtln";

    public static readonly string NSITES = "nsites";
    public static readonly string SITE = "site";
    public static readonly string NAME = "name";

    public static readonly string NFIELDS = "nfields";

    public static readonly string FILTERNOEMPTY = "filternoempty";
    public static readonly string FILTERCOL = "filtercol";
    public static readonly string FILTERVAL = "filterval";
    public static readonly string TABLE = "table";
    public static readonly string ORDERBY = "orderby";
    public static readonly string SQLCMD = "sqlcmd";

    public static readonly string ZTYPE = "type";
    public static readonly string ZFILE = "file";
    public static readonly string ZFOLDER = "folder";

    public static readonly string HLPFOLDER = "help";
    public static readonly string TMPFOLDER = "temp";
    public static readonly string WKBFOLDER = "workbooks";
    public static readonly string PDFFOLDER = "pdfs";
    public static readonly string WBKGENERIC = "generic";

    public static readonly string MALFROM = "Email service";
  }
  public struct dhtml {
    public static readonly string http = "http://";
    public static readonly string br = "<br/>";
    public static readonly string html = "<html>";
  }
  /**
   * helper deinitions for framework values
   */
  public struct frm {
    public static readonly string GUIVER = "5.06.21";
    public static readonly string FRMVER = "3.2.11.04522";
    public static readonly string HDRNOTRETRES = "[zhdr:[retresult:0]]";
  }
  public struct pages {
    public static readonly string PROXY = "proxy";
  }
  /**
   * languages
   */
  public struct language {
    public static readonly string DEF = "EN";
  }
  /**
   * layouts
   */
  public struct layout {
    public static readonly string DEF = "SAP";
  }
  public struct method {
    public static readonly string GET = "GET";
    public static readonly string POST = "POST";
    public static readonly string UPD = "UPDATE";
    public static readonly string DEL = "DELETE";
  }
  public struct doctype {
    public static readonly string HTML = "html";
    public static readonly string PDF = "pdf";
    public static readonly string EXCEL = "excel";
    public static readonly string TEXT = "txt";
  }
  public struct datatype {
    public static readonly string TRN = "TRN";
    public static readonly string QRY = "QRY";
    public static readonly string CSS = "CSS";
    public static readonly string TXT = "TXT";
  }
  /**
   * type of log
   */
  public struct logtype {
    public static readonly string HDR = "H";
    public static readonly string DTL = "D";
    public static readonly string ERR = "E";
  }
  /** 
   * standard development status
   */
  public struct devstatus {
    public static readonly string WIP = "WIP";
    public static readonly string HLD = "HLD";
    public static readonly string RDY = "RDY";
    public static readonly string REL = "REL";
  }
  public struct typecomp {
    public static readonly string COM = "com";
    public static readonly string SQL = "sql";
    public static readonly string SYS = "sys";
  }
  /**
   * core message errors
   * mostly by the proxy and core component
   */
  public struct cme {
    // communication
    public static readonly string ECNBRKN = "connection_broken";
    public static readonly string SITE_TIMEOUT = "site_timeout";
    public static readonly string SITE_NOT_AVAILABLE = "website_not_available";
    public static readonly string SITE_NOT_CONFIGURED = "website_not_configured";

    // acount
    public static readonly string USER_HAVE_NO_PASSWORD = "user_not_have_password";
    public static readonly string ENORGTS = "insufficient_rights";
    public static readonly string CANNOT_GET_PASSWORDS = "cannot_get_passwords";
    public static readonly string PASS_NOT_EXIST = "password_not_exist";
    public static readonly string PASS_NEW_ORI_SAME = "password_new_origin_same";
    public static readonly string PASS_NEW_REP_DIFF = "password_new_rep_differents";
    public static readonly string PASS_LEAST_6_LONG = "password_at_least_6_chars_long";
    public static readonly string PASS_NOMORE_16_LONG = "password_no_more_16_chars_long";
    public static readonly string PASS_ONLY_CARS_NUMS = "password_only_letters_numbers";
    public static readonly string PASS_LEAST_CHAR_AZ = "password_at_least_char_az";
    public static readonly string PASS_LEAST_NUM_09 = "password_at_least_num_09";

    // generic functions 
    public static readonly string E2MVARS = "too_much_variables";
    public static readonly string FILTER_EMPTY = "filter_is_empty";
    public static readonly string COLS_WRONG_NUMBER = "wrong_columns_number";
    public static readonly string TOO_MUCH_COLUMNS = "too_much_columns";
    public static readonly string ONLY_EMBEDDED_QUERIES = "only_embedded_queries";

    public static readonly string FUN_NOT_EXIST = "function_not_exist";
    public static readonly string PAGE_MISSING = "codebehind_is_missing";
    public static readonly string DBNAME_MISSING = "database_is_missing";
    public static readonly string FUN_MISSING = "function_is_missing";
    public static readonly string FUN_NAME_2_LONG = "function_name_2_long";
    public static readonly string FUN_NOT_LOADED = "could_not_load_function";
    public static readonly string DOC_NOT_LOADED = "could_not_load_document";

    public static readonly string WS_WRONG_FORMAT = "wrong_webservice_format";
    public static readonly string WRONG_SAVE_RESULT_PARAM = "wrong_save_result_param";

    public static readonly string MODULE_MISSING = "no_module_supplied";
    public static readonly string POST_DATA_MISSING = "none_post_data";
    public static readonly string SERVER_MISSING = "server_not_supplied";
    public static readonly string PORT_MISSING = "port_not_supplied";

    // database
    public static readonly string DB_APP_NOT_CONFIGURED = "none_application_db_was_configured";
    public static readonly string DATABASE_NOT_EXIST = "database_not_exist";

    public static readonly string QUERY_MISSING = "query_id_not_supplied";
    public static readonly string QUERY_NOT_REGISTERED = "qry_not_registered";
    public static readonly string QUERY_EMPTY = "qry_is_empty";
    public static readonly string QUERY_TOO_LONG = "query_too_long";
    public static readonly string QUERY_NOT_EXIST = "qury_not_exist";

    // file errors
    public static readonly string FILE_EMPTY = "file_empty";
    public static readonly string FILE_NOT_FOUND = "file_not_found";
    public static readonly string FILE_NOT_EXIST = "file_not_exist";
    public static readonly string FILE_INVALID = "file_invalid";
    public static readonly string FOLDER_EMPTY = "folder_empty";
    public static readonly string FILE_UPLOADED_EMPTY = "file_uploaded_empty";
    public static readonly string FILE_NOT_UPLOADED = "file_not_uploaded";

    // incomplete data
    public static readonly string INC_DATA_ID = "inc_data_id";
    public static readonly string INC_DATA_NAME = "inc_data_name";
    public static readonly string INC_DATA_CONTENT = "inc_data_content";
    public static readonly string INC_DATA_TYPE = "inc_data_type";
    public static readonly string INC_DATA_SYSTEM = "inc_data_system";
    public static readonly string INC_DATA_MODULE = "inc_data_module";
    public static readonly string INC_DATA_DEVPACK = "inc_data_dev_package";
    public static readonly string INC_DATA_PASS = "inc_dat_password";
    public static readonly string INC_DATA_ORIPASS = "inc_dat_oripass";
    public static readonly string INC_DATA_NEWUSER = "inc_dat_newuser";
    public static readonly string INC_DATA_REPUSER = "inc_dat_repuser";
    public static readonly string INC_DATA_NEWPASS = "inc_dat_newpass";
    public static readonly string INC_DATA_REPPASS = "inc_dat_reppass";
    public static readonly string INC_DATA_FILE = "inc_dat_file";
    public static readonly string INC_DATA_EMAIL = "inc_dat_email";
    public static readonly string INC_DATA_PHONE = "inc_dat_phone";
    public static readonly string INC_DATA_ADDR1 = "inc_dat_addr1";
    public static readonly string INC_DATA_FIRST_NAME = "inc_dat_first_name";
    public static readonly string INC_DATA_LAST_NAME = "inc_dat_last_name";
    public static readonly string INC_DATA_SOURCE = "inc_dat_source";
    public static readonly string INC_DATA_TARGET = "inc_dat_target";

    public static readonly string EMAILS_ARE_DIFF = "emails_are_diff";

    // development packages
    public static readonly string DEV_PACK_NOT_EXIST = "dev_package_not_exist";
    public static readonly string DEV_PACK_NOT_IN_WIP = "dev_package_not_in_wip";
    public static readonly string DEV_PACK_DOC_NOT_BELONG = "doc_not_belong_to_dev_package";
    public static readonly string DEV_PACK_USER_NOT_REG = "user_not_register_to_dev_package";
    //public static readonly string DEV_PACK_NOT_BLNG_USR_NO_REG = "doc_not_belong_to_pack_or_user_not_reg";

    // possible duplicated with the app defs
    public static readonly string REG_NOT_EXIST = "reg_not_exist";
    public static readonly string REG_ALREADY_EXIST = "reg_already_exist";
    public static readonly string INC_DATA_LIBRARY = "inc_dat_library";
    public static readonly string INC_DATA_DOCUMENT = "inc_dat_document";
    public static readonly string INC_DATA_USER = "inc_dat_user";
    public static readonly string INC_DATA_CMPY = "inc_dat_cmpy";
    public static readonly string INC_DATA_USER_NAME = "inc_dat_user_name";
    public static readonly string INC_DATA_CMPY_NAME = "inc_dat_cmpy_name";
    public static readonly string INC_DATA_LIBGRP = "inc_dat_libgrp";

    public static readonly string USER_NOT_EMAIL_REG = "user_have_not_email_registered";
    public static readonly string USER_NOT_REGISTERED = "user_not_registered";
    public static readonly string USER_NOT_EXIST = "user_not_exist";
    public static readonly string CMPY_NOT_EXIST = "cmpy_not_exist";
    public static readonly string USER_ALREADY_EXIST = "user_already_exist";
    public static readonly string EMAIL_ALREADY_EXIST = "email_already_exist";
    public static readonly string PHONE_ALREADY_EXIST = "phone_already_exist";
    public static readonly string MAIL_NOT_EXIST = "mail_not_exist";

    public static readonly string ERROR_IN_LINE = "error_in_line";
    public static readonly string BAD_SYNTAX = "syntax_bad";
    public static readonly string INTERNAL_ERROR = "internal_error";
    public static readonly string LOGIC_ERROR = "logic_error";

    public static readonly string DWNLOD_COUDNT_REDIR = "could_not_redirect_download";

    public static readonly string SRC_NOT_EXIST = "source_not_exist";
    public static readonly string SRC_STILL_EXIST = "source_still_exist";
    public static readonly string DST_NOT_EXIST = "destiny_not_exist";

    public static readonly string USER_ALREADY_JOINED = "user_already_joined";

    // workbooks
    public static readonly string WKB_NOT_GENERATED = "workbook_not_generated";
  }
  public struct cnts {
    public static readonly int MAXCOLSPERQRY = 96;
  }
  public struct funs {
    // none return data
    public static readonly string SAVE_SESSIONS = "save_sessions";
    public static readonly string RESET_GHOST_SES = "reset_ghost_sessions";
    public static readonly string FLUSH_LOGS = "flush_logs";
    public static readonly string NOTIFY_USE = "notify_use";
    public static readonly string CHECK_SESSION = "check_session";
    public static readonly string RELEASE_SESS = "release_sessions";

    // return data
    public static readonly string GET_FINAL_FUN = "get_final_fun";
    public static readonly string LOOK_RIGHTS = "lookrgt";
    //public static readonly string GET_LAST_CSS    = "get_last_css";
    //public static readonly string GUI_GET_TEXT    = "gui_get_texts";
    public static readonly string COPY_SESSION = "copy_session";
    public static readonly string GET_LAST_STATE = "get_last_state";
    public static readonly string GET_FILE = "get_file";
  }
}

#pragma once

#define cmp2chW(s,a,b) (*(long*)(s) == \
	((unsigned short)(a) | (unsigned short)(b) << 16))
#define cmp4chW(s,a,b,c,d) (*(long*)(s) == \
	((unsigned short)(a) | (unsigned short)(b) << 16) && \
	*(long*)(s+2) == \
	((unsigned short)(c) | (unsigned short)(d) << 16))
#define set2chW(s,a,b) (*(long*)(s) = \
	((unsigned short)(a) | (unsigned short)(b) << 16));
#define set4chW(s,a,b,c,d) (*(long*)(s) = \
	((unsigned short)(a) | (unsigned short)(b) << 16), \
	*(long*)(s+2) = \
	((unsigned short)(c) | (unsigned short)(d) << 16));

#define cmp2chA(s,a,b) (*(short*)(s) == \
	((unsigned char)(a) | (unsigned char)(b) << 8))
#define cmp4chA(s,a,b,c,d) (*(long*)(s) == \
	 ((unsigned char)(a) | (unsigned char)(b) << 8 |   \
	 (unsigned char)(c) << 16 | (unsigned char)(d) << 24))
#define set2chA(s,a,b) (*(short*)(s) = \
	((unsigned char)(a) | (unsigned char)(b) << 8));
#define set4chA(s,a,b,c,d) (*(long*)(s) = \
	 ((unsigned char)(a) | (unsigned char)(b) << 8 |   \
	 (unsigned char)(c) << 16 | (unsigned char)(d) << 24));

#ifdef UNICODE
#define cmp2ch(s,a,b) (*(long*)(s) == \
	((unsigned short)(a) | (unsigned short)(b) << 16))
#define cmp4ch(s,a,b,c,d) (*(long*)(s) == \
	((unsigned short)(a) | (unsigned short)(b) << 16) && \
	*(long*)(s+2) == \
	((unsigned short)(c) | (unsigned short)(d) << 16))
#define set2ch(s,a,b) (*(long*)(s) = \
	((unsigned short)(a) | (unsigned short)(b) << 16));
#define set4ch(s,a,b,c,d) (*(long*)(s) = \
	((unsigned short)(a) | (unsigned short)(b) << 16), \
	*(long*)(s+2) = \
	((unsigned short)(c) | (unsigned short)(d) << 16));
#else
#define cmp2ch(s,a,b) (*(short*)(s) == \
	((unsigned char)(a) | (unsigned char)(b) << 8))
#define cmp4ch(s,a,b,c,d) (*(long*)(s) == \
	 ((unsigned char)(a) | (unsigned char)(b) << 8 |   \
	 (unsigned char)(c) << 16 | (unsigned char)(d) << 24))
#define set2ch(s,a,b) (*(short*)(s) = \
	((unsigned char)(a) | (unsigned char)(b) << 8));
#define set4ch(s,a,b,c,d) (*(long*)(s) = \
	 ((unsigned char)(a) | (unsigned char)(b) << 8 |   \
	 (unsigned char)(c) << 16 | (unsigned char)(d) << 24));
#endif

#define TXTLEN(constant)	((sizeof(constant) / sizeof(TCHAR)) - 1)
#define SIZETCH				(sizeof(TCHAR))
#define KEYDECL(key, val)	const TCHAR key[] = val; const int key##LEN = TXTLEN(key);\

#ifdef UNICODE
#define _tmemset wmemset
#define _tmemcpy wmemcpy
#define _tmemcmp wmemcmp
#define _tmemchr wmemchr
#else
#define _tmemset memset
#define _tmemcpy memcpy
#define _tmemcmp memcmp
#define _tmemchr memchr
#endif

const unsigned char MRO_UNICODE_RET[2]	= {	unsigned char('\n'), unsigned char('\t')	};
const unsigned char MRO_UNICODE_BOM[2]	= {	unsigned char(0xFF), unsigned char(0xFE)	};
const unsigned char MRO_END_MSG_MRK[4]	= {	unsigned char(0xD), unsigned char(0xA),
											unsigned char(0xD), unsigned char(0xA)		};
const char			MRO_END_MSG_STR[5]	= {	char(0xD), char(0xA), char(0xD), char(0xA), 0 };

#define GUIVER _T("5.06.24")
#define GUIVERLEN (7)
#define FRMVER _T("3.2.11.04522")
#define FRMVERLEN (12)

KEYDECL(ZPROCNO, _T("zprocno"));								// id of the request(internal)

// header
KEYDECL(ZHEADER, _T("zhdr"));									// header key
KEYDECL(ZPRIORI, _T("priority"));								// priority of the request
KEYDECL(ZRETRES, _T("retresult"));								// if a response if needed
KEYDECL(ZSERVER, _T("server"));		const int ZSERVERMAX = 3;	// what server/service we use
KEYDECL(ZPKGNAM, _T("zpkgnam"));	const int ZPKGNAMMAX = 32;	// what packaged for free functions
KEYDECL(RETJSON, _T("retjson"));								// response as json format

// basics
KEYDECL(ZBASICS, _T("zbasics"));								// basics key
KEYDECL(ZIPADDR, _T("ipaddre"));	const int ZIPADDRMAX = 15;	// client's ip address
KEYDECL(ZMACNAM, _T("macname"));	const int ZMACNAMMAX = 15;	// client's ip address
KEYDECL(ZMACADR, _T("macaddr"));	const int ZMACADRMAX = 23;	// client's mac address
KEYDECL(ZCOMPNY, _T("zcompny"));	const int ZCOMPNYMAX = 3;	// company's id
KEYDECL(ZUSERID, _T("zzzuser"));	const int ZUSERIDMAX = 16;	// user's id
KEYDECL(ZPASSWR, _T("zpasswr"));	const int ZPASSWRMAX = 16;	// user's password
KEYDECL(ZWINUSR, _T("zwinusr"));	const int ZWINUSRMAX = 15;	// windows user
KEYDECL(ZWINDOM, _T("zwindom"));	const int ZWINDOMMAX = 15;	// windows domain
KEYDECL(ZSESINS, _T("zsesins"));								// session instance
KEYDECL(ZSESMAC, _T("zsesmac"));								// session machine
KEYDECL(ZSESCLI, _T("zsescli"));								// session client
KEYDECL(ZSESSES, _T("zsesses"));								// session session
//KEYDECL(ZAGENTZ, _T("zagentz"));
KEYDECL(ZLANGUA, _T("p_langu"));	const int ZLANGUAMAX = 2;	// language id
KEYDECL(ZLAYOUT, _T("zlayout"));	const int ZLAYOUTMAX = 3;	// layout id
KEYDECL(ZDOMAIN, _T("aserver"));	const int ZDOMAINMAX = 3;	// domain
KEYDECL(ZCERTIF, _T("zcertif"));

// extra-basics need to be worked out
KEYDECL(ZTRNCOD, _T("modcode"));	const int ZTRNCODMAX = 32;	// transaction code
KEYDECL(ZACTINO, _T("actionno"));								// unique action id (mostly sequential by session)
KEYDECL(ZMODULE, _T("wmodule"));	const int ZMODULEMAX = 3;	// module
KEYDECL(ZINSTAN, _T("instanc"));								// client's session instance
KEYDECL(ZEMPLID, _T("zemplid"));								// company own employee id

KEYDECL(ZFILE,	 _T("file"));									// file
KEYDECL(ZFOLDER, _T("folder"));									// folder

// debugging
KEYDECL(ZISDEBG, _T("isdebug"));								// debug flag
KEYDECL(ZDFAULT, _T("default"));								// default

// execution
KEYDECL(ZZNFUNS, _T("zznfuns"));								// how many function will be executed
KEYDECL(ZTYPCOM, _T("ztypcom"));	const int ZTYPCOMMAX = 3;	// type of component
KEYDECL(ZWEBSIT, _T("zwebsit"));	const int ZWEBSITMAX = 32;	// ZWEBSIT's name
KEYDECL(ZCOMPNM, _T("zcompnm"));	const int ZCOMPNMMAX = 32;	// component's name
KEYDECL(ZFUNNAM, _T("dcsfunc"));	const int ZFUNNAMMAX = 64;	// function's name
KEYDECL(PDOCMNT, _T("document"));
KEYDECL(ZVALUES, _T("zvalues"));								// the values sended by the client(mostly gui client)
KEYDECL(ZFUN00Z, _T("zfun00z"));								// for convinience functiion one's id
KEYDECL(ZFUN01Z, _T("zfun01z"));								// for convinience functiion two's id
KEYDECL(ZNORESZ, _T("znoresz"));								// no result
KEYDECL(ZBYGATE, _T("zbygate"));								// function call through gate/proxy
KEYDECL(ZPARAMS, _T("zparams"));								// parameters
KEYDECL(ZUSEBAS, _T("zusebas"));								// we need to use basics values
KEYDECL(ZA2KILL, _T("za2kill"));								// number of session's slice to kill
KEYDECL(ZPRXTIM, _T("prxtime"));								// proxy's time
KEYDECL(ZSITTIM, _T("sittime"));								// site's time


// execution services
KEYDECL(ZSAVERR, _T("zsaverr"));								// flag if we must save the error
KEYDECL(ZORIVAL, _T("zorival"));								// original values
KEYDECL(ZSTATUS, _T("zstatus"));								// original values
//KEYDECL(ZAVFUNS, _T("zavfuns"));								// function availables for the transaction
KEYDECL(ZLISTDT, _T("zlistdt"));								// data related to the list data
KEYDECL(ZLISTAF, _T("listaff"));								// list affected , 0..MAX LIST HANDLED
KEYDECL(ZSAVSTA, _T("zsavsta"));								// save state ???
KEYDECL(ZDSPTCH, _T("ZDSPTCH"));								// dispatch one webservice
KEYDECL(ZLSTRES, _T("lastres"));								// last result
KEYDECL(ZUSRPRM, _T("zusrprm"));								// user parameters

KEYDECL(ZEXEDAT, _T("exedata"));								// execel data
KEYDECL(ZEXETOT, _T("exetot"));									// execel data
KEYDECL(ZEXECLS, _T("execols"));								// execel data
KEYDECL(ZLSTDAT, _T("lstdata"));								// text data
KEYDECL(ZLSTTOT, _T("lsttot"));									// text data
KEYDECL(ZLSTCLS, _T("lstcols"));								// text data
KEYDECL(ZTXTDAT, _T("txtdata"));								// text data
KEYDECL(ZTXTTOT, _T("txttot"));									// text data

// rights
KEYDECL(ZACCRGT, _T("acc"));									// access to target

// service internals
KEYDECL(ZSVRTYP, _T("svrtype"));	const int ZSVRTYPMAX = 3;	// server's type
KEYDECL(ZADOCON, _T("con_ado"));								// database connection string(internal)
KEYDECL(ZSYSTEM, _T("zsystem"));	const int ZSYSTEMMAX = 3;	// system
KEYDECL(CURPATH, _T("curpath"));								// current server path
KEYDECL(ZURGTSZ, _T("zurgtsz"));								// user rights

// server-client
//KEYDECL(ZFRMVER, _T("zfrmver"));								// framework version		
//KEYDECL(ZCLIVER, _T("version"));								// client version accepted
KEYDECL(ZCOREDB, _T("ZCOREDB"));								// control dabatabase
KEYDECL(ZGUIAGN, _T("ZGUIAGN"));								// gui agent
KEYDECL(ZBRWTYP, _T("ZBRWTYP"));								// gui version 
KEYDECL(ZPSTMSG, _T("ZPSTMSG"));								// on client post a windows message
KEYDECL(ZPSTMSI, _T("ZPSTMSI"));								// on client post a windows message id
KEYDECL(ZKEYPRE, _T("ZKEYPRE"));
KEYDECL(ZDSPPAG, _T("ZDSPPAG"));
KEYDECL(ZRIGHT1, _T("0right1"));	const int ZRIGHT1MAX = 3;	// particular right

// log
KEYDECL(ZZNLOGS, _T("nlogs"));									// how many entries in log (none means 1)
KEYDECL(ZZTOLOG, _T("zztolog"));								// log key ??
KEYDECL(ZTXTLOG, _T("ztxtlog"));								// log text
KEYDECL(ZKEYLOG, _T("zkeylog"));								// log key
KEYDECL(ZTYPLOG, _T("ztyplog"));								// log type
KEYDECL(ZSAVLOG, _T("zsavlog"));								// do we save the log

// history session
KEYDECL(ZHISPOS, _T("zhispos"));
KEYDECL(ZHISTRN, _T("zhistrn"));
KEYDECL(ZHISDSC, _T("zhisdsc"));
KEYDECL(ZHISTYP, _T("zhistyp"));
KEYDECL(ZHISSIZ, _T("zhissiz"));

// core services
KEYDECL(ZGATSVR, _T("gat_svr"));	const int ZGATSVRMAX = 15;	// gate server
KEYDECL(ZGATPRT, _T("gatport"));								// gate port
KEYDECL(ZIISSVR, _T("iis_svr"));	const int ZIISSVRMAX = 15;	// IIS server
KEYDECL(ZIISPRT, _T("iisport"));								// IIS port
KEYDECL(ZWEBSVR, _T("web_svr"));	const int ZWEBSVRMAX = 31;	// web server

KEYDECL(ZSVRGAT, _T("gat"));

KEYDECL(ZFUNCAL, _T("funcall"));
KEYDECL(ZSITCAL, _T("sitcall"));

// errors
KEYDECL(ZERRORI, _T("zerrori"));								// error information
KEYDECL(ZERRORM, _T("zierror"));								// error messsage information
KEYDECL(ZERRORS, _T("zlerror"));								// error stacktrace information
KEYDECL(ZSERROR, _T("error01"));								// error string
KEYDECL(ZCERROR, _T("errorc1"));								// error string whitout translation
KEYDECL(ZNERROR, _T("errorn1"));								// error code
KEYDECL(ZHERROR, _T("errorh1"));								// error helper
KEYDECL(ZNERRIN, _T("errorin"));
KEYDECL(ZNERRLO, _T("errorln"));
KEYDECL(ZIERROR, _T("errori1"));								// error additional information
KEYDECL(ZLERROR, _T("errorl1"));								// error location

KEYDECL(ZSWARNG, _T("warning"));								// warnning description

// core component
KEYDECL(ZCTROBJ, _T("mroctrl.SessionMan"));						// control component
KEYDECL(ZPROXYP, _T("proxy.ashx"));						// control component
KEYDECL(ZCOREPG, _T("core.aspx"));						// control component

// client keys
KEYDECL(ZORIPAS, _T("oripass"));								// original password
KEYDECL(ZDATTIM, _T("datetim"));								// server date time 
KEYDECL(ZCURFLD, _T("zcurfld"));	const int ZCURFLDMAX = 63;	// current field focused
KEYDECL(ZEVENTN, _T("zeventn"));	const int ZEVENTNMAX = 63;
KEYDECL(ZSESTIM, _T("zsestim"));
KEYDECL(ZSETCUR, _T("zsetcur"));
KEYDECL(ZGOTOBC, _T("zgotobc"));
KEYDECL(ZGOTODO, _T("zgotodo"));
KEYDECL(ZNLSTAF, _T("nlistaf"));

// framework transactions
KEYDECL(ZTHOME, _T("S001"));									// main moduless transaction
//KEYDECL(ZTSMAIN, _T("tsmain"));									// main moduless transaction (deprecated)
KEYDECL(ZTRNPAS, _T("S000"));									// password transaction
KEYDECL(ZCHGPAS, _T("S003"));									// cghange password transaction
KEYDECL(ZTRNDEV, _T("SE02"));									// edition transaction
KEYDECL(ZTRNDLG, _T("S015"));
KEYDECL(ZTRNFLI, _T("S010"));

// server keys
KEYDECL(REQCHKD, _T("reqchkd"));								// mark as a requisition already checked
KEYDECL(RETPRMS, _T("retprms"));								// parameters that we must return
KEYDECL(EVENTFN, _T("eventfn"));								// event function
KEYDECL(BYPROXY, _T("zbproxy"));	
KEYDECL(ZRESEND, _T("zresend"));								// end respond mark any complete webservice must have it

// download
KEYDECL(ZDOWNLD, _T("ZISDWLD"));								// download key
KEYDECL(ZNDOWNS, _T("ZNDWLDS"));								// how many downloads
KEYDECL(ZDWNFSV, _T("ZDWLFS0"));								// from server
KEYDECL(ZDWNFPA, _T("ZDWLFP0"));								// from path
KEYDECL(ZDWNFFL, _T("ZDWLFF0"));								// specific file
KEYDECL(ZDWNTFL, _T("ZDWLTF0"));								// to file
KEYDECL(ZDWNTPA, _T("ZDWLTP0"));								// to path
KEYDECL(ZDWNDIR, _T("ZDWLDI0"));								// to specific place
KEYDECL(ZDWNWIT, _T("ZDWLWI0"));								// with what? default will be the client 

KEYDECL(ZISSHEL, _T("zisshell"));								// shell key
KEYDECL(ZNSHELS, _T("zshellfiles"));							// how many files ???
KEYDECL(ZSHELLP, _T("zshellpath0"));							// path
KEYDECL(ZSHELLA, _T("zshellaction0"));							// action
KEYDECL(ZSHELLR, _T("zshellprms0"));							// parameters

//gui
KEYDECL(ZUPDCLS, _T("updclis"));								// client's params to be change
KEYDECL(ZLOCACT, _T("zlocact"));
KEYDECL(ZSHWDLG, _T("ZSHWDLG"));								// show dialog is need ?
KEYDECL(ZDLGTYP, _T("zdlgtyp"));								// show dialog is need ?
KEYDECL(ZRTRNCD, _T("rmodule"));	const int ZRTRNCDMAX = 32;
KEYDECL(ZRTRNPR, _T("rmodprm"));
KEYDECL(ZTYPTRN, _T("typtran"));	const int ZTYPTRNMAX = 16;	// transaction's type
KEYDECL(ZTYPRED, _T("typread"));
KEYDECL(ZTYPRGT, _T("tyright"));
KEYDECL(ZFILE01, _T("file_01"));	const int ZFILE01MAX = 32;
KEYDECL(ZFILERS, _T("xfile01"));
KEYDECL(PDOCTYP, _T("doctype"));    const int PDOCTYPMAX = 3;
KEYDECL(FILENAME, _T("filename"));
KEYDECL(ZSHRTCT, _T("shortcut"));
KEYDECL(ZFINFUN, _T("finalfun"));

// generic functions
KEYDECL(PAGE_MISSING,_T("codebehind_is_missing"));
KEYDECL(FUN_MISSING,_T("function_is_missing"));

// standar events
KEYDECL(ZDOEVNT, _T("doevent"));								// main executor
KEYDECL(ZONENTR, _T("onenter"));								// enter event
KEYDECL(ZONLOAD, _T("onload"));									// load transaction event
KEYDECL(ZONUNLD, _T("onunload"));								// unload transaction event
KEYDECL(ZONRELD, _T("onreload"));								// realod transaction event (need more desc)
KEYDECL(ZONLINS, _T("oninsert"));								// insert on list event
KEYDECL(ZONLUPD, _T("onedit"));									// update on list event
KEYDECL(ZONLDEL, _T("ondelete"));								// delete on list event
KEYDECL(ZONF8,	_T("onf8"));									// update on list event
KEYDECL(ZONINS, _T("onins"));								// delete on list event

// standard parameters
KEYDECL(PNOEMPT, _T("notempty"));								

// standard errors
KEYDECL(ESESLKU, _T("session_reset_lack_use"));

const int EVNSIZE=32;	const int  EVNMAX=31;
const int FUNSIZE=64;	const int  FUNMAX=63;
const int MODSIZE=32;	const int  MODMAX=31;
const int CMPSIZE=32;	const int  CMPMAX=31;
const int USRSIZE=10;	
const int PASSIZE=10;
const int TYCSIZE=4;	const int  TYCMAX=3;
const int SECSIZE=4;	const int  SECMAX=3;

const int LGTSIZE=128;	const int LGTMAX=127;
const int LGESIZE=32;	const int LGEMAX=31;
const int LGKSIZE=40;	const int LGKMAX=39;
const int LGPSIZE=2;	const int LGPMAX=1;

// For optional arguments
#define vOpt COleVariant((long)DISP_E_PARAMNOTFOUND, VT_ERROR)

typedef void (*SETDATA)(LPTSTR); 
typedef void (*SETDATA2)(LPTSTR, const int); 
typedef BSTR (*GETDATA)(); 
typedef TCHAR* (*GETDATA2)(int&); 
typedef bool (*FUNCTION)(LPTSTR); 
typedef bool (*FUNCTION2)(LPTSTR, const int); 

struct libfuns
{
	CString libname;

	HMODULE lib;
	SETDATA2 setbasics;
	SETDATA2 setdata;
	GETDATA2 getdata;
	GETDATA2 getchanges;
	FUNCTION2 function;
};

#define equal4(f, s)\
		cmp4ch(f, s[0], s[1], s[2], s[3]) 

#define equal6(f, s)\
		cmp4ch(f, s[0], s[1], s[2], s[3]) && cmp2ch(&f[4], s[4], s[5])

#define equal8(f, s)\
		cmp4ch(f, s[0], s[1], s[2], s[3]) && cmp4ch(&f[4], s[4], s[5], s[6], s[7])

#if!defined COMPAREFUNS
#define COMPAREFUNS

typedef bool (*compfun)(const TCHAR* pp, const TCHAR* key);

static bool c1(const TCHAR* p, const TCHAR* k)
{ return p[0] == k[0]; }
static bool c2(const TCHAR* p, const TCHAR* k)
{ return cmp2ch(p, k[0],k[1]); }
static bool c3(const TCHAR* p, const TCHAR* k)
{ return cmp2ch(p, k[0],k[1]) && p[2] == k[2]; }
static bool c4(const TCHAR* p, const TCHAR* k)
{ return cmp4ch(p, k[0],k[1],k[2],k[3]); }
static bool c5(const TCHAR* p, const TCHAR* k)
{ return cmp4ch(p, k[0],k[1],k[2],k[3]) && p[4] == k[4]; }
static bool c6(const TCHAR* p, const TCHAR* k)
{ return equal6(p, k); }
static bool c7(const TCHAR* p, const TCHAR* k)
{ return equal6(p, k) && p[6] == k[6]; }
static bool c8(const TCHAR* p, const TCHAR* k)
{ return equal8(p, k); }
static bool c9(const TCHAR* p, const TCHAR* k)
{ return equal8(p, k) && p[8] == k[8]; }
static bool c10(const TCHAR* p, const TCHAR* k)
{ return equal8(p, k) && cmp2ch(&p[8], k[8], k[9]); }
static bool c11(const TCHAR* p, const TCHAR* k)
{ return equal8(p, k) && cmp2ch(&p[8], k[8], k[9]) && p[10] == k[10]; }
static bool c12(const TCHAR* p, const TCHAR* k)
{ return equal8(p, k) && cmp4ch(&p[8], k[8],k[9],k[10],k[11]); }
static bool c13(const TCHAR* p, const TCHAR* k)
{ return equal8(p, k) && cmp4ch(&p[8], k[8],k[9],k[10],k[11]) && p[12] == k[12]; }
static bool c14(const TCHAR* p, const TCHAR* k)
{ return equal8(p, k) && cmp4ch(&p[8], k[8],k[9],k[10],k[11]) && cmp2ch(&p[12], k[12], k[13]); }
static bool c15(const TCHAR* p, const TCHAR* k)
{ return equal8(p, k) && cmp4ch(&p[8], k[8],k[9],k[10],k[11]) && cmp2ch(&p[12], k[12], k[13]) && p[14] == k[14]; }
static bool c16(const TCHAR* p, const TCHAR* k)
{ return equal8(p, k) && cmp4ch(&p[8], k[8],k[9],k[10],k[11]) && cmp4ch(&p[12], k[12], k[13], k[14], k[15]); }

static bool c17(const TCHAR* p, const TCHAR* k)
{ return equal8(p, k) && cmp4ch(&p[8], k[8],k[9],k[10],k[11]) && 
		cmp4ch(&p[12], k[12], k[13], k[14], k[15]) && p[16] == k[16]; }
static bool c18(const TCHAR* p, const TCHAR* k)
{ return equal8(p, k) && cmp4ch(&p[8], k[8],k[9],k[10],k[11]) && 
		cmp4ch(&p[12], k[12], k[13], k[14], k[15]) && cmp2ch(&p[16], k[16], k[17]); }
static bool c19(const TCHAR* p, const TCHAR* k)
{ return equal8(p, k) && cmp4ch(&p[8], k[8],k[9],k[10],k[11]) && 
		cmp4ch(&p[12], k[12], k[13], k[14], k[15]) && cmp2ch(&p[16], k[16], k[17]) && p[18] == k[18]; }
static bool c20(const TCHAR* p, const TCHAR* k)
{ return equal8(p, k) && cmp4ch(&p[8], k[8],k[9],k[10],k[11]) && 
		cmp4ch(&p[12], k[12], k[13], k[14], k[15]) && cmp4ch(&p[16], k[16],k[17],k[18],k[19]); }
static bool c21(const TCHAR* p, const TCHAR* k)
{ return equal8(p, k) && cmp4ch(&p[8], k[8],k[9],k[10],k[11]) && 
		cmp4ch(&p[12], k[12], k[13], k[14], k[15]) && 
		cmp4ch(&p[16], k[16],k[17],k[18],k[19]) && p[20] == k[20]; }
static bool c22(const TCHAR* p, const TCHAR* k)
{ return equal8(p, k) && cmp4ch(&p[8], k[8],k[9],k[10],k[11]) && 
		cmp4ch(&p[12], k[12], k[13], k[14], k[15]) && 
		cmp4ch(&p[16], k[16],k[17],k[18],k[19]) && cmp2ch(&p[20], k[20], k[21]); }
static bool c23(const TCHAR* p, const TCHAR* k)
{ return equal8(p, k) && cmp4ch(&p[8], k[8],k[9],k[10],k[11]) && 
		cmp4ch(&p[12], k[12], k[13], k[14], k[15]) && 
		cmp4ch(&p[16], k[16],k[17],k[18],k[19]) && cmp2ch(&p[20], k[20], k[21]) && p[22]==k[22];}
static bool c24(const TCHAR* p, const TCHAR* k)
{ return equal8(p, k) && cmp4ch(&p[8], k[8],k[9],k[10],k[11]) && 
		cmp4ch(&p[12], k[12], k[13], k[14], k[15]) && 
		cmp4ch(&p[16], k[16],k[17],k[18],k[19]) && cmp4ch(&p[20], k[20], k[21],k[22],k[23]);}
static bool c25(const TCHAR* p, const TCHAR* k)
{ return equal8(p, k) && cmp4ch(&p[8], k[8],k[9],k[10],k[11]) && 
		cmp4ch(&p[12], k[12], k[13], k[14], k[15]) && 
		cmp4ch(&p[16], k[16],k[17],k[18],k[19]) && 
		cmp4ch(&p[20], k[20], k[21],k[22],k[23]) && p[24] == k[24];}
static bool c26(const TCHAR* p, const TCHAR* k)
{ return equal8(p, k) && cmp4ch(&p[8], k[8],k[9],k[10],k[11]) && 
		cmp4ch(&p[12], k[12], k[13], k[14], k[15]) && 
		cmp4ch(&p[16], k[16],k[17],k[18],k[19]) && 
		cmp4ch(&p[20], k[20], k[21],k[22],k[23]) && cmp2ch(&p[24], k[24], k[25]);}

static bool c27(const TCHAR* p, const TCHAR* k) { return _tmemcmp(p,k,27) == 0; }
static bool c28(const TCHAR* p, const TCHAR* k) { return _tmemcmp(p,k,28) == 0; }
static bool c29(const TCHAR* p, const TCHAR* k) { return _tmemcmp(p,k,29) == 0; }
static bool c30(const TCHAR* p, const TCHAR* k) { return _tmemcmp(p,k,30) == 0; }
static bool c31(const TCHAR* p, const TCHAR* k) { return _tmemcmp(p,k,31) == 0; }
static bool c32(const TCHAR* p, const TCHAR* k) { return _tmemcmp(p,k,32) == 0; }

static bool cx(const TCHAR* p, const TCHAR* k) { return _tcscmp(p, k) == 0; }

static compfun cfuns[64] = { nullptr, 
						&c1,&c2,&c3,&c4,&c5,&c6,&c7,&c8,&c9,&c10,
						&c11,&c12,&c13,&c14,&c15,&c16,&c17,&c18,&c19,&c20,
						&c21,&c22,&c23,&c24,&c25,&c26,&c27,&c28,&c29,&c30,
						&c31,&c32,
&cx,&cx,&cx,&cx,&cx,&cx,&cx,&cx,
&cx,&cx,&cx,&cx,&cx,&cx,&cx,&cx,&cx,&cx,
&cx,&cx,&cx,&cx,&cx,&cx,&cx,&cx,&cx,&cx,
&cx,&cx,&cx}; 
#endif

#if!defined MROHTTOI
#define MROHTTOI
static int _httoi(const TCHAR *value)
{
  struct CHexMap { TCHAR chr; int value; };
  static CHexMap HexMap[] =
  {	
	{'A',10},{'B',11},{'C',12},{'D',13},{'E',14},{'F',15},
	{'0', 0},{'1', 1},{'2', 2},{'3', 3},{'4', 4},{'5', 5},{'6',6},{'7',7},{'8',8},{'9',9},
	{'a',10},{'b',11},{'c',12},{'d',13},{'e',14},{'f',15}, {0,0}
  };

  register const TCHAR *s = value;
  int result = 0;

  register CHexMap* pm = nullptr;
  bool firsttime = true;
  register TCHAR l = 0;
  register TCHAR c = 0;
  while (l=*s)
  {
	bool found = false;
	for(pm = HexMap; c = pm->chr; ++pm)
	{
	  if (l == c)
	  {
		if (!firsttime) result <<= 4;
		result |= pm->value;
		found = true;
		break;
	  }
	}
	if (!found) break;
	++s;
	firsttime = false;
  }
  return result;
}
#endif
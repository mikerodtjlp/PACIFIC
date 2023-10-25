#include "stdafx.h"

/************************************************************************************
* file			: mrosvr.cpp
* description   : main mro server file
* purpose       : main engine for execute the transactions from the clients
* author        : miguel rodriguez
* date creation : 20 junio 2004
**************************************************************************************/

#include "mrosvr.h"
#include "exec.h"
#include "json.hpp"

using json = nlohmann::json;

#ifdef _DEBUG
#define new DEBUG_NEW
#endif

#define STP (s++)
DWORD info = EVENTLOG_INFORMATION_TYPE;

// helpers mostly statistics
ULONGLONG		gstarttime;
ULONGLONG   gprocesstime;
long        gaccessno;
long				gprocessesused;
long        gexecutedfuns;
long				gwaitinghits;
long				grejectedhits;
long				grejectedreqs;
long				gfreesdone;
long				gtolerance;
long				gchildtol;
int					gmaxdbcons;

// special variables that once the system is running never change
TCHAR       gcfgpcom[128];
TCHAR				gcurpath[MAX_PATH];		
TCHAR				gwork[512];
int					gworklen;
CParameters gservers;
CParameters	gsvrgrps;
CParameters gdefgrps;
TCHAR				gappstr[256];
TCHAR				gcorstr[256];
TCHAR				gerrload[256];
TCHAR				gsvrtype[4];
int					gexectyp;
TCHAR				gdomain[4];
TCHAR				gexename[32];

// vairables for the this server and for the administration server 
TCHAR       localsvr[16];
int					localsvrlen;
int         localprt;
TCHAR				gatsvr[16];
int					gatsvl;
int					gatprt;

// some services are specials
bool				loginit;
bool				debugmode;

TCHAR				funnum[MAXFUNSPROC][8];

// thread's priority
int					PRIO_LST;
int					PRIO_DSP;
int					PRIO_SCH;
int					PRIO_PRC;
int					PRIO_RSP;
int					PRIO_NOT;

bool				ajst_lst = true;
bool				ajst_dsp = true;
bool				ajst_sch = true;
bool*				ajst_prc = nullptr;
bool				ajst_rsp = true;
bool				ajst_not = true;

DWORD				ggapreqs = 0;
bool				debug_mode = false;

/** 
 *	implementation:
 *
 *	the basic idea is to have a thread called LISTENER (that actually is the main 
 *	thread) that receives requests from clients, then put them in a temporary queue; 
 *	then other 	specific thread called DISPATCHER waits for any request in that queue 
 *	in order to read the request and transform it into a workable object and send 
 *	a signal when that, then other thread called SCHEDULER waits for any request in 
 *	that queue that was processed, and when got one, it look for a place in other
 *	queue that contains cpus (threads from other queue that is a pool of threads)
 *	when we got boths we activate a signal that dispatch that cpu, that cpu is waiting
 *	for an activation, when that activation is signaled the cpu reads the already
 *	process request, interpret the header and basics, and execute some components,
 *	that components could be WIN32 COMS, direct sql, WIN32 dlls the components are 
 *	executed with 5 standard functions, one: passing the basics parameters, two: 
 *	passing the execution parameters, three: execution the final function, four: 
 *	retriving the result and five retriving information that have to change; the 
 *	result is sent directly to	the client, and then close the connection, then at 
 *	this point the process/cpu is completed and ready for be used for a new request.
 */

void goodbye(	const TCHAR* message, const int msglen, 
							const TCHAR* extrainfo, const int extlen) {
	TCHAR name[256];
	int len = ::GetModuleFileName(0, name, 255);
	int i = len;
	for(; i > 0; --i) 
		if(name[i] == '\\') { 
			++i; 
			break; 
		}

	TCHAR exe[1024];
	_tmemcpy(exe, &name[i], (len-i)+1);
	i=(len-i);
	exe[i++]= '-';

	_tmemcpy(&exe[i], message, msglen); i += msglen;
	if(extlen) {
		set2ch(&exe[i], '-',' '); i += 2;
		_tmemcpy(&exe[i], extrainfo, extlen); i += extlen;
	}
	exe[i] = 0; // end of final message
	write2event(EVENTLOG_ERROR_TYPE, exe); 

	// show stucked processes
	for (register int index = 0; index < MAX_CLIES; ++index) {
		sClients& clie = clies[index];
		if (clie.procid == -1) continue;
		mikefmt(exe, _T("%s-%s-%s : %s-%s-%s : %d-%d"),
			clie.machine,clie.user,clie.trans,
			/*clie.restop ? clie.typecom[clie.restop - 1] :*/ _T(""),
			/*clie.restop ? clie.component[clie.restop - 1] :*/ _T(""),
			clie.restop ? clie.function[clie.restop - 1] : _T(""),
			clie.step, procs[clie.procid].status);
			write2event(EVENTLOG_ERROR_TYPE, exe);
	}

	WSACleanup();
	exit(0);
}

void initialize_com_engine() {
	// very important: we initialize the com libraries to run threads that do not block
	// during calls, and for this, the coms must be in "both" mode, this is indispansable
	require(::CoInitializeEx(0, COINIT_MULTITHREADED) != S_OK, _T("could not load com libraries"));
}

void initialize_sockets() {
	WSADATA wsaData;
	require(WSAStartup(MAKEWORD(2,0),(LPWSADATA)&wsaData), _T("could not load socket libraries"));
}

/**
 *	this functions bassically deals with the parameters
 *	example:
 *	[domain:002][svrtype:app][ismain:0][isgate:0][maxprcs:0][timetol:60]
 */
void process_command_line() {
	// mainly for debugging, cause is very important to know where we are
	//if(!p.get(_T("workpath"), gcurpath, 8, MAX_PATH-1))			
		GetCurrentDirectory(MAX_PATH, gcurpath);				

	loginit		  = true;//p.isactv	(_T("loginit")	, 7);
	debugmode	  = false;// p.isactv(ZISDEBG, ZISDEBGLEN);

	MAX_PROCS	  = 16;// p.getint(_T("maxprcs"), 7);
	MAX_CLIES	  = 64;// p.getint(_T("maxclis"), 7);
	DEF_CLXPR	  = 8;// p.getint(_T("defcxpr"), 7);

	PRIO_LST	  = 1;// p.getint(_T("priolst"), 7);
	PRIO_DSP	  = 1;// p.getint(_T("priodis"), 7);
	PRIO_SCH	  = 0;// p.getint(_T("priosch"), 7);
	PRIO_PRC	  = 0;// p.getint(_T("prioprc"), 7);
	PRIO_RSP	  = 1;// p.getint(_T("priores"), 7);
	PRIO_NOT	  = -1;// p.getint(_T("priontf"), 7);

	ggapreqs	  = 0;// p.getlong(_T("gapreqs"), 7);
	gexectyp	  = 1;// p.getint(_T("exectyp"), 7);

	MAX_SBSIZ	  = 0;// p.getint(_T("maxsbsz"), 7);
	DEF_SBSIZ	  = 0;// p.getint(_T("defsbsz"), 7);

	gtolerance	= 60 * (debug_mode ? 15 : 5);// p.getint(_T("timetol"), 7);
	gchildtol	  = 140;// p.getint(_T("chldtol"), 7);
	gmaxdbcons	= 16;// p.getint(_T("maxdbcn"), 7, 16);

	// default values
	if(DEF_CLXPR  == 0) DEF_CLXPR	= 4;
	if(DEF_SBSIZ  == 0) DEF_SBSIZ	= 1024;

	// handling values
	if(MAX_PROCS  == 0) MAX_PROCS	 = DEF_MAX_PROCS;
	if(MAX_CLIES  == 0) MAX_CLIES	 = MAX_PROCS * DEF_CLXPR;
	if(MAX_CLIES > 128)	MAX_CLIES	 = 128; // MAXIMUM_WAIT_OBJECTS * 2
	if(MAX_SBSIZ  == 0) MAX_SBSIZ	 = DEF_SBSIZ;
	if(ggapreqs	  == 0) ggapreqs	 = 64;
	if(gtolerance == 0) gtolerance = 180;
	if(gchildtol  == 0) gchildtol	 = gtolerance;

	// get the domain(configuration)
	//require(!p.get(_T("domain")	, gdomain, 6, 3)			, _T("lack_domain_type"));
	set4ch(gdomain, '0', '0', '2', 0);
	// get the server type(app, log, rep,etc...)
	//require(!p.get(ZSVRTYP, gsvrtype, ZSVRTYPLEN,ZSVRTYPMAX), _T("lack_server_type"));
	set4ch(gsvrtype, 'g', 'a', 't', 0);
}

bool get_config_file(	const TCHAR* filename, 
											const TCHAR* appname, 
											CParameters& whole) {
	// check the current directory in order to get the drive
	TCHAR directory[MAX_PATH]; 
	GetCurrentDirectory(MAX_PATH, directory);
	TCHAR configfile[64];
	mikefmt(configfile, _T("%s\\cfgs\\%s"), directory, filename);

	CString rparams;
	if(mro::exist_file(configfile))	{
		CString helper;
		CUTF16File inputFile;
		inputFile.Open(configfile, CFile::modeRead);
		for(;;)	{
			if(inputFile.ReadString(helper) == false) break;
			rparams.Append(helper.GetBuffer(), helper.GetLength());
		}
		inputFile.Close();
		whole.set_value(rparams);
		return true;
	}
	else { 
		TCHAR log[1024];
		mikefmt(log, _T("%s - error could not find %s"), appname, configfile);
		mro::write_event(EVENTLOG_ERROR_TYPE, log);

		mikefmt(log, _T("%s - end"), appname);
		mro::write_event(EVENTLOG_INFORMATION_TYPE, log);
	}
	whole.clear();
	return false; 
}

void update_addresses(CParameters& addresses, CParameters& whole) {
	CString temp = whole.buffer();
	TCHAR ckey[64];
	int clk = 0;
	TCHAR cval[64];
	int clv = 0;
	int nkeys = addresses.nkeys();
	for(int i = 0; i<nkeys; ++i) {
		if(addresses.getpair(i, ckey, cval, clk, clv, 63, 63)) {
			temp.Replace(ckey,cval);
		}
	}
	whole.set_value(temp);
}

/**
 *	this function get the working data from the main server or if it is the main 
 *	server it read it from the configuration file, the working data retreive includes
 *	very important stuff like db connection string, ip of other server, etc...
 */
void initialize_server(/*CParameters& config*/) {
	/*CParameters addresses;
	bool found =  get_config_file(_T("addresses.txt"), gexename, addresses);
	if(!found) return;

	TCHAR file[1024];
	mikefmt(file, _T("%s\\cfgs\\mro%s.txt"), gcurpath, gdomain);
	require(mro::exist_file(file) == false, CString(file));
	CString temp;
	CUTF16File inputFile;
	inputFile.Open(file, CFile::modeRead);
	for(;inputFile.ReadString(file,1023);temp.Append(file));
	inputFile.Close();
	config.set_value(temp.GetBuffer(), temp.GetLength());

	update_addresses(addresses, config);

	require(config.isempty()				, _T("config_file_is_empty")	 );
	require(config.has(ZSERROR, ZSERRORLEN)	, config.get(ZSERROR, ZSERRORLEN));*/
}

/**
 *	this function prepare, reads or manipulate information for this specific server
 *	like its address, the log server that it has to access, the system, bassically
 *	process and takes the information needed from the data received from the server
 */
void initialize_local(/*CParameters& config,*/ int& s) {
write2event(info,_T("extract server configuration"), s); STP;
	// require(!config.get(_T("servers"), gservers, 7), _T("lack_of_servers"));
	gservers.set_value(_T("[gat_svr:127.0.0.1][gatport:7501][hlp_svr:127.0.0.1][web_svr:127.0.0.1:7000]"));

	//config.get(_T("svrgrps"), gsvrgrps, 7);
	//if(!gsvrgrps.get(_T("default"), gdefgrps, 7))
	//	gdefgrps.set_value(gsvrgrps);

write2event(info,_T("get local address"), s); STP;
	// we get the local server and port for further use 
	_tcscpy_s(localsvr, synservice::GetIpAddress());
	localsvrlen = _tcslen(localsvr);
	TCHAR item[64];
	int len = mikefmt(item, _T("%sport"), gsvrtype);
	localprt = gservers.getint(item, len);

write2event(info,_T("extract gat data"), s); STP;
	gatsvl = gservers.get(ZGATSVR, gatsvr, ZGATSVRLEN, ZGATSVRMAX);
	gatprt = gservers.getint(ZGATPRT, ZGATPRTLEN);
	if(is_127001(gatsvr, gatsvl))	{
		_tmemcpy(gatsvr, localsvr, localsvrlen + 1);
		gatsvr[gatsvl = localsvrlen] = 0;
	}

	// we get the system for the services works for
	defchar(system, ZSYSTEMMAX+1);
	// config.get(ZSYSTEM,system,ZSYSTEMLEN,ZSYSTEMMAX);
	_tcscpy_s(system, _T("LOC"));

	// we get the deafult database if it is empty, from the connection string 
	TCHAR connpwd[512];
	// config.get(_T("catlpwd"), connpwd, 7, 255);
	_tcscpy_s(connpwd, _T("Provider=SQLOLEDB.1;Password=mro;Persist Security Info=False;User ID=mro;Initial Catalog=CORE;Data Source=(local)\\sqlexpress01;MARS Connection=True"));

	// we get the deafult database if it is empty, from the connection string 
	CString cor;
	//config.get(_T("connctr"), cor, 7);
	cor = _T("Provider=SQLOLEDB.1;Password=mro;Persist Security Info=False;User ID=mro;Initial Catalog=CORE;Data Source=(local)\\sqlexpress01;MARS Connection=True");

	// we get the deafult database if it is empty, from the connection string 
	CString app;	
	//config.get(_T("connstr"), app, 7);
	app = _T("Provider=SQLOLEDB.1;Password=mro;Persist Security Info=False;User ID=mro;Initial Catalog=CORE;Data Source=(local)\\sqlexpress01;MARS Connection=True");

	// we get the specific service connection string if any
	//len = mikefmt(item, _T("conn%s"), gsvrtype);
	//if(config.has(item, len))
	//	config.get(item, app, len);

write2event(info, _T("get passwords"), s); STP;
write2event(info, connpwd, s); STP;
	Table obj;
	cConnection con;
	if(con.Open(_T(""),_T(""), connpwd, 8))	{
		TCHAR key[32];
		TCHAR val[32];
		con.execute(_T("exec get_mro_pass;"), obj);
		for(;!obj.IsEOF();obj.MoveNext())
		{
			obj.get(0,key,31);//if(!obj.get(0,key,31)) continue;
			obj.get(1,val,31);//if(!obj.get(1,val,31)) continue;
			cor.Replace(key,val);
			app.Replace(key,val);
write2event(info, CString(key) + _T(":") + CString(val), s); STP;
		}
	}
else write2event(info, con.get_err(), s); STP;

	bool connected = con.m_Cnn != NULL;
	con.Close();
	ensureex(!connected, _T("could not get security"), CString(connpwd));

	
	_tmemcpy(gcorstr, cor.GetBuffer(), cor.GetLength() + 1);
	_tmemcpy(gappstr, app.GetBuffer(), app.GetLength() + 1);

	// which is our main webport 
	int webport = 0;// params.getint(_T("webport"), 7);
	if(webport == 0) webport = 80;

	// which is our current ipaddress
	defchar(curipad, 64);
	//params.get(_T("curipad"), curipad, 7, 63);

	// optionals variables for the component
	//defchar(cfgpercom, 256);
	//params.get(_T("cfgpcom"), cfgpercom, 7, 255);

write2event(info,_T("forming component basic data"), s); STP;
#if defined(ENVIRONMENT64)
	// this working data never change during all the execution, and it is passed always to all processes
	gworklen = mikefmt(	gwork,	_T("[curipad:%s][webport:%d][%s:%s][locaddr:%s][locport:%d][%s:%s][%s:%d]")
								_T("[%s:%s][%s:%s][svrtype:%s][maxprcs:%d][timetol:%d][dbm:%llu][maxdbcn:%d]%s"), 
								curipad,
								webport,
								ZSYSTEM, system, 
								localsvr, localprt,  
								ZGATSVR, gatsvr, ZGATPRT, gatprt, 
								CURPATH, gcurpath, 
								ZADOCON, gappstr, 
								gsvrtype, MAX_PROCS, gtolerance, dbhelper::dbgbl, gmaxdbcons, 
								debugmode ? _T("[isdebug:1]") : _T("")/*,cfgpercom*/);
#endif
#if defined(ENVIRONMENT32)
	// this working data never change during all the execution, and it is passed always to all processes
	gworklen = mikefmt( gwork,	_T("[curipad:%s][webport:%d][%s:%s][locaddr:%s][locport:%d][%s:%s][%s:%d]")
								_T("[%s:%s][%s:%s][svrtype:%s][maxprcs:%d][timetol:%d][dbm:%ld][maxdbcn:%d]%s"),
								curipad,
								webport,
								ZSYSTEM, system,
								localsvr, localprt,
								ZGATSVR, gatsvr, ZGATPRT, gatprt,
								CURPATH, gcurpath,
								ZADOCON, gappstr,
								gsvrtype, MAX_PROCS, gtolerance, dbhelper::dbgbl, gmaxdbcons,
								debugmode ? _T("[isdebug:1]") : _T("")/*,cfgpercom*/);
#endif

	ensure(gworklen >= 512, _T("internal cfg error"));
}

extern stats statistics[STATISTICS_SIZE];
extern int statstop;
void collects_statistics() {
	COleDateTime now = COleDateTime::GetCurrentTime();
	static UINT process_runned		= 0;
	static UINT clients_accepted	= 0;
	static UINT functions_executed	= 0;

	if(statstop == STATISTICS_SIZE) statstop = 0;
	statistics[statstop].process_runned		= gprocessesused - process_runned;
	statistics[statstop].clients_accepted	= gaccessno - clients_accepted;
	statistics[statstop].functions_executed = gexecutedfuns - functions_executed;
	time(&statistics[statstop].when);
	++statstop;

	process_runned		= gprocessesused;
	clients_accepted	= gaccessno;
	functions_executed	= gexecutedfuns;
}

void check_4_update() {
	TCHAR n[128];	int ln = GetModuleFileName(0, n, 128);
	TCHAR o[128];	_tmemcpy(o, n, ln + 1);
	set4ch(&o[ln-4], 'X','X','X','.'); set4ch(&o[ln], 'e','x','e',0); 
	TCHAR y[128];	_tmemcpy(y, n, ln + 1);
	set4ch(&y[ln-4], 'Y','Y','Y','.'); set4ch(&y[ln], 'e','x','e',0); 

	if(mro::exist_file(o) && mro::exist_file(n)) {
		_tremove(y);
		_trename(o,y);
		WSACleanup();
		exit(0); // graceful_shutdown();
	}
}

unsigned __stdcall backgroud(LPVOID pParam) {
	HANDLE thrd = ::GetCurrentThread();
	for(UINT cycle = 0;;Sleep(1000*2),++cycle) {
		try {
			if(ajst_not) { 
				SetThreadPriority(thrd, PRIO_NOT); ajst_not = false; 
			}

			//if(cycle % 11 == 0 && !debugmode)	{ check_state();			continue; }
			if(cycle % 11 == 0)					{ check_state();			continue; }
			if(cycle % 17 == 0)					{ check_4_update();			continue; }
			if(cycle % 31 == 0)					{ collects_statistics();	continue; }
		}
		catch(_com_error &e)	{				}
		catch(CException *e)	{ e->Delete();	}
		catch(mroerr& e)		{				}
		catch(...)				{				}
	}
}

void initialize_start() {
	gstarttime = GetTickCount64();	// we start counting the time 
	process_command_line();			// read the command line and find out the configuration
}

SOCKET sockl;
int APIENTRY _tWinMain(	HINSTANCE hInstance, 
						HINSTANCE hPrevInstance, 
						LPTSTR lpCmdLine, 
						int nCmdShow) {
	// debug_mode = false;
	TCHAR* p = _tcsstr(lpCmdLine, _T("DEBUG"));
	debug_mode = p ? true : false;
	if (debug_mode) AfxMessageBox(_T("block for debugging"));

	get_exename(gexename);			// get the executable name of this service/node

	defchar(error, 512);
	defchar(extra, 512);

	write2event(info, _T("start"), 0);	

	int s = 1;

	try	{
		json object = { {"one", 1}, {"two", 2} };

		_tcscpy_s(gcfgpcom, lpCmdLine);

		// CParameters cfg;

		write2event(info, lpCmdLine, s);

		if(loginit) write2event(info,_T("process cmdline"), s);	
		STP; initialize_start();		// basics initilization	

		if(loginit) write2event(info,_T("init com engine"), s);	
		STP; initialize_com_engine();	// initialize the com libraries

		if(loginit) write2event(info,_T("init coms"), s);	
		STP; initialize_com();			// load win32 COM objects

		if(loginit) write2event(info,_T("init sql"), s);	
		STP; initialize_sql();			// load sql connections

		if(loginit) write2event(info,_T("init sockets channels"), s);	
		STP; initialize_sockets();		// initialize the sockets libraries

		if(loginit) write2event(info,_T("init memory manager"), s);	
		STP; initialize_mem_manager();	// create a memory pool for optimization

		if(loginit) write2event(info,_T("config server"), s);	
		STP; initialize_server(/*cfg*/);	// connect to control server to get information

		if(loginit) write2event(info,_T("config local"), s);	
		initialize_local(/*cfg,*/ s);	STP; // get the configuration for this particular node

		if(loginit) write2event(info,_T("init request"), s);	
		STP; initialize_requests();		// initialize the request structures

		if(loginit) write2event(info,_T("init processes"), s);	
		STP; initialize_processes();	// initialize the processes structures

		if(loginit) write2event(info,_T("prepare socket"), s);	
		STP; initialize_socket(sockl);	// we create the socket reader

		if(loginit) write2event(info,_T("prepare sql connections"), s);	
		if(loginit) write2event(info,gcorstr, s);	
		if(loginit) write2event(info,gappstr, s);	
		STP; initialize_sql_connections();	// initialize the sql connections and it's caches

		if(loginit) write2event(info,_T("init log"), s);	
		STP; initialize_log(actionlog);	// initialize the engine to manage errors

		if(loginit) write2event(info,_T("init functions"), s);	
		STP; initialize_funs();			// we create our functions cache

		if(loginit) write2event(info,_T("prepare threads"), s);	
		STP; initialize_threads();		// we create the threads for all the processing

		write2event(EVENTLOG_SUCCESS,_T("run"), s);	
		listener(sockl);				// main loop for accepting requests
	}
	catch(CString& e)		{	_tcscpy_s(error, e); }
	catch(TCHAR* e)			{	_tcscpy_s(error, e); }
	catch(_com_error &e){ _tcscpy_s(error, _T("_com_error"));	}
	catch(mroerr& e)		{	_tcscpy_s(error, e.description);		
												_tcscpy_s(extra, e.extrainfo); }
	catch(CException *e){	TCHAR d[512];
												e->GetErrorMessage(d,512);
												e->Delete();
												_tcscpy_s(error, d); }
	catch(...)					{	_tcscpy_s(error, _T("internal error"));	}

	// what step was the last one that was fullfilled
	int el =_tcslen(error);
	if(el > 511) el = 511;
	_ltot(s, &error[el], 10);

	int xl =_tcslen(extra);
	if(xl > 511) xl = 511;
	_ltot(s, &extra[xl], 10);

	goodbye(error, el, extra, xl);
	return 0;
}

#if !defined(AFX_SESSIONMAN_H__BE00A09E_0C66_4611_A122_4FC586BA3B10__INCLUDED_)
#define AFX_SESSIONMAN_H__BE00A09E_0C66_4611_A122_4FC586BA3B10__INCLUDED_

#if _MSC_VER > 1000
#pragma once
#endif // _MSC_VER > 1000
// SessionMan.h : header file
//

#include "mrofns.h"

/////////////////////////////////////////////////////////////////////////////
// CSessionMan command target

extern int MAXHISTORY;
extern int MAXSESSIONS;
extern int MAXUSERS;

const int DEFMAXHISTORY	= 32;
const int DEFMAXSESSIONS= 8;
const int DEFMAXUSERS	= 16;
const int DEFMAXMACHINES= 256;
const int DEFMAXNAMES	= 512;
const int DEFMAXTRANS	= 1024;

const int SESSTIMEOUT = 60;

// MDS
struct transactions
{
	void init()
	{
		name[len = 0] = 0;
	}
	TCHAR name[CMPSIZE];
	int len;
};
extern transactions* trans;
extern int MAXTRANS;
extern int trntop;
struct nombres
{
	void init()
	{
		name[len = 0] = 0;
	};
	TCHAR name[USRSIZE + 1];
	int len;
};
extern nombres* names;
extern int MAXNAMES;
extern int namtop;
// MDS

struct historia
{
	void init() 
	{ 
		trn = nullptr;
	}
	transactions* trn;
};
struct usuario;
struct session
{
	void init() 
	{ 
		online = false; 
		reseted = false; 
		hispos = -1; 
		histop = -1; 
		cmpy   = -1;
		agetyp = -1;
		ismobi = -1;
	}
	COleDateTime start;
	COleDateTime lastcontact;
	historia* history;
	int histop;
	int hispos;
	int access;
	bool online;
	bool reseted;
	int cmpy;
	int agetyp;
	int ismobi;
};
struct usuario
{
	void init() 
	{ 
		sestop=0; 
		sestime=SESSTIMEOUT;
		id = nullptr;
		tmplparms=nullptr; 
	};
	int access;
	int sestop;
	int sestime;
	nombres* id;
	CParameters* tmplparms;
	session* sessions;
};
struct maquina
{
	void init() 
	{ 
		usrtop=0;
		name[naml=0]=0;
		ip[adrl=0]=0;
		macaddr[mcal=0]=0;
	}
	TCHAR name[ZMACNAMMAX+1];
	TCHAR ip[ZIPADDRMAX+1];
	TCHAR macaddr[ZMACADRMAX+1];
	int naml;
	int adrl;
	int mcal;
	int usrtop;
	usuario* users;
};

extern maquina* machines;
extern int MAXMACHINES;
extern int mactop;
extern int instid;

class CSessionMan {

public:
	CSessionMan();           // protected constructor used by dynamic creation

// Attributes
public:
	void SetParameters(LPCTSTR params, const int len);
	BSTR GetParameters();
	bool DoOk(LPCTSTR fun, const int len);
	void SetBasics(LPCTSTR basics, const int len);
	BSTR GetValuesToChange();

	void reset_state();
	void _get_functions_info();

	CParameters _params;
	CParameters _basics;
	CParameters _values;
	bool newbasics;
	CString callstack;

	typedef void (CSessionMan::* FP)();
	struct FPD {
		FPD() : function(0), executions(0), errors(0) {};
		FPD(FP fun, unsigned long execs) : function(fun), executions(execs), errors(0) {};
		TCHAR name[64];
		FP function;
		unsigned long executions;
		unsigned long errors;
	};
	FPD* function;
	static bool firsttime;

	static FPD _map_functions[MAXFUNCTIONS];
	static int _max_funs;
	bool component_initialization();
	void component_set_basics();
	void component_post();
	static bool loaded;
	static bool initialized;
	void process_error(	const TCHAR* text, 
						const TCHAR* extra, 
						const TCHAR* info,
						const TCHAR* callstack,
						const TCHAR* funname,
						const TCHAR* errfile, 
						const int errline, const 
						TCHAR* errclass);

	static map<CString, CString> cache_docs;
	static CRITICAL_SECTION cstrans;
	static CRITICAL_SECTION sestrns;
	static CRITICAL_SECTION sesuser;
	static CRITICAL_SECTION csmachs;

	//static map<CString, map<CString, CString>> cacherights;
	//static map<CString, map<CString, CString>>::iterator endrights;
	//static CRITICAL_SECTION csrights;

	static map<CString, CString>  cachedescs;
	static map<CString, CString>::iterator enddescs;
	static CRITICAL_SECTION dsclock;

// Operations
public:
	CString _helper;
	//CParameters _key;
	int macid;
	int usrid;
	int sesid;

	bool get_session_ids();
	session* process_session_ids();
	void get_last_result(const int ins,
						const int mac, const int cli, const int ses, const int his, 
						const int cmpy,
						const TCHAR* trans, const int trnslen);
	
  //void check_access_type();
	void user_cmpy_check(const TCHAR* cmpy, const TCHAR* user);
	void detect_user_id(const TCHAR* id);
	void detect_cmpy_id(const TCHAR* user);
	void exist_company(const TCHAR* cmpy, const int lcmpy);
	void exist_user(const TCHAR* user, const int userlen);
	void can_enter();
	/*void rights_process();*/
	/*void rights_process(const int cmpy, const TCHAR* user);*/
	void process_descriptions();
	void process_user_params();
	void reset_desc_cache();
	//void reset_rights_cache();
	//void load_rights_into_cache(const int cmpy, const TCHAR* user);
	void process_profiles();
	void helper_profile	(const TCHAR* main, const int mainlen, const TCHAR* profile, const int proflen);

	/*void notify_framework(	const TCHAR* model, const TCHAR* service, const TCHAR* type, const TCHAR* comp, 
							const TCHAR* func, const TCHAR* params, const bool donesrc);
    */
	int obtain_rights	(	const int cmpy,
							const TCHAR* user, const int userlen,
							const TCHAR* trns, const int trnslen,
							TCHAR* rights);
	void have_right();
	void look_4_rights();
	void encrypt_password();

	void load_descriptions();
	void load_user_params();

	void read_js(const TCHAR* data, scmdimp* newstruct, /*const bool isonloadfun, TCHAR* funlist, int& lenfunlist, */const int procid);
	void read_cb(const TCHAR* data, scmdimp* newstruct, /*const bool isonloadfun, TCHAR* funlist, int& lenfunlist, */const int procid);
	void get_final_fun();
	void reset_ghost_session();
	void reset_ghost_sessions();
	void release_sessions();
	void delete_machine_session();
	void delete_user_session();
	void delete_session();
	void update_session_time();

	void get_sessions_info();
	void get_sessions_count();
	void notify_use();
	void check_session();
	//void get_last_state_nosafe();
	void get_last_state();
	void get_laststate(const bool safe);
	void copy_state();
	void get_user_logons();

	void create_session();
	void begin_session();
	void end_session();
	void copy_session();
	void session_set_company();

	transactions* findtrans(const TCHAR* us, const int lus);
	nombres* finduser(const TCHAR* us, const int lus);
	int look4macid(bool& newone, const TCHAR* us, const int lus);
	int look4usrid(bool& newone, const int macid, const TCHAR* us, const int lus);
	int look4sesid(bool& newone, const int macid, const int usrid);

	void gui_get_history();
	void gui_get_session_data();
	void gui_get_top();
	void gui_get_pos();
	void gui_insert_trans();
	void gui_goto_trans();
	void gui_go_back();
	void gui_go_home();
	void gui_go_forward();
	void gui_go_pos();
	//void gui_get_texts();
	void gui_get_entrance();
	void gui_get_home();

	void get_file_into_response();

	void _validate_pass();

	int _get_description(	const TCHAR* lang, const int ll, 
							const TCHAR* code, const int codelen,
							TCHAR* desc,
							CString& key);
	int _get_library(	const TCHAR* document, const int doclen, 
										const TCHAR* typ, const int typlen,
										const TCHAR* deflib, const int defliblen,  
										TCHAR* lib);
	void get_description();

	void _get_file();
	void get_file(	//const TCHAR* lib, const int liblen, 
					const TCHAR* document, const int doclen,
					const TCHAR* typ, const int typlen,
					CString& result, const TCHAR* typetrans, const int tytrlen, 
					const bool byforce);
	//void reset_document_cache();

	//void get_last_css();
	void load_sessions();
	void save_sessions();

	void execute();
	void get_files_info();
	void get_client_history();

	void apply_lang			(	CString& dest);
	void handle_defaults(	CString& dest);
	void expand_macros	(	cConnection& con, Table& obj, CParameters& lparms, 
												const bool byforce, 
												map<CString, CString>& cache, 
												set<CString>& procdocs,
											//const TCHAR* library, const int libeen,
												const TCHAR* dbsource, const int dbsrclen, 
											//const TCHAR* typ, const int typlen,
												CString& dest, const bool replacehtml);
	void load_fileDB		(	cConnection& con, Table& obj, CParameters& lparms,
												const bool byforce, 
												map<CString, CString>& cache, 
												set<CString>& procdocs,
												const TCHAR* query, const int qrylen,
												const TCHAR* library, const int liblen,
												const TCHAR* document, const int doclen, 
												const TCHAR* typ, const int typlen,
												CString& dest, const bool replacehtml);
	void read_file			(	cConnection& con, Table& obj, CParameters& lparms, 
												const bool byforce, 
												map<CString, CString>& cache, 
												set<CString>& procdocs,
												const TCHAR* query, const int qrylen,
											//const TCHAR* lib, const int liblen,
												const TCHAR* document, const int doclen, 
												const TCHAR* typ, const int typlen,
												CString& strXML,
												const bool replacehtml);
	void read_file			(	const bool byforce, 
												map<CString, CString>& cache, 
											//const TCHAR* lib, const int liblen,
												const TCHAR* document, const int doclen, 
												const TCHAR* typ, const int typlen,
												CString& strXML,
												const bool replacehtml);
};

#ifdef _DEBUG
#define CALLSTACK callstack.AppendChar('/');callstack.Append(_T(__FUNCTION__));
#else
#define CALLSTACK
#endif

/////////////////////////////////////////////////////////////////////////////

//{{AFX_INSERT_LOCATION}}
// Microsoft Visual C++ will insert additional declarations immediately before the previous line.

#endif // !defined(AFX_SESSIONMAN_H__BE00A09E_0C66_4611_A122_4FC586BA3B10__INCLUDED_)

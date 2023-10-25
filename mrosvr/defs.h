#pragma once

// helpers mostly statistics
extern ULONGLONG		gstarttime;			// when the server was started
extern ULONGLONG       gprocesstime;		// how many cpu time is done
extern long         gaccessno;			// how many accesses have been made
extern long			gprocessesused;		// how many processes we have used
extern long         gexecutedfuns;		// how many functions are executed
extern long			gwaitinghits;		// how many requests were put to wait
extern long			grejectedhits;		// how many requests were rejected
extern long			grejectedreqs;		// how many requests were rejected
extern long			gfreesdone;			// how many free calls have done (memory management)
extern long			gtolerance;			// how many can wait a tranasaction before be rejected
extern long			gchildtol;			// how many the redisptach child can wait
extern int			gmaxdbcons;			// how mnay db connections per process

// special variables that once the system is running never change
extern TCHAR        gcfgpcom[128];
extern TCHAR		gcurpath[MAX_PATH];		
extern TCHAR		gwork[512];			// info about the server that is passed to the components
extern int			gworklen;			// length for the previuos
extern CParameters	gservers;			// real addresses for the services
extern CParameters	gsvrgrps;			// group of balancing other services
extern CParameters  gdefgrps;			// default group of balancing other services
extern TCHAR		gappstr[256];		// database string conecction
extern TCHAR		gcorstr[256];		// database string conecction
extern TCHAR		gerrload[256];
extern TCHAR		gsvrtype[4];		// server's type
extern int			gexectyp;			// 0:standar, 1:respond on dispatcher
extern TCHAR		gdomain[4];			// domain of the instance
extern TCHAR		gexename[32];

// some very important addresses
extern TCHAR		localsvr[16];		// our local server's address
extern int			localsvrlen;		// our local server's address's lenght
extern int			localprt;			// our local server's port
extern TCHAR		gatsvr[16];			// our gate address
extern int			gatsvl;				// our gate address's length
extern int			gatprt;				// our gate port

// some services are specials
extern bool			loginit;			// save on event viewer the errors
extern bool			debugmode;			// are we on debugging

// our basic constants
const int MAXFUNSPROC			= 12;		// maximum number of functions per process
const int DEF_MAX_PROCS		= 16;		// default number of concurrent processes
const int DEF_MAX_CLIES		= 32;		// default ot max clients

enum mrostatus {	
	FREE,				// never has been taken
	ACCEPTED,
	TAKEN,			// enter for be processed
	READING,		// reading the request

	WAITING,		// it is waiting for some cpu 
	SCHEDULED,	// it has given a cpu but it havent used
	EXECUTING,	// it is executing right now
	RESPONDING,
	DONE,				// it has been processed

	KILLING,		// while is killing
	KILLED,			// has been killed
	FAILED			// it failed, mainly internally
};
extern TCHAR* statdescs[];

// this array of string contains the functions keys for example:
// zfun00z, zfun01z, zfun02z, etc..., due they are extensibly used
// so we generate all of them once, avoiding do it every time hence
// boosting performance
extern TCHAR funnum[MAXFUNSPROC][8];

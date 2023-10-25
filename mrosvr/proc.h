#pragma once

#define MAXGCPOOL 64

// main block of information that describes the state of the transaction in the queue
struct sProcess
{
	sProcess()
	{
		hasretprms		= false;
		workonvalues	= false;
		lastcbfun[0]	= 0;
		lastcbfunlen	= 0;

		funptr			= 0;							
		funlen			= 0;
		isbyevent		= false;

		nlogs			= 0;
		eventname[0]	= 0;
		eventnamelen	= 0;
	}

	int				id;								// id
	int				clieid;							// link to the client's queue
	mrostatus		status;						// current status

	HANDLE			thread;							// current thread of execution
	HANDLE			startexecuting;					// event to start executing the transaction
	unsigned long	accesses;					// number of access for this cpu

	synservice		gatesock;					// some one else will do it
	TCHAR			hstr256[256];

	bool			sendbasics;						

	int				currfun;
	CParameters		lastcbstr;
	TCHAR			lastcbfun[64];
	int				lastcbfunlen;

	CString			newvalues;						// some functions alter some values
	bool			workonvalues;						// flag that tell us the we need to change some values
	CParameters		values;						// the values of the function's params

	CParameters		rights;						// the right cache in the current trans(not a global cache)
	CParameters		webservice;					// the paraneters for the execution of the function
	bool			isbyevent;

	CParameters		tmpres;

	CParameters		fun_params;
	TCHAR*			funptr;						// holds tht functios or command value
	int				funlen;						// function's string length
	TCHAR			eventname[64];
	int				eventnamelen;

	bool			hasretprms;
	CParameters		ret_prms;
	CParameters		notempty;

	//int				saveresult;					// 0:none, 1:full, 2:repeat if the result need to be save

	CParameters		log[MAXFUNSPROC];			// data that'll be send to the log
	bool			savelog[MAXFUNSPROC];
	int				nlogs;						// how many logs we have

	TCHAR*			gcpool[MAXGCPOOL];			// pool of garbage
	int				gcpoolid;					// pool of memory index
	bool			selfkill;

	TCHAR*			memory[2];
	int				memlen[2];
};

#define STATISTICS_SIZE (24*60)
struct stats
{
	UINT process_runned;
	UINT clients_accepted;
	UINT functions_executed;
	time_t when;
};

extern sProcess* procs;						// the main block of cpus
extern HANDLE* cpus;						// the block for the events cpu's availability
extern UINT MAX_PROCS;						// number of cpus

TCHAR* get_memory(sProcess& proc, const int len, const int slot);
bool gccollect(sProcess& proc, const int howmany);
void initialize_processes();
void init_proc(sProcess& proc);
void check_state();
bool kill_process(int proc2kill);
int get_list_prcs(TCHAR* lista, const int lid, const int buffsize);
int _get_statistics(TCHAR* lista, const int buffsize);
void get_executing_functions(TCHAR* result);

#pragma once

struct eventcmd
{
	TCHAR name[EVNSIZE];		// action's name
	int namelen;
	CParameters params;			// main command line: the parameters that are sent to server
	TCHAR fun[FUNSIZE];         // function's name
	int funl;
	CParameters extprms;		// some extra parameters(normally constants)
	CParameters retprms;		// some parameters that has to return
	TCHAR modu[MODSIZE];
	int modul;
	TCHAR comp[CMPSIZE];		// component's name
	int compl;
	TCHAR tcom[TYCSIZE];		// component's type(com, dll, python, vb, etc...)
	int tcoml;
	TCHAR security[SECSIZE];    // right that must have the user, in order to execute it: acc, mod, etc...
	int seclen;

	TCHAR status[32];		// action take to notifies some predefined entities
	int statl;

	//TCHAR notif[LGTSIZE];		// action take to notifies some predefined entities
	//int notifl;

	CParameters history;
	//TCHAR logtxt[LGTSIZE];		// log for the transaccion, overrides the function's default
	//int logtl;
	////TCHAR logext[LGESIZE];		// log for the transaccion, overrides the function's default
	//int logel;
	//TCHAR logkey[LGKSIZE];		// log for the transaccion, overrides the function's default
	//int logkl;
	//TCHAR logtyp[LGPSIZE];		// log for the transaccion, overrides the function's default

	bool saveerror;				// indicate if when any error, it'll be saved

	cMroList beforefuns;		// list of actions before the main one
	cMroList parallelfuns;		// list of actions parallel with the manin one
	cMroList afterfuns;			// list of actions after the main one

	int savestate;				// most of the transactions save its current result, for later use

	eventcmd() { init(); }

	void init()
	{
		name[namelen = 0]	 = 0;
		params.clear();
		fun[funl=0]			= 0;

		extprms.clear();
		retprms.clear();

		modu[modul=0]		= 0;
		comp[compl=0]		= 0;
		tcom[tcoml=0]		= 0;
		security[seclen = 0]= 0;
		status[statl = 0]= 0;
		//notif[notifl=0]		= 0;

		history.clear();
		//logtxt[logtl=0]		= 0;
		///*logext[logel=0]		= 0;*/
		//logkey[logkl=0]		= 0;
		//logtyp[0]			= 0;
		saveerror			= false;

		beforefuns.clear();
		parallelfuns.clear();
		afterfuns.clear();

		savestate			= 0;
	};
};

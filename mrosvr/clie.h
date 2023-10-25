#pragma once

#define CLIEERRLEN 32

struct sClients { // main block of information that describes the state of the transaction in the queue

	sClients() { procid = -1;	id = -1; }

	int			id;
	mrostatus	status;								// status 
	int			procid;								// process queue's slice
	int			step;								// what is the current step

	CParameters request;							// the entire parameters(request) from the client
	CParameters header;								// header
	CParameters basics;								// basics
	CParameters grpsvrs;
	CParameters sysgrps;
	CParameters results[MAXFUNSPROC+2];				// here we get the components's result
	bool				pendingres[MAXFUNSPROC+2];
	int					restop;
	int					resstp;
	WSABUF			DataBuf[MAXFUNSPROC+2];				// 1 for unicode mark and 2 for the error special case
	int					bufftop;

	CString			tempstr;							// temporal helper variable, should be eliminated with a better desing

	// header values
	bool				retresult;							// if the request need returns or not
	bool				fromgate;							// is it executed by the gate or comes from the gate
	int					priority;							// priority of the request

	// server values

	TCHAR				_gatsvr[16];							// the gate/proxy server
	TCHAR*			pgatsvr;
	int					gatsvl;
	int					gatprt;								// the gate/proxy port

	time_t			dateprocess;                        // request date start
	long				accessno;                           // what access number

	// basic values
	TCHAR				machine[ZIPADDRMAX+1];
	int					maclen;
	TCHAR				trans[ZTRNCODMAX+1];				// request's transaction (only from gui if any)
	int					trnlen; 
	TCHAR				user[ZUSERIDMAX+1];					// request's user (only from gui if any)
	int					usrlen; 

	// function values
	int					nfuns;								// how many functions are to be executed
	//TCHAR		component[MAXFUNSPROC][ZCOMPNMMAX+1];// name of the component which is part of com_params but it is very often used so, we keep it separtally
	//int			cmplen[MAXFUNSPROC];
	TCHAR				function[MAXFUNSPROC][ZFUNNAMMAX+1];// name of the function which is part of com_params but it is very often used so, we keep it separtally
	int					funlen[MAXFUNSPROC];
	//TCHAR		typecom[MAXFUNSPROC][ZTYPCOMMAX+1];// function component type
	//int			tcmlen[MAXFUNSPROC];
	ULONGLONG		cputime[MAXFUNSPROC];

	// data-sockets values
	long		inlen;                              // request lenght
	int			packsin;							// how many packets arrived
	long		outlen;								// the response's lenght
	int			packsout;							// packets sended as a response
	bool		isunicode;							// do client is unicode ?
	bool		isfromexplorer;						// do the client is a browser ?

#ifdef MRO_IOCP_IMP 
	WSAOVERLAPPED RecvOverlapped;
#endif
	WSABUF		buffread;							// socket's helper
	SOCKET		socket;								// the socket
	ULONGLONG		reqst;								// start request's time
	ULONGLONG recst;								// start recv's time
	ULONGLONG		sndst;								// start send's time
	bool		reqreaded;
	bool		endmark;
	HANDLE		procreq;
	TCHAR		reqerror[CLIEERRLEN+1];
};

int get_list_reqs(TCHAR* lista, const int lid, const int buffsize);

extern sClients* clies;
extern UINT MAX_CLIES;
extern UINT DEF_CLXPR;
extern UINT MAX_SBSIZ;
extern UINT DEF_SBSIZ;


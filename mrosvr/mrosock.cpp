#include "stdafx.h"

/**
 * description	: implementation file for sockes
 * author				: miguel rodriguez ojeda
 * date					: febreaury 10 2003
 *
 * modification	: UNICODE handling
 *							: june 15 2006
 */

const int MAXADDRSTR		= 16;
const int MAXSTATICVECTOR	= 5;

int gen_service( TCHAR* data, const TCHAR* type, const TCHAR* comp, 
							const TCHAR* funname, const TCHAR* pkey, const TCHAR* pvalue) {
	return mikefmt(data,_T("%c%s%c1%c")
						_T("%c%s%c")
							_T("%c%s%c%s%c%c%s%c%s%c%c%s%c%s%c%c%s%c%s%c")
						_T("%c"), 
						CParameters::LEFT, ZZNFUNS, CParameters::SEP, CParameters::RIGHT,
						CParameters::LEFT, ZFUN00Z, CParameters::SEP, 
							CParameters::LEFT, ZCOMPNM, CParameters::SEP, comp,		CParameters::RIGHT,
							CParameters::LEFT, ZFUNNAM, CParameters::SEP, funname,	CParameters::RIGHT,
							CParameters::LEFT, ZTYPCOM, CParameters::SEP, type,		CParameters::RIGHT,
							CParameters::LEFT, pkey	  , CParameters::SEP, pvalue,	CParameters::RIGHT,
						CParameters::RIGHT);	
}
int gen_service( TCHAR* data, const TCHAR* type, const TCHAR* comp, 
							const TCHAR* funname, CParameters* params, CParameters* values) {
	return mikefmt(data,_T("%c%s%c1%c")
						_T("%c%s%c")
							_T("%c%s%c%s%c%c%s%c%s%c%c%s%c%s%c%s")
						_T("%c"), 
						_T("%c%s%c%s%c"), 
						CParameters::LEFT, ZZNFUNS, CParameters::SEP, CParameters::RIGHT,
						CParameters::LEFT, ZFUN00Z, CParameters::SEP, 
							CParameters::LEFT, ZCOMPNM, CParameters::SEP, comp,		CParameters::RIGHT,
							CParameters::LEFT, ZFUNNAM, CParameters::SEP, funname,	CParameters::RIGHT,
							CParameters::LEFT, ZTYPCOM, CParameters::SEP, type,		CParameters::RIGHT,
							params ? params->buffer():_T(""),
						CParameters::RIGHT,
						CParameters::LEFT, ZVALUES, CParameters::SEP, values ? values->buffer():_T(""), CParameters::RIGHT);	
}

void handle_error(CString* presult, const TCHAR* e) {
	presult->Format(_T("%c%s%cclient(%s)%c"), CParameters::LEFT, ZSERROR, CParameters::SEP, e, CParameters::RIGHT);
}

unsigned __stdcall process_request(void* param) {
	sSocketData* mydata	= static_cast<sSocketData*>(param);
	CWnd* hwnd					= mydata->hwnd;
	UINT message				= mydata->message_id;
	CString* result			= mydata->result;
	SOCKET sock					= mydata->socket;
	bool isdynamic			= mydata->creat_type == 2;

	// we create one in advance because is obvious that we must have at least one
	// remember that although we use UNICODE as default, we must work on char because
	// the sockets librearies work only on char
	std::vector<char*> rescache;
	std::vector<char*>::iterator endcache	= rescache.end();
	std::vector<char*>::iterator begcache	= rescache.begin();
	std::vector<char*> res;
	std::vector<char*>::iterator end			= res.end();
	std::vector<char*>::iterator begin		= res.begin();
	TCHAR* presult			= nullptr; 
	bool dynrecbuff			= false;
	TCHAR* pres					= nullptr;
	char* buffer				= nullptr;
	char* bigbuffer			= nullptr;
	int bigbuffindex		= 0;
	UINT packets				= 0;
	char svector[MAXSTATICVECTOR * (MROSOCK_BUFIN + 6)];

	try	{
		//analasing SO_REUSEADDR
		//int one = 1;
		//setsockopt(sock,SOL_SOCKET,SO_REUSEADDR/*|SO_REUSEPORT*/,(char*)&one,sizeof(one));

		int rVal;
		int iLen = MAXADDRSTR;

		// create the socket
		SOCKADDR_IN serverInfo;
		serverInfo.sin_family = AF_INET; 
		rVal = WSAStringToAddress(mydata->server, AF_INET, NULL, (LPSOCKADDR)&serverInfo, &iLen);
		serverInfo.sin_port = htons(mydata->port);
		require(rVal == SOCKET_ERROR, _T("sockets(s2a)"));
		rVal = connect(sock,(LPSOCKADDR)&serverInfo, sizeof(serverInfo));
		require(rVal == SOCKET_ERROR, _T("sockets(con)"));

		WSABUF DataBuf[3];

#ifdef UNICODE
		DataBuf[0].buf = (char*)MRO_UNICODE_BOM;
		DataBuf[0].len = sizeof(MRO_UNICODE_BOM);
		DataBuf[1].buf = (char*)mydata->message;
		DataBuf[1].len = mydata->msglen * sizeof(wchar_t);
		DataBuf[2].buf = (char*)MRO_END_MSG_MRK;
		DataBuf[2].len = sizeof(MRO_END_MSG_MRK);
		int packs = 3;
#else
//		DataBuf[0].buf = mydata->message;
//		DataBuf[0].len = mydata->msglen;
//		DataBuf[1].buf = (char*)_T("¦");
//		DataBuf[1].len = 1;
//		int packs = 2;
		DataBuf[0].buf = mydata->message;
		DataBuf[0].len = mydata->msglen;
		DataBuf[1].buf = (char*)MRO_END_MSG_MRK;
		DataBuf[1].len = sizeof(MRO_END_MSG_MRK);
		int packs = 2;
#endif

		DWORD sent = 0;
		rVal = WSASend(sock, &DataBuf[0], packs, &sent, 0, 0, 0);
#ifdef UNICODE
		ensure(sent != DataBuf[1].len + sizeof(MRO_UNICODE_BOM) + sizeof(MRO_END_MSG_MRK), _T("sockets(sn0)"));
#else
		ensure(sent != DataBuf[0].len + sizeof(MRO_END_MSG_MRK), _T("sockets(sn0)"));
#endif
		require(rVal == SOCKET_ERROR, _T("sockets(sn1)"));

		//shutdown(sock, SD_SEND); // we no longer send data, in attemp to stop bugs

		bool isunicode	= false;
		UINT nchars		= 0;
		char helper[5];
		helper[4] = '\0';

		for(;;) {
			if(packets < MAXSTATICVECTOR) {
				buffer = &svector[packets*(MROSOCK_BUFIN + 6)]; // first we use the cache
			}
			else {
				if(bigbuffindex == 0) {
					bigbuffer = (char*)malloc(MAXSTATICVECTOR * (MROSOCK_BUFIN + 6));
					rescache.push_back(bigbuffer);
				}
				buffer = &bigbuffer[bigbuffindex*(MROSOCK_BUFIN + 6)];
				if(++bigbuffindex == MAXSTATICVECTOR) bigbuffindex = 0;
			}

			WSABUF recbuf;
			recbuf.buf		= (char*)&buffer[4];
			recbuf.len		= MROSOCK_BUFIN;
			DWORD receive	= 0;
			DWORD flags		= 0;
			rVal			= WSARecv(sock, &recbuf, 1, &receive, &flags, 0, 0);

			// this is a patch that I need to know about it more(why marks WSAECONNRESET)
			if(rVal == SOCKET_ERROR) {
				DWORD err = GetLastError();
				require(err != WSAECONNRESET, _T("sockets(rec)"));
				rVal = 0;
			}
			rVal = receive;

			if(rVal == 0) break; // some went wrong, may be the server fell mean while we were receiving data

			if(	packets == 0 && 
				((unsigned char)buffer[4]) == MRO_UNICODE_BOM[0] &&
				((unsigned char)buffer[5]) == MRO_UNICODE_BOM[1]) { 
				isunicode = true;
				if(receive == 2) continue;
				set2chA(&buffer[4], ' ', 0); 
			}

			*(int*)buffer = rVal; // trick for insert on the first two bytes the lenght of the current block of data

			++packets; 
			nchars += rVal; 

			if(packets > MAXSTATICVECTOR)
				res.push_back(buffer);
		}

		//shutdown(sock, SD_BOTH); 
		closesocket(sock);
		sock = INVALID_SOCKET;

		try {
			// manage memory for efficency, the 256 extra is because we will 
			// add some extra stuff(times), besides if the totalsize excedes
			// the stack frame size it catches the exception and respons
			// aproporely, this is a bug all we have to do is find that if it
			// breaks the limit and use dynamic memory if it so
			int totalsize = nchars + 2 + 256;
			dynrecbuff = totalsize > (1024 * 16);
			pres = presult = dynrecbuff ? (TCHAR*)malloc(totalsize) : (TCHAR*)alloca(totalsize);
		}
		catch(CException *e){ e->Delete();	if(mydata->creat_type==0) throw _T("response_2_long");	}
		catch(...)			{				if(mydata->creat_type==0) throw _T("response_2_long");	}

		int smax = packets > MAXSTATICVECTOR ? MAXSTATICVECTOR : packets;
		for(register int si = 0; si < smax; ++si) {
			char* q = &svector[si*(MROSOCK_BUFIN + 6)];
			int l = *(int*)q;

#ifdef UNICODE
			if(isunicode) {
				memcpy(pres, &q[4], sizeof(char)*l);
				pres += (l/2);
			}
			else {
				MultiByteToWideChar( CP_ACP, 0, &q[4], l, (LPWSTR)pres, l);
				pres += l;
			}
#else
			memcpy(pres, &q[4], sizeof(char)*l);
			pres += l;
#endif
		}

		// we may be used more memory than or static cache
		if(packets > MAXSTATICVECTOR) {
			end = res.end();
			begin = res.begin();
			for(auto iter = begin; iter != end; ++iter) {
				char* q = *iter;
				int l = *(int*)q;

#ifdef UNICODE
				if(isunicode) {
					memcpy(pres, &q[4], sizeof(char)*l);
					pres += (l/2);
				}
				else {
					MultiByteToWideChar( CP_ACP, 0, &q[4], l, (LPWSTR)pres, l);
					pres += l;
				}
#else
				memcpy(pres, &q[4], sizeof(char)*l);
				pres += l;
#endif
			}
		}

		if(mydata->ret_result == false) {
			set4ch(pres, CParameters::LEFT, 'z', 'n', 'o');					pres += 4;
			set4ch(pres, 'r', 'e', 's', 'z');								pres += 4;
			set4ch(pres, CParameters::SEP, '1', CParameters::RIGHT, 0);		pres += 3;
		}

		*pres = '\0';

		if(mydata->alive) result->SetString(presult, pres - presult);
	}
	catch(const TCHAR* e)	{ if(mydata->alive) handle_error(result, e);					}
	catch(CException *e)	{ TCHAR d[512]; e->GetErrorMessage(d,512); e->Delete();
							  if(mydata->alive) handle_error(result, d);					}
	catch(mroerr& e)		{ if(mydata->alive) handle_error(result, e.description);		}
	catch(...)				{ if(mydata->alive) handle_error(result, _T("unhandled_error"));}

	// as soon as we get the result we trigger the response to the client
	// concurrently we must send the finish message
	if(mydata->alive && hwnd) hwnd->PostMessage( message, mydata->process_id, (LPARAM)mydata->response); 

	// if a dynamic buffer was need it we delete it
	if(dynrecbuff && presult) free(presult);

	// once we've done with the string management we free the auxiliary memory
	if(bigbuffer)	{
		endcache = rescache.end();
		begcache = rescache.begin();
		for(auto iter = begcache; iter != endcache; ++iter) 
			free(*iter);
	}
	
	// if we used a dynamycally extended string we delete it
	if(mydata->isdynmsg) free(mydata->message);

	// if was dynamically/asyncronously we free it
	if(isdynamic) free(mydata);
	else mydata->status = asynservice::maxpool;	// until this point we could say that the process is done

	return 0;
}

sSocketData asynservice::sdpool[asynservice::maxpool] = { 
						{false,-1}, {false,-1}, {false,-1}, {false,-1}, 
						{false,-1}, {false,-1}, {false,-1}, {false,-1} 
};

void asynservice::execute(	const TCHAR* server, 
							const int port, 
							const TCHAR* command, 
							const int cmdlen, 
							webdata* response, 
							CWnd* hwnd__, 
							UINT message__, 
							int process_id, 
							const bool ret_result) {
	require(hwnd__ == 0 || message__ == 0, _T("need CWnd"));

	// if we are making the call asyncronous the memory has to persist
	sSocketData* mydata = nullptr;
	int slotfound = -1;
	for(register int i=0; i<maxpool; ++i)	{
		if(sdpool[i].status == -1 || sdpool[i].status == maxpool) { 
			mydata = &sdpool[i];
			sdpool[i].status = 1;
			slotfound = i;
			break;
		}
	}
	// couldn't find any available slow, we use a dynamic creation type
	if(!mydata) mydata = (sSocketData*)malloc(sizeof(sSocketData));	

	// create the working thread 
	UINT thread_id;
	HANDLE thread_h = (HANDLE)_beginthreadex(0, 0 , process_request, mydata, CREATE_SUSPENDED, &thread_id);

	mydata->init();
	mydata->creat_type	= slotfound != -1 ? 1:2;
	mydata->alive		= true;
	mydata->status		= 0;
	mydata->process_id  = process_id;       // this is the id from the running transactions list
	mydata->hwnd        = hwnd__;           // when it is sent asyncronous this is the window where the message is sent
	mydata->message_id  = message__;        // when it is sent asyncronous this is the messages sent we finish
	_tmemcpy(mydata->server, server, 16);	// server's ip
	mydata->server[15]  = '\0';
	mydata->port        = port;             // server's port
	mydata->ret_result	= ret_result;
	mydata->result      = &response->result;	
	mydata->response    = response;

	// message processing
	mydata->msglen = cmdlen == 0 ? _tcslen(command) : cmdlen;
	// the (MSGBUFFER-2), is because not only we are gona have the message
	// but we will have (1) a terminator(|) and the (2) the ending null
	if(mydata->msglen < (MSGBUFFER-2)) mydata->message = mydata->msg;
	else {
		mydata->isdynmsg = true;
		mydata->msg[0] = 0;														// mark as default buffer not used
		mydata->message = (TCHAR*)malloc(sizeof(TCHAR)*(mydata->msglen + 2));	// for the null terminator and then message end
	}
	_tmemcpy(mydata->message, command, mydata->msglen);

	// create our socket
	mydata->socket = socket(PF_INET, SOCK_STREAM, IPPROTO_TCP); 
	if(mydata->socket == SOCKET_ERROR)
	{
		DWORD err = ::GetLastError();
		CString target;
		target.Format(_T("%s:%d:%d:"), server, port,err);
		target.Append(command, cmdlen);
		requireex(true, _T("sockets"), target);
	}
	ResumeThread(thread_h);
	CloseHandle(thread_h);
}

void synservice::execute(	const TCHAR* server, 
							const int port, 
							const TCHAR* command, 
							const int cmdlen, 
							CString& result__, 
							const bool ret_result) {	
	sSocketData* mydata = &data;
	mydata->init();
	mydata->creat_type	= 0;
	mydata->alive		= true;
	mydata->status		= 0;
//	mydata->process_id  = process_id;       // this is the id from the running transactions list
//	mydata->hwnd        = hwnd__;           // when it is sent asyncronous this is the window where the message is sent
//	mydata->message_id  = message__;        // when it is sent asyncronous this is the messages sent we finish
	_tmemcpy(mydata->server, server, 16);	// server's ip
	mydata->server[15]  = '\0';
	mydata->port        = port;             // server's port
	mydata->ret_result	= ret_result;
	mydata->result      = &result__;		
	mydata->response	= 0;

	// message processing
	mydata->msglen = cmdlen == 0 ? _tcslen(command) : cmdlen;
	mydata->message = const_cast<TCHAR*>(command);
	mydata->isdynmsg = false;
	mydata->msg[0] = 0;						// mark as default buffer not used

	// create our socket
	mydata->socket = socket(PF_INET, SOCK_STREAM, IPPROTO_TCP);
	if(mydata->socket == SOCKET_ERROR) {
		DWORD err = ::GetLastError();
		CString target;
		target.Format(_T("%s:%d:%d:"), server, port,err);
		target.Append(command, cmdlen);
		requireex(true, _T("sockets"), target);
	}
	process_request(mydata);
}
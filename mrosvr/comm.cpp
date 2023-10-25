#include "stdafx.h"

/************************************************************************************
* description   : mro server
* purpose       : execute any transacitions from clients
* author        : miguel rodriguez
* date creation : 20 junio 2004
* change        : 1 marzo
*                 call coms direct from the function execute instead of CMroCom::call_com)
* change        : 10 febrero 
*                 change to global queue array, and the access controled by semaphores
**************************************************************************************/

#include "mrosvr.h"

void get_ip_and_name(SOCKET s, TCHAR* ip, int& iplen) {
	struct sockaddr_in addr;
	socklen_t len = sizeof addr;
	getpeername(s, (struct sockaddr*)&addr, &len);
	char address[32];
	iplen = mikefmtA(address, "%s", inet_ntoa(addr.sin_addr));
	MultiByteToWideChar(CP_ACP,0, address, iplen + 1, ip, iplen + 1);

//    struct in_addr addr2;
//	addr2.s_addr = inet_addr(address);
//	struct hostent *remoteHost = gethostbyaddr((char *) &addr2, 4, AF_INET);
//	namelen = mikefmtA(address, "%s", remoteHost->h_name);
//	MultiByteToWideChar(CP_ACP,0, address, namelen + 1, name, namelen + 1);

	// important validation, in some server, the gethostbyaddr return
	// the fullname eg: mrodriguez.sola.com.mx, we dont need it like 
	// that so we check it and if it so we delete the full name tail
//	TCHAR* start = (TCHAR*) _tmemchr(name, _T('.'), (size_t)namelen);
//	if(start) namelen = start - name;
//	name[namelen] = 0;
}

void close_socket(sClients& clie) {
	if(clie.socket != INVALID_SOCKET)	{
		//shutdown(clie.socket, SD_BOTH); 
		closesocket(clie.socket);
		clie.socket = INVALID_SOCKET;
		clie.retresult = false;
	}
}

void respond(int clieid, TCHAR* dtl, const int dtllen, const bool finalpacket) {
	sClients& clie = clies[clieid];
	if(clie.retresult == false) return;

	int rdtllen = dtllen;

	if(clie.isfromexplorer)	{
		CString& temporal = clie.tempstr;
		if(clie.packsout == 0)
			temporal.Empty();
		else
			temporal = _T(',');

		if(dtllen) temporal.Append(dtl, dtllen);

		dtl		= 0;
		rdtllen	= 0;

		if(temporal.IsEmpty() == false) {
			CParameters::tojson(temporal);
			dtl		= temporal.GetBuffer();
			rdtllen = temporal.GetLength();
		}
	}

	ULONGLONG startpack = GetTickCount64();
	clie.outlen += send_buff_2_client(	clie,
										clie.packsout, 
										dtl, rdtllen,
										clie.packsout == 0,
										finalpacket);
	clie.sndst += GetTickCount64() - startpack;
}

long send_buff_2_client(	sClients& clie, int& packets_out, 
							TCHAR* dtl, const int dtllen, 
							const bool firstpacket,
							const bool finalpacket) {
++packets_out;

	if(dtllen == 0) return 0;

	DWORD finallen	= 0;

	try	{
		if(firstpacket)	{
			clie.DataBuf[clie.bufftop].buf = (char*)MRO_UNICODE_BOM;
			clie.DataBuf[clie.bufftop].len = 2;
			++clie.bufftop;
		}
		if(dtllen) {
			clie.DataBuf[clie.bufftop].buf = (char*)dtl;
			clie.DataBuf[clie.bufftop].len = dtllen * sizeof(wchar_t);
			++clie.bufftop;
		}
		if(finalpacket)	{
			int rVal = WSASend(clie.socket, &clie.DataBuf[0], clie.bufftop, &finallen, 0, 0, 0);
			if(rVal == SOCKET_ERROR) return 0;
		}
	}
	catch(CException *e)	{ e->Delete();	}
	catch(mroerr& e)			{ }
	catch(...)						{	}
	return finallen;
}

long send_direct_2_client(	SOCKET s, 
							TCHAR* dtl, const int dtllen, 
							const bool firstpacket) {
	if(dtllen == 0) return 0;

	DWORD finallen	= 0;
	int packid		= 0;
	WSABUF DataBuf[4];

	try	{
		if(firstpacket)	{
			DataBuf[packid].buf = (char*)MRO_UNICODE_BOM;
			DataBuf[packid].len = 2;
			++packid;
		}
		if(dtllen) {
			DataBuf[packid].buf = (char*)dtl;
			DataBuf[packid].len = dtllen * sizeof(wchar_t);
			++packid;
		}
		int rVal = WSASend(s, &DataBuf[0], packid, &finallen, 0, 0, 0);
		if(rVal == SOCKET_ERROR) return 0;
	}
	catch(CException *e)	{ e->Delete();	}
	catch(mroerr& e)			{ }
	catch(...)						{ }
	return finallen;
}

void process_html(sClients& clie) {
	TCHAR* begin = clie.request.buffer();
	int offsetpost = -1;
	int offsetget = -1;
	if(cmp4ch(&begin[0], 'P','O','S','T') && begin[4] == ' ') offsetpost = 0;
	else
	if(cmp4ch(&begin[1], 'P','O','S','T') && begin[5] == ' ') offsetpost = 1;
	else 
	if(cmp4ch(&begin[0], 'G','E','T',' ')) offsetget = 0;
	else
	if(cmp4ch(&begin[1], 'G','E','T',' ')) offsetget = 1;

	if(offsetpost != -1 || offsetget !=- 1)	{
		if(wchar_t* http = _tcsstr(begin, _T("HTTP/")))	{
//			clie.isfromexplorer	= true;
			clie.isunicode		= true;

			CString hack = clie.request.copy();
			int end = hack.Find(_T(" HTTP/"));
			if(end != -1)	{
				hack.Delete(end, hack.GetLength() - end);
				if(offsetpost != -1) hack.Replace(_T("POST /"), _T(""));
				if(offsetget != -1) hack.Replace(_T("GET /"), _T(""));
			}
			if(_tmemchr(hack, _T('%'), hack.GetLength()))	{
				hack.Replace(_T("%5B"), _T("[")); 
				hack.Replace(_T("%5D"), _T("]"));
				hack.Replace(_T("%5E"), _T("^"));
				hack.Replace(_T("%20"), _T(" "));
				hack.Replace(_T("%7C"), _T("|"));
			}
			clie.request.set_value(hack);
		}
	}
}

/*
	return meaning: true means end of request, false end not found
*/
bool fill_request(sClients& clie, char* p, const int plen)
{
	int rVal = plen;
	if(rVal == 0) return true;	// lenght zero will treat as request end, dont you think?

	// check if it is a unicode request, we check only the first package and 
	// if it so we clean the unicode's mark, in other to clean the request
	if(clie.packsin == 0 &&	((unsigned char)p[0]) == MRO_UNICODE_BOM[0] && 
							((unsigned char)p[1]) == MRO_UNICODE_BOM[1])
		clie.isunicode = true;

	// in unicode request we write 2 bytes for the termination null for this part
	if(clie.isunicode) { set2chA(&p[rVal], 0, 0); }
	else p[rVal] = 0;

	// check for end of the message(has to be very improved)
	bool endfound = false;

	if(rVal > 3) // we need packets with at least 4 char long in order to check the whole mark
	{
		if(cmp4chA(&p[rVal-4], MRO_END_MSG_MRK[0], MRO_END_MSG_MRK[1], MRO_END_MSG_MRK[2],MRO_END_MSG_MRK[3]))	
		{ 
			endfound = true; 
			p[rVal - 4] = 0; rVal-=4; 
		}
	}
	else
	{
		if(clie.packsin > 0) // we need not to be the first packet because we need at least one back
		{ 
			char* b = (char*)clie.tempstr.GetBuffer();
			int l = clie.tempstr.GetLength() * sizeof(wchar_t);

			char c[4];
			if(rVal == 3) { c[0]=*(b+l-2); c[1]=p[0];     c[2]=p[1];     c[3]=p[2]; } else
			if(rVal == 2) { c[0]=*(b+l-4); c[1]=*(b+l-2); c[2]=p[0];     c[3]=p[1]; } else
			if(rVal == 1) { c[0]=*(b+l-6); c[1]=*(b+l-4); c[2]=*(b+l-2); c[3]=p[0]; }

			if(cmp4chA(c, MRO_END_MSG_MRK[0], MRO_END_MSG_MRK[1], MRO_END_MSG_MRK[2],MRO_END_MSG_MRK[3]))	
			{ 
				endfound = true; 
//				if(clie.isunicode) 
					clie.tempstr.Delete(l-(4-rVal), 4-rVal); 
//				else 
//					clie.tempstr.Delete(l-(4-rVal), 4-rVal); 
				p[rVal=0]=0;
			}
		}
	}

	if(clie.packsin == 0) 
	{
//		if(clie.isunicode) 
//		{ 
			rVal-=2; 
			if(!endfound) clie.tempstr.SetString((wchar_t*)(p+2), rVal / 2); 
//		}
//		else clie.tempstr = (char*)p;
	}
	else 
	{
//		if(clie.isunicode) 
			clie.tempstr.Append((wchar_t*)p, rVal / 2);
//		else 
//			clie.tempstr += (char*)p;
	}

	clie.inlen += rVal;		// our request lenght grow of course
	++clie.packsin;			// one more package read

	// check if the package comes from some web browser
	if(endfound)
	{
		if(clie.packsin == 1)// && clie.isunicode)
			clie.request.set_value((wchar_t*)(p+2), rVal / 2);
		else
			clie.request.set_value(clie.tempstr);
		process_html(clie);
		return clie.endmark = true;
	}
	
	return false;
}

#ifdef MRO_IOCP_IMP 
DWORD read_iocp_request(sClients& clie)
{
	DWORD Flags = 0;
	DWORD RecvBytes = 0;
	int rc = WSARecv(clie.socket, &clie.buffread, 1, &RecvBytes, &Flags, &clie.RecvOverlapped, NULL);
	if( rc == SOCKET_ERROR)
	{ 
		if( WSAGetLastError() == WSA_IO_PENDING ) return -1;
		close_socket(clie); 
		return -2;
	}
    int r = WSAWaitForMultipleEvents(1, &clie.RecvOverlapped.hEvent, TRUE, INFINITE, TRUE);
	ensure(r == WSA_WAIT_FAILED, _T("iocp_WSAWaitForMultipleEvents"));

	r = WSAGetOverlappedResult(clie.socket, &clie.RecvOverlapped, &RecvBytes, FALSE, &Flags);
	ensure(r == FALSE, _T("iocp_WSAGetOverlappedResult"));
    WSAResetEvent(clie.RecvOverlapped.hEvent);

	if(rc == 0 && RecvBytes == 0) return -2;
	return RecvBytes;
}
#else
DWORD read_block_request(sClients& clie)
{
	DWORD receive	= 0;
	DWORD flags		= 0;
	int rVal = WSARecv(clie.socket, &clie.buffread, 1, &receive, &flags, 0, 0);
	if(rVal == SOCKET_ERROR) 
	{	
		if( WSAGetLastError() == WSA_IO_PENDING ) return -1;
		close_socket(clie); 
		return -2; 
	}
	return receive;
}
#endif

bool read_request(sClients& clie)
{
	if(clie.reqreaded == false)
	{
		char buffer[MROSOCK_BUFIN + 1];
		clie.buffread.buf = buffer;
		clie.buffread.len = MROSOCK_BUFIN;
#ifdef MRO_IOCP_IMP 
		DWORD receive = read_iocp_request(clie);
#else
		DWORD receive = read_block_request(clie);
#endif
		if(receive == -2) return true;
		if(receive != -1)
			if(clie.reqreaded = fill_request(clie, buffer, receive))	// no more to read;
				clie.recst = GetTickCount64() - clie.recst;				// we take the time and funs for statistics
	}
	ensure(clie.packsin >= 64, _T("request_2_big"));
	return clie.reqreaded;
}

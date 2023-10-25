#pragma once

#include <winsock2.h>
#include <ws2tcpip.h>
#include <Iphlpapi.h>

#pragma comment (lib, "ws2_32.lib")
#pragma comment (lib, "Iphlpapi.lib")

#include <vector>
using namespace std;

#include "process.h"

struct sSocketData
{
#define MSGBUFFER	4096

	bool			alive;
	int				status;
	int				process_id;
	CWnd*			hwnd;
	UINT			message_id;
	TCHAR			server[16];
	int				port;

	bool			isdynmsg;
	TCHAR*			message;
	int				msglen;
	TCHAR			msg[MSGBUFFER];

	CString*		result;
	SOCKET			socket;
	int				creat_type; // 0 stack, 1 pool, 2 dynamic
	bool			ret_result;
	webdata*		response;

	void init()
	{
		alive		= false;
		status		= 0;
		process_id	= 0;
		hwnd		= 0;
		message_id	= 0;
		server[0]	= 0;
		port		= 0;

		isdynmsg	= false;
		message		= 0;
		msglen		= 0;
		msg[0]		= 0;

		result		= 0;
		socket		= 0;
		creat_type	= -1;
		ret_result	= false;
	}
};

#define MROSOCK_BUFOUT	2560
#define MROSOCK_BUFIN	2560

class asynservice
{
public:
	static const int maxpool = 8;
private:
	static sSocketData sdpool[maxpool];

public:

	void execute(	const TCHAR* server, 
					const int port, 
					const TCHAR* command, 
					const int cmdlen,
					webdata* response, 
					CWnd* hwnd__ = 0, 
					UINT message__ = 0, 
					int process_id = 0,
					const bool ret_result = true);
};

class synservice
{
	sSocketData data;

public:

	void execute(	const TCHAR* server, 
					const int port, 
					const TCHAR* command, 
					const int cmdlen,
					mro::CParameters& result__, 
					const bool ret_result = true)
	{
		CString result;
		execute(server, port, command, cmdlen, result, ret_result);
		result__.set_value(result);
	}

	void execute(	const TCHAR* server, 
					const int port, 
					const TCHAR* command, 
					const int cmdlen,
					CString& result__, 
					const bool ret_result = true);

	static CString GetMACaddress()
	{
		CFixedStringT<CString, 64> mac;

		IP_ADAPTER_INFO AdapterInfo[16];       // Allocate information 
											 // for up to 16 NICs
		DWORD dwBufLen = sizeof(AdapterInfo);  // Save memory size of buffer
 
		DWORD dwStatus = GetAdaptersInfo(      // Call GetAdapterInfo
										AdapterInfo,                 // [out] buffer to receive data
										&dwBufLen);                  // [in] size of receive data buffer
		//assert(dwStatus == ERROR_SUCCESS);  // Verify return value is 
										  // valid, no buffer overflow
 
		PIP_ADAPTER_INFO pAdapterInfo = AdapterInfo; // Contains pointer to
										// current adapter info
		do {
			//PrintMACaddress(pAdapterInfo->Address); // Print MAC address
			unsigned char* MACData = pAdapterInfo->Address;
			mac.Format(_T("%02X-%02X-%02X-%02X-%02X-%02X"),
										MACData[0], MACData[1], MACData[2], MACData[3], MACData[4], MACData[5]);
			pAdapterInfo = pAdapterInfo->Next;    // Progress through 
											  // linked list
			if(!mac.IsEmpty()) break;
		}
		while(pAdapterInfo);                    // Terminate if last adapter

		return mac;
	}

	static CString GetMachineNumber()
	{
		TCHAR   szBuffer[MAX_COMPUTERNAME_LENGTH + 1];
		DWORD  dwNameSize = MAX_COMPUTERNAME_LENGTH + 1;
		GetComputerName( szBuffer, &dwNameSize );
		return CString(szBuffer);
	}
	static int GetMachineNumber(TCHAR* szBuffer)
	{
		DWORD  dwNameSize = MAX_COMPUTERNAME_LENGTH + 1;
		GetComputerName( szBuffer, &dwNameSize );
		return dwNameSize;
	}
	static CString GetIpAddress()
	{
		CFixedStringT<CString, 32> l_strIp;

		char name[255];
		PHOSTENT hostinfo;
		char *ip;

		if( gethostname ( name, sizeof(name)) == 0)
		{
			if((hostinfo = gethostbyname(name)) != NULL)
			{
				int nCount = 0;
				while(hostinfo->h_addr_list[nCount])
				{
					ip = inet_ntoa (*(struct in_addr *)hostinfo->h_addr_list[nCount]);
					l_strIp = ip;
					++nCount;
				}
			}
		}
		return l_strIp;
	}
	/*static void GetPrimaryIp(char* buffer, size_t buflen)
	{
		int sock = socket(AF_INET, SOCK_DGRAM, 0);

		const char* kGoogleDnsIp = "8.8.8.8";
		u_short kDnsPort = 53;
		struct sockaddr_in serv;
		memset(&serv, 0, sizeof(serv));
		serv.sin_family = AF_INET;
		serv.sin_addr.s_addr = inet_addr(kGoogleDnsIp);
		serv.sin_port = htons(kDnsPort);

		int err = connect(sock, (const sockaddr*)&serv, sizeof(serv));

		sockaddr_in name;
		socklen_t namelen = sizeof(name);
		err = getsockname(sock, (sockaddr*)&name, &namelen);

		const char* p = inet_ntop(AF_INET, &name.sin_addr, buffer, buflen);

		closesocket(sock);//close(sock);
	}
	static CString getlocalip()
	{
		CFixedStringT<CString, 32> l_strIp = _T("?");
		char name[256];
		GetPrimaryIp(name, 256);
		l_strIp = name;
		return l_strIp;
	}*/

	static CString GetIpAddressWithZeros()
	{
		CFixedStringT<CString, 32> l_strIp = GetIpAddress();

		TCHAR l_str[16];
		int fin1 = l_strIp.Find('.');
		int ini2 = fin1 + 1;
		int fin2 = l_strIp.Find('.', ini2);
		int ini3 = fin2 + 1;
		int fin3 = l_strIp.Find('.', ini3);
		mro::mikefmt(	l_str, _T("%03s.%03s.%03s.%03s"), l_strIp.Mid(0, fin1), l_strIp.Mid(ini2, fin2 - ini2), 
						l_strIp.Mid(ini3, fin3 - ini3), l_strIp.Mid(fin3 + 1));
		return l_str;
	}
};

int gen_service( TCHAR* data, const TCHAR* type, const TCHAR* comp, 
							const TCHAR* funname, const TCHAR* pkey, const TCHAR* pvalue);
int gen_service( TCHAR* data, const TCHAR* type, const TCHAR* comp, 
							const TCHAR* funname, mro::CParameters* params, mro::CParameters* values);

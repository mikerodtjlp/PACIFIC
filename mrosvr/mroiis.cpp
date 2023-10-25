#include "stdafx.h"

//#include "\mrosys\mrocore\mroiis.h"

#pragma comment(lib, "wininet.lib")

// Download a file.   
//  
// Pass the URL of the file to url.  
//  
// To reload a file, pass true to reload.  
//  
// To specify an update function that is called after  
// each buffer is read, pass a pointer to that  
// function as the third parameter.  If no update  
// function is desired, then let the third parameter  
// default to null.  
bool Download::download(TCHAR *url, bool reload,   const TCHAR* destination,
						void (*update)(unsigned long, unsigned long)) 
{  
  ofstream fout;				// output stream  
  unsigned char buf[BUF_SIZE];	// input buffer  
  unsigned long numrcved;		// number of bytes read  
  unsigned long filelen;		// length of file on disk  
  HINTERNET hIurl, hInet;		// Internet handles  
  unsigned long contentlen;		// length of content  
  unsigned long len;			// length of contentlen  
  unsigned long total = 0;		// running total of bytes received  
  TCHAR header[80];				// holds Range header  
  
  try {  
	if(!ishttp(url))  
	  throw DLExc(_T("must_HTTP_url"));  
  
	// Open the file specified by url.  
	// The open stream will be returned  
	// in fout.  If reload is true, then  
	// any preexisting file will be truncated.  
	// The length of any preexisting file (after   
	// possible truncation) is returned.  
	filelen = openfile(url, reload, fout, destination);  
  
	// See if Internet connection available.  
	if(InternetAttemptConnect(0) != ERROR_SUCCESS)   
	  throw DLExc(_T("cant_connect"));  
  
	// Open Internet connection.  
	hInet = InternetOpen(_T("downloader"), INTERNET_OPEN_TYPE_DIRECT, NULL, NULL, 0);  
  
	if(hInet == NULL)   
	  throw DLExc(_T("cant_open_connection"));  
  
	// Construct header requesting range of data.  
	if(filelen == 0) header[0]=0;
	else _stprintf(header, _T("Range:bytes=%d-"), filelen);   
  
	// Open the URL and request range.  
	hIurl = InternetOpenUrl(hInet, url, header, -1, INTERNET_FLAG_NO_CACHE_WRITE, 0);  
  
	if(hIurl == NULL) throw DLExc(_T("cant_open_url"));  
  
	// Confirm that HTTP/1.1 or greater is supported.  
	if(!httpverOK(hIurl))  
	  throw DLExc(_T("HTTP/1.1_not_supported"));  
	 
	// Get content length.  
	len = sizeof contentlen;  
	if(!HttpQueryInfo(hIurl,  
					  HTTP_QUERY_CONTENT_LENGTH |  
					  HTTP_QUERY_FLAG_NUMBER,  
					  &contentlen, &len, NULL))  
	  throw DLExc(_T("file_or_content_length_not_found"));  
  
	// If existing file (if any) is not complete,  
	// then finish downloading.  
	if(filelen != contentlen && contentlen)   
	  do {  
		// Read a buffer of info.  
		if(!InternetReadFile(hIurl, &buf,  
							 BUF_SIZE, &numrcved))  
		  throw DLExc(_T("during_download"));  
  
		 // Write buffer to disk.  
		 fout.write((const char *) buf, numrcved);  
		 if(!fout.good())   
		   throw DLExc(_T("writing_file"));  
	   
		 total += numrcved; // update running total  
  
		 // Call update function, if specified.  
		 if(update && numrcved > 0)  
		   update(contentlen+filelen, total+filelen);  
  
	  } while(numrcved > 0);  
	else  
	  if(update)  
		update(filelen, filelen);  
  
  } 
  catch(DLExc) 
  {  
	fout.close();  
	InternetCloseHandle(hIurl);  
	InternetCloseHandle(hInet);  
	throw; // rethrow the exception for use by caller  
  }  
  
  fout.close();  
  InternetCloseHandle(hIurl);  
  InternetCloseHandle(hInet);  
  
  return true;  
}  
  
// Return true if HTTP version of 1.1 or greater.  
bool Download::httpverOK(HINTERNET hIurl) 
{  
  TCHAR str[80];  
  unsigned long len = 79;  
  
  // Get HTTP version.  
  if(!HttpQueryInfo(hIurl, HTTP_QUERY_VERSION, &str, &len, NULL))  
	return false;  
 
  // First, check major version number.  
  TCHAR *p = _tcschr(str, '/'); 
  ++p; 
  if(*p == '0') return false; // can use HTTP 0.x 
 
  // Now, find start of minor HTTP version number.  
  p = _tcschr(str, '.');  
  ++p;  
  
  // Convert to int.  
  int minorVerNum = _tstoi(p);  
  
  if(minorVerNum > 0) return true;  
  return false;  
}  

/**
 * Extract the filename from the URL.  Return false if  
 * the filename cannot be found.  
 */
bool Download::getfname(TCHAR *url, TCHAR *fname) 
{  
	// Find last /.  
	TCHAR *p = _tcsrchr(url, '/');  
  
	// Copy filename after the last /.   
	if(p && (_tcslen(p) < MAX_FILENAME_SIZE)) 
	{  
		++p;  
		_tcscpy_s(fname, MAX_FILENAME_SIZE, p);  
		return true;  
	}  
	else return false;  
}  
  
/**
 * Open the output file, initialize the output  
 * stream, and return the file's length.  If  
 * reload is true, first truncate any preexisting  
 * file.  
 */
unsigned long Download::openfile(TCHAR *url,  
								 bool reload,  
								 ofstream &fout,
								 const TCHAR* destination) 
{  
  TCHAR fname[MAX_FILENAME_SIZE];  

	if(destination)
	{
		int len = _tcslen(destination);
		_tmemcpy(fname, destination, len);
		fname[len] = '\0';
	}
	else
	{
		if(!getfname(url, fname))   
		throw DLExc(_T("file_name_error"));  
	}
  
	if(!reload)   
		fout.open(fname, ios::binary | ios::out |  
						 ios::app | ios::ate);    
	else  
		fout.open(fname, ios::binary | ios::out |  
						 ios::trunc);    
  
	if(!fout)  
		throw DLExc(_T("cant_open_output_file"));    
  
	// Get current file length.  
	return fout.tellp();  
}  
 
/**
 * Confirm that the URL specifies HTTP.  
 */
bool Download::ishttp(TCHAR *url) 
{  
  TCHAR str[5] = _T("");  
  
  // Get first four characters from URL.  
  _tcsnccpy(str, url, 4);  
  
  // Convert to lowercase  
  for(TCHAR *p=str; *p; p++) *p = tolower(*p);  
	
  return !_tcscmp(_T("http"), str);  
}














//////////////////////////////
bool Download::upload(TCHAR* server, TCHAR* upfile, TCHAR* tofile)
{
	INTERNET_BUFFERS BufferIn = {0};
	DWORD dwBytesRead;
	DWORD dwBytesWritten;
	BYTE pBuffer[1024]; // Read from file in 1K chunks
	BOOL bRead, bRet;
	HINTERNET hInternet;
	HINTERNET hConnect;
	HINTERNET hRequest;

	 BufferIn.dwStructSize = sizeof( INTERNET_BUFFERS );

	// See if Internet connection available.  
	if(InternetAttemptConnect(0) != ERROR_SUCCESS)   
	  throw DLExc(_T("cant_connect"));  
  
	if(!(hInternet  = InternetOpen(_T("uploader"),  
									INTERNET_OPEN_TYPE_DIRECT,  NULL, NULL,  0)))
	  throw DLExc(_T("cant_connect"));  

	if(!(hConnect = InternetConnect(hInternet, server, 
									INTERNET_DEFAULT_FTP_PORT,//8309,//INTERNET_DEFAULT_FTP_PORT, 
									_T("Anonymous"),
									_T("dcsuser@sola.com"),
									INTERNET_SERVICE_FTP, 0, 0)))
	  throw DLExc(_T("cant_connect"));  

	if(!FtpPutFile(hConnect, upfile, tofile, FTP_TRANSFER_TYPE_BINARY, 0))
	   throw DLExc(_T("error"));

	InternetCloseHandle(hConnect);
	InternetCloseHandle(hInternet);
	return TRUE;
}

int Download::simple_download(const TCHAR *url, TCHAR* destinybuff)
{
	if(!destinybuff) return 0;
	destinybuff[0] = 0;

  unsigned char buf[BUF_SIZE + 1];	// input buffer  
  unsigned long numrcved;			// number of bytes read  
  HINTERNET hIurl, hInet;			// Internet handles  
  unsigned long contentlen;			// length of content  
  unsigned long total = 0;			// running total of bytes received  
  
  try {  
  
	// See if Internet connection available.  
	if(InternetAttemptConnect(0) != ERROR_SUCCESS)   
	  throw DLExc(_T("cant_connect"));  
  
	// Open Internet connection.  
	hInet = InternetOpen(	_T("downloader"),  
							INTERNET_OPEN_TYPE_DIRECT,  NULL, NULL,  0);  
  
	if(hInet == NULL)   
	  throw DLExc(_T("cant_open_connection"));  

	DWORD dwTimeoutSecs = 50 * 1000;
	InternetSetOption(hInet, INTERNET_OPTION_CONNECT_TIMEOUT, &dwTimeoutSecs, 4);

	// Open the URL and request range.  
	hIurl = InternetOpenUrl(hInet, url, NULL, -1, INTERNET_FLAG_NO_CACHE_WRITE, 0);  
  
	if(hIurl == NULL) throw DLExc(_T("cant_open_url"));  
  
	// Confirm that HTTP/1.1 or greater is supported.  
	if(!httpverOK(hIurl))  
	  throw DLExc(_T("HTTP/1.1_not_supported"));  
	 
	bool isunicode = false;
	bool firstround = true;
	  do {  
		// Read a buffer of info.  
		if(!InternetReadFile(hIurl, &buf, BUF_SIZE, &numrcved))  
		  throw DLExc(_T("during_download"));  

		// check if it is a unicode request, we check only the first package and 
		// if it so we clean the unicode's mark, in other to clean the request
		if(	firstround &&	((unsigned char)buf[0]) == MRO_UNICODE_BOM[0] &&
							((unsigned char)buf[1]) == MRO_UNICODE_BOM[1])
		{ 
			isunicode = true;
			firstround = false;
		}

		if(isunicode)
			MultiByteToWideChar( CP_ACP, 0, (LPCSTR)buf, numrcved, (LPWSTR)&destinybuff[total], numrcved);
		else
		{
			memcpy(&destinybuff[total], buf, numrcved);
		}

		 total += numrcved; // update running total  
	  } while(numrcved > 0);  
  
  } 
  catch(DLExc) 
  {  
	InternetCloseHandle(hIurl);  
	InternetCloseHandle(hInet);  
  
	throw; // rethrow the exception for use by caller  
  }  
  
  InternetCloseHandle(hIurl);  
  InternetCloseHandle(hInet);  
  
  return total;  
}

void Download::simple_download(const TCHAR *url, cpairs& destinybuff)
{
	CString result;
	simple_download(url, result);
	destinybuff.set_value(result);
}

void Download::simple_download(const TCHAR *url, CString& destinybuff)
{
	destinybuff.Empty();

  unsigned char buf[BUF_SIZE + 1]; // input buffer  
  unsigned long numrcved;  // number of bytes read  
  HINTERNET hIurl, hInet;  // Internet handles  
  unsigned long contentlen;// length of content  
  unsigned long total = 0; // running total of bytes received  
  
  try 
  {  
  
	// See if Internet connection available.  
	if(InternetAttemptConnect(0) != ERROR_SUCCESS)   
	  throw DLExc(_T("cant_connect"));  
  
	// Open Internet connection.  
	hInet = InternetOpen(	_T("downloader"),  
							INTERNET_OPEN_TYPE_DIRECT,  NULL, NULL,  0);  
  
	if(hInet == NULL)   
	  throw DLExc(_T("cant_open_connection"));  
  
	DWORD dwTimeoutSecs = 50 * 1000;
	InternetSetOption(hInet, INTERNET_OPTION_CONNECT_TIMEOUT, &dwTimeoutSecs, 4);

	// Open the URL and request range.  
	hIurl = InternetOpenUrl(hInet, url,  NULL, -1,  INTERNET_FLAG_NO_CACHE_WRITE, 0);  
  
	if(hIurl == NULL) throw DLExc(_T("cant_open_url"));  
  
	// Confirm that HTTP/1.1 or greater is supported.  
	if(!httpverOK(hIurl))  
	  throw DLExc(_T("HTTP/1.1_not_supported"));  

	bool isunicode = false;
	bool firstround = true;
	  do {  
		// Read a buffer of info.  
		if(!InternetReadFile(hIurl, &buf, BUF_SIZE, &numrcved))  
		  throw DLExc(_T("during_download"));  

		// check if it is a unicode request, we check only the first package and 
		// if it so we clean the unicode's mark, in other to clean the request
		if(	firstround &&	((unsigned char)buf[0]) == MRO_UNICODE_BOM[0] &&
							((unsigned char)buf[1]) == MRO_UNICODE_BOM[1])
			isunicode = true;

		firstround = false;

		if(isunicode)
			destinybuff.Append((TCHAR*)buf, numrcved / 2);
		else
		{
			buf[numrcved] = 0;
			destinybuff  += (char*)buf;
		}

		 total += numrcved; // update running total  
	  } while(numrcved > 0);  
  
  } catch(DLExc) {  
	InternetCloseHandle(hIurl);  
	InternetCloseHandle(hInet);  
  
	throw; // rethrow the exception for use by caller  
  }  
  
  InternetCloseHandle(hIurl);  
  InternetCloseHandle(hInet);  
}


int UnicodeToAnsi(LPCWSTR s, int cw, CHAR* psz)
{
if (s==NULL) return 0;//NULL;
//int cw=lstrlenW(s);
if (cw==0) return 0;//{CHAR *psz=new CHAR[1];*psz='\0';return psz;}
int cc=WideCharToMultiByte(CP_ACP,0,s,cw,NULL,0,NULL,NULL);
if (cc==0) return 0;//NULL;
//CHAR *psz=new CHAR[cc+1];
cc=WideCharToMultiByte(CP_ACP,0,s,cw,psz,cc,NULL,NULL);
if (cc==0) 
{
//	delete[] psz;
	return 0;//NULL;
}
psz[cc]='\0';
return cc;
}

void Download::check_internet(const TCHAR* agent)
{
	HINTERNET hInet;
	try
	{
		hInet = InternetOpen(agent,  INTERNET_OPEN_TYPE_DIRECT,  NULL, NULL,  0);  
		if(hInet == NULL)	throw DLExc(_T("Can't open connection."));  
	} 
	catch(DLExc) 
	{  
		InternetCloseHandle(hInet);  
		throw; // rethrow the exception for use by caller  
	}  
 
	InternetCloseHandle(hInet);  
}

void Download::simple_download_post(const TCHAR *url, 
									cpairs& destinybuff,
									UINT* packets,
									UINT* nchars)
{
	CString result;
	simple_download_post(url, result, packets, nchars);
	destinybuff.set_value(result);
}

void Download::simple_download_post(const TCHAR *url, 
									CString& destinybuff,
									UINT* packets,
									UINT* nchars)
{
	destinybuff.Empty();

	HINTERNET hIurl, hInet;  // Internet handles  
	HINTERNET hIConn;
	unsigned long contentlen;// length of content  
	unsigned long total = 0; // running total of bytes received  

	TCHAR server[64];
	TCHAR prt[8];
	TCHAR page[512];

	TCHAR* start	= (TCHAR*)_tcsstr(url, _T("//")); start += 2;
	TCHAR* sep		= (TCHAR*)_tcsstr(start, _T(":"));
	TCHAR* slash	= (TCHAR*)_tcsstr(sep, _T("/")); 
	TCHAR* frmdata	= (TCHAR*)_tcsstr(slash, _T("?")); 
	TCHAR* frmdata2 = (TCHAR*)_tcsstr(frmdata+1, _T("?"));
	if(frmdata2) frmdata = frmdata2;

	_tmemcpy(server, start, sep-start);
	server[sep-start] = 0;
	++sep;

	_tmemcpy(prt, sep, slash-sep);
	prt[slash-sep] = 0;
	int port = _ttoi(prt);
	++slash;

	_tmemcpy(page, slash, frmdata-slash);
	page[frmdata-slash] = 0;
	++frmdata;

	try 
	{  
		// Open Internet connection.  
		hInet = InternetOpen(_T("mrogui2"),  INTERNET_OPEN_TYPE_DIRECT,  NULL, NULL,  0);  
		if(hInet == NULL)	throw DLExc(_T("Can't open connection."));  
  
		DWORD dwTimeoutconn = 30 * 1000;
		DWORD dwTimeoutreci = 180 * 1000;
		InternetSetOption(hInet, INTERNET_OPTION_CONNECT_TIMEOUT, &dwTimeoutconn, sizeof(dwTimeoutconn));
		InternetSetOption(hInet, INTERNET_OPTION_RECEIVE_TIMEOUT, &dwTimeoutreci, sizeof(dwTimeoutreci));


		// Open the URL and request range.  
		hIConn = InternetConnect(hInet, server, port, NULL,  NULL,  INTERNET_SERVICE_HTTP,  0,  1);
		if(hIConn == NULL)	throw DLExc(_T("InternetConnect"));  
  
		hIurl = HttpOpenRequest(  hIConn,  _T("POST"),  page,  _T("HTTP/1.1"),  NULL,  NULL,  INTERNET_FLAG_RELOAD,  NULL);
		if(hIurl == NULL) throw DLExc(_T("Can't open url."));  

		static TCHAR hdrs[] = _T("Content-Type: application/x-www-form-urlencoded");
		// it is safe use stack memory because, the reason for using the webservice by post is because the GET
		// does not allow more the 2000 characters on the message, and unsing POST we can send the message apart
		// so the URL always be short, less than 2000 chracters, so it is save to use this memory strategy
		int hdrlen = 47;
		int packlen = _tcslen(frmdata);

#ifdef UNICODE
		CHAR asc[8192];
		CHAR* pasc = asc;
		if(packlen > 8192)
			pasc = (CHAR*)alloca(sizeof(CHAR)*(packlen + 1));

		int asclen = UnicodeToAnsi((LPCWSTR)frmdata, packlen, pasc);
		if(!HttpSendRequest(hIurl, hdrs, hdrlen, pasc, asclen)) 
#else
		if(!HttpSendRequest(hIurl, hdrs, hdrlen, frmdata, packlen)) 
#endif
		{
			DWORD err = ::GetLastError();
			if(err == ERROR_INTERNET_TIMEOUT) throw DLExc(_T("webservice timeout"));
			throw DLExc(_T("HttpSendRequest"));  
		}

		// Confirm that HTTP/1.1 or greater is supported.  
		//if(!httpverOK(hIurl))  throw DLExc(_T("HTTP/1.1 not supported."));  

		//WinHttpReceiveResponse( hIurl, NULL);

#define MAXROUNDS 64
		int bufstp=0;
		unsigned char buf[(64+1)*MAXROUNDS]; // input buffer  
		unsigned long numrcved;				// number of bytes read  
		bool isunicode = false;
		int round = 0;

		do 
		{  
			// Read a buffer of info.  
			if(!InternetReadFile(hIurl, &buf[bufstp], 64, &numrcved))  
				throw DLExc(_T("Error occurred during download."));  

			// check if it is a unicode request, we check only the first package and 
			// if it so we clean the unicode's mark, in other to clean the request
			if(	round == 0 &&	((unsigned char)buf[bufstp  ]) == MRO_UNICODE_BOM[0] &&
								((unsigned char)buf[bufstp+1]) == MRO_UNICODE_BOM[1])
				isunicode = true;

			bufstp += numrcved;
			bool pass2str = (round!=0 && round%(MAXROUNDS-1)==0) || numrcved == 0;

			if(isunicode)
			{
				if(pass2str) destinybuff.Append((TCHAR*)buf, numrcved / 2);
			}
			else
			{
				if(pass2str)
				{ 
					if(bufstp>0)
					{
						buf[bufstp] = 0;
						if(round<=MAXROUNDS && destinybuff.IsEmpty()) destinybuff = CA2W((LPCSTR)buf, CP_UTF8);
						else destinybuff += CA2W((LPCSTR)buf, CP_UTF8);
					}
				}
			}
			if(pass2str) bufstp=0;

			total += numrcved; // update running total  
			++round;
		} 
		while(numrcved > 0);  
  
		if(packets) *packets=round;
		if(nchars) *nchars=total;
	} 
	catch(DLExc) 
	{  
		InternetCloseHandle(hIurl);  
		InternetCloseHandle(hIConn);  
		InternetCloseHandle(hInet);  
		throw; // rethrow the exception for use by caller  
	}  
  
	InternetCloseHandle(hIurl);  
	InternetCloseHandle(hIConn);  
	InternetCloseHandle(hInet);  
}

bool Download::url_alive(TCHAR* url, const DWORD timeout)
{  
  HINTERNET hIurl, hInet;  // Internet handles  
  TCHAR header[80];         // holds Range header  
  
  try {  
	if(!ishttp(url))  
	  throw DLExc(_T("Must be HTTP url."));  
  
	// See if Internet connection available.  
	if(InternetAttemptConnect(0) != ERROR_SUCCESS)   
	  throw DLExc(_T("Can't connect."));  
  
	// Open Internet connection.  
	hInet = InternetOpen(_T("downloader"), INTERNET_OPEN_TYPE_DIRECT,  NULL, NULL,  0);  
  
	if(hInet == NULL)   
	  throw DLExc(_T("Can't open connection."));  
  
	// Construct header requesting range of data.  
	_stprintf(header, _T("Range:bytes=%d-"), 0);   

	// manage time out if is it supplied
	if(timeout)
	{
		DWORD tmout = timeout * 1000;
		InternetSetOption(hInet, INTERNET_OPTION_CONNECT_TIMEOUT, &tmout, sizeof(tmout));
	}

	// Open the URL and request range.  
	hIurl = InternetOpenUrl(hInet, url, header, -1,  INTERNET_FLAG_NO_CACHE_WRITE, 0);  
  
	if(hIurl == NULL) throw DLExc(_T("Can't open url."));  
  } 
  catch(DLExc) 
  {  
	InternetCloseHandle(hIurl);  
	InternetCloseHandle(hInet);  
	return false;
  }  
  
  InternetCloseHandle(hIurl);  
  InternetCloseHandle(hInet);  
  return true;  
}  

/*
bool Download::upload(TCHAR* server, TCHAR* upfile, TCHAR* tofile)
{
	INTERNET_BUFFERS BufferIn = {0};
	DWORD dwBytesRead;
	DWORD dwBytesWritten;
	BYTE pBuffer[1024]; // Read from file in 1K chunks
	BOOL bRead, bRet;
	HINTERNET hInternet;
	HINTERNET hConnect;
	HINTERNET hRequest;

	 BufferIn.dwStructSize = sizeof( INTERNET_BUFFERS );

	// See if Internet connection available.  
	if(InternetAttemptConnect(0) != ERROR_SUCCESS)   
	  throw DLExc(_T("Can't connect."));  
  
	if(!(hInternet  = InternetOpen(_T("uploader"),  
									INTERNET_OPEN_TYPE_DIRECT,  NULL, NULL,  0)))
	  throw DLExc(_T("Can't open."));  

	if(!(hConnect = InternetConnect(hInternet, server, 
									INTERNET_DEFAULT_HTTP_PORT, 
//									_T(""), _T(""), 
									_T("IUSR_OSM-DCSSERVER"),// user
									_T("notiene"),// pass
									INTERNET_SERVICE_HTTP, 0, 0)))
	  throw DLExc(_T("Can't connect."));  

	 if(!(hRequest = HttpOpenRequest (hConnect, _T("PUT"),
										tofile, NULL, NULL, NULL,  0, 0)))
	 {
		DWORD e = GetLastError();
		throw DLExc(_T("some error"));
	 }

	 HANDLE hFile = CreateFile (upfile, GENERIC_READ, FILE_SHARE_READ,
								NULL, OPEN_EXISTING, FILE_ATTRIBUTE_NORMAL, NULL);
	 if(hFile == INVALID_HANDLE_VALUE)
		 throw DLExc(_T("file not found"));

	 BufferIn.dwBufferTotal = GetFileSize (hFile, NULL);

	 if(!HttpSendRequestEx( hRequest, &BufferIn, NULL, HSR_INITIATE, 0))
	 {
		DWORD e = GetLastError();
		throw DLExc(_T("some error"));
	 }

	 DWORD sum = 0;
	 do
	 {
		if(!(bRead = ReadFile (hFile, pBuffer, sizeof(pBuffer), &dwBytesRead, NULL)))
			throw DLExc(_T("some error"));

if(dwBytesRead != 0)
{
		if(!(bRet=InternetWriteFile( hRequest, pBuffer, dwBytesRead, &dwBytesWritten)))
			throw DLExc(_T("some error"));

		sum += dwBytesWritten;
}
	 }
	 while (dwBytesRead == sizeof(pBuffer)) ;

	 CloseHandle (hFile);

	 if(!HttpEndRequest(hRequest, NULL, 0, 0))
	   throw DLExc(_T("some error"));

	InternetCloseHandle(hConnect);
	InternetCloseHandle(hInternet);
	InternetCloseHandle(hRequest);
	return TRUE;
}

int Download::simple_download(const TCHAR *url, TCHAR* destinybuff)
{
	if(!destinybuff) return 0;
	destinybuff[0] = 0;

  unsigned char buf[BUF_SIZE + 1]; // input buffer  
  unsigned long numrcved;  // number of bytes read  
  HINTERNET hIurl, hInet;  // Internet handles  
  unsigned long contentlen;// length of content  
  unsigned long total = 0; // running total of bytes received  
  
  try {  
  
	// See if Internet connection available.  
	if(InternetAttemptConnect(0) != ERROR_SUCCESS)   
	  throw DLExc(_T("Can't connect."));  
  
	// Open Internet connection.  
	hInet = InternetOpen(	_T("downloader"),  
							INTERNET_OPEN_TYPE_DIRECT,  NULL, NULL,  0);  
  
	if(hInet == NULL)   
	  throw DLExc(_T("Can't open connection."));  
  
	// Open the URL and request range.  
	hIurl = InternetOpenUrl(hInet, url,  NULL, -1,  INTERNET_FLAG_NO_CACHE_WRITE, 0);  
  
	if(hIurl == NULL) throw DLExc(_T("Can't open url."));  
  
	// Confirm that HTTP/1.1 or greater is supported.  
	if(!httpverOK(hIurl))  
	  throw DLExc(_T("HTTP/1.1 not supported."));  
	 
	bool isunicode = false;
	bool firstround = true;
	  do {  
		// Read a buffer of info.  
		if(!InternetReadFile(hIurl, &buf, BUF_SIZE, &numrcved))  
		  throw DLExc(_T("Error occurred during download."));  

		// check if it is a unicode request, we check only the first package and 
		// if it so we clean the unicode's mark, in other to clean the request
		if(	firstround &&	((unsigned char)buf[0]) == MRO_UNICODE_BOM[0] &&
							((unsigned char)buf[1]) == MRO_UNICODE_BOM[1])
		{ 
			isunicode = true;
			firstround = false;
		}

		if(isunicode)
			MultiByteToWideChar( CP_ACP, 0, (LPCSTR)buf, numrcved, (LPWSTR)&destinybuff[total], numrcved);
		else
		{
			memcpy(&destinybuff[total], buf, numrcved);
		}

		 total += numrcved; // update running total  
	  } while(numrcved > 0);  
  
  } catch(DLExc) {  
	InternetCloseHandle(hIurl);  
	InternetCloseHandle(hInet);  
  
	throw; // rethrow the exception for use by caller  
  }  
  
  InternetCloseHandle(hIurl);  
  InternetCloseHandle(hInet);  
  
  return total;  
}

void Download::simple_download(const TCHAR *url, CString& destinybuff)
{
	destinybuff.Empty();

  unsigned char buf[BUF_SIZE + 1]; // input buffer  
  unsigned long numrcved;  // number of bytes read  
  HINTERNET hIurl, hInet;  // Internet handles  
  unsigned long contentlen;// length of content  
  unsigned long total = 0; // running total of bytes received  
  
  try {  
  
	// See if Internet connection available.  
	if(InternetAttemptConnect(0) != ERROR_SUCCESS)   
	  throw DLExc(_T("Can't connect."));  
  
	// Open Internet connection.  
	hInet = InternetOpen(	_T("downloader"),  
							INTERNET_OPEN_TYPE_DIRECT,  NULL, NULL,  0);  
  
	if(hInet == NULL)   
	  throw DLExc(_T("Can't open connection."));  
  
	// Open the URL and request range.  
	hIurl = InternetOpenUrl(hInet, url,  NULL, -1,  INTERNET_FLAG_NO_CACHE_WRITE, 0);  
  
	if(hIurl == NULL) throw DLExc(_T("Can't open url."));  
  
	// Confirm that HTTP/1.1 or greater is supported.  
	if(!httpverOK(hIurl))  
	  throw DLExc(_T("HTTP/1.1 not supported."));  

	bool isunicode = false;
	bool firstround = true;
	  do {  
		// Read a buffer of info.  
		if(!InternetReadFile(hIurl, &buf, BUF_SIZE, &numrcved))  
		  throw DLExc(_T("Error occurred during download."));  

		// check if it is a unicode request, we check only the first package and 
		// if it so we clean the unicode's mark, in other to clean the request
		if(	firstround &&	((unsigned char)buf[0]) == MRO_UNICODE_BOM[0] &&
							((unsigned char)buf[1]) == MRO_UNICODE_BOM[1])
			isunicode = true;

		firstround = false;

		if(isunicode)
			destinybuff.Append((TCHAR*)buf, numrcved / 2);
		else
		{
			buf[numrcved] = 0;
			destinybuff  += (char*)buf;
		}

		 total += numrcved; // update running total  
	  } while(numrcved > 0);  
  
  } catch(DLExc) {  
	InternetCloseHandle(hIurl);  
	InternetCloseHandle(hInet);  
  
	throw; // rethrow the exception for use by caller  
  }  
  
  InternetCloseHandle(hIurl);  
  InternetCloseHandle(hInet);  
}*/
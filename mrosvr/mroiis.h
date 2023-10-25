#pragma once

#include <iostream>  
#include <windows.h>  
#include <wininet.h>  
#include <fstream>  
#include <cstdio>  
  
using namespace std;  
  
const int MAX_ERRMSG_SIZE	= 60;  
const int MAX_EXTMSG_SIZE	= 20;
const int MAX_FILENAME_SIZE = 512;  
const int BUF_SIZE			= 1024;  
  
// Exception class for download errors.  
class DLExc 
{  
  TCHAR err[MAX_ERRMSG_SIZE];
  TCHAR ext[MAX_EXTMSG_SIZE];

public:  

  DLExc(const TCHAR *exc) 
  {  
	if(_tcslen(exc) < MAX_ERRMSG_SIZE)  
	  _tcscpy_s(err, exc);
	ext[0] = 0;
  }  
  DLExc(const TCHAR *exc, const TCHAR* exx) 
  {  
	if(_tcslen(exc) < MAX_ERRMSG_SIZE)  
	  _tcscpy_s(err, exc);
	if(_tcslen(exx) < MAX_EXTMSG_SIZE)  
	  _tcscpy_s(ext, exx);
  }    
  // Return a pointer to the error message.  
  const TCHAR * geterr() {  return err;  }  
  const TCHAR * getext() {  return ext;  }  
};  
  
// A class for downloading files from the Internet.  
class Download 
{  
  static bool ishttp(TCHAR *url);  
  static bool httpverOK(HINTERNET hIurl);  
  static bool getfname(TCHAR *url, TCHAR *fname);  
  static unsigned long openfile(TCHAR *url, bool reload,  ofstream &fout, 
								const TCHAR* destination = NULL);  
public:  
	static void check_internet(const TCHAR* agent);
	static bool url_alive(TCHAR *url, const DWORD timeout = 0);

	static bool download(TCHAR *url, bool restart=false,  const TCHAR* destination = NULL,
						void (*update)(unsigned long, unsigned long)=NULL);  

	static bool upload(TCHAR* server, TCHAR* sourcefile, TCHAR* tofolder); 


	static int simple_download(const TCHAR *url, TCHAR* destinybuff);  
	static void simple_download(const TCHAR *url, CString& destinybuff);  
	static void simple_download(const TCHAR *url, mro::CParameters& destinybuff);  
	static void simple_download_post(const TCHAR *url, CString& destinybuff,
									UINT* packets=nullptr,
									UINT* nchars=nullptr);  
	static void simple_download_post(const TCHAR *url, mro::CParameters& destinybuff,
									UINT* packets=nullptr,
									UINT* nchars=nullptr);  
};  


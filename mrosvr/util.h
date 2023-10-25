#pragma once

//#include <winsock2.h>
#include <sys/stat.h>
#include <io.h>

#define defchar(variable, len) TCHAR variable[len]; variable[0] = 0;

namespace mro {

	int gethome(TCHAR* foldermain);

	struct sMemGblManager
	{
		TCHAR* pmem[2];
		int len[2];
	};
	class memhelper
	{
	public:
		static void initialize_memhelper(const unsigned maxprocs);
		static void get_mem_from_gbl_manager(TCHAR** presult, const int lon, const int procplace, const int slot=0);
		static sMemGblManager* memgbl;
	};

	int  mikefmtA(char* p, const char* fmt, ...);
	int  mikefmt(TCHAR* p, const TCHAR* fmt, ...);

	int  strfmt1(TCHAR* p, const TCHAR* ftm, const TCHAR* val, const int fmtlen=0, const int vallen=0);
	int  strfmt2(TCHAR* p, const TCHAR* ftm, const TCHAR* val1, const TCHAR* val2, 
							const int fmtlen=0, const int val1len=0, const int val2len=0);

	int  s_ifmt1(TCHAR* p, const TCHAR* ftm, const TCHAR* sval, const int ival, 
									const int fmtlen=0, const int vallen=0);
	int  intfmt1(TCHAR* p, const TCHAR* ftm, const int val, const int fmtlen=0);
	int  intfmt2(TCHAR* p, const TCHAR* ftm, const int val1, const int val2, const int fmtlen=0);
	int  chrfmt1(TCHAR* p, const TCHAR* ftm, const int val, const int fmtlen=0);

	CString int_to_str(const int p_iValue);
	//CString double_to_str(const double p_iValue);

	bool create_process(const TCHAR* process, DWORD priority = NORMAL_PRIORITY_CLASS, const TCHAR* currdir = nullptr, const TCHAR* name = nullptr);

	bool exist_file(const TCHAR* filename);
	bool is_unicode_file(const TCHAR* filename);
	CString get_user_name();
	bool write_event(DWORD type, LPTSTR message);
	void get_exename(TCHAR* gexename);


	int get_process_running_count(const TCHAR* processname);
	bool evaluate_downloaded_file(const TCHAR* to);
	void show_file(const TCHAR* file, const TCHAR* specific);
	void clear_folder(const TCHAR* folder, const TCHAR* filter, const int lessthan);
	void split_address(CParameters& lastconfig, const TCHAR* node, const int nl, 
									TCHAR* server, const TCHAR* keysvr,
									TCHAR* port, const TCHAR* keyprt,
									TCHAR* domain, const TCHAR* keydom);
	int gen_tot_list(TCHAR* p, const int lid, const int nrows, 
					const TCHAR* ex = nullptr, const int exl = 0);
	int get_value_len(const TCHAR* p);

	struct mdefs
	{
		static TCHAR* cols[64];
		static int colslen[64];
		static TCHAR* vars[64];
		static int varslen[64];
	};
}

/*class CMroFile
{
public:
	CMroFile() {};
	CMroFile(const CString& p_str_file, const CString& p_str_args) : _p_file(0) { open(p_str_file, p_str_args); };
	CMroFile(const CString& p_str_file, const CString& p_str_args, const TCHAR* p_str_text)  : _p_file(0) { open(p_str_file, p_str_args); write(p_str_text); };
	virtual ~CMroFile() { close(); };

	inline void open(const CString& p_str_file, const CString& p_str_args) {
		if((_p_file = _tfopen(p_str_file, p_str_args)) == 0)
			throw CString(_T("error in file"));
	};

	inline void close() const { if(_p_file) fclose(_p_file); };
	inline void write(const CString& p_str_text) { fwrite(p_str_text, p_str_text.GetLength(),1,_p_file); };
	inline static void remove(const CString& p_str_file) { _tremove(p_str_file); };
	inline bool isopen() { return _p_file != 0; };

	static bool exist(const CString& p_str_file) { try { CMroFile l_temp(p_str_file, _T("r")); return l_temp.isopen(); } catch(CString e) { return false;} };
private:
	FILE* _p_file;
};*/

/*class CMroLocalLog : public CMroFile
{
public:
	CMroLocalLog(const TCHAR* p_str_file) : CMroFile(), _str_file(p_str_file) {};
	CMroLocalLog(const TCHAR* p_str_file, const CString& p_str_text) : CMroFile(), _str_file(p_str_file) { write(p_str_text); };
	CMroLocalLog(const TCHAR* p_str_file, const CString& p_str_text, const CString& p_str_final_text) : CMroFile(), _str_file(p_str_file), _str_final_text(p_str_final_text) { write(p_str_text); };
	~CMroLocalLog() { write(_str_final_text); };
	inline void write(const CString& p_str_text) { CMroFile::open(_str_file, _T("a")); CMroFile::write(p_str_text); CMroFile::close(); };

private:
	CString _str_file;
	CString _str_final_text;
};*/

/*class CSessionManagment : public ISessionMan
{
public:
	CSessionManagment() { CreateDispatch("mroctrl.SessionMan"); };
	~CSessionManagment() { ReleaseDispatch(); };
};*/

//class CFind2ID : public IFindID
//{
//public:
	//CFind2ID() { CreateDispatch(_T("dcsutil.findid")); };
	//~CFind2ID() { ReleaseDispatch(); };
//};

/*class COSeekObj : public COleDispatchDriver
{
public:
	COSeekObj(){} // Calls COleDispatchDriver default constructor
	COSeekObj(LPDISPATCH pDispatch) : COleDispatchDriver(pDispatch) {}
	COSeekObj(const COSeekObj& dispatchSrc) : COleDispatchDriver(dispatchSrc) {}

	// Attributes
public:

	// Operations
public:


	// ISeekObj methods
public:
	void SetParameters(LPCTSTR p_strParameters)
	{
		static BYTE parms[] = VTS_BSTR ;
		InvokeHelper(0x1, DISPATCH_METHOD, VT_EMPTY, NULL, parms, p_strParameters);
	}
	CString GetParameters()
	{
		CString result;
		InvokeHelper(0x2, DISPATCH_METHOD, VT_BSTR, (void*)&result, NULL);
		return result;
	}
	short DoOk(LPCTSTR p_strParameters)
	{
		short result;
		static BYTE parms[] = VTS_BSTR ;
		InvokeHelper(0x3, DISPATCH_METHOD, VT_I2, (void*)&result, parms, p_strParameters);
		return result;
	}
	void SetBasics(LPCTSTR p_strParameters)
	{
		static BYTE parms[] = VTS_BSTR;
		InvokeHelper(0x4, DISPATCH_METHOD, VT_EMPTY, NULL, parms, p_strParameters);
	}
	CString GetValuesToChange()
	{
		CString result;
		InvokeHelper(0x5, DISPATCH_METHOD, VT_BSTR, (void*)&result, NULL);
		return result;
	}
	CString getlog()
	{
		CString result;
		InvokeHelper(0x6, DISPATCH_METHOD, VT_BSTR, (void*)&result, NULL);
		return result;
	}

	// ISeekObj properties
public:

};*/

/*class OSeekObj : public COSeekObj
{
public:
	OSeekObj(const CString& system__) 
	{ 
		require(!CreateDispatch(_T("dcsutl.SeekObj.") + system__), _T("could not create the com dcsutl.SeekObj.") + system__);
	};
	OSeekObj() { if(!CreateDispatch(_T("dcsutl.SeekObj"))) AfxMessageBox(_T("error dcsutl.seekobj")); };
	~OSeekObj() { ReleaseDispatch(); };
};*/

//class CAjusteQty : public IAjuste
//{
//public:
	//CAjusteQty() { CreateDispatch("dcsutil.ajuste"); };
	//~CAjusteQty() { ReleaseDispatch(); };
//};

//class CMroMessage : public IMessage
//{
//public:
	//CMroMessage() { CreateDispatch("dcsutil.message"); };
	//~CMroMessage() { ReleaseDispatch(); };
//};

//CString get_status_desc(const int status, const CString& p_str_language, bool p_b_long_description = true);

//class CMroSystem
//{
//	static HANDLE DDBToDIB( CBitmap& bitmap, DWORD dwCompression, CPalette* pPal );
//	static BOOL WriteDIB( CString szFile, HANDLE hDIB);

//public:

//	static CString get_os()
//	{
//		CString strFmt, Temp;
//		CString strSistemaOp;
//		OSVERSIONINFO OsVersionInfo;
//		OsVersionInfo.dwOSVersionInfoSize = sizeof(OSVERSIONINFO);
//		GetVersionEx(&OsVersionInfo);
//		switch (OsVersionInfo.dwPlatformId)
//		{
//		case VER_PLATFORM_WIN32s: Temp = _T("windows 3.1"); break;
//		case VER_PLATFORM_WIN32_WINDOWS: 
//				if(OsVersionInfo.dwMinorVersion == 0)
//					Temp = _T("windows 95"); 
//				if(OsVersionInfo.dwMinorVersion == 10)
//					Temp = _T("windows 98"); 
//				break;
//		case VER_PLATFORM_WIN32_NT: Temp = _T("windows NT"); break;
//		default: Temp = _T("unknown");
//		}
//		strFmt = _T("%s, version %d.%d");
//		strSistemaOp.Format(strFmt, Temp, OsVersionInfo.dwMajorVersion,OsVersionInfo.dwMinorVersion);
//		return strSistemaOp;
//	}

//	static CString get_processor()
//	{
//		CString strFmt, Temp;

//		CString strInfoSis;
//		SYSTEM_INFO SystemInfo;
//		GetSystemInfo(&SystemInfo);
//		switch(SystemInfo.dwProcessorType)
//		{
//		case PROCESSOR_INTEL_386: Temp = _T("intel 386"); break;
//		case PROCESSOR_INTEL_486: Temp = _T("intel 486"); break;
//		case PROCESSOR_INTEL_PENTIUM: Temp = _T("intel pentium"); break;
//		default: Temp = _T("unknown");
//		}
//		strFmt = _T("%s");
//		strInfoSis.Format(strFmt, Temp);
//		return strInfoSis;
//	}

//	static CString get_ram_disp()
//	{
//		CString strFmt, Temp;

		// Memoria fisica y virtual
//		CString strMemory;
//		MEMORYSTATUS MemStat;
//		MemStat.dwLength = sizeof(MEMORYSTATUS);
//		GlobalMemoryStatus(&MemStat);
//		//Memoria RAM disponible
//		strFmt = _T("%ld KB");
//		strMemory.Format(strFmt,MemStat.dwAvailPhys / 1024L);
//		return strMemory;
//	}

//	static CString get_ram_tot()
//	{
//		CString strFmt, Temp;
//
//		// Memoria fisica y virtual
//		CString strMemory;
//		MEMORYSTATUS MemStat;
//		MemStat.dwLength = sizeof(MEMORYSTATUS);
//		GlobalMemoryStatus(&MemStat);
//		//Memoria RAM total
//		strFmt = _T("%ld KB");
//		strMemory.Format(strFmt,MemStat.dwTotalPhys / 1024L);
//		return strMemory;
//	}

//	static bool is_mouse_over(CView* obj, UINT ctrl_id)
//	{
//		CPoint	p;
//		GetCursorPos(&p);
//		CRect	wr;
//		obj->GetDlgItem(ctrl_id)->GetWindowRect(wr);
//		return static_cast<bool>(wr.PtInRect(p));
//	}

//	static bool get_class_id( CString p_str_object_name, CLSID& l_p_clsid)
//	{
//		CLSID l_Clsid;
//		TCHAR* l_charName = NULL;
//		l_charName = p_str_object_name.GetBuffer(p_str_object_name.GetLength());
//		LPWSTR l_wstrName = new wchar_t[strlen(l_charName)+1];
//		MultiByteToWideChar(CP_ACP,0,l_charName,strlen(l_charName)+1,l_wstrName,strlen(l_charName)+1);
//		HRESULT l_h_result = CLSIDFromProgID(l_wstrName,&l_Clsid);
//		delete l_wstrName;
//		l_p_clsid = l_Clsid;
//		return (l_h_result == NOERROR);
//	}
//	static bool is_registered(const CString& p_str_object_name)
//	{ 
//		CLSID l_no_use;
//		return get_class_id(p_str_object_name, l_no_use);
//	}
//	static CString IntToStr(const int p_iValue, const int p_iFillCount = 1)
//	{
//		TCHAR fill[16];
//		mro::mikefmt(fill, _T("%%0%dd"), p_iFillCount);
//		TCHAR result[16];
//		mro::mikefmt(result, fill, p_iValue);
//		return result;
//	}

//	static CString get_user_name()
//	{
//		TCHAR   szBuffer[MAX_COMPUTERNAME_LENGTH + 1];
//		DWORD  dwNameSize = MAX_COMPUTERNAME_LENGTH + 1;
//		GetUserName( szBuffer, &dwNameSize );
//		return CString(szBuffer);
//	}

//	static COleDateTime StrToDateTime(const CString& str_time)
//	{
//		if(str_time.GetLength() == 17)
//			return COleDateTime(2000+atoi(str_time.Mid(0,2)),atoi(str_time.Mid(3,2)),atoi(str_time.Mid(6,2)),atoi(str_time.Mid(9,2)),atoi(str_time.Mid(12,2)),atoi(str_time.Mid(15,2)));
//		if(str_time.GetLength() == 19)
//			return COleDateTime(atoi(str_time.Mid(0,4)),atoi(str_time.Mid(5,2)),atoi(str_time.Mid(8,2)),atoi(str_time.Mid(11,2)),atoi(str_time.Mid(14,2)),atoi(str_time.Mid(17,2)));
//		return COleDateTime(0,0,0,0,0,0);
//	}
//};

/**
 * description      : header file for sockes
 * author           : miguel rodriguez ojeda
 * date             : febreaury 10 2003
 *
 * modification     : ...
 *
 */
class webdata
{
public:
	webdata() {};
	webdata(	TCHAR* server, const int serverlen, const int port, 
				TCHAR* service,const int svrlen, 
				bool isonenter,const TCHAR* eventn, const int evlen, 
				const TCHAR* header, const TCHAR* basics, CWnd* wnd, 
				const int id, DWORD start, const bool retres, 
				const unsigned long actionno, const int synctype) 
	{ 
		init(server,serverlen, port,service,svrlen,
			isonenter,eventn,evlen,
			header,basics,wnd,id,start,retres, actionno, synctype); 
	}
	void init(	TCHAR* server, const int serverlen, const int port, 
				TCHAR* service,const int svrlen, 
				bool isonenter,const TCHAR* eventn, const int evlen, 
				const TCHAR* header, const TCHAR* basics, CWnd* wnd, 
				const int id, DWORD start, const bool retres, 
				const unsigned long actionno, const int synctype)
	{ 
		if(serverlen) _tmemcpy(this->server,server,(this->serverlen=serverlen)+1);
		this->port = port;

		this->isonenter = isonenter;
		_tmemcpy(this->eventn, eventn, evlen);
		this->evlen = evlen;
		this->header.SetString(header);
		this->basics.SetString(basics);
		this->service.Empty();
		if(svrlen) this->service.SetString(service, svrlen); 
		this->result.Empty();
		this->hwnd = wnd;
		this->trans_id = id;
		this->start = start;
		this->retres = retres;
		this->packs = 0;
		this->nchars = 0;
		this->actionno = actionno;
		this->synctype = synctype;
		this->url.Empty();
	};
	TCHAR server[16];
	int serverlen;
	int port;

	bool isonenter;
	TCHAR eventn[65];
	int evlen;
	CString basics;
	CString header;
	CString service;
	CString result;
	CString url;
	CWnd* hwnd;
	int trans_id;
	DWORD start;
	bool retres;
	UINT packs;
	UINT nchars;
	unsigned long actionno;
	int synctype;
};

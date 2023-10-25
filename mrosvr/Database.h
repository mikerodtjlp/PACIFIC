#pragma once

#if defined __mro_lib__
	#define __mro_decl_type__ __declspec(dllexport)
#else
	#if defined __mro_include_code__
		#define __mro_decl_type__ 
	#else
		#define __mro_decl_type__  __declspec(dllimport)
	#endif
#endif

#define CATCHERROR(ptr,a)	catch(_com_error &e)\
							{\
								ErrorHandler(e,_str_error);\
								ptr=NULL;\
								return a;\
							}

#define CATCHERRGET			catch(_com_error &e)\
							{\
								ErrorHandler(e,_str_error);\
								mikefmt(_str_error, _T("%s\n**For Field Name:%s"),_str_error,FieldName);\
								return 0;\
							}

// when compile check/update this path, in order to work
//#import "c:\Program Files\Common Files\System\ADO\msado15.dll" \
//              rename("EOF", "EndOfFile")
#import "C:\mrosys\PACIFIC\db\msado60_Backcompat.tlb" \
              rename("EOF", "EndOfFile")

typedef  ADODB::_RecordsetPtr	RecPtr;
typedef ADODB::_ConnectionPtr	CnnPtr; 

class Table;

class cConnection
{
public:
	CnnPtr m_Cnn;
	TCHAR _str_error[1024];
	TCHAR _str_query_source[64];
	int _conid;

	cConnection();
	~cConnection() 	{ if(m_Cnn) { Close(); m_Cnn = nullptr; } }

	bool Open(TCHAR* UserName, TCHAR* Pwd,TCHAR* CnnStr, const int timeout = 60);
	bool Close();
	bool OpenTbl(int Mode, TCHAR* CmdStr, Table& Tbl);
	void execute(const TCHAR* CmdStr);
	void execute(const TCHAR* CmdStr, Table& Tbl);
//	void GetErrorErrStr(TCHAR* ErrStr);
	inline CString get_err() { return _str_error; };
	void settimeout(long seconds);
	ULONGLONG lastaccess;
};

class Table
{
public:
	RecPtr m_Rec;
	TCHAR _str_error[512];
	Table();

	inline CString get_err() { return _str_error; };

	bool IsEOF();
	bool IsBOF();
	bool is_value_null(const TCHAR* fieldname);

	HRESULT MoveNext();
	HRESULT MovePrevious();
	HRESULT MoveFirst();
	HRESULT MoveLast();
	int AddNew();
	int Update();
	int Add(TCHAR* FieldName, TCHAR* FieldValue);
	int Add(TCHAR* FieldName,int FieldValue);
	int Add(TCHAR* FieldName,float FieldValue);
	int Add(TCHAR* FieldName,double FieldValue);
	int Add(TCHAR* FieldName,long FieldValue);

	bool Get(TCHAR* FieldName, TCHAR* FieldValue);
	bool Get(TCHAR* FieldName, CString& FieldValue);
	bool Get(TCHAR* FieldName,int& FieldValue);
	bool Get(TCHAR* FieldName,float& FieldValue);
	bool Get(TCHAR* FieldName,double& FieldValue);
	bool Get(TCHAR* FieldName,double& FieldValue,int Scale);
	bool Get(TCHAR* FieldName,long& FieldValue);

	_variant_t get_variant(const int index);

	int get(const TCHAR* FieldName, TCHAR* res, const int maxres=0);
	CString get(const TCHAR* FieldName);
	void get(const TCHAR* FieldName, CString& res);

	int getint(const TCHAR* FieldName);
	BYTE getbyte(const TCHAR* FieldName);
	long getlong(const TCHAR* FieldName);
	float getfloat(const TCHAR* FieldName);
	double getdouble(const TCHAR* FieldName);
	void getdate(const TCHAR* FieldName, COleDateTime& result);
	COleDateTime getdate(const TCHAR* FieldName);
	CString getdatedt(const TCHAR* FieldName);
	CString getdated(const TCHAR* FieldName);
	CString getdatet(const TCHAR* FieldName);
	void getdatedt(const TCHAR* FieldName, CString& dt);

	int get(const int index, TCHAR* result, const int maxres=0);
	CString get(const int index);
	int get(const int index, CString& res);

	int getint(const int index);
	BYTE getbyte(const int index);
	long getlong(const int index);
	float getfloat(const int index);
	double getdouble(const int index);
	void getdate(const int index, COleDateTime& result);
	COleDateTime getdate(const int index);
	CString getdatedt(const int index);
	CString getdated(const int index);
	CString getdatet(const int index);
	void getdatedt(const int index, CString& dt);

	long get_column_count();
	ADODB::DataTypeEnum get_column_type(const int col__);
	CString  get_column_name(const int col__);
	int get_column_name(const int col, TCHAR* result);

	int get_ws_type(UINT id, ADODB::DataTypeEnum type, TCHAR* value);
};

/*
	this class is intended only for embed some useful tracking information
	to the query, nothing more nothing less, because it is a lot of useful
	to monitor and see exactly who is doing what on the database server
	the other thing embedded is the handling exception support this make
	more modern and robust the sql processing on the database sever
*/
class cCommand
{
public:
	cCommand(CParameters& _basics) 
	{ 
		// the header
		set4ch(header, '-', '-', ' ', ' '); len = 4;
		len += _basics.get(ZMACNAM, &header[len], ZMACNAMLEN);
		header[len++] = ' ';
		len += _basics.get(ZUSERID, &header[len], ZUSERIDLEN);
		header[len++] = ' ';
		len += _basics.get(ZTRNCOD, &header[len], ZTRNCODLEN);
		set4ch(&header[len], ' ', ' ', '\n', 0);
		len += 3;

		// we add the exeception handling begin
		set4ch(&header[len], 'b', 'e', 'g', 'i');	len += 4;
		set4ch(&header[len], 'n', ' ', 't', 'r');	len += 4;
		set2ch(&header[len], 'y', '\n');			len += 2;

		footlen = mikefmt(footer,	_T("\nend try\n")
									_T("begin catch\n")
										_T("declare @errmsg varchar(256)\n")
										_T("set @errmsg = ERROR_MESSAGE()\n")
										_T("RAISERROR (@errmsg, 16,1)\n")
									_T("end catch\n"));
	}

	void Format(const TCHAR *pszFormat, ...)
	{
		va_list argList;
		va_start( argList, pszFormat );
		_helper.FormatV( pszFormat, argList );
		va_end( argList );

		_data.SetString(header, len);
		_data.Append(_helper);
		TCHAR* p = (TCHAR*) _tmemchr(_data.GetBuffer(),  _T('&'), _data.GetLength());
		if(p)
		{
			_helper.Replace(_T("&lt;"), _T("   <"));
			_helper.Replace(_T("&gt;"), _T("   >"));
		}
		_data.Append(footer, footlen);
	}

	void operator = (const TCHAR* str) 
	{ 
		_data.SetString(header, len); 
		_data.Append(str);
		TCHAR* p = (TCHAR*) _tmemchr(_data.GetBuffer(),  _T('&'), _data.GetLength());
		if(p)
		{
			_helper.Replace(_T("&lt;"), _T("   <"));
			_helper.Replace(_T("&gt;"), _T("   >"));
		}
		_data.Append(footer, footlen);
	}

	operator const TCHAR*() { return _data.GetBuffer(); }
	operator const CString() { return _data; }

	void Replace(const TCHAR* o, const TCHAR* n) { _data.Replace(o, n); }
	CString _data;
	CString _helper;
	int len;
	TCHAR header[128];
	int footlen;
	TCHAR footer[256];
};

#define getconnection(con)\
	dbhelper dbmanager##con(_basics.getint(ZPROCNO, ZPROCNOLEN));\
	cConnection& con = dbmanager##con.get_db_from_gbl_manager();

#define getconnectionx(con, query)\
	dbhelper dbmanager##con(_basics.getint(ZPROCNO, ZPROCNOLEN));\
	cConnection& con = dbmanager##con.get_db_from_gbl_manager();\
	Table& query = dbmanager##con.get_qry_from_gbl_manager();

struct sDBGblManager
{
	cConnection* pcon;
	Table* pqry;
	int pos;
	int top;
};
class dbhelper
{
public:
	static void initialize_dbhelper(const unsigned maxprocs, const int maxconns);
	static void initialize_stack(const int place);
	static sDBGblManager* dbgbl;
	static TCHAR connection[1024];
	static long timeout;

	static int maxconnections;

	explicit dbhelper(const int place) : procplace(place) {}
	~dbhelper() { ret_db_to_gbl_manager(); }
	cConnection& get_db_from_gbl_manager();
	Table& get_qry_from_gbl_manager();
	void ret_db_to_gbl_manager();

	int procplace;
};


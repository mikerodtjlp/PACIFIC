#include "stdafx.h"

//#include <stdio.h>
//#include <iostream.h>
#include <comdef.h>
#include <conio.h>
#include "Database.h"


ADODB::_RecordsetPtr rec1=NULL;

_variant_t  vtMissing1(DISP_E_PARAMNOTFOUND, VT_ERROR); 

void ErrorHandler(_com_error &e, TCHAR* ErrStr) {
//	mikefmt(ErrStr,"Error:\n");
//	mikefmt(ErrStr,_T("[errcode:%08lx][errdesc:%s]"),e.Error(), (TCHAR*) e.Description());
	mikefmt(ErrStr,_T("%08lx %s"),e.Error(), (TCHAR*) e.Description());
//	mikefmt(ErrStr,_T("%s%s;"), ErrStr, (TCHAR*) e.Description());
//	mikefmt(ErrStr,_T("%scode=%08lx;"),ErrStr ,e.Error());
//	mikefmt(ErrStr,_T("%scode meaning=%s;"), ErrStr, (TCHAR*) e.ErrorMessage());
//	mikefmt(ErrStr,_T("%src=%s;"), ErrStr, (TCHAR*) e.Source());
//	mikefmt(ErrStr,"%sDescription = %s",ErrStr, (TCHAR*) e.Description());
}

cConnection::cConnection() {
	m_Cnn=NULL;
	_str_error[0] = _T('\0');
	lastaccess = GetTickCount64();
	_conid = -1;
}

bool cConnection::Open(TCHAR* UserName, TCHAR* Pwd, TCHAR* CnnStr, const int timeout) {
	_conid = -1;
	//cnn->Open(strCnn,"sa","sa",NULL);
	try	{
		HRESULT hr;
		hr    = m_Cnn.CreateInstance( __uuidof( ADODB::Connection ) );
		m_Cnn->put_ConnectionTimeout(timeout);
		m_Cnn->Open(CnnStr, UserName, Pwd, NULL);

//Table tbl;
//RecPtr t_Rec = m_Cnn->Execute(_T("select @@SPID as conid"), NULL, 1);
//tbl.m_Rec=t_Rec;
//_conid = tbl.getint(_T("conid"));
	}
	catch(_com_error &e) {
		ErrorHandler(e,_str_error);
		m_Cnn = 0;
		_conid = -1;
		return false;
	}
//	CATCHERROR(m_Cnn, _str_error)

	return true;
}

bool cConnection::Close() {
	try	{
		if(m_Cnn) { m_Cnn->Close(); m_Cnn = 0; _conid = -1; }
	}
	catch(_com_error &e) {
	}
	return true;
}

bool cConnection::OpenTbl(int Mode, TCHAR* CmdStr, Table &Tbl) {
	if(m_Cnn==NULL)	{
		Tbl.m_Rec=NULL;
		mikefmt(_str_error,_T("invalid_Connection"));
		return 0;
	}
	RecPtr t_Rec=NULL;
	try	{
		//t_Rec->putref_ActiveConnection(m_Cnn);
		//vtMissing<<-->>_variant_t((IDispatch *) m_Cnn, true)
		t_Rec.CreateInstance( __uuidof( ADODB::Recordset ) );
		t_Rec->Open(CmdStr,_variant_t((IDispatch *) m_Cnn, true),ADODB::adOpenStatic,ADODB::adLockOptimistic,Mode);
	}
	
	CATCHERROR(Tbl.m_Rec,0)

	Tbl.m_Rec=t_Rec;
	_str_error[0] = _T('\0');
	return 1;
}

void cConnection::execute(const TCHAR* CmdStr) {
	try	{
		require(m_Cnn == 0, _T("connection_not_open"));
		m_Cnn->Execute(CmdStr, NULL, 1);
	}
	catch(TCHAR* e)	{
		if(m_Cnn != 0) Close();
		m_Cnn = 0;
		int l = wcsnlen_s(CmdStr, 1024);
		if (l > 64) l = 64;
		_tmemcpy(_str_error, CmdStr, l);
		set2ch(&_str_error[l], ' ', ' ');
		_tcscpy_s(&_str_error[l+2], 1024-(l+2), e);
		require(true, get_err());
	}
	catch(_com_error &e) {
		if(m_Cnn != 0) Close();
		m_Cnn = 0;
		int l = wcsnlen_s(CmdStr, 1024);
		if (l > 64) l = 64;
		_tmemcpy(_str_error, CmdStr, l);
		set2ch(&_str_error[l], ' ', ' ');
		ErrorHandler(e, &_str_error[l+2]);
		require(true, get_err());
	}
	catch(CException *e) {
		if(m_Cnn != 0) Close();
		m_Cnn = 0;
		TCHAR szCause[1024];
		e->GetErrorMessage(szCause,1024);
		e->Delete();
		int l = wcsnlen_s(CmdStr, 1024);
		if (l > 64) l = 64;
		_tmemcpy(_str_error, CmdStr, l);
		set2ch(&_str_error[l], ' ', ' ');
		_tcscpy_s(&_str_error[l+2], 1024-(l+2), szCause);
		require(true, get_err());
	}
	catch(mroerr& e) {
		if(m_Cnn != 0) Close();
		m_Cnn = 0;
		int l = wcsnlen_s(CmdStr, 1024);
		if (l > 64) l = 64;
		_tmemcpy(_str_error, CmdStr, l);
		set2ch(&_str_error[l], ' ', ' ');
		_tcscpy_s(&_str_error[l+2], 1024-(l+2), e.description);
		require(true, get_err());
	}
	catch(...) {
		if(m_Cnn != 0) Close();
		m_Cnn = 0;
		int l = wcsnlen_s(CmdStr, 1024);
		if (l > 64) l = 64;
		_tmemcpy(_str_error, CmdStr, l);
		set2ch(&_str_error[l], ' ', ' ');
		_tcscpy_s(&_str_error[l+2], 1024-(l+2), _T("unhanled_error_ADO"));
		require(true, get_err());
	}
	_str_error[0] = _T('\0');
}

void cConnection::execute(const TCHAR* CmdStr, Table& Tbl) {
	RecPtr t_Rec=NULL;
	try	{
		require(m_Cnn == 0, _T("connection_not_open"));
		t_Rec = m_Cnn->Execute(CmdStr, NULL, 1);
	}
	catch(TCHAR* e)	{
		if(m_Cnn != 0) Close();
		m_Cnn = 0;
		int l = wcsnlen_s(CmdStr, 1024);
		if (l > 64) l = 64;
		_tmemcpy(_str_error, CmdStr, l);
		set2ch(&_str_error[l], ' ', ' ');
		_tcscpy_s(&_str_error[l+2], 1024-(l+2), e);
		Tbl.m_Rec = NULL;
		require(true, get_err());
	}
	catch(_com_error &e) {
		if(m_Cnn != 0) Close();
		m_Cnn = 0;
		int l = wcsnlen_s(CmdStr, 1024);
		if (l > 64) l = 64;
		_tmemcpy(_str_error, CmdStr, l); 
		set2ch(&_str_error[l], ' ', ' ');
		ErrorHandler(e, &_str_error[l+2]);
		Tbl.m_Rec = NULL;
		require(true, get_err());
	}
	catch(CException *e) {
		if(m_Cnn != 0) Close();
		m_Cnn = 0;
		TCHAR szCause[1024];
		e->GetErrorMessage(szCause,1024);
		e->Delete();
		int l = wcsnlen_s(CmdStr, 1024);
		if (l > 64) l = 64;
		_tmemcpy(_str_error, CmdStr, l);
		set2ch(&_str_error[l], ' ', ' ');
		_tcscpy_s(&_str_error[l+2], 1024-(l+2), szCause);
		require(true, get_err());
	}
	catch(mroerr& e) {
		if(m_Cnn != 0) Close();
		m_Cnn = 0;
		int l = wcsnlen_s(CmdStr, 1024);
		if (l > 64) l = 64;
		_tmemcpy(_str_error, CmdStr, l);
		set2ch(&_str_error[l], ' ', ' ');
		_tcscpy_s(&_str_error[l+2], 1024-(l+2), e.description);
		require(true, get_err());
	}
	catch(...) {
		if(m_Cnn != 0) Close();
		m_Cnn = 0;
		int l = wcsnlen_s(CmdStr, 1024);
		if (l > 64) l = 64;
		_tmemcpy(_str_error, CmdStr, l);
		set2ch(&_str_error[l], ' ', ' ');
		_tcscpy_s(&_str_error[l+2], 1024-(l+2), _T("unhanled_error_ADO"));
		require(true, get_err());
	}
	Tbl.m_Rec=t_Rec;
	_str_error[0] = _T('\0');
}

void cConnection::settimeout(long seconds) {
	try	{
		m_Cnn->put_CommandTimeout(seconds);
	}
	catch(_com_error &e) {
		throw CString((TCHAR*)e.Description());
	}
	_str_error[0] = _T('\0');
}

Table::Table() {
	m_Rec=NULL;
}

bool Table::IsEOF() {
	bool rs;
	if(m_Rec==NULL)	{
		_tcscpy_s(_str_error, _T("invalid_record"));
		return true;//-1;
	}
	try {
		rs=m_Rec->EndOfFile;
	}
	
	CATCHERROR(m_Rec, _str_error)

	_str_error[0] = _T('\0');
	return rs;
}

bool Table::IsBOF() {
	bool rs;
	if(m_Rec==NULL)	{
		_tcscpy_s(_str_error, _T("invalid_record"));
		return true;//-1;
	}
	try {
		rs=m_Rec->BOF;
	}
	
	CATCHERROR(m_Rec, _str_error)

	_str_error[0] = _T('\0');
	return rs;
}

long Table::get_column_count() {
	return m_Rec->GetFields()->GetCount();
}

int Table::get_ws_type(UINT i, ADODB::DataTypeEnum type, TCHAR* value) {
	switch(type) {
	case ADODB::adSmallInt: return mikefmt(value, _T("%ld"), (long)getbyte(i));
	case ADODB::adInteger: return mikefmt(value, _T("%ld"), (long)getint(i)); 

	case ADODB::adDate:
	case ADODB::adDBDate:
	case ADODB::adDBTime:
	case ADODB::adDBTimeStamp: return mikefmt(value, _T("%s"), getdate(i).Format(_T("%Y/%m/%d %H:%M:%S")));

	case ADODB::adWChar:
	case ADODB::adVarWChar:
	case ADODB::adLongVarWChar:
	case ADODB::adBSTR:
	case ADODB::adChar:
	case ADODB::adVarChar:
	case ADODB::adLongVarChar: return mikefmt(value, _T("%s"), get(i));

	case ADODB::adDouble: return mikefmt(value, _T("%f"), (float)getdouble(i));
	case ADODB::adBinary: return mikefmt(value, _T("%ld"), (long)getbyte(i));
	default: return mikefmt(value, _T("%ld"), (long)getbyte(i));
	}
}

ADODB::DataTypeEnum Table::get_column_type(const int col__) {
	_variant_t index;
	index.vt = VT_I2;
	index.iVal = col__;

	ADODB::FieldsPtr pFldProp = m_Rec->GetFields();
	ADODB::FieldPtr pp = pFldProp->GetItem(index);
	ADODB::DataTypeEnum result = pp->GetType();
	return result;
}

int Table::get_column_name(const int col__, TCHAR* result) {
	_variant_t index;
	index.vt = VT_I2;
	index.iVal = col__;

	ADODB::FieldsPtr pFldProp = m_Rec->GetFields();
	ADODB::FieldPtr pp = pFldProp->GetItem(index);

	return mikefmt(result, _T("%s"),  (LPCTSTR)((_bstr_t)pp->GetName()));
}

CString Table::get_column_name(const int col__) {
	_variant_t index;
	index.vt = VT_I2;
	index.iVal = col__;

	ADODB::FieldsPtr pFldProp = m_Rec->GetFields();
	ADODB::FieldPtr pp = pFldProp->GetItem(index);

	CString result = (LPCTSTR)((_bstr_t)pp->GetName());
//	CString result = (LPCSTR)((_bstr_t)pp->GetName());
//	CString result = ((_bstr_t)pp->GetName());
	return result;
}

bool Table::Get(TCHAR* FieldName, TCHAR* FieldValue) {
	try	{
		_variant_t  vtValue = m_Rec->Fields->GetItem(FieldName)->GetValue();
		if(vtValue.vt == VT_NULL) return false;
		mikefmt(FieldValue, _T("%s"),(LPCTSTR)((_bstr_t)vtValue.bstrVal));
	}

	CATCHERRGET

	_str_error[0] = _T('\0');
	return 1;
}

bool Table::Get(TCHAR* FieldName, CString& FieldValue) {
	try	{
		_variant_t  vtValue = m_Rec->Fields->GetItem(FieldName)->GetValue();
		if(vtValue.vt == VT_NULL) return false;
		FieldValue.Format(_T("%s"),(LPCTSTR)((_bstr_t)vtValue.bstrVal));
	}

	CATCHERRGET

	_str_error[0] = _T('\0');
	return 1;
}

bool Table::Get(TCHAR* FieldName,int& FieldValue) {
	try	{
		_variant_t  vtValue = m_Rec->Fields->GetItem(FieldName)->GetValue();
		if(vtValue.vt == VT_NULL) return false;
		FieldValue=vtValue.iVal;
	}

	CATCHERRGET

	_str_error[0] = _T('\0');
	return 1;
}

bool Table::Get(TCHAR* FieldName,float& FieldValue) {
	try	{
		_variant_t  vtValue = m_Rec->Fields->GetItem(FieldName)->GetValue();
		if(vtValue.vt == VT_NULL) return false;
		FieldValue=vtValue.fltVal;
	}

	CATCHERRGET

	_str_error[0] = _T('\0');
	return 1;
}

bool Table::Get(TCHAR* FieldName,double& FieldValue) {
	try	{
		_variant_t  vtValue = m_Rec->Fields->GetItem(FieldName)->GetValue();
		if(vtValue.vt == VT_NULL) return false;
		FieldValue=vtValue.dblVal;
		//GetDec(vtValue,FieldValue,3);
	}

	CATCHERRGET

	_str_error[0] = _T('\0');
	return 1;
}

_variant_t Table::get_variant(const int index) {
	_variant_t FieldName;
	FieldName.vt = VT_I2;
	FieldName.iVal = index;

	_variant_t  vtValue;
	vtValue.vt = VT_NULL;

	try	{
		vtValue = m_Rec->Fields->GetItem(FieldName)->GetValue();
	}
	catch(_com_error &e)
	{
		ErrorHandler(e, _str_error);
		mikefmt(_str_error, _T("%s\n**For Field Name:%s"),_str_error,FieldName);
	}
	return vtValue;
}

int Table::get(const int index, TCHAR* result, const int maxres) {
	int res = 0;
	_variant_t FieldName;
	FieldName.vt = VT_I2;
	FieldName.iVal = index;

	try	{
		_variant_t  vtValue = m_Rec->Fields->GetItem(FieldName)->GetValue();
		if(vtValue.vt != VT_NULL)
		{
			res = (reinterpret_cast<UINT*>(vtValue.bstrVal)[-1])/(sizeof(TCHAR));
			if(maxres > 0 && res > maxres) res = maxres;
			_tmemcpy(result, (LPCTSTR)((_bstr_t)vtValue.bstrVal), res);
			result[res]=0;
			//res = mikefmt(result, _T("%s"),(LPCTSTR)((_bstr_t)vtValue.bstrVal));
			_str_error[0] = _T('\0');
		}
		else res = result[0] = _str_error[0] = _T('\0');
	}
	catch(_com_error &e)
	{
		ErrorHandler(e, _str_error);
		mikefmt(_str_error, _T("%s\n**For Field Name:%s"),_str_error,FieldName);
		result[0] = _T('\0');
		res = 0;
	}
	return res;
}

int Table::get(const TCHAR* FieldName, TCHAR* result, const int maxres) {
	int res = 0;
	try	{
		_variant_t  vtValue = m_Rec->Fields->GetItem(FieldName)->GetValue();
		if(vtValue.vt != VT_NULL) {
			res = (reinterpret_cast<UINT*>(vtValue.bstrVal)[-1])/(sizeof(TCHAR));
			if(maxres > 0 && res > maxres) res = maxres;
			_tmemcpy(result, (LPCTSTR)((_bstr_t)vtValue.bstrVal), res);
			result[res]=0;
//			res = mikefmt(result, _T("%s"),(LPCTSTR)((_bstr_t)vtValue.bstrVal));
			_str_error[0] = _T('\0');
		}
		else res = result[0] = _str_error[0] = _T('\0');
	}
	catch(_com_error &e) {
		ErrorHandler(e, _str_error);
		mikefmt(_str_error, _T("%s\n**For Field Name:%s"),_str_error,FieldName);
		result[0] = _T('\0');
		res = 0;
	}
	return res;
}

CString Table::get(const TCHAR* FieldName) {
	CString result = _T("");
	try	{
		_variant_t  vtValue = m_Rec->Fields->GetItem(FieldName)->GetValue();
		if(vtValue.vt != VT_NULL) {
			int res = (reinterpret_cast<UINT*>(vtValue.bstrVal)[-1])/(sizeof(TCHAR));
			result.SetString((LPCTSTR)((_bstr_t)vtValue.bstrVal), res);
//			result.Format(_T("%s"),(LPCTSTR)((_bstr_t)vtValue.bstrVal));
			_str_error[0] = _T('\0');
		}
		return result;
	}
//	CATCHERRGET
	catch(_com_error &e) {
		ErrorHandler(e,_str_error);
		mikefmt(_str_error, _T("%s\n**For Field Name:%s"),_str_error,FieldName);
		return result;
	}
}

CString Table::get(const int index) {
	_variant_t FieldName;
	FieldName.vt = VT_I2;
	FieldName.iVal = index;

	CString result = _T("");
	try {
		_variant_t  vtValue = m_Rec->Fields->GetItem(FieldName)->GetValue();
		if(vtValue.vt != VT_NULL) {
			int res = (reinterpret_cast<UINT*>(vtValue.bstrVal)[-1])/(sizeof(TCHAR));
			result.SetString((LPCTSTR)((_bstr_t)vtValue.bstrVal), res);
//			result.Format(_T("%s"),(LPCTSTR)((_bstr_t)vtValue.bstrVal));
			_str_error[0] = _T('\0');
		}
		return result;
	}
//	CATCHERRGET
	catch(_com_error &e) {
		ErrorHandler(e,_str_error);
		mikefmt(_str_error, _T("%s\n**For Field Name:%s"),_str_error,FieldName);
		return result;
	}
}

void Table::get(const TCHAR* FieldName, CString& result) {
	result.Empty();
	try	{
		_variant_t  vtValue = m_Rec->Fields->GetItem(FieldName)->GetValue();
		if(vtValue.vt != VT_NULL) {
//			result.Format(_T("%s"),(LPCTSTR)((_bstr_t)vtValue.bstrVal));
			int res = (reinterpret_cast<UINT*>(vtValue.bstrVal)[-1])/(sizeof(TCHAR));
			result.SetString((LPCTSTR)((_bstr_t)vtValue.bstrVal), res);
			_str_error[0] = _T('\0');
		}
	}
//	CATCHERRGET
	catch(_com_error &e) {
		ErrorHandler(e,_str_error);
		mikefmt(_str_error, _T("%s\n**For Field Name:%s"),_str_error,FieldName);
	}
}

int Table::get(const int index, CString& result) {
	_variant_t FieldName;
	FieldName.vt = VT_I2;
	FieldName.iVal = index;
	result.Empty();
	int len = 0;
	try	{
		_variant_t  vtValue = m_Rec->Fields->GetItem(FieldName)->GetValue();
		if(vtValue.vt != VT_NULL) {
			len = (reinterpret_cast<UINT*>(vtValue.bstrVal)[-1])/(sizeof(TCHAR));
			result.SetString((LPCTSTR)((_bstr_t)vtValue.bstrVal), len);
			_str_error[0] = _T('\0');
		}
	}
//	CATCHERRGET
	catch(_com_error &e) {
		ErrorHandler(e,_str_error);
		mikefmt(_str_error, _T("%s\n**For Field Name:%s"),_str_error,FieldName);
	}
	return len;
}

int Table::getint(const TCHAR* FieldName) {
	int result = -1;
	try	{
		_variant_t  vtValue = m_Rec->Fields->GetItem(FieldName)->GetValue();
		if(vtValue.vt == VT_NULL) return result;
		result = vtValue.iVal;
	}

	CATCHERRGET

	_str_error[0] = _T('\0');
	return result;
}
int Table::getint(const int index) {
	_variant_t FieldName;
	FieldName.vt = VT_I2;
	FieldName.iVal = index;

	int result = -1;
	try	{
		_variant_t  vtValue = m_Rec->Fields->GetItem(FieldName)->GetValue();
		if(vtValue.vt == VT_NULL) return result;
		result = vtValue.iVal;
	}

	CATCHERRGET

	_str_error[0] = _T('\0');
	return result;
}

BYTE Table::getbyte(const TCHAR* FieldName) {
	int result = -1;
	try	{
		_variant_t  vtValue = m_Rec->Fields->GetItem(FieldName)->GetValue();
		if(vtValue.vt == VT_NULL) return result;
		result = vtValue.bVal;
	}

	CATCHERRGET

	_str_error[0] = _T('\0');
	return result;
}

BYTE Table::getbyte(const int index) {
	_variant_t FieldName;
	FieldName.vt = VT_I2;
	FieldName.iVal = index;

	int result = -1;
	try	{
		_variant_t  vtValue = m_Rec->Fields->GetItem(FieldName)->GetValue();
		if(vtValue.vt == VT_NULL) return result;
		result = vtValue.bVal;
	}

	CATCHERRGET

	_str_error[0] = _T('\0');
	return result;
}
long Table::getlong(const TCHAR* FieldName) {
	long result = -1;
	try	{
		_variant_t  vtValue = m_Rec->Fields->GetItem(FieldName)->GetValue();
		if(vtValue.vt == VT_NULL) return result;
		result = vtValue.lVal;
	}

	CATCHERRGET

	_str_error[0] = _T('\0');
	return result;
}
long Table::getlong(const int index) {
	_variant_t FieldName;
	FieldName.vt = VT_I2;
	FieldName.iVal = index;
	long result = -1;
	try	{
		_variant_t  vtValue = m_Rec->Fields->GetItem(FieldName)->GetValue();
		if(vtValue.vt == VT_NULL) return result;
		result = vtValue.lVal;
	}

	CATCHERRGET

	_str_error[0] = _T('\0');
	return result;
}
float Table::getfloat(const TCHAR* FieldName) {
	float FieldValue = -1;
	try	{
		_variant_t  vtValue = m_Rec->Fields->GetItem(FieldName)->GetValue();
		if(vtValue.vt == VT_NULL) return FieldValue;
		FieldValue=vtValue.fltVal;
	}

	CATCHERRGET

	_str_error[0] = _T('\0');
	return FieldValue;
}
float Table::getfloat(const int index) {
	_variant_t FieldName;
	FieldName.vt = VT_I2;
	FieldName.iVal = index;
	float FieldValue = -1;
	try	{
		_variant_t  vtValue = m_Rec->Fields->GetItem(FieldName)->GetValue();
		if(vtValue.vt == VT_NULL) return FieldValue;
		FieldValue=vtValue.fltVal;
	}

	CATCHERRGET

	_str_error[0] = _T('\0');
	return FieldValue;
}
double Table::getdouble(const TCHAR* FieldName) {
	double FieldValue = -1;
	try	{
		_variant_t  vtValue = m_Rec->Fields->GetItem(FieldName)->GetValue();
		if(vtValue.vt == VT_NULL) return FieldValue;
		FieldValue = vtValue.dblVal;
	}

	CATCHERRGET

	_str_error[0] = _T('\0');
	return FieldValue;
}

double Table::getdouble(const int index) {
	_variant_t FieldName;
	FieldName.vt = VT_I2;
	FieldName.iVal = (SHORT)index;
	double FieldValue = -1;
	try	{
		_variant_t  vtValue = m_Rec->Fields->GetItem(FieldName)->GetValue();
		if(vtValue.vt == VT_NULL) return FieldValue;
		FieldValue = vtValue.dblVal;
	}

	CATCHERRGET

	_str_error[0] = _T('\0');
	return FieldValue;
}

void Table::getdate(const TCHAR* FieldName, COleDateTime& result) {
	try	{
		_variant_t  vtValue = m_Rec->Fields->GetItem(FieldName)->GetValue();
		if(vtValue.vt == VT_NULL) 	{	
			result.SetDateTime(1900, 1, 1, 1, 1, 1); 
			return;	
		}
		result = vtValue.date;
		if(result.m_dt == 0)		{	
			result.SetDateTime(1900, 1, 1, 1, 1, 1); 
			return;	
		}
		if(result.GetStatus() == COleDateTime::invalid) {
			result.SetDateTime(1900, 1, 1, 1, 1, 1); 
			return;
		}
		if(result.GetYear() < 100)	{	
			result.SetDateTime(1900, 1, 1, 1, 1, 1); 
			return;	
		}
	}
	catch(_com_error &e) {
		ErrorHandler(e,_str_error);
		mikefmt(_str_error, _T("%s\n**For Field Name:%s"),_str_error,FieldName);
		result = (long)0;
	}
	_str_error[0] = _T('\0');
}
COleDateTime Table::getdate(const TCHAR* FieldName) {
	COleDateTime result;
	getdate(FieldName, result);
	return result;
}
CString Table::getdatedt(const TCHAR* FieldName) {
	return getdate(FieldName).Format(_T("%Y/%m/%d %H:%M:%S"));
}
CString Table::getdated(const TCHAR* FieldName) {
	return getdate(FieldName).Format(_T("%Y/%m/%d"));
}
CString Table::getdatet(const TCHAR* FieldName) {
	return getdate(FieldName).Format(_T("%H:%M:%S"));
}
void Table::getdatedt(const TCHAR* FieldName, CString& result) {
	COleDateTime dt;
	getdate(FieldName, dt);
	result = dt.Format(_T("%Y/%m/%d %H:%M:%S"));
}

void Table::getdate(const int index, COleDateTime& result) {
	_variant_t FieldName;
	FieldName.vt = VT_I2;
	FieldName.iVal = (SHORT)index;
	try {
		_variant_t  vtValue = m_Rec->Fields->GetItem(FieldName)->GetValue();
		if(vtValue.vt == VT_NULL)	{	
			result.SetDateTime(1900, 1, 1, 1, 1, 1);	
			return;		
		}
		result = vtValue.date;
		if(result.m_dt == 0) {	
			result.SetDateTime(1900, 1, 1, 1, 1, 1);	
			return;		
		}
		if(result.GetStatus() == COleDateTime::invalid) {
			result.SetDateTime(1900, 1, 1, 1, 1, 1); 
			return;
		}
		if(result.GetYear() < 100) {	
			result.SetDateTime(1900, 1, 1, 1, 1, 1);	
			return;		
		}
	}
	catch(_com_error &e) {
		ErrorHandler(e,_str_error);
		mikefmt(_str_error, _T("%s\n**For Field Name:%s"),_str_error,FieldName);
		result = (long)0;
	}
	_str_error[0] = _T('\0');
}
COleDateTime Table::getdate(const int index) {
	COleDateTime result;
	getdate(index, result);
	return result;
}
CString Table::getdatedt(const int index) {
	return getdate(index).Format(_T("%Y/%m/%d %H:%M:%S"));
}
CString Table::getdated(const int index) {
	return getdate(index).Format(_T("%Y/%m/%d"));
}
CString Table::getdatet(const int index) {
	return getdate(index).Format(_T("%H:%M:%S"));
}
void Table::getdatedt(const int index, CString& result)
{
	COleDateTime dt;
	getdate(index, dt);
	result = dt.Format(_T("%Y/%m/%d %H:%M:%S"));
}

bool Table::is_value_null(const TCHAR* fieldname) {
	try	{
		_str_error[0] = _T('\0');
		_variant_t  vtValue = m_Rec->Fields->GetItem(fieldname)->GetValue();
		if(vtValue.vt == VT_NULL) { _tcscpy_s(_str_error, _T("null value")); return true; }
		return false;
	}
	catch(_com_error &e) {
		ErrorHandler(e,_str_error);
		mikefmt(_str_error,_T("%s\n**For Field Name:%s"),_str_error,fieldname);
		return true;
	}
}

HRESULT Table::MoveNext() {
	HRESULT hr;
	try	{
		hr=m_Rec->MoveNext();
	}
	catch(_com_error &e) {
		ErrorHandler(e,_str_error);
		//m_Rec=NULL;
		return -2;
	}
	_str_error[0] = _T('\0');
	return hr;
}

HRESULT Table::MovePrevious() {
	HRESULT hr;
	try	{
		hr=m_Rec->MovePrevious();
	}
	catch(_com_error &e) {
		ErrorHandler(e,_str_error);
		//m_Rec=NULL;
		return -2;
	}
	_str_error[0] = _T('\0');
	return hr;
}

HRESULT Table::MoveFirst() {
	HRESULT hr;
	try	{
		hr=m_Rec->MoveFirst();
	}
	catch(_com_error &e) {
		ErrorHandler(e,_str_error);
		//m_Rec=NULL;
		return -2;
	}
	_str_error[0] = _T('\0');
	return hr;
}

HRESULT Table::MoveLast() {
	HRESULT hr;
	try	{
		hr=m_Rec->MoveLast();
	}
	catch(_com_error &e) {
		ErrorHandler(e,_str_error);
		//m_Rec=NULL;
		return -2;
	}
	_str_error[0] = _T('\0');
	return hr;
}

int dbhelper::maxconnections = 0;

void dbhelper::initialize_dbhelper(const UINT maxprocs, const int maxconns) {
	if(dbgbl == 0) {
		dbhelper::maxconnections = maxconns;
		dbgbl = new sDBGblManager[maxprocs];
		for(int i = 0; i < maxprocs; i++) {
			dbgbl[i].pcon = new cConnection[dbhelper::maxconnections];
			dbgbl[i].pqry = new Table[dbhelper::maxconnections];
			dbgbl[i].pos = 0;
			dbgbl[i].top = 0;
		}
	}
}

void dbhelper::initialize_stack(const int place) {
	dbgbl[place].pos = 0;
	dbgbl[place].top = 0;
}

Table& dbhelper::get_qry_from_gbl_manager() {
	// we dont have to open the querys or check other state, because the connection 
	// does that but the connection has to have a destiny process resultset, so we 
	// only create them a give a reference to it
	int pos = dbgbl[procplace].pos - 1; // the -1 is because who incrementes was the connection 
	require(pos < 0 || pos >= dbhelper::maxconnections, _T("out_of_db_queries"));
	Table& query = dbgbl[procplace].pqry[pos];
	return query;
}

cConnection& dbhelper::get_db_from_gbl_manager() {
	int& pos = dbgbl[procplace].pos;
	cConnection& con = dbgbl[procplace].pcon[pos];
	++pos;
	if(pos > dbgbl[procplace].top) dbgbl[procplace].top = pos; 
	require(pos < 0 || pos >= dbhelper::maxconnections, _T("out_of_db_connections"));

	ULONGLONG now = GetTickCount64();
	double diff = ((double)(now - con.lastaccess)) / CLOCKS_PER_SEC;
	con.lastaccess = now;

	if(diff > (60 * 2))	{
		try {
			for(int i = pos; i < dbhelper::maxconnections; ++i) {
				cConnection& toclose = dbgbl[procplace].pcon[i];
				if(toclose.m_Cnn) {
					toclose.Close();
					toclose.m_Cnn = 0;
				}
			}
		}
		catch(_com_error &e)	{ }
		catch(CException *e)	{ e->Delete(); }
		catch(mroerr& e)			{ }
		catch(...)						{ }
	}

	if(con.m_Cnn == 0) {
		require(!con.Open(_T(""),_T(""), connection), con.get_err());
		con.settimeout(timeout);
	}

	return con;
}
void dbhelper::ret_db_to_gbl_manager() {
	int& pos = dbgbl[procplace].pos;
	pos--;
	require(pos < 0 || pos >= dbhelper::maxconnections, _T("return_of_connections"));
}
sDBGblManager* dbhelper::dbgbl = 0;
long dbhelper::timeout = 0;
TCHAR dbhelper::connection[1024];

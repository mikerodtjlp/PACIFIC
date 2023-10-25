#include "stdafx.h"

//#include "mroctrl.h"
#include "SessionMan.h"

void CSessionMan::check_chat()
{
/*	CString me;
	_basics.get(ZUSERID, me, ZUSERIDLEN);

	try
	{
		if(!::TryEnterCriticalSection(&cschat)) return;
		iterchat = chat.lower_bound(me);
		if(iterchat != endchat && !(chat.key_comp()(me, iterchat->first)))
		{
			map<CString, list<CString> >& chat2 = (*iterchat).second;
			if(chat2.begin()->second.empty() == false) // found something
			{
				_params.set(_T("zchthasz"), _T("1"), 8, 1);
				_params.set(_T("zchtfrmz"), chat2.begin()->first, 8);
			}
		}
		::LeaveCriticalSection(&cschat);
	}
	catch(CException *e)	{ 	::LeaveCriticalSection(&cschat); throw; }
	catch(mroerr&)			{	::LeaveCriticalSection(&cschat); throw;	}
	catch(...)				{	::LeaveCriticalSection(&cschat); throw;	}*/
}

void CSessionMan::read_chat()
{
/*	CString me;	   _basics.get(ZUSERID, me, ZUSERIDLEN);
	CString other; _params.get(_T("cusrini"), other, 7);

	other.TrimRight();
	other.MakeLower();

	require(me.IsEmpty() || other.IsEmpty(), _T("inc_dat_user"));

	try
	{
		if(!::TryEnterCriticalSection(&cschat)) return;

		iterchat = chat.lower_bound(me);
		if(iterchat != endchat && !(chat.key_comp()(me, iterchat->first)))
		{
			map<CString, list<CString> >& chat2 = (*iterchat).second; // tomamos los mensajes para me

			int ini = 0;
			for(;;)
			{
				// desglozamos la lista para sacar cada persona
				CString persona;
				int fin = other.Find(';', ini);
				if(fin == -1) break; // ya no hay mas en la lista

				persona = other.Mid(ini, fin - ini);
				ini = fin + 1;

				persona.TrimLeft();
				persona.TrimRight();
				persona.Remove(';');
				if(persona.GetLength() ==  0) break;
				if(persona             == me) continue; // no se puede leer a uno mismo;

				// buscamos en los mensajes para me los de la otra persona de la lista
				map<CString, list<CString> >::iterator iter2 = chat2.lower_bound(persona);
				if(iter2 != chat2.end() && !(chat2.key_comp()(persona, iter2->first)))
				{
					if((*iter2).second.empty() == false)
					{
						list<CString>::iterator liter = (*iter2).second.begin();

						me.Format(_T("%s %s: %s"), persona, _T("say"), *liter);//get_desc(_T("say")), *liter);
						_params.set(_T("uchatbox0"), me, 9);
						_params.set(_T("uchatbox0img"), persona, 12);
						(*iter2).second.erase(liter);
						break;
					}
				}
			}
		}
		::LeaveCriticalSection(&cschat);
	}
	catch(CException *e)	{ 	::LeaveCriticalSection(&cschat); throw; }
	catch(mroerr&)			{	::LeaveCriticalSection(&cschat); throw;	}
	catch(...)				{	::LeaveCriticalSection(&cschat); throw;	}*/
}

void CSessionMan::write_chat()
{
/*	CString message;
	_params.get(_T("ctxtini"), message, 7);
	if(message.IsEmpty()) return;

	CString me;
	_basics.get(ZUSERID, me, ZUSERIDLEN);
	if(me.IsEmpty()) _params.get(ZUSERID, me, ZUSERIDLEN);
	me.TrimRight();
	me.MakeLower();

	CString other;
	_params.get(_T("cusrini"), other, 7);
	other.TrimRight();
	other.MakeLower();

	require(me.IsEmpty() || other.IsEmpty(), _T("inc_dat_user"));

	try
	{
		::EnterCriticalSection(&cschat);

		int ini = 0;
		for(;;)
		{
			// desglozamos la lista para sacar cada persona
			CString persona;
			int fin = other.Find(_T(';'), ini);
			if(fin == -1) break; // ya no hay mas en la lista

			persona = other.Mid(ini, fin - ini);
			ini = fin + 1;

			persona.TrimLeft();
			persona.TrimRight();
			persona.Remove(_T(';'));
			if(persona.GetLength() ==  0) break;
			if(persona             == me) continue; // no se puede mandar a uno mismo;

			chat[persona][me].push_back(message);
		}
		endchat = chat.end();

		::LeaveCriticalSection(&cschat);
	}
	catch(CException *e)	{ 	::LeaveCriticalSection(&cschat); throw; }
	catch(mroerr&)			{	::LeaveCriticalSection(&cschat); throw;	}
	catch(...)				{	::LeaveCriticalSection(&cschat); throw;	}

	_params.set(_T("utext"), _T(""), 5);

	CString msg;
	msg.Format(_T("%s %s: %s"), me, _T("say"), message);
	_params.set(_T("uchatbox0"), msg, 9);
	_params.set(_T("uchatbox0img"), me, 12);*/
}

void CSessionMan::borrar_chat()
{
}

void CSessionMan::_generic_excel_2_sql()
{
	CParameters exedata;  _params.get(ZEXEDAT, exedata, ZEXEDATLEN); 

	int total			= exedata.getint(ZEXETOT,ZEXETOTLEN);
	int cols			= exedata.getint(ZEXECLS,ZEXECLSLEN);
	int columns			= exedata.getint(_T("colsneeded"),10);
	bool checkisempty	= exedata.getint(_T("checkisempty"),12);

	TCHAR sqlcmd[1024];
	int lensqlcmd = _params.get(_T("sqlcmd"), sqlcmd); 

	if(cols != columns)
	{
		CString error;
		error.Format(_T("%d %s %d"), cols, _T("mustbe"), columns);
		require(true, error);
	}
	require(checkisempty && total <= 0, _T("file_empty"));

	CString command;
	TCHAR helper[2048];
	TCHAR col[16];
	TCHAR cell[16];

	getconnectionx(con, obj);
	for(int i = 1; i <= total; ++i)
	{
		command.SetString(sqlcmd, lensqlcmd);
		for(int j = 0; j < cols; ++j)
		{
			mikefmt(col, _T("col%d"), j);
			mikefmt(cell, _T("%c%d"), (TCHAR)(65 + j), i);
			exedata.get(cell, helper);
			command.Replace(col, helper);
		}
		con.execute(command.GetBuffer());
	}
}

void CSessionMan::_generic_txt_2_sql()
{
	CParameters txtdata;  _params.get(ZTXTDAT, txtdata, ZTXTDATLEN); 

	int total			= txtdata.getint(ZTXTTOT, ZTXTTOTLEN);
	int cols			= 0;
	int columns			= txtdata.getint(_T("colsneeded"));
	bool checkisempty	= txtdata.getint(_T("checkisempty"));

	TCHAR sqlcmd[1024];
	int lensqlcmd = _params.get(_T("sqlcmd"), sqlcmd); 

	require(checkisempty && total <= 0, _T("empty_file"));

	CString linetxt;
	TCHAR lineid[16];
	mikefmt(lineid, _T("txtln%d"), 0);
	linetxt = _params.get(lineid);
	TCHAR* p = linetxt.GetBuffer();
	for(;*p++;) if(*p == ',') ++cols;
	if(cols > 0) ++cols;

	require(cols != columns, _T("wrong_columns_number"));

	CString command;
	CString helper;
	TCHAR col[16];
	TCHAR cell[16];
	int fields = 0;

	getconnectionx(con, obj);
	for(int i = 0; i < total; ++i)
	{
		command.SetString(sqlcmd, lensqlcmd);

		mikefmt(lineid, _T("txtln%d"), i);
		linetxt = txtdata.get(lineid);
		TCHAR* p = linetxt.GetBuffer();
		TCHAR* q = p;
		fields = 1;
		int j = 0;
		for(;*p;++p) 
		{
			if(*p == _T(',')) 
			{
				int len = p - q;
				helper.SetString(q, len);
				helper.Trim();
				q = p+1;
				mikefmt(col, _T("col%d"), j++);
				command.Replace(col, helper);
				++fields;
			}
		}
		int len = p - q;
		helper.SetString(q, len);
		helper.Trim();
		q = p+1;
		mikefmt(col, _T("col%d"), j++);
		command.Replace(col, helper);

		if(fields != cols)
		{
			helper.Format(	_T("%s %d"), _T("error_in_line"), i);
			require(true, helper);
		}
		con.execute(command.GetBuffer());
	}
}

void CSessionMan::sql2txt()
{
	cCommand command(_basics);

	CString filename	= _params.get(_T("cfileini"),8);
	command				= _params.get(_T("command"),7);

	command.Replace(_T(" {{ "), _T(" < ")); // temporary patch
	command.Replace(_T(" }} "), _T(" > ")); // temporary patch

	require(filename.IsEmpty(), _T("inc_dat_file"));

	defchar(curpath, 128);
	_basics.get(CURPATH, curpath, CURPATHLEN, 127);

	// generate a temporary file 
	COleDateTime now = COleDateTime::GetCurrentTime();
	CString file;
	CString target_file;
	file.Format(		_T("%d%d%d%d%.0f.txt"), 
						now.GetDayOfYear(), now.GetHour(), now.GetMinute(), 
						now.GetSecond(), ((double)clock()));
	target_file.Format(	_T("%s\\temp\\%s"), curpath, file);
	if(mro::exist_file(target_file)) _tremove(target_file);
	CUTF16File obj_file;
	obj_file.Open(target_file, CFile::modeWrite | CFile::modeCreate);

	{
	getconnectionx(con, obj);
	con.execute(command, obj);
	std::vector<ADODB::DataTypeEnum> mcols_types;
	UINT cols = obj.get_column_count();
	mcols_types.reserve(cols);
	for(UINT i = 0; i < cols; ++i)
	{
		ADODB::DataTypeEnum type = obj.get_column_type(i);
		mcols_types.push_back(type);
	}

	CString help;
	TCHAR row[1024];
	for(;!obj.IsEOF(); obj.MoveNext())
	{
		TCHAR* p = row;
		int lenght = 0;
		for(UINT i = 0; i < cols; ++i)
		{
			switch(obj.get_column_type(i))
			{
				case ADODB::adSmallInt:		lenght = mikefmt(p, _T("%d"), obj.getbyte(i)); break;
				case ADODB::adInteger:		lenght = mikefmt(p, _T("%d"), obj.getint(i)); break;
				case ADODB::adDate:
				case ADODB::adDBDate:
				case ADODB::adDBTime:
				case ADODB::adDBTimeStamp:	lenght = mikefmt(p,_T("%s"), obj.getdate(i).Format(_T("%Y/%m/%d %H:%M:%S"))); break;

				case ADODB::adLongVarWChar:
				case ADODB::adLongVarChar:	help = obj.get(i); 
											if(help.GetLength() > 1000) help = _T("value to long");
											lenght = mikefmt(p, _T("%s"), help); break;

				case ADODB::adWChar:
				case ADODB::adVarWChar:
				case ADODB::adBSTR:
				case ADODB::adChar:

				case ADODB::adVarChar:		lenght = obj.get(i, p);
											break;

				case ADODB::adDouble:		lenght = mikefmt(p, _T("%.2f"), obj.getdouble(i)); break;
				case ADODB::adBinary:		lenght = mikefmt(p, _T("%d"), obj.getbyte(i)); break;
				default:
					lenght = mikefmt(p, _T("%d"), obj.getbyte(i)); break;
			}

			p += lenght;
			*p = _T('\t');
			++p;
		}
		p--; // le quitamos el tab de mas

		set2ch(p, _T('\n'), 0);
		p += 2;
		obj_file.WriteString(row);
	}
	}

	obj_file.Close();

	static CString server = synservice::GetIpAddress() + _T(":") + _basics.get(_T("webport"));

	auto download		= filedownload();
    download.server		= server;
    download.folder		= _T("temp");
    download.file		= file;
    download.topath		= filename;
	download.to_result(_params);

	auto shl	= shell();
	shl.shellpath = filename;
	shl.to_result(_params);
}

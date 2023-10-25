#pragma once

void get_error_description(sClients& clie, sProcess& proc, CParameters& result, 
				const bool fromcomp);
void write2event(DWORD type, const TCHAR* msg, const int step = 0);

void manage_exception(	CParameters& result, const TCHAR* text, const TCHAR* extra, const TCHAR* info, 
						const TCHAR* funname, const TCHAR* errfile, const int errline,
	const int step);

void process_error(sClients& clie,
				   sProcess& proc,
				   CParameters& result,
				   const bool saveerror,
				   const int errid);

int get_error(sClients& clie, sProcess& proc, 
				const TCHAR* errcode, const int errlen, TCHAR* errdesc, const int errdesclen, 
				const bool fromcomp);

#pragma once

void initialize_com(/*CParameters& prms*/);

bool core_call(		CParameters& params,
					const TCHAR* command, const int cmdlen, 
					CParameters& result, sProcess& proc);
bool core_call(		const TCHAR* params, const int paramslen, 
					const TCHAR* command, const int cmdlen, 
					CParameters& result, sProcess& proc);

bool execute_com(	CParameters& params, 
					const TCHAR* command, const int cmdlen,
					CParameters& result, 
					const int index, 
					CString* valuestochange);

void terminate_com(	const int procid);
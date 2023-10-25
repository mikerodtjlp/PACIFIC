#pragma once

int find_pending_proc();

void system_calls(const int index, const TCHAR* parameters, CParameters& result, const TCHAR* pbasics, CString* valuestochange);
bool execute_sys(		const TCHAR* component, const int complen, 
						CParameters& params,
						const TCHAR* command, const int cmdlen,
						CParameters& result, 
						const int index, 
						CString* valuestochange);

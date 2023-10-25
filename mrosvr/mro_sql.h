#pragma once

//extern bool dbavailable;
//void check_db();

void initialize_sql(/*CParameters& prms*/);
void initialize_sql_connections();

bool execute_sql(		const TCHAR* component, const int complen, 
						CParameters& params, 
						const TCHAR* command, const int cmdlen,
						CParameters& result, 
						const int index, 
						CString* valuestochange);

void terminate_sql(	const int procid);

//void save_result(sClients& clie, sProcess& proc);

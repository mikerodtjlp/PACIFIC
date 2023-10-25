#pragma once

bool is_127001(const TCHAR* ip, const int iplen);
void finish_process(sClients& clie, sProcess& proc, const mrostatus status);
void finish_request(sClients& clie);
void post_process(sClients& clie, sProcess& proc);
CParameters& get_response(sClients& clie, const bool er);
unsigned __stdcall apply_functions(LPVOID pParam);

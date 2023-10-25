#pragma once

using namespace std;

void initialize_funs();
bool reload_codebehind(const TCHAR* cbfile, const int cbfilelen, sProcess& proc);
bool load_function(sClients& clie, sProcess& proc);

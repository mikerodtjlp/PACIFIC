#pragma once

using namespace std;

// structura de la cola del log
#define MAX_FUNS_PER_CODEBEHIND 48
struct scmdimp
{
	eventcmd mcmd[MAX_FUNS_PER_CODEBEHIND];
	int cmdmax;
}; 

void initialize_funs_dll();

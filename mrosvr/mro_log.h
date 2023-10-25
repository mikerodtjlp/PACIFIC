#pragma once

#define MAXLOGQUEUE 8
#define MAXLOGPARMS 512

// structura de la cola del log
struct slogimp
{
	mrostatus status;
	TCHAR params[MAXLOGPARMS];
	int prmslen;
	bool saveit;
}; 

struct slog
{
	slogimp thequeue[MAXLOGQUEUE];
	int pos;
	TCHAR final_command[(MAXLOGPARMS * MAXLOGQUEUE) + 1];
};


extern slog* actionlog;
extern long loghits;

//extern slog* errorlog;
//extern long errorhits;

void initialize_log(slog*& plog);

void flush_logs(slog* plog, const int procid);

void save_log(sClients& clie, sProcess& proc, slog* plog);

int get_list_log(slog*plog, TCHAR* lista, const int lid, const int buffsize);


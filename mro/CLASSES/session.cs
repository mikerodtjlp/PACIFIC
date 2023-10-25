#if NETCOREAPP
using Microsoft.AspNetCore.Http;
#endif

using System;
using System.Text;

using pairs = mro.CParameters;

namespace mro {

  public struct sess {
    public int zsesins;
    public int zsesmac;
    public int zsescli;
    public int zsesses;
  }

  //extern bool tagsloaded;

  //const int MAXHISTORY	= 24;
  //const int MAXSESSIONS	= 6;
  //const int MAXUSERS	= 12;

  public class historia {
    void init() { trasname = string.Empty; }
    public string trasname;
  };

  public class session {
    //public static const int maxlastaction = 32;

    public void init() { online = false; reseted = false; hispos = -1; histop = -1; sestime = 30; }

    public string strsession;

    public DateTime start;
    public DateTime lastcontact;
    public historia[] history = new historia[24];//[MAXHISTORY];
    public int histop;
    public int hispos;

    public int access;
    public bool online;
    public bool reseted;
    public int sestime;
  };
  public class usuario {
    public void init() { sestop = 0; user = string.Empty; tmplparms.clear(); }
    public string user;
    public mroJSON tmplparms = new mroJSON();

    public int access;
    public int sestop;
    public session[] sessions = new session[6]; //[MAXSESSIONS];
  };
  public class maquina {
    public void init() { usrtop = 0; ipaddress = string.Empty; }
    public string ipaddress;

    public int usrtop;
    public usuario[] users = new usuario[12];//[MAXUSERS];
  };

  //extern maquina* machines;
  //extern int MAXMACHINES;
  //extern int mactop;
}

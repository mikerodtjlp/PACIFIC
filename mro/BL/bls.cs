using System;
using mro;

namespace mro.BL {
  public static class bls {
    private static control_BL ctrbl = null;
    private static readonly object ctrlock = new object();
    public static control_BL get_ctrbl() {
      if (ctrbl == null) {
        lock (ctrlock) {
          if (ctrbl == null)
            ctrbl = new control_BL();
        }
      }
      return ctrbl;
    }
  }
}

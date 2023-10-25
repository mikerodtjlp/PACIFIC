using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using mro;

namespace sfc.BL {
   public static class bls {
      private static casting_BL cstbl = null;
      private static readonly object cstlock = new object();
      public static casting_BL get_cstbl() {
         if (cstbl == null) {
            lock (cstlock) {
               if (cstbl == null)
                  cstbl = new casting_BL(null);
            }
         }
         return cstbl;
      }

      private static manufacture_BL manbl = null;
      private static readonly object manlock = new object();
      public static manufacture_BL get_manbl() {
         if (manbl == null) {
            lock (manlock) {
               if (manbl == null)
                  manbl = new manufacture_BL(null);
            }
         }
         return manbl;
      }

      private static packaging_BL pkgbl = null;
      private static readonly object pkglock = new object();
      public static packaging_BL get_pkgbl() {
         if (pkgbl == null) {
            lock (pkglock) {
               if (pkgbl == null)
                  pkgbl = new packaging_BL(null);
            }
         }
         return pkgbl;
      }

      private static planning_BL plnbl = null;
      private static readonly object plnlock = new object();
      public static planning_BL get_plnbl() {
         if (plnbl == null) {
            lock (plnlock) {
               if (plnbl == null)
                  plnbl = new planning_BL(null);
            }
         }
         return plnbl;
      }

      private static qc_BL qcbl = null;
      private static readonly object qclock = new object();
      public static qc_BL get_qcbl() {
         if (qcbl == null) {
            lock (qclock) {
               if (qcbl == null)
                  qcbl = new qc_BL(null);
            }
         }
         return qcbl;
      }
   }
}

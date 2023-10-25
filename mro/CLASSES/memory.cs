#if NETCOREAPP
using Microsoft.AspNetCore.Http;
#endif
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Web;

namespace mro {
  public class mem {
    public static string join2(char s0, char s1) {
      var s = new StringBuilder();
      s.Append(s0);
      s.Append(s1);
      return s.ToString();
    }
    public static string join2(char s0, string s1) {
      var s = new StringBuilder();
      s.Append(s0);
      s.Append(s1);
      return s.ToString();
    }
    public static string join2(string s0, string s1) {
      var s = new StringBuilder(s0);
      s.Append(s1);
      return s.ToString();
    }
    public static string join2(string s0, int s1) {
      var s = new StringBuilder();
      s.Append(s0);
      s.Append(s1.tostr());
      return s.ToString();
    }
    public static string json(string s0) {
      var s = new StringBuilder();
      s.Append('{');
      s.Append(s0);
      s.Append('}');
      return s.ToString();
    }
    public static string json(StringBuilder s0) {
      var s = new StringBuilder();
      s.Append('{');
      s.Append(s0);
      s.Append('}');
      return s.ToString();
    }
    public static string join3(char s0, string s1, char s2) {
      var s = new StringBuilder();
      s.Append(s0);
      s.Append(s1);
      s.Append(s2);
      return s.ToString();
    }
    public static string join3(string s0, char s1, string s2) {
      var s = new StringBuilder();
      s.Append(s0);
      s.Append(s1);
      s.Append(s2);
      return s.ToString();
    }
    public static string join3(string s0, string s1, string s2) {
      var s = new StringBuilder(s0);
      s.Append(s1);
      s.Append(s2);
      return s.ToString();
    }
    public static string join3(string s0, StringBuilder s1, string s2) {
      var s = new StringBuilder(s0);
      s.Append(s1);
      s.Append(s2);
      return s.ToString();
    }
    public static string join4(string s0, string s1, string s2, string s3) {
      var s = new StringBuilder(s0);
      s.Append(s1);
      s.Append(s2);
      s.Append(s3);
      return s.ToString();
    }
    public static string join5(string s0, string s1, string s2, string s3, string s4) {
      var s = new StringBuilder(s0);
      s.Append(s1);
      s.Append(s2);
      s.Append(s3);
      s.Append(s4);
      return s.ToString();
    }
    public static string join5(string s0, int s1, string s2, string s3, string s4) {
      var s = new StringBuilder(s0);
      s.Append(s1);
      s.Append(s2);
      s.Append(s3);
      s.Append(s4);
      return s.ToString();
    }
    public static string join5(string s0, int s1, int s2, int s3, int s4) {
      var s = new StringBuilder(s0);
      s.Append(s1);
      s.Append(s2);
      s.Append(s3);
      s.Append(s4);
      return s.ToString();
    }
    public static string join6(string s0, int s1, int s2, int s3, int s4, int s5) {
      var s = new StringBuilder(s0);
      s.Append(s1);
      s.Append(s2);
      s.Append(s3);
      s.Append(s4);
      s.Append(s5);
      return s.ToString();
    }
    public static string join7(string s0, string s1, string s2,
       string s3, string s4, string s5, string s6) {
      var s = new StringBuilder(s0);
      s.Append(s1);
      s.Append(s2);
      s.Append(s3);
      s.Append(s4);
      s.Append(s5);
      s.Append(s6);
      return s.ToString();
    }
    public static string join9(string s0, string s1, string s2,
       string s3, string s4, string s5, string s6, string s7, string s8) {
      var s = new StringBuilder(s0);
      s.Append(s1);
      s.Append(s2);
      s.Append(s3);
      s.Append(s4);
      s.Append(s5);
      s.Append(s6);
      s.Append(s7);
      s.Append(s8);
      return s.ToString();
    }
    public static string join9(StringBuilder s0, string s1, string s2,
       string s3, string s4, string s5, string s6, string s7, string s8) {
      var s = new StringBuilder();
      s.Append(s0);
      s.Append(s1);
      s.Append(s2);
      s.Append(s3);
      s.Append(s4);
      s.Append(s5);
      s.Append(s6);
      s.Append(s7);
      s.Append(s8);
      return s.ToString();
    }
    public static bool _tmemcmp(StringBuilder s1, int sl, string s2) {
      if (sl != s2.Length) return false;
      for (int i = 0; i < sl; ++i) if (s1[i] != s2[i]) return false;
      return true;
    }
    public static bool _tmemcmp(char[] s1, int sl, string s2) {
      if (sl != s2.Length) return false;
      for (int i = 0; i < sl; ++i) if (s1[i] != s2[i]) return false;
      return true;
    }
    public static bool _tmemcmp(char[] s1, int sl, ref char[] s2, int sl2) {
      if (sl != sl2) return false;
      for (int i = 0; i < sl; ++i) if (s1[i] != s2[i]) return false;
      return true;
    }
    public static bool _tmemcmp(StringBuilder s1, string s2) {
      int sl = s1.Length;
      if (sl != s2.Length) return false;
      for (int i = 0; i < sl; ++i) if (s1[i] != s2[i]) return false;
      return true;
    }
    public static bool _tmemcmp(string s1, StringBuilder s2) {
      int sl = s1.Length;
      if (sl != s2.Length) return false;
      for (int i = 0; i < sl; ++i) if (s1[i] != s2[i]) return false;
      return true;
    }
    public static bool _tmemcmp(string s1, string s2) {
      int sl = s1.Length;
      if (sl != s2.Length) return false;
      for (int i = 0; i < sl; ++i) if (s1[i] != s2[i]) return false;
      return true;
    }
    public static bool _tmemcmp(StringBuilder s1, StringBuilder s2) {
      int sl = s1.Length;
      if (sl != s2.Length) return false;
      for (int i = 0; i < sl; ++i) if (s1[i] != s2[i]) return false;
      return true;
    }
    // The unsafe keyword allows pointers to be used within
    // the following method:
    public static unsafe void _tmemcpy(char[] dst,
                               int doff,
                               char[] src,
                               int soff,
                               int count) {
      if (src == null || soff < 0 || dst == null || doff < 0 || count < 0) return;
      int srcLen = src.Length;
      int dstLen = dst.Length;
      if (srcLen - soff < count || dstLen - doff < count) return;

      int bsize = sizeof(int);
      // The following fixed statement pins the location of the src and dst 
      // objects in memory so that they will not be moved by garbage collection.          
      fixed (char* pSrc = src, pDst = dst) {
        char* ps = pSrc + soff;
        char* pd = pDst + doff;

        // Loop over the count in blocks of 4 bytes, copying an
        // integer (4 bytes) at a time:
        for (int n = 0; n < count / bsize; n++) {
          *(pd + 0) = *(ps + 0); *(pd + 1) = *(ps + 1);
          *(pd + 2) = *(ps + 2); *(pd + 3) = *(ps + 3);//*((int*)pd) = *((int*)ps);
          pd += bsize;
          ps += bsize;
        }

        // Complete the copy by moving any bytes that weren't
        // moved in blocks of 4:
        for (int n = 0; n < count % bsize; n++) {
          *pd = *ps;
          pd++;
          ps++;
        }
      }
    }
  }
}

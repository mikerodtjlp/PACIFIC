using System;
using System.Collections.Generic;

namespace mro.BO {
   public class query_result {
      public query_result(List<List<string>> rows) { data = rows; }
      public query_result(List<List<string>> rows, List<table_col> columns) { data = rows; cols = columns; }
      public query_result(Exception e) { error = e; }

      public List<List<string>> data = new List<List<string>>();
      public List<table_col> cols = new List<table_col>();
      public Exception error { get; set; }
   }
}

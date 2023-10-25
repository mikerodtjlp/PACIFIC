using System;

namespace sfc.BO
{
    public class oven
    {
        public oven(string number) { id = number; }
        public string id            { get;  set; } 
        public string description   { get;  set; }
        public string type          { get; set; }
        public product prod         { get;  set; }
        public basenum baseno       { get; set; }
        public int qty              { get; set; }
    }
}

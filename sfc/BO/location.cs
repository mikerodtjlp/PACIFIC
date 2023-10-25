using System;

namespace sfc.BO
{
    public class location
    {
        public location() { }
        public location(string code) { id = code; }
        public string id {get; set;}
        public string description {get; set; }
    }
}

using System;
using mro;

namespace sfc.BO
{
    public class box_status
    {
        public box_status(string status) { this.status = status; }
        public string status = string.Empty;
        public const string AUTHORIZED = "A";
        public const string PENDING = "P";
        public const string UPLOADED = "U";
    }
}

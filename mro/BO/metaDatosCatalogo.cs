using System;

namespace mro.BO
{
    [Serializable]
    public class metaDatosCatalogo
    {
        public string table_name { get; set; }
        public string column_name { get; set; }
        public string is_nullable { get; set; }
        public string data_type { get; set; }
        public string max_length { get; set; }
        public string constraint_Type { get; set; }
        public string table_name2 { get; set; }
        public string column_name2 { get; set; }
    }
}

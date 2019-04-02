using System;

namespace DataMapperPerformanceExec
{
    public class Usuario
    {
        public int id { get; set; }
        public String password_hash { get; set; }
        public String password_salt { get; set; }
        public String username { get; set; }
    }
}
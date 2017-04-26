using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataMapperUnitTest
{
    public class Usuario
    {
        public String id { get; set; }
        public String username { get; set; }
        public String password_hash { get; set; }
        public String password_salt { get; set; }
    }
}

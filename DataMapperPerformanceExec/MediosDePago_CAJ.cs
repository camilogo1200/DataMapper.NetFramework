using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataMapperPerformanceExec
{
    public class MediosDePago_CAJ
    {
        [Column("MDP_IdMedioPago")]
        public short IdMedioPago { get; set; }

        [Column("MDP_NombreMedioPago")]
        public string NombreMedioPago { get; set; }

        [Column("MDP_Descripcion")]
        public string Descripcion { get; set; }
    }
}

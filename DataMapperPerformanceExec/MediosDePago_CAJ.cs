using System.ComponentModel.DataAnnotations.Schema;

namespace DataMapperPerformanceExec
{
    public class MediosDePago_CAJ
    {
        [Column("MDP_Descripcion")]
        public string Descripcion { get; set; }

        [Column("MDP_IdMedioPago")]
        public short IdMedioPago { get; set; }

        [Column("MDP_NombreMedioPago")]
        public string NombreMedioPago { get; set; }
    }
}
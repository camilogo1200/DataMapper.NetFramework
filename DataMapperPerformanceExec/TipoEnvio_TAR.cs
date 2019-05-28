using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataMapperPerformanceExec
{
    public class TipoEnvio_TAR
    {
        [Column("TEN_CodigoMinisterio")]
        public decimal CodigoMinisterio { get; set; }

        [Column("TEN_CreadoPor")]
        public string CreadoPor { get; set; }

        [Column("TEN_Descripcion")]
        public string Descripcion { get; set; }

        [Column("TEN_Estado")]
        public string Estado { get; set; }

        [Column("TEN_FechaGrabacion")]
        public DateTime FechaGrabacion { get; set; }

        [Column("TEN_IdTipoEnvio")]
        public short IdTipoEnvio { get; set; }

        [Column("TEN_Nombre")]
        public string Nombre { get; set; }

        [Column("TEN_PesoMaximo")]
        public decimal PesoMaximo { get; set; }

        [Column("TEN_PesoMinimo")]
        public decimal PesoMinimo { get; set; }
    }
}

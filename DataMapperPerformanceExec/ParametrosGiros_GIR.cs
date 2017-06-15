using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataMapperPerformanceExec
{
    public class ParametrosGiros_GIR
    {
        /// <summary>
        /// Identificador del parametro.
        /// </summary>
        [Column("PAG_IdParametro")]
        public string IdParametro { get; set; }

        /// <summary>
        /// Campo que guarda el valor del parametro
        /// </summary>
        [Column("PAG_ValorParametro")]
        public string ValorParametro { get; set; }

        /// <summary>
        /// Descripcion del parametro
        /// </summary>
        [Column("PAG_Descripcion")]
        public string Descripcion { get; set; }

        /// <summary>
        /// Fecha de grabacion del parametro
        /// </summary>
        [Column("PAG_FechaGrabacion")]
        public DateTime FechaGrabacion { get; set; }

        /// <summary>
        /// Campo que guarda quien creo el parametro.
        /// </summary>
        [Column("PAG_CreadoPor")]
        public string CreadoPor { get; set; }
    }
}

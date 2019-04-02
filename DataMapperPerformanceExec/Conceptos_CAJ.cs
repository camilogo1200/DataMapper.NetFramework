using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace DataMapperPerformanceExec
{
    public class Conceptos_CAJ
    {
        [Column("CCA_CreadoPor")]
        public long CreadoPor { get; set; }

        [Column("CCA_CuentaContable")]
        public string CuentaContable { get; set; }

        [Column("CCA_Descripcion")]
        public string Descripcion { get; set; }

        [Column("CCA_EsIngreso")]
        public bool EsIngreso { get; set; }

        [Column("CCA_FechaCreacion")]
        public DateTime FechaCreacion { get; set; }

        [Column("CCA_IdCategoriaConcepto")]
        public short IdCategoriaConcepto { get; set; }

        [Column("CCA_IdConcepto")]
        public short IdConcepto { get; set; }

        [Column("CCA_IdServicio")]
        public int IdServicio { get; set; }

        [Column("CCA_Nombre")]
        public string Nombre { get; set; }

        [Column("CCA_RequiereBancoDestino")]
        public bool RequiereBancoDestino { get; set; }

        [Column("CCA_RequiereCsOrigen")]
        public bool RequiereCsOrigen { get; set; }

        [Column("CCA_RequiereDocumento")]
        public bool RequiereDocumento { get; set; }

        [Column("CCA_RequiereInactivo")]
        public bool RequiereInactivo { get; set; }

        [Column("CCA_RequiereTercero")]
        public bool RequiereTercero { get; set; }
    }
}
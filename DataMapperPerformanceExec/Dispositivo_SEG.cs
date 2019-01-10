using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataMapperPerformanceExec
{

    public class Dispositivo_SEG
    {
        [Column("DSE_UuidDispositivo")]
        Guid Uuid { get; set; }
        [Column("DSE_IdCentroServicio")]
        long CentroServicio { get; set; }
        [Column("DSE_FirmaDigital")]
        string FirmaDigital { get; set; }
        [Column("DSE_FechaCreacion")]
        DateTime FechaCreacion { get; set; }
        [Column("DSE_IdTipoDispositivo")]
        byte IdTipoDispositivo { get; set; }
        [Column("DSE_IdSistemaOperativo")]
        byte IdSistemaOperativo { get; set; }
        [Column("DSE_TieneAutorizacion")]
        bool TieneAutorizacion { get; set; }
        [Column("DSE_CodigoRegistro")]
        string CodigoRegistro { get; set; }
        [Column("DSE_DireccionMAC")]
        string MAC { get; set; }
        [Column("DSE_NombreMaquina")]
        string NombreMaquina { get; set; }
        [Column("DSE_NombreUsuario")]
        string NombreUsuario { get; set; }
        [Column("DSE_Idpais")]
        string IdPais { get; set; }
    }
}

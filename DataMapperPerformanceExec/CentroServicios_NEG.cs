using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations.Schema;
using System.Runtime.Serialization;

namespace CentroServicios
{
    public class CentroServicios_NEG
    {
        /// <summary>
        /// Indica si el centro de servicios es un PAM.
        /// </summary>
        [Column("CES_AplicaPAM")]
        public bool AplicaPAM { get; set; }

        /// <summary>
        /// Barrio del centro de servicios.
        /// </summary>
        [Column("CES_Barrio")]
        public string Barrio { get; set; }

        /// <summary>
        /// Valor fijo de base con el cual debe iniciar la caja de una agencia o punto cada vez que
        /// se haga una apertura.
        /// </summary>
        [Column("CES_BaseInicialCaja")]
        public decimal BaseInicialCaja { get; set; }

        /// <summary>
        /// </summary>
        [Column("CES_Biometrico")]
        public bool Biometrico { get; set; }

        /// <summary>
        /// ID del centro de costos asociado al centro de servicios.
        /// </summary>
        [Column("CES_IdCentroCostos")]
        public string CentroCostos { get; set; }

        /// <summary>
        /// Indica el municipio en el que se encuentra el centro de servicios.
        /// </summary>
        [Column("CES_IdCiudad")]
        public long Ciudad { get; set; }

        /// <summary>
        /// Clasifica a la agencia dependiendo de sus ingresos.
        /// </summary>
        [Column("CES_ClasGirosPorIngresos")]
        public string ClasGirosPorIngresos { get; set; }

        /// <summary>
        /// Codigo interno de 4/72
        /// </summary>
        [Column("CES_Codigo472")]
        public string Codigo472 { get; set; }

        /// <summary>
        /// Código de la bodega asociado al centro de servicios.
        /// </summary>
        [Column("CES_CodigoBodega")]
        public string CodigoBodega { get; set; }

        /// <summary>
        /// Indica el codigo postal del centro de servicios.
        /// </summary>
        [Column("CES_CodigoPostal")]
        public string CodigoPostal { get; set; }

        /// <summary>
        /// Este codigo es el que utiliza 4/72 para identificar las agencias de la empresa.
        /// </summary>
        [Column("CES_CodigoSPN")]
        public string CodigoSPN { get; set; }

        /// <summary>
        /// Indica el usuario por el cual fue creado el centro de servicios.
        /// </summary>
        [Column("CES_CreadoPor")]
        public long CreadoPor { get; set; }

        /// <summary>
        /// Digito de Verificacion.
        /// </summary>
        [Column("CES_DigitoVerificacion")]
        public string DigitoVerificacion { get; set; }

        /// <summary>
        /// Direccion del centro de servicios.
        /// </summary>
        [Column("CES_Direccion")]
        public string Direccion { get; set; }

        /// <summary>
        /// </summary>
        [Column("CES_DireccionAjustada")]
        public string DireccionAjustada { get; set; }

        /// <summary>
        /// Email del centro de servicios.
        /// </summary>
        [Column("CES_Email")]
        public string Email { get; set; }

        /// <summary>
        /// Indica el estado del centro de servicios: Activo, Inactivo, En Liquidacion.
        /// </summary>
        [Column("CES_Estado")]
        public short Estado { get; set; }

        /// <summary>
        /// Fax del centro de servicios.
        /// </summary>
        [Column("CES_Fax")]
        public string Fax { get; set; }

        /// <summary>
        /// Fecha en la cual se abre ese punto
        /// </summary>
        [Column("CES_FechaApertura")]
        public DateTime? FechaApertura { get; set; }

        /// <summary>
        /// Fecha en el cual se cierra el punto
        /// </summary>
        [Column("CES_FechaCierre")]
        public DateTime? FechaCierre { get; set; }

        /// <summary>
        /// Indica la fecha en la cual se creo el punto de atencion.
        /// </summary>
        [Column("CES_FechaGrabacion")]
        public DateTime FechaGrabacion { get; set; }

        /// <summary>
        /// Hora Cierre CS
        /// </summary>
        [Column("HCS_HoraFin")]
        public int HoraCierre { get; set; }

        /// <summary>
        /// Hora apertura CS
        /// </summary>
        [Column("HCS_HoraInicio")]
        public int HoraInicio { get; set; }

        /// <summary>
        /// Identificador del clasificador del canal de ventas, este es como se clasifica un centro
        /// de servicos en la aplicación de la ERP.
        /// </summary>
        [Column("CES_IdClasificadorCanalVenta")]
        public short IdClasificadorCanalVenta { get; set; }

        /// <summary>
        /// Id del centro de servicios
        /// </summary>
        [Column("CES_IdCentroServicios")]
        public long IdentificacionCS { get; set; }

        /// <summary>
        /// Indica el numero de identificacion del responsable de centro de servicios.
        /// </summary>
        [Column("CES_IdPersonaResponsable")]
        public long IdPersonaResponsable { get; set; }

        /// <summary>
        /// No ID propietario
        /// </summary>
        [Column("CES_IdPropietario")]
        public int IdPropietario { get; set; }

        /// <summary>
        /// Indica la zona en la cual se encuentra ubicada el Centro de Servicio en la Ciudad
        /// </summary>
        [Column("CES_IdZonaUbicacion")]
        public long? IdZonaUbicacion { get; set; }

        /// <summary>
        /// Coordenada "Latitud" de la ubicacion del centro de servicios.
        /// </summary>
        [Column("CES_Latitud")]
        public decimal? Latitud { get; set; }

        /// <summary>
        /// Coordenada "Longitud" de la ubicacion del centro de servicios.
        /// </summary>
        [Column("CES_Longitud")]
        public decimal? Longitud { get; set; }

        /// <summary>
        /// </summary>
        [Column("CES_NombreAMostrar")]
        public string NombreAMostrar { get; set; }

        /// <summary>
        /// Nombre del centro de servicios.
        /// </summary>
        [Column("CES_Nombre")]
        public string NombreCES { get; set; }

        /// <summary>
        /// Indica el valor maximo que se puede pagar por un giro en el centro de servicio.
        /// </summary>
        [Column("CES_NombreCiudad")]
        public string NombreCiudad { get; set; }

        /// <summary>
        /// Indica el valor maximo que se puede pagar por un giro en el centro de servicio.
        /// </summary>
        [Column("CES_NombreDepartamento")]
        public string NombreDepartamento { get; set; }

        /// <summary>
        /// Nombre tipo del centro de servicios.
        /// </summary>
        [Column("NombreTipoCS")]
        public string NombreTipoCS { get; set; }

        /// <summary>
        /// Indica si la agencia puede o no pagar giros.
        /// </summary>
        [Column("CES_PuedePagarGiros")]
        public bool PagarGiros { get; set; }

        /// <summary>
        /// Indica si el centro de servicios admite pagos al cobro.
        /// </summary>
        [Column("CES_AdmiteFormaPagoAlCobro")]
        public bool PagoAlCobro { get; set; }

        /// <summary>
        /// Peso maximo del centro de servicios para envio o recepcion.
        /// </summary>
        [Column("CES_PesoMaximo")]
        public decimal PesoMaximo { get; set; }

        /// <summary>
        /// Indica si la agencia puede o no recibir giros.
        /// </summary>
        [Column("CES_PuedeRecibirGiros")]
        public bool RecibirGiros { get; set; }

        /// <summary>
        /// Indica si el centro de servicios es sistematizado o manual.
        /// </summary>
        [Column("CES_Sistematizada")]
        public bool Sistematizada { get; set; }

        /// <summary>
        /// Primer telefono del centro de servicios.
        /// </summary>
        [Column("CES_Telefono1")]
        public string Telefono1 { get; set; }

        /// <summary>
        /// Segundo telefono del centro de servicios.
        /// </summary>
        [Column("CES_Telefono2")]
        public string Telefono2 { get; set; }

        /// <summary>
        /// Tipo del centro de servicios.
        /// </summary>
        [Column("CES_Tipo")]
        public short TipoCES { get; set; }

        /// <summary>
        /// Identificador del tipo de propiedad.
        /// </summary>
        [Column("CES_IdTipoPropiedad")]
        public short TipoPropiedad { get; set; }

        /// <summary>
        /// Valor máximo acumulado que la agencia puede recaudar por concepto de recepción de giros.
        /// </summary>
        [Column("CES_TopeMaximoPorGiros")]
        public decimal TopeMaximoPorGiros { get; set; }

        /// <summary>
        /// Valor máximo acumulado que la agencia puede pagar por concepto de pago de giros.
        /// </summary>
        [Column("CES_TopeMaximoPorPagos")]
        public decimal TopeMaximoPorPagos { get; set; }

        /// <summary>
        /// Indica el valor maximo que se puede pagar por un giro en el centro de servicio.
        /// </summary>
        [Column("CES_TopePagoGiros")]
        public long? TopePagoGiros { get; set; }

        /// <summary>
        /// Indicador si la agencia puede vender tiquetes o voucher prepago.
        /// </summary>
        [Column("CES_VendePrepago")]
        public bool VendePrepago { get; set; }

        /// <summary>
        /// Volumen maximo del centro de servicios para envio o recepcion.
        /// </summary>
        [Column("CES_VolumenMaximo")]
        public decimal VolumenMaximo { get; set; }

        /// <summary>
        /// Indica la zona en la que se encuentra el centro de servicios.
        /// </summary>
        [Column("CES_IdZona")]
        public string Zona { get; set; }
    }
}


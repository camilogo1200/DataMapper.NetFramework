﻿using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace DataMapperPerformanceExec
{
    public class AdmisionGiro_GIR
    {
        /// <summary>
        /// Determina si la admisión es automática
        /// </summary>
        [Column("ADG_AdmisionAutomatica")]
        public bool AdmisionAutomatica { get; set; }

        /// <summary>
        /// Año de Creación
        /// </summary>
        [Column("ADG_AnoCreacion")]
        public short AnoCreacion { get; set; }

        /// <summary>
        /// Creado por
        /// </summary>
        [Column("ADG_CreadoPor")]
        public long CreadoPor { get; set; }

        /// <summary>
        /// Dia de Creación
        /// </summary>
        [Column("ADG_DiaCreacion")]
        public short DiaCreacion { get; set; }

        /// <summary>
        /// Digito de Verificación del giro
        /// </summary>
        [Column("ADG_DigitoVerificacion")]
        public string DigitoVerificacion { get; set; }

        /// <summary>
        /// Fecha de grabación
        /// </summary>
        [Column("ADG_FechaGrabacion")]
        public DateTime FechaGrabacion { get; set; }

        /// <summary>
        /// Centro de servidio de Destino
        /// </summary>
        [Column("ADG_IdCentroServicioDestino")]
        public long IdCentroServicioDestino { get; set; }

        /// <summary>
        /// Centro de servicio de Origen
        /// </summary>
        [Column("ADG_IdCentroServicioOrigen")]
        public long IdCentroServicioOrigen { get; set; }

        /// <summary>
        /// Id del destinatario
        /// </summary>
        [Column("ADG_IdDestinatario")]
        public long IdDestinatario { get; set; }

        /// <summary>
        /// Identificación del destinatario
        /// </summary>
        [Column("IdentificacionDes")]
        public string IdentificacionDestinatario { get; set; }

        /// <summary>
        /// Identificación del remitente
        /// </summary>
        [Column("IdentificacionRem")]
        public string IdentificacionRemitente { get; set; }

        /// <summary>
        /// Identificación del Estado del Giro
        /// </summary>
        [Column("ADG_IdEstadoGiro")]
        public short IdEstadoGiro { get; set; }

        /// <summary>
        /// Identificador de la Factura Del Giro.
        /// </summary>
        [Column("ADG_IdFacturaGiro")]
        public long IdFacturaGiro { get; set; }

        /// <summary>
        /// Idn del Remitente
        /// </summary>
        [Column("ADG_IdRemitente")]
        public long IdRemitente { get; set; }

        /// <summary>
        /// Identificación de Telemercadeo
        /// </summary>
        [Column("ADG_IdTelemercadeo")]
        public long IdTelemercadeo { get; set; }

        /// <summary>
        /// Identificación de Transmisión
        /// </summary>
        [Column("ADG_IdTransmision")]
        public long IdTransmision { get; set; }

        /// <summary>
        /// Identificación Transmisión Telefónica
        /// </summary>
        [Column("ADG_IdTransmisionTelefonica")]
        public long? IdTransmisionTelefonica { get; set; }

        /// <summary>
        /// Mes de Creación
        /// </summary>
        [Column("ADG_MesCreacion")]
        public short MesCreacion { get; set; }

        [Column("ADG_NombreDestinatario")]
        public string NombreDestinatario { get; set; }

        [Column("ADG_NombreRemitente")]
        public string NombreRemitente { get; set; }

        public string NumeroCelularDestinatario { get; set; }

        public string NumeroCelularRemitente { get; set; }

        /// <summary>
        /// Para agregar comentario adicionales en la Admisión del Giro.
        /// </summary>
        [Column("ADG_Observaciones")]
        public string Observaciones { get; set; }

        /// <summary>
        /// Ruta Declaración Origenes
        /// </summary>
        [Column("ADG_RutaDeclaracionOrigenes")]
        public string RutaDeclaracionOrigenes { get; set; }

        /// <summary>
        /// Tarifa por cPorte
        /// </summary>
        [Column("ADG_TarifaPorcPorte")]
        public decimal TarifaPorcPorte { get; set; }

        public string TipoCentroServicioSuministros { get; set; }

        public string Usuario { get; set; }

        /// <summary>
        /// Valor del giro
        /// </summary>
        [Column("ADG_ValorGiro")]
        public decimal ValorGiro { get; set; }

        /// <summary>
        /// Valor del Porte
        /// </summary>
        [Column("ADG_ValorPorte")]
        public decimal ValorPorte { get; set; }

        [Column("ADG_ValorTotal")]
        public decimal? ValorTotal { get; set; }
    }
}
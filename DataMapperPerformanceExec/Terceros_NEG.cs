﻿using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace DataMapperPerformanceExec
{
    /// <summary>
    /// Entidad tercero
    /// </summary>
    public class Terceros_NEG
    {
        /// <summary>
        /// Define la ciudad del tercero
        /// </summary>
        [Column("TER_Ciudad")]
        public long Ciudad { get; set; }

        /// <summary>
        /// Clave telefonica para realizar giros
        /// </summary>
        [Column("TER_ClaveTelefonicaGiros")]
        public string ClaveTelefonicaGiros { get; set; }

        /// <summary>
        /// Id usuario crea
        /// </summary>
        [Column("TER_CreadoPor")]
        public long CreadoPor { get; set; }

        /// <summary>
        /// Direccion
        /// </summary>
        [Column("TER_Direccion")]
        public string Direccion { get; set; }

        /// <summary>
        /// Direccion normalizada
        /// </summary>
        [Column("TER_DireccionNormalizada")]
        public string DireccionNormalizada { get; set; }

        /// <summary>
        /// Correo electronico
        /// </summary>
        [Column("TER_Email")]
        public string Email { get; set; }

        /// <summary>
        /// Es cliente externo
        /// </summary>
        [Column("TER_EsCliente")]
        public bool? EsCliente { get; set; }

        /// <summary>
        /// Define si el cliente es credito o no
        /// </summary>
        [Column("TER_EsClienteCredito")]
        public bool EsClienteCredito { get; set; }

        /// <summary>
        /// Es cliente contratista
        /// </summary>
        [Column("TER_EsContratista")]
        public bool? EsContratista { get; set; }

        /// <summary>
        /// Es cliente interno
        /// </summary>
        [Column("TER_EsEmpleado")]
        public bool? EsEmpleado { get; set; }

        /// <summary>
        /// Es persona juridica
        /// </summary>
        [Column("TER_EsPersonaJuridica")]
        public bool? EsPersonaJuridica { get; set; }

        /// <summary>
        /// Es cliente proveedor
        /// </summary>
        [Column("TER_EsProveedor")]
        public bool? EsProveedor { get; set; }

        /// <summary>
        /// Fecha de creacion
        /// </summary>
        [Column("TER_FechaGrabacion")]
        public DateTime? FechaGrabacion { get; set; }

        /// <summary>
        /// Numero de identificacion
        /// </summary>
        [Column("TER_Identificacion")]
        public string Identificacion { get; set; }

        /// <summary>
        /// Id del tercero
        /// </summary>
        [Column("TER_IdTercero")]
        public long IdTercero { get; set; }

        /// <summary>
        /// Primer apellido
        /// </summary>
        [Column("TER_Apellido1")]
        public string PrimerApellido { get; set; }

        /// <summary>
        /// Primer nombre
        /// </summary>
        [Column("TER_Nombre1")]
        public string PrimerNombre { get; set; }

        /// <summary>
        /// Razon social
        /// </summary>
        [Column("TER_RazonSocial")]
        public string RazonSocial { get; set; }

        /// <summary>
        /// Muestra si es representanyte legal
        /// </summary>
        [Column("TER_EsRepresentanteLegal")]
        public bool Representantelegal { get; set; }

        /// <summary>
        /// Segundo apellido
        /// </summary>
        [Column("TER_Apellido2")]
        public string SegundoApellido { get; set; }

        /// <summary>
        /// Segundo nombre
        /// </summary>
        [Column("TER_Nombre2")]
        public string SegundoNombre { get; set; }

        /// <summary>
        /// Numero de telefono
        /// </summary>
        [Column("TER_Telefono")]
        public string Telefono { get; set; }

        /// <summary>
        /// Id del tipo de identificacion
        /// </summary>
        [Column("TER_TipoId")]
        public short TipoIdentificacion { get; set; }
    }
}
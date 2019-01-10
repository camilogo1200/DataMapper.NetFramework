using DataMapper;
using System;
using System.Data.SqlClient;

namespace DataMapperPerformanceExec
{
    class Program
    {

        static void Main(String[] args)
        {


            //DataMapper<Terceros_NEG> mapperter = DataMapper<Terceros_NEG>.Instancia;

            //Terceros_NEG tercero = new Terceros_NEG();

            //tercero.Ciudad = 4;
            //tercero.CreadoPor = 8302002;
            //tercero.Direccion = "AGE/ABEJORRAL/ANT/COL/CALLE 51 # 49-47";
            //tercero.Email = "";
            //tercero.EsCliente = true;
            //tercero.Identificacion = "53177962";
            //tercero.PrimerApellido = "ALBA";
            //tercero.PrimerNombre = "JOSE";
            //tercero.SegundoApellido = "MOLANO";
            //tercero.SegundoNombre = "NULL";
            //tercero.Telefono = "3151261613";
            //tercero.TipoIdentificacion = 1;
            //tercero.FechaGrabacion = DateTime.Now;
            //SqlParameter[] parameters = mapperter.createParametersFromEntity(tercero);
            //tercero = mapperter.ExecuteCreateSP<Terceros_NEG>(tercero, "paInsertaActualizaTercero_NEG", parameters);


            DataMapper<AdmisionGiro_GIR> mapper = DataMapper<AdmisionGiro_GIR>.Instancia;

            AdmisionGiro_GIR Giros = new AdmisionGiro_GIR();


            Random r = new Random();

            Giros.IdFacturaGiro = r.Next();
            Giros.DigitoVerificacion = Convert.ToString(r.Next(1, 50));
            Giros.IdEstadoGiro = 1;
            Giros.FechaGrabacion = DateTime.Now;
            Giros.DiaCreacion = short.Parse(DateTime.Now.Day.ToString());
            Giros.MesCreacion = short.Parse(DateTime.Now.Month.ToString());
            Giros.AnoCreacion = short.Parse(DateTime.Now.Year.ToString());
            Giros.IdTransmisionTelefonica = 0;
            Giros.AdmisionAutomatica = true;
            Giros.IdCentroServicioOrigen = 1295;
            Giros.IdCentroServicioDestino = 2664;
            Giros.ValorGiro = 123456;
            Giros.TarifaPorcPorte = 7500;
            Giros.ValorPorte = 7500;
            Giros.Observaciones = "pruebamapper";
            Giros.IdRemitente = 8302002;
            Giros.IdDestinatario = 8376097;
            Giros.CreadoPor = 8302002;
            Giros.IdTransmision = 1;
            Giros.IdTelemercadeo = 1;
            Giros.IdEstadoGiro = 1;
            Giros.RutaDeclaracionOrigenes = "1";
            Giros.IdTransmisionTelefonica = 1;
            Giros.NombreDestinatario = "WILMER  PEÑUELA ESPINOSA";
            Giros.NombreRemitente = "YEISON GUSTAVO ALBARRACIN MOLINA";
            Giros.NumeroCelularDestinatario = "3164646464";
            Giros.NumeroCelularRemitente = "3115262042";
            Giros.Usuario = "YeisonGAlbarracinM";
            SqlParameter[] parameters = mapper.createParametersFromEntity(Giros);
            Giros = mapper.ExecuteCreateSP<AdmisionGiro_GIR>(Giros, "giros.paInsertarGiro", parameters);
        }
    }

}

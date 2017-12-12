using CentroServicios;
using DataMapper;
using RestSharp;
using RestSharp.Deserializers;
using Servidor.Comun.DataMapper;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

namespace DataMapperPerformanceExec
{
    class Program
    {

        static void Main(String[] args)
        {


            DataMapper<Terceros_NEG> mapperter = DataMapper<Terceros_NEG>.Instancia;

            Terceros_NEG tercero = new Terceros_NEG();

            tercero.Ciudad = 4;
            tercero.CreadoPor = 8302002;
            tercero.Direccion = "AGE/ABEJORRAL/ANT/COL/CALLE 51 # 49-47";
            tercero.Email = "";
            tercero.EsCliente = true;
            tercero.Identificacion = "53177962";
            tercero.PrimerApellido = "ALBA";
            tercero.PrimerNombre = "JOSE";
            tercero.SegundoApellido = "MOLANO";
            tercero.SegundoNombre = "NULL";
            tercero.Telefono = "3151261613";
            tercero.TipoIdentificacion = 1;
            tercero.FechaGrabacion = DateTime.Now;
            SqlParameter[] parameters = mapperter.createParametersFromEntity(tercero);
            tercero = mapperter.ExecuteCreateSP<Terceros_NEG>(tercero, "paInsertaActualizaTercero_NEG", parameters);


            //DataMapper<AdmisionGiro_GIR> mapper = DataMapper<AdmisionGiro_GIR>.Instancia;

            ////ICollection<AdmisionGiro_GIR> pr = mapper.ExecuteSelectSP("paObtenerTiposdeEntrega");
            ////mapper.GetAll();
            //AdmisionGiro_GIR Giros = null;


            ////string URL = Reposirorioparametros.ObtenerParametrosPorId("URLSUMINISTROS").ElementAt(0).ValorParametro;
            //string URL = "http://190.60.249.210/co.servidor.servicios.webapi/api/Suministros/ObtenerNumeroSuministroActual/5";   //URL + "ObtenerNumeroSuministroActual/5";
            //                                                                                                                     //for (int i = 0; i <= 1000; i++)
            //                                                                                                                     //{
            //Giros = new AdmisionGiro_GIR();
            //SqlParameter[] parameters;

            //RestClient client = new RestClient(URL);
            //var request = new RestRequest(Method.GET);
            //long IdSuministros = 0;
            //request.AddHeader("usuario", "admin");
            //IRestResponse response = client.Execute(request);
            //if (response.StatusCode == HttpStatusCode.OK && !(String.Equals("null", response.Content, StringComparison.OrdinalIgnoreCase)))
            //{
            //    JsonDeserializer json = new JsonDeserializer();
            //    SuministroWrapper output = json.Deserialize<SuministroWrapper>(new RestResponse { Content = response.Content });
            //    if (output != null)
            //    {
            //        IdSuministros = output.ValorActual;
            //    }
            //    Random r = new Random();
            //    //TODO consumir api suministros
            //    Giros.IdFacturaGiro = IdSuministros;
            //    Giros.DigitoVerificacion = Convert.ToString(r.Next(1, 50));
            //    Giros.IdEstadoGiro = 1;
            //    Giros.FechaGrabacion = DateTime.Now;
            //    Giros.DiaCreacion = short.Parse(DateTime.Now.Day.ToString());
            //    Giros.MesCreacion = short.Parse(DateTime.Now.Month.ToString());
            //    Giros.AnoCreacion = short.Parse(DateTime.Now.Year.ToString());
            //    Giros.IdTransmisionTelefonica = 0;
            //    Giros.AdmisionAutomatica = true;
            //    Giros.IdCentroServicioOrigen = 1295;
            //    Giros.IdCentroServicioDestino = 2664;
            //    Giros.ValorGiro = 123456;
            //    Giros.TarifaPorcPorte = 7500;
            //    Giros.ValorPorte = 7500;
            //    Giros.Observaciones = "pruebamapper";
            //    Giros.IdRemitente = 8302002;
            //    Giros.IdDestinatario = 8376097;
            //    Giros.CreadoPor = 8302002;
            //    Giros.IdTransmision = 1;
            //    Giros.IdTelemercadeo = 1;
            //    Giros.IdEstadoGiro = 1;
            //    Giros.RutaDeclaracionOrigenes = "1";
            //    Giros.IdTransmisionTelefonica = 1;
            //    Giros.NombreDestinatario = "WILMER  PEÑUELA ESPINOSA";
            //    Giros.NombreRemitente = "YEISON GUSTAVO ALBARRACIN MOLINA";
            //    Giros.NumeroCelularDestinatario = "3164646464";
            //    Giros.NumeroCelularRemitente = "3115262042";
            //    Giros.Usuario = "YeisonGAlbarracinM";
            //    //mapper.Create(ref Giros);
            //    parameters = mapper.createParametersFromEntity(Giros);
            //    Giros = mapper.ExecuteCreateSP<AdmisionGiro_GIR>(Giros, "giros.paInsertarGiro", parameters);
            //    //}


            //    //            //for (int i = 0; i <= 100; i++)
            //    //            //{
            //    //            //    DataMapper<ParametrosGiros_GIR> mapper = null;
            //    //            //    mapper = DataMapper<ParametrosGiros_GIR>.Instancia;
            //    //            //    ICollection<ParametrosGiros_GIR> r = mapper.findByAttribute("URLSUMINISTROS", "IdParametro");
            //    //            //    Console.WriteLine(r);
            //    //        }
            //    //        //DataMapper<AdmisionGiro_GIR> mapper = DataMapper<AdmisionGiro_GIR>.Instancia;
            //    //        //string column = "ADG_IdDestinatario";
            //    //        //ICollection<AdmisionGiro_GIR> result = mapper.findByAttribute("11", "IdFacturaGiro",true, column);
            //    //        //Console.WriteLine(result);
            //    //        //Console.Read();

            //    //        Console.ReadLine();

            //    //    }
            //    //}

            //    }
        }
    }
}



//        Stopwatch st = new Stopwatch();
//        st.Start();
//        try
//        {
//            int init = 201;
//            for (int i = 0; i < 2; i++)
//            {
//                new Thread(e =>
//                {
//                    NewMethod(init);
//                }).Start();
//                init += 2001;
//            }
//            List<int> lista = new List<int>();
//            for (int i = 201; i < 22201; i++)
//            {
//                lista.Add(i);
//            }
//            Random r = new Random(50000);

//            Parallel.ForEach(lista, item => { NewMethod(item); });

//            mapper.GetAll();

//        }
//        catch (Exception ex)
//        {
//            Console.WriteLine(ex);
//            Console.Read();
//        }
//        st.Stop();
//        Console.WriteLine("Time : " + st.Elapsed);
//        Console.WriteLine("Time Milliseconds : " + st.ElapsedMilliseconds);
//        Console.WriteLine("Time ElapsedTicks : " + st.ElapsedTicks);
//        Console.ReadLine();
//    }

//    private static void NewMethod(int init)
//    {
//        DataMapper<AdmisionGiro_GIR> mapper = DataMapper<AdmisionGiro_GIR>.Instancia;


//        AdmisionGiro_GIR gir = new AdmisionGiro_GIR
//        {

//            IdFacturaGiro = init,
//            DigitoVerificacion = "5",
//            AdmisionAutomatica = true,
//            IdCentroServicioOrigen = 1,
//            IdCentroServicioDestino = 2,
//            ValorGiro = 12000,
//            TarifaPorcPorte = 0,
//            ValorPorte = 0,
//            ValorTotal = 10000,
//            Observaciones = "prueba15",
//            IdRemitente = 1,
//            IdDestinatario = 2,
//            FechaGrabacion = DateTime.Now,
//            AnoCreacion = 2017,
//            MesCreacion = 05,
//            DiaCreacion = 22,
//            CreadoPor = 1,
//            IdTransmision = 1,
//            IdTelemercadeo = 1,
//            IdEstadoGiro = 1,
//            RutaDeclaracionOrigenes = "1",
//            IdTransaccionGiro = "2",
//            IdTransmisionTelefonica = 1

//        };
//        Console.Write(".");
//        SqlParameter[] parameters = mapper.createParametersFromEntity(gir);
//        mapper.ExecuteCreateSP<AdmisionGiro_GIR>("paInsertarGiro", parameters);
//        gir.IdFacturaGiro = i;
//        mapper.Create(ref gir);

//}
//    }
//}

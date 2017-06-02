using CentroServicios;
using DataMapper;
using Servidor.Comun.DataMapper;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace DataMapperPerformanceExec
{
    class Program
    {

        static void Main(String[] args)
        {
            DataMapper<AdmisionGiro_GIR> mapper = DataMapper<AdmisionGiro_GIR>.Instancia;
            string column = "ADG_IdDestinatario";
            ICollection<AdmisionGiro_GIR> result = mapper.findByAttribute("11", "IdFacturaGiro",true, column);
            Console.WriteLine(result);
            Console.Read();

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

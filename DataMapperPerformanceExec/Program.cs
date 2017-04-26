using DataMapper;
using Servidor.Comun.DataMapper;
using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace DataMapperPerformanceExec
{
    class Program
    {
        static void Main(string[] args)
        {
            DataMapper<Terceros_NEG> mapper = DataMapper<Terceros_NEG>.Instancia;
            //Stopwatch st = new Stopwatch();

            String attribute = "Identificacion";
            String value = "1020756125";
            //st.Start();
            Terceros_NEG u = new Terceros_NEG
            {
              TipoIdentificacion= 1,
              Identificacion= "1020756127",
              PrimerNombre= "Camilo",
              SegundoNombre= "Andres",
              PrimerApellido= "Corredor",
              SegundoApellido= "pepito",
              RazonSocial= null,
              Telefono= "3000000",
              Direccion= "Calle 5 # 5 -89",
              Email= "pruebas@gmail.com",
              FechaGrabacion= DateTime.Now,
              CreadoPor= 1,
              IdTercero= 2867240,
              EsEmpleado= true,
              EsCliente= true,
              EsPersonaJuridica= false,
              DireccionNormalizada= null,
              ClaveTelefonicaGiros= "123456",
              EsProveedor= false,
              EsContratista= false
            };

            for (int i = 0; i < 2000; i++)
            {
                ICollection<Terceros_NEG> collection = mapper.findByAttribute(value, attribute, false);
            }
            // mapper.Create(ref u);
            //mapper.Update(u);
            //mapper.Delete(u);
            //st.Stop();
            //Console.WriteLine("Elapsed FindByAttr = {0}",st.ElapsedMilliseconds);



            //st.Restart();
            //for (int i = 0; i < 2000; i++)
            //{
            //    mapper.Create(ref u);
            //}
            //st.Stop();
            //Console.WriteLine("Elapsed Create = {0}", st.ElapsedMilliseconds);

            // Int64 i = mapper.Count();

            //Console.ReadLine();
        }
    }
}

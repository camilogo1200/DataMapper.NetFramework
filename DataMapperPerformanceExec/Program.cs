using DataMapper;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;

namespace DataMapperPerformanceExec
{
    internal class Program
    {
        private static void Main(String[] args)
        {
            try
            {
                //DataMapper<Conceptos_CAJ> repositorio = DataMapper<Conceptos_CAJ>.Instancia; ;
                DataMapper<TipoEnvio_TAR> mapper = DataMapper<TipoEnvio_TAR>.Instancia;

                //ICollection<Conceptos_CAJ> conceptos = repositorio.ExecuteSelectSP("cajas.paObtenerConceptosCategoria", new SqlParameter("@IdCategoriaConcepto", 9));

                var TipoEnvios = mapper.ExecuteSelectSP("dbo.paObtenerTipoEmpaque");

                Console.ReadLine();
            }
            catch (Exception EX)
            {
                Console.WriteLine(EX.Message);
                Console.ReadLine();
            }
        }
    }
}
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
                DataMapper<Conceptos_CAJ> repositorio = DataMapper<Conceptos_CAJ>.Instancia; ;

                ICollection<Conceptos_CAJ> conceptos = repositorio.ExecuteSelectSP("cajas.paObtenerConceptosCategoria", new SqlParameter("@IdCategoriaConcepto", 9));
                
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
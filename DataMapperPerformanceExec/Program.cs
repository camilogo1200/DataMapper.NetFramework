using CentroServicios;
using DataMapper;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;

namespace DataMapperPerformanceExec
{
    class Program
    {

        static void Main(String[] args)
        {
            try
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
                DataMapper<MediosDePago_CAJ> mapper = DataMapper<MediosDePago_CAJ>.Instancia; ;
                //mapper = DataMapper<CentroServicios_NEG>.Instancia;
                //ICollection<CentroServicios_NEG> lcentrosServicio = mapper.ExecuteSelectSP("negocio.paObtenerHorariosCentrosServicio");
                ICollection<MediosDePago_CAJ> lMediosPago = mapper.ExecuteSelectSP("cajas.paObtenerMediosPago");
                if (lMediosPago == null || lMediosPago.Count < 1)
                {
                    throw new Exception("Medios de pago no disponibles en cajas.");
                }

                //DataMapper<Conceptos_CAJ> mapper2 = DataMapper<Conceptos_CAJ>.Instancia;

                //Conceptos_CAJ concepto = new Conceptos_CAJ
                //{
                //    CreadoPor = -1,
                //    CuentaContable = null,
                //    Nombre = "Prueba DataMapper :-P",
                //    IdServicio = 8,
                //    IdCategoriaConcepto = 13
                //};

                //int IdConceptoPrincipal = 12;

                //Conceptos_CAJ conceptso = mapper2.ExecuteSelectSP("cajas.paObtenerDuplaConcepto", new SqlParameter("IdConcepto", IdConceptoPrincipal))?.ElementAt(0);

                //int? dupla = conceptso?.IdConcepto;
                //if (dupla == null || dupla == 0)
                //{
                //    throw new Exception("No se a encontrado dupla para el concepto");
                //}
                //Console.WriteLine(Convert.ToInt32(dupla));
                Console.ReadLine();
            }
            catch (Exception EX) {
                Console.WriteLine(EX.Message);
                Console.ReadLine();
            }
        }
    }

}

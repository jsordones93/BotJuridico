using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Web;

namespace ChatBot.Clases
{
    public class PreguntasAclaratorias
    {
        public static string devuelveConcepto(string cad)//busqueda de preguntas relacionadas con conceptos
        {
            string concep = "";
            List<Concepto> listaConceptos = new List<Concepto>();
            using (SqlConnection cnx = new SqlConnection("Data Source=DESKTOP-CSTLMVP\\SQLEXPRESS;Initial Catalog=chat_bot;Integrated Security=True"))
            {
                cnx.Open();
                //cargando tabla Concepto en lista de Conceptos
                SqlCommand cmdConceptos = new SqlCommand(string.Format("Select * from Concepto"),cnx);
                SqlDataReader readerConceptos = cmdConceptos.ExecuteReader();
                while (readerConceptos.Read())
                {
                    Concepto concepto = new Concepto();
                    concepto.codCon = readerConceptos.GetString(0);
                    concepto.nombre = readerConceptos.GetString(1);
                    concepto.descripcion = readerConceptos.GetString(2);
                    listaConceptos.Add(concepto);
                }
                cad = cad.ToLower();//se vuelve minuscula la cadena de parametro
                bool sw = false;
                    foreach (Concepto co in listaConceptos)
                    {
                        if (!sw)
                        {
                            if (co.nombre.Equals("folio real") && cad.IndexOf("folio real") != -1)
                            {
                                concep = co.descripcion;
                                sw = true;
                            }
                            if (co.nombre.Equals("contrato") && cad.IndexOf("contrato") != -1)
                            {
                                concep = co.descripcion;
                                sw = true;
                            }
                        
                        }
                    }
                
                /*
                SqlCommand cmd = new SqlCommand(string.Format("Select * from Concepto"), cnx);
                SqlDataReader reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    Concepto concepto = new Concepto();
                    concepto.codCon = reader.GetString(0);
                    concepto.nombre = reader.GetString(1);
                    concepto.descripcion = reader.GetString(2);
                    lista.Add(concepto);
                }*/
                cnx.Close();
                return concep;
            }
             
        }
            
             
        
    }
}
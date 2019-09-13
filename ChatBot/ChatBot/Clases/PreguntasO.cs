using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Web;

namespace ChatBot.Clases
{
    public class PreguntasO
    {
        public static string devuelveBotonesO(string cad)
        {
            List<Respuesta> listaResp = new List<Respuesta>();
            List<Entidad_Sinonimos> listaSin = new List<Entidad_Sinonimos>();
            List<Pregunta> listPre = new List<Pregunta>();
            cad = cad.ToLower();
            string respuesta = "";
            using (SqlConnection cnx = new SqlConnection("Data Source=DESKTOP-CSTLMVP\\SQLEXPRESS;Initial Catalog=chat_bot;Integrated Security=True"))
            {

                SqlCommand cmd;
                SqlDataReader reader;
                cnx.Open();
                //cargando tabla Respuesta en lista de Respuestas listResp
                cmd = new SqlCommand(string.Format("Select * from Respuesta"), cnx);
                reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    Respuesta resp = new Respuesta();
                    resp.codRes = reader.GetString(0);
                    resp.codPreg = reader.GetString(1);
                    resp.nroSin = reader.GetInt32(2);
                    resp.descripcion = reader.GetString(3);
                    listaResp.Add(resp);
                }
                cnx.Close();

                cnx.Open();
                //cargando tabla Sinonimos en lista de Sinonimos listSin
                cmd = new SqlCommand(string.Format("Select * from Entidad_Sinonimos"), cnx);
                reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    Entidad_Sinonimos ensin = new Entidad_Sinonimos();
                    ensin.nro = reader.GetInt32(0);
                    ensin.termino = reader.GetString(1);
                    ensin.sinonimo1 = reader.GetString(2);
                    ensin.sinonimo2 = reader.GetString(3);
                    ensin.sinonimo3 = reader.GetString(4);
                    ensin.sinonimo4 = reader.GetString(5);
                    ensin.sinonimo5 = reader.GetString(6);
                    listaSin.Add(ensin);
                }
                cnx.Close();
                /*
                cnx.Open();
                //cargando tabla Preguntas en lista de Preguntas listPre
                cmd = new SqlCommand(string.Format("Select * from Pregunta"), cnx);
                reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                   Pregunta pre = new Pregunta();
                    pre.codPreg = reader.GetString(0);
                    pre.descripcion = reader.GetString(1);
                    pre.tipo = reader.GetString(2);
                    listPre.Add(pre);
                }
                cnx.Close();
                */
                //bool sw = false;
                foreach (Respuesta pRes in listaResp)
                {
                    foreach(Entidad_Sinonimos ensin in listaSin)
                    {
                        if(pRes.nroSin == ensin.nro)
                        {
                            if (cad.IndexOf(ensin.termino) != -1 || cad.IndexOf(ensin.sinonimo1) != -1 || cad.IndexOf(ensin.sinonimo2) != -1 || cad.IndexOf(ensin.sinonimo3) != -1 || cad.IndexOf(ensin.sinonimo5) != -1 || cad.IndexOf(ensin.sinonimo4) != -1)
                                respuesta = pRes.descripcion;
                                
                        }
                    }
                }
                return respuesta;
            }
        }
    }
}
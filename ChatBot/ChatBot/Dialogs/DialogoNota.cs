using System;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.Luis;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Bot.Builder.Luis.Models;
using System.Data.SqlClient;
using Microsoft.Bot.Connector;
namespace ChatBot.Dialogs
{
    //[LuisModel(modelID: "94018890-6d35-4e39-a6cf-d46c82b79ae4", subscriptionKey: "d3d02d538bc749d0b5b60900ca419d5f")]
    [LuisModel(modelID: "bff13aff-589c-47a2-b4b1-6fa0d30b1a62", subscriptionKey: "50dec5fec7294f6a9596901e82bc05b9")]
    [Serializable]
    public class Dialogo : LuisDialog<object>
    {
        const string tramCv = "trámite/pasos para compra venta", saberCv = "saber sobre compra venta", otraPreg = "otra pregunta";
        const string req = "Requisitos";
        const string proc = "Procedimiento";
        const string quien = "¿Quien solicita?";
        const string costo = "Costo";
        const string tiempo = "Duración";
        static bool swCm = false, swPr = false, swAl = false, swRd = false;
        //para minuta
        const string esCasada = "¿Si la persona es casada?";
        const string nombreIncompleto = "¿Si no está completo el nombre del vendedor?";
        //-------------------I N T E N C I O N E S---------------------------
        //INTENCIONES COMUNES
        [LuisIntent("None")]
        public async Task None(IDialogContext context, LuisResult result)
        {
            await context.PostAsync("No logro entender, intenta con otro mensaje, por ejemplo saludame, di gracias, despidete o pide la referencia de un abogado");
            await Task.Delay(2000);
            await context.PostAsync("¿necesitas ayuda?");
        }
        [LuisIntent("Saludar")]
        public async Task Saluda(IDialogContext context, LuisResult result)
        {
            await context.PostAsync("Hola, soy tu amigo bot y puedo contestar preguntas de compra venta de inmuebles");
            await Task.Delay(2000);
            PromptDialog.Choice(context, SeleccionInicial, new[] { tramCv, saberCv, otraPreg }, "¿Qué deseas preguntar?", "Elige una opción", 3);
        }
        [LuisIntent("Agradecer")]
        public async Task Agradece(IDialogContext context, LuisResult result)
        {
            await context.PostAsync("De nada, tu amigo bot esta a tu servicio");
            await Task.Delay(2000);
        }
        [LuisIntent("Despedirse")]
        public async Task Despedir(IDialogContext context, LuisResult result)
        {
            await context.PostAsync("Tu amigo bot se despide");
            await Task.Delay(2000);
        }
        [LuisIntent("Notariar")]
        public async Task notaria(IDialogContext context, LuisResult result)
        {
            await context.PostAsync("Tu amigo bot te puede proporcionar el siguiente enlace de DIRNOPLU");
            await Task.Delay(2000);
            //invocando tarjeta de notario
            var reply = context.MakeMessage();
            reply.Attachments.Add(getImageCardNotario());
            await context.PostAsync(reply);
            await Task.Delay(5000);
            //invocando botones de continuar preguntando o despedirse
            mostrarBotonesContinuarDespedida(context);
        }
        [LuisIntent("NombreIncompleto")]
        public async Task incompleto(IDialogContext context, LuisResult result)
        {
            await context.PostAsync("Pedir modificación de datos mediante regularización de propietario.");
            await Task.Delay(2000);
            mostrarBotonesContinuarDespedida(context);
        }
        //INTENCIONES PARA INDAGACION
        [LuisIntent("Zonas")]
        public async Task zona(IDialogContext context, LuisResult result)
        {
            await context.PostAsync("Tu amigo bot te puede proporcionar el siguiente enlace de zonas de riesgo en La Paz");
            await Task.Delay(2000);
            //invocando tarjetas de zonas
            var reply = context.MakeMessage();
            reply.Attachments.Add(getImageCardZonas());
            await context.PostAsync(reply);
            await Task.Delay(5000);
            //invocando botones de continuar preguntando o despedirse
            mostrarBotonesContinuarDespedida(context);
        }
        [LuisIntent("BuscarAbogado")]
        public async Task buscarAbogado(IDialogContext context, LuisResult result)
        {
            await context.PostAsync("Tu amigo bot te puede proporcionar el siguiente enlace para que puedas buscar un abogado");
            await Task.Delay(2000);
            //invocando tarjeta de abogado
            var reply = context.MakeMessage();
            reply.Attachments.Add(getImageCardRPA());
            await context.PostAsync(reply);
            await Task.Delay(5000);
            //invocando botones de continuar preguntando o despedirse
            mostrarBotonesContinuarDespedida(context);
        }
        [LuisIntent("Legitimar")]
        public async Task legitima(IDialogContext context, LuisResult result)
        {
            await Task.Delay(2000);
            //invocando tarjeta de indagacion de propietario legitimo
            var reply = context.MakeMessage();
            reply.Attachments.Add(getImageIndaLeg());
            await context.PostAsync(reply);
            await Task.Delay(5000);
            //invocando botones de continuar preguntando o despedirse
            mostrarBotonesContinuarDespedida(context);
        }

        [LuisIntent("Deudas")]
        public async Task deuda(IDialogContext context, LuisResult result)
        {
            await context.PostAsync("Tu puedes verificar que el Folio Real este libre de gravámenes");
            await Task.Delay(2000);
            //invocando tarjeta de folio libre de gravámenes
            var reply = context.MakeMessage();
            reply.Attachments.Add(getImageCardFolio());
            await context.PostAsync(reply);
            await Task.Delay(5000);
            //invocando botones de continuar preguntando o despedirse
            mostrarBotonesContinuarDespedida(context);
        }
        [LuisIntent("Casado")]
        public async Task casado(IDialogContext context, LuisResult result)
        {
            var cad = result.Query;
            cad = cad.ToLower();
            if (cad.Equals("esta casada") || cad.Equals("persona casada"))
            {
                await context.PostAsync("Deben celebrar el contrato ambas partes.");
            }
            else
                await context.PostAsync("Tu puedes pedir un certificado al propietario sobre su estado civil");
            await Task.Delay(2000);
            //invocando botones de continuar preguntando o despedirse
            mostrarBotonesContinuarDespedida(context);
        }
        [LuisIntent("Teoria")]
        public async Task teoria(IDialogContext context, LuisResult result)
        {
            await context.PostAsync("Más información de conceptos teóricos aquí");
            await Task.Delay(2000);
            //invocando tarjeta de teoria
            var reply = context.MakeMessage();
            reply.Attachments.Add(getImageCardTeoria());
            await context.PostAsync(reply);
            await Task.Delay(5000);
            //invocando botones de continuar preguntando o despedirse
            mostrarBotonesContinuarDespedida(context);
        }
        [LuisIntent("Institucion")]
        public async Task institucion(IDialogContext context, LuisResult result)
        {
            await context.PostAsync("Te puedo proporcionar las siguientes instituciones que puedes visitar");
            await Task.Delay(2000);
            //invocando tarjeta de teoria
            var reply = context.MakeMessage();
            reply.Attachments.Add(getImageCarDDRR());
            await context.PostAsync(reply);
            await Task.Delay(3000);
            reply = context.MakeMessage();
            reply.Attachments.Add(getImageCardNotario());
            await context.PostAsync(reply);
            await Task.Delay(3000);
            reply = context.MakeMessage();
            reply.Attachments.Add(getImageCardRuat());
            await context.PostAsync(reply);
            await Task.Delay(3000);
            reply = context.MakeMessage();
            reply.Attachments.Add(getImageCardRPA());
            await context.PostAsync(reply);
            await Task.Delay(3000);
            //invocando botones de continuar preguntando o despedirse
            mostrarBotonesContinuarDespedida(context);
        }
        //INTENCIONES PARA TRAMITE CV
        [LuisIntent("Indagar")]
        public async Task indagacion(IDialogContext context, LuisResult result)
        {
            await Task.Delay(2000);
            PromptDialog.Choice(context,
                                SelectionIndagacion,
                                new[] { "chequear superficies zonas", "contratar abogado", "Es propietario legitimo", "Propiedad no tiene deudas", "Propietario Casado" },
                                "¿Qué deseas indagar?",
                                "Elige una opción", 3);
        }
        [LuisIntent("Contrato")]
        public async Task contrato_minuta(IDialogContext context, LuisResult result)
        {
            await Task.Delay(2000);
            PromptDialog.Choice(context,
                         SelectionContrato, new[] { req, proc, quien, costo, tiempo, esCasada, nombreIncompleto },
                         "¿Qué deseas saber respecto al contrato-minuta?",
                         "Elige una opción", 3);
        }
        [LuisIntent("Protocolización")]
        public async Task protocolizacion(IDialogContext context, LuisResult result)
        {
            await Task.Delay(2000);
            PromptDialog.Choice(context,
                         SelectionProtocol,
                         new[] { req, proc, quien, costo, tiempo },
                         "¿Qué deseas saber respecto a la protocolización?",
                         "Elige una opción", 3);
        }
        [LuisIntent("PagoAlcaldia")]
        public async Task pagoAlcaldia(IDialogContext context, LuisResult result)
        {
            await Task.Delay(2000);
            PromptDialog.Choice(context,
                                SelectionPagoAlcaldia,
                                new[] { req, proc, quien, costo, tiempo },
                                "¿Qué deseas saber respecto al pago de impuestos en la alcaldía?",
                                "Elige una opción", 3);
        }
        [LuisIntent("RegistroDerecho")]
        public async Task registroDerecho(IDialogContext context, LuisResult result)
        {
            await Task.Delay(2000);
            PromptDialog.Choice(context,
                                SelectionRegistro,
                                new[] { req, proc, quien, costo, tiempo },
                                "¿Qué deseas saber respecto registro de derecho de propietario?",
                                "Elige una opción", 3);
        }
        //INTENCIONES SELECCION
        [LuisIntent("Requisitos")]
        public async Task requisitos(IDialogContext context, LuisResult result)
        {
            var cadena = result.Query;
            bool lcSwCm = swCm, lcSwPr = swPr, lcSwAl = swAl, lcSwRd = swRd;
            await Task.Delay(2000);
            cadena = cadena.ToLower();
            bool sw = false;
            if (cadena.Equals("requerimiento minuta") || (cadena.IndexOf("requisitos") != -1 && (cadena.IndexOf("minuta") != -1 || cadena.IndexOf("contrato") != -1)) || (cadena.Equals("requisitos") && lcSwCm))
            {
                var reply = context.MakeMessage();
                reply.Attachments.Add(getImageMinutaReq());
                await context.PostAsync(reply);
                await Task.Delay(3000);
                sw = true;
            }
            if (cadena.Equals("requerimiento protocolizacion") || (cadena.IndexOf("requisitos") != -1 && (cadena.IndexOf("protocolización") != -1 || cadena.IndexOf("protocolizacion") != -1)) || (cadena.Equals("requisitos") && lcSwPr))
            {
                var reply = context.MakeMessage();
                reply.Attachments.Add(getImageProtoReq());
                await context.PostAsync(reply);
                await Task.Delay(3000);
                sw = true;
            }
            if (cadena.Equals("requerimiento alcaldia") || (cadena.IndexOf("requisitos") != -1 && (cadena.IndexOf("alcaldia") != -1 || cadena.IndexOf("alcaldía") != -1)) || (cadena.Equals("requisitos") && lcSwAl))
            {
                var reply = context.MakeMessage();
                reply.Attachments.Add(getImageAlcadiaReq());
                await context.PostAsync(reply);
                await Task.Delay(3000);
                sw = true;
            }
            if (cadena.Equals("requerimiento derecho") || (cadena.IndexOf("requisitos") != -1 && (cadena.IndexOf("derecho") != -1)) || (cadena.Equals("requisitos") && lcSwRd))
            {
                var reply = context.MakeMessage();
                reply.Attachments.Add(getImageDerechoReq());
                await context.PostAsync(reply);
                await Task.Delay(3000);
                sw = true;
            }
            if (!sw)
            {
                await context.PostAsync("Vuelve a redactar mejor tu pregunta (req)");
            }
            mostrarBotonesContinuarDespedida(context);
        }
        [LuisIntent("Procedimiento")]
        public async Task procedimiento(IDialogContext context, LuisResult result)
        {
            var cadena = result.Query;
            bool lcSwCm = swCm, lcSwPr = swPr, lcSwAl = swAl, lcSwRd = swRd;
            await Task.Delay(2000);
            cadena = cadena.ToLower();
            bool sw = false;
            if (cadena.Equals("procedimiento minuta") || (cadena.IndexOf("procedimiento") != -1 && (cadena.IndexOf("minuta") != -1 || cadena.IndexOf("contrato") != -1)) || (cadena.Equals("procedimiento") && lcSwCm))
            {
                var reply = context.MakeMessage();
                reply.Attachments.Add(getImageMinutaProc());
                await context.PostAsync(reply);
                await Task.Delay(3000);
                sw = true;
            }
            if (cadena.Equals("procedimiento protocolizacion") || (cadena.IndexOf("procedimiento") != -1 && (cadena.IndexOf("protocolización") != -1 || cadena.IndexOf("protocolizacion") != -1)) || (cadena.Equals("procedimiento") && lcSwPr))
            {
                var reply = context.MakeMessage();
                reply.Attachments.Add(getImageProtoProc());
                await context.PostAsync(reply);
                await Task.Delay(3000); await context.PostAsync("Hacer algo con procedimiento de protocolizacion");
                sw = true;
            }
            if (cadena.Equals("procedimiento alcaldia") || (cadena.IndexOf("procedimiento") != -1 && (cadena.IndexOf("alcaldia") != -1 || cadena.IndexOf("alcaldía") != -1)) || (cadena.Equals("procedimiento") && lcSwAl))
            {
                var reply = context.MakeMessage();
                reply.Attachments.Add(getImageAlcaldiaProc());
                await context.PostAsync(reply);
                await Task.Delay(3000);
                sw = true;
            }
            if (cadena.Equals("procedimiento derecho") || (cadena.IndexOf("procedimiento") != -1 && (cadena.IndexOf("derecho") != -1)) || (cadena.Equals("procedimiento") && lcSwRd))
            {
                var reply = context.MakeMessage();
                reply.Attachments.Add(getImageDerechoProc());
                await context.PostAsync(reply);
                await Task.Delay(3000);
                sw = true;
            }
            if (!sw)
            {
                await context.PostAsync("Vuelve a redactar mejor tu pregunta (proc)");
            }
            mostrarBotonesContinuarDespedida(context);
        }
        [LuisIntent("Quien")]
        public async Task quie(IDialogContext context, LuisResult result)
        {
            var cadena = result.Query;
            bool lcSwCm = swCm, lcSwPr = swPr, lcSwAl = swAl, lcSwRd = swRd;
            await Task.Delay(2000);
            cadena = cadena.ToLower();
            bool sw = false;
            if (cadena.Equals("quien minuta") || ((cadena.IndexOf("quien") != -1 || cadena.IndexOf("quienes") != -1) && (cadena.IndexOf("minuta") != -1 || cadena.IndexOf("contrato") != -1)) || (cadena.Equals("¿quien solicita?") && lcSwCm))
            {
                var reply = context.MakeMessage();
                reply.Attachments.Add(getImageMinutaQuien());
                await context.PostAsync(reply);
                await Task.Delay(3000);
                sw = true;
            }
            if (cadena.Equals("quien protocolizacion") || ((cadena.IndexOf("quien") != -1 || cadena.IndexOf("quienes") != -1) && (cadena.IndexOf("protocolización") != -1 || cadena.IndexOf("protocolizacion") != -1)) || (cadena.Equals("¿quien solicita?") && lcSwPr))
            {
                var reply = context.MakeMessage();
                reply.Attachments.Add(getImageProtoQuien());
                await context.PostAsync(reply);
                await Task.Delay(3000);
                sw = true;
            }
            if (cadena.Equals("quien alcaldia") || ((cadena.IndexOf("quien") != -1 || cadena.IndexOf("quienes") != -1) && (cadena.IndexOf("alcaldia") != -1 || cadena.IndexOf("alcaldía") != -1)) || (cadena.Equals("¿quien solicita?") && lcSwAl))
            {
                var reply = context.MakeMessage();
                reply.Attachments.Add(getImageAlcaldiaQuien());
                await context.PostAsync(reply);
                await Task.Delay(3000);
                sw = true;
            }
            if (cadena.Equals("quien derecho") || ((cadena.IndexOf("quien") != -1 || cadena.IndexOf("quienes") != -1) && (cadena.IndexOf("derecho") != -1)) || (cadena.Equals("¿quien solicita?") && lcSwRd))
            {
                var reply = context.MakeMessage();
                reply.Attachments.Add(getImageDerechoQuien());
                await context.PostAsync(reply);
                await Task.Delay(3000);
                sw = true;
            }
            if (!sw)
            {
                await context.PostAsync("Vuelve a redactar mejor tu pregunta (quien)");
            }
            mostrarBotonesContinuarDespedida(context);
        }
        [LuisIntent("Costo")]
        public async Task costos(IDialogContext context, LuisResult result)
        {
            var cadena = result.Query;
            bool lcSwCm = swCm, lcSwPr = swPr, lcSwAl = swAl, lcSwRd = swRd;
            await Task.Delay(2000);
            cadena = cadena.ToLower();
            bool sw = false;
            if (cadena.Equals("costo minuta") || (cadena.IndexOf("costo") != -1 && (cadena.IndexOf("minuta") != -1 || cadena.IndexOf("contrato") != -1)) || (cadena.Equals("costo") && lcSwCm))
            {
                var reply = context.MakeMessage();
                reply.Attachments.Add(getImageMinutaCosto());
                await context.PostAsync(reply);
                await Task.Delay(3000);
                sw = true;
            }
            if (cadena.Equals("costo protocolizacion") || ((cadena.IndexOf("costo") != -1 && (cadena.IndexOf("protocolización") != -1 || cadena.IndexOf("protocolizacion") != -1)) || (cadena.Equals("costo") && lcSwPr)))
            {
                var reply = context.MakeMessage();
                reply.Attachments.Add(getImageProtoCosto());
                await context.PostAsync(reply);
                await Task.Delay(3000);
                sw = true;
            }
            if (cadena.Equals("costo alcaldia") || (cadena.IndexOf("costo") != -1 && (cadena.IndexOf("alcaldia") != -1 || cadena.IndexOf("alcaldía") != -1)) || (cadena.Equals("costo") && lcSwAl))
            {
                var reply = context.MakeMessage();
                reply.Attachments.Add(getImageAlcaldiaCosto());
                await context.PostAsync(reply);
                await Task.Delay(3000);
                sw = true;
            }
            if (cadena.Equals("costo derecho") || (cadena.IndexOf("costo") != -1 && (cadena.IndexOf("derecho") != -1)) || (cadena.Equals("costo") && lcSwRd))
            {
                var reply = context.MakeMessage();
                reply.Attachments.Add(getImageDerechoCosto());
                await context.PostAsync(reply);
                await Task.Delay(3000);
                sw = true;
            }
            if (!sw)
            {
                await context.PostAsync("Vuelve a redactar mejor tu pregunta (costo)");
            }
            mostrarBotonesContinuarDespedida(context);
        }
        [LuisIntent("Duracion")]
        public async Task duracion(IDialogContext context, LuisResult result)
        {
            var cadena = result.Query;
            bool lcSwCm = swCm, lcSwPr = swPr, lcSwAl = swAl, lcSwRd = swRd;
            await Task.Delay(2000);
            cadena = cadena.ToLower();
            bool sw = false;
            if (cadena.Equals("tiempo minuta") || ((cadena.IndexOf("tiempo") != -1 || cadena.IndexOf("duracion") != -1 || cadena.IndexOf("duración") != -1) && (cadena.IndexOf("minuta") != -1 || cadena.IndexOf("contrato") != -1)) || (cadena.Equals("duración") && lcSwCm))
            {
                var reply = context.MakeMessage();
                reply.Attachments.Add(getImageMinutaDuracion());
                await context.PostAsync(reply);
                await Task.Delay(3000);
                sw = true;
            }
            if (cadena.Equals("tiempo protocolizacion") || ((cadena.IndexOf("tiempo") != -1 || cadena.IndexOf("duracion") != -1 || cadena.IndexOf("duración") != -1) && (cadena.IndexOf("protocolización") != -1 || cadena.IndexOf("protocolizacion") != -1)) || (cadena.Equals("duración") && lcSwPr))
            {
                var reply = context.MakeMessage();
                reply.Attachments.Add(getImageProtoDuracion());
                await context.PostAsync(reply);
                await Task.Delay(3000);
                sw = true;
            }
            if (cadena.Equals("tiempo alcaldia") || ((cadena.IndexOf("tiempo") != -1 || cadena.IndexOf("duracion") != -1 || cadena.IndexOf("duración") != -1) && (cadena.IndexOf("alcaldia") != -1 || cadena.IndexOf("alcaldía") != -1)) || (cadena.Equals("duración") && lcSwAl))
            {
                var reply = context.MakeMessage();
                reply.Attachments.Add(getImageAlcadiaDuracion());
                await context.PostAsync(reply);
                await Task.Delay(3000);
                sw = true;
            }
            if (cadena.Equals("tiempo derecho") || ((cadena.IndexOf("tiempo") != -1 || cadena.IndexOf("duracion") != -1 || cadena.IndexOf("duración") != -1) && (cadena.IndexOf("derecho") != -1)) || (cadena.Equals("duración") && lcSwRd))
            {
                var reply = context.MakeMessage();
                reply.Attachments.Add(getImageDerechoDuracion());
                await context.PostAsync(reply);
                await Task.Delay(3000);
                sw = true;
            }
            if (!sw)
            {
                await context.PostAsync("Vuelve a redactar mejor tu pregunta (dur)");
            }
            mostrarBotonesContinuarDespedida(context);
        }
        //INTENCIONES PREGUNTAS
        [LuisIntent("PreguntasProfundizacion")]
        public async Task profundizacion(IDialogContext context, LuisResult result)
        {
            await context.PostAsync("Entiendo tu pregunta, como una pregunta de profundizacion");
            await Task.Delay(2000);
        }
        [LuisIntent("PreguntasNormativa")]
        public async Task normativa(IDialogContext context, LuisResult result)
        {
            await context.PostAsync("Puedes consultar más información de la normativa vigente de compra venta en el siguiente enlace");
            await Task.Delay(2000);
            //invocando tarjeta de normativa
            var reply = context.MakeMessage();
            reply.Attachments.Add(getImageCardNormativa());
            await context.PostAsync(reply);
            await Task.Delay(5000);
            //invocando botones de continuar preguntando o despedirse
            mostrarBotonesContinuarDespedida(context);
        }
        [LuisIntent("PreguntasOpcion")]
        public async Task opcion(IDialogContext context, LuisResult result)
        {
            var cadena = result.Query;
            await Task.Delay(2000);
            if (cadena.Equals(saberCv))
                PromptDialog.Choice(context,
                             SeleccionPreguntasSaber,
                             new[] { "Normativa", "Teoría", "Concepto compraventa", "Institución", "Trámite compraventa" },
                             "¿Qué deseas saber sobre la compra y venta de inmuebles?",
                              "opcion no valida", 3);
            else
                PromptDialog.Choice(context,
                                    SeleccionPreguntasTramite,
                                    new[] { "Indagación", "Contrato-Minuta", "Protocolización", "Pago Impuestos a la Alcaldia", "Registro de Derecho de Propietario" },
                                    "¿Qué deseas saber sobre el trámite?",
                                    "opcion no valida", 3);
        }
        [LuisIntent("PreguntasAclaratorias")]
        public async Task aclaratorias(IDialogContext context, LuisResult result)
        {
            var cadena = result.Query;
            cadena = cadena.ToLower();
            //await context.PostAsync(cadena);
            string cadenaRes = Clases.PreguntasAclaratorias.devuelveConcepto(cadena);
            if (!cadenaRes.Equals(""))
            {
                await Task.Delay(2000);
                await context.PostAsync(cadenaRes);
                if (cadena.IndexOf("folio real") != -1)
                {
                    var reply = context.MakeMessage();
                    reply.Attachments.Add(getImageCardFolioReal());
                    await context.PostAsync(reply);
                }
                if (cadena.IndexOf("catastro") != -1 || cadena.IndexOf("certificado catastral") != -1)
                {
                    var reply = context.MakeMessage();
                    reply.Attachments.Add(getImageCardCatastro());
                    await context.PostAsync(reply);
                }
                await Task.Delay(5000);
                mostrarBotonesContinuarDespedida(context);
            }
            else
                await context.PostAsync("concepto no hallado");
        }
        //----------------------------------FIN DE INTENCIONES-----------------------------------------------------------------------
        //tarjetas enriquecidas
        private Attachment getImageCardNormativa()
        {
            var imageCard = new HeroCard
            {
                Images = new List<CardImage>{
                    new CardImage(){
                        Url ="http://www3.gobiernodecanarias.org/medusa/ecoblog/mravsane/files/2013/05/libros1.jpg"
                    }
                },
                Buttons = new List<CardAction>{
                    new CardAction(ActionTypes.OpenUrl,title:"Ir al sitio",
                    value:"https://drive.google.com/drive/folders/1JGFMiWziYTWRCmZhgcXPdiZN_cHmN0XE?usp=sharing")
             }
            };
            return imageCard.ToAttachment();
        }
        private Attachment getImageCardCatastro()
        {
            var imageCard = new HeroCard
            {
                Title = "Modelo Certificado catastral",
                Images = new List<CardImage>{
                    new CardImage(){
                        Url ="https://images.slideplayer.es/1/92923/slides/slide_6.jpg"
                    }
                },
                Buttons = new List<CardAction>{
                    new CardAction(ActionTypes.OpenUrl,title:"Mira la foto en mayor tamaño",value:"https://images.slideplayer.es/1/92923/slides/slide_6.jpg")
             }
            };
            return imageCard.ToAttachment();
        }
        private Attachment getImageCardFolioReal()
        {
            var imageCard = new HeroCard
            {
                Title = "Modelo Folio Real",
                Images = new List<CardImage>{
                    new CardImage(){
                        Url ="https://lh3.googleusercontent.com/-5nyyNOLYq7I/We0oPFJus2I/AAAAAAAAO0k/wTzJiTRxcxM8DM7Vrp7R7Jlhh_uk1J-bgCHMYCw/folio-real-bolivia-informa-2017-reyqui%255B7%255D?imgmax=800"
                    }
                },
                Buttons = new List<CardAction>{
                    new CardAction(ActionTypes.OpenUrl,title:"Mira la foto en mayor tamaño",value:"https://lh3.googleusercontent.com/-5nyyNOLYq7I/We0oPFJus2I/AAAAAAAAO0k/wTzJiTRxcxM8DM7Vrp7R7Jlhh_uk1J-bgCHMYCw/folio-real-bolivia-informa-2017-reyqui%255B7%255D?imgmax=800")
             }
            };
            return imageCard.ToAttachment();
        }
        private Attachment getImageCardNotario()
        {
            var imageCard = new HeroCard
            {
                Title = "DIRNOPLU",
                Subtitle = "Dirección del notario plurinacional",
                Images = new List<CardImage>{
                    new CardImage(){
                        Url ="https://www.notariadoplurinacional.gob.bo/wp-content/uploads/2018/05/Logo-finalRECTANGULAR2-1024x305.png"
                    }
             },
                Buttons = new List<CardAction>{
                    new CardAction(ActionTypes.OpenUrl,title:"Ir al sitio",value:"https://www.notariadoplurinacional.gob.bo/index.php/servnotariales/")
                }
            };
            return imageCard.ToAttachment();
        }
        private Attachment getImageCardRPA()
        {
            var imageCard = new HeroCard
            {
                Title = "R.P.A",
                Subtitle = "Registro público de la abogacía",
                Images = new List<CardImage>{
                    new CardImage(){
                        Url = "http://rpa.justicia.gob.bo/rpa/app/images/images/Diapositiva1(1).PNG"
                    }
                },
                Buttons = new List<CardAction>{
                    new CardAction(ActionTypes.OpenUrl,title:"Ir al sitio",value:"https://rpa2.justicia.gob.bo/#/Busqueda")
                }
            };
            return imageCard.ToAttachment();
        }
        private Attachment getImageCardRuat()
        {
            var imageCard = new HeroCard
            {
                Title = "R.U.A.T",
                Subtitle = "Regístro Único para la Administración Tributaria Municipal",
                Images = new List<CardImage>{
                    new CardImage(){
                        Url = "https://www.ruat.gob.bo/resources/img/banner/banner_izq.png"
                    }
                },
                Buttons = new List<CardAction>{
                    new CardAction(ActionTypes.OpenUrl,title:"Ir al sitio",value:"https://www.ruat.gob.bo/Principal.jsf")
                }
            };
            return imageCard.ToAttachment();
        }
        private Attachment getImageCarDDRR()
        {
            var imageCard = new HeroCard
            {
                Title = "Derechos Reales",
                Images = new List<CardImage>{
                    new CardImage(){
                        Url = "http://magistratura.organojudicial.gob.bo/images/BannerBlancoOficial.jpg"
                    }
                },
                Buttons = new List<CardAction>{
                    new CardAction(ActionTypes.OpenUrl,title:"Ir al sitio",
                    value:"http://magistratura.organojudicial.gob.bo/index.php/2013-05-07-15-26-51/consultaddrr")
                }
            };
            return imageCard.ToAttachment();
        }
        private Attachment getImageCardZonas()
        {
            var imageCard = new HeroCard
            {
                Title = "Mapa de riesgos de la ciudad de La Paz",
                Images = new List<CardImage>{
                    new CardImage(){
                        Url ="http://sitservicios.lapaz.bo/sit/riesgos/images/logos_gamlp.jpg"
                    }
                },
                Buttons = new List<CardAction>{
                    new CardAction(ActionTypes.OpenUrl,title:"Ir al sitio",
                    value:"http://sitservicios.lapaz.bo/sit/riesgos/?fbclid=IwAR36Hl--rsn1JImAhpiCGjC6xgQYY5rqnpnRbLYd6ZZRUlF65tAmhshPeMI")
                }
            };
            return imageCard.ToAttachment();
        }
        private Attachment getImageCardCasado()
        {
            var imageCard = new HeroCard
            {
                Title = "Ejemplo Certificado de Matrimonio",
                Images = new List<CardImage>{
                    new CardImage(){
                        Url = "https://1.bp.blogspot.com/-0j_a5gDtvxA/VsFVi_cEXjI/AAAAAAAALqU/-vFQT71bahA/s1600/Encuentran%2Bcertificado%2Bde%2Bmatrimonio%2Bde%2BGabriela%2BZapata.jpg"
                    }
                },
                Buttons = new List<CardAction>{
                    new CardAction(ActionTypes.OpenUrl,title:"Mirar la foto en mayor tamaño",value:"https://1.bp.blogspot.com/-0j_a5gDtvxA/VsFVi_cEXjI/AAAAAAAALqU/-vFQT71bahA/s1600/Encuentran%2Bcertificado%2Bde%2Bmatrimonio%2Bde%2BGabriela%2BZapata.jpg")
                }
            };
            return imageCard.ToAttachment();
        }
        private Attachment getImageCardFolio()
        {
            var imageCard = new HeroCard
            {
                Title = "Folio Real",
                Images = new List<CardImage>{
                    new CardImage(){
                        Url = "https://lh3.googleusercontent.com/-5nyyNOLYq7I/We0oPFJus2I/AAAAAAAAO0k/wTzJiTRxcxM8DM7Vrp7R7Jlhh_uk1J-bgCHMYCw/folio-real-bolivia-informa-2017-reyqui%255B7%255D?imgmax=800"
                    }
                },
                Buttons = new List<CardAction>{
                    new CardAction(ActionTypes.OpenUrl,title:"Más información respecto al gravamen del folio aqui",
                    value:"https://drive.google.com/drive/folders/12Vj2BQUsCK3rSjqDgzvZhl6zI56pBNk0?usp=sharing")
                }
            };
            return imageCard.ToAttachment();
        }
        private Attachment getImageCardTeoria()
        {
            var imageCard = new HeroCard
            {
                Title = "Teoria",
                Images = new List<CardImage>{
                    new CardImage(){
                        Url = "https://4.bp.blogspot.com/-rKvU1cIf20U/U91XTZNoS9I/AAAAAAAAKKA/su4jqwoMK4g/s1600/Teoria-derecho.jpg"
                    }
                },
                Buttons = new List<CardAction>{
                    new CardAction(ActionTypes.OpenUrl,title:"Ir sitio",
                    value:"https://drive.google.com/drive/folders/1_Fx2LwZA8e0nB15UCoOdkVSGmNpUZVCG?usp=sharing")
                }
            };
            return imageCard.ToAttachment();
        }
        //tarjetas imagenes solamente
        private Attachment getImageIndaLeg()
        {
            var imageCard = new HeroCard
            {
                Images = new List<CardImage>{
                    new CardImage(){
                        Url = "https://i.ibb.co/922ztry/indagacion-img1.png"
                    }
                }
            };
            return imageCard.ToAttachment();
        }
        private Attachment getImageMinutaReq()
        {
            var imageCard = new HeroCard
            {
                Images = new List<CardImage>{
                    new CardImage(){
                        Url = "https://i.ibb.co/HznxXpj/minuta-img1.png"
                    }
                }
            };
            return imageCard.ToAttachment();
        }
        private Attachment getImageProtoReq()
        {
            var imageCard = new HeroCard
            {
                Images = new List<CardImage>{
                    new CardImage(){
                        Url = "https://i.ibb.co/rxBzgkj/proto-img1.png"
                    }
                }
            };
            return imageCard.ToAttachment();
        }
        private Attachment getImageAlcadiaReq()
        {
            var imageCard = new HeroCard
            {
                Images = new List<CardImage>{
                    new CardImage(){
                        Url = "https://i.ibb.co/cymmDX4/alcaldia-img1.png"
                    }
                }
            };
            return imageCard.ToAttachment();
        }
        private Attachment getImageDerechoReq()
        {
            var imageCard = new HeroCard
            {
                Images = new List<CardImage>{
                    new CardImage(){
                        Url = "https://i.ibb.co/sPfLFHf/derecho-img1.png"
                    }
                }
            };
            return imageCard.ToAttachment();
        }
        private Attachment getImageMinutaProc()
        {
            var imageCard = new HeroCard
            {
                Images = new List<CardImage>{
                    new CardImage(){
                        Url = "https://i.ibb.co/1sg0cJQ/minuta-img2.png"
                    }
                }
            };
            return imageCard.ToAttachment();
        }
        private Attachment getImageProtoProc()
        {
            var imageCard = new HeroCard
            {
                Images = new List<CardImage>{
                    new CardImage(){
                        Url = "https://i.ibb.co/hfV1n95/proto-img2.png"
                    }
                }
            };
            return imageCard.ToAttachment();
        }
        private Attachment getImageAlcaldiaProc()
        {
            var imageCard = new HeroCard
            {
                Images = new List<CardImage>{
                    new CardImage(){
                        Url = "https://i.ibb.co/BzyP3Pc/alcaldia-img2.png"
                    }
                }
            };
            return imageCard.ToAttachment();
        }
        private Attachment getImageDerechoProc()
        {
            var imageCard = new HeroCard
            {
                Images = new List<CardImage>{
                    new CardImage(){
                        Url = "https://i.ibb.co/BCg40hG/derecho-img2.png"
                    }
                }
            };
            return imageCard.ToAttachment();
        }
        private Attachment getImageMinutaQuien()
        {
            var imageCard = new HeroCard
            {
                Images = new List<CardImage>{
                    new CardImage(){
                        Url = "https://i.ibb.co/RbzZz3c/minuta-img3.png"
                    }
                }
            };
            return imageCard.ToAttachment();
        }
        private Attachment getImageProtoQuien()
        {
            var imageCard = new HeroCard
            {
                Images = new List<CardImage>{
                    new CardImage(){
                        Url = "https://i.ibb.co/T1qNB3B/proto-img3.png"
                    }
                }
            };
            return imageCard.ToAttachment();

        }
        private Attachment getImageAlcaldiaQuien()
        {
            var imageCard = new HeroCard
            {
                Images = new List<CardImage>{
                    new CardImage(){
                        Url = "https://i.ibb.co/P4YbYJh/alcaldia-img3.png"
                    }
                }
            };
            return imageCard.ToAttachment();
        }
        private Attachment getImageDerechoQuien()
        {
            var imageCard = new HeroCard
            {
                Images = new List<CardImage>{
                    new CardImage(){
                        Url = "https://i.ibb.co/Lzb74rY/derecho-img3.png"
                    }
                }
            };
            return imageCard.ToAttachment();
        }

        private Attachment getImageMinutaCosto()
        {
            var imageCard = new HeroCard
            {
                Images = new List<CardImage>{
                    new CardImage(){
                        Url = "https://i.ibb.co/TLWx2hw/minuta-img4.png"
                    }
                }
            };
            return imageCard.ToAttachment();
        }
        private Attachment getImageProtoCosto()
        {
            var imageCard = new HeroCard
            {
                Images = new List<CardImage>{
                    new CardImage(){
                        Url = "https://i.ibb.co/2MVk6Fz/proto-img4.png"
                    }
                }
            };
            return imageCard.ToAttachment();
        }
        private Attachment getImageAlcaldiaCosto()
        {
            var imageCard = new HeroCard
            {
                Images = new List<CardImage>{
                    new CardImage(){
                        Url = "https://i.ibb.co/dtsWgxQ/alcaldia-img4.png"
                    }
                }
            };
            return imageCard.ToAttachment();

        }
        private Attachment getImageDerechoCosto()
        {
            var imageCard = new HeroCard
            {
                Images = new List<CardImage>{
                    new CardImage(){
                        Url = "https://i.ibb.co/Z2jxGG9/derecho-img4.png"
                    }
                }
            };
            return imageCard.ToAttachment();
        }
        private Attachment getImageMinutaDuracion()
        {
            var imageCard = new HeroCard
            {
                Images = new List<CardImage>{
                    new CardImage(){
                        Url = "https://i.ibb.co/gRQDmW8/minuta-img5.png"
                    }
                }
            };
            return imageCard.ToAttachment();
        }
        private Attachment getImageProtoDuracion()
        {
            var imageCard = new HeroCard
            {
                Images = new List<CardImage>{
                    new CardImage(){
                        Url = "https://i.ibb.co/VwNnDZL/proto-img5.png"
                    }
                }
            };
            return imageCard.ToAttachment();
        }
        private Attachment getImageAlcadiaDuracion()
        {
            var imageCard = new HeroCard
            {
                Images = new List<CardImage>{
                    new CardImage(){
                        Url = "https://i.ibb.co/fnSYxDT/alcaldia-img5.png"
                    }
                }
            };
            return imageCard.ToAttachment();
        }
        private Attachment getImageDerechoDuracion()
        {
            var imageCard = new HeroCard
            {
                Images = new List<CardImage>{
                    new CardImage(){
                        Url = "https://i.ibb.co/BTPpkMB/derecho-img5.png"
                    }
                }
            };
            return imageCard.ToAttachment();

        }

        //https://i.ibb.co/HznxXpj/minuta-img1.png
        //Métodos de selección
        private async Task SeleccionPreguntasSaber(IDialogContext context, IAwaitable<string> result)
        {
            try
            {
                string selected = await result;
                Activity myActivity = new Activity();
                switch (selected)
                {
                    //Aca vienen las opciones desplegadas en los botones anteriores
                    case "Normativa":
                        myActivity.Text = "normativa";
                        break;
                    case "Teoría":
                        myActivity.Text = "Teoría";
                        break;
                    case "Concepto compraventa":
                        myActivity.Text = "que es compraventa";
                        break;
                    case "Institución":
                        myActivity.Text = "institucion";
                        break;
                    case "Trámite compraventa":
                        myActivity.Text = "trámite/pasos para compra venta";
                        break;
                    default:
                        break;
                }
                await MessageReceived(context, Awaitable.FromItem(myActivity));
            }
            catch (Exception e)
            {
                throw;
            }
        }
        private async Task SeleccionPreguntasTramite(IDialogContext context, IAwaitable<string> result)
        {
            try
            {
                string selected = await result;
                Activity myActivity = new Activity();
                switch (selected)
                {
                    case "Indagación":
                        myActivity.Text = "Indagación";
                        break;
                    case "Contrato-Minuta":
                        myActivity.Text = "Contrato-Minuta";
                        break;
                    case "Protocolización":
                        myActivity.Text = "protocolizacion";
                        break;
                    case "Pago Impuestos a la Alcaldia":
                        myActivity.Text = "pago alcaldia";
                        break;
                    case "Registro de Derecho de Propietario":
                        myActivity.Text = "registro propietario";
                        break;
                    default:
                        break;
                }
                await MessageReceived(context, Awaitable.FromItem(myActivity));
            }
            catch (Exception e)
            {
                throw;
            }
        }
        private async Task SeleccionLegitimar(IDialogContext context, IAwaitable<string> result)
        {
            try
            {
                string selected = await result;
                Activity myActivity = new Activity();
                switch (selected)
                {
                    case "folio real":
                        myActivity.Text = "folio real";
                        break;
                    case "titulo de propiedad":
                        myActivity.Text = "titulo de propiedad";
                        break;
                    case "Ci. del o de los vendedores":
                        myActivity.Text = "ci";
                        break;
                    case "buscar abogado":
                        myActivity.Text = "buscar abogado";
                        break;
                    case "Certificado catastral":
                        myActivity.Text = "Certificado catastral";
                        break;
                    case "Impuestos de los dos ultimos años":
                        myActivity.Text = "impuestos";
                        break;
                    default:
                        break;
                }
                await MessageReceived(context, Awaitable.FromItem(myActivity));
            }
            catch (Exception e)
            {
                throw;
            }
        }
        //SELECCION TRAMITES
        private async Task SelectionIndagacion(IDialogContext context, IAwaitable<string> result)
        {
            try
            {
                var opcion = await result;
                Activity myActivity = new Activity();
                switch (opcion)
                {
                    case "chequear superficies zonas":
                        myActivity.Text = "chequear superficies zonas";
                        break;
                    case "contratar abogado":
                        myActivity.Text = "contratar abogado";
                        break;
                    case "Es propietario legitimo":
                        myActivity.Text = "Es propietario legitimo";
                        break;
                    case "Propiedad no tiene deudas":
                        myActivity.Text = "Propiedad no tiene deudas";
                        break;
                    case "Propietario Casado":
                        myActivity.Text = "Propietario Casado";
                        break;
                    default:
                        break;
                }
                await MessageReceived(context, Awaitable.FromItem(myActivity));
            }
            catch (Exception e)
            {
                throw;
            }
        }
        private async Task SelectionContrato(IDialogContext context, IAwaitable<string> result)
        {
            try
            {
                swCm = true;
                swPr = swAl = swRd = false;
                var opcion = await result;
                Activity myActivity = new Activity();
                switch (opcion)
                {
                    case req:
                        myActivity.Text = "requerimiento minuta";
                        break;
                    case proc:
                        myActivity.Text = "procedimiento minuta";
                        break;
                    case quien:
                        myActivity.Text = "quien minuta";
                        break;
                    case costo:
                        myActivity.Text = "costo minuta";
                        break;
                    case tiempo:
                        myActivity.Text = "tiempo minuta";
                        break;
                    case esCasada:
                        myActivity.Text = "persona casada";
                        break;
                    case nombreIncompleto:
                        myActivity.Text = "nombre incompleto";
                        break;
                    default:
                        break;
                }
                await MessageReceived(context, Awaitable.FromItem(myActivity));
            }
            catch (Exception e)
            {
                throw;
            }
        }
        private async Task SelectionProtocol(IDialogContext context, IAwaitable<string> result)
        {
            try
            {
                swPr = true;
                swCm = swAl = swRd = false;
                var opcion = await result;
                Activity myActivity = new Activity();
                switch (opcion)
                {
                    case req:
                        myActivity.Text = "requerimiento protocolizacion";
                        break;
                    case proc:
                        myActivity.Text = "procedimiento protocolizacion";
                        break;
                    case quien:
                        myActivity.Text = "quien protocolizacion";
                        break;
                    case costo:
                        myActivity.Text = "costo protocolizacion";
                        break;
                    case tiempo:
                        myActivity.Text = "tiempo protocolizacion";
                        break;
                    default:
                        break;
                }
                await MessageReceived(context, Awaitable.FromItem(myActivity));
            }
            catch (Exception e)
            {
                throw;
            }
        }
        private async Task SelectionPagoAlcaldia(IDialogContext context, IAwaitable<string> result)
        {
            try
            {
                swAl = true;
                swCm = swPr = swRd = false;
                var opcion = await result;
                Activity myActivity = new Activity();
                switch (opcion)
                {
                    case req:
                        myActivity.Text = "requerimiento alcaldia";
                        break;
                    case proc:
                        myActivity.Text = "procedimiento alcaldia";
                        break;
                    case quien:
                        myActivity.Text = "quien alcaldia";
                        break;
                    case costo:
                        myActivity.Text = "costo alcaldia";
                        break;
                    case tiempo:
                        myActivity.Text = "tiempo alcaldia";
                        break;
                    default:
                        break;
                }
                await MessageReceived(context, Awaitable.FromItem(myActivity));
            }
            catch (Exception e)
            {
                throw;
            }
        }
        private async Task SelectionRegistro(IDialogContext context, IAwaitable<string> result)
        {
            try
            {
                swRd = true;
                swCm = swPr = swAl = false;
                var opcion = await result;
                Activity myActivity = new Activity();
                switch (opcion)
                {
                    case req:
                        myActivity.Text = "requerimiento derecho";
                        break;
                    case proc:
                        myActivity.Text = "procedimiento derecho";
                        break;
                    case quien:
                        myActivity.Text = "quien derecho";
                        break;
                    case costo:
                        myActivity.Text = "costo derecho";
                        break;
                    case tiempo:
                        myActivity.Text = "tiempo derecho";
                        break;
                    default:
                        break;
                }
                await MessageReceived(context, Awaitable.FromItem(myActivity));
            }
            catch (Exception e)
            {
                throw;
            }
        }
        //SELECCION GENERAL
        private async Task Selection(IDialogContext context, IAwaitable<string> result)
        {
            var opcion = await result;
            switch (opcion)
            {
                case "continuar":
                    await context.PostAsync("Ok, puedes escribir una nueva pregunta o guiarte con los botones generados en el chat haciendo click en ellos");
                    break;
                case "despedida":
                    Activity myActivity = new Activity();
                    myActivity.Text = "despedida";
                    await MessageReceived(context, Awaitable.FromItem(myActivity));
                    break;
                default:
                    break;
            }
        }
        private async Task SeleccionInicial(IDialogContext context, IAwaitable<string> result)
        {
            try
            {
                var opcion = await result;
                Activity myActivity = new Activity();
                switch (opcion)
                {
                    case tramCv:
                        myActivity.Text = tramCv;
                        await MessageReceived(context, Awaitable.FromItem(myActivity));
                        break;
                    case saberCv:
                        myActivity.Text = saberCv;
                        await MessageReceived(context, Awaitable.FromItem(myActivity));
                        break;
                    case otraPreg:
                        await context.PostAsync("Ok ¿qué deseas preguntar?");
                        break;
                    default:
                        break;
                }
            }
            catch (Exception e)
            {
                throw;
            }
        }
        public void mostrarBotonesContinuarDespedida(IDialogContext context)
        {
            PromptDialog.Choice(context, Selection, new[] { "continuar", "despedida" }, "¿Qué deseas hacer?", "Elige una opción", promptStyle: PromptStyle.Keyboard);
        }

    }
}
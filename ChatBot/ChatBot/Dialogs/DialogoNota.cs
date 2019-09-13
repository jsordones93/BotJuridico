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
    [LuisModel(modelID: "94018890-6d35-4e39-a6cf-d46c82b79ae4", subscriptionKey: "d3d02d538bc749d0b5b60900ca419d5f")]
    [Serializable]
    public class DialogoNota : LuisDialog<object>
    {
        const string cheqSuper = "chequear superficies zonas", contraAbog = "contratar abogado", esPropLegit = "Es propietario legitimo", noDeudas = "Propiedad no tiene deudas", esCasado = "Propietario Casado", continuar = "continuar", despedida = "despedida";
        const string folioReal = "folio real", titProp = "título de propiedad", ciVend = "Ci. del o de los vendedores", buscaAbog = "buscar abogado", certfCatas = "Certificado catastral", impuestos = "Impuestos de los dos ultimos años";
        const string Indagacion = "Indagación";
        //-------------------I N T E N C I O N E S---------------------------
        [LuisIntent("None")]
        public async Task None(IDialogContext context, LuisResult result)
        {
            await context.PostAsync("Intenta con otro mensaje, por ejemplo saludame, di gracias, despidete o pide la referencia de un abogado");
            await Task.Delay(2000);
            await context.PostAsync("¿necesitas ayuda?");
        }
        [LuisIntent("Saludar")]
        public async Task Saluda(IDialogContext context, LuisResult result)
        {
            await context.PostAsync("Hola, soy tu amigo bot y puedo contestar preguntas de compra venta de inmuebles");
            await Task.Delay(2000);
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
            await context.PostAsync("Tu amigo bot te puede proporcionar el siguiente enlace de un notario");
            await Task.Delay(2000);
            //invocando tarjetas
            var reply = context.MakeMessage();
            reply.Attachments.Add(getImageCard1());
            await context.PostAsync(reply);
            PromptDialog.Choice(context, Selection, new[] { continuar, despedida }, "¿Qué deseas hacer?", "Elige una opción", promptStyle: PromptStyle.Keyboard);
        }
       
        //intenciones al presionar Indagación
        [LuisIntent("Zonas")]
        public async Task zona(IDialogContext context, LuisResult result)
        {
            await context.PostAsync("Tu amigo bot te puede proporcionar el siguiente enlace de zonas de riesgo en La Paz");
            await Task.Delay(2000);
            //invocando tarjetas
            var reply = context.MakeMessage();
            reply.Attachments.Add(getImageCard2());
            await context.PostAsync(reply);
            PromptDialog.Choice(context, Selection, new[] { continuar, despedida }, "¿Qué deseas hacer?", "Elige una opción", promptStyle: PromptStyle.Keyboard);
        }
        [LuisIntent("Contratar")]
        public async Task Contrata(IDialogContext context, LuisResult result)
        {
            await context.PostAsync("Tu amigo bot te puede proporcionar el siguiente enlace de un abogado");//rpa registro publico de abogados
            await Task.Delay(2000);
            var reply = context.MakeMessage();
            reply.Attachments.Add(getImageCard());
            await context.PostAsync(reply);
            PromptDialog.Choice(context, Selection, new[] { continuar, despedida }, "¿Qué deseas hacer?", "Elige una opción", promptStyle: PromptStyle.Keyboard);
        }
        [LuisIntent("Legitimar")]
        public async Task legitima(IDialogContext context, LuisResult result)
        {
            await Task.Delay(2000);
            PromptDialog.Choice(context, Selection, new[] { folioReal, titProp, ciVend, buscaAbog, certfCatas, impuestos }, "Tu puedes pedir cierta documentación al propietario", "Elige una opción", promptStyle: PromptStyle.Keyboard);//son botones o son texto?
        }
        [LuisIntent("Deudas")]
        public async Task deuda(IDialogContext context, LuisResult result)
        {
            await context.PostAsync("Tu puedes verificar que el Folio Real este libre de gravámenes");
            await Task.Delay(2000);
            PromptDialog.Choice(context, Selection, new[] { continuar, despedida }, "¿Qué deseas hacer?", "Elige una opción", promptStyle: PromptStyle.Keyboard);
        }
        [LuisIntent("Casado")]
        public async Task casado(IDialogContext context, LuisResult result)
        {
            await context.PostAsync("Tu puedes pedir un certificado al propietario sobre su estado civil");
            await Task.Delay(2000);
            PromptDialog.Choice(context, Selection, new[] { continuar, despedida }, "¿Qué deseas hacer?", "Elige una opción", promptStyle: PromptStyle.Keyboard);
        }
        //sector preguntas
        [LuisIntent("PreguntasProfundizacion")]
        public async Task profundizacion(IDialogContext context, LuisResult result)
        {
            await context.PostAsync("Entiendo tu pregunta, como una pregunta de profundizacion");
            await Task.Delay(2000);
        }
        [LuisIntent("PreguntasNormativa")]
        public async Task normativa(IDialogContext context, LuisResult result)
        {
            await context.PostAsync("Entiendo tu pregunta, como una pregunta sobre la normativa vigente");
            await Task.Delay(2000);
        }
        [LuisIntent("Indagación")]
        public async Task indagacion(IDialogContext context, LuisResult result)
        {
            PromptDialog.Choice(context, Selection2, new[] { cheqSuper, contraAbog, esPropLegit, noDeudas, esCasado }, "¿Qué deseas indagar?", "Elige una opción", promptStyle: PromptStyle.Keyboard);
            //PromptDialog.Choice(context, Selection, new[] { cheqSuper, contraAbog, esPropLegit, noDeudas, esCasado }, "¿Qué deseas indagar?", "Elige una opción", promptStyle: PromptStyle.Keyboard);
        }
        [LuisIntent("PreguntasOpcion")]
        public async Task opcion(IDialogContext context, LuisResult result)
        {
            var cadena = result.Query;
            await context.PostAsync(cadena);
            string cadenaRes = Clases.PreguntasO.devuelveBotonesO(cadena);
            if (!cadenaRes.Equals(""))
            {
                List<string> listaBotonesCad = new List<string>();
                string btn = "";
                for (int i = 0; i < cadenaRes.Length; i++)
                {
                    if (cadenaRes[i] != ' ' || i == cadenaRes.Length - 1)
                    {
                        btn = btn + cadenaRes[i];
                        if (i == cadenaRes.Length - 1)
                            listaBotonesCad.Add(btn);
                    }
                    else
                    {
                        listaBotonesCad.Add(btn);
                        btn = "";
                    }

                }
                await Task.Delay(2000);
                PromptDialog.Choice(context, Selection, listaBotonesCad, "¿Qué deseas hacer?", "Elige una opción", promptStyle: PromptStyle.Keyboard);
            }
            else
                await context.PostAsync("Vuelve a preguntar...");
        }
        [LuisIntent("PreguntasAclaratorias")]
        public async Task aclaratorias(IDialogContext context, LuisResult result)
        {
            var cadena = result.Query;
            await context.PostAsync(cadena);
            string cadenaRes = Clases.PreguntasAclaratorias.devuelveConcepto(cadena);
            await context.PostAsync(cadenaRes);
            await Task.Delay(2000);
            PromptDialog.Choice(context, Selection, new[] { continuar, despedida }, "¿Qué deseas hacer?", "Elige una opción", promptStyle: PromptStyle.Keyboard);
        }
        //---------------------------------------------------------------------------------------------------------
       
        //tarjetas enriquecidas
        private Attachment getImageCard1()
        {
            var imageCard = new HeroCard
            {
                Title = "Imagen",
                Subtitle = "Image Develoment",
                Images = new List<CardImage>{
                    new CardImage(){
                        Url ="https://universoabiertoblog.files.wordpress.com/2017/01/aco_bot.jpg"
                    }
             },
                Buttons = new List<CardAction>{
                    new CardAction(ActionTypes.OpenUrl,title:"Ir al sitio",value:"https://www.notariadoplurinacional.gob.bo/index.php/servnotariales/")
                }
            };
            return imageCard.ToAttachment();
        }
        private Attachment getImageCard()
        {
            var imageCard = new HeroCard
            {
                Title = "Imagen",
                Subtitle = "Image Develoment",
                Images = new List<CardImage>{
                    new CardImage(){
                        Url = "https://universoabiertoblog.files.wordpress.com/2017/01/aco_bot.jpg"
                    }
                },
                Buttons = new List<CardAction>{
                    new CardAction(ActionTypes.OpenUrl,title:"Ir al sitio",value:"https://www.icalp.org.bo/Default") //http://rpa2.justicia.gob.bo/#/Busqueda
                }
            };
            return imageCard.ToAttachment();
        }
        private Attachment getImageCard2()
        {
            var imageCard = new HeroCard
            {
                Title = "Imagen",
                Subtitle = "Image Develoment",
                Images = new List<CardImage>{
                    new CardImage(){
                        Url ="https://universoabiertoblog.files.wordpress.com/2017/01/aco_bot.jpg"
                    }
                },
                Buttons = new List<CardAction>{new CardAction(ActionTypes.OpenUrl,title:"Ir al sitio",value:"http://sitservicios.lapaz.bo/sit/riesgos/?fbclid=IwAR36Hl--rsn1JImAhpiCGjC6xgQYY5rqnpnRbLYd6ZZRUlF65tAmhshPeMI")
}
            };
            return imageCard.ToAttachment();
        }
        //otros metodos
        private async Task Selection(IDialogContext context, IAwaitable<string> result)
        {
            var opcion = await result;
            switch (opcion)
            {
                case continuar:
                    await context.PostAsync("Ok, continua preguntantdo");
                    break;
                case despedida:
                    await context.PostAsync("Hasta luego");
                    break;
                case Indagacion:
                    context.Wait(MessageReceived);
                    break;
                default:
                    break;
            }
        }
        private async Task Selection2(IDialogContext context, IAwaitable<string> result)
        {
            var opcion = await result;
        }
    }       
    
}
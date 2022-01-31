﻿using Newtonsoft.Json;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProyectoParking.Servicios
{
    static class ServicioComprobarCara
    {
        public static FaceAttributes ComprobarCara(string imagen)
        {
            var response = PostCara(imagen);
            Root[] respuesta = JsonConvert.DeserializeObject<Root[]>(response.Content);
            return respuesta[0].faceAttributes;
        }

        public static IRestResponse PostCara(string imagen)
        {
            //Cambiar variables a variables de configuracion
            //Con la version 1.0 funciona, probar el lunes con el proyecto de consola si va
            var client = new RestClient("https://servicio-face-proyecto-parking.cognitiveservices.azure.com/face/v1.0/");
            var request = new RestRequest("detect", Method.POST);
            string data = "{ 'url':'" + imagen + "'}";
            request.AddHeader("Ocp-Apim-Subscription-Key", "38cac79a9bd04f648466d0b24ad1d9f5");
            request.AddHeader("Content-Type", "application/json");
            request.AddJsonBody(data);
            request.AddParameter("returnFaceAttributes", "age, gender", ParameterType.QueryString);
            var response = client.Execute(request);
            return response;
        }

        //Clases para convertir el JSON
        public class FaceRectangle
        {
            public int top { get; set; }
            public int left { get; set; }
            public int width { get; set; }
            public int height { get; set; }
        }

        public class FaceAttributes
        {
            public string gender { get; set; }
            public double age { get; set; }
        }

        public class Root
        {
            public string faceId { get; set; }
            public FaceRectangle faceRectangle { get; set; }
            public FaceAttributes faceAttributes { get; set; }
        }
    }
}

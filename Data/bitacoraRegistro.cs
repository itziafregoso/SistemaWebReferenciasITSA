using SistemaWeb.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web;

namespace SistemaWeb.Data
{
    public class bitacoraRegistro
    {

        public void registroBitacora(string cadena, int idUser, string operacion)
        {
            DataBase db = new DataBase();
            string localIP="";

            IPHostEntry host = Dns.GetHostEntry(Dns.GetHostName());// objeto para guardar la ip
            foreach (IPAddress ip in host.AddressList)
            {
                if (ip.AddressFamily.ToString() == "InterNetwork")
                {
                    localIP = ip.ToString();// esta es nuestra ip
                }
            }

            Eventos nuevoEvento = new Eventos
            {
                fecha = DateTime.Now,
                hora = DateTime.Now.ToString("hh:mm"),
                operacion = operacion,
                descripcion = cadena,
                IdUsuariosAd = idUser,
                ip = localIP
            };
            db.Eventos.Add(nuevoEvento);
            db.SaveChanges();
        }

    }
}
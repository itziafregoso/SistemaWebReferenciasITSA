using SistemaWeb.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using static System.Net.WebRequestMethods;

namespace SistemaWeb.Data
{
    public class coloresSistema
    {

        public void recuperarColores()
        {
            DataBase db = new DataBase();

            OpcionesSistema os = db.OpcionesSistema.FirstOrDefault();
            HttpContext.Current.Session["colorPrimario"] = os.colorPrimario;
            HttpContext.Current.Session["colorPrimarioAl"] = os.colorPrimarioAl;
            HttpContext.Current.Session["colorSecundario"] = os.colorSecundario;

            HttpContext.Current.Session["colorTitulos"] = os.colorTitulos;
            HttpContext.Current.Session["colorTexto"] = os.colorTexto;

            HttpContext.Current.Session["colorBVer"] = os.colorBVer;
            HttpContext.Current.Session["colorBVerAl"] = os.colorBVerAl;
            HttpContext.Current.Session["colorBEditar"] = os.colorBEditar;
            HttpContext.Current.Session["colorBEditarAl"] = os.colorBEditarAl;
            HttpContext.Current.Session["colorBEliminar"] = os.colorBEliminar;
            HttpContext.Current.Session["colorBEliminarAl"] = os.colorBEliminarAl;
            HttpContext.Current.Session["colorBComprar"] = os.colorBComprar;
            HttpContext.Current.Session["colorBComprarAl"] = os.colorBComprarAl;
        }


        public void recuperarParametros()
        {
            DataBase db = new DataBase();

            OpcionesSistema os = db.OpcionesSistema.FirstOrDefault();
            HttpContext.Current.Session["numCuenta"] = os.numCuenta;
            HttpContext.Current.Session["cuenClave"] = os.cuenClave;
            HttpContext.Current.Session["nomBuzon"] = os.nomBuzon;
            HttpContext.Current.Session["constanteRef"] = os.constanteRef;
            HttpContext.Current.Session["numRap"] = os.numRap;
        }
    }
}
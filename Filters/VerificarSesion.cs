using SistemaWeb.Controllers;
using SistemaWeb.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace SistemaWeb.Filters
{
    public class VerificarSesion : ActionFilterAttribute
    {
        private UsuariosAd pUsuario;
        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            try
            {
                base.OnActionExecuting(filterContext);
                DataBase db = new DataBase();             

                pUsuario = (UsuariosAd)HttpContext.Current.Session["User"];

                //Valida permisos de usuario
                if (pUsuario == null)
                {
                    if(filterContext.Controller is HomeController == false)
                    {
                        filterContext.HttpContext.Response.Redirect("/Home/Index");
                    }
                }
                else
                {

                    UsuariosAd actUsuario = db.UsuariosAd.FirstOrDefault(e => e.IdUsuariosAd == pUsuario.IdUsuariosAd);
                    pUsuario = actUsuario;
                    PerfilesAd pPerfil = db.PerfilesAd.FirstOrDefault(e => e.IdPerfil == pUsuario.IdPerfil);

                    if (pUsuario.estado == false && pUsuario != null)
                    {
                        HttpContext.Current.Session["User"] = null;
                    }

                    List<string> operacionesAutorizadas = new List<string>();

                    if (pPerfil.adminadareas == true)
                        operacionesAutorizadas.Add("adminadareas");

                    if (pPerfil.administrarareas == true)
                        operacionesAutorizadas.Add("administrarareas");

                    if (pPerfil.adminparametros == true)
                        operacionesAutorizadas.Add("adminparametros");

                    if (pPerfil.adminperfiles == true)
                        operacionesAutorizadas.Add("adminperfiles");

                    if (pPerfil.adminservicio == true)
                        operacionesAutorizadas.Add("adminservicio");

                    if (pPerfil.admintiposervicios == true)
                        operacionesAutorizadas.Add("admintiposervicios");

                    if (pPerfil.adminusuarios == true)
                        operacionesAutorizadas.Add("adminusuarios");

                    if (pPerfil.bitacoraeventos == true)
                        operacionesAutorizadas.Add("bitacoraeventos");

                    if (pPerfil.generarcompra == true)
                        operacionesAutorizadas.Add("generarcompra");

                    if (pPerfil.subirarchivo == true)
                        operacionesAutorizadas.Add("subirarchivo");

                    if (pPerfil.verreferencias == true)
                        operacionesAutorizadas.Add("verreferencias");

                    if (pPerfil.verservicios == true)
                        operacionesAutorizadas.Add("verservicios");

                    if (pPerfil.adminalumnos == true)
                        operacionesAutorizadas.Add("adminalumnos");

                    if (pPerfil.veralumnos == true)
                        operacionesAutorizadas.Add("veralumnos");

                    if (pPerfil.subiralumnos == true)
                        operacionesAutorizadas.Add("subiralumnos");

                    if (pPerfil.verhistorico == true)
                        operacionesAutorizadas.Add("verhistorico");

                    if (pPerfil.restaurarsistema == true)
                        operacionesAutorizadas.Add("restaurarsistema");

                    HttpContext.Current.Session["Permisos"] = null;
                    HttpContext.Current.Session["Permisos"] = operacionesAutorizadas;

                    HttpContext.Current.Session["NombreUser"] = pUsuario.nombre;
                    HttpContext.Current.Session["Rol"] = pPerfil.nombreperfil;
                }

            }
            catch (Exception)
            {
                filterContext.Result = new RedirectResult("~/Home/Index");
            }
        }

    }
}
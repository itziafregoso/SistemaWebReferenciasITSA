using DocumentFormat.OpenXml.Wordprocessing;
using SistemaWeb.Data;
using SistemaWeb.Filters;
using SistemaWeb.Models;
using SistemaWeb.Models.ViewModels;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Linq.Dynamic;
using System.Linq.Expressions;

namespace SistemaWeb.Controllers
{
    public class PerfilesController : Controller
    {
        // Atributos DataTable
        public string draw = "";
        public string start = "";
        public string length = "";
        public string sortColumn = "";
        public string sortColumnDir = "";
        public string searchValue = "";
        public int pageSize, skip, recordsTotal;

        //Conexión a base de datos
        DataBase db = new DataBase();
        coloresSistema cs = new coloresSistema();
        bitacoraRegistro br = new bitacoraRegistro();

        [AuthorizeUser(nombreOperacion: "adminperfiles")]
        //Administrar perfiles administrativos
        [HttpGet]
        public ActionResult Index(int? id, string op, string user)
        {
            if(TempData["error"] != null)
            {
                ViewBag.error = TempData["error"];
            }

            // Recupera colores almacenados en BD
            cs.recuperarColores();

            ViewBag.op = op;
            ViewBag.user = user;
            if (id != null)
            {
                // Habilita panel para ver información de usuario
                PerfilesAd usuario = db.PerfilesAd.Find(id);
                if (usuario == null)
                {
                    return HttpNotFound();
                }

                ViewBag.Name = db.PerfilesAd.ToList();
                ViewBag.Editar = true;
                ViewBag.id = id;
                return View(usuario);
            }
            else
            {
                //Muestra panel de administrar perfiles
                ViewBag.Editar = false;
                return View();
            }
        }

        [AuthorizeUser(nombreOperacion: "adminperfiles")]
        //Crear perfil nuevo
        [HttpPost]
        public ActionResult Index([ModelBinder(typeof(PerfilBinder))] PerfilesAd perfil)
        {
            var pUser = (from d in db.PerfilesAd
                         where d.nombreperfil == perfil.nombreperfil
                         select d).FirstOrDefault();

            if (pUser != null)
            {
                ViewBag.error = "Ya existe un perfil registrado con el mismo nombre";

                //Recupera colores almacenados en base de datos
                cs.recuperarColores();

                return View();
            }

            if (ModelState.IsValid)
            {

                if (perfil.adminperfiles == true && perfil.verservicios == true && perfil.admintiposervicios == true && perfil.verreferencias == true && perfil.adminusuarios == true &&
                    perfil.administrarareas == true && perfil.adminservicio == true && perfil.bitacoraeventos == true && perfil.subirarchivo == true && perfil.generarcompra == true &&
                    perfil.adminparametros == true && perfil.adminadareas == true && perfil.veralumnos == true && perfil.adminalumnos == true && perfil.subiralumnos == true && perfil.verhistorico == true
                    && perfil.restaurarsistema == true)
                {
                    ViewBag.error = "Solo el perfil ADMINISTRADOR puede tener acceso completo";

                    //Recupera colores almacenados en base de datos
                    cs.recuperarColores();

                    return View();
                }


                    if (perfil.adminperfiles == true | perfil.verservicios == true | perfil.admintiposervicios == true | perfil.verreferencias == true | perfil.adminusuarios == true |
                    perfil.administrarareas == true | perfil.adminservicio == true | perfil.bitacoraeventos == true | perfil.subirarchivo == true | perfil.generarcompra == true |
                    perfil.adminparametros == true | perfil.adminadareas == true | perfil.veralumnos == true | perfil.adminalumnos == true | perfil.subiralumnos == true | perfil.verhistorico == true
                    | perfil.restaurarsistema == true)
                {
                    // Guardar usuario nuevo en BD
                    db.PerfilesAd.Add(perfil);
                    db.SaveChanges();
                }
                else
                {
                    ViewBag.error = "Debe conceder al menos un acceso al perfil";

                    //Recupera colores almacenados en base de datos
                    cs.recuperarColores();

                    return View();
                }




                // Añadir proceso a bitácora de eventos
                String cadena = "Se registró el perfil " + perfil.nombreperfil;
                int idUser = (int)Session["IdUser"];

                br.registroBitacora(cadena, idUser, "Registro");

            }

            return RedirectToAction("Index", new { op = "Agregar", user = perfil.nombreperfil });
        }

        // Crea objeto Perfil para añadirlo a BD
        public class PerfilBinder : IModelBinder
        {
            public object BindModel(ControllerContext controllerContext, ModelBindingContext bindingContext)
            {
                HttpContextBase objContext = controllerContext.HttpContext;
                string cnombre = objContext.Request.Form["nombre"];
                string adminperfiles = objContext.Request.Form["adminperfiles"];
                string verservicios = objContext.Request.Form["verservicios"];
                string admintiposervicios = objContext.Request.Form["admintiposervicios"];
                string verreferencias = objContext.Request.Form["verreferencias"];
                string adminusuarios = objContext.Request.Form["adminusuarios"];
                string administrarareas = objContext.Request.Form["administrarareas"];
                string adminservicio = objContext.Request.Form["adminservicio"];
                string bitacoraeventos = objContext.Request.Form["bitacoraeventos"];
                string subirarchivo = objContext.Request.Form["subirarchivo"];
                string generarcompra = objContext.Request.Form["generarcompra"];
                string parametros = objContext.Request.Form["adminparametros"];
                string area = objContext.Request.Form["adminadareas"];

                string veralumnos = objContext.Request.Form["veralumnos"];
                string adminalumnos = objContext.Request.Form["adminalumnos"];
                string cargaralumnos = objContext.Request.Form["cargaralumnos"];
                string verhistorico = objContext.Request.Form["verhistorico"];
                string restaurarsistema = objContext.Request.Form["restaurarsistema"];

                bool ap, vs, ats, vr, au, aa, aserv, be, sa, gc, apa, ada, va, adal, cal, vh,rs;

                if (adminperfiles == "on") { ap = true; } else { ap = false; }
                if (verservicios == "on") { vs = true; } else { vs = false; }
                if (admintiposervicios == "on") { ats = true; } else { ats = false; }
                if (verreferencias == "on") { vr = true; } else { vr = false; }
                if (adminusuarios == "on") { au = true; } else { au = false; }
                if (administrarareas == "on") { aa = true; } else { aa = false; }
                if (adminservicio == "on") { aserv = true; } else { aserv = false; }
                if (bitacoraeventos == "on") { be = true; } else { be = false; }
                if (subirarchivo == "on") { sa = true; } else { sa = false; }
                if (generarcompra == "on") { gc = true; } else { gc = false; }
                if (parametros == "on") { apa = true; } else { apa = false; }
                if (area == "on") { ada = true; } else { ada = false; }

                if (veralumnos == "on") { va = true; } else { va = false; }
                if (adminalumnos == "on") { adal = true; } else { adal = false; }
                if (cargaralumnos == "on") { cal = true; } else { cal = false; }
                if (verhistorico == "on") { vh = true; } else { vh = false; }
                if (restaurarsistema == "on") { rs = true; } else { rs = false; }

                PerfilesAd objPerfil = new PerfilesAd
                {
                    nombreperfil = cnombre,
                    adminperfiles = ap,
                    verservicios = vs,
                    admintiposervicios = ats,
                    verreferencias = vr,
                    adminusuarios = au,
                    administrarareas = aa,
                    adminservicio = aserv,
                    bitacoraeventos = be,
                    subirarchivo = sa,
                    generarcompra = gc,
                    adminparametros = apa,
                    adminadareas = ada,
                    veralumnos = va,
                    adminalumnos = adal,
                    subiralumnos = cal,
                    verhistorico = vh,
                    restaurarsistema = rs
                };

                return objPerfil;
            }
        }

        [AuthorizeUser(nombreOperacion: "adminperfiles")]
        public ActionResult JsonPerfiles()
        {
            List<TablePerfilesViewModel> lista = new List<TablePerfilesViewModel>();

            var draw = Request.Form.GetValues("draw").FirstOrDefault();
            var start = Request.Form.GetValues("start").FirstOrDefault();
            var length = Request.Form.GetValues("length").FirstOrDefault();
            var sortColumn = Request.Form.GetValues("columns[" + Request.Form.GetValues("order[0][column]").FirstOrDefault() + "][name]").FirstOrDefault();
            var sortColumnDir = Request.Form.GetValues("order[0][dir]").FirstOrDefault();
            var searchValue = Request.Form.GetValues("search[value]").FirstOrDefault();

            pageSize = length != null ? Convert.ToInt32(length) : 0;
            skip = start != null ? Convert.ToInt32(start) : 0;
            recordsTotal = 0;

            using (DataBase db = new DataBase())
            {

                IQueryable<TablePerfilesViewModel> query = (from d in db.PerfilesAd
                                                           select new TablePerfilesViewModel
                                                           {
                                                               IdPerfil = d.IdPerfil,
                                                               nombre = d.nombreperfil,
                                                               accesos = (d.adminperfiles ? 1 : 0) + (d.adminadareas ? 1 : 0) + (d.verservicios ? 1 : 0) + (d.admintiposervicios ? 1 : 0) + (d.verreferencias ? 1 : 0) + (d.adminusuarios ? 1 : 0) + (d.administrarareas ? 1 : 0) + (d.adminservicio ? 1 : 0) + (d.bitacoraeventos ? 1 : 0) + (d.subirarchivo ? 1 : 0) + (d.generarcompra ? 1 : 0) + (d.adminparametros ? 1 : 0) + (d.veralumnos ? 1 : 0) + (d.adminalumnos ? 1 : 0) + (d.subiralumnos ? 1 : 0) + (d.verhistorico ? 1 : 0) + (d.restaurarsistema ? 1 : 0),
                                                               restricciones = (!d.adminperfiles ? 1 : 0) + (!d.adminadareas ? 1 : 0) + (!d.verservicios ? 1 : 0) + (!d.admintiposervicios ? 1 : 0) + (!d.verreferencias ? 1 : 0) + (!d.adminusuarios ? 1 : 0) + (!d.administrarareas ? 1 : 0) + (!d.adminservicio ? 1 : 0) + (!d.bitacoraeventos ? 1 : 0) + (!d.subirarchivo ? 1 : 0) + (!d.generarcompra ? 1 : 0) + (!d.adminparametros ? 1 : 0) + (!d.veralumnos ? 1 : 0) + (!d.adminalumnos ? 1 : 0) + (!d.subiralumnos ? 1 : 0) + (!d.verhistorico ? 1 : 0) + (!d.restaurarsistema ? 1 : 0),
                                                           });

                if (searchValue != "")
                    query = query.Where(d => d.nombre.Contains(searchValue));

                if (!(string.IsNullOrEmpty(sortColumn) && string.IsNullOrEmpty(sortColumnDir)))
                {
                    query = query.OrderBy(sortColumn + " " + sortColumnDir);
                }

                recordsTotal = query.Count();

                lista = query.Skip(skip).Take(pageSize).ToList();
            }

            return Json(new { draw = draw, recordsFiltered = recordsTotal, recordsTotal = recordsTotal, data = lista });
        }


        [AuthorizeUser(nombreOperacion: "adminperfiles")]
        // Ventana para mostrar datos de perfil
        [HttpGet]
        public ActionResult ConfirmarPerfiles(string op, int id)
        {
            cs.recuperarColores();

            PerfilesAd usuario = db.PerfilesAd.Find(id);
            ViewBag.msg = op;
            ViewBag.nombre = usuario.nombreperfil;

            if (op == "Confirmar")
            {
                var actUser = TempData["usuarioActualizado"] as PerfilesAd;
                TempData["usuarioActualizado"] = TempData["usuarioActualizado"];
                return View(actUser);
            }
            return View(usuario);
        }

        [AuthorizeUser(nombreOperacion: "adminperfiles")]
        // Realiza operación a perfil, eliminar o actualizar
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ConfirmarPerfiles(int id, string op, [Bind(Include = "IdPerfil, nombreperfil, adminperfiles, verservicios, admintiposervicios, verreferencias, adminusuarios, administrarareas, adminparametros, adminservicio, generarcompra, bitacoraeventos,subirarchivo,adminadareas,adminalumnos,veralumnos,subiralumnos,verhistorico,restaurarsistema")] PerfilesAd nuevoPerfil)
        {
            if (op == "Eliminar")
            {
                PerfilesAd perfilAd = db.PerfilesAd.Find(id);
                var nombre = perfilAd.nombreperfil;

                if(nombre == "ADMINISTRADOR")
                {
                    TempData["error"] = "El perfil ADMINISTRADOR no puede ser eliminado";

                    //Recupera colores almacenados en base de datos
                    cs.recuperarColores();

                    return RedirectToAction("Index");
                }

                db.PerfilesAd.Remove(perfilAd);
                try
                {
                    db.SaveChanges();
                }catch (Exception ex)
                {
                    TempData["error"] = "Existen usuarios con este perfil asignado";

                    //Recupera colores almacenados en base de datos
                    cs.recuperarColores();

                    return RedirectToAction("Index");
                }
                

                // Añadir proceso a bitácora
                String cadena = "Se eliminó el perfil " + perfilAd.nombreperfil;
                int idUser = (int)Session["IdUser"];

                br.registroBitacora(cadena, idUser, "Eliminación");

                return RedirectToAction("Index", new { op = "Eliminar", user = nombre });
            }
            else if (op == "Actualizar")
            {
                TempData["usuarioActualizado"] = nuevoPerfil;

                var pUser = (from d in db.PerfilesAd
                             where d.nombreperfil == nuevoPerfil.nombreperfil && d.IdPerfil != nuevoPerfil.IdPerfil
                             select d).FirstOrDefault();

                if (nuevoPerfil.nombreperfil == "ADMINISTRADOR")
                {
                    TempData["error"] = "El perfil ADMINISTRADOR no puede ser editado";

                    //Recupera colores almacenados en base de datos
                    cs.recuperarColores();

                    return RedirectToAction("Index");
                }

                if (pUser != null)
                {
                    TempData["error"] = "Ya existe un perfil registrado con el mismo nombre";
                    return RedirectToAction("Index");
                }

                return RedirectToAction("ConfirmarPerfiles", new { op = "Confirmar", id = id });
            }
            else
            {
                if (ModelState.IsValid)
                {
                    nuevoPerfil = TempData["usuarioActualizado"] as PerfilesAd;
                    db.Entry(nuevoPerfil).State = EntityState.Modified;
                    db.SaveChanges();

                    // Añadir proceso a bitácora
                    String cadena = "Se actualizó el perfil " + nuevoPerfil.nombreperfil;
                    int idUser = (int)Session["IdUser"];

                    br.registroBitacora(cadena, idUser, "Actualización");

                    return RedirectToAction("Index", new { op = "Actualizar", user = nuevoPerfil.nombreperfil });
                }
                return RedirectToAction("Index");
            }
        }

    }
}
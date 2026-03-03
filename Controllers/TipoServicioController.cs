using SistemaWeb.Data;
using SistemaWeb.Filters;
using SistemaWeb.Models;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Linq.Dynamic;
using SistemaWeb.Models.ViewModels;

namespace SistemaWeb.Controllers
{
    public class TipoServicioController : Controller
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

        [AuthorizeUser(nombreOperacion: "admintiposervicios")]
        // Administrar tipos de servicios
        [HttpGet]
        public ActionResult Index(int? id, string op, string user)
        {
            if (TempData["error"] != null)
            {
                ViewBag.error = TempData["error"];
            }

            // Recupera colores de BD
            cs.recuperarColores();

            ViewBag.op = op;
            ViewBag.user = user;
            if (id != null)
            {
                // Muestra información de un tipo de servicio
                TipoServicios usuario = db.TipoServicios.Find(id);
                if (usuario == null)
                {
                    return HttpNotFound();
                }

                ViewBag.Name = db.TipoServicios.ToList();
                ViewBag.Editar = true;
                ViewBag.id = id;
                return View(usuario);
            }
            else
            {
                //Muestra panel principal de administrar tipos
                ViewBag.Editar = false;
                return View();
            }
        }

        [AuthorizeUser(nombreOperacion: "admintiposervicios")]
        //Agrega un nuevo tipo de servicio
        [HttpPost]
        public ActionResult Index([ModelBinder(typeof(TServicioBinder))] TipoServicios usuario)
        {

            var pUser = (from d in db.TipoServicios
                         where d.tipo == usuario.tipo
                         select d).FirstOrDefault();

            if(pUser != null)
            {
                ViewBag.error = "Ya existe un tipo de servicio registrado con el mismo nombre";

                //Recupera colores almacenados en base de datos
                cs.recuperarColores();

                return View();
            }

            if (ModelState.IsValid)
            {
                //Registra el tipo de servicio en la BD
                db.TipoServicios.Add(usuario);
                db.SaveChanges();


                // Añadir proceso a bitácora de eventos
                String cadena = "Se registró el tipo de servicio " + usuario.tipo;
                int idUser = (int)Session["IdUser"];

                br.registroBitacora(cadena, idUser, "Registro");


            }

            return RedirectToAction("Index", new { op = "Agregar", user = usuario.tipo });
        }

        // Crea un objeto tipo de servicio para añadir a BD
        public class TServicioBinder : IModelBinder
        {
            public object BindModel(ControllerContext controllerContext, ModelBindingContext bindingContext)
            {
                HttpContextBase objContext = controllerContext.HttpContext;
                string cnombre = objContext.Request.Form["tipo"];

                TipoServicios objPerfil = new TipoServicios
                {
                    tipo = cnombre
                };

                return objPerfil;
            }
        }

        [AuthorizeUser(nombreOperacion: "admintiposervicios")]
        public ActionResult JsonTipos()
        {
            List<TableAreasAdministrativasViewModel> lista = new List<TableAreasAdministrativasViewModel>();

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

                IQueryable<TableAreasAdministrativasViewModel> query = (from d in db.TipoServicios
                                                                        select new TableAreasAdministrativasViewModel
                                                                        {
                                                                            IdAreaAd = d.IdTS,
                                                                            nombre = d.tipo
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

        [AuthorizeUser(nombreOperacion: "admintiposervicios")]
        // Información sobre un tipo de servicio
        [HttpGet]
        public ActionResult ConfirmarTipoServicio(string op, int id)
        {
            cs.recuperarColores();

            TipoServicios usuario = db.TipoServicios.Find(id);
            ViewBag.msg = op;
            ViewBag.nombre = usuario.tipo;

            if (op == "Confirmar")
            {
                var actUser = TempData["usuarioActualizado"] as TipoServicios;
                TempData["usuarioActualizado"] = TempData["usuarioActualizado"];
                return View(actUser);
            }
            return View(usuario);
        }

        [AuthorizeUser(nombreOperacion: "admintiposervicios")]
        //Realiza un operación sobre un tipo de servicio, eliminar o actualizar
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ConfirmarTipoServicio(int id, string op, [Bind(Include = "IdTS, tipo")] TipoServicios nuevoPerfil)
        {
            if (op == "Eliminar")
            {
                TipoServicios perfilAd = db.TipoServicios.Find(id);
                var nombre = perfilAd.tipo;
                db.TipoServicios.Remove(perfilAd);
                db.SaveChanges();

                // Añadir proceso a bitácora de eventos
                String cadena = "Se eliminó el tipo de servicio " + perfilAd.tipo;
                int idUser = (int)Session["IdUser"];

                br.registroBitacora(cadena, idUser, "Eliminación");

                return RedirectToAction("Index", new { op = "Eliminar", user = nombre });
            }
            else if (op == "Actualizar")
            {
                TempData["usuarioActualizado"] = nuevoPerfil;

                var pUser = (from d in db.TipoServicios
                             where d.tipo == nuevoPerfil.tipo && d.IdTS != nuevoPerfil.IdTS
                             select d).FirstOrDefault();

                if (pUser != null)
                {
                    TempData["error"] = "Ya existe un tipo de servicio registrado con el mismo nombre";
                    return RedirectToAction("Index");
                }

                return RedirectToAction("ConfirmarTipoServicio", new { op = "Confirmar", id = id });
            }
            else
            {
                if (ModelState.IsValid)
                {
                    nuevoPerfil = TempData["usuarioActualizado"] as TipoServicios;
                    db.Entry(nuevoPerfil).State = EntityState.Modified;
                    db.SaveChanges();

                    // Añadir proceso a bitácora de eventos
                    String cadena = "Se actualizó el tipo de servicio " + nuevoPerfil.tipo;
                    int idUser = (int)Session["IdUser"];

                    br.registroBitacora(cadena, idUser, "Actualización");

                    return RedirectToAction("Index", new { op = "Actualizar", user = nuevoPerfil.tipo });
                }
                return RedirectToAction("Index");
            }
        }

    }
}
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

namespace SistemaWeb.Controllers
{
    public class AreasController : Controller
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

        [AuthorizeUser(nombreOperacion: "administrarareas")]
        // Ventana para administrar áreas
        [HttpGet]
        public ActionResult Index(int? id, string op, string user)
        {
            if (TempData["error"] != null)
            {
                ViewBag.error = TempData["error"];
            }

            //Recupera colores almacenados en BD
            cs.recuperarColores();

            ViewBag.op = op;
            ViewBag.user = user;
            if (id != null)
            {
                // Muestra información de un área
                Areas usuario = db.Areas.Find(id);
                if (usuario == null)
                {
                    return HttpNotFound();
                }

                ViewBag.Name = db.Areas.ToList();
                ViewBag.Editar = true;
                ViewBag.id = id;
                return View(usuario);
            }
            else
            {
                // Muestra panel principal de administrar áreas
                ViewBag.Editar = false;
                return View();
            }
        }

        [AuthorizeUser(nombreOperacion: "administrarareas")]
        //Crea una nueva área para almacenar en BD
        [HttpPost]
        public ActionResult Index([ModelBinder(typeof(AreaBinder))] Areas usuario)
        {
            var pUser = (from d in db.Areas
                         where d.nombrearea == usuario.nombrearea
                         select d).FirstOrDefault();

            if (pUser != null)
            {
                ViewBag.error = "Ya existe un área registrada con el mismo nombre";

                //Recupera colores almacenados en base de datos
                cs.recuperarColores();

                return View();
            }

            if (ModelState.IsValid)
            {
                //Guarda nueva área en BD
                db.Areas.Add(usuario);
                db.SaveChanges();


                // Añadir a bitácora de eventos
                String cadena = "Se registró el área " + usuario.nombrearea;
                int idUser = (int)Session["IdUser"];

                br.registroBitacora(cadena, idUser, "Registro");

            }

            return RedirectToAction("Index", new { op = "Agregar", user = usuario.nombrearea });
        }

        //Crea objeto Area para guardar en BD
        public class AreaBinder : IModelBinder
        {
            public object BindModel(ControllerContext controllerContext, ModelBindingContext bindingContext)
            {
                HttpContextBase objContext = controllerContext.HttpContext;
                string cnombre = objContext.Request.Form["nombrearea"];

                Areas objPerfil = new Areas
                {
                    nombrearea = cnombre
                };

                return objPerfil;
            }
        }

        [AuthorizeUser(nombreOperacion: "administrarareas")]
        //Consulta JSON que recupera todas las areas de la BD
        public ActionResult JsonAreas()
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

                IQueryable<TableAreasAdministrativasViewModel> query = (from d in db.Areas
                                                                        select new TableAreasAdministrativasViewModel
                                                                        {
                                                                            IdAreaAd = d.IdArea,
                                                                            nombre = d.nombrearea
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


        [AuthorizeUser(nombreOperacion: "administrarareas")]
        //Ventana para ver información de un área
        [HttpGet]
        public ActionResult ConfirmarArea(string op, int id)
        {
            cs.recuperarColores();

            Areas usuario = db.Areas.Find(id);
            ViewBag.msg = op;
            ViewBag.nombre = usuario.nombrearea;

            if (op == "Confirmar")
            {
                var actUser = TempData["usuarioActualizado"] as Areas;
                TempData["usuarioActualizado"] = TempData["usuarioActualizado"];
                return View(actUser);
            }
            return View(usuario);
        }

        [AuthorizeUser(nombreOperacion: "administrarareas")]
        //Realiza operación sobre un área, eliminar o actualizar
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ConfirmarArea(int id, string op, [Bind(Include = "IdArea, nombrearea")] Areas nuevoPerfil)
        {
            if (op == "Eliminar")
            {
                Areas perfilAd = db.Areas.Find(id);
                var nombre = perfilAd.nombrearea;
                db.Areas.Remove(perfilAd);
                db.SaveChanges();


                // Añadir proceso a bitácora de eventos
                String cadena = "Se eliminó el área " + perfilAd.nombrearea;
                int idUser = (int)Session["IdUser"];

                br.registroBitacora(cadena, idUser, "Eliminación");

                return RedirectToAction("Index", new { op = "Eliminar", user = nombre });
            }
            else if (op == "Actualizar")
            {
                TempData["usuarioActualizado"] = nuevoPerfil;

                var pUser = (from d in db.Areas
                             where d.nombrearea == nuevoPerfil.nombrearea && d.IdArea != nuevoPerfil.IdArea
                             select d).FirstOrDefault();

                if (pUser != null)
                {
                    TempData["error"] = "Ya existe un área registrada con el mismo nombre";
                    return RedirectToAction("Index");
                }

                return RedirectToAction("ConfirmarArea", new { op = "Confirmar", id = id });
            }
            else
            {
                if (ModelState.IsValid)
                {
                    nuevoPerfil = TempData["usuarioActualizado"] as Areas;
                    db.Entry(nuevoPerfil).State = EntityState.Modified;
                    db.SaveChanges();


                    // Añadir proceso a bitácora de eventos
                    String cadena = "Se actualizó el área " + nuevoPerfil.nombrearea;
                    int idUser = (int)Session["IdUser"];

                    br.registroBitacora(cadena, idUser, "Actualización");

                    return RedirectToAction("Index", new { op = "Actualizar", user = nuevoPerfil.nombrearea });
                }
                return RedirectToAction("Index");
            }
        }




    }
}
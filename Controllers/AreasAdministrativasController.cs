using SistemaWeb.Data;
using SistemaWeb.Filters;
using SistemaWeb.Models;
using SistemaWeb.Models.ViewModels;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using System.Linq.Dynamic;

namespace SistemaWeb.Controllers
{
    public class AreasAdministrativasController : Controller
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

        [AuthorizeUser(nombreOperacion: "adminadareas")]
        [HttpGet]
        public ActionResult Index(int? id, string op, string user)
        {

            if (TempData["error"] != null)
            {
                ViewBag.error = TempData["error"];
            }

            cs.recuperarColores();

            ViewBag.op = op;
            ViewBag.user = user;
            if (id != null)
            {
                // Muestra información de área seleccionada
                AreasAd usuario = db.AreasAd.Find(id);
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
                //Muestra panel principal de administrar areasad
                ViewBag.Editar = false;
                return View();
            }
        }

        [AuthorizeUser(nombreOperacion: "adminadareas")]
        //Registra nueva área creada
        [HttpPost]
        public ActionResult Index([ModelBinder(typeof(AreaAdBinder))] AreasAd usuario)
        {


            var pUser = (from d in db.AreasAd
                         where d.nombrearead == usuario.nombrearead
                         select d).FirstOrDefault();

            if(pUser != null)
            {
                ViewBag.error = "Ya existe un área registrada con el mismo nombre";

                //Recupera colores almacenados en base de datos
                cs.recuperarColores();

                return View();
            }

            if (ModelState.IsValid)
            {
                // Almacena área en base de datos
                db.AreasAd.Add(usuario);
                db.SaveChanges();


                // Añadir proceso a bitácora de eventos
                String cadena = "Se registró el área administrativa " + usuario.nombrearead;
                int idUser = (int)Session["IdUser"];

                br.registroBitacora(cadena,idUser,"Registro");

            }

            return RedirectToAction("Index", new { op = "Agregar", user = usuario.nombrearead });
        }

        //Crea objeto areaAd para añadir a BD
        public class AreaAdBinder : IModelBinder
        {
            public object BindModel(ControllerContext controllerContext, ModelBindingContext bindingContext)
            {
                HttpContextBase objContext = controllerContext.HttpContext;
                string cnombre = objContext.Request.Form["nombrearead"];

                AreasAd objPerfil = new AreasAd
                {
                    nombrearead = cnombre
                };

                return objPerfil;
            }
        }


        [AuthorizeUser(nombreOperacion: "adminadareas")]
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

                IQueryable<TableAreasAdministrativasViewModel> query = (from d in db.AreasAd
                                                           select new TableAreasAdministrativasViewModel
                                                           {
                                                               IdAreaAd = d.IdAreaAd,
                                                               nombre = d.nombrearead
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

        [AuthorizeUser(nombreOperacion: "adminadareas")]
        // Ventana que muestra información de un área administrativa
        [HttpGet]
        public ActionResult ConfirmarAreaAdministrativa(string op, int id)
        {
            cs.recuperarColores();

            AreasAd usuario = db.AreasAd.Find(id);
            ViewBag.msg = op;
            ViewBag.nombre = usuario.nombrearead;

            if (op == "Confirmar")
            {
                var actUser = TempData["usuarioActualizado"] as AreasAd;
                TempData["usuarioActualizado"] = TempData["usuarioActualizado"];
                return View(actUser);
            }
            return View(usuario);
        }

        [AuthorizeUser(nombreOperacion: "adminadareas")]
        // Ventana que realiza la operación sobre un areaAd, eliminar o actualizar
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ConfirmarAreaAdministrativa(int id, string op, [Bind(Include = "IdAreaAd, nombrearead")] AreasAd nuevoPerfil)
        {
            if (op == "Eliminar")
            {
                AreasAd perfilAd = db.AreasAd.Find(id);
                var nombre = perfilAd.nombrearead;
                db.AreasAd.Remove(perfilAd);
                try
                {
                    db.SaveChanges();
                }
                catch (Exception ex)
                {
                    TempData["error"] = "Existen usuarios con esta área asignada";

                    //Recupera colores almacenados en base de datos
                    cs.recuperarColores();

                    return RedirectToAction("Index");
                }


                // Añadir proceso a bitácora de eventos
                String cadena = "Se eliminó el área administrativa " + perfilAd.nombrearead;
                int idUser = (int)Session["IdUser"];

                br.registroBitacora(cadena, idUser,"Eliminación");

                return RedirectToAction("Index", new { op = "Eliminar", user = nombre });
            }
            else if (op == "Actualizar")
            {
                TempData["usuarioActualizado"] = nuevoPerfil;


                var pUser = (from d in db.AreasAd
                             where d.nombrearead == nuevoPerfil.nombrearead && d.IdAreaAd != nuevoPerfil.IdAreaAd
                             select d).FirstOrDefault();

                if (pUser != null)
                {
                    TempData["error"] = "Ya existe un área registrada con el mismo nombre";
                    return RedirectToAction("Index");
                }

                return RedirectToAction("ConfirmarAreaAdministrativa", new { op = "Confirmar", id = id });
            }
            else
            {
                if (ModelState.IsValid)
                {
                    nuevoPerfil = TempData["usuarioActualizado"] as AreasAd;
                    db.Entry(nuevoPerfil).State = EntityState.Modified;
                    db.SaveChanges();


                    // Añadir proceso a bitácora de eventos
                    String cadena = "Se actualizó el área administrativa " + nuevoPerfil.nombrearead;
                    int idUser = (int)Session["IdUser"];

                    br.registroBitacora(cadena, idUser, "Actualización");

                    return RedirectToAction("Index", new { op = "Actualizar", user = nuevoPerfil.nombrearead });
                }
                return RedirectToAction("Index");
            }
        }
    }
}
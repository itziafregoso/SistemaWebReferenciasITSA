using ClosedXML.Excel;
using DocumentFormat.OpenXml.Wordprocessing;
using SistemaWeb.Data;
using SistemaWeb.Filters;
using SistemaWeb.Models;
using SistemaWeb.Models.ViewModels;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Linq.Dynamic;

namespace SistemaWeb.Controllers
{
    public class ServiciosController : Controller
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

        [AuthorizeUser(nombreOperacion: "adminservicio")]
        // Vista administrar servicios
        [HttpGet]
        public ActionResult Index(int? id, string op, string user)
        {
            if (TempData["error"] != null)
            {
                ViewBag.error = TempData["error"];
            }

            //Recupera colores de BD
            cs.recuperarColores();

            //Recupera valores para combobox
            ViewBag.IdArea = new SelectList(db.Areas, "IdArea", "nombrearea");
            ViewBag.IdTS = new SelectList(db.TipoServicios, "IdTS", "tipo");
            ViewBag.op = op;
            ViewBag.user = user;
            if (id != null)
            {
                Servicios usuario = db.Servicios.Find(id);
                if (usuario == null)
                {
                    return HttpNotFound();
                }

                ViewBag.Name = db.Servicios.ToList();
                ViewBag.Editar = true;
                ViewBag.id = id;
                return View(usuario);
            }
            else
            {
                ViewBag.Editar = false;
                return View();
            }
        }

        [AuthorizeUser(nombreOperacion: "adminservicio")]
        //Proceso para añadir nuevo servicio a BD
        [HttpPost]
        public ActionResult Index([ModelBinder(typeof(ServicioBinder))] Servicios usuario)
        {
            ViewBag.IdArea = new SelectList(db.Areas, "IdArea", "nombrearea");
            ViewBag.IdTS = new SelectList(db.TipoServicios, "IdTS", "tipo");


            var pUser = (from d in db.Servicios
                         where d.nomservicio == usuario.nomservicio && d.IdArea == usuario.IdArea && d.IdTS == usuario.IdTS
                         select d).FirstOrDefault();

            if(pUser != null)
            {
                ViewBag.error = "Ya existe un servicio registrado con el mismo nombre";

                //Recupera valores para combobox
                ViewBag.IdArea = new SelectList(db.Areas, "IdArea", "nombrearea");
                ViewBag.IdTS = new SelectList(db.TipoServicios, "IdTS", "tipo");

                //Recupera colores almacenados en base de datos
                cs.recuperarColores();

                return View();
            }

            if(usuario.costo == 0)
            {
                ViewBag.error = "El costo del servicio debe ser mayor a cero";

                //Recupera valores para combobox
                ViewBag.IdArea = new SelectList(db.Areas, "IdArea", "nombrearea");
                ViewBag.IdTS = new SelectList(db.TipoServicios, "IdTS", "tipo");

                //Recupera colores almacenados en base de datos
                cs.recuperarColores();

                return View();
            }

            if(usuario.IdArea == 0 | usuario.IdTS == 0)
            {
                ViewBag.error = "Debe asignar un área y un tipo de servicio";

                //Recupera valores para combobox
                ViewBag.IdArea = new SelectList(db.Areas, "IdArea", "nombrearea");
                ViewBag.IdTS = new SelectList(db.TipoServicios, "IdTS", "tipo");

                //Recupera colores almacenados en base de datos
                cs.recuperarColores();

                return View();
            }

            if (ModelState.IsValid)
            {
                db.Servicios.Add(usuario);
                db.SaveChanges();

                // Añadir proceso a bitácora de eventos
                String cadena = "Se registró el servicio " + usuario.nomservicio;
                int idUser = (int)Session["IdUser"];

                br.registroBitacora(cadena, idUser, "Registro");

            }
            return RedirectToAction("Index", new { op = "Agregar", user = usuario.nomservicio });
        }


        //Crea objeto servicio para añadirlo a la BD
        public class ServicioBinder : IModelBinder
        {
            public object BindModel(ControllerContext controllerContext, ModelBindingContext bindingContext)
            {
                HttpContextBase objContext = controllerContext.HttpContext;
                string cnombre = objContext.Request.Form["nomservicio"];
                int carea = Convert.ToInt32(objContext.Request.Form["IdArea"]);
                int ctipos = Convert.ToInt32(objContext.Request.Form["IdTS"]);
                string cobjetivo = objContext.Request.Form["Objetivo"];
                string cduracion = objContext.Request.Form["duracion"];
                double ccosto = Convert.ToDouble(objContext.Request.Form["costo"]);
                string cprerq = objContext.Request.Form["prerrequisitos"];
                int cconta = Convert.ToInt32(objContext.Request.Form["cuetacontable"]);
                string cestado = objContext.Request.Form["estado"];
                int cvig = Convert.ToInt32(objContext.Request.Form["diasvigencia"]);
                int cservmax = Convert.ToInt32(objContext.Request.Form["serviciosmaxacobrar"]);

                bool fe;

                if (cestado == "on") { fe = true; } else { fe = false; }

                Servicios objPerfil = new Servicios
                {
                    nomservicio = cnombre,
                    IdArea = carea,
                    IdTS = ctipos,
                    Objetivo = cobjetivo,
                    duracion = cduracion,
                    costo = Convert.ToDecimal(ccosto),
                    prerrequisitos = cprerq,
                    diasvigencia = cvig,
                    serviciosmaxacobrar = cservmax,
                    cuetacontable = cconta,
                    estado = fe
                };

                return objPerfil;
            }
        }

        [AuthorizeUser(nombreOperacion: "adminservicio")]
        public ActionResult JsonServicios()
        {
            List<TableServiciosViewModel> lista = new List<TableServiciosViewModel>();

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

                IQueryable<TableServiciosViewModel> query = (from d in db.Servicios
                                                           select new TableServiciosViewModel
                                                           {
                                                               IdServicio = d.contro,
                                                               tipo = d.TipoServicios.tipo,
                                                               area = d.Areas.nombrearea,
                                                               estado = d.estado,
                                                               costo = d.costo,
                                                               nombre = d.nomservicio,
                                                               maxcobrar = d.serviciosmaxacobrar,
                                                               cuentacontable = d.cuetacontable
                                                           });

                if (searchValue != "")
                    query = query.Where(d => d.nombre.Contains(searchValue) || d.tipo.Contains(searchValue) || d.area.Contains(searchValue));

                if (!(string.IsNullOrEmpty(sortColumn) && string.IsNullOrEmpty(sortColumnDir)))
                {
                    query = query.OrderBy(sortColumn + " " + sortColumnDir);
                }

                recordsTotal = query.Count();

                lista = query.Skip(skip).Take(pageSize).ToList();
            }

            return Json(new { draw = draw, recordsFiltered = recordsTotal, recordsTotal = recordsTotal, data = lista });
        }

        [AuthorizeUser(nombreOperacion: "generarcompra")]
        public ActionResult JsonServiciosCompra()
        {
            List<TableServiciosViewModel> lista = new List<TableServiciosViewModel>();

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

                IQueryable<TableServiciosViewModel> query = (from d in db.Servicios
                                                             where d.estado == true
                                                             select new TableServiciosViewModel
                                                             {
                                                                 IdServicio = d.contro,
                                                                 tipo = d.TipoServicios.tipo,
                                                                 area = d.Areas.nombrearea,
                                                                 costo = d.costo,
                                                                 nombre = d.nomservicio,
                                                             });

                if (searchValue != "")
                    query = query.Where(d => d.nombre.Contains(searchValue) || d.tipo.Contains(searchValue) || d.area.Contains(searchValue));

                if (!(string.IsNullOrEmpty(sortColumn) && string.IsNullOrEmpty(sortColumnDir)))
                {
                    query = query.OrderBy(sortColumn + " " + sortColumnDir);
                }

                recordsTotal = query.Count();

                lista = query.Skip(skip).Take(pageSize).ToList();
            }

            return Json(new { draw = draw, recordsFiltered = recordsTotal, recordsTotal = recordsTotal, data = lista });
        }

        [AuthorizeUser(nombreOperacion: "adminservicio")]
        //Ventana que muestra información de un servicio
        [HttpGet]
        public ActionResult ConfirmarServicio(string op, int id)
        {
            cs.recuperarColores();

            ViewBag.IdArea = new SelectList(db.Areas, "IdArea", "nombrearea");
            ViewBag.IdTS = new SelectList(db.TipoServicios, "IdTS", "tipo");
            Servicios usuario = db.Servicios.Find(id);
            ViewBag.msg = op;
            ViewBag.nombre = usuario.nomservicio;

            if (op == "Confirmar")
            {
                var actUser = TempData["usuarioActualizado"] as Servicios;
                TempData["usuarioActualizado"] = TempData["usuarioActualizado"];
                return View(actUser);
            }
            return View(usuario);
        }

        [AuthorizeUser(nombreOperacion: "adminservicio")]
        //Ventana que realiza operacion sobre servicio, eliminar o actualizar
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ConfirmarServicio(int id, string op, [Bind(Include = "contro,IdArea,IdTS,nomservicio,Objetivo,duracion,costo,prerrequisitos,diasvigencia,serviciosmaxacobrar,cuetacontable,estado")] Servicios nuevoPerfil)
        {
            if (op == "Eliminar")
            {
                Servicios perfilAd = db.Servicios.Find(id);

                if (perfilAd == null)
                {
                    TempData["error"] = "No se encontró el servicio.";
                    return RedirectToAction("Index");
                }

                // Verificar si tiene ventas asociadas
                bool tieneVentas = db.Ventas.Any(v => v.contro == id);
                if (tieneVentas)
                {
                    TempData["error"] = "No se puede eliminar el servicio '" + perfilAd.nomservicio + "' porque tiene ventas registradas.";
                    return RedirectToAction("Index");
                }

                var nombre = perfilAd.nomservicio;
                db.Servicios.Remove(perfilAd);
                db.SaveChanges();

                // Añadir proceso a bitácora de eventos
                String cadena = "Se eliminó el servicio " + perfilAd.nomservicio;
                int idUser = (int)Session["IdUser"];
                br.registroBitacora(cadena, idUser, "Eliminación");

                return RedirectToAction("Index", new { op = "Eliminar", user = nombre });
            }
            else if (op == "Actualizar")
            {
                TempData["usuarioActualizado"] = nuevoPerfil;

                var pUser = (from d in db.Servicios
                             where d.nomservicio == nuevoPerfil.nomservicio && d.contro != nuevoPerfil.contro && d.IdArea == nuevoPerfil.IdArea && d.IdTS == nuevoPerfil.IdTS
                             select d).FirstOrDefault();

                if (pUser != null)
                {
                    TempData["error"] = "Ya existe un servicio registrado con el mismo nombre";
                    return RedirectToAction("Index");
                }

                if (nuevoPerfil.costo == 0)
                {
                    TempData["error"] = "El costo del servicio debe ser mayor a cero";
                    return RedirectToAction("Index");
                }

                return RedirectToAction("ConfirmarServicio", new { op = "Confirmar", id = id });
            }
            else
            {
                if (ModelState.IsValid)
                {
                    nuevoPerfil = TempData["usuarioActualizado"] as Servicios;
                    db.Entry(nuevoPerfil).State = EntityState.Modified;
                    db.SaveChanges();


                    // Añadir proceso a bitácora de eventos
                    String cadena = "Se actualizó el servicio " + nuevoPerfil.nomservicio;
                    int idUser = (int)Session["IdUser"];

                    br.registroBitacora(cadena, idUser, "Actualización");

                    return RedirectToAction("Index", new { op = "Actualizar", user = nuevoPerfil.nomservicio });
                }
                return RedirectToAction("Index");
            }
        }
        
        [AuthorizeUser(nombreOperacion: "verservicios")]
        //Vista de solo lectura, servicios
        public ActionResult VerServicios()
        {
            cs.recuperarColores();

            ViewBag.IdArea = new SelectList(db.Areas, "IdArea", "nombrearea");
            ViewBag.IdTS = new SelectList(db.TipoServicios, "IdTS", "tipo");

            return View();
        }

        // Generar Reporte
        public ActionResult GenerarReporte(string estadoRep, string areaRep, string servicioRep)
        {
            //Validacion parametros
            if (estadoRep == "")
            {
                estadoRep = "TODOS";
            }


            if (servicioRep == "")
            {
                servicioRep = "TODOS";
            }
            else
            {
                TipoServicios pa = db.TipoServicios.Find(Convert.ToInt32(servicioRep));
                servicioRep = pa.tipo;
            }


            if (areaRep == "")
            {
                areaRep = "TODAS";
            }
            else
            {
                Areas ad = db.Areas.Find(Convert.ToInt32(areaRep));
                areaRep = ad.nombrearea;
            }


            var fechaArchivo = DateTime.Now.ToShortDateString();

            using (var workbook = new XLWorkbook())
            {
                var worksheet = workbook.Worksheets.Add("Servicios registrados");

                // Definir titulo largo
                worksheet.Cell("A1").Value = "Reporte de servicios registrados";
                var range = worksheet.Range("A1:K1");
                range.Merge().Style.Font.SetBold().Font.FontSize = 16;

                // Definir ficha
                worksheet.Cell("A2").Value = "REPORTE GENERADO";
                range = worksheet.Range("A2:B3");
                range.Merge().Style.Font.SetBold().Font.FontSize = 13;
                range.Merge().Style.Fill.BackgroundColor = XLColor.FromArgb(153, 20, 38);
                range.Merge().Style.Font.FontColor = XLColor.White;
                worksheet.Row(2).Height = 30;


                worksheet.Cell("C2").Value = "Generado por " + Session["NombreUser"] + " el día " + fechaArchivo + " con los parámetros: Estado de servicio - " + estadoRep + ", Tipo de servicio - " + servicioRep + ", Área - " + areaRep;
                range = worksheet.Range("C2:K3");
                range.Merge().Style.Font.FontSize = 12;

                // Impresión de datos
                DataTable table = new DataTable();
                table.TableName = "Servicios_Registrados";
                table.Columns.Add("ESTADO", typeof(string));
                table.Columns.Add("NOMBRE", typeof(string));
                table.Columns.Add("ÁREA", typeof(string));
                table.Columns.Add("TIPO DE SERVICIO", typeof(string));
                table.Columns.Add("OBJETIVO", typeof(string));
                table.Columns.Add("DURACIÓN", typeof(string));
                table.Columns.Add("PRERREQUISITOS", typeof(string));
                table.Columns.Add("DÍAS DE VIGENCIA", typeof(string));
                table.Columns.Add("COSTO", typeof(string));
                table.Columns.Add("MÁXIMO A COBRAR", typeof(string));
                table.Columns.Add("CUENTA CONTABLE", typeof(string));

                List<Servicios> usuarios = db.Servicios.ToList();
                bool valorEstado = true;
                string textValor;

                if (estadoRep != "TODOS")
                {
                    if (estadoRep == "ACTIVO")
                    {
                        valorEstado = true;
                    }
                    else
                    {
                        valorEstado = false;
                    }
                }

                foreach (Servicios us in usuarios)
                {
                    if (us.estado.ToString() == "True")
                    {
                        textValor = "ACTIVO";
                    }
                    else
                    {
                        textValor = "INACTIVO";
                    }

                    if (estadoRep == "TODOS")
                    {
                        if (servicioRep == "TODOS")
                        {
                            if (areaRep == "TODAS")
                            {
                                table.Rows.Add(textValor, us.nomservicio, us.Areas.nombrearea, us.TipoServicios.tipo, us.Objetivo, us.duracion, us.prerrequisitos, us.diasvigencia, us.costo, us.serviciosmaxacobrar, us.cuetacontable);
                            }
                            else
                            {
                                if (areaRep == us.Areas.nombrearea)
                                {
                                    table.Rows.Add(textValor, us.nomservicio, us.Areas.nombrearea, us.TipoServicios.tipo, us.Objetivo, us.duracion, us.prerrequisitos, us.diasvigencia, us.costo, us.serviciosmaxacobrar, us.cuetacontable);
                                }
                            }
                        }
                        else
                        {
                            if (servicioRep == us.TipoServicios.tipo)
                            {
                                if (areaRep == "TODAS")
                                {
                                    table.Rows.Add(textValor, us.nomservicio, us.Areas.nombrearea, us.TipoServicios.tipo, us.Objetivo, us.duracion, us.prerrequisitos, us.diasvigencia, us.costo, us.serviciosmaxacobrar, us.cuetacontable);
                                }
                                else
                                {
                                    if (areaRep == us.Areas.nombrearea)
                                    {
                                        table.Rows.Add(textValor, us.nomservicio, us.Areas.nombrearea, us.TipoServicios.tipo, us.Objetivo, us.duracion, us.prerrequisitos, us.diasvigencia, us.costo, us.serviciosmaxacobrar, us.cuetacontable);
                                    }
                                }
                            }
                        }
                    }
                    else
                    {
                        if (valorEstado == us.estado)
                        {
                            if (servicioRep == "TODOS")
                            {
                                if (areaRep == "TODAS")
                                {
                                    table.Rows.Add(textValor, us.nomservicio, us.Areas.nombrearea, us.TipoServicios.tipo, us.Objetivo, us.duracion, us.prerrequisitos, us.diasvigencia, us.costo, us.serviciosmaxacobrar, us.cuetacontable);
                                }
                                else
                                {
                                    if (areaRep == us.Areas.nombrearea)
                                    {
                                        table.Rows.Add(textValor, us.nomservicio, us.Areas.nombrearea, us.TipoServicios.tipo, us.Objetivo, us.duracion, us.prerrequisitos, us.diasvigencia, us.costo, us.serviciosmaxacobrar, us.cuetacontable);
                                    }
                                }
                            }
                            else
                            {
                                if (servicioRep == us.TipoServicios.tipo)
                                {
                                    if (areaRep == "TODAS")
                                    {
                                        table.Rows.Add(textValor, us.nomservicio, us.Areas.nombrearea, us.TipoServicios.tipo, us.Objetivo, us.duracion, us.prerrequisitos, us.diasvigencia, us.costo, us.serviciosmaxacobrar, us.cuetacontable);
                                    }
                                    else
                                    {
                                        if (areaRep == us.Areas.nombrearea)
                                        {
                                            table.Rows.Add(textValor, us.nomservicio, us.Areas.nombrearea, us.TipoServicios.tipo, us.Objetivo, us.duracion, us.prerrequisitos, us.diasvigencia, us.costo, us.serviciosmaxacobrar, us.cuetacontable);
                                        }
                                    }
                                }
                            }
                        }
                    }
                }

                worksheet.Cell("A4").InsertTable(table);
                worksheet.Table(0).Theme = XLTableTheme.None;

                // Estilos

                worksheet.Columns().Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center);
                worksheet.Columns().Style.Alignment.SetVertical(XLAlignmentVerticalValues.Center);
                worksheet.Columns().Style.Alignment.WrapText = true;
                worksheet.Columns().AdjustToContents();

                // Añadir proceso a bitácora de eventos
                String cadena = "Generó reporte de servicios";
                int idUser = (int)Session["IdUser"];

                br.registroBitacora(cadena, idUser, "Reportes");

                using (MemoryStream stream = new MemoryStream())
                {
                    workbook.SaveAs(stream);
                    return File(stream.ToArray(), "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "Rp_Servicios - " + fechaArchivo + ".xlsx");
                }
            }
        }
    }
}
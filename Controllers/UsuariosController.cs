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
    public class UsuariosController : Controller
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

        //Administrar usuarios
        [AuthorizeUser(nombreOperacion: "adminusuarios")]
        [HttpGet]
        public ActionResult Index(int? id, string op, string user)
        {
            if (TempData["error"] != null)
            {
                ViewBag.error = TempData["error"];
            }

            //Recupera colores almacenados en base de datos
            cs.recuperarColores();

            //Obtiene datos para combobox
            ViewBag.IdAreaAd = new SelectList(db.AreasAd, "IdAreaAd", "nombrearead");
            ViewBag.IdPerfil = new SelectList(db.PerfilesAd, "IdPerfil", "nombreperfil");
            ViewBag.op = op;
            ViewBag.user = user;

            //Muestra ventana con información de usuario si id tiene un valor
            if (id != null)
            {
                UsuariosAd usuario = db.UsuariosAd.Find(id);
                if (usuario == null)
                {
                    return HttpNotFound();
                }

                ViewBag.Name = db.UsuariosAd.ToList();
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

        public ActionResult JsonUsuarios()
        {
            List<TableUsuariosViewModel> lista = new List<TableUsuariosViewModel>();

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

                IQueryable<TableUsuariosViewModel> query = (from d in db.UsuariosAd
                                                           select new TableUsuariosViewModel
                                                           {
                                                               IdUsuario = d.IdUsuariosAd,
                                                               nombre = d.nombre,
                                                               iniciales = d.iniciales,
                                                               area = d.AreasAd.nombrearead,
                                                               perfil = d.PerfilesAd.nombreperfil,
                                                               estado = d.estado
                                                           });

                if (searchValue != "")
                    query = query.Where(d => d.nombre.Contains(searchValue) || d.iniciales.Contains(searchValue) || d.area.Contains(searchValue) || d.perfil.Contains(searchValue));

                if (!(string.IsNullOrEmpty(sortColumn) && string.IsNullOrEmpty(sortColumnDir)))
                {
                    query = query.OrderBy(sortColumn + " " + sortColumnDir);
                }

                recordsTotal = query.Count();

                lista = query.Skip(skip).Take(pageSize).ToList();
            }

            return Json(new { draw = draw, recordsFiltered = recordsTotal, recordsTotal = recordsTotal, data = lista });
        }

        [AuthorizeUser(nombreOperacion: "adminusuarios")]
        //Recibe paramétros para registrar un nuevo usuario
        [HttpPost]
        public ActionResult Index([ModelBinder(typeof(UsuariosBinder))] UsuariosAd usuario)
        {

            var pUser = (from d in db.UsuariosAd
                         where d.correoelectronico == usuario.correoelectronico
                         select d).FirstOrDefault();

            if(pUser != null){
                ViewBag.error = "Ya existe un usuario registrado con el mismo correo";

                //Recupera valores para combobox
                ViewBag.IdAreaAd = new SelectList(db.AreasAd, "IdAreaAd", "nombrearead");
                ViewBag.IdPerfil = new SelectList(db.PerfilesAd, "IdPerfil", "nombreperfil");

                //Recupera colores almacenados en base de datos
                cs.recuperarColores();

                return View();
            }

            if (ModelState.IsValid)
            {
                //Registrar usuario
                usuario.contrasena = Encrypt.GetSHA256(usuario.contrasena);

                db.UsuariosAd.Add(usuario);
                db.SaveChanges();

                // Añade proceso a bitácora de eventos
                String cadena = "Se registró el usuario " + usuario.nombre;
                int idUser = (int)Session["IdUser"];
                Eventos nuevoEvento = new Eventos
                {
                    fecha = DateTime.Now,
                    hora = DateTime.Now.ToString("hh:mm"),
                    operacion = "Registro",
                    descripcion = cadena,
                    IdUsuariosAd = idUser,
                    ip = Request.UserHostAddress
                };
                db.Eventos.Add(nuevoEvento);
                db.SaveChanges();
            }

            //Recupera valores para combobox
            ViewBag.IdAreaAd = new SelectList(db.AreasAd, "IdAreaAd", "nombrearead");
            ViewBag.IdPerfil = new SelectList(db.PerfilesAd, "IdPerfil", "nombreperfil");

            //Muestra página con mensaje de proceso finalizado
            return RedirectToAction("Index", new { op = "Agregar", user = usuario.nombre });
        }

        //ModeloBinder que crea el objeto Usuario para ser registrado
        public class UsuariosBinder : IModelBinder
        {
            public object BindModel(ControllerContext controllerContext, ModelBindingContext bindingContext)
            {
                //Obtiene valores de los campos que se encuentran en Administrar Usuarios
                HttpContextBase objContext = controllerContext.HttpContext;
                string cnombre = objContext.Request.Form["nombre"];
                string ciniciales = objContext.Request.Form["iniciales"];
                int cidperfil = Convert.ToInt32(objContext.Request.Form["IdPerfil"]);
                string ccorreo = objContext.Request.Form["correoelectronico"];
                string cestado = objContext.Request.Form["estado"];
                int cidarea = Convert.ToInt32(objContext.Request.Form["IdAreaAd"]);
                string cpass = objContext.Request.Form["contrasena"];

                bool estadofinal;

                if (cestado == "on")
                {
                    estadofinal = true;
                }
                else
                {
                    estadofinal = false;
                }

                UsuariosAd objUsuario = new UsuariosAd
                {
                    nombre = cnombre,
                    iniciales = ciniciales,
                    IdPerfil = cidperfil,
                    correoelectronico = ccorreo,
                    estado = estadofinal,
                    IdAreaAd = cidarea,
                    contrasena = cpass
                };

                //Devuelve objeto Usuario creado
                return objUsuario;
            }
        }

        // Ventana para mostrar datos de usuario
        [AuthorizeUser(nombreOperacion: "adminusuarios")]
        [HttpGet]
        public ActionResult ConfirmarUsuarios(string op, int id)
        {
            //Recupera colores de sistema
            cs.recuperarColores();

            //Obtiene usuario a mostrar y valores para combobox
            UsuariosAd usuario = db.UsuariosAd.Find(id);
            ViewBag.IdAreaAd = new SelectList(db.AreasAd, "IdAreaAd", "nombrearead");
            ViewBag.IdPerfil = new SelectList(db.PerfilesAd, "IdPerfil", "nombreperfil");
            ViewBag.msg = op;
            ViewBag.nombre = usuario.nombre;

            //Si se realizó una actualización de información se guardan cambios
            if (op == "Confirmar")
            {
                var actUser = TempData["usuarioActualizado"] as UsuariosAd;
                TempData["usuarioActualizado"] = TempData["usuarioActualizado"];
                return View(actUser);
            }

            return View(usuario);
        }

        [AuthorizeUser(nombreOperacion: "adminusuarios")]
        // Generar Reporte
        public ActionResult GenerarReporte( string estadoRep, string perfilRep, string areaRep)
        {
            //Validacion parametros
            if (estadoRep == "")
            {
                estadoRep = "TODOS";
            }
                

            if (perfilRep == "")
            {
                perfilRep = "TODOS";
            }
            else
            {
                PerfilesAd pa = db.PerfilesAd.Find(Convert.ToInt32(perfilRep));
                perfilRep = pa.nombreperfil;
            }
                

            if (areaRep == "")
            {
                areaRep = "TODAS";
            }
            else
            {
                AreasAd ad = db.AreasAd.Find(Convert.ToInt32(areaRep));
                areaRep = ad.nombrearead;
            }
                

            var fechaArchivo = DateTime.Now.ToShortDateString();

            using (var workbook = new XLWorkbook())
            {
                var worksheet = workbook.Worksheets.Add("Usuarios registrados");

                // Definir titulo largo
                worksheet.Cell("A1").Value = "Reporte de usuarios registrados";
                var range = worksheet.Range("A1:F1");
                range.Merge().Style.Font.SetBold().Font.FontSize = 16;

                // Definir ficha
                worksheet.Cell("A2").Value = "REPORTE GENERADO";
                range = worksheet.Range("A2:B3");
                range.Merge().Style.Font.SetBold().Font.FontSize = 13;
                range.Merge().Style.Fill.BackgroundColor = XLColor.FromArgb(153, 20, 38);
                range.Merge().Style.Font.FontColor = XLColor.White;
                worksheet.Row(2).Height = 30;
                

                worksheet.Cell("C2").Value = "Generado por " + Session["NombreUser"] + " el día " + fechaArchivo + " con los parámetros: Estado de usuario - " + estadoRep + ", Perfil - " + perfilRep + ", Área - " + areaRep;
                range = worksheet.Range("C2:F3");
                range.Merge().Style.Font.FontSize = 12;

                // Impresión de datos
                DataTable table = new DataTable();
                table.TableName = "Usuarios_Registrados";
                table.Columns.Add("ESTADO", typeof(string));
                table.Columns.Add("NOMBRE", typeof(string));
                table.Columns.Add("INICIALES", typeof(string));
                table.Columns.Add("CORREO ELECTRÓNICO", typeof(string));
                table.Columns.Add("PERFIL", typeof(string));
                table.Columns.Add("ÁREA ADMINISTRATIVA", typeof(string));

                List<UsuariosAd> usuarios = db.UsuariosAd.ToList();
                bool valorEstado=true;
                string textValor;

                if (estadoRep != "TODOS") { 
                    if(estadoRep == "ACTIVO")
                    {
                        valorEstado = true;
                    }
                    else
                    {
                        valorEstado= false;
                    }
                }

                foreach (UsuariosAd us in usuarios)
                {
                    if(us.estado.ToString() == "True")
                    {
                        textValor = "ACTIVO";
                    }
                    else
                    {
                        textValor = "INACTIVO";
                    }

                    if (estadoRep == "TODOS")
                    {
                        if (perfilRep == "TODOS")
                        {
                            if (areaRep == "TODAS")
                            {
                                table.Rows.Add(textValor, us.nombre, us.iniciales, us.correoelectronico, us.PerfilesAd.nombreperfil, us.AreasAd.nombrearead);
                            }
                            else
                            {
                                if (areaRep == us.AreasAd.nombrearead)
                                {
                                    table.Rows.Add(textValor, us.nombre, us.iniciales, us.correoelectronico, us.PerfilesAd.nombreperfil, us.AreasAd.nombrearead);
                                }
                            }
                        }
                        else
                        {
                            if (perfilRep == us.PerfilesAd.nombreperfil)
                            {
                                if (areaRep == "TODAS")
                                {
                                    table.Rows.Add(textValor, us.nombre, us.iniciales, us.correoelectronico, us.PerfilesAd.nombreperfil, us.AreasAd.nombrearead);
                                }
                                else
                                {
                                    if (areaRep == us.AreasAd.nombrearead)
                                    {
                                        table.Rows.Add(textValor, us.nombre, us.iniciales, us.correoelectronico, us.PerfilesAd.nombreperfil, us.AreasAd.nombrearead);
                                    }
                                }
                            }
                        }
                    }
                    else
                    {
                        if (valorEstado == us.estado)
                        {
                            if (perfilRep == "TODOS")
                            {
                                if (areaRep == "TODAS")
                                {
                                    table.Rows.Add(textValor, us.nombre, us.iniciales, us.correoelectronico, us.PerfilesAd.nombreperfil, us.AreasAd.nombrearead);
                                }
                                else
                                {
                                    if (areaRep == us.AreasAd.nombrearead)
                                    {
                                        table.Rows.Add(textValor, us.nombre, us.iniciales, us.correoelectronico, us.PerfilesAd.nombreperfil, us.AreasAd.nombrearead);
                                    }
                                }
                            }
                            else
                            {
                                if (perfilRep == us.PerfilesAd.nombreperfil)
                                {
                                    if (areaRep == "TODAS")
                                    {
                                        table.Rows.Add(textValor, us.nombre, us.iniciales, us.correoelectronico, us.PerfilesAd.nombreperfil, us.AreasAd.nombrearead);
                                    }
                                    else
                                    {
                                        if (areaRep == us.AreasAd.nombrearead)
                                        {
                                            table.Rows.Add(textValor, us.nombre, us.iniciales, us.correoelectronico, us.PerfilesAd.nombreperfil, us.AreasAd.nombrearead);
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

                using (MemoryStream stream = new MemoryStream())
                {
                    workbook.SaveAs(stream);
                    return File(stream.ToArray(), "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "Rp_Usuarios - " + fechaArchivo + ".xlsx");
                }
            }
        }

        [AuthorizeUser(nombreOperacion: "adminusuarios")]
        //Realiza operación, eliminar o actualizar
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ConfirmarUsuarios(int id, string op, [Bind(Include = "IdUsuariosAd,nombre,iniciales,IdPerfil,correoelectronico,estado,IdAreaAd,contrasena")] UsuariosAd nuevoUsuario)
        {
            if (op == "Eliminar")
            {
                UsuariosAd usuariosAd = db.UsuariosAd.Find(id);
                var nombre = usuariosAd.nombre;

                if(usuariosAd.PerfilesAd.nombreperfil == "ADMINISTRADOR")
                {
                    var usuariosAdmin = db.UsuariosAd.Where(u => u.PerfilesAd.nombreperfil == "ADMINISTRADOR").ToList();

                    if (usuariosAdmin.Count == 1)
                    {
                        TempData["error"] = "Debe existir al menos un ADMINISTRADOR en el sistema";
                        return RedirectToAction("Index");
                    }
                }

                UsuariosAd usuarioYo = Session["User"] as UsuariosAd;

                if(usuarioYo.IdUsuariosAd == usuariosAd.IdUsuariosAd)
                {
                    TempData["error"] = "No puedes eliminar tu propio usuario del sistema";
                    return RedirectToAction("Index");
                }

                db.UsuariosAd.Remove(usuariosAd);


                try
                {
                    db.SaveChanges();
                }catch (Exception ex)
                {
                    TempData["error"] = "Existen registros con este usuario asignado";
                    return RedirectToAction("Index");
                }

                // Añadir proceso a bitácora de eventos
                String cadena = "Se eliminó el usuario " + usuariosAd.nombre;
                int idUser = (int)Session["IdUser"];
                Eventos nuevoEvento = new Eventos
                {
                    fecha = DateTime.Now,
                    hora = DateTime.Now.ToString("hh:mm"),
                    operacion = "Eliminación",
                    descripcion = cadena,
                    IdUsuariosAd = idUser,
                    ip = Request.UserHostAddress
                };
                db.Eventos.Add(nuevoEvento);
                db.SaveChanges();

                return RedirectToAction("Index", new { op = "Eliminar", user = nombre });
            }
            else if (op == "Actualizar")
            {
                TempData["usuarioActualizado"] = nuevoUsuario;

                var pUser = (from d in db.UsuariosAd
                             where d.correoelectronico == nuevoUsuario.correoelectronico && d.IdUsuariosAd != nuevoUsuario.IdUsuariosAd
                             select d).FirstOrDefault();

                if(pUser != null)
                {
                    TempData["error"] = "Ya existe un usuario registrado con el mismo correo";
                    return RedirectToAction("Index");
                }

                UsuariosAd nw = Session["User"] as UsuariosAd;
                if (nuevoUsuario.IdUsuariosAd == nw.IdUsuariosAd && nuevoUsuario.estado == false)
                {
                    TempData["error"] = "No puedes inhabilitar tu propio usuario";
                    return RedirectToAction("Index");
                }

                return RedirectToAction("ConfirmarUsuarios", new { op = "Confirmar", id = id });
            }
            else
            {
                if (ModelState.IsValid)
                {
                    nuevoUsuario = TempData["usuarioActualizado"] as UsuariosAd;

                    nuevoUsuario.contrasena = Encrypt.GetSHA256(nuevoUsuario.contrasena);

                    db.Entry(nuevoUsuario).State = EntityState.Modified;
                    db.SaveChanges();

                    // Añadir proceso a bitácora
                    String cadena = "Se actualizó el usuario " + nuevoUsuario.nombre;
                    int idUser = (int)Session["IdUser"];
                    Eventos nuevoEvento = new Eventos
                    {
                        fecha = DateTime.Now,
                        hora = DateTime.Now.ToString("hh:mm"),
                        operacion = "Actualización",
                        descripcion = cadena,
                        IdUsuariosAd = idUser,
                        ip = Request.UserHostAddress
                    };
                    db.Eventos.Add(nuevoEvento);
                    db.SaveChanges();

                    return RedirectToAction("Index", new { op = "Actualizar", user = nuevoUsuario.nombre });
                }
                return RedirectToAction("Index");
            }
        }
    }
}

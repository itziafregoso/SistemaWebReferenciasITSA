using DocumentFormat.OpenXml.Office2010.Excel;
using SistemaWeb.Data;
using SistemaWeb.Filters;
using SistemaWeb.Models;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Text;
using System.Linq.Dynamic;
using System.Web;
using System.Web.Mvc;
using SistemaWeb.Models.LINQModels;
using SistemaWeb.Models.ViewModels;
using System.Data;

namespace SistemaWeb.Controllers
{
    public class AlumnosController : Controller
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

        [AuthorizeUser(nombreOperacion: "adminalumnos")]
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
            ViewBag.op = op;
            ViewBag.user = user;

            //Muestra ventana con información de usuario si id tiene un valor
            if (id != null)
            {
                Clientes usuario = db.Clientes.Find(id);
                if (usuario == null)
                {
                    return HttpNotFound();
                }

                ViewBag.Name = db.Clientes.ToList();
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

        public ActionResult JsonAlumnos()
        {
            List<TableAlumnosViewModel> lista = new List<TableAlumnosViewModel>();

            var draw = Request.Form.GetValues("draw").FirstOrDefault();
            var start = Request.Form.GetValues("start").FirstOrDefault();
            var length = Request.Form.GetValues("length").FirstOrDefault();
            var sortColumn = Request.Form.GetValues("columns[" + Request.Form.GetValues("order[0][column]").FirstOrDefault() + "][name]").FirstOrDefault();
            var sortColumnDir = Request.Form.GetValues("order[0][dir]").FirstOrDefault();
            var searchValue = Request.Form.GetValues("search[value]").FirstOrDefault();

            pageSize = length != null ? Convert.ToInt32(length) : 0;
            skip = start != null ? Convert.ToInt32(start) : 0;
            recordsTotal = 0;

            using (DataBase db = new DataBase()) {                 

                IQueryable<TableAlumnosViewModel> query = (from d in db.Clientes
                                                           where d.matricula != null
                        select new TableAlumnosViewModel
                        {
                            IdCliente = d.IdCliente,
                            nombre_ = d.nombre_ + " " + d.apellidos,
                            matricula = d.matricula
                        });

                if (searchValue != "")
                    query = query.Where(d => d.nombre_.Contains(searchValue) || d.matricula.Contains(searchValue));

                if (!(string.IsNullOrEmpty(sortColumn) && string.IsNullOrEmpty(sortColumnDir)))
                {
                    query = query.OrderBy(sortColumn + " " + sortColumnDir);
                }

                recordsTotal = query.Count();

                lista = query.Skip(skip).Take(pageSize).ToList();
            }

            return Json(new {draw =draw, recordsFiltered = recordsTotal, recordsTotal = recordsTotal, data = lista});
        }

        //Recibe paramétros para registrar un nuevo alumno
        [AuthorizeUser(nombreOperacion: "adminalumnos")]
        [HttpPost]
        public ActionResult Index([ModelBinder(typeof(AlumnosBinder))] Clientes usuario)
        {

            var pUser = (from d in db.Clientes
                         where d.matricula== usuario.matricula
                         select d).FirstOrDefault();

            if (pUser != null)
            {
                ViewBag.error = "Ya existe un alumno registrado con la misma matrícula";

                //Recupera colores almacenados en base de datos
                cs.recuperarColores();

                return View();
            }

            if (ModelState.IsValid)
            {
                db.Clientes.Add(usuario);
                db.SaveChanges();

                // Añade proceso a bitácora de eventos
                String cadena = "Se registró el alumno " + usuario.nombre_;
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

            //Muestra página con mensaje de proceso finalizado
            return RedirectToAction("Index", new { op = "Agregar", user = usuario.nombre_ });
        }

        public class AlumnosBinder : IModelBinder
        {
            public object BindModel(ControllerContext controllerContext, ModelBindingContext bindingContext)
            {
                //Obtiene valores de los campos que se encuentran en Administrar Usuarios
                HttpContextBase objContext = controllerContext.HttpContext;
                string anombre = objContext.Request.Form["nombre"];
                string aapellidos = objContext.Request.Form["apellidos"];
                string amatricula = objContext.Request.Form["matricula"];
                string acorreo = objContext.Request.Form["correoelectronico"];
                string acalle = objContext.Request.Form["calle"];
                string anoext = objContext.Request.Form["noext"];
                string anoint = objContext.Request.Form["noint"];
                string acolonia = objContext.Request.Form["colonia"];
                string acp = objContext.Request.Form["cp"];
                string aciudad = objContext.Request.Form["ciudad"];
                string aestado = objContext.Request.Form["estado"];

                Clientes objUsuario = new Clientes
                {
                    nombre_ = anombre,
                    apellidos = aapellidos,
                    matricula = amatricula,
                    correoelectronico = acorreo,
                    calle = acalle,
                    numeroex = anoext,
                    numeroin = anoint,
                    colonia = acolonia,
                    cp = acp,
                    ciudad = aciudad,
                    estado = aestado,
                    tipopersona = "FÍSICA",
                    rfc_ = null
                };

                //Devuelve objeto Usuario creado
                return objUsuario;
            }
        }

        [AuthorizeUser(nombreOperacion: "adminalumnos")]
        [HttpGet]
        public ActionResult ConfirmarAlumnos(string op, int id)
        {
            //Recupera colores de sistema
            cs.recuperarColores();

            //Obtiene usuario a mostrar y valores para combobox
            Clientes usuario = db.Clientes.Find(id);
            ViewBag.msg = op;
            ViewBag.nombre = usuario.nombre_;

            //Si se realizó una actualización de información se guardan cambios
            if (op == "Confirmar")
            {
                var actUser = TempData["usuarioActualizado"] as Clientes;
                TempData["usuarioActualizado"] = TempData["usuarioActualizado"];
                return View(actUser);
            }

            return View(usuario);
        }

        //Realiza operación, eliminar o actualizar
        [AuthorizeUser(nombreOperacion: "adminalumnos")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ConfirmarAlumnos(int id, string op, 
            [Bind(Include = "IdCliente,nombre_,apellidos,matricula,correoelectronico,calle,numeroex,numeroin,colonia,cp,ciudad,estado,tipopersona,rfc_")] Clientes nuevoUsuario)
        {
            if (op == "Eliminar")
            {
                Clientes usuariosAd = db.Clientes.Find(id);
                var nombre = usuariosAd.nombre_;
                db.Clientes.Remove(usuariosAd);

                try
                {
                    db.SaveChanges();
                }
                catch (Exception ex)
                {
                    TempData["error"] = "Existen registros con este alumno asignado";
                    return RedirectToAction("Index");
                }

                // Añadir proceso a bitácora de eventos
                String cadena = "Se eliminó el alumno " + usuariosAd.nombre_;
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

                var pUser = (from d in db.Clientes
                             where d.matricula == nuevoUsuario.matricula && d.IdCliente != nuevoUsuario.IdCliente
                             select d).FirstOrDefault();

                if (pUser != null)
                {
                    TempData["error"] = "Ya existe un usuario registrado con la misma matrícula";
                    return RedirectToAction("Index");
                }

                return RedirectToAction("ConfirmarAlumnos", new { op = "Confirmar", id = id });
            }
            else
            {
                if (ModelState.IsValid)
                {
                    nuevoUsuario = TempData["usuarioActualizado"] as Clientes;
                    db.Entry(nuevoUsuario).State = EntityState.Modified;
                    db.SaveChanges();

                    // Añadir proceso a bitácora
                    String cadena = "Se actualizó el usuario " + nuevoUsuario.nombre_;
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

                    return RedirectToAction("Index", new { op = "Actualizar", user = nuevoUsuario.nombre_ });
                }
                return RedirectToAction("Index");
            }
        }


        [AuthorizeUser(nombreOperacion: "subiralumnos")]
        [HttpGet]
        public void Exportar()
        {
            List<LINQClientes> lista = null;
            using (DataBase db = new DataBase())
            {
                lista = (from d in db.Clientes
                         where d.matricula =="PlantillaPrueba"
                         select new LINQClientes
                         {
                             matricula = d.matricula,
                             nombre_ = d.nombre_,
                             apellidos = d.apellidos,
                             correoelectronico = d.correoelectronico,
                             calle = d.calle,
                             numeroex = d.numeroex,
                             numeroin= d.numeroin,
                             colonia= d.colonia,
                             cp = d.cp,
                             ciudad = d.ciudad,
                             estado = d.estado,
                         }).ToList();
            }

            StringWriter sw = new StringWriter();

            sw.WriteLine("\"Matrícula\",\"Nombre\",\"Apellidos\",\"Correo\",\"Calle\",\"Número exterior\",\"Número interior\",\"Colonia\",\"CP\",\"Ciudad\",\"Estado\"");
            Response.ClearContent();
            Response.ContentEncoding = Encoding.Unicode;
            Response.AddHeader("content-disposition", "attachment;filename=AlumnosFormato.csv");
            Response.ContentType = "text/csv;";

            foreach (var alumnos in lista)
            {
                sw.WriteLine(String.Format("\"{0}\",\"{1}\",\"{2}\",\"{3}\",\"{4}\",\"{5}\",\"{6}\",\"{7}\",\"{8}\",\"{9}\"",
                    alumnos.matricula,
                    alumnos.nombre_,
                    alumnos.apellidos,
                    alumnos.correoelectronico,
                    alumnos.calle,
                    alumnos.numeroex,
                    alumnos.numeroin,
                    alumnos.colonia,
                    alumnos.cp,
                    alumnos.ciudad,
                    alumnos.estado
                    ));
            }

            Response.Write(sw.ToString());
            Response.End();
        }

        [AuthorizeUser(nombreOperacion: "subiralumnos")]
        [HttpGet]
        public ActionResult Carga()
        {
            cs.recuperarColores();

            return View();
        }

        [AuthorizeUser(nombreOperacion: "subiralumnos")]
        [HttpPost]
        public ActionResult Carga(HttpPostedFileBase archivoAlumnos)
        {
            cs.recuperarColores();
            string resultado = "";


            List<Clientes> lista = new List<Clientes>();
            string filePath = string.Empty;
            if (archivoAlumnos != null)
            {
                string path = Server.MapPath("~/Uploads/");
                if (!Directory.Exists(path))
                {
                    Directory.CreateDirectory(path);
                }
                filePath = path + Path.GetFileName(archivoAlumnos.FileName);
                string extension = Path.GetExtension(archivoAlumnos.FileName);
                archivoAlumnos.SaveAs(filePath);

                string csvData = Regex.Replace(System.IO.File.ReadAllText(filePath, Encoding.GetEncoding("iso-8859-1")), @"[\0]+", "");

                int conteo = 0;

                foreach (string row in csvData.Split('\n'))
                {
                    if (conteo != 0)
                    {
                        if (!string.IsNullOrEmpty(row))
                        {
                            lista.Add(new Clientes
                            {
                                tipopersona = "FÍSICA",
                                matricula = (row.Split(',')[0]).ToString().Replace("\"", "").Replace(@"\\", "").ToUpper(),
                                nombre_ = (row.Split(',')[1]).ToString().Replace("\"", "").Replace(@"\\", "").ToUpper(),
                                apellidos = (row.Split(',')[2]).ToString().Replace("\"", "").Replace(@"\\", "").ToUpper(),
                                correoelectronico = (row.Split(',')[3]).ToString().Replace("\"", "").Replace(@"\\", ""),
                                calle = (row.Split(',')[4]).ToString().Replace("\"", "").Replace(@"\\", "").ToUpper(),
                                numeroex = (row.Split(',')[5]).ToString().Replace("\"", "").Replace(@"\\", "").ToUpper(),
                                numeroin = (row.Split(',')[6]).ToString().Replace("\"", "").Replace(@"\\", "").ToUpper(),
                                colonia = (row.Split(',')[7]).ToString().Replace("\"", "").Replace(@"\\", "").ToUpper(),
                                cp = (row.Split(',')[8]).ToString().Replace("\"", "").Replace(@"\\", ""),
                                ciudad = (row.Split(',')[9]).ToString().Replace("\"", "").Replace(@"\\", "").ToUpper(),
                                estado = (row.Split(',')[10]).ToString().Replace("\"", "").Replace(@"\\", "").ToUpper(),
                            });                            
                        }
                    }
                    conteo++;
                }

                using (DataBase db = new DataBase())
                {
                    int cuenta = 2;
                    foreach (Clientes alumnoRegistrado in lista)
                    {
                        var alumnoEncontrado = db.Clientes.Where(d => d.matricula == alumnoRegistrado.matricula).FirstOrDefault();
                        if (alumnoEncontrado != null)
                        {
                            ViewBag.error = "Existe un alumno ya registrado con la misma matrícula. Línea " + cuenta + ".";
                            return View();
                        }
                        cuenta++;
                    }
                }

                string nombre = archivoAlumnos.FileName.ToString();

                // Añadir proceso a bitácora de eventos
                String cadena = "Se subió el archivo " + nombre;
                int idUser = (int)Session["IdUser"];

                br.registroBitacora(cadena, idUser, "Registro");

                TempData["listaAlumno"] = lista;
                return RedirectToAction("Subir");
            }

            ViewBag.error = "No se encontraron registros en el archivo cargado.";
            return View();
        }

        [AuthorizeUser(nombreOperacion: "subiralumnos")]
        [HttpGet]
        public ActionResult Subir()
        {
            List<Clientes> listaAlumno = TempData["listaAlumno"] as List<Clientes>;
            cs.recuperarColores();

            ViewBag.alumnosTotal = listaAlumno.Count;
            TempData["listaAlumnado"] = listaAlumno;

            // Genera reporte de incidencias
            ViewBag.Operacion = "Reporte";
            return View();
        }

        [AuthorizeUser(nombreOperacion: "subiralumnos")]
        [HttpPost]        
        public ActionResult Subir(List<Clientes> listaAlumnos)
        {
            List<Clientes> listaAlumno = TempData["listaAlumnado"] as List<Clientes>;
            cs.recuperarColores();

            //Cargar alumnos
            using(DataBase db = new DataBase())
            {
                foreach(Clientes alumno in listaAlumno)
                {
                    db.Clientes.Add(alumno);
                }
                db.SaveChanges();
            }

            return RedirectToAction("Index", "Perfil");
        }

        [AuthorizeUser(nombreOperacion: "veralumnos")]
        [HttpGet]
        public ActionResult VerAlumnos(int? id, string op, string user)
        {
            cs.recuperarColores();

            return View();
        }


    }
}
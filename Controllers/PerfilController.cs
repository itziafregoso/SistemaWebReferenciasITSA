using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using SistemaWeb.Models;
using SistemaWeb.Repositories;
using SistemaWeb.Filters;
using System.IO;
using ClosedXML.Excel;
using SistemaWeb.Data;
using System.Linq.Dynamic;
using SistemaWeb.Models.ViewModels;
using DocumentFormat.OpenXml.Spreadsheet;

namespace SistemaWeb.Controllers
{
    public class PerfilController : Controller
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

        // Muestra panel principal de usuario
        public ActionResult Index()
        {
            //Recupera colores almacenados en base de datos
            cs.recuperarColores();

            Session["UsuariosActivos"] = db.UsuariosAd.Count();
            Session["ServiciosReg"] = db.Servicios.Count();
            Session["ReferenciaGen"] = db.Referencias.Count();

            return View();
        }

        public ActionResult JsonReferencias()
        {
            List<TableReferenciasViewModel> lista = new List<TableReferenciasViewModel>();

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

                IQueryable<TableReferenciasViewModel> query = (from d in db.Referencias
                                                           select new TableReferenciasViewModel
                                                           {
                                                               noReferencia = d.numref,
                                                               estado = d.estadoref,
                                                               fechaEmision = d.fechaemision,
                                                               fechaEstado= d.fechaestado,
                                                               fechaVencimiento = d.fechavencimiento,
                                                               monto = d.monto,
                                                               cliente = d.Clientes.nombre_,

                                                           });

                if (searchValue != "")
                    query = query.Where(d => d.noReferencia.Contains(searchValue) || d.estado.Contains(searchValue));

                if (!(string.IsNullOrEmpty(sortColumn) && string.IsNullOrEmpty(sortColumnDir)))
                {
                    query = query.OrderBy(sortColumn + " " + sortColumnDir);
                }

                recordsTotal = query.Count();

                lista = query.Skip(skip).Take(pageSize).ToList();
            }

            return Json(new { draw = draw, recordsFiltered = recordsTotal, recordsTotal = recordsTotal, data = lista });
        }

        public ActionResult JsonProductos(string id)
        {
            List<TableComprasViewModel> lista = new List<TableComprasViewModel>();

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

                IQueryable<TableComprasViewModel> query = (from d in db.Ventas
                                                               where d.numref == id
                                                               select new TableComprasViewModel
                                                               {
                                                                   producto = d.Servicios.nomservicio,
                                                                   cantidad = d.cantidad,
                                                                   monto = (d.costount * d.cantidad)
                                                               });

                if (!(string.IsNullOrEmpty(sortColumn) && string.IsNullOrEmpty(sortColumnDir)))
                {
                    query = query.OrderBy(sortColumn + " " + sortColumnDir);
                }

                recordsTotal = query.Count();

                lista = query.Skip(skip).Take(pageSize).ToList();
            }

            return Json(new { draw = draw, recordsFiltered = recordsTotal, recordsTotal = recordsTotal, data = lista });
        }

        [AuthorizeUser(nombreOperacion: "verreferencias")]
        [HttpGet]
        public ActionResult VerReferencias(string id, string op)
        {
            cs.recuperarColores();
            ViewBag.op = op;

            if (id != null)
            {
                Referencias usuario = db.Referencias.Find(id);
                if (usuario == null)
                {
                    return HttpNotFound();
                }

                ViewBag.Name = db.Referencias.ToList();
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

        [AuthorizeUser(nombreOperacion: "verreferencias")]
        [HttpGet]
        public ActionResult ConfirmarReferencia(string id, string op)
        {
            cs.recuperarColores();
            //Obtiene usuario a mostrar y valores para combobox
            Referencias usuario = db.Referencias.Find(id);

            if (op == "Eliminar")
            {
                if(usuario == null)
                {
                    return RedirectToAction("VerReferencias");
                }

                if(usuario.estadoref != "Emitida")
                {
                    if (usuario.estadoref != "EMITIDA")
                    {
                        return RedirectToAction("VerReferencias");
                    }
                }          
            }

            ViewBag.msg = op;
            ViewBag.nombre = usuario.numref;

            return View(usuario);
        }

        [AuthorizeUser(nombreOperacion: "verreferencias")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ConfirmarReferencia(string id, string op, string msg)
        {
            if (op == "Eliminar")
            {
                Referencias usuariosAd = db.Referencias.Find(id);
                var nombre = usuariosAd.numref;

                usuariosAd.estadoref = "CANCELADA";
                usuariosAd.fechaestado = DateTime.Now;
                try
                {
                    db.SaveChanges();
                }
                catch (Exception ex)
                {
                    return RedirectToAction("VerReferencias");
                }

                // Añadir proceso a bitácora de eventos
                String cadena = "Se canceló la referencia " + usuariosAd.numref;
                int idUser = (int)Session["IdUser"];

                br.registroBitacora(cadena, idUser, "Cancelación");

                return RedirectToAction("VerReferencias", new { op = "Eliminar", user = nombre });
            }
            else
            {
                return RedirectToAction("VerReferencias");
            }
        }

        [AuthorizeUser(nombreOperacion: "bitacoraeventos")]
        public ActionResult JsonBitacora()
        {
            List<TableBitacoraViewModel> lista = new List<TableBitacoraViewModel>();

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

                IQueryable<TableBitacoraViewModel> query = (from d in db.Eventos
                                                               select new TableBitacoraViewModel
                                                               {
                                                                   fecha = d.fecha,
                                                                   hora = d.hora,
                                                                   operacion = d.operacion,
                                                                   descripcion = d.descripcion,
                                                                   usuario = d.UsuariosAd.nombre,
                                                                   ip = d.ip
                                                               });

                if (searchValue != "")
                    query = query.Where(d => d.usuario.Contains(searchValue) || d.ip.Contains(searchValue) || d.operacion.Contains(searchValue) || d.descripcion.Contains(searchValue));

                if (!(string.IsNullOrEmpty(sortColumn) && string.IsNullOrEmpty(sortColumnDir)))
                {
                    query = query.OrderBy(sortColumn + " " + sortColumnDir);
                }

                recordsTotal = query.Count();

                lista = query.Skip(skip).Take(pageSize).ToList();
            }

            return Json(new { draw = draw, recordsFiltered = recordsTotal, recordsTotal = recordsTotal, data = lista });
        }

        [AuthorizeUser(nombreOperacion: "bitacoraeventos")]
        public ActionResult BitacoraEventos()
        {
            cs.recuperarColores();

            return View();
        }

        [AuthorizeUser(nombreOperacion: "adminparametros")]
        [HttpGet]
        public ActionResult ParametrosSistema(int? id)
        {
            cs.recuperarColores();
            cs.recuperarParametros();

            if (id != null)
            {
                DiasHabiles diaHabil = db.DiasHabiles.Find(id);
                if (diaHabil == null)
                {
                }
                else {
                    db.DiasHabiles.Remove(diaHabil);
                    db.SaveChanges();
                }
            }

            return View();
        }

        [AuthorizeUser(nombreOperacion: "adminparametros")]
        [HttpPost]
        public ActionResult ParametrosSistema(DateTime? fechaHabil, string operacion,
            string colorPrimario, string colorPrimarioAl, string colorSecundario,
            string colorTitulos, string colorTexto,
            string colorBVer, string colorBVerAl, string colorBEditar, string colorBEditarAl, string colorBEliminar, string colorBEliminarAl,
            string colorBComprar, string colorBComprarAl,
            string numCuenta, string cuenClave, string nomBuzon, string constanteRef, string numRap)
        {
            if (operacion == "registrarDiaHabil")
            {

                if (fechaHabil == null)
                {
                    ViewBag.error = "Debe añadir una fecha para ser registrada";
                    return View();
                }

                var pUser = (from d in db.DiasHabiles
                             where d.fechaHabil == fechaHabil.Value
                             select d).FirstOrDefault();

                if (pUser != null)
                {
                    ViewBag.error = "La fecha ingresada ya está registrada";
                    return View();
                }

                DiasHabiles diaHabil = new DiasHabiles {
                    fechaHabil = (fechaHabil.Value).Date
                };

                db.DiasHabiles.Add(diaHabil);
                db.SaveChanges();

                // Añadir proceso a bitácora de eventos
                String cadena = "Se registró el día hábil " + diaHabil;
                int idUser = (int)Session["IdUser"];

                br.registroBitacora(cadena, idUser, "Registro");

            } else if (operacion == "cambiarColor")
            {
                OpcionesSistema vOP = db.OpcionesSistema.FirstOrDefault();

                vOP.colorPrimario = colorPrimario;
                vOP.colorPrimarioAl = colorPrimarioAl;
                vOP.colorSecundario = colorSecundario;
                vOP.colorTitulos = colorTitulos;
                vOP.colorTexto = colorTexto;
                vOP.colorBVer = colorBVer;
                vOP.colorBVerAl = colorBVerAl;
                vOP.colorBEditar = colorBEditar;
                vOP.colorBEditarAl = colorBEditarAl;
                vOP.colorBEliminar = colorBEliminar;
                vOP.colorBEliminarAl = colorBEliminarAl;
                vOP.colorBComprar = colorBComprar;
                vOP.colorBComprarAl = colorBComprarAl;
                db.SaveChanges();

                // Añadir proceso a bitácora de eventos
                String cadena = "Se actualizaron los colores del sistema";
                int idUser = (int)Session["IdUser"];

                br.registroBitacora(cadena, idUser, "Actualización");

            } else if (operacion == "parametrosSistema")
            {
                if (numCuenta.Length != 10)
                {
                    ViewBag.error = "El número de cuenta debe contener 10 dígitos";
                    return View();
                }

                if (cuenClave.Length != 18)
                {
                    ViewBag.error = "La cuenta CLABE debe contener 18 dígitos";
                    return View();
                }

                if (numRap.Length != 4)
                {
                    ViewBag.error = "El número RAP debe contener 4 dígitos";
                    return View();
                }

                if (!numCuenta.All(char.IsDigit))
                {
                    ViewBag.error = "El número de cuenta debe contener solo números";
                    return View();
                }

                if (!cuenClave.All(char.IsDigit))
                {
                    ViewBag.error = "La cuenta CLABE debe contener solo números";
                    return View();
                }

                if (!constanteRef.All(char.IsDigit))
                {
                    ViewBag.error = "La constante de referencia debe ser un número";
                    return View();
                }

                if (!numRap.All(char.IsDigit))
                {
                    ViewBag.error = "El número RAP debe contener solo números";
                    return View();
                }

                OpcionesSistema vOP = db.OpcionesSistema.FirstOrDefault();
                vOP.numCuenta = numCuenta;
                vOP.cuenClave = cuenClave;
                vOP.nomBuzon = nomBuzon;
                vOP.constanteRef = constanteRef;
                vOP.numRap = numRap;
                db.SaveChanges();

                // Añadir proceso a bitácora de eventos
                String cadena = "Se actualizaron los parámetros del sistema";
                int idUser = (int)Session["IdUser"];

                br.registroBitacora(cadena, idUser, "Actualización");

            }

            return RedirectToAction("ParametrosSistema");
        }

        [AuthorizeUser(nombreOperacion: "adminparametros")]
        public ActionResult JsonHorarios()
        {
            List<TableHorariosViewModel> lista = new List<TableHorariosViewModel>();

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

                IQueryable<TableHorariosViewModel> query = (from d in db.DiasHabiles
                                                           select new TableHorariosViewModel
                                                           {
                                                               IdFecha = d.Id,
                                                               fecha = d.fechaHabil
                                                           });

                if (!(string.IsNullOrEmpty(sortColumn) && string.IsNullOrEmpty(sortColumnDir)))
                {
                    query = query.OrderBy(sortColumn + " " + sortColumnDir);
                }

                recordsTotal = query.Count();

                lista = query.Skip(skip).Take(pageSize).ToList();
            }

            return Json(new { draw = draw, recordsFiltered = recordsTotal, recordsTotal = recordsTotal, data = lista });
        }

        [AuthorizeUser(nombreOperacion: "subirarchivo")]
        [HttpGet]
        public ActionResult ArchivoPago()
        {
            cs.recuperarColores();

            return View();
        }

        [AuthorizeUser(nombreOperacion: "subirarchivo")]
        [HttpPost]
        public ActionResult ArchivoPago(HttpPostedFileBase archivoTXT)
        {
            cs.recuperarColores();
            string resultado = "";

            try
            {
                //Obtengo la data del archivo
                resultado = new StreamReader(archivoTXT.InputStream).ReadToEnd();
            }
            catch (Exception ex)
            {
                ViewBag.mensaje = "No se ha podido cargar el archivo correctamente";
                return View();
            }

            string nombre = archivoTXT.FileName.ToString();

            //Identificar Nombre
            string[] subs = nombre.Split('-');
            string fechaFormato;

            if (nombre.Substring(nombre.Length - 3, 3) != "txt")
            {
                ViewBag.mensaje = "Solo se admiten archivos en formato .txt";
                return View();
            }
            else
            {
                try
                {
                    fechaFormato = subs[1].Substring(6, 2) + "/" + subs[1].Substring(4, 2) + "/" + subs[1].Substring(0, 4);
                    string fechaActual = (DateTime.Now).ToShortDateString();

                    if(subs[0].Substring(0,3) != "RHD" || subs[0].Substring(subs[0].Length - 3, 3) != "RHD")
                    {
                        ViewBag.mensaje = "El layout debe ser tipo RHD";
                        return View();
                    }

                    OpcionesSistema op = db.OpcionesSistema.FirstOrDefault();

                    string digCuenta = op.numCuenta.Substring(op.numCuenta.Length - 4,4);

                    if(digCuenta != subs[0].Substring(3, 4))
                    {
                        ViewBag.mensaje = "El número de cuenta no corresponde al registrado en el sistema";
                        return View();
                    }

                    if(subs[0].Substring(7,3) != "_MX")
                    {
                        ViewBag.mensaje = "El país en la nomenclatura del archivo no es el correspondiente";
                        return View();
                    }

                    string buzonDato = op.nomBuzon.ToString();

                    if(subs[0].Substring(10,buzonDato.Length) != buzonDato)
                    {
                        ViewBag.mensaje = "El nombre de buzón no corresponde al registrado en el sistema";
                        return View();
                    }

                    if(subs[1].Substring(8,1) != "_")
                    {
                        ViewBag.mensaje = "El nombre del archivo no corresponde con la nomenclatura estándar";
                        return View();
                    }

                    if (!subs[1].Substring(9, 9).All(char.IsDigit))
                    {
                        ViewBag.mensaje = "El nombre del archivo no corresponde con la nomenclatura estándar";
                        return View();
                    }

                    if (fechaFormato == fechaActual)
                    {
                        ViewBag.mensaje = "El layout no es vigente. Cargar un layout más reciente";
                        return View();
                    }
                    else
                    {
                        if(Convert.ToDateTime(fechaFormato) >= Convert.ToDateTime(fechaActual))
                        {
                            ViewBag.mensaje = "La fecha del formato no es válida. Cargar un layout correcto";
                            return View();
                        }
                        TempData["fechaFormato"] = fechaFormato;
                    }
                }
                catch (Exception ex)
                {
                    ViewBag.mensaje = "El nombre del archivo no corresponde con la nomenclatura estándar";
                    return View();
                }
            }

            // Separar lineas
            string[] referencias = resultado.Split('\n');

            List<string> ccheque = new List<string>();
            List<string> fecha = new List<string>();
            List<string> importe = new List<string>();
            List<string> tipotrans = new List<string>();
            List<string> referencia = new List<string>();

            //Eliminar lineas en blanco
            referencias = referencias.Where(val => val != "" && val != "\r").ToArray();

            if(referencias.Length > 0)
            {
                //Separa referencias parámetros
                for (var i = 0; i < referencias.Length; i++)
                {
                    //Guarda valores eliminando espacios en blanco
                    ccheque.Add(referencias[i].Substring(0, 10).Replace(" ", String.Empty));
                    fecha.Add(referencias[i].Substring(10, 8).Replace(" ", String.Empty));
                    importe.Add(referencias[i].Substring(29, 13).Replace(" ", String.Empty));
                    tipotrans.Add(referencias[i].Substring(42, 2).Replace(" ", String.Empty));
                    referencia.Add(referencias[i].Substring(74, 40).Replace(" ", String.Empty));
                }
            }
            else
            {
                ViewBag.mensaje = "No se encontraron referencias";
                return View();
            }


            TempData["ref"] = referencia.ToList();
            TempData["ccheque"] = ccheque.ToList();
            TempData["fecha"] = fecha.ToList();
            TempData["importe"] = importe.ToList();
            TempData["tipotrans"] = tipotrans.ToList();
            TempData["nombreArchivo"] = nombre;


            // Añadir proceso a bitácora de eventos
            String cadena = "Se subió el archivo " + nombre;
            int idUser = (int)Session["IdUser"];

            br.registroBitacora(cadena, idUser, "Registro");

            return RedirectToAction("ConciliarReferencias");
        }

        [AuthorizeUser(nombreOperacion: "subirarchivo")]
        [HttpGet]
        public ActionResult ConciliarReferencias()
        {
            cs.recuperarColores();

            // Recuperar listas almacenadas
            List<String> referencia = TempData["ref"] as List<String>;
            List<String> cchque = TempData["ccheque"] as List<String>;
            List<String> fecha = TempData["fecha"] as List<String>;
            List<String> importe = TempData["importe"] as List<String>;
            List<String> tipotrans = TempData["tipotrans"] as List<String>;
            TempData["nombreArchivo"] = TempData["nombreArchivo"];
            String fechaFormato = TempData["fechaFormato"] as String;

            TempData["fechaFormato"] = fechaFormato;

            // Listas para almacenar errores
            List<String> refError = new List<string>();
            List<String> tipoError = new List<string>();

            List<String> referenciasLectura = new List<string>();

            //Validar referencia
            int contadorPos = 0; //Contador para elemento lista
            try
            {
                OpcionesSistema op = db.OpcionesSistema.FirstOrDefault();
                string monto;
                foreach (String unicaRef in referencia)
                {
                    if (referenciasLectura.Contains(unicaRef))
                    {
                        // La referencia no coincide con importe
                        refError.Add(unicaRef);
                        tipoError.Add("La referencia se encuentra duplicada en el layout");
                    }


                    if(cchque[contadorPos] == op.numCuenta)
                    {
                        // Evalua que sea un abono
                        if (tipotrans[contadorPos] == "CR")
                        {

                            //Busca la referencia en BD
                            var pRef = (from d in db.Referencias
                                        where d.numref == unicaRef
                                        select d).FirstOrDefault();

                            //Evalua parámetros
                            if (pRef != null)
                            {
                                if (pRef.estadoref == "EMITIDA")
                                {
                                    string fechaCadena = fecha[contadorPos].Substring(6, 2) + "/" + fecha[contadorPos].Substring(4, 2) + "/" + fecha[contadorPos].Substring(0, 4);
                                    DateTime fechaReg = Convert.ToDateTime(fechaCadena);
                                    if (fechaReg <= pRef.fechavencimiento)
                                    {
                                        if(fechaCadena == fechaFormato)
                                        {
                                            monto = importe[contadorPos].Substring(0, importe[contadorPos].Length - 2) + "." + importe[contadorPos].Substring(importe[contadorPos].Length - 2, 2);
                                            if (Convert.ToDecimal(monto) != pRef.monto)
                                            {
                                                // La referencia no coincide con importe
                                                refError.Add(unicaRef);
                                                tipoError.Add("El importe no es igual al generado en el sistema");
                                            }
                                        }
                                        else
                                        {
                                            refError.Add(unicaRef);
                                            tipoError.Add("La fecha del layout no corresponde a la fecha de la referencia");
                                        }
                                    }
                                    else
                                    {
                                        // La referencia fue pagada tiempo después
                                        refError.Add(unicaRef);
                                        tipoError.Add("La referencia fue pagada después del tiempo vigente");
                                    }
                                }
                                else
                                {
                                    if (pRef.estadoref == "CADUCA")
                                    {
                                        // La referencia es caduca
                                        refError.Add(unicaRef);
                                        tipoError.Add("La referencia fue marcada como caduca anteriormente");
                                    }
                                    else if (pRef.estadoref == "CONCILIADA")
                                    {
                                        // La referencia es conciliada
                                        refError.Add(unicaRef);
                                        tipoError.Add("La referencia fue marcada como conciliada anteriormente");
                                    }
                                    else if (pRef.estadoref == "CANCELADA")
                                    {
                                        // La referencia es cancelada
                                        refError.Add(unicaRef);
                                        tipoError.Add("La referencia fue marcada como cancelada anteriormente");
                                    }
                                }
                            }
                            else
                            {
                                // No se encontró la referencia
                                refError.Add(unicaRef);
                                tipoError.Add("No se encontró la referencia");
                            }
                        }
                        else
                        {
                            // Se registró un cargo
                            refError.Add(unicaRef);
                            tipoError.Add("La referencia fue registrada como cargo");
                        }
                    }
                    else
                    {
                        // Num cuenta no es igual
                        refError.Add(unicaRef);
                        tipoError.Add("El número de cuenta no es igual al registrado en sistema");
                    }

                    contadorPos = contadorPos + 1;
                    referenciasLectura.Add(unicaRef);
                }
            }catch (Exception ex)
            {
                return RedirectToAction("ArchivoPago");
            }

            TempData["refError"] = refError;
            TempData["tipoError"] = tipoError;
            TempData["refTotal"] = referencia;
            TempData["importe"] = importe;
            TempData["fecha"] = fecha;

            ViewBag.refError = refError;
            ViewBag.tipoError = tipoError;
            ViewBag.refTotal = referencia;

            if(referencia == null)
            {
                return RedirectToAction("ArchivoPago");
            }


            // Define si realizar una póliza de pago o reporte de incidencias
            if (refError.Count <= 0)
            {
                // Genera póliza de pago
                ViewBag.Operacion = "Poliza";

                return View();
            }
            else
            {
                // Genera reporte de incidencias
                ViewBag.Operacion = "Reporte";
                return View();
            }
        }

        // Método POST
        [AuthorizeUser(nombreOperacion: "subirarchivo")]
        [HttpPost]
        public ActionResult ConciliarReferencias(string operador, string operacion = "Final")
        {
            cs.recuperarColores();

            ViewBag.refError = TempData["refError"];
            ViewBag.tipoError = TempData["tipoError"];
            ViewBag.refTotal = TempData["refTotal"];
            ViewBag.nombreArchivo = TempData["nombreArchivo"];

            TempData["refTotal"] = TempData["refTotal"];
            TempData["importe"] = TempData["importe"];
            TempData["fecha"] = TempData["fecha"];
            TempData["refError"] = TempData["refError"];
            TempData["tipoError"] = TempData["tipoError"];

            Session["refError"] = null;
            Session["tipoError"] = null;
            Session["refTotal"] = null;
            Session["importe"] = null;

            ViewBag.Operador = operador;

            if(operador == "Poliza")
            {
                List<String> referencia = TempData["refTotal"] as List<String>;
                List<String> fecha = TempData["fecha"] as List<String>;

                TempData["refTotal"] = referencia;
                int contador = 0;

                foreach (string r in referencia)
                {
                    string fechaCadena = fecha[contador].Substring(6, 2) + "/" + fecha[contador].Substring(4, 2) + "/" + fecha[contador].Substring(0, 4);

                    Referencias rf = db.Referencias.Find(r);
                    rf.fechaestado = Convert.ToDateTime(fechaCadena); ;
                    rf.estadoref = "CONCILIADA";

                    db.SaveChanges();
                    contador++;
                }

                String fechaFormato = TempData["fechaFormato"] as string;
                DateTime fechaVencimiento = Convert.ToDateTime(fechaFormato);

                List<Referencias> refCad = db.Referencias.Where(u => u.estadoref == "EMITIDA" && u.fechavencimiento <= fechaVencimiento).ToList();
                contador = 0;

                foreach (Referencias r in refCad)
                {
                    string fechaCadena = fecha[contador].Substring(6, 2) + "/" + fecha[contador].Substring(4, 2) + "/" + fecha[contador].Substring(0, 4);
                    Referencias rf = db.Referencias.Find(r.numref);
                    rf.fechaestado = Convert.ToDateTime(fechaCadena);
                    rf.estadoref = "CADUCA";

                    db.SaveChanges();
                }

                // Añadir proceso a bitácora de eventos
                String cadena = "Se conciliaron referencias";
                int idUser = (int)Session["IdUser"];

                br.registroBitacora(cadena, idUser, "Conciliación");
            }
            else
            {
                // Añadir proceso a bitácora de eventos
                String cadena = "Se generó reporte de incidencias";
                int idUser = (int)Session["IdUser"];

                br.registroBitacora(cadena, idUser, "Incidencias");
            }

            ViewBag.Operacion = "Final";
            return View();
        }

        [HttpGet]
        public ActionResult MiPerfil()
        {
            cs.recuperarColores();

            UsuariosAd miUsuario = Session["User"] as UsuariosAd;

            AreasAd area = db.AreasAd.Find(miUsuario.IdAreaAd);

            ViewBag.mNombre = miUsuario.nombre;
            ViewBag.mPerfil = miUsuario.PerfilesAd.nombreperfil;
            ViewBag.mArea = area.nombrearead;
            ViewBag.mCorreo = miUsuario.correoelectronico;
            ViewBag.mInicial = miUsuario.iniciales;

            return View();
        } 

        [HttpPost]
        public ActionResult MiPerfil(string contrasena, string confirmarcontrasena)
        {
            UsuariosAd miUsuario = Session["User"] as UsuariosAd;

            var pUser = (from d in db.UsuariosAd
                         where d.IdUsuariosAd == miUsuario.IdUsuariosAd
                         select d).FirstOrDefault();

            contrasena = Encrypt.GetSHA256(contrasena);

            if (pUser.contrasena == contrasena)
            {
                if (contrasena == confirmarcontrasena)
                {
                    ViewBag.error = "La nueva contraseña no puede ser igual a la actual";
                    return View();
                }
                else
                {                   
                    UsuariosAd userActual = db.UsuariosAd.Find(miUsuario.IdUsuariosAd);
                    userActual.contrasena = Encrypt.GetSHA256(confirmarcontrasena);
                    db.SaveChanges();
                }
            }
            else
            {
                ViewBag.error = "La contraseña actual es incorrecta";
                return View();
            }


            return RedirectToAction("MiPerfil");
        }

        [AuthorizeUser(nombreOperacion: "subirarchivo")]
        public ActionResult ReporteIncidencias()
        {
            if (Session["refError"] == null)
            {
                Session["refError"] = TempData["refError"];
            }

            if (Session["tipoError"] == null)
            {
                Session["tipoError"] = TempData["tipoError"];
            }

            List<String> refError = Session["refError"] as List<String>;
            List<String> tipoError = Session["tipoError"] as List<String>;

            using (var workbook = new XLWorkbook())
            {
                var worksheet = workbook.Worksheets.Add("Reporte de incidencias");
                worksheet.Cell("A1").Value = "Referencias";
                int col = 2;
                string celda = "A" + col;
                foreach (var item in refError)
                {
                    worksheet.Cell(celda).Value = "'" + item;
                    col++;
                    celda = "A" + col;
                }

                worksheet.Cell("B1").Value = "Tipo de error";
                col = 2;
                celda = "B" + col;
                foreach (var item in tipoError)
                {
                    worksheet.Cell(celda).Value = "'" + item;
                    col++;
                    celda = "B" + col;
                }

                String dt = DateTime.Now.ToShortDateString();

                worksheet.Columns().Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center);
                worksheet.Columns().AdjustToContents();

                using (MemoryStream stream = new MemoryStream())
                {
                    workbook.SaveAs(stream);
                    return File(stream.ToArray(), "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "Reporte Incidencias - " + dt + ".xlsx");
                }
            }
        }

        [AuthorizeUser(nombreOperacion: "subirarchivo")]
        public ActionResult PolizaPago()
        {
            if (Session["refTotal"] == null)
            {
                Session["refTotal"] = TempData["refTotal"];
            }

            if (Session["importe"] == null)
            {
                Session["importe"] = TempData["importe"];
            }

            List<String> referencia = Session["refTotal"] as List<String>;
            List<String> importe = Session["importe"] as List<String>;

            using (var workbook = new XLWorkbook())
            {
                var worksheet = workbook.Worksheets.Add("Póliza de pago");
                worksheet.SheetView.ZoomScale = 145;

                /*ENCABEZADO*/
                worksheet.Range("A1:D1");
                worksheet.Range("A1:D1").Merge().Value = "SISTEMA AUTOMATIZADO DE ADMINISTRACION Y CONTABILIDAD GUBERNAMENTAL SAACG.NET";
                worksheet.Range("A1:D1").Merge().Style.Fill.BackgroundColor = XLColor.FromHtml("#244062");
                worksheet.Range("A1:D1").Merge().Style.Font.FontColor = XLColor.White;
                worksheet.Range("A1:D1").Merge().Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                worksheet.Range("A1:D1").Merge().Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                worksheet.Range("A1:D1").Merge().Style.Font.FontName = "Arial Black";
                worksheet.Range("A1:D1").Merge().Style.Font.FontSize = 11;
                worksheet.Row(1).Height = 32.25;
                worksheet.Row(2).Height = 17.25;
                worksheet.Row(3).Height = 17.25;
                worksheet.Row(4).Height = 17.25;
                worksheet.Row(5).Height = 17.25;
                worksheet.Row(6).Height = 17.25;
                worksheet.Row(7).Height = 17.25;
                worksheet.Column("A").Width = 24.71;
                worksheet.Column("B").Width = 15.43;
                worksheet.Column("C").Width = 15.14;
                worksheet.Column("D").Width = 70;


                // CABECERA fecha
                worksheet.Cell("A2").Style.Fill.BackgroundColor = XLColor.FromHtml("#8DB4E2");
                worksheet.Cell("A2").Style.Font.Bold = true;
                worksheet.Cell("A2").Style.Font.FontName = "Arial";
                worksheet.Cell("A2").Style.Font.FontSize = 9;
                worksheet.Cell("A2").Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;
                worksheet.Cell("A2").Style.Alignment.Vertical = XLAlignmentVerticalValues.Bottom;
                worksheet.Cell("A2").Value = "FECHA:";

                // DATOS fecha                
                worksheet.Cell("B2").Style.Font.FontName = "Courier";
                worksheet.Cell("B2").Style.Font.FontSize = 10;
                worksheet.Cell("B2").Style.Font.Bold = true;
                worksheet.Cell("B2").Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;
                worksheet.Cell("B2").Style.Alignment.Vertical = XLAlignmentVerticalValues.Bottom;
                worksheet.Cell("B2").Value = DateTime.Now.ToShortDateString();

                // CABECERA tipo
                worksheet.Cell("A3").Style.Fill.BackgroundColor = XLColor.FromHtml("#8DB4E2");
                worksheet.Cell("A3").Style.Font.Bold = true;
                worksheet.Cell("A3").Style.Font.FontName = "Arial";
                worksheet.Cell("A3").Style.Font.FontSize = 9;
                worksheet.Cell("A3").Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;
                worksheet.Cell("A3").Style.Alignment.Vertical = XLAlignmentVerticalValues.Bottom;
                worksheet.Cell("A3").Value = "TIPO DE POLIZA:";

                // DATOS tipo         
                worksheet.Cell("B3").Style.Font.FontName = "Courier";
                worksheet.Cell("B3").Style.Font.FontSize = 10;
                worksheet.Cell("B3").Style.Font.Bold = true;
                worksheet.Cell("B3").Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;
                worksheet.Cell("B3").Style.Alignment.Vertical = XLAlignmentVerticalValues.Bottom;
                worksheet.Cell("B3").Value = "D";

                // CABECERA cheque
                worksheet.Cell("A4").Style.Fill.BackgroundColor = XLColor.FromHtml("#8DB4E2");
                worksheet.Cell("A4").Style.Font.Bold = true;
                worksheet.Cell("A4").Style.Font.FontName = "Arial";
                worksheet.Cell("A4").Style.Font.FontSize = 9;
                worksheet.Cell("A4").Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;
                worksheet.Cell("A4").Style.Alignment.Vertical = XLAlignmentVerticalValues.Bottom;
                worksheet.Cell("A4").Value = "NO. CHEQUE:";

                // CABECERA concepto
                worksheet.Cell("A5").Style.Fill.BackgroundColor = XLColor.FromHtml("#8DB4E2");
                worksheet.Cell("A5").Style.Font.Bold = true;
                worksheet.Cell("A5").Style.Font.FontName = "Arial";
                worksheet.Cell("A5").Style.Font.FontSize = 9;
                worksheet.Cell("A5").Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;
                worksheet.Cell("A5").Style.Alignment.Vertical = XLAlignmentVerticalValues.Bottom;
                worksheet.Cell("A5").Value = "CONCEPTO:";

                // DATOS tipo
                worksheet.Range("B5:D5");
                worksheet.Range("B5:D5").Merge().Style.Font.FontName = "Courier";
                worksheet.Range("B5:D5").Merge().Style.Font.FontSize = 10;
                worksheet.Range("B5:D5").Merge().Style.Font.Bold = true;
                worksheet.Range("B5:D5").Merge().Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;
                worksheet.Range("B5:D5").Merge().Style.Alignment.Vertical = XLAlignmentVerticalValues.Bottom;
                worksheet.Range("B5:D5").Merge().Value = "Deposito del día";

                // CABECERA concepto
                worksheet.Cell("A6").Style.Fill.BackgroundColor = XLColor.FromHtml("#8DB4E2");
                worksheet.Cell("A6").Style.Font.Bold = true;
                worksheet.Cell("A6").Style.Font.FontName = "Arial";
                worksheet.Cell("A6").Style.Font.FontSize = 9;
                worksheet.Cell("A6").Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;
                worksheet.Cell("A6").Style.Alignment.Vertical = XLAlignmentVerticalValues.Bottom;
                worksheet.Cell("A6").Value = "BENEFICIARIO:";

                // DATOS beneficiario
                worksheet.Range("B6:D6");
                worksheet.Range("B6:D6").Merge().Style.Font.FontName = "Courier";
                worksheet.Range("B6:D6").Merge().Style.Font.FontSize = 10;
                worksheet.Range("B6:D6").Merge().Style.Font.Bold = true;
                worksheet.Range("B6:D6").Merge().Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;
                worksheet.Range("B6:D6").Merge().Style.Alignment.Vertical = XLAlignmentVerticalValues.Bottom;
                worksheet.Range("B6:D6").Merge().Value = "Instituto Tecnológico Superior de Atlixco";

                // CABECERA sumas iguales
                worksheet.Cell("A7").Style.Fill.BackgroundColor = XLColor.FromHtml("#8DB4E2");
                worksheet.Cell("A7").Style.Font.Bold = true;
                worksheet.Cell("A7").Style.Font.FontName = "Arial";
                worksheet.Cell("A7").Style.Font.FontSize = 9;
                worksheet.Cell("A7").Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;
                worksheet.Cell("A7").Style.Alignment.Vertical = XLAlignmentVerticalValues.Bottom;
                worksheet.Cell("A7").Value = "SUMAS IGUALES";

                // DATOS sumas iguales A         
                worksheet.Cell("B7").Style.Font.FontName = "Courier";
                worksheet.Cell("B7").Style.Font.FontSize = 10;
                worksheet.Cell("B7").Style.Font.Bold = true;
                worksheet.Cell("B7").Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;
                worksheet.Cell("B7").Style.Alignment.Vertical = XLAlignmentVerticalValues.Bottom;
                worksheet.Cell("B7").DataType = XLDataType.Number;
                worksheet.Cell("B7").FormulaA1 = "=SUM(B9:B" + (referencia.Count + 9) + ")";

                // DATOS sumas iguales B
                worksheet.Cell("C7").Style.Font.FontName = "Courier";
                worksheet.Cell("C7").Style.Font.FontSize = 10;
                worksheet.Cell("C7").Style.Font.Bold = true;
                worksheet.Cell("C7").Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;
                worksheet.Cell("C7").Style.Alignment.Vertical = XLAlignmentVerticalValues.Bottom;
                worksheet.Cell("C7").DataType = XLDataType.Number;
                worksheet.Cell("C7").FormulaA1 = "=SUM(C9:C" + (referencia.Count + 9) + ")";

                // CABECERA cuenta
                worksheet.Cell("A8").Style.Fill.BackgroundColor = XLColor.FromHtml("#8DB4E2");
                worksheet.Cell("A8").Style.Font.Bold = true;
                worksheet.Cell("A8").Style.Font.FontName = "Arial";
                worksheet.Cell("A8").Style.Font.FontSize = 10;
                worksheet.Cell("A8").Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;
                worksheet.Cell("A8").Style.Alignment.Vertical = XLAlignmentVerticalValues.Bottom;
                worksheet.Cell("A8").Value = "CUENTA";

                // CABECERA cargo
                worksheet.Cell("B8").Style.Fill.BackgroundColor = XLColor.FromHtml("#8DB4E2");
                worksheet.Cell("B8").Style.Font.Bold = true;
                worksheet.Cell("B8").Style.Font.FontName = "Arial";
                worksheet.Cell("B8").Style.Font.FontSize = 10;
                worksheet.Cell("B8").Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                worksheet.Cell("B8").Style.Alignment.Vertical = XLAlignmentVerticalValues.Bottom;
                worksheet.Cell("B8").Value = "CARGO";

                // CABECERA abono
                worksheet.Cell("C8").Style.Fill.BackgroundColor = XLColor.FromHtml("#8DB4E2");
                worksheet.Cell("C8").Style.Font.Bold = true;
                worksheet.Cell("C8").Style.Font.FontName = "Arial";
                worksheet.Cell("C8").Style.Font.FontSize = 10;
                worksheet.Cell("C8").Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                worksheet.Cell("C8").Style.Alignment.Vertical = XLAlignmentVerticalValues.Bottom;
                worksheet.Cell("C8").Value = "ABONO";

                // CABECERA concepto
                worksheet.Cell("D8").Style.Fill.BackgroundColor = XLColor.FromHtml("#8DB4E2");
                worksheet.Cell("D8").Style.Font.Bold = true;
                worksheet.Cell("D8").Style.Font.FontName = "Arial";
                worksheet.Cell("D8").Style.Font.FontSize = 10;
                worksheet.Cell("D8").Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                worksheet.Cell("D8").Style.Alignment.Vertical = XLAlignmentVerticalValues.Bottom;
                worksheet.Cell("D8").Value = "CONCEPTO POR MOVIMIENTO";

                int col = 9;
                string celda = "D" + col;
                foreach (var item in referencia)
                {
                    worksheet.Cell("D" + col).Value = "'" + 123;
                    worksheet.Cell(celda).Value = "'" + item;
                    worksheet.Cell(celda).Style.Font.FontName = "Courier";
                    worksheet.Cell(celda).Style.Font.FontSize = 10;
                    worksheet.Cell(celda).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;
                    worksheet.Cell(celda).Style.Alignment.Vertical = XLAlignmentVerticalValues.Bottom;
                    col++;
                    celda = "D" + col;
                }

                col = 9;
                celda = "B" + col;
                foreach (var item in importe)
                {
                    worksheet.Cell(celda).Value = (item.Substring(0,item.Length-2))+"."+(item.Substring(item.Length -2,2));
                    worksheet.Cell(celda).Style.Font.FontName = "Courier";
                    worksheet.Cell(celda).Style.Font.FontSize = 10;
                    worksheet.Cell(celda).DataType = XLDataType.Number;
                    worksheet.Cell(celda).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;
                    worksheet.Cell(celda).Style.Alignment.Vertical = XLAlignmentVerticalValues.Bottom;
                    col++;
                    celda = "B" + col;
                }

                String dt = DateTime.Now.ToShortDateString();


                using (MemoryStream stream = new MemoryStream())
                {
                    workbook.SaveAs(stream);
                    return File(stream.ToArray(), "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "Póliza Pago - " + dt + ".xlsx");
                }
            }
        }

        // Genera Ficha de pago en PDF
        public ActionResult GenerarPDF(string id, string estado)
        {
            TempData["tipoPDF"] = estado;

            if (estado == "CONCILIADA")
            {
                List<Ventas> newmodel = db.Ventas.Where(u => u.numref == id).ToList();

                List<Servicios> carrito = new List<Servicios>();

                foreach (Ventas v in newmodel)
                {
                    Servicios c = db.Servicios.Find(v.contro);
                    c.costo = Math.Round(v.costount, 2);                    

                    carrito.Add(c);
                }

                ViewBag.Carrito = carrito;

                ViewBag.Referencia = id;

                decimal total = 0;
                if (carrito != null)
                {
                    foreach (var valor in carrito)
                    {
                        total = total + Convert.ToDecimal(valor.costo);
                    }

                    total = decimal.Round(total, 2);
                }

                RepositoryProducto repo = new RepositoryProducto();
                List<Servicios> prod = repo.GetProductos();

                ViewBag.lista = prod;

                ViewBag.Total = total;

                Referencias rf = db.Referencias.Find(id);

                ViewBag.Vigencia = rf.fechaestado.ToShortDateString();
                ViewBag.Folio = rf.numref.Substring(0, 7);

                Referencias refAc = db.Referencias.Find(id);
                Clientes clienteHoy = db.Clientes.Find(refAc.IdCliente);
                OpcionesSistema op = db.OpcionesSistema.FirstOrDefault();

                ViewBag.NombreCliente = clienteHoy.nombre_ + " " + clienteHoy.apellidos;

                if (clienteHoy.rfc_ != "" && clienteHoy.rfc_ != null)
                {
                    ViewBag.rfcMat = clienteHoy.rfc_;
                }
                else
                {
                    ViewBag.rfcMat = clienteHoy.matricula;
                }

                ViewBag.codVer = (rf.fechavencimiento.ToShortDateString()).Replace("/", "") + rf.numref.Substring(0, 7) + ((decimal.Round(rf.monto, 2)).ToString()).Replace(".","") + (rf.fechaemision.ToShortDateString()).Replace("/", "");

                return new Rotativa.ViewAsPdf("GenerarPDF") { FileName = "Comprobante de pago.pdf" };

            }
            else
            {
                List<Ventas> newmodel = db.Ventas.Where(u => u.numref == id).ToList();

                List<Servicios> carrito = new List<Servicios>();

                foreach (Ventas v in newmodel)
                {
                    Servicios c = db.Servicios.Find(v.contro);
                    c.costo = Math.Round(v.costount, 2);

                    carrito.Add(c);
                }

                ViewBag.Carrito = carrito;

                ViewBag.Referencia = id;

                decimal total = 0;
                if (carrito != null)
                {
                    foreach (var valor in carrito)
                    {
                        total = total + Convert.ToDecimal(valor.costo);
                    }

                    total = decimal.Round(total, 2);
                }

                RepositoryProducto repo = new RepositoryProducto();
                List<Ventas> prod = db.Ventas.ToList();

                ViewBag.lista = prod;

                ViewBag.Total = total;

                Referencias rf = db.Referencias.Find(id);

                ViewBag.Vigencia = rf.fechavencimiento.ToShortDateString();
                ViewBag.Folio = rf.numref.Substring(0,7);

                Referencias refAc = db.Referencias.Find(id);
                Clientes clienteHoy = db.Clientes.Find(refAc.IdCliente);
                OpcionesSistema op = db.OpcionesSistema.FirstOrDefault();

                ViewBag.NombreCliente = clienteHoy.nombre_ + " " + clienteHoy.apellidos;

                if (clienteHoy.rfc_ != "" && clienteHoy.rfc_ != null)
                {
                    ViewBag.rfcMat = clienteHoy.rfc_;
                }
                else
                {
                    ViewBag.rfcMat = clienteHoy.matricula;
                }

                ViewBag.RAP = op.numRap;
                ViewBag.CLABE = op.cuenClave;

                //return View();

                return new Rotativa.ViewAsPdf("GenerarPDF") { FileName = "OrdenDeCobro.pdf" };
            }
        }

        [AuthorizeUser(nombreOperacion: "verreferencias")]
        // Generar Reporte
        public ActionResult ReporteReferencia(string estadoRep, string fechaInicio, string fechaFinal)
        {
            //Validacion parametros
            if (estadoRep == "")
            {
                estadoRep = "TODAS";
            }

            var fechaArchivo = DateTime.Now.ToShortDateString();

            using (var workbook = new XLWorkbook())
            {
                var worksheet = workbook.Worksheets.Add("Referencias registradas");

                // Definir titulo largo
                worksheet.Cell("A1").Value = "Reporte de referencias registradas";
                var range = worksheet.Range("A1:S1");
                range.Merge().Style.Font.SetBold().Font.FontSize = 16;

                // Definir ficha
                worksheet.Cell("A2").Value = "REPORTE GENERADO";
                range = worksheet.Range("A2:B3");
                range.Merge().Style.Font.SetBold().Font.FontSize = 13;
                range.Merge().Style.Fill.BackgroundColor = XLColor.FromArgb(153, 20, 38);
                range.Merge().Style.Font.FontColor = XLColor.White;
                worksheet.Row(2).Height = 30;


                worksheet.Cell("C2").Value = "Generado por " + Session["NombreUser"] + " el día " + fechaArchivo + " con los parámetros: Estado de referencia - " + estadoRep + ", Fecha inicio - " + fechaInicio + ", Fecha fin - " + fechaFinal;
                range = worksheet.Range("C2:S3");
                range.Merge().Style.Font.FontSize = 12;

                // Impresión de datos
                DataTable table = new DataTable();
                table.TableName = "Referencias_Registradas";
                table.Columns.Add("ESTADO DE REFERENCIA", typeof(string));
                table.Columns.Add("REFERENCIA", typeof(string));
                table.Columns.Add("CUENTA CONTABLE", typeof(string));
                table.Columns.Add("CONCEPTO", typeof(string));
                table.Columns.Add("FECHA DE EMISIÓN", typeof(string));
                table.Columns.Add("FECHA DE VENCIMIENTO", typeof(string));
                table.Columns.Add("FECHA DE ESTADO", typeof(string));
                table.Columns.Add("MONTO", typeof(decimal));
                table.Columns.Add("NOMBRE DE CLIENTE", typeof(string));
                table.Columns.Add("APELLIDOS DE CLIENTE", typeof(string));
                table.Columns.Add("MATRICULA", typeof(string));
                table.Columns.Add("RFC", typeof(string));
                table.Columns.Add("TIPO DE PERSONA", typeof(string));
                table.Columns.Add("CORREO ELECTRÓNICO", typeof(string));
                table.Columns.Add("CALLE DOMICILIO", typeof(string));
                table.Columns.Add("NUMERO EXTERIOR DOMICILIO", typeof(string));
                table.Columns.Add("NUMERO INTERIOR DOMICILIO", typeof(string));
                table.Columns.Add("COLONIA", typeof(string));
                table.Columns.Add("CODIGO POSTAL", typeof(string));
                table.Columns.Add("CIUDAD", typeof(string));
                table.Columns.Add("ESTADO", typeof(string));

                List<Ventas> usuarios = db.Ventas.ToList();

                foreach (Ventas us in usuarios)
                {
                    if (estadoRep == "TODAS")
                    {
                        if (fechaInicio == "")
                        {
                            if (fechaFinal == "")
                            {
                                table.Rows.Add(us.Referencias.estadoref, us.numref,us.Servicios.cuetacontable, us.Servicios.nomservicio, us.Referencias.fechaemision.ToShortDateString(), us.Referencias.fechavencimiento.ToShortDateString(), us.Referencias.fechaestado.ToShortDateString(), Math.Round((us.costount * us.cantidad),2), us.Referencias.Clientes.nombre_, us.Referencias.Clientes.apellidos, 
                                    us.Referencias.Clientes.matricula, us.Referencias.Clientes.rfc_, us.Referencias.Clientes.tipopersona, us.Referencias.Clientes.correoelectronico, us.Referencias.Clientes.calle, us.Referencias.Clientes.numeroex, us.Referencias.Clientes.numeroin, us.Referencias.Clientes.colonia, us.Referencias.Clientes.cp,
                                    us.Referencias.Clientes.ciudad, us.Referencias.Clientes.estado);
                            }
                            else
                            {
                                if (Convert.ToDateTime(fechaFinal) >= Convert.ToDateTime(us.Referencias.fechaemision.ToShortDateString()))
                                {
                                    table.Rows.Add(us.Referencias.estadoref, us.numref, us.Servicios.cuetacontable, us.Servicios.nomservicio, us.Referencias.fechaemision.ToShortDateString(), us.Referencias.fechavencimiento.ToShortDateString(), us.Referencias.fechaestado.ToShortDateString(), Math.Round((us.costount * us.cantidad), 2), us.Referencias.Clientes.nombre_, us.Referencias.Clientes.apellidos,
                                        us.Referencias.Clientes.matricula, us.Referencias.Clientes.rfc_, us.Referencias.Clientes.tipopersona, us.Referencias.Clientes.correoelectronico, us.Referencias.Clientes.calle, us.Referencias.Clientes.numeroex, us.Referencias.Clientes.numeroin, us.Referencias.Clientes.colonia, us.Referencias.Clientes.cp,
                                        us.Referencias.Clientes.ciudad, us.Referencias.Clientes.estado);
                                }
                            }
                        }
                        else
                        {
                            if (Convert.ToDateTime(fechaInicio) <= Convert.ToDateTime(us.Referencias.fechaemision.ToShortDateString()))
                            {
                                if (fechaFinal == "")
                                {
                                    table.Rows.Add(us.Referencias.estadoref, us.numref, us.Servicios.cuetacontable, us.Servicios.nomservicio, us.Referencias.fechaemision.ToShortDateString(), us.Referencias.fechavencimiento.ToShortDateString(), us.Referencias.fechaestado.ToShortDateString(), Math.Round((us.costount * us.cantidad), 2), us.Referencias.Clientes.nombre_, us.Referencias.Clientes.apellidos,
                                        us.Referencias.Clientes.matricula, us.Referencias.Clientes.rfc_, us.Referencias.Clientes.tipopersona, us.Referencias.Clientes.correoelectronico, us.Referencias.Clientes.calle, us.Referencias.Clientes.numeroex, us.Referencias.Clientes.numeroin, us.Referencias.Clientes.colonia, us.Referencias.Clientes.cp,
                                        us.Referencias.Clientes.ciudad, us.Referencias.Clientes.estado);
                                }
                                else
                                {
                                    if (Convert.ToDateTime(fechaFinal) >= Convert.ToDateTime(us.Referencias.fechaemision.ToShortDateString()))
                                    {
                                        table.Rows.Add(us.Referencias.estadoref, us.numref, us.Servicios.cuetacontable, us.Servicios.nomservicio, us.Referencias.fechaemision.ToShortDateString(), us.Referencias.fechavencimiento.ToShortDateString(), us.Referencias.fechaestado.ToShortDateString(), Math.Round((us.costount * us.cantidad), 2), us.Referencias.Clientes.nombre_, us.Referencias.Clientes.apellidos,
                                            us.Referencias.Clientes.matricula, us.Referencias.Clientes.rfc_, us.Referencias.Clientes.tipopersona, us.Referencias.Clientes.correoelectronico, us.Referencias.Clientes.calle, us.Referencias.Clientes.numeroex, us.Referencias.Clientes.numeroin, us.Referencias.Clientes.colonia, us.Referencias.Clientes.cp,
                                            us.Referencias.Clientes.ciudad, us.Referencias.Clientes.estado);
                                    }
                                }
                            }
                        }
                    }
                    else
                    {
                        if (estadoRep == us.Referencias.estadoref)
                        {
                            if (fechaInicio == "")
                            {
                                if (fechaFinal == "")
                                {
                                    table.Rows.Add(us.Referencias.estadoref, us.numref, us.Servicios.cuetacontable, us.Servicios.nomservicio, us.Referencias.fechaemision.ToShortDateString(), us.Referencias.fechavencimiento.ToShortDateString(), us.Referencias.fechaestado.ToShortDateString(), Math.Round((us.costount * us.cantidad), 2), us.Referencias.Clientes.nombre_, us.Referencias.Clientes.apellidos,
                                        us.Referencias.Clientes.matricula, us.Referencias.Clientes.rfc_, us.Referencias.Clientes.tipopersona, us.Referencias.Clientes.correoelectronico, us.Referencias.Clientes.calle, us.Referencias.Clientes.numeroex, us.Referencias.Clientes.numeroin, us.Referencias.Clientes.colonia, us.Referencias.Clientes.cp,
                                        us.Referencias.Clientes.ciudad, us.Referencias.Clientes.estado);
                                }
                                else
                                {
                                    if (Convert.ToDateTime(fechaFinal) >= Convert.ToDateTime(us.Referencias.fechaemision.ToShortDateString()))
                                    {
                                        table.Rows.Add(us.Referencias.estadoref, us.numref, us.Servicios.cuetacontable, us.Servicios.nomservicio, us.Referencias.fechaemision.ToShortDateString(), us.Referencias.fechavencimiento.ToShortDateString(), us.Referencias.fechaestado.ToShortDateString(), Math.Round((us.costount * us.cantidad), 2), us.Referencias.Clientes.nombre_, us.Referencias.Clientes.apellidos,
                                            us.Referencias.Clientes.matricula, us.Referencias.Clientes.rfc_, us.Referencias.Clientes.tipopersona, us.Referencias.Clientes.correoelectronico, us.Referencias.Clientes.calle, us.Referencias.Clientes.numeroex, us.Referencias.Clientes.numeroin, us.Referencias.Clientes.colonia, us.Referencias.Clientes.cp,
                                            us.Referencias.Clientes.ciudad, us.Referencias.Clientes.estado);
                                    }
                                }
                            }
                            else
                            {
                                if (Convert.ToDateTime(fechaInicio) <= Convert.ToDateTime(us.Referencias.fechaemision.ToShortDateString()))
                                {
                                    if (fechaFinal == "")
                                    {
                                        table.Rows.Add(us.Referencias.estadoref, us.numref, us.Servicios.cuetacontable, us.Servicios.nomservicio, us.Referencias.fechaemision.ToShortDateString(), us.Referencias.fechavencimiento.ToShortDateString(), us.Referencias.fechaestado.ToShortDateString(), Math.Round((us.costount * us.cantidad), 2), us.Referencias.Clientes.nombre_, us.Referencias.Clientes.apellidos,
                                            us.Referencias.Clientes.matricula, us.Referencias.Clientes.rfc_, us.Referencias.Clientes.tipopersona, us.Referencias.Clientes.correoelectronico, us.Referencias.Clientes.calle, us.Referencias.Clientes.numeroex, us.Referencias.Clientes.numeroin, us.Referencias.Clientes.colonia, us.Referencias.Clientes.cp,
                                            us.Referencias.Clientes.ciudad, us.Referencias.Clientes.estado);
                                    }
                                    else
                                    {
                                        if (Convert.ToDateTime(fechaFinal) >= Convert.ToDateTime(us.Referencias.fechaemision.ToShortDateString()))
                                        {
                                            table.Rows.Add(us.Referencias.estadoref, us.numref, us.Servicios.cuetacontable, us.Servicios.nomservicio, us.Referencias.fechaemision.ToShortDateString(), us.Referencias.fechavencimiento.ToShortDateString(), us.Referencias.fechaestado.ToShortDateString(), Math.Round((us.costount * us.cantidad), 2), us.Referencias.Clientes.nombre_, us.Referencias.Clientes.apellidos,
                                                us.Referencias.Clientes.matricula, us.Referencias.Clientes.rfc_, us.Referencias.Clientes.tipopersona, us.Referencias.Clientes.correoelectronico, us.Referencias.Clientes.calle, us.Referencias.Clientes.numeroex, us.Referencias.Clientes.numeroin, us.Referencias.Clientes.colonia, us.Referencias.Clientes.cp,
                                                us.Referencias.Clientes.ciudad, us.Referencias.Clientes.estado);
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
                String cadena = "Generó reporte de referencias";
                int idUser = (int)Session["IdUser"];

                br.registroBitacora(cadena, idUser, "Reportes");

                using (MemoryStream stream = new MemoryStream())
                {
                    workbook.SaveAs(stream);
                    return File(stream.ToArray(), "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "Rp_Referencias - " + fechaArchivo + ".xlsx");
                }
            }
        }

        [AuthorizeUser(nombreOperacion: "bitacoraeventos")]
        // Generar Reporte
        public ActionResult ReporteBitacora(string estadoRep, string fechaInicio, string fechaFinal)
        {
            //Validacion parametros
            if (estadoRep == "")
            {
                estadoRep = "TODAS";
            }

            var fechaArchivo = DateTime.Now.ToShortDateString();

            using (var workbook = new XLWorkbook())
            {
                var worksheet = workbook.Worksheets.Add("Eventos registrados");

                // Definir titulo largo
                worksheet.Cell("A1").Value = "Reporte de eventos registrados";
                var range = worksheet.Range("A1:F1");
                range.Merge().Style.Font.SetBold().Font.FontSize = 16;

                // Definir ficha
                worksheet.Cell("A2").Value = "REPORTE GENERADO";
                range = worksheet.Range("A2:B3");
                range.Merge().Style.Font.SetBold().Font.FontSize = 13;
                range.Merge().Style.Fill.BackgroundColor = XLColor.FromArgb(153, 20, 38);
                range.Merge().Style.Font.FontColor = XLColor.White;
                worksheet.Row(2).Height = 30;


                worksheet.Cell("C2").Value = "Generado por " + Session["NombreUser"] + " el día " + fechaArchivo + " con los parámetros: Tipo de operación - " + estadoRep + ", Fecha inicio - " + fechaInicio + ", Fecha fin - " + fechaFinal;
                range = worksheet.Range("C2:F3");
                range.Merge().Style.Font.FontSize = 12;

                // Impresión de datos
                DataTable table = new DataTable();
                table.TableName = "Eventos_Registrados";
                table.Columns.Add("FECHA", typeof(string));
                table.Columns.Add("HORA", typeof(string));
                table.Columns.Add("OPERACIÓN", typeof(string));
                table.Columns.Add("DESCRIPCIÓN", typeof(string));
                table.Columns.Add("USUARIO OPERADOR", typeof(string));
                table.Columns.Add("IP", typeof(string));

                List<Eventos> usuarios = db.Eventos.ToList();

                foreach (Eventos us in usuarios)
                {
                    if (estadoRep == "TODAS")
                    {
                        if (fechaInicio == "")
                        {
                            if (fechaFinal == "")
                            {
                                table.Rows.Add(us.fecha.ToShortDateString(), us.hora, us.operacion, us.descripcion, us.UsuariosAd.correoelectronico, us.ip);
                            }
                            else
                            {
                                if (Convert.ToDateTime(fechaFinal) >= Convert.ToDateTime(us.fecha.ToShortDateString()))
                                {
                                    table.Rows.Add(us.fecha.ToShortDateString(), us.hora, us.operacion, us.descripcion, us.UsuariosAd.correoelectronico, us.ip);
                                }
                            }
                        }
                        else
                        {
                            if (Convert.ToDateTime(fechaInicio) <= Convert.ToDateTime(us.fecha.ToShortDateString()))
                            {
                                if (fechaFinal == "")
                                {
                                    table.Rows.Add(us.fecha.ToShortDateString(), us.hora, us.operacion, us.descripcion, us.UsuariosAd.correoelectronico, us.ip);
                                }
                                else
                                {
                                    if (Convert.ToDateTime(fechaFinal) >= Convert.ToDateTime(us.fecha.ToShortDateString()))
                                    {
                                        table.Rows.Add(us.fecha.ToShortDateString(), us.hora, us.operacion, us.descripcion, us.UsuariosAd.correoelectronico, us.ip);
                                    }
                                }
                            }
                        }
                    }
                    else
                    {
                        if (estadoRep == us.operacion)
                        {
                            if (fechaInicio == "")
                            {
                                if (fechaFinal == "")
                                {
                                    table.Rows.Add(us.fecha.ToShortDateString(), us.hora, us.operacion, us.descripcion, us.UsuariosAd.correoelectronico, us.ip);
                                }
                                else
                                {
                                    if (Convert.ToDateTime(fechaFinal) >= Convert.ToDateTime(us.fecha.ToShortDateString()))
                                    {
                                        table.Rows.Add(us.fecha.ToShortDateString(), us.hora, us.operacion, us.descripcion, us.UsuariosAd.correoelectronico, us.ip);
                                    }
                                }
                            }
                            else
                            {
                                if (Convert.ToDateTime(fechaInicio) <= Convert.ToDateTime(us.fecha.ToShortDateString()))
                                {
                                    if (fechaFinal == "")
                                    {
                                        table.Rows.Add(us.fecha.ToShortDateString(), us.hora, us.operacion, us.descripcion, us.UsuariosAd.correoelectronico, us.ip);
                                    }
                                    else
                                    {
                                        if (Convert.ToDateTime(fechaFinal) >= Convert.ToDateTime(us.fecha.ToShortDateString()))
                                        {
                                            table.Rows.Add(us.fecha.ToShortDateString(), us.hora, us.operacion, us.descripcion, us.UsuariosAd.correoelectronico, us.ip);
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
                String cadena = "Generó reporte de bitácora de eventos";
                int idUser = (int)Session["IdUser"];

                br.registroBitacora(cadena, idUser, "Reportes");

                using (MemoryStream stream = new MemoryStream())
                {
                    workbook.SaveAs(stream);
                    return File(stream.ToArray(), "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "Rp_Bitácora - " + fechaArchivo + ".xlsx");
                }
            }
        }

    }
}
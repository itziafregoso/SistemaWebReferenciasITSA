using SistemaWeb.Data;
using SistemaWeb.Filters;
using SistemaWeb.Models;
using SistemaWeb.Repositories;
using System;
using System.Collections.Generic;
using System.Data.Entity.Validation;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Mvc;

namespace SistemaWeb.Controllers
{
    public class CompraController : Controller
    {
        //Conexión a base de datos
        DataBase db = new DataBase();
        RepositoryProducto repo;
        coloresSistema cs = new coloresSistema();

        //Catalogo de servicios
        public CompraController()
        {
            repo = new RepositoryProducto();
        }

        //Consulta SELECT .JSON de Usuarios Administrativos
        public JsonResult GetAlumnos()
        {
            return this.Json((from obj in db.Clientes select new { matricula = obj.matricula, rfc = obj.rfc_, tipo = obj.tipopersona, nombre = obj.nombre_, apellidos = obj.apellidos, correo = obj.correoelectronico, calle = obj.calle, noext = obj.numeroex, noint = obj.numeroin, colonia = obj.colonia, cp = obj.cp, ciudad = obj.ciudad, estado = obj.estado }), JsonRequestBehavior.AllowGet);
        }

        [AuthorizeUser(nombreOperacion: "generarcompra")]
        //Vista GenerarCompra
        [HttpGet]
        public ActionResult Index(int? id, int? cantidad, string operacion)
        {
            ViewBag.ProductoOriginal = db.Servicios.ToList();

            cs.recuperarColores();

            string dError = TempData["error"] as string;

            if(dError != null && dError != "") 
            {
                ViewBag.Carrito = Session["Productos"];
                ViewBag.error = dError;
                return View();
            }

            if (id != null)
            {
                if (operacion == "eliminar")
                {
                    try
                    {
                        List<Servicios> lista = (List<Servicios>)Session["Productos"];

                        var itemToRemove = lista.Single(r => r.contro == id);
                        lista.Remove(itemToRemove);

                        if (lista.Count() == 0)
                        {
                            Session["Productos"] = null;
                        }
                        else
                        {
                            Session["Productos"] = lista;
                        }
                    }catch (Exception ex)
                    {

                    }
                }
                else if (operacion == "ver")
                {
                    Servicios usuario = db.Servicios.Find(id);
                    if (usuario == null)
                    {
                        return HttpNotFound();
                    }

                    ViewBag.id = id;
                    ViewBag.operacion = operacion;
                    ViewBag.Carrito = Session["Productos"];

                    return View(usuario);
                }
            }

            decimal total = 0;
            List<Servicios> listaTotal = (List<Servicios>)Session["Productos"];

            if (Session["Productos"] != null)
            {
                foreach (var valor in listaTotal)
                {
                    total = total + Convert.ToDecimal(valor.costo);
                }

                total = decimal.Round(total, 2);

                ViewBag.Total = total;
            }

            ViewBag.Carrito = Session["Productos"];
            List<Servicios> prod = this.repo.GetProductos();
            return View(prod);
        }

        [AuthorizeUser(nombreOperacion: "generarcompra")]
        //Añadir producto a carrito
        [HttpPost]
        public ActionResult Index(int id, int cantidad)
        {
            ViewBag.ProductoOriginal = db.Servicios.ToList();
            Servicios servicio = db.Servicios.Find(id);

            List<Servicios> codigosProductos;



            if (Session["Productos"] == null)
            {
                codigosProductos = new List<Servicios>();
            }
            else
            {
                codigosProductos = Session["Productos"] as List<Servicios>;

                foreach(Servicios serv in codigosProductos)
                {
                    if(serv.IdArea != servicio.IdArea)
                    {
                        ViewBag.carrito = Session["Productos"];
                        TempData["error"] = "Solo se pueden agregar productos de la misma área al carrito";
                        return RedirectToAction("Index");
                    }
                }

                try
                {
                    var itemToRemove = codigosProductos.Single(r => r.contro == id);
                    codigosProductos.Remove(itemToRemove);
                }
                catch (Exception ex)
                {

                }
            }

            
            servicio.costo = servicio.costo * cantidad;

            codigosProductos.Add(servicio);
            Session["Productos"] = codigosProductos;

            ViewBag.Carrito = Session["Productos"];
            return RedirectToAction("Index");
        }

        [AuthorizeUser(nombreOperacion: "generarcompra")]
        //Vista resumenCompra
        public ActionResult ResumenCompra()
        {
            cs.recuperarColores();

            if (Session["Productos"] == null)
            {
                return RedirectToAction("Index");
            }

            ViewBag.Carrito = Session["Productos"];

            decimal total = 0;
            int vigencia = 0;
            List<Servicios> listaTotal = (List<Servicios>)Session["Productos"];
            if (Session["Productos"] != null)
            {
                foreach (var valor in listaTotal)
                {
                    total = total + Convert.ToDecimal(valor.costo);
                    if (vigencia == 0)
                    {
                        vigencia = valor.diasvigencia;
                    }
                    else if (vigencia > valor.diasvigencia)
                    {
                        vigencia = valor.diasvigencia;
                    }

                }

                total = decimal.Round(total, 2);

                ViewBag.Total = total;
                TempData["Vigencia"] = vigencia;
            }

            List<Servicios> prod = this.repo.GetProductos();

            return View(prod);
        }

        [AuthorizeUser(nombreOperacion: "generarcompra")]
        [HttpGet]
        public ActionResult DatosCliente()
        {
            cs.recuperarColores();

            if (Session["Productos"] == null)
            {
                return RedirectToAction("Index");
            }

            return View();
        }

        [AuthorizeUser(nombreOperacion: "generarcompra")]
        [HttpPost]
        public ActionResult DatosCliente(string matriculaEst, string tipoCliente, string rfc, string tipoPer,
            string nombre, string apellidos, string correo, string calle, string noext, string noint, string colonia,
            string cp, string ciudad, string estado)
        { 

            if (Session["Productos"] == null)
            {
                return RedirectToAction("Index");
            }

            TempData["Vigencia"] = TempData["Vigencia"];

            if (tipoCliente == "rdAlum")
            {
                var pUser = (from d in db.Clientes
                             where d.matricula == matriculaEst
                             select d).FirstOrDefault();

                if (pUser == null)
                {
                    ViewBag.error = "No se encontró un alumno con la misma matrícula";
                    return View();
                }
                else
                {
                    if(noint == "")
                    {
                        noint = null;
                    }

                    if(rfc == "")
                    {
                        rfc = null;
                    }

                    TempData["ClienteParcial"] = pUser;
                }
            }
            else
            {
                Clientes usuarioFinal;

                usuarioFinal = (from d in db.Clientes
                             where d.rfc_ == rfc
                             select d).FirstOrDefault();

                if(usuarioFinal == null)
                {
                    Clientes clienteNuevo = new Clientes();
                    clienteNuevo.nombre_ = nombre;
                    clienteNuevo.rfc_ = rfc;
                    clienteNuevo.tipopersona = tipoPer;
                    clienteNuevo.apellidos = apellidos;
                    clienteNuevo.correoelectronico = correo;
                    clienteNuevo.calle = calle;
                    clienteNuevo.numeroex = noext;
                    clienteNuevo.numeroin = noint;
                    clienteNuevo.colonia = colonia;
                    clienteNuevo.cp = cp;
                    clienteNuevo.ciudad = ciudad;
                    clienteNuevo.estado = estado;

                    db.Clientes.Add(clienteNuevo);
                    db.SaveChanges();                    

                    usuarioFinal = (from d in db.Clientes
                                      where d.rfc_ == clienteNuevo.rfc_
                                      select d).FirstOrDefault();
                }

                TempData["ClienteParcial"] = usuarioFinal;
            }

            return RedirectToAction("FinalizarCompra");
        }

        //Método para calcular fecha de vencimiento
        static DateTime FechaEntrega(DateTime fechaPedido, int espera)
        {
            DateTime dt = fechaPedido;
            DataBase db = new DataBase();

            var consulta = from datos in db.DiasHabiles select datos;

            List<DiasHabiles> listaHabiles = consulta.ToList();

            String fechaLista, fechaDT;

            for(int i = 0; i<espera; i++)
            {
                if(dt.DayOfWeek == DayOfWeek.Saturday || dt.DayOfWeek == DayOfWeek.Sunday)
                {
                    i--;
                }
                else
                {
                    for(int y = 0; y<listaHabiles.Count-1; y++)
                    {
                        fechaLista = (listaHabiles[y].fechaHabil).ToShortDateString();
                        fechaDT = dt.ToShortDateString();
                        if (fechaLista == fechaDT)
                        {
                            i--;
                            y = listaHabiles.Count();
                        }
                    }

                }
                dt = dt.AddDays(+1);
            }

            return dt;
        }

        [AuthorizeUser(nombreOperacion: "generarcompra")]
        //Vista finalizar compra y generación de referencia
        public ActionResult FinalizarCompra()
        {
            cs.recuperarColores();

            ViewBag.Vigencia = TempData["Vigencia"];
            ViewBag.Cliente = TempData["ClienteParcial"];

            Clientes usuarioCliente = TempData["ClienteParcial"] as Clientes;

            decimal total = 0;
            List<Servicios> listaTotal = (List<Servicios>)Session["Productos"];
            if (Session["Productos"] != null)
            {
                foreach (var valor in listaTotal)
                {
                    total = total + Convert.ToDecimal(valor.costo);
                }

                total = decimal.Round(total, 2);
            }

            string totalPago = total.ToString();

            ViewBag.TotalPago = totalPago;

            totalPago = string.Join("", totalPago.Split('.', ','));

            DateTime FechaVigencia = FechaEntrega(DateTime.Now, ViewBag.Vigencia);

            ViewBag.FechaFinal = FechaVigencia.ToString("dd/MM/yyyy");
            Session["FechaCaduca"] = FechaVigencia.ToString("dd/MM/yyyy");

            DateTime fechaEmision = DateTime.Now;

            string año = FechaVigencia.ToString("yyyy");
            string mes = FechaVigencia.ToString("MM");
            string dia = FechaVigencia.ToString("dd");

            int añoRef = (Convert.ToInt32(año) - 2014) * 372;
            int mesRef = (Convert.ToInt32(mes) - 1) * 31;
            int diaRef = (Convert.ToInt32(dia) - 1);

            string referenciaAño = (añoRef + mesRef + diaRef).ToString();

            if (referenciaAño.Length < 4)
            {
                for (int i = 0; i <= referenciaAño.Length - 4; i++)
                {
                    referenciaAño = "0" + referenciaAño;
                }
            }

            int[] ponderadosTotalPagar = { 7, 1, 3, 7, 1, 3, 7, 1, 3, 7, 1, 3, 7 };
            string referenciaTotal = "0";

            for (var x = 0; x < totalPago.Length; x++)
            {
                String valor = (totalPago[totalPago.Length - (x + 1)]).ToString();
                String ponderado = (ponderadosTotalPagar[ponderadosTotalPagar.Length - (x + 1)]).ToString();

                referenciaTotal = (Convert.ToInt32(referenciaTotal) + (Convert.ToInt32(valor) * Convert.ToInt32(ponderado))).ToString();
            }

            referenciaTotal = (Convert.ToInt32(referenciaTotal) % 10).ToString();

            int[] ponderadosReferencia = { 23, 19, 17, 13, 11, 23, 19, 17, 13, 11, 23, 19, 17, 13, 11, 23, 19, 17, 13, 11 };

            int totalRegistros = db.Referencias.Count();



            string PrimerReferencia = (totalRegistros + 1).ToString();

            if (PrimerReferencia.Length < 5)
            {
                for (int i = 0; PrimerReferencia.Length < 5; i++)
                {
                    PrimerReferencia = "0" + PrimerReferencia;
                }
            }

            string añoDigito = DateTime.Now.ToShortDateString();


            PrimerReferencia = añoDigito.Substring(añoDigito.Length - 2, 2) + PrimerReferencia;

            OpcionesSistema pam = db.OpcionesSistema.FirstOrDefault();

            string cv = PrimerReferencia + referenciaAño + referenciaTotal + pam.constanteRef;
            string referenciaCV = "0";

            for (var x = 0; x < cv.Length; x++)
            {
                String valor = (cv[cv.Length - (x + 1)]).ToString();
                String ponderado = (ponderadosReferencia[ponderadosReferencia.Length - (x + 1)]).ToString();

                referenciaCV = (Convert.ToInt32(referenciaCV) + (Convert.ToInt32(valor) * Convert.ToInt32(ponderado))).ToString();
            }

            referenciaCV = ((Convert.ToInt32(referenciaCV) % 97) + 1).ToString();

            if(referenciaCV.Length == 1)
            {
                referenciaCV = "0" + referenciaCV;
            }

            ViewBag.Carrito = Session["Productos"];
            ViewBag.Referencia = cv + referenciaCV;

            Session["FinCarrito"] = Session["Productos"];

            TempData["carritoFinal"] = Session["Productos"];

            Session["Folio"] = PrimerReferencia;

            Session["Productos"] = null;

            TempData["UsuarioAlumno"] = null;

            // AÑADIR REFERENCIA

            Referencias nuevaReferencia = new Referencias
            {
                numref = (cv + referenciaCV),
                estadoref = "EMITIDA",
                fechaemision = fechaEmision,
                fechaestado = fechaEmision,
                fechavencimiento = FechaVigencia,
                monto = total,
                IdCliente = usuarioCliente.IdCliente
            };

            db.Referencias.Add(nuevaReferencia);
            db.SaveChanges();


            // AÑADIR VENTA
            List<Servicios> prod = this.repo.GetProductos();
            double totalCantidad=0;
            List<double> cantidadesProd = new List<double>();
            List<double> precioUnt = new List<double>();

            foreach (var valor in listaTotal)
            {
                foreach (Servicios original in prod)
                {
                    if (valor.contro == original.contro)
                    {
                        totalCantidad = (Convert.ToDouble(valor.costo) / Convert.ToDouble(original.costo));
                        cantidadesProd.Add(totalCantidad);
                        precioUnt.Add(Convert.ToDouble(original.costo));
                    }
                };
            };

                int contador = 0;
                foreach (var valor in listaTotal)
                {
                    Ventas nuevaVenta = new Ventas
                    {
                        numref = (cv + referenciaCV),
                        contro = valor.contro,
                        cantidad = Convert.ToInt32(cantidadesProd[contador]),
                        costount = Convert.ToInt32(precioUnt[contador])
                    };
                    db.Ventas.Add(nuevaVenta);
                    db.SaveChanges();
                    contador++;
                };               



            // AÑADIR A BITÁCORA
            String cadena = "Se generó la compra " + (cv + referenciaCV);
            int idUser = (int)Session["IdUser"];
            Eventos nuevoEvento = new Eventos
            {
                fecha = DateTime.Now,
                hora = DateTime.Now.ToString("hh:mm"),
                operacion = "Compra",
                descripcion = cadena,
                IdUsuariosAd = idUser,
                ip = Request.UserHostAddress
            };
            db.Eventos.Add(nuevoEvento);
            db.SaveChanges();

            return View();
        }

    }
}
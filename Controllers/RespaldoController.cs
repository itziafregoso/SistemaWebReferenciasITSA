using SistemaWeb.Data;
using SistemaWeb.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Data.SqlClient;
using System.Data.Entity.Validation;
using System.Data.Entity;
using SistemaWeb.Filters;

namespace SistemaWeb.Controllers
{
    public class RespaldoController : Controller
    {
        bitacoraRegistro br = new bitacoraRegistro();

        [AuthorizeUser(nombreOperacion: "restaurarsistema")]
        [HttpGet]
        public ActionResult Restaurar()
        {
            if (TempData["error"] != null)
            {
                ViewBag.mensaje = TempData["error"];
                TempData["error"] = null;
            }

            int anioPost;

            using(BD_Historico db = new BD_Historico())
            {
               
                var ultimoRegistro = db.HReferencias.OrderByDescending(d => d.Hanio).FirstOrDefault();

                if (ultimoRegistro == null)
                {
                    anioPost = 2022;
                }
                else
                {
                    anioPost = ultimoRegistro.Hanio + 1;
                }

                if (anioPost == 1)
                {
                    anioPost = 2022;
                }

            }

            ViewBag.anio = anioPost;

            return View();
        }

        [AuthorizeUser(nombreOperacion: "restaurarsistema")]        
        [HttpPost]
        public ActionResult Restaurar(string password)
        {
            UsuariosAd user = HttpContext.Session["User"] as UsuariosAd;
            string claveScript = Encrypt.GetSHA256(password);
            int anioFinal;

            using (DataBase db = new DataBase())
            {
                var userFind = db.UsuariosAd.Where(d => d.IdUsuariosAd == user.IdUsuariosAd && d.contrasena == claveScript).FirstOrDefault();

                if (userFind == null)
                {
                    TempData["error"] = "La contraseña no corresponde al usuario actual.";
                    return RedirectToAction("Restaurar");
                }                

                // Inicializar conexión con BD Histórico
                using(BD_Historico Hdb = new BD_Historico())
                {
                    int anioAnterior = Hdb.HClientes.OrderByDescending(d => d.Hanio).FirstOrDefault()?.Hanio ?? 0;
                    anioAnterior++;
                    anioFinal = anioAnterior;

                    if (anioAnterior == 1)
                    {
                        anioFinal = 2022;
                    }

                    var Clientes = db.Clientes.ToList();
                    var Referencias = db.Referencias.ToList();
                    var Servicios = db.Servicios.ToList();
                    var Ventas = db.Ventas.ToList();


                    // Copiar CLIENTES
                    foreach (var datoCliente in Clientes)
                    {
                        var registroCliente = new HClientes
                        {
                            Hanio = anioFinal,
                            IdCliente = datoCliente.IdCliente,
                            rfc = datoCliente.rfc_,
                            tipopersona= datoCliente.tipopersona,
                            matricula = datoCliente.matricula,
                            nombre = datoCliente.nombre_,
                            apellidos= datoCliente.apellidos,
                            correoelectronico= datoCliente.correoelectronico
                        };

                        // Agrega el objeto nuevo a la tabla de la base de datos B
                        Hdb.HClientes.Add(registroCliente);
                    }

                    // Copiar REFERENCIAS
                    foreach (var datoReferencia in Referencias)
                    {
                        var registroRef = new HReferencias
                        {
                            Hanio = anioFinal,
                            numref = datoReferencia.numref,
                            estadoref = datoReferencia.estadoref,
                            fechaemision = datoReferencia.fechaemision,
                            fechaestado = datoReferencia.fechaestado,
                            fechavencimiento = datoReferencia.fechavencimiento,
                            monto= datoReferencia.monto,
                            IdCliente = datoReferencia.IdCliente
                        };

                        Hdb.HReferencias.Add(registroRef);
                    }

                    // Copiar SERVICIOS
                    foreach (var datoServicio in Servicios)
                    {
                        var registroServicio = new HServicios
                        {
                            Hanio = anioFinal,
                            contro = datoServicio.contro,
                            nomservicio = datoServicio.nomservicio,
                            costo = datoServicio.costo,
                            cuentacontable = datoServicio.cuetacontable,
                            nombrearea = datoServicio.Areas.nombrearea,
                            tipo = datoServicio.TipoServicios.tipo
                        };

                        Hdb.HServicios.Add(registroServicio);
                    }

                    // Copiar VENTAS
                    foreach (var datoVentas in Ventas)
                    {
                        var registroVentas = new HVentas
                        {
                            Hanio = anioFinal,
                            IdVenta = datoVentas.IdVenta,
                            numref = datoVentas.numref,
                            contro = datoVentas.contro,
                            cantidad = datoVentas.cantidad,
                            costounit = datoVentas.costount
                        };

                        Hdb.HVentas.Add(registroVentas);
                    }

                    Hdb.SaveChanges();
                }

                // Ejecuta reseteo
                db.Database.ExecuteSqlCommand("DELETE FROM Eventos; DELETE FROM Ventas; DELETE FROM Referencias;");

                // Reinicia los valores de ID
                db.Database.ExecuteSqlCommand("DBCC CHECKIDENT (Eventos, RESEED, 0); DBCC CHECKIDENT (Ventas, RESEED, 0);");

                db.SaveChanges();

                String cadena = "Se respaldó y reinició el sistema bajo el año fiscal " + anioFinal;
                int idUser = (int)Session["IdUser"];
                br.registroBitacora(cadena, idUser, "Registro");
            }       

            return RedirectToAction("Index","Home");
        }
    }
}
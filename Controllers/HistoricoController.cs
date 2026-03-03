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
    public class HistoricoController : Controller
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
        BD_Historico db = new BD_Historico();
        coloresSistema cs = new coloresSistema();
        bitacoraRegistro br = new bitacoraRegistro();

        [AuthorizeUser(nombreOperacion: "verhistorico")]
        [HttpGet]
        public ActionResult Index(string id, int? anio, string op)
        {
            cs.recuperarColores();
            ViewBag.op = op;

            if (id != null && anio != null)
            {
                using(BD_Historico db = new BD_Historico()) { 

                HReferencias refd = db.HReferencias.Where(d => d.numref == id && d.Hanio == anio).FirstOrDefault();
                if (refd == null)
                {
                    return HttpNotFound();
                }

                HClientes clienteAsc = db.HClientes.Where(d => d.IdCliente == refd.IdCliente).FirstOrDefault();
                ViewBag.Cliente = clienteAsc;
                ViewBag.Name = db.HReferencias.ToList();
                ViewBag.Editar = true;
                ViewBag.id = id;
                return View(refd);
                }
            }
            else
            {
                ViewBag.Editar = false;
                return View();
            }
        }

        [AuthorizeUser(nombreOperacion: "verhistorico")]
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

            using (BD_Historico db = new BD_Historico())
            {

                IQueryable<TableReferenciasViewModel> query = (from d in db.HReferencias
                                                               select new TableReferenciasViewModel
                                                               {
                                                                   noReferencia = d.numref,
                                                                   estado = d.estadoref,
                                                                   fechaEmision = d.fechaemision,
                                                                   monto = d.monto,
                                                                   anio = d.Hanio

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

        [AuthorizeUser(nombreOperacion: "verhistorico")]
        public ActionResult JsonProductos(string id, int anio)
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

            using (BD_Historico db = new BD_Historico())
            {

                IQueryable<TableComprasViewModel> query = (from d in db.HVentas
                                                           join s in db.HServicios on d.contro equals s.contro
                                                           where d.numref == id && d.Hanio == anio
                                                           && s.Hanio == anio
                                                           select new TableComprasViewModel
                                                           {
                                                               producto = s.nomservicio, 
                                                               cantidad = d.cantidad,
                                                               monto = (d.costounit * d.cantidad)
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

        // Generar Reporte
        [AuthorizeUser(nombreOperacion: "verhistorico")]
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
                worksheet.Cell("A1").Value = "Reporte de referencias registradas | Histórico";
                var range = worksheet.Range("A1:O1");
                range.Merge().Style.Font.SetBold().Font.FontSize = 16;

                // Definir ficha
                worksheet.Cell("A2").Value = "REPORTE GENERADO";
                range = worksheet.Range("A2:B3");
                range.Merge().Style.Font.SetBold().Font.FontSize = 13;
                range.Merge().Style.Fill.BackgroundColor = XLColor.FromArgb(153, 20, 38);
                range.Merge().Style.Font.FontColor = XLColor.White;
                worksheet.Row(2).Height = 30;


                worksheet.Cell("C2").Value = "Generado por " + Session["NombreUser"] + " el día " + fechaArchivo + " con los parámetros: Estado de referencia - " + estadoRep + ", Fecha inicio - " + fechaInicio + ", Fecha fin - " + fechaFinal;
                range = worksheet.Range("C2:O3");
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

                List<HVentas> usuarios = db.HVentas.ToList();

                foreach (HVentas us in usuarios)
                {
                    HReferencias referenciaActual = db.HReferencias.Where(d => d.numref == us.numref && d.Hanio == us.Hanio).FirstOrDefault();
                    HClientes clienteActual = db.HClientes.Where(d => d.IdCliente == referenciaActual.IdCliente && d.Hanio == us.Hanio).FirstOrDefault();
                    HServicios servicioActual = db.HServicios.Where(d => d.contro == us.contro && d.Hanio == us.Hanio).FirstOrDefault();


                    if (estadoRep == "TODAS")
                    {
                        if (fechaInicio == "")
                        {
                            if (fechaFinal == "")
                            {
                                table.Rows.Add(referenciaActual.estadoref, us.numref, servicioActual.cuentacontable, servicioActual.nomservicio, referenciaActual.fechaemision.ToShortDateString(), referenciaActual.fechavencimiento.ToShortDateString(), referenciaActual.fechaestado.ToShortDateString(), Math.Round((us.costounit * us.cantidad), 2), clienteActual.nombre, clienteActual.apellidos,
                                    clienteActual.matricula, clienteActual.rfc, clienteActual.tipopersona, clienteActual.correoelectronico
                                    );
                            }
                            else
                            {
                                if (Convert.ToDateTime(fechaFinal) >= Convert.ToDateTime(referenciaActual.fechaemision.ToShortDateString()))
                                {
                                    table.Rows.Add(referenciaActual.estadoref, us.numref, servicioActual.cuentacontable, servicioActual.nomservicio, referenciaActual.fechaemision.ToShortDateString(), referenciaActual.fechavencimiento.ToShortDateString(), referenciaActual.fechaestado.ToShortDateString(), Math.Round((us.costounit * us.cantidad), 2), clienteActual.nombre, clienteActual.apellidos,
                                        clienteActual.matricula, clienteActual.rfc, clienteActual.tipopersona, clienteActual.correoelectronico);
                                }
                            }
                        }
                        else
                        {
                            if (Convert.ToDateTime(fechaInicio) <= Convert.ToDateTime(referenciaActual.fechaemision.ToShortDateString()))
                            {
                                if (fechaFinal == "")
                                {
                                    table.Rows.Add(referenciaActual.estadoref, us.numref, servicioActual.cuentacontable, servicioActual.nomservicio, referenciaActual.fechaemision.ToShortDateString(), referenciaActual.fechavencimiento.ToShortDateString(), referenciaActual.fechaestado.ToShortDateString(), Math.Round((us.costounit * us.cantidad), 2), clienteActual.nombre, clienteActual.apellidos,
                                        clienteActual.matricula, clienteActual.rfc, clienteActual.tipopersona, clienteActual.correoelectronico);
                                }
                                else
                                {
                                    if (Convert.ToDateTime(fechaFinal) >= Convert.ToDateTime(referenciaActual.fechaemision.ToShortDateString()))
                                    {
                                        table.Rows.Add(referenciaActual.estadoref, us.numref, servicioActual.cuentacontable, servicioActual.nomservicio, referenciaActual.fechaemision.ToShortDateString(), referenciaActual.fechavencimiento.ToShortDateString(), referenciaActual.fechaestado.ToShortDateString(), Math.Round((us.costounit * us.cantidad), 2), clienteActual.nombre, clienteActual.apellidos,
                                            clienteActual.matricula, clienteActual.rfc, clienteActual.tipopersona, clienteActual.correoelectronico);
                                    }
                                }
                            }
                        }
                    }
                    else
                    {
                        if (estadoRep == referenciaActual.estadoref)
                        {
                            if (fechaInicio == "")
                            {
                                if (fechaFinal == "")
                                {
                                    table.Rows.Add(referenciaActual.estadoref, us.numref, servicioActual.cuentacontable, servicioActual.nomservicio, referenciaActual.fechaemision.ToShortDateString(), referenciaActual.fechavencimiento.ToShortDateString(), referenciaActual.fechaestado.ToShortDateString(), Math.Round((us.costounit * us.cantidad), 2), clienteActual.nombre, clienteActual.apellidos,
                                        clienteActual.matricula, clienteActual.rfc, clienteActual.tipopersona, clienteActual.correoelectronico);
                                }
                                else
                                {
                                    if (Convert.ToDateTime(fechaFinal) >= Convert.ToDateTime(referenciaActual.fechaemision.ToShortDateString()))
                                    {
                                        table.Rows.Add(referenciaActual.estadoref, us.numref, servicioActual.cuentacontable, servicioActual.nomservicio, referenciaActual.fechaemision.ToShortDateString(), referenciaActual.fechavencimiento.ToShortDateString(), referenciaActual.fechaestado.ToShortDateString(), Math.Round((us.costounit * us.cantidad), 2), clienteActual.nombre, clienteActual.apellidos,
                                            clienteActual.matricula, clienteActual.rfc, clienteActual.tipopersona, clienteActual.correoelectronico);
                                    }
                                }
                            }
                            else
                            {
                                if (Convert.ToDateTime(fechaInicio) <= Convert.ToDateTime(referenciaActual.fechaemision.ToShortDateString()))
                                {
                                    if (fechaFinal == "")
                                    {
                                        table.Rows.Add(referenciaActual.estadoref, us.numref, servicioActual.cuentacontable, servicioActual.nomservicio, referenciaActual.fechaemision.ToShortDateString(), referenciaActual.fechavencimiento.ToShortDateString(), referenciaActual.fechaestado.ToShortDateString(), Math.Round((us.costounit * us.cantidad), 2), clienteActual.nombre, clienteActual.apellidos,
                                            clienteActual.matricula, clienteActual.rfc, clienteActual.tipopersona, clienteActual.correoelectronico);
                                    }
                                    else
                                    {
                                        if (Convert.ToDateTime(fechaFinal) >= Convert.ToDateTime(referenciaActual.fechaemision.ToShortDateString()))
                                        {
                                            table.Rows.Add(referenciaActual.estadoref, us.numref, servicioActual.cuentacontable, servicioActual.nomservicio, referenciaActual.fechaemision.ToShortDateString(), referenciaActual.fechavencimiento.ToShortDateString(), referenciaActual.fechaestado.ToShortDateString(), Math.Round((us.costounit * us.cantidad), 2), clienteActual.nombre, clienteActual.apellidos,
                                                clienteActual.matricula, clienteActual.rfc, clienteActual.tipopersona, clienteActual.correoelectronico);
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
                String cadena = "Generó reporte de referencias histórico";
                int idUser = (int)Session["IdUser"];

                br.registroBitacora(cadena, idUser, "Reportes");

                using (MemoryStream stream = new MemoryStream())
                {
                    workbook.SaveAs(stream);
                    return File(stream.ToArray(), "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "Rp_HistoricoReferencias - " + fechaArchivo + ".xlsx");
                }
            }
        }

    }
}
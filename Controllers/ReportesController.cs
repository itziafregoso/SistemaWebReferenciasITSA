using Rotativa;
using SistemaWeb.Filters;
using SistemaWeb.Models;
using SistemaWeb.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace SistemaWeb.Controllers
{
    public class ReportesController : Controller
    {
        Servicios dbContext = new Servicios();
        DataBase db = new DataBase();

        // GET: Reportes
        public ActionResult Index()
        {
            return View();
        }

        RepositoryProducto repo;

        public ReportesController()
        {
            repo = new RepositoryProducto();
        }

        // Genera Ficha de pago en PDF
        public ActionResult OrdenDeCobro(String referencia)
        {
            IEnumerable<Servicios> newmodel = Session["FinCarrito"] as IEnumerable<Servicios>;
            ViewBag.Carrito = newmodel;

            ViewBag.Referencia = referencia;

            decimal total = 0;
            if (newmodel != null)
            {
                foreach (var valor in newmodel)
                {
                    total = total + Convert.ToDecimal(valor.costo);
                }

                total = decimal.Round(total, 2);
            }

            List<Servicios> prod = this.repo.GetProductos();

            ViewBag.lista = prod;

            ViewBag.Total = total;
            ViewBag.Vigencia = Session["FechaCaduca"];
            ViewBag.Folio = Session["Folio"];

            Referencias refAc = db.Referencias.Find(referencia);
            Clientes clienteHoy = db.Clientes.Find(refAc.IdCliente);
            OpcionesSistema op = db.OpcionesSistema.FirstOrDefault();

            ViewBag.NombreCliente = clienteHoy.nombre_ + " " +clienteHoy.apellidos;

            if(clienteHoy.rfc_ != "" && clienteHoy.rfc_ != null)
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

            return new Rotativa.ViewAsPdf("OrdenDeCobro") { FileName = "OrdenDeCobro.pdf" };
        }
    }
}
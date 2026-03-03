using SistemaWeb.Models;
using SistemaWeb.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace SistemaWeb.Controllers
{
    public class ProductoController : Controller
    {
        RepositoryProducto repo;

        public ProductoController()
        {
            repo = new RepositoryProducto();
        }


    }
}

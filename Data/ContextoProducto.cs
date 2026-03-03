using SistemaWeb.Models;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;

namespace SistemaWeb.Data
{
    public class ContextoProducto : DbContext
    {
        public ContextoProducto() : base("ProductosConexion") { }
        public DbSet<Producto> Productos { get; set; }
    }
}
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SistemaWeb.Models.ViewModels
{
    public class TableServiciosViewModel
    {
        public int IdServicio { get; set; }
        public string tipo { get; set; }
        public string area { get; set; }
        public bool estado { get; set; }
        public decimal costo { get; set; }
        public string nombre { get; set; }
        public int maxcobrar { get; set; }
        public int cuentacontable { get; set; }
    }
}
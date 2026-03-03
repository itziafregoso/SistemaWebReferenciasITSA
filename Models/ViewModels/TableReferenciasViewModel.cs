using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SistemaWeb.Models.ViewModels
{
    public class TableReferenciasViewModel
    {
        public DateTime fechaEmision { get; set; }
        public DateTime fechaEstado { get; set; }
        public DateTime fechaVencimiento { get; set; }
        public string noReferencia { get; set; }
        public string estado { get; set; }
        public decimal monto { get; set; }
        public string cliente { get; set; }

        public int anio { get; set; }
    }
}
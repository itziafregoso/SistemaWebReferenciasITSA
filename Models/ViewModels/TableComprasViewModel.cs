using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SistemaWeb.Models.ViewModels
{
    public class TableComprasViewModel
    {
        public string producto { get; set; }
        public int cantidad { get; set; }
        public decimal monto { get; set; }
    }
}
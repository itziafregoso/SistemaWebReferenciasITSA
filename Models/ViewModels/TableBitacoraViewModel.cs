using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SistemaWeb.Models.ViewModels
{
    public class TableBitacoraViewModel
    {
        public DateTime fecha { get; set; }
        public string hora { get; set; }
        public string operacion { get; set; }
        public string descripcion { get; set; }
        public string usuario { get; set; }
        public string ip { get; set; }
    }
}
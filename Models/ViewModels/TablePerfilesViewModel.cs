using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SistemaWeb.Models.ViewModels
{
    public class TablePerfilesViewModel
    {
        public int IdPerfil { get; set; }
        public int accesos { get; set; }
        public int restricciones { get; set; }
        public string nombre { get; set; }
    }
}
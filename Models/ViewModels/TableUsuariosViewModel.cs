using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SistemaWeb.Models.ViewModels
{
    public class TableUsuariosViewModel
    {
        public int IdUsuario { get; set; }
        public string nombre { get; set; }
        public string iniciales { get; set; }   
        public string area { get; set; }
        public string perfil { get; set; }
        public bool estado { get; set; }
    }
}
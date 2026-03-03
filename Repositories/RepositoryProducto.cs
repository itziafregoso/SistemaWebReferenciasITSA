using SistemaWeb.Data;
using SistemaWeb.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SistemaWeb.Repositories
{
    public class RepositoryProducto
    {
        DataBase contexto;
        public RepositoryProducto()
        {
            this.contexto = new DataBase();
        }

        //Obtiene servicios
        public List<Servicios> GetProductos()
        {
            var consulta = from datos in contexto.Servicios select datos;
            return consulta.ToList();
        }

        //Busca servicios
        public List<Servicios> BuscarProductos(List<int> id)
        {
            var consulta = from datos in contexto.Servicios where id.Contains(datos.contro) select datos;
            return consulta.ToList();
        }
    }
}
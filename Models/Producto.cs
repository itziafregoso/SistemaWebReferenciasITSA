using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace SistemaWeb.Models
{
    [Table("Servicios")]
    public class Producto
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        [Column("contro")]
        public int IdProducto { get; set; }

        [Column("nomservicio")]
        public String nombre { get; set; }

        [Column("costo")]
        public decimal Precio { get; set; }
        public virtual TipoServicios TipoServicios { get; set; }

    }
}
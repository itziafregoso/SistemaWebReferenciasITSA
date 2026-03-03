using SistemaWeb.Data;
using SistemaWeb.Models;
using System.Linq;
using System.Web.Mvc;

namespace SistemaWeb.Controllers
{
    public class HomeController : Controller
    {
        //Conexión a base de datos
        DataBase db = new DataBase();
        coloresSistema cs = new coloresSistema();
        bitacoraRegistro br = new bitacoraRegistro();

        //Mostrar Login
        public ActionResult Index(string msg, int intentos=5)
        {
            //Recupera colores almacenados en BD
            cs.recuperarColores();

            if (Session["User"] != null)
            {
                return RedirectToAction("Index","Perfil");
            }

            //Verifica el número de intentos restantes y mensaje de error para login
            ViewBag.Error = msg;
            ViewBag.intentos = intentos;
            return View();
        }


        // Ejecuta la comprobación de iniciar sesión
        [HttpPost]
        public ActionResult Login(string email, string password, int intentos)
        {
            string msg;
            var userLog = new UsuariosAd();

            if (email != "" && password != "")
            {
                // Busca el usuario por correo y contraseña
                using (Models.DataBase db = new Models.DataBase())
                {
                    var pUser = (from d in db.UsuariosAd
                                 where d.correoelectronico == email
                                 select d).FirstOrDefault();

                    password = Encrypt.GetSHA256(password);

                    if (pUser == null || pUser.contrasena != password)
                    {
                        // Envia mensaje con error de inicio de sesión
                        msg = "Datos incorrectos. Verifica usuario y contraseña. Te quedan " + (intentos-2) + " intentos.";
                        intentos = intentos - 1;
                        return RedirectToAction("Index", "Home", new { msg = msg, intentos = intentos });
                    }

                    if(pUser.estado == false)
                    {
                        // Envia mensaje con error de inicio de sesión
                        msg = "El usuario está deshabilitado en el sistema. Te quedan " + (intentos-2) + " intentos.";
                        intentos = intentos - 1;
                        return RedirectToAction("Index", "Home", new { msg = msg, intentos = intentos });
                    }

                    //Almacena los datos del usuario que inició sesión
                    HttpContext.Session["User"] = pUser;
                    Session["IdUser"] = pUser.IdUsuariosAd;
                    Session["NombreUser"] = pUser.nombre;
                    Session["Rol"] = pUser.PerfilesAd.nombreperfil;
                    userLog = pUser;
                }

                string cadena = "El usuario inició sesión";
                int idUser = (int)Session["IdUser"];

                br.registroBitacora(cadena, idUser, "Sesión");

                return RedirectToAction("Index", "Perfil");
            }
            else
            {
                // Envia mensaje con error de inicio de sesión
                msg = "Debes ingresar un correo y una contraseña.";
                return RedirectToAction("Index", "Home", new { msg = msg, intentos = intentos });
            }
        }


        public ActionResult Logout()
        {
            UsuariosAd userLog = HttpContext.Session["User"] as UsuariosAd;

            string cadena = "El usuario cerró sesión";
            int idUser = (int)Session["IdUser"];

            br.registroBitacora(cadena, idUser, "Sesión");

            Session["User"] = null;
            Session["NombreUser"] = null;
            Session["Rol"] = null;
            Session["Permisos"] = null;
            return RedirectToAction("Index");
        }



    }
}
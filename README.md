# 💳 Sistema Web de Referencias de Pago — ITSA

> Sistema web para la generación y gestión de referencias de pago del **Instituto Tecnológico Superior de Atlixco (ITSA)**. Desarrollado con ASP.NET MVC y SQL Server, siguiendo la metodología SCRUM.

![Versión](https://img.shields.io/badge/Versión-2.0-darkred)
![Framework](https://img.shields.io/badge/.NET_Framework-ASP.NET_MVC-blue)
![BD](https://img.shields.io/badge/Base_de_Datos-SQL_Server_2022-CC2927)
![Estado](https://img.shields.io/badge/Estado-En_producción-green)

---

## 📋 Descripción

Este sistema permite al personal administrativo del ITSA generar referencias de pago únicas para servicios institucionales, conciliar pagos bancarios y mantener un registro histórico por año fiscal. Fue solicitado por la **Subdirección de Servicios Administrativos** y aprobado por el **Departamento de Recursos Financieros**.

---

## 🚀 Funcionalidades principales

- **Gestión de usuarios y roles** con permisos personalizables
- **Catálogo de servicios** organizado por áreas y tipos
- **Generación de referencias de pago** únicas con ficha PDF
- **Conciliación bancaria** mediante carga de archivos de pago
- **Reportes financieros**: pólizas de pago y reportes de incidencias
- **Registro histórico** por año fiscal
- **Bitácora de eventos** de todas las operaciones del sistema
- **Carga masiva de alumnos** mediante archivo CSV

---

## 🛠️ Tecnologías

| Herramienta | Versión | Uso |
|---|---|---|
| Visual Studio | 2022 Community | IDE de desarrollo |
| ASP.NET MVC | .NET Framework | Framework web |
| C# | — | Lenguaje de programación |
| SQL Server | 2022 Express | Base de datos |
| SSMS | 19 | Administración de BD |
| Bootstrap | — | Estilos y UI |

---

## ⚙️ Requisitos

### Instalación local (desarrollo)

| Componente | Requerimiento mínimo |
|---|---|
| Sistema Operativo | Windows 7 o superior |
| Procesador | Intel Core Celeron G5905 3.5 GHz |
| RAM | 4 GB |
| Almacenamiento | 100 GB HDD |
| Resolución | 1280 × 720 |

### Servidor de producción

| Componente | Requerimiento mínimo |
|---|---|
| Sistema Operativo | Windows Server 2019 |
| Procesador | Intel Xeon E3-1290 3.60 GHz |
| RAM | 8 GB |
| Almacenamiento | 500 GB HDD |
| Software | IIS + SQL Server 2019 |

---

## 📦 Instalación local

### 1. Clonar el repositorio

```bash
git clone https://github.com/itziafregoso/SistemaWebReferenciasITSA.git
```

### 2. Instalar Visual Studio 2022 Community

Descargar desde [visualstudio.microsoft.com](https://visualstudio.microsoft.com/es/vs/) e instalar con:
- ✅ Carga de trabajo: **Desarrollo de ASP.NET y web**
- ✅ Componente: **Plantillas de proyecto y de elemento de .NET Framework**

### 3. Instalar SQL Server 2022 Express y SSMS 19

Instalar con configuración estándar. Crear un usuario y contraseña independientes de la autenticación de Windows.

### 4. Importar las bases de datos

Importar desde SSMS las dos bases de datos del proyecto:

```
BD_Sys_IP       → Base de datos principal
BD_Historico    → Base de datos de registros históricos
```

### 5. Configurar la cadena de conexión

En la raíz del proyecto, abrir `Web.config` y editar la etiqueta `<connectionStrings>`:

```xml
<!-- Con usuario y contraseña (recomendado) -->
<add name="BD_Sys_IP"
     connectionString="data source=TU_SERVIDOR\SQLEXPRESS;
                        initial catalog=BD_Sys_IP;
                        integrated security=False;
                        MultipleActiveResultSets=True;
                        user id=TU_USUARIO;
                        password=TU_CONTRASEÑA;"
     providerName="System.Data.EntityClient" />
```

Reemplazar `TU_SERVIDOR`, `TU_USUARIO` y `TU_CONTRASEÑA` con los valores de tu instalación local.

### 6. Compilar y ejecutar

Abrir el proyecto en Visual Studio y presionar `F5` o `Ctrl+F5` para iniciar la aplicación.

---

## 🗂️ Estructura del proyecto

```
SistemaWebReferenciasITSA/
├── Content/            # Estilos CSS e imágenes (Bootstrap, Site.css)
├── Controllers/        # Lógica de negocio (13 controladores)
├── Filters/            # Autenticación y autorización (AuthorizeUser, VerificarSesion)
├── Models/
│   ├── LINQModels/     # Modelos de acceso a datos
│   └── ViewModels/     # Modelos de vista para cada sección
├── Scripts/            # Librerías JavaScript (jQuery, Bootstrap — 31 archivos)
├── Views/              # Vistas .cshtml por controlador (50 archivos)
│   ├── Alumnos/
│   ├── Compra/
│   ├── Perfil/
│   ├── Servicios/
│   ├── Shared/         # Layout principal y vistas parciales
│   └── Usuarios/
└── Web.config          # Configuración de la aplicación y cadenas de conexión
```

---

## 👥 Roles de usuario

| Rol | Descripción |
|---|---|
| **Administrador** | Control total del sistema: usuarios, configuración, respaldos y restauración |
| **Administrador de Catálogo** | Gestión de servicios y generación de referencias de pago |
| **Administrador Financiero** | Conciliación bancaria, pólizas de pago y reportes de incidencias |

> Los roles son totalmente personalizables desde la interfaz del sistema.

---

## 🔒 Seguridad

- Autenticación por usuario y contraseña
- Contraseñas encriptadas con **SHA256**
- Control de acceso por operación mediante el parámetro `AuthorizeUser`
- Bitácora automática de todos los eventos del sistema (usuario, IP, operación, fecha y hora)

---

## 📄 Documentación

La documentación técnica completa del sistema está disponible en el archivo [`Manual_Tecnico_v2.0.pdf`](./docs/Manual_Tecnico_v2.0.pdf), que incluye:

- Diagramas de modelo relacional
- Diagrama de clases
- Diagrama de casos de uso
- Diccionario de datos
- Mapa de navegación
- Guía de instalación y configuración

---

## 📞 Soporte

Este soporte aplica para la **versión 3.0** del sistema. Para versiones posteriores, contactar a los desarrolladores más recientes registrados en el proyecto.

| Desarrollador | Correo |
|---|---|
| Itzia Fernanda Fregoso Martinez | itziafregoso@outlook.com |


Al reportar un problema, incluir: descripción del error, cuándo ocurrió, pasos para reproducirlo y cualquier mensaje de error recibido.

---

*Desarrollado para el Instituto Tecnológico Superior de Atlixco — Ingeniería en Sistemas Computacionales*

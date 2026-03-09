
-- --------------------------------------------------
-- Entity Designer DDL Script for SQL Server 2005, 2008, 2012 and Azure
-- --------------------------------------------------
-- Date Created: 03/16/2026 23:22:31
-- Generated from EDMX file: D:\Sys 2.1\version2.0\Models\BDSistema.edmx
-- --------------------------------------------------

SET QUOTED_IDENTIFIER OFF;
GO
USE [BD_Sys_IP];
GO
IF SCHEMA_ID(N'dbo') IS NULL EXECUTE(N'CREATE SCHEMA [dbo]');
GO

-- --------------------------------------------------
-- Dropping existing FOREIGN KEY constraints
-- --------------------------------------------------

IF OBJECT_ID(N'[dbo].[FK_Evento_UsuariosAd]', 'F') IS NOT NULL
    ALTER TABLE [dbo].[Eventos] DROP CONSTRAINT [FK_Evento_UsuariosAd];
GO
IF OBJECT_ID(N'[dbo].[FK_Referencias_Clientes]', 'F') IS NOT NULL
    ALTER TABLE [dbo].[Referencias] DROP CONSTRAINT [FK_Referencias_Clientes];
GO
IF OBJECT_ID(N'[dbo].[FK_Servicios_Areas]', 'F') IS NOT NULL
    ALTER TABLE [dbo].[Servicios] DROP CONSTRAINT [FK_Servicios_Areas];
GO
IF OBJECT_ID(N'[dbo].[FK_Servicios_TipoServicios]', 'F') IS NOT NULL
    ALTER TABLE [dbo].[Servicios] DROP CONSTRAINT [FK_Servicios_TipoServicios];
GO
IF OBJECT_ID(N'[dbo].[FK_UsuariosAd_AreasAd]', 'F') IS NOT NULL
    ALTER TABLE [dbo].[UsuariosAd] DROP CONSTRAINT [FK_UsuariosAd_AreasAd];
GO
IF OBJECT_ID(N'[dbo].[FK_UsuariosAd_PerfilesAd]', 'F') IS NOT NULL
    ALTER TABLE [dbo].[UsuariosAd] DROP CONSTRAINT [FK_UsuariosAd_PerfilesAd];
GO
IF OBJECT_ID(N'[dbo].[FK_Ventas_Referencias]', 'F') IS NOT NULL
    ALTER TABLE [dbo].[Ventas] DROP CONSTRAINT [FK_Ventas_Referencias];
GO
IF OBJECT_ID(N'[dbo].[FK_Ventas_Servicios]', 'F') IS NOT NULL
    ALTER TABLE [dbo].[Ventas] DROP CONSTRAINT [FK_Ventas_Servicios];
GO

-- --------------------------------------------------
-- Dropping existing tables
-- --------------------------------------------------

IF OBJECT_ID(N'[dbo].[Areas]', 'U') IS NOT NULL
    DROP TABLE [dbo].[Areas];
GO
IF OBJECT_ID(N'[dbo].[AreasAd]', 'U') IS NOT NULL
    DROP TABLE [dbo].[AreasAd];
GO
IF OBJECT_ID(N'[dbo].[Clientes]', 'U') IS NOT NULL
    DROP TABLE [dbo].[Clientes];
GO
IF OBJECT_ID(N'[dbo].[DiasHabiles]', 'U') IS NOT NULL
    DROP TABLE [dbo].[DiasHabiles];
GO
IF OBJECT_ID(N'[dbo].[Eventos]', 'U') IS NOT NULL
    DROP TABLE [dbo].[Eventos];
GO
IF OBJECT_ID(N'[dbo].[OpcionesSistema]', 'U') IS NOT NULL
    DROP TABLE [dbo].[OpcionesSistema];
GO
IF OBJECT_ID(N'[dbo].[PerfilesAd]', 'U') IS NOT NULL
    DROP TABLE [dbo].[PerfilesAd];
GO
IF OBJECT_ID(N'[dbo].[Referencias]', 'U') IS NOT NULL
    DROP TABLE [dbo].[Referencias];
GO
IF OBJECT_ID(N'[dbo].[Servicios]', 'U') IS NOT NULL
    DROP TABLE [dbo].[Servicios];
GO
IF OBJECT_ID(N'[dbo].[TipoServicios]', 'U') IS NOT NULL
    DROP TABLE [dbo].[TipoServicios];
GO
IF OBJECT_ID(N'[dbo].[UsuariosAd]', 'U') IS NOT NULL
    DROP TABLE [dbo].[UsuariosAd];
GO
IF OBJECT_ID(N'[dbo].[Ventas]', 'U') IS NOT NULL
    DROP TABLE [dbo].[Ventas];
GO

-- --------------------------------------------------
-- Creating all tables
-- --------------------------------------------------

-- Creating table 'Areas'
CREATE TABLE [dbo].[Areas] (
    [IdArea] int IDENTITY(1,1) NOT NULL,
    [nombrearea] nvarchar(30)  NOT NULL
);
GO

-- Creating table 'AreasAd'
CREATE TABLE [dbo].[AreasAd] (
    [IdAreaAd] int IDENTITY(1,1) NOT NULL,
    [nombrearead] nvarchar(25)  NOT NULL
);
GO

-- Creating table 'Clientes'
CREATE TABLE [dbo].[Clientes] (
    [IdCliente] int IDENTITY(1,1) NOT NULL,
    [rfc_] nvarchar(13)  NULL,
    [tipopersona] nvarchar(6)  NOT NULL,
    [matricula] nvarchar(9)  NULL,
    [nombre_] nvarchar(30)  NOT NULL,
    [apellidos] nvarchar(40)  NOT NULL,
    [correoelectronico] nvarchar(40)  NOT NULL,
    [calle] nvarchar(30)  NOT NULL,
    [numeroex] nvarchar(10)  NULL,
    [numeroin] nvarchar(5)  NULL,
    [colonia] nvarchar(30)  NOT NULL,
    [cp] nvarchar(10)  NOT NULL,
    [ciudad] nvarchar(30)  NOT NULL,
    [estado] nvarchar(30)  NOT NULL
);
GO

-- Creating table 'DiasHabiles'
CREATE TABLE [dbo].[DiasHabiles] (
    [Id] int IDENTITY(1,1) NOT NULL,
    [fechaHabil] datetime  NOT NULL
);
GO

-- Creating table 'Eventos'
CREATE TABLE [dbo].[Eventos] (
    [IdEvento] int IDENTITY(1,1) NOT NULL,
    [fecha] datetime  NOT NULL,
    [hora] nvarchar(5)  NOT NULL,
    [operacion] nvarchar(15)  NOT NULL,
    [descripcion] nvarchar(150)  NOT NULL,
    [IdUsuariosAd] int  NOT NULL,
    [ip] nvarchar(15)  NOT NULL
);
GO

-- Creating table 'OpcionesSistema'
CREATE TABLE [dbo].[OpcionesSistema] (
    [IdOpc] int IDENTITY(1,1) NOT NULL,
    [colorPrimario] varchar(7)  NOT NULL,
    [colorPrimarioAl] varchar(7)  NOT NULL,
    [colorSecundario] varchar(7)  NOT NULL,
    [colorTitulos] varchar(7)  NOT NULL,
    [colorTexto] varchar(7)  NOT NULL,
    [colorBEliminar] varchar(7)  NOT NULL,
    [colorBEliminarAl] varchar(7)  NOT NULL,
    [colorBVer] varchar(7)  NOT NULL,
    [colorBVerAl] varchar(7)  NOT NULL,
    [colorBEditar] varchar(7)  NOT NULL,
    [colorBEditarAl] varchar(7)  NOT NULL,
    [colorBComprar] varchar(7)  NOT NULL,
    [colorBComprarAl] varchar(7)  NOT NULL,
    [numCuenta] varchar(10)  NOT NULL,
    [nomBuzon] varchar(10)  NOT NULL,
    [cuenClave] varchar(18)  NOT NULL,
    [constanteRef] varchar(1)  NOT NULL,
    [numRap] varchar(18)  NOT NULL
);
GO

-- Creating table 'PerfilesAd'
CREATE TABLE [dbo].[PerfilesAd] (
    [IdPerfil] int IDENTITY(1,1) NOT NULL,
    [nombreperfil] nvarchar(25)  NOT NULL,
    [adminperfiles] bit  NOT NULL,
    [adminadareas] bit  NOT NULL,
    [verservicios] bit  NOT NULL,
    [admintiposervicios] bit  NOT NULL,
    [verreferencias] bit  NOT NULL,
    [adminusuarios] bit  NOT NULL,
    [administrarareas] bit  NOT NULL,
    [adminservicio] bit  NOT NULL,
    [bitacoraeventos] bit  NOT NULL,
    [subirarchivo] bit  NOT NULL,
    [generarcompra] bit  NOT NULL,
    [adminparametros] bit  NOT NULL,
    [adminalumnos] bit  NOT NULL,
    [veralumnos] bit  NOT NULL,
    [subiralumnos] bit  NOT NULL,
    [verhistorico] bit  NOT NULL,
    [restaurarsistema] bit  NOT NULL
);
GO

-- Creating table 'Referencias'
CREATE TABLE [dbo].[Referencias] (
    [numref] nvarchar(15)  NOT NULL,
    [estadoref] nvarchar(10)  NOT NULL,
    [fechaemision] datetime  NOT NULL,
    [fechaestado] datetime  NOT NULL,
    [fechavencimiento] datetime  NOT NULL,
    [monto] decimal(19,4)  NOT NULL,
    [IdCliente] int  NOT NULL
);
GO

-- Creating table 'Servicios'
CREATE TABLE [dbo].[Servicios] (
    [contro] int IDENTITY(1,1) NOT NULL,
    [IdArea] int  NOT NULL,
    [IdTS] int  NOT NULL,
    [nomservicio] nvarchar(30)  NOT NULL,
    [Objetivo] nvarchar(150)  NULL,
    [duracion] nvarchar(15)  NULL,
    [costo] decimal(7,2)  NOT NULL,
    [prerrequisitos] nvarchar(150)  NULL,
    [diasvigencia] int  NOT NULL,
    [serviciosmaxacobrar] int  NOT NULL,
    [cuetacontable] int  NOT NULL,
    [estado] bit  NOT NULL
);
GO

-- Creating table 'TipoServicios'
CREATE TABLE [dbo].[TipoServicios] (
    [IdTS] int IDENTITY(1,1) NOT NULL,
    [tipo] nvarchar(25)  NOT NULL
);
GO

-- Creating table 'UsuariosAd'
CREATE TABLE [dbo].[UsuariosAd] (
    [IdUsuariosAd] int IDENTITY(1,1) NOT NULL,
    [nombre] nvarchar(30)  NOT NULL,
    [iniciales] nvarchar(6)  NOT NULL,
    [IdPerfil] int  NOT NULL,
    [correoelectronico] nvarchar(60)  NOT NULL,
    [estado] bit  NOT NULL,
    [IdAreaAd] int  NOT NULL,
    [contrasena] nvarchar(200)  NULL
);
GO

-- Creating table 'Ventas'
CREATE TABLE [dbo].[Ventas] (
    [IdVenta] int IDENTITY(1,1) NOT NULL,
    [numref] nvarchar(15)  NOT NULL,
    [contro] int  NOT NULL,
    [cantidad] int  NOT NULL,
    [costount] decimal(19,4)  NOT NULL
);
GO

-- --------------------------------------------------
-- Creating all PRIMARY KEY constraints
-- --------------------------------------------------

-- Creating primary key on [IdArea] in table 'Areas'
ALTER TABLE [dbo].[Areas]
ADD CONSTRAINT [PK_Areas]
    PRIMARY KEY CLUSTERED ([IdArea] ASC);
GO

-- Creating primary key on [IdAreaAd] in table 'AreasAd'
ALTER TABLE [dbo].[AreasAd]
ADD CONSTRAINT [PK_AreasAd]
    PRIMARY KEY CLUSTERED ([IdAreaAd] ASC);
GO

-- Creating primary key on [IdCliente] in table 'Clientes'
ALTER TABLE [dbo].[Clientes]
ADD CONSTRAINT [PK_Clientes]
    PRIMARY KEY CLUSTERED ([IdCliente] ASC);
GO

-- Creating primary key on [Id] in table 'DiasHabiles'
ALTER TABLE [dbo].[DiasHabiles]
ADD CONSTRAINT [PK_DiasHabiles]
    PRIMARY KEY CLUSTERED ([Id] ASC);
GO

-- Creating primary key on [IdEvento] in table 'Eventos'
ALTER TABLE [dbo].[Eventos]
ADD CONSTRAINT [PK_Eventos]
    PRIMARY KEY CLUSTERED ([IdEvento] ASC);
GO

-- Creating primary key on [IdOpc] in table 'OpcionesSistema'
ALTER TABLE [dbo].[OpcionesSistema]
ADD CONSTRAINT [PK_OpcionesSistema]
    PRIMARY KEY CLUSTERED ([IdOpc] ASC);
GO

-- Creating primary key on [IdPerfil] in table 'PerfilesAd'
ALTER TABLE [dbo].[PerfilesAd]
ADD CONSTRAINT [PK_PerfilesAd]
    PRIMARY KEY CLUSTERED ([IdPerfil] ASC);
GO

-- Creating primary key on [numref] in table 'Referencias'
ALTER TABLE [dbo].[Referencias]
ADD CONSTRAINT [PK_Referencias]
    PRIMARY KEY CLUSTERED ([numref] ASC);
GO

-- Creating primary key on [contro] in table 'Servicios'
ALTER TABLE [dbo].[Servicios]
ADD CONSTRAINT [PK_Servicios]
    PRIMARY KEY CLUSTERED ([contro] ASC);
GO

-- Creating primary key on [IdTS] in table 'TipoServicios'
ALTER TABLE [dbo].[TipoServicios]
ADD CONSTRAINT [PK_TipoServicios]
    PRIMARY KEY CLUSTERED ([IdTS] ASC);
GO

-- Creating primary key on [IdUsuariosAd] in table 'UsuariosAd'
ALTER TABLE [dbo].[UsuariosAd]
ADD CONSTRAINT [PK_UsuariosAd]
    PRIMARY KEY CLUSTERED ([IdUsuariosAd] ASC);
GO

-- Creating primary key on [IdVenta] in table 'Ventas'
ALTER TABLE [dbo].[Ventas]
ADD CONSTRAINT [PK_Ventas]
    PRIMARY KEY CLUSTERED ([IdVenta] ASC);
GO

-- --------------------------------------------------
-- Creating all FOREIGN KEY constraints
-- --------------------------------------------------

-- Creating foreign key on [IdArea] in table 'Servicios'
ALTER TABLE [dbo].[Servicios]
ADD CONSTRAINT [FK_Servicios_Areas]
    FOREIGN KEY ([IdArea])
    REFERENCES [dbo].[Areas]
        ([IdArea])
    ON DELETE NO ACTION ON UPDATE NO ACTION;
GO

-- Creating non-clustered index for FOREIGN KEY 'FK_Servicios_Areas'
CREATE INDEX [IX_FK_Servicios_Areas]
ON [dbo].[Servicios]
    ([IdArea]);
GO

-- Creating foreign key on [IdAreaAd] in table 'UsuariosAd'
ALTER TABLE [dbo].[UsuariosAd]
ADD CONSTRAINT [FK_UsuariosAd_AreasAd]
    FOREIGN KEY ([IdAreaAd])
    REFERENCES [dbo].[AreasAd]
        ([IdAreaAd])
    ON DELETE NO ACTION ON UPDATE NO ACTION;
GO

-- Creating non-clustered index for FOREIGN KEY 'FK_UsuariosAd_AreasAd'
CREATE INDEX [IX_FK_UsuariosAd_AreasAd]
ON [dbo].[UsuariosAd]
    ([IdAreaAd]);
GO

-- Creating foreign key on [IdCliente] in table 'Referencias'
ALTER TABLE [dbo].[Referencias]
ADD CONSTRAINT [FK_Referencias_Clientes]
    FOREIGN KEY ([IdCliente])
    REFERENCES [dbo].[Clientes]
        ([IdCliente])
    ON DELETE NO ACTION ON UPDATE NO ACTION;
GO

-- Creating non-clustered index for FOREIGN KEY 'FK_Referencias_Clientes'
CREATE INDEX [IX_FK_Referencias_Clientes]
ON [dbo].[Referencias]
    ([IdCliente]);
GO

-- Creating foreign key on [IdUsuariosAd] in table 'Eventos'
ALTER TABLE [dbo].[Eventos]
ADD CONSTRAINT [FK_Evento_UsuariosAd]
    FOREIGN KEY ([IdUsuariosAd])
    REFERENCES [dbo].[UsuariosAd]
        ([IdUsuariosAd])
    ON DELETE NO ACTION ON UPDATE NO ACTION;
GO

-- Creating non-clustered index for FOREIGN KEY 'FK_Evento_UsuariosAd'
CREATE INDEX [IX_FK_Evento_UsuariosAd]
ON [dbo].[Eventos]
    ([IdUsuariosAd]);
GO

-- Creating foreign key on [IdPerfil] in table 'UsuariosAd'
ALTER TABLE [dbo].[UsuariosAd]
ADD CONSTRAINT [FK_UsuariosAd_PerfilesAd]
    FOREIGN KEY ([IdPerfil])
    REFERENCES [dbo].[PerfilesAd]
        ([IdPerfil])
    ON DELETE NO ACTION ON UPDATE NO ACTION;
GO

-- Creating non-clustered index for FOREIGN KEY 'FK_UsuariosAd_PerfilesAd'
CREATE INDEX [IX_FK_UsuariosAd_PerfilesAd]
ON [dbo].[UsuariosAd]
    ([IdPerfil]);
GO

-- Creating foreign key on [numref] in table 'Ventas'
ALTER TABLE [dbo].[Ventas]
ADD CONSTRAINT [FK_Ventas_Referencias]
    FOREIGN KEY ([numref])
    REFERENCES [dbo].[Referencias]
        ([numref])
    ON DELETE NO ACTION ON UPDATE NO ACTION;
GO

-- Creating non-clustered index for FOREIGN KEY 'FK_Ventas_Referencias'
CREATE INDEX [IX_FK_Ventas_Referencias]
ON [dbo].[Ventas]
    ([numref]);
GO

-- Creating foreign key on [IdTS] in table 'Servicios'
ALTER TABLE [dbo].[Servicios]
ADD CONSTRAINT [FK_Servicios_TipoServicios]
    FOREIGN KEY ([IdTS])
    REFERENCES [dbo].[TipoServicios]
        ([IdTS])
    ON DELETE NO ACTION ON UPDATE NO ACTION;
GO

-- Creating non-clustered index for FOREIGN KEY 'FK_Servicios_TipoServicios'
CREATE INDEX [IX_FK_Servicios_TipoServicios]
ON [dbo].[Servicios]
    ([IdTS]);
GO

-- Creating foreign key on [contro] in table 'Ventas'
ALTER TABLE [dbo].[Ventas]
ADD CONSTRAINT [FK_Ventas_Servicios]
    FOREIGN KEY ([contro])
    REFERENCES [dbo].[Servicios]
        ([contro])
    ON DELETE NO ACTION ON UPDATE NO ACTION;
GO

-- Creating non-clustered index for FOREIGN KEY 'FK_Ventas_Servicios'
CREATE INDEX [IX_FK_Ventas_Servicios]
ON [dbo].[Ventas]
    ([contro]);
GO

-- --------------------------------------------------
-- Script has ended
-- --------------------------------------------------
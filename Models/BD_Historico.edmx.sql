
-- --------------------------------------------------
-- Entity Designer DDL Script for SQL Server 2005, 2008, 2012 and Azure
-- --------------------------------------------------
-- Date Created: 03/16/2026 23:21:39
-- Generated from EDMX file: D:\Sys 2.1\version2.0\Models\BD_Historico.edmx
-- --------------------------------------------------

SET QUOTED_IDENTIFIER OFF;
GO
USE [BD_Historico];
GO
IF SCHEMA_ID(N'dbo') IS NULL EXECUTE(N'CREATE SCHEMA [dbo]');
GO

-- --------------------------------------------------
-- Dropping existing FOREIGN KEY constraints
-- --------------------------------------------------


-- --------------------------------------------------
-- Dropping existing tables
-- --------------------------------------------------

IF OBJECT_ID(N'[dbo].[HClientes]', 'U') IS NOT NULL
    DROP TABLE [dbo].[HClientes];
GO
IF OBJECT_ID(N'[dbo].[HReferencias]', 'U') IS NOT NULL
    DROP TABLE [dbo].[HReferencias];
GO
IF OBJECT_ID(N'[dbo].[HServicios]', 'U') IS NOT NULL
    DROP TABLE [dbo].[HServicios];
GO
IF OBJECT_ID(N'[dbo].[HVentas]', 'U') IS NOT NULL
    DROP TABLE [dbo].[HVentas];
GO

-- --------------------------------------------------
-- Creating all tables
-- --------------------------------------------------

-- Creating table 'HClientes'
CREATE TABLE [dbo].[HClientes] (
    [HIdCliente] int IDENTITY(1,1) NOT NULL,
    [Hanio] int  NOT NULL,
    [IdCliente] int  NOT NULL,
    [rfc] nvarchar(13)  NULL,
    [tipopersona] nvarchar(6)  NOT NULL,
    [matricula] nvarchar(9)  NULL,
    [nombre] nvarchar(30)  NOT NULL,
    [apellidos] nvarchar(40)  NOT NULL,
    [correoelectronico] nvarchar(40)  NOT NULL
);
GO

-- Creating table 'HReferencias'
CREATE TABLE [dbo].[HReferencias] (
    [Href] int IDENTITY(1,1) NOT NULL,
    [Hanio] int  NOT NULL,
    [numref] nvarchar(15)  NOT NULL,
    [estadoref] nvarchar(10)  NOT NULL,
    [fechaemision] datetime  NOT NULL,
    [fechaestado] datetime  NOT NULL,
    [fechavencimiento] datetime  NOT NULL,
    [monto] decimal(19,4)  NOT NULL,
    [IdCliente] int  NOT NULL
);
GO

-- Creating table 'HServicios'
CREATE TABLE [dbo].[HServicios] (
    [Hcontro] int IDENTITY(1,1) NOT NULL,
    [Hanio] int  NOT NULL,
    [contro] int  NOT NULL,
    [nomservicio] nvarchar(30)  NOT NULL,
    [costo] decimal(7,2)  NULL,
    [cuentacontable] int  NOT NULL,
    [nombrearea] nvarchar(30)  NOT NULL,
    [tipo] nvarchar(25)  NOT NULL
);
GO

-- Creating table 'HVentas'
CREATE TABLE [dbo].[HVentas] (
    [HIdVenta] int IDENTITY(1,1) NOT NULL,
    [Hanio] int  NOT NULL,
    [IdVenta] int  NOT NULL,
    [numref] nvarchar(15)  NOT NULL,
    [contro] int  NOT NULL,
    [cantidad] int  NOT NULL,
    [costounit] decimal(19,4)  NOT NULL
);
GO

-- --------------------------------------------------
-- Creating all PRIMARY KEY constraints
-- --------------------------------------------------

-- Creating primary key on [HIdCliente] in table 'HClientes'
ALTER TABLE [dbo].[HClientes]
ADD CONSTRAINT [PK_HClientes]
    PRIMARY KEY CLUSTERED ([HIdCliente] ASC);
GO

-- Creating primary key on [Href] in table 'HReferencias'
ALTER TABLE [dbo].[HReferencias]
ADD CONSTRAINT [PK_HReferencias]
    PRIMARY KEY CLUSTERED ([Href] ASC);
GO

-- Creating primary key on [Hcontro] in table 'HServicios'
ALTER TABLE [dbo].[HServicios]
ADD CONSTRAINT [PK_HServicios]
    PRIMARY KEY CLUSTERED ([Hcontro] ASC);
GO

-- Creating primary key on [HIdVenta] in table 'HVentas'
ALTER TABLE [dbo].[HVentas]
ADD CONSTRAINT [PK_HVentas]
    PRIMARY KEY CLUSTERED ([HIdVenta] ASC);
GO

-- --------------------------------------------------
-- Creating all FOREIGN KEY constraints
-- --------------------------------------------------

-- --------------------------------------------------
-- Script has ended
-- --------------------------------------------------
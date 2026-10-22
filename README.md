---

*Desarrollado para el proyecto Farmacia Genesis, Diriamba, Nicaragua.*

---

# 🏥 Farmacia Genesis API

![C#](https://img.shields.io/badge/C%23-239120?style=for-the-badge&logo=c-sharp&logoColor=white)
![ASP.NET Core](https://img.shields.io/badge/asp.net_core-%235C2D91.svg?style=for-the-badge&logo=dotnet&logoColor=white)
![SQL Server](https://img.shields.io/badge/sql_server-%23CC2927.svg?style=for-the-badge&logo=microsoft-sql-server&logoColor=white)
![Status](https://img.shields.io/badge/Status-En_Desarrollo-green?style=for-the-badge)

API RESTful desarrollada para el sistema de gestión integral de **Farmacia Genesis** (Diriamba). Este backend maneja la lógica de negocio, el control de inventario, facturación, compras y la administración general de la farmacia, proporcionando los datos necesarios para el frontend en React/Next.js.

## 📋 Tabla de Contenidos

- [Características Principales](#-características-principales)
- [Arquitectura y Tecnologías](#-arquitectura-y-tecnologías)
- [Requisitos Previos](#-requisitos-previos)
- [Instalación y Ejecución](#-instalación-y-ejecución)
- [Estructura de la Base de Datos](#-estructura-de-la-base-de-datos)
- [Documentación de la API](#-documentación-de-la-api)
- [Colaboradores](#-colaboradores)

---

## ✨ Características Principales

*   **Gestión de Inventario:** Control de stock en tiempo real, alertas de bajo inventario y seguimiento de fechas de caducidad.
*   **Módulo de Ventas/Facturación:** Procesamiento de transacciones y registro de comprobantes.
*   **Módulo de Compras:** Gestión de proveedores y reabastecimiento.
*   **Autenticación y Autorización:** Roles y permisos (Ej: Administrador, Cajero).
*   **Análisis de Datos:** Endpoints preparados para la generación de reportes y esquemas analíticos (Data Warehouse).

---

## 🛠️ Arquitectura y Tecnologías

El proyecto sigue una arquitectura de capas, implementando el **Patrón Repositorio** para desacoplar el acceso a datos.

*   **Lenguaje:** C#
*   **Framework:** ASP.NET Core (Web API)
*   **Base de Datos:** Microsoft SQL Server
*   **Acceso a Datos:**Entity Framework Core.
*   **Documentación:** Swagger
*   **Control de Versiones:** Git & GitHub

---

## ⚙️ Requisitos Previos

Antes de clonar el proyecto, asegúrate de tener instalado:

*   [.NET 8.0 SDK](https://dotnet.microsoft.com/download) (o la versión que estés utilizando).
*   [SQL Server](https://www.microsoft.com/es-es/sql-server/sql-server-downloads) (Express o Developer).
*   [SQL Server Management Studio (SSMS)](https://learn.microsoft.com/en-us/sql/ssms/download-sql-server-management-studio-ssms) o Azure Data Studio.
*   Git.

---

## 🚀 Instalación y Ejecución

Sigue estos pasos para levantar el entorno de desarrollo local:

1.  **Clonar el repositorio:**
    ```bash
    git clone [https://github.com/tu-usuario/api-farmacia-genesis.git](https://github.com/tu-usuario/api-farmacia-genesis.git)
    cd api-farmacia-genesis
    ```

2.  **Configurar la Base de Datos:**
    *   Abre el archivo `appsettings.Development.json`.
    *   Busca la cadena de conexión (`DefaultConnection`) y modifícala para que apunte a tu instancia local de SQL Server:
        ```json
        "ConnectionStrings": {
          "DefaultConnection": "Server=TU_SERVIDOR;Database=FarmaciaGenesisDB;Trusted_Connection=True;TrustServerCertificate=True;"
        }
        ```

3.  **Configurar secretos:**
    *   Para Docker Compose, copia `.env.example` a `.env` y configura una clave JWT aleatoria, única y de al menos 32 bytes, junto con las credenciales requeridas. No reutilices claves ni confirmes `.env` al repositorio.
    *   Para ejecutar con `dotnet run`, configura `JwtSettings__SecretKey` como variable de entorno; la aplicación no inicia si falta o es demasiado corta.
    *   Si credenciales anteriores estuvieron versionadas, rótalas: quitar `.env` del índice no las elimina del historial de Git.

4.  **Aplicar Migraciones / Scripts:**
    *   Si usas *Entity Framework*:
        ```bash
        dotnet ef database update
        ```
    *   Si usas *Scripts SQL puros/Dapper*, ejecuta el script de creación de tablas y *Stored Procedures* ubicado en la carpeta `/ScriptsSQL`.

5.  **Ejecutar la API:**
    ```bash
    dotnet run
    ```
    La API estará disponible en `https://localhost:7725`.

---

## 🗄️ Estructura de la Base de Datos

La base de datos está diseñada utilizando un modelo relacional. Además, incluye estructuras preparadas para procesos ETL y un modelo de estrella (Star Schema) para la inteligencia de negocios de la farmacia.

*Nota para el equipo: Las definiciones de los cubos OLAP y dimensiones se encuentran documentadas en Confluence.*

---

## 📖 Documentación de la API

La API está documentada con **Swagger**. Al ejecutar el proyecto en modo desarrollo, puedes acceder a la interfaz de prueba navegando a:

`https://localhost:port/swagger`

### Ejemplo de Petición (Obtener Producto)

**Endpoint:** `GET /api/productos/{id}`

**Respuesta Exitosa (200 OK):**
```json
{
  "idProducto": 105,
  "nombre": "Paracetamol 500mg",
  "categoria": "Analgésico",
  "stock": 150,
  "precioVenta": 25.50,
  "fechaCaducidad": "2027-12-01T00:00:00"
}

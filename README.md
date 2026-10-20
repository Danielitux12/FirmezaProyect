# Firmeza

Sistema de gestión y facturación desarrollado en .NET 10 siguiendo los principios de Clean Architecture.

## 🏗️ Arquitectura de la Solución

El proyecto está estructurado en capas desacopladas:

- **`Firmeza.Domain`**: Entidades del negocio (`Client`, `Product`, `Sale`, `SaleDetail`) e interfaces de repositorios.
- **`Firmeza.Application`**: Casos de uso, DTOs y lógica de negocio.
- **`Firmeza.Infrastructure`**: Persistencia de datos mediante Entity Framework Core, repositorios e integración con PostgreSQL.
- **`Firmeza.API`**: Interfaz de programación de aplicaciones REST (ASP.NET Core Web API).
- **`Firmeza.Web`**: Interfaz de usuario web (ASP.NET Core MVC).

## 🚀 Requisitos Previos

- [.NET 10 SDK](https://dotnet.microsoft.com/)
- [Docker](https://www.docker.com/) & Docker Compose
- [PostgreSQL 17](https://www.postgresql.org/) (opcional si se ejecuta con Docker)

## ⚙️ Configuración y Ejecución

1. **Configurar variables de entorno:**
   Copia el archivo de ejemplo y ajusta las credenciales si es necesario:
   ```bash
   cp .env.example .env
   ```

2. **Iniciar la base de datos con Docker:**
   ```bash
   docker compose up -d
   ```

3. **Compilar y ejecutar la API:**
   ```bash
   dotnet build
   dotnet run --project src/Firmeza.API
   ```

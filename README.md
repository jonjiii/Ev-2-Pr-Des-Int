# Proyecto — Instrucciones para correr en macOS y Windows

Este repositorio contiene dos microservicios .NET 9:

- Sistema de Mantención (gRPC + HTTP): [SistemaMantencion.Web/Program.cs](SistemaMantencion.Web/Program.cs) — proyecto [SistemaMantencion.Web/SistemaMantencion.Web.csproj](SistemaMantencion.Web/SistemaMantencion.Web.csproj)  
- Sistema Comercial (HTTP que consume gRPC): [SistemaComercial.Web/Program.cs](SistemaComercial.Web/Program.cs) — proyecto [SistemaComercial.Web/SistemaComercial.Web.csproj](SistemaComercial.Web/SistemaComercial.Web.csproj)

Archivos importantes:
- Protos: [Protos/camionetas.proto](Protos/camionetas.proto)  
- Cliente gRPC: [`MantencionGrpcClient`](SistemaComercial.Web/Services/MantencionGrpcClient.cs) ([file](SistemaComercial.Web/Services/MantencionGrpcClient.cs))  
- Servicio gRPC: [`MantencionGrpcService`](SistemaMantencion.Web/Services/MantencionGrpcService.cs) ([file](SistemaMantencion.Web/Services/MantencionGrpcService.cs))  
- DbContexts: [`MantencionDbContext`](SistemaMantencion.Web/Data/MantencionDbContext.cs) ([file](SistemaMantencion.Web/Data/MantencionDbContext.cs)), [`ComercialDbContext`](SistemaComercial.Web/Data/ComercialDbContext.cs) ([file](SistemaComercial.Web/Data/ComercialDbContext.cs))  
- Ajustes de conexión: [SistemaMantencion.Web/appsettings.json](SistemaMantencion.Web/appsettings.json), [SistemaComercial.Web/appsettings.json](SistemaComercial.Web/appsettings.json)

Requisitos (Windows)
- .NET 9 SDK instalado: https://dotnet.microsoft.com/
- PostgreSQL (o Docker). Las apps usan por defecto el puerto 5433 en appsettings; en Windows el Postgres suele usar 5432, ajusta el puerto si es necesario.

Opciones para PostgreSQL en Windows
1. Usar Docker (recomendado rápido):
   - Ejecuta (mapear host 5433 -> container 5432 para no cambiar appsettings):
     ```powershell
     docker run -d --name pg \
       -e POSTGRES_USER=postgres \
       -e POSTGRES_PASSWORD=postgres \
       -e POSTGRES_DB=postgres \
       -p 5433:5432 \
       postgres:15
     ```
   - Luego crea las bases que necesites (desde host):  
     ```powershell
     docker exec -it pg psql -U postgres -c "CREATE DATABASE mantencion_db;"
     docker exec -it pg psql -U postgres -c "CREATE DATABASE comercial_db;"
     ```

2. Usar instalación nativa:
   - Asegúrate del puerto y credenciales en [SistemaMantencion.Web/appsettings.json](SistemaMantencion.Web/appsettings.json) y [SistemaComercial.Web/appsettings.json](SistemaComercial.Web/appsettings.json). Cambia `Port=5433` a `Port=5432` si tu Postgres usa 5432.
   - Crea las DBs con psql:
     ```powershell
     createdb -U postgres mantencion_db
     createdb -U postgres comercial_db
     ```

Cómo correr la solución en Windows
1. Abrir PowerShell o CMD en la raíz del repositorio.
2. Restaurar y compilar:
   ```powershell
   dotnet restore
   dotnet build
   ```
3. Ejecutar primero el servicio de Mantención (gRPC server), que escucha por defecto en `http://localhost:5287`:
   ```powershell
   dotnet run --project SistemaMantencion.Web
   ```
   - Verifica la configuración en [SistemaMantencion.Web/Properties/launchSettings.json](SistemaMantencion.Web/Properties/launchSettings.json) y en [SistemaMantencion.Web/appsettings.json](SistemaMantencion.Web/appsettings.json).

4. Luego ejecutar el servicio Comercial:
   ```powershell
   dotnet run --project SistemaComercial.Web
   ```
   - Verifica que el cliente gRPC apunte a `http://localhost:5287` en [`MantencionGrpcClient`](SistemaComercial.Web/Services/MantencionGrpcClient.cs).

Notas útiles
- El servidor de mantención expone gRPC y también endpoints HTTP minimal API en [SistemaMantencion.Web/Program.cs](SistemaMantencion.Web/Program.cs).  
- Si cambias puertos, actualiza la URL en [`MantencionGrpcClient`](SistemaComercial.Web/Services/MantencionGrpcClient.cs) o en la inyección (`new MantencionGrpcClient("http://localhost:5287")`) en [SistemaComercial.Web/Program.cs](SistemaComercial.Web/Program.cs).  
- Las apps usan `EnsureCreated()` para inicializar esquemas; asegúrate de que la base exista o créala previamente. Los DbContexts están en [`MantencionDbContext`](SistemaMantencion.Web/Data/MantencionDbContext.cs) y [`ComercialDbContext`](SistemaComercial.Web/Data/ComercialDbContext.cs).

Firewall y puertos
- En Windows asegúrate de permitir los puertos 5278 (Comercial) y 5287 (Mantención) si el firewall los bloquea.

Problemas comunes
- Si el cliente gRPC no se conecta: confirma que Mantención esté corriendo y que la URL y puerto sean correctos en [`MantencionGrpcClient`](SistemaComercial.Web/Services/MantencionGrpcClient.cs).
- Si hay errores de DB: revisa credenciales y puerto en [SistemaMantencion.Web/appsettings.json](SistemaMantencion.Web/appsettings.json) y [SistemaComercial.Web/appsettings.json](SistemaComercial.Web/appsettings.json).

Eso es todo — corre Mantención primero, luego Comercial. Si quieres, puedo agregar scripts .bat/.ps1 para automatizar el arranque en Windows.
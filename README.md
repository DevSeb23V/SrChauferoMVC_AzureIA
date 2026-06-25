# SrChauferoMVC_AzureIA

Proyecto ASP.NET Core MVC para restaurante Sr. Chaufero.

## Incluye
- Modelo Vista Controlador.
- Login con autenticación multifactor tipo CAPTCHA.
- Diseño visual con Bootstrap.
- CRUD básico de platos y pedidos.
- Módulos de mesas, inventario y dashboard.
- Consumo configurable de Azure OpenAI.
- Conexión preparada para Azure SQL Database.

## Usuario demo
Usuario: admin
Contraseña: Admin123*

## Azure SQL
Editar `appsettings.json` y colocar la cadena de conexión de Azure SQL.

## Migración rápida
En Visual Studio: Herramientas > Administrador de paquetes NuGet > Consola:

Update-Database

También puedes ejecutar directamente; el proyecto usa EnsureCreated para crear tablas si la conexión es válida.

## Azure OpenAI
Configurar en `appsettings.json`:
- Endpoint
- ApiKey
- Deployment
- ApiVersion

Si no configuras Azure OpenAI, el módulo IA responde con una recomendación demo.

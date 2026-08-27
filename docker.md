# Docker - API

La API está containerizada mediante un Dockerfile multi-stage.

## Multi-stage build

El Dockerfile utiliza tres etapas:

- `build`: restaura dependencias y compila la aplicación usando el SDK de .NET.
- `publish`: genera los archivos necesarios para ejecutar la aplicación.
- `final`: utiliza únicamente ASP.NET Core Runtime y copia los archivos publicados.

Esto permite mantener la imagen final más limpia y sin las herramientas necesarias únicamente para compilar.

## Configuración

Las configuraciones sensibles no se almacenan en la imagen.

Las variables de entorno se proporcionan al ejecutar el contenedor y pueden variar según el ambiente (DEV, QA, PROD).

ASP.NET Core permite sobrescribir valores de `appsettings.json` mediante variables de entorno.

Por ejemplo:

`Jwt__SecretKey` → `Jwt:SecretKey`

## Puerto

La aplicación escucha en el puerto `8080` dentro del contenedor.
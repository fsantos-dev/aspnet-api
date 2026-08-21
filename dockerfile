#1. FROM trae el entorno de .NET

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build


#2. rutas de dependencias a instalar
COPY src/MiApp.API/MiApp.API.csproj src/MiApp.API/
COPY src/MiApp.Application/MiApp.Application.csproj src/MiApp.Application/
COPY src/MiApp.Domain/MiApp.Domain.csproj src/MiApp.Domain/
COPY src/MiApp.Infrastructure/MiApp.Infrastructure.csproj src/MiApp.Infrastructure/

#3. dotnet restore instala las dependencias de tu proyecto
RUN dotnet restore src/MiApp.API/MiApp.API.csproj


#4. copiar todo el codigo y compilar
COPY . .
WORKDIR /src/MiApp.API
RUN dotnet build -c Release -o /app/build

#5. Publicar la aplicación
#dotnet publish prepara tu aplicación para ser ejecutada fuera del entorno de desarrollo. 
#Es decir, toma tu proyecto .NET y genera todos los archivos necesarios 
#para desplegarlo en un servidor o dentro de un contenedor Docker.
FROM build AS publish
RUN dotnet publish -c Release -o /app/publish


#7. Imagen final (runtime)
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
# WORKDIR /app = "esta será la carpeta de trabajo de mi aplicación dentro del contenedor".
WORKDIR /app


#Documenta/declara el puerto que usa el contenedor.
EXPOSE 8080


# 7. Copiar los archivos publicados
COPY --from=publish /app/publish .

#8 Indicar que la aplicación se ejecutará en Production
ENV ASPNETCORE_ENVIRONMENT=Production


# 9. Punto de entrada
ENTRYPOINT ["dotnet", "MiApp.API.dll"]
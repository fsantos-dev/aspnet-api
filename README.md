# 📘 Guía de Buenas Prácticas para ASP.NET Core (Enterprise)

> Este documento recopila decisiones de arquitectura, patrones y buenas prácticas comunes en proyectos empresariales con ASP.NET Core y Clean Architecture.

---

# 1. Mapeo de DTOs

Existen dos enfoques principales:

## ✅ Mapeo Manual

- Más explícito.
- Más fácil de depurar.
- No requiere librerías externas.
- Muy recomendado para aprender.

```csharp
return new UserDto
{
    Id = user.Id,
    Name = user.Name,
    Email = user.Email
};
```

---

## ✅ AutoMapper

- Reduce código repetitivo.
- Muy usado en proyectos antiguos y grandes.
- Requiere configurar perfiles (`Profile`).

```csharp
CreateMap<User, UserDto>();
```

---

## Recomendación

✔ Aprender primero el mapeo manual.

Si el proyecto lo amerita, incorporar AutoMapper.

---

# 2. Acceso a datos

## Opción 1 - Repositorios

```
Controller
    ↓
Service
    ↓
Repository
    ↓
DbContext
```

---

## Opción 2 - Unit Of Work

```
Controller
    ↓
Service
    ↓
UnitOfWork
    ├── UserRepository
    ├── CategoryRepository
    └── ProductRepository
```

---

## Recomendación

En .NET moderno muchos proyectos usan únicamente repositorios porque `DbContext` ya implementa gran parte del patrón Unit of Work.

---

# 3. Logging

Se puede utilizar:

- Logs propios (`ILogger<T>`)
- Serilog

Serilog es una de las opciones más utilizadas en proyectos empresariales por su facilidad para almacenar logs en archivos, bases de datos o servicios externos.

---

# 4. Documentación de la API

Opciones:

- Swagger
- Scalar

Actualmente **Scalar** está ganando mucha popularidad por ofrecer una interfaz moderna.

---

# 5. Manejo global de excepciones

Nunca manejar excepciones en todos los controladores.

Opciones:

- Middleware propio de excepciones
- `IExceptionHandler` (ASP.NET Core moderno)

Centralizar siempre el manejo de errores.

---

# 6. Organización de Middlewares

## Opción sencilla

```csharp
app.UseAuthentication();
app.UseAuthorization();
app.UseSwagger();
...
```

---

## Opción recomendada

Crear un método de extensión.

```csharp
app.UseApplicationMiddlewares();
```

Dentro del método registrar todos los middlewares.

Esto mantiene limpio el `Program.cs`.

---

# 7. Mapeos Manuales

Aunque no se utilice AutoMapper, es recomendable tener una carpeta exclusiva para los mapeos.

```
Application
│
├── Mappings
│   ├── UserMapper.cs
│   ├── CategoryMapper.cs
│   └── ProductMapper.cs
```

Evita duplicar código.

---

# 8. Repositorios vs DbContext

Dos enfoques válidos.

## Con repositorios

```
Application
    ↓
Repository
    ↓
DbContext
```

---

## Sin repositorios

```
Application
    ↓
DbContext
```

Muchos proyectos modernos omiten los repositorios cuando únicamente utilizan Entity Framework Core.

---

# 9. LINQ

LINQ funciona sobre:

- Datos en memoria (`List<T>`)
- Base de datos (`IQueryable`)

En consultas a base de datos se recomienda:

1. Filtrar
2. Ordenar
3. Seleccionar
4. Paginar
5. Ejecutar (`ToListAsync()`)

Nunca llamar `ToListAsync()` demasiado pronto.

---

# 10. LINQ en memoria vs Base de datos

## Memoria

Ejecuta código C#.

```csharp
users.Where(u => MiMetodo(u));
```

---

## Base de datos

Solo puede traducir expresiones compatibles con SQL.

```csharp
_context.Users
    .Where(u => u.IsActive)
```

No todos los métodos de C# pueden convertirse a SQL.

---

# 11. Configuración de Entity Framework

Evitar tener todos los `ModelBuilder` dentro del `DbContext`.

Crear una carpeta:

```
Infrastructure
│
├── Configurations
│   ├── UserConfiguration.cs
│   ├── CategoryConfiguration.cs
│   └── ProductConfiguration.cs
```

Y registrar todas las configuraciones desde el `DbContext`.

```csharp
modelBuilder.ApplyConfigurationsFromAssembly(
    typeof(AppDbContext).Assembly);
```

---

# 12. Configuración de JWT

Es recomendable configurar:

- Validación del Token.
- Respuesta personalizada cuando el Token no existe.
- Respuesta personalizada cuando el Token es inválido.
- Respuesta cuando el Token expiró.

Esto mejora la experiencia del cliente.

---

# 13. Orden de los Middlewares

El orden es muy importante.

Ejemplo recomendado:

```
Exception Middleware

↓

Logging Middleware

↓

HTTPS

↓

Authentication

↓

Authorization

↓

Endpoints
```

La autenticación siempre debe ejecutarse antes que la autorización.

---

# 14. Secretos

Nunca guardar:

- Connection Strings
- JWT Secret
- API Keys
- Passwords

Dentro del repositorio.

Utilizar:

- Azure Key Vault
- AWS Secrets Manager
- HashiCorp Vault
- Variables de entorno
- Secretos locales

---

# 15. Seguridad de la información

Buenas prácticas:

- Utilizar siempre HTTPS.
- Encriptar información sensible.
- Nunca almacenar contraseñas en texto plano.
- Utilizar hash seguro para contraseñas (por ejemplo, BCrypt o el `PasswordHasher` de ASP.NET Core Identity).
- Validar y sanitizar la información recibida.
- Proteger datos sensibles tanto del cliente al servidor como del servidor al cliente cuando el negocio lo requiera.

---

# 📌 Resumen

| Tema | Opciones |
|-------|----------|
| DTO Mapping | Manual / AutoMapper |
| Acceso a datos | Repository / Unit Of Work / DbContext |
| Logs | ILogger / Serilog |
| Documentación | Swagger / Scalar |
| Excepciones | Middleware / IExceptionHandler |
| Middlewares | Program.cs / Extension Methods |
| Mapeos Manuales | Carpeta `Mappings` |
| Entity Framework | Configurations |
| LINQ | Memoria / Base de datos |
| Secretos | Vault o Variables de Entorno |
| Seguridad | HTTPS + Hash + Encriptación cuando aplique |

---

> **Nota:** No existe una única arquitectura "correcta". La elección entre estas alternativas dependerá del tamaño del proyecto, los estándares del equipo, los requisitos del negocio y el equilibrio entre simplicidad, mantenibilidad y escalabilidad.
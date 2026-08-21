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

En .NET moderno muchos proyectos usan únicamente repositorios porque `DbContext` ya implementa gran parte del patrón Unit of Work. Entonces se puede usar service -> repositorio -> dbcontext o incluso service -> dbcontext sin repositorio cuando las operaciones son sencillas, si no usamos dbcontext usamos service -> unitOfWork -> repositorios

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

# 16. Validación de datos

Existen tres enfoques principales para validar los datos que llegan a una aplicación ASP.NET Core.

## ✅ Opción 1 - Validaciones Manuales

Consiste en validar directamente dentro del servicio o caso de uso.

```csharp
if (string.IsNullOrWhiteSpace(dto.Name))
    throw new ArgumentException("El nombre es obligatorio.");

if (dto.Name.Length > 100)
    throw new ArgumentException("El nombre no puede superar los 100 caracteres.");
```

### Ventajas

- Muy sencillo de entender.
- No requiere librerías adicionales.
- Ideal para aprender los fundamentos.

### Desventajas

- Se repite mucho código.
- Mezcla la lógica de validación con la lógica del negocio.
- Difícil de mantener en proyectos grandes.

---

## ✅ Opción 2 - Data Annotations

ASP.NET Core permite validar utilizando atributos sobre los DTOs.

```csharp
public class CreateCategoryDto
{
    [Required]
    [MaxLength(100)]
    public string Name { get; set; } = string.Empty;

    [MaxLength(300)]
    public string? Description { get; set; }
}
```

### Ventajas

- Integración nativa con ASP.NET Core.
- Muy poco código.
- Adecuado para proyectos pequeños o medianos.

### Desventajas

- Los DTOs terminan llenos de atributos.
- Las reglas complejas son difíciles de expresar.
- Las validaciones quedan acopladas al DTO.

---

## ✅ Opción 3 - FluentValidation

Consiste en crear una clase exclusiva para validar cada DTO.

```csharp
public class CreateCategoryValidator : AbstractValidator<CreateCategoryDto>
{
    public CreateCategoryValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty()
            .MaximumLength(100);

        RuleFor(x => x.Description)
            .MaximumLength(300);
    }
}
```

### Ventajas

- Mantiene separadas las responsabilidades.
- Código limpio y fácil de mantener.
- Muy flexible para reglas complejas.
- Excelente integración con ASP.NET Core.
- Muy utilizado en proyectos empresariales modernos.

### Desventajas

- Requiere instalar una librería externa.
- Agrega una pequeña curva de aprendizaje.

---

## Validaciones del dominio

Las validaciones anteriores protegen principalmente la **entrada de datos** (DTOs).

Sin embargo, las **reglas de negocio** deben protegerse dentro de las entidades del dominio.

```csharp
public void Rename(string newName)
{
    if (string.IsNullOrWhiteSpace(newName))
        throw new DomainException("El nombre es obligatorio.");

    Name = newName;
}
```

Esto garantiza que la entidad nunca pueda quedar en un estado inválido, incluso si es creada o modificada desde un proceso interno, un Worker, una prueba unitaria u otro componente que no pase por un controlador HTTP.

---

## Recomendación

Para proyectos empresariales modernos con ASP.NET Core y Clean Architecture se suele utilizar la siguiente combinación:

- **FluentValidation** para validar los DTOs que llegan a la aplicación.
- **Entidades del dominio** para proteger las invariantes o reglas de negocio.
- **Entity Framework Core** para reforzar restricciones en la base de datos (índices únicos, longitudes máximas, claves foráneas, etc.).

Esta combinación proporciona una arquitectura limpia, mantenible y robusta.

---

# 15. TIPS DE CODIGO

- [FromBody] ya no es necesario en los parametros cuano usamos apicontroller
- Podemos obviar el contructor y usar primary constructor esto ahorra codigo
- cuando tenemos una sola dependencia podemos obviar los corchetes y usar => 
- Podemos usar IActionResult o ActionResult<CategoryDto> para mas legibilidad del codigo

Buenas prácticas:

- Utilizar siempre HTTPS.
- Encriptar información sensible.
- Nunca almacenar contraseñas en texto plano.
- Utilizar hash seguro para contraseñas (por ejemplo, BCrypt o el `PasswordHasher` de ASP.NET Core Identity).
- Validar y sanitizar la información recibida.
- Proteger datos sensibles tanto del cliente al servidor como del servidor al cliente cuando el negocio lo requiera.

---
# 15. ENTITIS
Tiene propiedades que representan datos -> columnas
Y propiedades que representan relaciones -> navegacion para acceder mas facil a los datoos con EF Core


# 16. AGREGAR NUEVAS COLUMNAS CUANDO LA APP YA ESTA EN PRODUCCION
Aqui toca tener cuidado con todos los escenarios por que la aplicacion ya esta en produccion y si agregamos una nueva columna toca revisar que valor se le va dejar o como peude afectar el cambio y tomar una decision


La regla que quiero que te quede

Cuando agregues cualquier campo a una tabla existente, hazte estas 4 preguntas:

1. ¿Puede ser NULL?
¿Es obligatorio?
2. ¿Qué valor tendrán los registros existentes?
¿Default?
¿Se puede calcular?
¿Se puede obtener de otra tabla?
¿Hay que preguntarle al negocio?
3. ¿Cuántos registros hay?
1.000 registros

es muy diferente de:

500.000.000 registros
4. ¿El cambio puede bloquear o afectar producción?

Esto depende del motor de BD, tamaño de tabla, índices, tipo de alteración, versión del motor, estrategia de despliegue, etc.

Por eso hay una escala aproximada
Campo nuevo con DEFAULT
        ↓
🟢 Generalmente sencillo


Campo nullable
        ↓
🟢 Generalmente sencillo


Campo obligatorio + datos existentes
        ↓
🟡 Necesita planificación


Campo derivado de otros datos
        ↓
🟡/🔴 Depende


Nueva FK con datos existentes
        ↓
🟡/🔴 Más delicado


Cambiar/eliminar una columna utilizada
        ↓
🔴 Potencialmente peligroso


Cambiar estructura de una tabla enorme
        ↓
🔴 Requiere planificación seria


# 16. PRUEBAS UNITARIAS (xunit)
- Moq 
- FluentAssertions
- [Fact] //es una prueba unitaria cada uno de estos 
- Mock<ICategoryRepository>	Crea un objeto falso del repositorio que no usa la base de datos real.
- Setup(...).ReturnsAsync(...)	Configura el mock para que devuelva una categoría predefinida cuando se llame a CreateAsync.
- Verify(...)	Verifica que el repositorio fue llamado una vez, lo que confirma que el servicio delegó correctamente.
- FluentAssertions	Permite escribir aserciones más legibles: result.Id.Should().Be(1)
- Prueba de excepción	Verifica que el servicio lance una ArgumentException cuando el nombre está vacío.


Pero CategoryService depende de otras cosas:

CategoryService
    │
    ├── ICurrentUserService
    ├── ICategoryRepository
    ├── IValidator<CreateCategoryDto>
    └── IValidator<UpdateCategoryDto>

Si usamos las implementaciones reales, estaríamos probando muchas cosas simultáneamente.

Por ejemplo:

CategoryService
     ↓
Repository real
     ↓
SQL Server
     ↓
Base de datos

Eso ya no sería un unit test puro.

## Algo importante sobre Arrange / Act / Assert

Cada [Fact] es una prueba completa y tiene su propio:

Arrange : preparar los datos
   ↓
Act : ejecutar la accion 
   ↓
Assert : comprobar la accion o lo esperado


3. ¿Qué hacemos con las dependencias?

Las reemplazamos por mocks.

Un mock es básicamente:

"Una versión falsa de una dependencia que yo controlo durante el test."

Por eso:

Mock<ICategoryRepository>

### FLUJO
Estándar: AAA

Toda prueba normalmente sigue:

ARRANGE → ACT → ASSERT
1. ARRANGE — ¿Qué necesito preparar?

Hazte estas preguntas:

1. ¿Qué método estoy probando?

Ejemplo:

UpdateAsync()

2. ¿Qué datos necesita?

Mira sus parámetros:

id
updateDto

Entonces preparas esos datos.

3. ¿Qué dependencias utiliza el método?

Mira el constructor del servicio.

Por ejemplo:

ICurrentUserService
ICategoryRepository
IValidator

Esas dependencias normalmente las conviertes en mocks.

4. ¿Qué comportamiento necesito de cada dependencia para que el escenario sea exitoso?

Por ejemplo:

CurrentUserService → devuelve UserId = 5
Validator          → dice "válido"
Repository         → encuentra la categoría

Aquí utilizas principalmente:

Setup()
2. ACT — ¿Qué estoy ejecutando?

Hazte una sola pregunta:

¿Cuál es exactamente la acción que quiero probar?

Por ejemplo:

UpdateAsync(id, updateDto)

Ese es el centro de la prueba.

Idealmente tienes una acción principal.

3. ASSERT — ¿Qué espero que ocurra?

Aquí tienes dos tipos de preguntas.

A. ¿Qué resultado espero?

Por ejemplo:

¿El resultado existe?
¿Tiene el ID correcto?
¿Tiene el nuevo nombre?
¿Tiene la nueva descripción?

Usas:

FluentAssertions

como:

result.Should()...
B. ¿Qué interacciones espero?

Pregúntate:

¿Qué dependencias debería haber utilizado mi servicio?

Por ejemplo:

¿Buscó la categoría?
¿La actualizó?
¿Cuántas veces?

Usas:

Verify()

Ejemplo conceptual:

GetByIdAsync → Once
UpdateAsync  → Once
El ciclo de preguntas

Cuando vayas a crear cualquier prueba exitosa, sigue este ciclo:

1. ¿QUÉ ESTOY PROBANDO?
        ↓
2. ¿QUÉ NECESITA PARA FUNCIONAR?
        ↓
3. ¿QUÉ DATOS NECESITO?
        ↓
4. ¿QUÉ DEPENDENCIAS TIENE?
        ↓
5. ¿QUÉ DEBEN DEVOLVER ESAS DEPENDENCIAS?
        ↓
6. EJECUTO EL MÉTODO
        ↓
7. ¿QUÉ RESULTADO ESPERO?
        ↓
8. ¿QUÉ DEPENDENCIAS DEBIERON SER LLAMADAS?

# 17. Docker
1. Crear docker file
2. Crear el compose con los servicios de la base de datos y la api(dockerfile)

- al crear el compose con todas las credenciales de configuracion lo mejor es tener todas estas llaves en un vault y en el compose refereciarlo

Dockerfile:

¿Cómo construyo y ejecuto mi aplicación?

Docker Compose:

¿Qué servicios necesita mi sistema y cómo se conectan?

1. Dónde ejecutas
Ejecución	      |  API	     |  SQL Server	            |  Secretos
Local sin Docker  |	Tu PC	     |  SQL Server de tu PC	    | User Secrets
Docker local	  | Contenedor	 |  Contenedor              | .env
Producción Docker | Contenedor	 |  Servidor/DB producción  | Vault

Exactamente: appsettings.Production.json sigue siendo útil aunque no pongas secretos ahí.

Su propósito es guardar configuración específica de producción que NO sea sensible. y los secretos en su forma correspondiente entonces compose o dotnet run ejecutaran el ambiente que le digamos pero los secretos los tomara de donde los tengamos

## Comandos que más vas a usar
docker build -t mi-app .        # construye una imagen desde el Dockerfile
docker run -p 3000:3000 mi-app  # crea y corre un contenedor
docker ps                       # lista contenedores corriendo
docker images                   # lista imágenes descargadas
docker stop <id>                # detiene un contenedor
docker compose build            # Construir las imágenes    
docker-compose up               # levanta todo lo definido en el compose
docker compose up -d --build    # construir y levantar al tiempo

normalemnte se tiene el compose loca con el que se crean las imagenes se suben a docker hub y alla se descargan en e servidor de prodccuion y con su propio compose levanta las imagenes


builder.Configuration toma las llaves no importan en donde esten en secretos locales o en vaules del servidor

# 18. ejecutar los diferentes ambientes 
$env:ASPNETCORE_ENVIRONMENT="Production" <-- ambiente a ejecutar
dotnet run


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
| Validación | Manual / Data Annotations / FluentValidation + Reglas de Dominio |


---

> **Nota:** No existe una única arquitectura "correcta". La elección entre estas alternativas dependerá del tamaño del proyecto, los estándares del equipo, los requisitos del negocio y el equilibrio entre simplicidad, mantenibilidad y escalabilidad.

# Siberia

With the increasing number of front-end frameworks, tools supporting **OData** protocol and the substantial amount of time saved from this type of two-way binding, 
time has come to create an OData pipeline. Siberia is a **Web API** exposing any type of database in order to **perform CRUD operations** on them. This pipeline supports also batch,
which allows to merge mutiple API calls into a single HTTP request. Hence reducing number of HTTP requests while increasing performance.

In the source code, **GenericODataController** defines how every CRUD operation is handled, regardless of database context or entity type. Then a **factory** was 
implemented to create as many controllers as entity types defined. This design pattern use a **source code generator** included in the project. The generator reads
every entity type from each database context in the project and creates corresponding controllers and entity data models. The result is a **fully operational OData pipeline** which 
is generated programmatically.

## Technology Stack

Solution contains two versions of the same OData Web API: **.NET** (previously .NET Core) and **.NET Framework**.

- .NET 6.0
- Entity Framework Core 6.0.6
- OData Core 7.12
- .NET Standard 2.0
- .NET Framework 4.8
- Entity Framework 6.4.4
- Microsoft.AspNet.OData 7.5.15

## How To Use .NET Version

This version is aimed to leverage the latest features available in .NET 6, like Source Generator. This Web API configuration is made of three steps: **use Scaffold-DbContext, update Program.cs and build the project**.

### 1. Scaffold Context And Entity Types

In order to run this OData pipeline, we need to add the different database contexts to the model.
This article treats the **database first approach**. In order to generate the context and all entity type classes, we use **Scaffold-DbContext** which is the standard way used with this type of approach. For this command to generate an entity type, **the database table must have a primary key**. 
Here is an example of command to use in NuGet Package Manager Console.

```sh
Scaffold-DbContext "Data Source=DELL-XPS;Initial Catalog=NordStreamDb;Integrated Security=True" Microsoft.EntityFrameworkCore.SqlServer -OutputDir Models/NordStreamDb -NoOnConfiguring -UseDatabaseNames -DataAnnotations -Force
```

**NoOnConfiguring** and **DataAnnotations** are mandatory as context is injected in Program.cs and OData need primary keys in entity classes. For more details, please check Entity Framework Core tools reference [here](https://docs.microsoft.com/en-us/ef/core/cli/powershell#scaffold-dbcontext).

### 2. Program.cs

The only files modified manually should be **Program.cs** and **appsettings.json**. In first place, add the model namespace of your own database like in this example. Then add OData route components and context dependency injection for each database you want to add to the Web API.
```cs
using Microsoft.AspNetCore.OData;
using Microsoft.AspNetCore.OData.Batch;
using Microsoft.EntityFrameworkCore;
using Siberia.CoreWebAPI.Models.EntityDataModel;
using Siberia.CoreWebAPI.Models.NordStreamDb; // Add your database model namespace like this

var builder = WebApplication.CreateBuilder(args);

var odataBatchHandler = new DefaultODataBatchHandler();
odataBatchHandler.MessageQuotas.MaxNestingDepth = 2;
odataBatchHandler.MessageQuotas.MaxOperationsPerChangeset = 10;
odataBatchHandler.MessageQuotas.MaxReceivedMessageSize = 100;

// Route your OData controllers and add dependency injection of the corresponding context here
builder.Services.AddControllers()
    .AddOData(opt => opt.AddRouteComponents("NordStreamDb", NordStreamDbEntityDataModel.GeEntityTypeDataModel(), odataBatchHandler));

builder.Services
    .AddDbContext<NordStreamDbContext>(options => options.UseSqlServer(builder.Configuration.GetConnectionString("NordStreamDbEntities")));
```

### 3. Just Build, It Generates Controllers

Controllers are generated programmatically using .NET new feature called **Source Generator**. This feature is a new kind of component that C# developers can use to do two major things:

1. Retrieve a Compilation object that represents all user code that is being compiled. This object can be inspected and you can write code that works with the syntax and semantic models for the code being compiled, just like with analyzers today.
1. Generate C# source files that can be added to a Compilation object during the course of compilation. In other words, you can provide additional source code as input to a compilation while the code is being compiled.

The generator is implemented in Source Library project. Web API includes the library reference as follows.
```csproj
<ProjectReference Include="..\Siberia.SourceLibrary\Siberia.SourceLibrary.csproj" OutputItemType="Analyzer" ReferenceOutputAssembly="false" />
```

By analyzing context files present in Compilation object, the source generator detect all entity types and implements a controller for each one of them. The same process exists for EntityDataModel required by OData. 
The resulting files will be found in **Siberia.CoreWebAPI > Dependencies > Analyzers > SourceGenerator**.

As this part is automated, no action is required. **Just build the project for triggering source code generation**. Swagger provides all available endpoint URLs.

## How To Use .NET Framework Version

**Do not update "Microsoft.Extensions.DependencyInjection" NuGet packages as "Microsoft.AspNet.OData" needs current project version**.

### 1. Add Model

The first step is adding your database model to solution folder. Do it with any approach as long as in the end a DbContext class and entity type classes are implemented. As OData endpoints will need a primary key for each entity type, add **key** manually if necessary like this.

```cs
using System.ComponentModel.DataAnnotations;

public partial class Pipeline
{
    [Key]
    public int Id { get; set; }
    public string Name { get; set; }
    public string Location { get; set; }
}
```

### 2. Implement OData Controllers

In order to expose each database entities, create a class that inherits the **GenericODataController** class for each entity type. GenericODataController provides an implementation of CRUD methods using OData protocol.
```cs
public class PipelinesController : GenericODataController<NordStreamDbEntities, Pipeline> { public PipelinesController() { Context = new NordStreamDbEntities(); } }
```


### 3. Route In Web.config

Lasts steps consist in building the **Entity Data Model** and **routing the controllers** previously defined. All exposed entity types are register in the builder before the EdmModel is created.

```cs
// NordStreamDb
var builder = new ODataConventionModelBuilder();
builder.EntitySet<Pipeline>("Pipelines");
builder.EntitySet<Society>("Societies");
config.MapODataServiceRoute(
    routeName: "NordStreamDb",
    routePrefix: "NordStreamDb",
    model: builder.GetEdmModel(),
    batchHandler: odataBatchHandler);
```
In this example, the URL to access **Pipeline** entities in **NordStreamDb** would look like that.

```
https://localhost:44300/NordStreamDb/Pipelines
```

## Deploy Sample Database, Postman

Siberia exposes a sample database called NordStreamDb. With the aim of deploying this database, excute SQL query contained in **NordStreamDb.sql** file. Now sample data are available, with JSON format, through OData route implemented in Siberia.

A collection of Postman HTTP requests is also included in the solution. These requests allows to test each Web API CRUD methods.
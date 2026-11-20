# Registro de Videojuegos — ADO.NET + SQLite

Programa de consola en C# que demuestra el acceso a datos con **ADO.NET** usando el proveedor oficial **Microsoft.Data.Sqlite**. Crea una base de datos SQLite, una tabla de videojuegos, inserta registros con parámetros y los consulta con un `SqliteDataReader`.

> Actividad de investigación — Programación en C# con .NET, Semana 12: Fundamentos de acceso a datos con ADO.NET.

## ¿Qué hace el programa?

1. Abre una conexión con `SqliteConnection` al archivo `videojuegos.db` (si no existe, se crea).
2. Crea la tabla `Videojuegos` (6 columnas) con `CREATE TABLE IF NOT EXISTS`.
3. Inserta 5 videojuegos con `SqliteCommand` y **parámetros** (`$titulo`, `$genero`, ...).
4. Consulta y muestra todos los registros con `SqliteDataReader`.
5. Hace una consulta filtrada (calificación ≥ 9.0) usando también un parámetro.
6. Libera todos los recursos con `using` (conexión, comandos y lectores).

Si se ejecuta por segunda vez, el programa detecta que los datos ya existen en el archivo y no los inserta de nuevo: así se comprueba la **persistencia**.

## Requisitos

- [.NET SDK 10](https://dotnet.microsoft.com/download) (o superior)
- Conexión a internet la primera vez (para descargar el paquete NuGet `Microsoft.Data.Sqlite`)

Comprueba la instalación con:

```bash
dotnet --version
```

## Cómo ejecutarlo

```bash
git clone https://github.com/luiscantillo/VideojuegosAdoNet.git
cd VideojuegosAdoNet
dotnet restore
dotnet run
```

El archivo `videojuegos.db` se crea en la carpeta desde donde se ejecuta el programa.

Para empezar desde cero, borra `videojuegos.db` y vuelve a ejecutar `dotnet run`.

## Estructura del proyecto

```
VideojuegosAdoNet/
├── Program.cs                 # Código fuente principal
├── VideojuegosAdoNet.csproj   # Proyecto .NET y referencia a Microsoft.Data.Sqlite
├── README.md
└── .gitignore
```

## Tabla `Videojuegos`

| Columna      | Tipo    | Descripción                       |
|--------------|---------|-----------------------------------|
| Id           | INTEGER | Clave primaria autoincremental    |
| Titulo       | TEXT    | Nombre del juego (único)          |
| Genero       | TEXT    | Género                            |
| Plataforma   | TEXT    | Consola o PC                      |
| Anio         | INTEGER | Año de lanzamiento                |
| Calificacion | REAL    | Nota de 0 a 10                    |

## Componentes de ADO.NET usados

| Componente         | Uso en el programa                                          |
|--------------------|-------------------------------------------------------------|
| `SqliteConnection` | Abre y cierra la conexión con el archivo `videojuegos.db`   |
| `SqliteCommand`    | Ejecuta `CREATE TABLE`, `INSERT` y `SELECT`                 |
| `SqliteParameter`  | Envía los valores de forma segura (evita inyección SQL)     |
| `SqliteDataReader` | Recorre los resultados fila por fila                        |

## Ver la base de datos (opcional)

Puedes abrir `videojuegos.db` con [DB Browser for SQLite](https://sqlitebrowser.org/) o, si tienes la herramienta `sqlite3`:

```bash
sqlite3 videojuegos.db "SELECT * FROM Videojuegos;"
```

## Autor

* **Estudiante:** Luis Miguel Cantillo Sarmiento
* **Asignatura:** Electiva de profundización I - 4BR
* **Docente:** Eduardo Pimienta
* **Institución:** Corporación Universitaria Americana

// ============================================================
//  Registro de Videojuegos con ADO.NET + SQLite
// ============================================================
using System.Text;
using Microsoft.Data.Sqlite;

Console.OutputEncoding = Encoding.UTF8;

// 1. Cadena de conexión: SQLite guarda TODA la base de datos en un solo archivo.
//    Si "videojuegos.db" no existe, el proveedor lo crea al abrir la conexión.
const string cadenaConexion = "Data Source=videojuegos.db";

Console.WriteLine("=== REGISTRO DE VIDEOJUEGOS (ADO.NET + SQLite) ===\n");

// 2. 'using' garantiza que la conexión se cierre y libere al terminar el bloque,
//    incluso si ocurre una excepción.
using (var conexion = new SqliteConnection(cadenaConexion))
{
    conexion.Open();
    Console.WriteLine($"[OK] Conexión abierta. Estado: {conexion.State}");
    string ruta = Path.GetFullPath(conexion.DataSource);
    Console.WriteLine($"     Archivo de base de datos: {ruta}\n");

    CrearTabla(conexion);
    InsertarVideojuegos(conexion);
    MostrarVideojuegos(conexion);
    MostrarPorCalificacionMinima(conexion, 9.0);
}

Console.WriteLine("\n[OK] Conexión cerrada y recursos liberados. Fin del programa.");


// ------------------------------------------------------------
// Crea la tabla si todavía no existe (DDL con ExecuteNonQuery)
// ------------------------------------------------------------
static void CrearTabla(SqliteConnection conexion)
{
    using var comando = conexion.CreateCommand();
    comando.CommandText = """
        CREATE TABLE IF NOT EXISTS Videojuegos (
            Id           INTEGER PRIMARY KEY AUTOINCREMENT,
            Titulo       TEXT    NOT NULL UNIQUE,
            Genero       TEXT    NOT NULL,
            Plataforma   TEXT    NOT NULL,
            Anio         INTEGER NOT NULL,
            Calificacion REAL    NOT NULL
        );
        """;
    comando.ExecuteNonQuery();
    Console.WriteLine("[OK] Tabla 'Videojuegos' lista.");
}

// ------------------------------------------------------------
// Inserta registros usando SqliteCommand + Parameters
// ------------------------------------------------------------
static void InsertarVideojuegos(SqliteConnection conexion)
{
    // Primero se cuenta cuántos registros hay (ExecuteScalar devuelve un solo valor).
    using var contar = conexion.CreateCommand();
    contar.CommandText = "SELECT COUNT(*) FROM Videojuegos;";
    long total = (long)contar.ExecuteScalar()!;
    if (total > 0)
    {
        Console.WriteLine($"[INFO] La tabla ya tiene {total} registros de una ejecución anterior.");
        Console.WriteLine("       No se insertan de nuevo: ¡los datos persisten en el archivo!\n");
        return;
    }

    // Datos de ejemplo (Videojuego es un record definido al final del archivo).
    Videojuego[] videojuegos =
    [
        new("The Legend of Zelda: Breath of the Wild", "Aventura", "Nintendo Switch", 2017, 9.7),
        new("Hollow Knight", "Metroidvania", "PC", 2017, 9.0),
        new("God of War", "Acción", "PlayStation 4", 2018, 9.4),
        new("Stardew Valley", "Simulación", "PC", 2016, 8.9),
        new("Celeste", "Plataformas", "Nintendo Switch", 2018, 9.1)
    ];

    // Un solo comando reutilizable: la instrucción SQL lleva marcadores ($titulo, ...)
    // y los valores viajan aparte como parámetros (evita la inyección SQL).
    using var insertar = conexion.CreateCommand();
    insertar.CommandText = """
        INSERT INTO Videojuegos (Titulo, Genero, Plataforma, Anio, Calificacion)
        VALUES ($titulo, $genero, $plataforma, $anio, $calificacion);
        """;

    var pTitulo       = insertar.Parameters.Add("$titulo",       SqliteType.Text);
    var pGenero       = insertar.Parameters.Add("$genero",       SqliteType.Text);
    var pPlataforma   = insertar.Parameters.Add("$plataforma",   SqliteType.Text);
    var pAnio         = insertar.Parameters.Add("$anio",         SqliteType.Integer);
    var pCalificacion = insertar.Parameters.Add("$calificacion", SqliteType.Real);

    foreach (var juego in videojuegos)
    {
        pTitulo.Value       = juego.Titulo;
        pGenero.Value       = juego.Genero;
        pPlataforma.Value   = juego.Plataforma;
        pAnio.Value         = juego.Anio;
        pCalificacion.Value = juego.Calificacion;

        int filas = insertar.ExecuteNonQuery();   // devuelve las filas afectadas
        Console.WriteLine($"[INSERT] {juego.Titulo} ({filas} fila insertada)");
    }
    Console.WriteLine();
}

// ------------------------------------------------------------
// Consulta todos los registros con SqliteDataReader
// ------------------------------------------------------------
static void MostrarVideojuegos(SqliteConnection conexion)
{
    using var consulta = conexion.CreateCommand();
    consulta.CommandText = """
        SELECT Id, Titulo, Genero, Plataforma, Anio, Calificacion
        FROM Videojuegos
        ORDER BY Id;
        """;

    Console.WriteLine("--- Videojuegos almacenados ---");
    ImprimirEncabezado();

    // El DataReader avanza fila por fila, solo hacia adelante y en modo lectura.
    using var lector = consulta.ExecuteReader();
    while (lector.Read())
    {
        ImprimirFila(lector);
    }
}

// ------------------------------------------------------------
// Consulta filtrada: también usa un parámetro en el SELECT
// ------------------------------------------------------------
static void MostrarPorCalificacionMinima(SqliteConnection conexion, double minima)
{
    using var consulta = conexion.CreateCommand();
    consulta.CommandText = """
        SELECT Id, Titulo, Genero, Plataforma, Anio, Calificacion
        FROM Videojuegos
        WHERE Calificacion >= $minima
        ORDER BY Calificacion DESC;
        """;
    consulta.Parameters.AddWithValue("$minima", minima);

    Console.WriteLine($"\n--- Videojuegos con calificación >= {minima:0.0} ---");
    ImprimirEncabezado();

    using var lector = consulta.ExecuteReader();
    int encontrados = 0;
    while (lector.Read())
    {
        ImprimirFila(lector);
        encontrados++;
    }
    Console.WriteLine($"Total encontrados: {encontrados}");
}

static void ImprimirEncabezado()
{
    Console.WriteLine($"{"Id",-3} {"Título",-40} {"Género",-13} " +
                      $"{"Plataforma",-16} {"Año",-5} {"Nota",4}");
    Console.WriteLine(new string('-', 86));
}

static void ImprimirFila(SqliteDataReader lector)
{
    // Se leen las columnas por posición (0, 1, 2...) con métodos tipados.
    int id            = lector.GetInt32(0);
    string titulo     = lector.GetString(1);
    string genero     = lector.GetString(2);
    string plataforma = lector.GetString(3);
    int anio          = lector.GetInt32(4);
    double nota       = lector.GetDouble(5);

    Console.WriteLine($"{id,-3} {titulo,-40} {genero,-13} " +
                      $"{plataforma,-16} {anio,-5} {nota,4:0.0}");
}

// Representa un videojuego antes de guardarlo en la base de datos.
record Videojuego(string Titulo, string Genero, string Plataforma, int Anio, double Calificacion);

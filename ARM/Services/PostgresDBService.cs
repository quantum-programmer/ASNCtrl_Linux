using Npgsql;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Runtime.InteropServices;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Serilog;
using static System.Net.Mime.MediaTypeNames;
using Microsoft.Extensions.Hosting;
using ARM.Models;
using Avalonia;
using HarfBuzzSharp;


namespace ARM.Services;

public class PostgresDBService : IDBService
{
    private static string? connectionString;
    private static string? _connectionString;
    private NpgsqlDataSource dataSource;
    private string? _currentUser;



    private void InitializeDataSource(string login, string decryptedPass)
    {
        string portKey = RuntimeInformation.IsOSPlatform(OSPlatform.Windows)
            ? "WindowsDBPort"
            : "LinuxDBPort";
        string hostKey = RuntimeInformation.IsOSPlatform(OSPlatform.Windows)
            ? "WindowsDBHost"
            : "LinuxDBHost";
        string dbKey = RuntimeInformation.IsOSPlatform(OSPlatform.Windows)
            ? "WindowsDBase"
            : "LinuxDBase";

        //if (!int.TryParse(ConfigurationManager.AppSettings[portKey], out int port))
        //    throw new ArgumentException("Invalid port configuration");

        //string host = ConfigurationManager.AppSettings[hostKey];
        //string dBase = ConfigurationManager.AppSettings[dbKey];

    //    if (string.IsNullOrEmpty(host) || string.IsNullOrEmpty(dBase))
    //        throw new ArgumentException("Missing database configuration");

    //    var builder = new NpgsqlConnectionStringBuilder
    //    {
    //        Host = host,
    //        Database = dBase,
    //        Port = port,
    //        Username = login, // Используем параметр метода
    //        Password = decryptedPass // Используем параметр метода
    //    };

    //    dataSource = NpgsqlDataSource.Create(builder.ConnectionString);
    //    _connectionString = builder.ConnectionString;
    }

    public string GetConnectionString()
    {
        return _connectionString ?? throw new InvalidOperationException("Data source is not initialized.");
    }

    public bool CheckConnection()
    {
        try
        {
            var connection = dataSource.OpenConnection();
            var result = connection != null;
            if (result) connection.Close();
            Log.Information("Cоединение с БД установлено.");
            return result;
        }
        catch (Exception ex)
        {
            Log.Information("Не удалось установить соединение с БД. " + ex.Message);
            return false;
        }
    }


    public async Task<int?> Login(string username, string password)
    {
        int? privileges = null;

        try
        {


            NpgsqlConnectionStringBuilder builder = new NpgsqlConnectionStringBuilder
            {
                /*Host = ConfigurationManager.AppSettings[
                    RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? "WindowsDBHost" : "LinuxDBHost"],
                Database = ConfigurationManager.AppSettings[
                    RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? "WindowsDBase" : "LinuxDBase"],
                Port = int.Parse(ConfigurationManager.AppSettings[
                    RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? "WindowsDBPort" : "LinuxDBPort"]),*/
                Host = "localhost",
                Database = "OilCtrl",
                Port = 5433,
                Username = username, // Переданный логин
                Password = password  // Переданный пароль
            };

            dataSource = NpgsqlDataSource.Create(builder.ConnectionString);

            await using var connection = await dataSource.OpenConnectionAsync();

            using var groupCommand = new NpgsqlCommand(@"SELECT max(
                          case when pg_roles.rolname = 'admin' then 5
                            else 
                          case when pg_roles.rolname = 'configurator' then 4
                            else
                          case when pg_roles.rolname = 'master' then 3
                            else 
                          case when pg_roles.rolname = 'operator' then 2
                            else 
                          case when pg_roles.rolname = 'reader' then 1
                            else
                          0
                          end end end end end) as level
                        FROM pg_roles
                        WHERE
                           EXISTS (SELECT * FROM pg_user WHERE pg_user.usename = @Login)
                           AND pg_has_role(@Login, oid, 'member')", connection);

            groupCommand.Parameters.AddWithValue("@Login", username);

            var reader = await groupCommand.ExecuteReaderAsync();
            if (reader.Read())
            {
                privileges = reader.IsDBNull(0) ? null : reader.GetInt32(0);
            }
            _currentUser = username;
            await LogActionAsync($"Пользователь {username} вошел в систему."); // Логируем вход
            Log.Information($"Пользователь {username} вошел в систему.");
            return privileges;
        }
        catch (Exception ex)
        {
            Log.Information($"Ошибка в методе 'Login': {ex.Message}");
            Console.WriteLine($"Ошибка в методе 'Login': {ex.Message}");
            return null;
        }
    }

    private async Task LogActionAsync(string actionDescription)
    {
        try
        {
            if (_currentUser == null)
            {
                _currentUser = "postgres";
            }

            string pcName = Environment.MachineName; // Получаем имя компьютера

            await using var connection = await dataSource.OpenConnectionAsync();
            using var command = new NpgsqlCommand(
                @"SET ROLE admin; INSERT INTO public.""UserLog"" (""Time"", ""PcName"", ""UserName"", ""Message"") 
              VALUES (@time, @pcName, @userName, @message);", connection);

            command.Parameters.AddWithValue("@time", DateTime.UtcNow);
            command.Parameters.AddWithValue("@pcName", pcName);
            command.Parameters.AddWithValue("@userName", _currentUser);
            command.Parameters.AddWithValue("@message", actionDescription);

            await command.ExecuteNonQueryAsync();
            Log.Information($"Создан лог авторизации пользователя (LogActionAsync)");
        }
        catch (Exception ex)
        {
            Log.Information($"Ошибка логирования действия: {ex.Message}");
        }
    }



    public async Task<List<PostModel>> LoadPostsAsync()
    {
        var posts = new List<PostModel>();

        try
        {
            string DecryptedPass = CryptoPass.GetPass();
            NpgsqlConnectionStringBuilder builder = new NpgsqlConnectionStringBuilder
            {
                /*Host = ConfigurationManager.AppSettings[
                    RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? "WindowsDBHost" : "LinuxDBHost"],
                Database = ConfigurationManager.AppSettings[
                    RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? "WindowsDBase" : "LinuxDBase"],
                Port = int.Parse(ConfigurationManager.AppSettings[
                    RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? "WindowsDBPort" : "LinuxDBPort"]),*/
                Host = "localhost",
                Database = "OilCtrl",
                Port = 5432,
                Username = "postgres", // Переданный логин
                Password = DecryptedPass//"j06gOuqDHwWkvpWf"  // Переданный пароль
            };

            dataSource = NpgsqlDataSource.Create(builder.ConnectionString);

            await using var connection = await dataSource.OpenConnectionAsync();
            using var command = new NpgsqlCommand(@"SELECT ""PostNumber"", ""VehicleNumber"", ""DriverName"", ""FuelType"", ""Volume"", ""Dose"", ""Side"", ""Earth"", ""MachineType"", ""id"" FROM public.""Posts"";", connection);

            var reader = await command.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                posts.Add(new PostModel
                {
                    PostNumber = reader.GetInt32(0),
                    VehicleNumber = reader.GetString(1),
                    DriverName = reader.GetString(2),
                    FuelType = reader.GetString(3),
                    Volume = reader.GetInt32(4),
                    Dose = reader.GetInt32(5),
                    Side = reader.GetInt16(6),
                    Earth = reader.GetInt32(7),
                    MachineType = reader.GetInt16(8),
                    id = reader.GetInt32(9),
                });
            }
        }
        catch (Exception ex)
        {
            Log.Information($"Ошибка при получении постов: {ex.Message}");
        }

        return posts;
    }


    //Справочник продуктов
    public async Task<List<ProductModel>> GetProductsAsync()
    {
        var result = new List<ProductModel>();

        if (dataSource == null)
            throw new InvalidOperationException("DataSource не инициализирован. Сначала вызовите GetPostsAsync.");

        await using var conn = await dataSource.OpenConnectionAsync();

        var sql = @"
SELECT 
    P.""Product"" AS Id,
    P.""Name"",
    P.""FactW_ProdType"",
    P.""FactW_K0"",
    P.""FactW_K1"",
    P.""FactW_K2"",
    P.""ShortName"",
    P.""FullName"",
    P.""Nomenkl"",
    P.""JRExclude"",
    P.""ProdGrp"",
    P.""IsResultProd"",
    P.""ProdColor""
FROM public.""Products"" P
ORDER BY P.""Product"";";

        await using var cmd = new NpgsqlCommand(sql, conn);
        await using var reader = await cmd.ExecuteReaderAsync();

        while (await reader.ReadAsync())
        {
            result.Add(new ProductModel
            {
                Product = reader.GetInt32(0),
                Name = reader.GetString(1),
                FactW_ProdType = reader.IsDBNull(2) ? null : reader.GetInt32(2),
                FactW_K0 = reader.GetDouble(3),
                FactW_K1 = reader.GetDouble(4),
                FactW_K2 = reader.GetDouble(5),
                ShortName = reader.IsDBNull(6) ? "" : reader.GetString(6),
                FullName = reader.IsDBNull(7) ? "" : reader.GetString(7),
                Nomenkl = reader.IsDBNull(8) ? "" : reader.GetString(8),
                JRExclude = reader.GetBoolean(9),
                ProdGrp = reader.IsDBNull(10) ? null : reader.GetInt32(10),
                IsResultProd = reader.GetBoolean(11),
                ProdColor = reader.IsDBNull(12) ? null : reader.GetInt32(12),
                // --- Lookup списки ---
                FactWProdTypesLookup = Lookups.FactWProdTypes,
                ProdColorsLookup = Lookups.ProdColors
            });
        }

        return result;
    }

    public async Task<int> InsertProductAsync(ProductModel p)
    {
        if (dataSource == null)
            throw new InvalidOperationException("DataSource не инициализирован. Сначала вызовите GetPostsAsync.");

        await using var conn = await dataSource.OpenConnectionAsync();

        var columns = new List<string>();
        var values = new List<string>();
        var parameters = new List<NpgsqlParameter>();

        void AddParam(string column, object? value)
        {
            if (value != null)
            {
                string paramName = "@" + column;
                columns.Add($"\"{column}\"");
                values.Add(paramName);
                parameters.Add(new NpgsqlParameter(paramName, value));
            }
        }

        // Добавляем только ненулевые поля
        AddParam("Product", p.Product);
        AddParam("Name", p.Name);
        AddParam("FactW_ProdType", p.FactW_ProdType);
        AddParam("FactW_K0", p.FactW_K0);
        AddParam("FactW_K1", p.FactW_K1);
        AddParam("FactW_K2", p.FactW_K2);
        AddParam("ShortName", p.ShortName);
        AddParam("FullName", p.FullName);
        AddParam("Nomenkl", p.Nomenkl);
        AddParam("JRExclude", p.JRExclude);
        AddParam("ProdGrp", p.ProdGrp);
        AddParam("IsResultProd", p.IsResultProd);
        AddParam("ProdColor", p.ProdColor);

        string sql = $@"
    INSERT INTO ""Products"" ({string.Join(", ", columns)})
    VALUES ({string.Join(", ", values)})
    RETURNING ""Product"";";

        await using var cmd = new NpgsqlCommand(sql, conn);
        cmd.Parameters.AddRange(parameters.ToArray());

        var result = await cmd.ExecuteScalarAsync();
        return Convert.ToInt32(result); // Product у тебя smallint → вернётся int
    }


    public async Task UpdateProductAsync(ProductModel p)
    {
        if (dataSource == null)
            throw new InvalidOperationException("DataSource не инициализирован. Сначала вызовите GetPostsAsync.");

        await using var conn = await dataSource.OpenConnectionAsync();

        var sql = @"
    UPDATE public.""Products""
    SET ""Product""=@Product,
        ""Name""=@Name,
        ""FactW_ProdType""=@FactW_ProdType,
        ""FactW_K0""=@FactW_K0,
        ""FactW_K1""=@FactW_K1,
        ""FactW_K2""=@FactW_K2,
        ""ShortName""=@ShortName,
        ""FullName""=@FullName,
        ""Nomenkl""=@Nomenkl,
        ""JRExclude""=@JRExclude,
        ""ProdGrp""=@ProdGrp,
        ""IsResultProd""=@IsResultProd,
        ""ProdColor""=@ProdColor
    WHERE ""Product""=@OriginalProduct;";

        await using var cmd = new NpgsqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("@Product", p.Product);
        cmd.Parameters.AddWithValue("@OriginalProduct", p.OriginalProduct);
        cmd.Parameters.AddWithValue("@Name", p.Name);
        cmd.Parameters.AddWithValue("@FactW_ProdType", (object?)p.FactW_ProdType ?? DBNull.Value);
        cmd.Parameters.AddWithValue("@FactW_K0", p.FactW_K0);
        cmd.Parameters.AddWithValue("@FactW_K1", p.FactW_K1);
        cmd.Parameters.AddWithValue("@FactW_K2", p.FactW_K2);
        cmd.Parameters.AddWithValue("@ShortName", p.ShortName);
        cmd.Parameters.AddWithValue("@FullName", p.FullName);
        cmd.Parameters.AddWithValue("@Nomenkl", p.Nomenkl);
        cmd.Parameters.AddWithValue("@JRExclude", p.JRExclude);
        cmd.Parameters.AddWithValue("@ProdGrp", (object?)p.ProdGrp ?? DBNull.Value);
        cmd.Parameters.AddWithValue("@IsResultProd", p.IsResultProd);
        cmd.Parameters.AddWithValue("@ProdColor", (object?)p.ProdColor ?? DBNull.Value);

        await cmd.ExecuteNonQueryAsync();
    }

    public async Task DeleteProductAsync(int id)
    {
        if (dataSource == null)
            throw new InvalidOperationException("DataSource не инициализирован. Сначала вызовите GetPostsAsync.");

        await using var conn = await dataSource.OpenConnectionAsync();

        var sql = @"DELETE FROM public.""Products"" WHERE ""Product""=@Id;";
        await using var cmd = new NpgsqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("@Id", id);

        await cmd.ExecuteNonQueryAsync();
    }


    //Справочник постов
    public async Task<List<PostModel>> GetPostsAsync()
    {
        var result = new List<PostModel>();

        if (dataSource == null)
            throw new InvalidOperationException("DataSource не инициализирован. Сначала вызовите GetPostsAsync.");

        await using var conn = await dataSource.OpenConnectionAsync();

        var sql = @"
SELECT
  P.""Place"",
  P.""Post"",
  P.""Point"",
  P.""Side"",
  P.""FactVMethod"",
  P.""FactWMethod"",
  P.""Direction"",
  P.""MachineType"",
  P.""UserTypedTemperature"",
  P.""UpDownFill"",
  P.""StartReversed"",
  P.""CtrlType"",
  P.""KMXFill"",
  P.""isEPost"",
  (SELECT I.""Item"" FROM ""Items"" I WHERE I.""Name"" = 'Post' || P.""Post""::text || '.Density' LIMIT 1) AS ""Density"",
  (SELECT I.""Item"" FROM ""Items"" I WHERE I.""Name"" = 'Post' || P.""Post""::text || '.Temperature' LIMIT 1) AS ""Temperature"",
  (SELECT I.""Item"" FROM ""Items"" I WHERE I.""Name"" = 'Post' || P.""Post""::text || '.LabDensity' LIMIT 1) AS ""LabDensity"",
  (SELECT I.""Item"" FROM ""Items"" I WHERE I.""Name"" = 'Post' || P.""Post""::text || '.LabTemperature' LIMIT 1) AS ""LabTemperature"",
  (SELECT CASE WHEN COUNT(DISTINCT T.""Tank"") = 1 THEN MIN(T.""Tank"") ELSE NULL END
   FROM ""DTanks"" T
   INNER JOIN ""DTubes"" U ON T.""Point"" = U.""Point1""
   WHERE U.""Point2"" = P.""Point"") AS ""Tank"",
  (SELECT H.""Number""
   FROM ""DObjs"" H
   INNER JOIN ""Items"" IP
     ON IP.""Name"" = 'HydroMeter' || H.""Number""::text || '.Point'
    AND IP.""Item"" = P.""Point"" -- заменил ItemTimeToVal, см. выше
   WHERE H.""ClassName"" = 'HydroMeter'
   LIMIT 1) AS ""HydroMeter"",
  (SELECT X.""ValN""
   FROM ""DObjs"" O
   LEFT JOIN ""Items"" IPt
     ON IPt.""Name"" = 'Post' || O.""Number""::text || '.PointOut'
   LEFT JOIN ""DObjsEx"" X
     ON X.""Obj"" = O.""Obj"" AND X.""Param"" = 'MType'
   WHERE O.""ClassName"" = 'Mixer' AND O.""Number"" = P.""Post""
   LIMIT 1) AS ""MType"",
  P.""LastRecordBUIJournal"",
  P.""TotalVLast""
FROM ""DPosts"" P
ORDER BY P.""Place"", P.""Post"";";

        await using var cmd = new NpgsqlCommand(sql, conn);
    await using var reader = await cmd.ExecuteReaderAsync();

    while (await reader.ReadAsync())
    {
        result.Add(new PostModel
        {
            Place = reader.GetInt16(0),
            Post = reader.GetInt16(1),
            Point = reader.IsDBNull(2) ? null : reader.GetInt16(2),
            Side = reader.IsDBNull(3) ? null : reader.GetInt16(3),
            FactVMethod = reader.IsDBNull(4) ? null : reader.GetInt16(4),
            FactWMethod = reader.IsDBNull(5) ? null : reader.GetInt16(5),
            Direction = reader.IsDBNull(6) ? null : reader.GetInt16(6),
            MachineType = reader.IsDBNull(7) ? null : reader.GetInt16(7),
            UserTypedTemperature = reader.IsDBNull(8) ? null : reader.GetBoolean(8),
            UpDownFill = reader.IsDBNull(9) ? null : reader.GetInt16(9),
            StartReversed = reader.IsDBNull(10) ? null : reader.GetInt16(10),
            CtrlType = reader.IsDBNull(11) ? null : reader.GetInt16(11),
            KMXFill = reader.IsDBNull(12) ? null : reader.GetBoolean(12),
            IsEPost = reader.IsDBNull(13) ? null : reader.GetInt16(13),
            Density = reader.IsDBNull(14) ? null : reader.GetInt16(14),
            Temperature = reader.IsDBNull(15) ? null : reader.GetInt16(15),
            LabDensity = reader.IsDBNull(16) ? null : reader.GetInt16(16),
            LabTemperature = reader.IsDBNull(17) ? null : reader.GetInt16(17),
            Tank = reader.IsDBNull(18) ? null : reader.GetInt32(18),
            HydroMeter = reader.IsDBNull(19) ? null : reader.GetInt32(19),
            MType = reader.IsDBNull(20) ? null : reader.GetInt32(20),
            LastRecordBUIJournal = reader.IsDBNull(21) ? null : reader.GetInt64(21),
            TotalVLast = reader.IsDBNull(22) ? null : reader.GetDecimal(22),
            OriginalPost = reader.GetInt16(1),
            //FactWProdTypesLookup = Lookups.FactWProdTypes,
            //ProdColorsLookup = Lookups.ProdColors
            FactVMethodsLookup = Lookups.FactVMethods,
            FactWMethodsLookup = Lookups.FactWMethods,
            DirectionsLookup = Lookups.Directions,
            MachineTypesLookup = Lookups.MachineTypes,
            CtrlTypesLookup = Lookups.CtrlTypes,
            UpDownFillsLookup = Lookups.UpDownFills,
            UserTypedTemperaturesLookup = Lookups.UserTypedTemperatures,
            StartReversedsLookup = Lookups.StartReverseds
        });
    }

    return result;
}

    public async Task<int> InsertPostAsync(PostModel model)
    {
        await using var conn = await dataSource.OpenConnectionAsync();

        var columns = new List<string>();
        var values = new List<string>();
        var parameters = new List<NpgsqlParameter>();

        void AddParam(string column, object? value)
        {
            if (value != null)
            {
                string paramName = "@" + column;
                columns.Add($"\"{column}\"");
                values.Add(paramName);
                parameters.Add(new NpgsqlParameter(paramName, value));
            }
        }

        // Добавляем только НЕ null поля
        AddParam("Post", model.Post);
        AddParam("Place", model.Place);
        AddParam("Point", model.Point);
        AddParam("Side", model.Side);
        AddParam("Direction", model.Direction);
        AddParam("FactVMethod", model.FactVMethod);
        AddParam("FactWMethod", model.FactWMethod);
        AddParam("MachineType", model.MachineType);
        AddParam("UserTypedTemperature", model.UserTypedTemperature);
        AddParam("StartReversed", model.StartReversed);
        AddParam("UpDownFill", model.UpDownFill);
        AddParam("CtrlType", model.CtrlType);
        AddParam("IsEPost", model.IsEPost);
        AddParam("KMXFill", model.KMXFill);
        AddParam("LastRecordBUIJournal", model.LastRecordBUIJournal);
        AddParam("TotalVLast", model.TotalVLast);

        string sql = $@"
        INSERT INTO ""DPosts"" ({string.Join(", ", columns)})
        VALUES ({string.Join(", ", values)})
        RETURNING ""Post"";";

        await using var cmd = new NpgsqlCommand(sql, conn);
        cmd.Parameters.AddRange(parameters.ToArray());

        var result = await cmd.ExecuteScalarAsync();

        // PostgreSQL вернёт smallint → безопасно приводим
        return Convert.ToInt32(result);
    }


    public async Task UpdatePostAsync(PostModel model)
    {
        await using var conn = await dataSource.OpenConnectionAsync();
        var sql = @"UPDATE ""DPosts"" SET
                  ""Place"" = @Place,
                  ""Post"" = @Post,
                  ""Point"" = @Point,
                  ""Side"" = @Side,
                  ""FactVMethod"" = @FactVMethod,
                  ""FactWMethod"" = @FactWMethod,
                  ""Direction"" = @Direction,
                  ""MachineType"" = @MachineType,
                  ""UserTypedTemperature"" = @UserTypedTemperature,
                  ""UpDownFill"" = @UpDownFill,
                  ""StartReversed"" = @StartReversed,
                  ""CtrlType"" = @CtrlType,
                  ""KMXFill"" = @KMXFill,
                  ""isEPost"" = @IsEPost,
                  ""LastRecordBUIJournal"" = @LastRecordBUIJournal,
                  ""TotalVLast"" = @TotalVLast
                WHERE ""Post"" = @OriginalPost;";

        await using var cmd = new NpgsqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("@Place", model.Place);
        cmd.Parameters.AddWithValue("@Point", (object?)model.Point ?? DBNull.Value);
        cmd.Parameters.AddWithValue("@Side", (object?)model.Side ?? DBNull.Value);
        cmd.Parameters.AddWithValue("@FactVMethod", (object?)model.FactVMethod ?? DBNull.Value);
        cmd.Parameters.AddWithValue("@FactWMethod", (object?)model.FactWMethod ?? DBNull.Value);
        cmd.Parameters.AddWithValue("@Direction", (object?)model.Direction ?? DBNull.Value);
        cmd.Parameters.AddWithValue("@MachineType", (object?)model.MachineType ?? DBNull.Value);
        cmd.Parameters.AddWithValue("@UserTypedTemperature", (object?)model.UserTypedTemperature ?? DBNull.Value);
        cmd.Parameters.AddWithValue("@UpDownFill", (object?)model.UpDownFill ?? DBNull.Value);
        cmd.Parameters.AddWithValue("@StartReversed", (object?)model.StartReversed ?? DBNull.Value);
        cmd.Parameters.AddWithValue("@CtrlType", (object?)model.CtrlType ?? DBNull.Value);
        cmd.Parameters.AddWithValue("@KMXFill", (object?)model.KMXFill ?? DBNull.Value);
        cmd.Parameters.AddWithValue("@IsEPost", (object?)model.IsEPost ?? DBNull.Value);
        cmd.Parameters.AddWithValue("@LastRecordBUIJournal", (object?)model.LastRecordBUIJournal ?? DBNull.Value);
        cmd.Parameters.AddWithValue("@TotalVLast", (object?)model.TotalVLast ?? DBNull.Value);
        cmd.Parameters.AddWithValue("@Post", model.Post);
        cmd.Parameters.AddWithValue("@OriginalPost", model.OriginalPost);
        await cmd.ExecuteNonQueryAsync();
    }

    public async Task DeletePostAsync(int postId)
    {
        await using var conn = await dataSource.OpenConnectionAsync();
        var sql = @"DELETE FROM ""DPosts"" WHERE ""Post"" = @Post;";
        await using var cmd = new NpgsqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("@Post", postId);
        await cmd.ExecuteNonQueryAsync();
    }
    //Справочник резервуаров
    public async Task<List<TankModel>> GetTanksAsync()
    {
        var list = new List<TankModel>();
        await using var conn = await dataSource.OpenConnectionAsync();

        var sql = @"SELECT ""Tank"", ""Point"", ""Product"", ""Name"", ""MaxH"", ""LevelSensAddres""
                FROM ""DTanks"" ORDER BY ""Tank"";";

        await using var cmd = new NpgsqlCommand(sql, conn);
        await using var reader = await cmd.ExecuteReaderAsync();

        while (await reader.ReadAsync())
        {
            list.Add(new TankModel
            {
                Tank = reader.GetInt16(0),
                Point = reader.GetInt16(1),
                Product = reader.IsDBNull(2) ? null : (short?)reader.GetInt16(2),
                Name = reader.IsDBNull(3) ? "" : reader.GetString(3),
                MaxH = reader.IsDBNull(4) ? null : (short?)reader.GetInt16(4),
                LevelSensAddres = reader.IsDBNull(5) ? null : reader.GetInt32(5),
                OriginalTank = reader.GetInt16(0)
            });
        }

        return list;
    }

    public async Task<int> InsertTankAsync(TankModel model)
    {
        await using var conn = await dataSource.OpenConnectionAsync();

        var columns = new List<string>();
        var values = new List<string>();
        var parameters = new List<NpgsqlParameter>();

        void AddParam(string column, object? value)
        {
            if (value != null)
            {
                string paramName = "@" + column;
                columns.Add($"\"{column}\"");
                values.Add(paramName);
                parameters.Add(new NpgsqlParameter(paramName, value));
            }
        }

        // Добавляем только те поля, которые реально используешь
        AddParam("Tank", model.Tank);
        AddParam("Point", model.Point);
        AddParam("Product", model.Product);
        AddParam("Name", model.Name);
        AddParam("MaxH", model.MaxH);
        AddParam("LevelSensAddres", model.LevelSensAddres);

        string sql = $@"
    INSERT INTO ""DTanks"" ({string.Join(", ", columns)})
    VALUES ({string.Join(", ", values)})
    RETURNING ""Tank"";";

        await using var cmd = new NpgsqlCommand(sql, conn);
        cmd.Parameters.AddRange(parameters.ToArray());

        var result = await cmd.ExecuteScalarAsync();

        return Convert.ToInt32(result); // PostgreSQL вернёт smallint
    }


    public async Task UpdateTankAsync(TankModel model)
    {
        await using var conn = await dataSource.OpenConnectionAsync();
        var sql = @"UPDATE ""DTanks"" SET
                  ""Point"" = @Point,
                  ""Product"" = @Product,
                  ""Name"" = @Name,
                  ""MaxH"" = @MaxH,
                  ""LevelSensAddres"" = @LevelSensAddres
                WHERE ""Tank"" = @OriginalTank;";

        await using var cmd = new NpgsqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("@Point", model.Point);
        cmd.Parameters.AddWithValue("@Product", (object?)model.Product ?? DBNull.Value);
        cmd.Parameters.AddWithValue("@Name", (object?)model.Name ?? DBNull.Value);
        cmd.Parameters.AddWithValue("@MaxH", (object?)model.MaxH ?? DBNull.Value);
        cmd.Parameters.AddWithValue("@LevelSensAddres", (object?)model.LevelSensAddres ?? DBNull.Value);
        cmd.Parameters.AddWithValue("@OriginalTank", model.OriginalTank);

        await cmd.ExecuteNonQueryAsync();
    }

    public async Task DeleteTankAsync(short tankId)
    {
        await using var conn = await dataSource.OpenConnectionAsync();
        var sql = @"DELETE FROM ""DTanks"" WHERE ""Tank"" = @Tank;";
        await using var cmd = new NpgsqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("@Tank", tankId);
        await cmd.ExecuteNonQueryAsync();
    }

    //Справочник трубопровод
    public async Task<List<TubeModel>> GetTubesAsync()
    {
        var result = new List<TubeModel>();
        await using var conn = await dataSource.OpenConnectionAsync();

        var sql = @"SELECT ""Tube"", ""Point1"", ""Point2""
                FROM ""DTubes""
                ORDER BY ""Tube"";";

        await using var cmd = new NpgsqlCommand(sql, conn);
        await using var reader = await cmd.ExecuteReaderAsync();

        while (await reader.ReadAsync())
        {
            result.Add(new TubeModel
            {
                Tube = reader.GetInt16(0),
                Point1 = reader.GetInt16(1),
                Point2 = reader.GetInt16(2)
            });
        }
        return result;
    }

    public async Task<int> InsertTubeAsync(TubeModel model)
    {
        await using var conn = await dataSource.OpenConnectionAsync();

        string sql = @"
        INSERT INTO ""DTubes"" (""Tube"", ""Point1"", ""Point2"")
        VALUES (@Tube, @Point1, @Point2)
        RETURNING ""Tube"";";

        await using var cmd = new NpgsqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("@Tube", model.Tube);
        cmd.Parameters.AddWithValue("@Point1", model.Point1);
        cmd.Parameters.AddWithValue("@Point2", model.Point2);

        var result = await cmd.ExecuteScalarAsync();
        return Convert.ToInt32(result);
    }

    public async Task UpdateTubeAsync(TubeModel model)
    {
        await using var conn = await dataSource.OpenConnectionAsync();

        string sql = @"
        UPDATE ""DTubes""
        SET ""Point1"" = @Point1,
            ""Point2"" = @Point2
        WHERE ""Tube"" = @Tube;";

        await using var cmd = new NpgsqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("@Point1", model.Point1);
        cmd.Parameters.AddWithValue("@Point2", model.Point2);
        cmd.Parameters.AddWithValue("@Tube", model.Tube);

        await cmd.ExecuteNonQueryAsync();
    }

    public async Task DeleteTubeAsync(int tubeId)
    {
        await using var conn = await dataSource.OpenConnectionAsync();
        var sql = @"DELETE FROM ""DTubes"" WHERE ""Tube"" = @Tube;";
        await using var cmd = new NpgsqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("@Tube", tubeId);
        await cmd.ExecuteNonQueryAsync();
    }

    public async Task<List<ARMReport>> GetAllReportsAsync()
    {
        var reports = new List<ARMReport>();
        try
        {
            await using var connection = await dataSource.OpenConnectionAsync();
            await using var command = new NpgsqlCommand(@" SELECT * FROM public.""Rep_REPORTS"" 
                                                               where ""UNIQUE_ID"" is not null 
                                                             ", connection);
            {
                await using var reader = await command.ExecuteReaderAsync();

                while (await reader.ReadAsync())
                {
                    var report = new ARMReport
                    {
                        ARMReportID = reader.GetInt32(reader.GetOrdinal("REPORT_ID")),
                        Name = reader.IsDBNull(reader.GetOrdinal("NAME")) ? null : reader.GetString(reader.GetOrdinal("NAME")),
                    };
                    reports.Add(report);
                }
            }

        }
        catch (Exception ex)
        {
            Log.Error(ex, "Error while fetching reports from the database.");
            throw;
        }

        return reports;
    }
}
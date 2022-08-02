using System.Data;
using System.Data.SqlClient;
using System.Dynamic;
using System.Reflection;

namespace RP.SOI.DotNet.Services;

public interface IDbService
{
    List<ModelClass> GetList<ModelClass>(string sql, params object?[] list);
    List<dynamic> GetList(string sql, params object?[] list);
    DataTable GetTable(string sql, params object?[] list);
    int ExecSQL(string sql, params object?[] list);
}

public class DBServiceException : Exception
{
    public DBServiceException(string? message) : base(message)
    {
    }
}

public class DbService : IDbService
{
    private static readonly string? env =
       Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");
    private static readonly IConfiguration config =
       new ConfigurationBuilder()
          .SetBasePath(Directory.GetCurrentDirectory())
          .AddJsonFile("appsettings.json")
          .AddJsonFile($"appsettings.{env}.json")
          .Build()
          .GetSection("ConnectionStrings");

    private static readonly string DB_CONNECTION = config.GetValue<String>("DefaultConnection") ?? "";
    private string DB_SQL = "";

    public List<dynamic> GetList(string sql, params object?[] list)
    {
        return GetTable(sql, list).ToDynamic();
    }

    public List<ModelClass> GetList<ModelClass>(string sql, params object?[] list)
    {
        return GetTable(sql, list).ToStatic<ModelClass>();
    }

    public DataTable GetTable(string sql, params object?[] list)
    {
        CheckConnectionString();
        for (int i = 0; i < list.Length; i++)
        {
            if (list[i] is string parameter)
            {
                list[i] = parameter?.EscQuote(); // prevent SQL Injection
            }
        }

        DB_SQL = String.Format(sql, list);
        DataTable datatable = new();
        using SqlConnection connection = new(DB_CONNECTION);    // ensures disposal
        using SqlDataAdapter adapter = new(DB_SQL, connection); // ensures disposal
        adapter.Fill(datatable);
        return datatable;
    }

    public int ExecSQL(string sql, params object?[] list)
    {
        CheckConnectionString();
        for (int i = 0; i < list.Length; i++)
        {
            if (list[i] is string parameter)
            {
                list[i] = parameter?.EscQuote(); // prevent SQL Injection
            }
        }

        DB_SQL = String.Format(sql, list);
        using SqlConnection connection = new(DB_CONNECTION);   // ensures disposal
        using SqlCommand command = connection.CreateCommand(); // ensures disposal
        connection.Open();
        command.CommandText = DB_SQL;
        int rowsAffected = command.ExecuteNonQuery();
        return rowsAffected;
    }

    private static void CheckConnectionString()
    {
        if (DB_CONNECTION == "")
        {
            throw new DBServiceException("Missing Database Connection String");
        }
    }
}

public static class DBServiceHelper
{
    public static List<DTO> ToStatic<DTO>(this DataTable dt)
    {
        DTO obj = (DTO)Activator.CreateInstance(typeof(DTO))!;
        foreach (DataColumn column in dt.Columns)
        {
            PropertyInfo? objProperty = obj.GetType().GetProperty(column.ColumnName, BindingFlags.Public | BindingFlags.Instance);
            if (objProperty == null)
            {
                throw new DBServiceException($"No corresponding property in Model for DB Column {column.ColumnName}");
            }
            var baseModelType = Nullable.GetUnderlyingType(objProperty.PropertyType);
            baseModelType ??= objProperty.PropertyType;
            if (column.DataType != baseModelType)
            {
                var conflictingTypes = $"({column.DataType}, {baseModelType})";
                throw new DBServiceException($"Incompatible column types {conflictingTypes} for {column.ColumnName}");
            }
        }

        var list = new List<DTO>();
        foreach (DataRow row in dt.Rows)
        {
            obj = (DTO)Activator.CreateInstance(typeof(DTO))!;
            foreach (DataColumn column in dt.Columns)
            {
                PropertyInfo? objProperty = obj.GetType().GetProperty(column.ColumnName, BindingFlags.Public | BindingFlags.Instance);
                if (row[column] == DBNull.Value)
                {
                    objProperty!.SetValue(obj, null);
                }
                else
                {
                    objProperty!.SetValue(obj, row[column]);
                }
            }
            list.Add(obj);
        }
        return list;
    }

    public static List<dynamic> ToDynamic(this DataTable dt)
    {
        var dynList = new List<dynamic>();
        foreach (DataRow row in dt.Rows)
        {
            dynamic dynObj = new ExpandoObject();
            var propDict = (IDictionary<string, object>)dynObj;
            foreach (DataColumn column in dt.Columns)
            {
                propDict[column.ColumnName] = row[column];
            }
            dynList.Add(dynObj);
        }
        return dynList;
    }

    public static ExpandoObject ToExpando(this object objAnonymous)
    {
        IDictionary<string, object> dictAnonymous = new RouteValueDictionary(objAnonymous)!;
        IDictionary<string, object> objExpando = new ExpandoObject()!;
        foreach (var item in dictAnonymous)
            objExpando.Add(item);
        return (ExpandoObject)objExpando!;
    }

    public static List<dynamic> ToExpandoList<T>(this List<T> l)
    {
        return (from o in l select ToExpando(o)).ToList<dynamic>().ToList();
    }

    public static List<dynamic> ToExpandoList(this IQueryable<dynamic> query)
    {
        return query.ToList().ToExpandoList();
    }
    

    public static string EscQuote(this string line) => line.Replace("'", "''");

}


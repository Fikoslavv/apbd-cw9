using System.Collections;
using System.Text;
using Microsoft.Data.SqlClient;

namespace apbd_cw9;

public abstract class ServiceBase<T> : IService<T>
{
    protected readonly string connectionString;

    public ServiceBase(string connectionString)
    {
        this.connectionString = connectionString;
    }

    protected string GetSqlWhere(IEnumerable<KeyValuePair<string, string[]>> fields, out Action<SqlCommand> cmdParamsFiller)
    {
        var paramsToFill = new LinkedList<KeyValuePair<string, string>>();
        cmdParamsFiller = command => { foreach (var pair in paramsToFill) command.Parameters.AddWithValue(pair.Key, pair.Value); };

        if (!fields.Any())
        {
            return string.Empty;
        }

        string getExpression(KeyValuePair<string, string[]> field)
        {
            if (field.Value.Length == 1)
            {
                paramsToFill.AddLast(new KeyValuePair<string, string>('@' + field.Key, field.Value.Single()));
                return $"{field.Key} = @{field.Key}";
            }
            else
            {
                var builder = new StringBuilder(field.Key).Append(" in (");
                foreach (var value in field.Value)
                {
                    var param = $"@{field.Key}_{Math.Abs(value.GetHashCode())}";
                    builder.Append(' ').Append(param).Append(',');
                    paramsToFill.AddLast(new KeyValuePair<string, string>(param, value));
                }
                builder[^1] = ')';
                return builder.ToString();
            }
        }

        var enumerator = fields.GetEnumerator();
        enumerator.MoveNext();
        var builder = new StringBuilder(" where ").Append(getExpression(enumerator.Current));

        while (enumerator.MoveNext()) builder.Append(" and ").Append(getExpression(enumerator.Current));

        return builder.ToString();
    }

    public abstract IEnumerable<T> GetData(IEnumerable<KeyValuePair<string, string[]>> fields);

    public SqlConnection GetNewConnection() => new(this.connectionString);

    public abstract bool InsertData(T value);
    public abstract bool InsertData(T value, SqlConnection connection, SqlTransaction? transaction = null);
    public abstract bool UpdateData(T value);
    public abstract bool UpdateData(T value, SqlConnection connection, SqlTransaction? transaction = null);
    public abstract bool DeleteData(T value);
    public abstract bool DeleteData(T value, SqlConnection connection, SqlTransaction? transaction = null);

    public Tout? RunStoredProcedure<Tout>(string procedureName, Func<SqlCommand, Tout> executor, params KeyValuePair<string, object>[] parameters)
    {
        using (var connection = this.GetNewConnection())
        using (SqlCommand command = new("addProductToWarehouse", connection))
        {
            command.CommandType = System.Data.CommandType.StoredProcedure;

            foreach (var pair in parameters) command.Parameters.AddWithValue(pair.Key, pair.Value);

            connection.Open();
            try { return executor(command); } catch { return default; }
        }
        
    }

    public IEnumerator<T> GetEnumerator() => this.GetData([]).GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}

namespace apbd_cw9;

public interface IService<T> : IEnumerable<T>
{
    public abstract IEnumerable<T> GetData(IEnumerable<KeyValuePair<string, string[]>> fields);
    public abstract Microsoft.Data.SqlClient.SqlConnection GetNewConnection();
    public abstract bool InsertData(T value);
    public abstract bool InsertData(T value, Microsoft.Data.SqlClient.SqlConnection connection, Microsoft.Data.SqlClient.SqlTransaction? transaction = null);
    public abstract bool UpdateData(T value);
    public abstract bool UpdateData(T value, Microsoft.Data.SqlClient.SqlConnection connection, Microsoft.Data.SqlClient.SqlTransaction? transaction = null);
    public abstract bool DeleteData(T value);
    public abstract bool DeleteData(T value, Microsoft.Data.SqlClient.SqlConnection connection, Microsoft.Data.SqlClient.SqlTransaction? transaction = null);
    public abstract Tout? RunStoredProcedure<Tout>(string procedureName, Func<Microsoft.Data.SqlClient.SqlCommand, Tout> executor, params KeyValuePair<string, object>[] parameters);
}

using Microsoft.Data.SqlClient;

namespace apbd_cw9;

public class WarehouseService : ServiceBase<Warehouse>
{
    public WarehouseService(string connectionString) : base(connectionString) { }

    public override IEnumerable<Warehouse> GetData(IEnumerable<KeyValuePair<string, string[]>> fields)
    {
        using (SqlConnection connection = new(this.connectionString))
        using (SqlCommand command = new("select idWarehouse, name, address from warehouse" + this.GetSqlWhere(fields, out var cmdParamsFiller), connection))
        {
            cmdParamsFiller(command);
            connection.Open();

            SqlDataReader reader;
            try { reader = command.ExecuteReader(); } catch { yield break; }

            using (reader)
            {
                var idOridinal = reader.GetOrdinal("idWarehouse");
                var nameOridinal = reader.GetOrdinal("name");
                var addressOridinal = reader.GetOrdinal("address");
    
                while (reader.Read())
                {
                    yield return new()
                    {
                        IdWarehouse = reader.GetInt32(idOridinal),
                        Name = reader.GetString(nameOridinal),
                        Address = reader.GetString(addressOridinal),
                    };
                }
            }
        }
    }

    public override bool InsertData(Warehouse value)
    {
        using SqlConnection connection = new(this.connectionString);
        connection.Open();

        return this.InsertData(value, connection);
    }

    public override bool InsertData(Warehouse value, SqlConnection connection, SqlTransaction? transaction = null)
    {
        var commandStr = value.IdWarehouse < 0 ? "insert into warehouse (name, address) values (@name, @address); select scope_identity()" : "insert into warehouse (idWarehouse, name, address) values (@idWarehouse, @name, @address); select scope_identity()";
        using (SqlCommand command = new(commandStr, connection))
        {
            if (transaction is not null) command.Transaction = transaction;
            if (value.IdWarehouse >= 0) command.Parameters.AddWithValue("@idWarehouse", value.IdWarehouse);
            command.Parameters.AddWithValue("@name", value.Name);
            command.Parameters.AddWithValue("@address", value.Address);

            try
            {
                value.IdWarehouse = Convert.ToInt32(command.ExecuteScalar());
                return true;
            }
            catch { return false; }
        }
    }

    public override bool UpdateData(Warehouse value)
    {
        using SqlConnection connection = new(this.connectionString);
        connection.Open();

        return this.UpdateData(value, connection);
    }

    public override bool UpdateData(Warehouse value, SqlConnection connection, SqlTransaction? transaction = null)
    {
        using (SqlCommand command = new("update warehouse set name = @name, address = @address where idWarehouse = @idWarehouse", connection))
        {
            if (transaction is not null) command.Transaction = transaction;
            command.Parameters.AddWithValue("@idWarehouse", value.IdWarehouse);
            command.Parameters.AddWithValue("@name", value.Name);
            command.Parameters.AddWithValue("@address", value.Address);

            return command.ExecuteNonQuery() > 0;
        }
    }

    public override bool DeleteData(Warehouse value)
    {
        using SqlConnection connection = new(this.connectionString);
        connection.Open();

        return this.DeleteData(value, connection);
    }

    public override bool DeleteData(Warehouse value, SqlConnection connection, SqlTransaction? transaction = null)
    {
        using (SqlCommand command = new("delete from warehouse where idWarehouse = @idWarehouse", connection))
        {
            if (transaction is not null) command.Transaction = transaction;
            command.Parameters.AddWithValue("@idWarehouse", value.IdWarehouse);

            return command.ExecuteNonQuery() > 0;
        }
    }
}

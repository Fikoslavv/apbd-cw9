using Microsoft.Data.SqlClient;

namespace apbd_cw9;

public class OrderService : ServiceBase<Order>
{
    public OrderService(string connectionString) : base(connectionString) { }

    public override IEnumerable<Order> GetData(IEnumerable<KeyValuePair<string, string[]>> fields)
    {
        using (SqlConnection connection = new(this.connectionString))
        using (SqlCommand command = new("select idOrder, idProduct, amount, createdAt, fulfilledAt from \"order\"" + this.GetSqlWhere(fields, out var cmdParamsFiller), connection))
        {
            cmdParamsFiller(command);
            connection.Open();

            using (var reader = command.ExecuteReader())
            {
                var idOridinal = reader.GetOrdinal("idOrder");
                var idProductOridinal = reader.GetOrdinal("idProduct");
                var amountOridinal = reader.GetOrdinal("amount");
                var createdAtOridinal = reader.GetOrdinal("createdAt");
                var fulfilledAtOridinal = reader.GetOrdinal("fulfilledAt");

                while (reader.Read())
                {
                    yield return new()
                    {
                        IdOrder = reader.GetInt32(idOridinal),
                        IdProduct = reader.GetInt32(idProductOridinal),
                        Amount = reader.GetInt32(amountOridinal),
                        CreatedAt = reader.GetDateTime(createdAtOridinal),
                        FulfilledAt = reader.IsDBNull(fulfilledAtOridinal) ? null : reader.GetDateTime(fulfilledAtOridinal),
                    };
                }
            }
        }
    }

    public override bool InsertData(Order value)
    {
        using SqlConnection connection = new(this.connectionString);
        connection.Open();

        return this.InsertData(value, connection);
    }

    public override bool InsertData(Order value, SqlConnection connection, SqlTransaction? transaction = null)
    {
        var commandStr = value.IdOrder < 0 ? "insert into \"order\" (idProduct, amount, createdAt, fulfilledAt) values (@idProduct, @amount, sysdatetime(), @fulfilledAt)" : "insert into \"order\" (idOrder, idProduct, amount, createdAt, fulfilledAt) values (@idOrder, @idProduct, @amount, sysdatetime(), @fulfilledAt)";
        commandStr += "; select idOrder, createdAt from \"order\" where idOrder = scope_identity()";
        using (SqlCommand command = new(commandStr, connection))
        {
            if (transaction is not null) command.Transaction = transaction;
            if (value.IdOrder >= 0) command.Parameters.AddWithValue("@idOrder", value.IdOrder);
            command.Parameters.AddWithValue("@idProduct", value.IdProduct);
            command.Parameters.AddWithValue("@amount", value.Amount);
            command.Parameters.AddWithValue("@fulfilledAt", value.FulfilledAt is not null ? value.FulfilledAt : DBNull.Value);

            try
            {
                using var reader = command.ExecuteReader();
                reader.Read();

                value.IdOrder = reader.GetInt32(reader.GetOrdinal("idOrder"));
                value.CreatedAt = reader.GetDateTime(reader.GetOrdinal("createdAt"));

                return true;
            }
            catch { return false; }
        }
    }

    public override bool UpdateData(Order value)
    {
        using SqlConnection connection = new(this.connectionString);
        connection.Open();

        return this.UpdateData(value, connection);
    }

    public override bool UpdateData(Order value, SqlConnection connection, SqlTransaction? transaction = null)
    {
        using (SqlCommand command = new("update \"order\" set idProduct = @idProduct, amount = @amount, createdAt = @createdAt, fulfilledAt = @fulfilledAt where idOrder = @idOrder", connection))
        {
            if (transaction is not null) command.Transaction = transaction;
            command.Parameters.AddWithValue("@idOrder", value.IdOrder);
            command.Parameters.AddWithValue("@idProduct", value.IdProduct);
            command.Parameters.AddWithValue("@amount", value.Amount);
            command.Parameters.AddWithValue("@createdAt", value.CreatedAt);
            command.Parameters.AddWithValue("@fulfilledAt", value.FulfilledAt is not null ? value.FulfilledAt : DBNull.Value);

            return command.ExecuteNonQuery() > 0;
        }
    }

    public override bool DeleteData(Order value)
    {
        using SqlConnection connection = new(this.connectionString);
        connection.Open();

        return this.DeleteData(value, connection);
    }

    public override bool DeleteData(Order value, SqlConnection connection, SqlTransaction? transaction = null)
    {
        using (SqlCommand command = new("delete from \"order\" where idOrder = @idOrder", connection))
        {
            if (transaction is not null) command.Transaction = transaction;
            command.Parameters.AddWithValue("@idOrder", value.IdOrder);

            return command.ExecuteNonQuery() > 0;
        }
    }
}

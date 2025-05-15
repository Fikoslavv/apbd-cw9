using Microsoft.Data.SqlClient;

namespace apbd_cw9;

public class ProductWarehouseService : ServiceBase<ProductWarehouse>
{
    public ProductWarehouseService(string connectionString) : base(connectionString) { }

    public override IEnumerable<ProductWarehouse> GetData(IEnumerable<KeyValuePair<string, string[]>> fields)
    {
        using (SqlConnection connection = new(this.connectionString))
        using (SqlCommand command = new("select idProductWarehouse, idWarehouse, idProduct, idOrder, amount, price, createdAt from product_warehouse" + this.GetSqlWhere(fields, out var cmdParamsFiller), connection))
        {
            cmdParamsFiller(command);
            connection.Open();

            using (var reader = command.ExecuteReader())
            {
                var idOridinal = reader.GetOrdinal("idProductWarehouse");
                var idWarehouseOridinal = reader.GetOrdinal("idWarehouse");
                var idProductOridinal = reader.GetOrdinal("idProduct");
                var idOrderOridinal = reader.GetOrdinal("idOrder");
                var amountOridinal = reader.GetOrdinal("amount");
                var priceOridinal = reader.GetOrdinal("price");
                var createdAtOridinal = reader.GetOrdinal("createdAt");

                while (reader.Read())
                {
                    yield return new()
                    {
                        IdProductWarehouse = reader.GetInt32(idOridinal),
                        IdWarehouse = reader.GetInt32(idWarehouseOridinal),
                        IdProduct = reader.GetInt32(idProductOridinal),
                        IdOrder = reader.GetInt32(idOrderOridinal),
                        Amount = reader.GetInt32(amountOridinal),
                        Price = reader.GetDecimal(priceOridinal),
                        CreatedAt = reader.GetDateTime(createdAtOridinal),
                    };
                }
            }
        }
    }

    public override bool InsertData(ProductWarehouse value)
    {
        using SqlConnection connection = new(this.connectionString);
        connection.Open();

        return this.InsertData(value, connection);
    }

    public override bool InsertData(ProductWarehouse value, SqlConnection connection, SqlTransaction? transaction = null)
    {
        var commandStr = value.IdProductWarehouse < 0 ? "insert into product_warehouse (idWarehouse, idProduct, idOrder, amount, price, createdAt) values (@idWarehouse, @idProduct, @idOrder, @amount, @price, getdate())" : "insert into product_warehouse (idProductWarehouse, idWarehouse, idProduct, idOrder, amount, price, createdAt) values (@idProductWarehouse, @idWarehouse, @idProduct, @idOrder, @amount, @price, getdate())";
        commandStr += "; select idProductWarehouse, createdAt from product_warehouse where idProductWarehouse = scope_identity()";
        using (SqlCommand command = new(commandStr, connection))
        {
            if (transaction is not null) command.Transaction = transaction;
            if (value.IdProductWarehouse >= 0) command.Parameters.AddWithValue("@idProductWarehouse", value.IdProductWarehouse);
            command.Parameters.AddWithValue("@idWarehouse", value.IdWarehouse);
            command.Parameters.AddWithValue("@idProduct", value.IdProduct);
            command.Parameters.AddWithValue("@idOrder", value.IdOrder);
            command.Parameters.AddWithValue("@amount", value.Amount);
            command.Parameters.AddWithValue("@price", value.Price);

            try
            {
                using var reader = command.ExecuteReader();
                reader.Read();

                value.IdProductWarehouse = reader.GetInt32(reader.GetOrdinal("idProductWarehouse"));
                value.CreatedAt = reader.GetDateTime(reader.GetOrdinal("createdAt"));

                return true;
            }
            catch { return false; }
        }
    }

    public override bool UpdateData(ProductWarehouse value)
    {
        using SqlConnection connection = new(this.connectionString);
        connection.Open();

        return this.UpdateData(value, connection);
    }

    public override bool UpdateData(ProductWarehouse value, SqlConnection connection, SqlTransaction? transaction = null)
    {
        using (SqlCommand command = new("update product_warehouse set idWarehouse = @idWarehouse, idProduct = @idProduct, idOrder = @idOrder, amount = @amount, price = @price, createdAt = @createdAt where idProductWarehouse = @idProductWarehouse", connection))
        {
            if (transaction is not null) command.Transaction = transaction;
            command.Parameters.AddWithValue("@idProductWarehouse", value.IdProductWarehouse);
            command.Parameters.AddWithValue("@idWarehouse", value.IdWarehouse);
            command.Parameters.AddWithValue("@idProduct", value.IdProduct);
            command.Parameters.AddWithValue("@idOrder", value.IdOrder);
            command.Parameters.AddWithValue("@amount", value.Amount);
            command.Parameters.AddWithValue("@price", value.Price);
            command.Parameters.AddWithValue("@createdAt", value.CreatedAt);

            return command.ExecuteNonQuery() > 0;
        }
    }

    public override bool DeleteData(ProductWarehouse value)
    {
        using SqlConnection connection = new(this.connectionString);
        connection.Open();

        return this.DeleteData(value, connection);
    }

    public override bool DeleteData(ProductWarehouse value, SqlConnection connection, SqlTransaction? transaction = null)
    {
        using (SqlCommand command = new("delete from product_warehouse where idProductWarehouse = @idProductWarehouse", connection))
        {
            if (transaction is not null) command.Transaction = transaction;
            command.Parameters.AddWithValue("@idProductWarehouse", value.IdProductWarehouse);

            return command.ExecuteNonQuery() > 0;
        }
    }
}

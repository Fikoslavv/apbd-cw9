using Microsoft.Data.SqlClient;

namespace apbd_cw9;

public class ProductService : ServiceBase<Product>
{
    public ProductService(string connectionString) : base(connectionString) { }

    public override IEnumerable<Product> GetData(IEnumerable<KeyValuePair<string, string[]>> fields)
    {
        using (SqlConnection connection = new(this.connectionString))
        using (SqlCommand command = new("select idProduct, name, description, price from product" + this.GetSqlWhere(fields, out var cmdParamsFiller), connection))
        {
            cmdParamsFiller(command);
            connection.Open();

            using (var reader = command.ExecuteReader())
            {
                var idProductOridinal = reader.GetOrdinal("idProduct");
                var nameOridinal = reader.GetOrdinal("name");
                var descriptionOridinal = reader.GetOrdinal("description");
                var priceOridinal = reader.GetOrdinal("price");

                while (reader.Read())
                {
                    yield return new()
                    {
                        IdProduct = reader.GetInt32(idProductOridinal),
                        Name = reader.GetString(nameOridinal),
                        Description = reader.GetString(descriptionOridinal),
                        Price = reader.GetDecimal(priceOridinal),
                    };
                }
            }
        }
    }

    public override bool InsertData(Product value)
    {
        using SqlConnection connection = new(this.connectionString);
        connection.Open();

        return this.InsertData(value, connection);
    }

    public override bool InsertData(Product value, SqlConnection connection, SqlTransaction? transaction = null)
    {
        var commandStr = value.IdProduct < 0 ? "insert into product (name, description, price) values (@name, @description, @price); select scope_identity()" : "insert into product (idProduct, name, description, price) values (@idProduct, @name, @description, @price); select scope_identity()";
        using (SqlCommand command = new(commandStr, connection))
        {
            if (transaction is not null) command.Transaction = transaction;
            if (value.IdProduct >= 0) command.Parameters.AddWithValue("@idProduct", value.IdProduct);
            command.Parameters.AddWithValue("@name", value.Name);
            command.Parameters.AddWithValue("@description", value.Description);
            command.Parameters.AddWithValue("@price", value.Price);

            try
            {
                value.IdProduct = Convert.ToInt32(command.ExecuteScalar());
                return true;
            }
            catch { return false; }
        }
    }

    public override bool UpdateData(Product value)
    {
        using SqlConnection connection = new(this.connectionString);
        connection.Open();

        return this.UpdateData(value, connection);
    }

    public override bool UpdateData(Product value, SqlConnection connection, SqlTransaction? transaction = null)
    {
        using (SqlCommand command = new("update product set name = @name, description = @description, price = @price where idProduct = @idProduct", connection))
        {
            if (transaction is not null) command.Transaction = transaction;
            command.Parameters.AddWithValue("@idProduct", value.IdProduct);
            command.Parameters.AddWithValue("@name", value.Name);
            command.Parameters.AddWithValue("@description", value.Description);
            command.Parameters.AddWithValue("@price", value.Price);

            return command.ExecuteNonQuery() > 0;
        }
    }

    public override bool DeleteData(Product value)
    {
        using SqlConnection connection = new(this.connectionString);
        connection.Open();

        return this.DeleteData(value, connection);
    }

    public override bool DeleteData(Product value, SqlConnection connection, SqlTransaction? transaction = null)
    {
        using (SqlCommand command = new("delete from product where idProduct = @idProduct", connection))
        {
            if (transaction is not null) command.Transaction = transaction;
            command.Parameters.AddWithValue("@idProduct", value.IdProduct);

            return command.ExecuteNonQuery() > 0;
        }
    }
}

using System;
using System.Collections.Generic;
using System.Reflection;
using Microsoft.VisualBasic.CompilerServices;
using Npgsql; 

namespace MiniORM
{
    public class ORMContext
    {
        private readonly string _connectionString;

        public ORMContext(string connectionString)
        {
            _connectionString = connectionString;
        }

        /// <summary>
        /// Получает все записи из таблицы
        /// </summary>
        public List<T> ReadAll<T>(string tableName) where T : new()
        {
            var list = new List<T>();

            using (var connection = new NpgsqlConnection(_connectionString))
            {
                connection.Open();
                var command = new NpgsqlCommand($"SELECT * FROM \"{tableName}\"", connection);

                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        list.Add(Map<T>(reader));
                    }
                }
            }
            return list;
        }

        /// <summary>
        /// Получает одну запись по ID
        /// </summary>
        public T ReadById<T>(string tableName, int id) where T : new()
        {
            using (var connection = new NpgsqlConnection(_connectionString))
            {
                connection.Open();
                var command = new NpgsqlCommand($"SELECT * FROM \"{tableName}\" WHERE \"Id\" = @id", connection);
                command.Parameters.AddWithValue("@id", id);

                using (var reader = command.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        return Map<T>(reader);
                    }
                }
            }
            return default(T);
        }
        
        public string GetValuById(string tableName, string column, int id)
        {
            using (var connection = new NpgsqlConnection(_connectionString))
            {
                connection.Open();
                var command = new NpgsqlCommand($"SELECT \"{column}\" FROM \"{tableName}\" WHERE \"Id\" = @id", connection);
                command.Parameters.AddWithValue("@id", id);

                using (var reader = command.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        return reader[column].ToString();
                    }
                }
            }
            return string.Empty;
        }

        /// <summary>
        /// Превращает строку из Базы Данных в объект C# (Маппинг)
        /// </summary>
        private T Map<T>(NpgsqlDataReader reader) where T : new()
        {
            var entity = new T();
            var properties = typeof(T).GetProperties();

            foreach (var prop in properties)
            {
                if (!ColumnExists(reader, prop.Name)) continue;

                var value = reader[prop.Name];
                
                if (value != DBNull.Value)
                {
                    try
                    {
                        var convertedValue = Convert.ChangeType(value, prop.PropertyType);
                        prop.SetValue(entity, convertedValue);
                    }
                    catch
                    {
                        Console.WriteLine($"Ошибка маппинга поля {prop.Name}");
                    }
                }
            }
            return entity;
        }

        private bool ColumnExists(NpgsqlDataReader reader, string columnName)
        {
            for (int i = 0; i < reader.FieldCount; i++)
            {
                if (reader.GetName(i).Equals(columnName, StringComparison.OrdinalIgnoreCase)) 
                    return true;
            }
            return false;
        }
        
        /// <summary>
        /// Выбирает записи, где колонка columnName равна value.
        /// Пример: ReadWhere<Review>("Reviews", "TourId", 1);
        /// </summary>
        public List<T> ReadWhere<T>(string tableName, string columnName, object value) where T : new()
        {
            var list = new List<T>();
            using (var connection = new NpgsqlConnection(_connectionString))
            {
                connection.Open();
                var sql = $"SELECT * FROM \"{tableName}\" WHERE \"{columnName}\" = @val";
        
                using (var command = new NpgsqlCommand(sql, connection))
                {
                    command.Parameters.AddWithValue("@val", value);
                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            list.Add(Map<T>(reader));
                        }
                    }
                }
            }
            return list;
        }
        
        public void ExecuteSql(string sql)
        {
            using (var connection = new NpgsqlConnection(_connectionString))
            {
                connection.Open();
                using (var command = new NpgsqlCommand(sql, connection))
                {
                    command.ExecuteNonQuery();
                }
            }
        }

        public void Insert<T>(string tableName, T entity) where T : new()
        {
            var props = typeof(T).GetProperties().Where(p => p.Name != "Id" && p.Name != "Error").ToArray();
            var columns = string.Join(",", props.Select(p => $"\"{p.Name}\""));
            var values = string.Join(",", props.Select(p => $"@{p.Name}"));
            
            string sql = $"INSERT INTO \"{tableName}\" ({columns}) VALUES ({values})";

            using (var conn = new NpgsqlConnection(_connectionString))
            {
                conn.Open();
                var command = new NpgsqlCommand(sql, conn);
                foreach (var prop in props)
                {
                    command.Parameters.AddWithValue($"@{prop.Name}", prop.GetValue(entity) ?? DBNull.Value);
                }
                command.ExecuteNonQuery();
            }
        }

        public void Update<T>(string tableName, T entity, int id) where T : new()
        {
            var props = typeof(T).GetProperties().Where(p => p.Name != "Id" && p.Name != "Error").ToArray();
            var columns = string.Join(",", props.Select(p => $"\"{p.Name}\""));
            var values = string.Join(",", props.Select(p => $"@{p.Name}"));
            
            string insertNew = $"INSERT INTO \"{tableName}\" ({columns}) VALUES ({values})";
            
            using (var conn = new NpgsqlConnection(_connectionString))
            {
                conn.Open();
                
                string deleteOld = $@"Delete from ""tours"" where ""Id""={id}";
                var command = new NpgsqlCommand(deleteOld, conn);
                command.ExecuteNonQuery();
                
                command = new NpgsqlCommand(insertNew, conn);
                foreach (var prop in props)
                {
                    command.Parameters.AddWithValue($"@{prop.Name}", prop.GetValue(entity) ?? DBNull.Value);
                }
                command.ExecuteNonQuery();
            }
        }
        
        public void Delete(string tableName, int id)
        {
            
            using (var conn = new NpgsqlConnection(_connectionString))
            {
                conn.Open();
                
                string deleteOld = $@"Delete from ""{tableName}"" where ""Id""={id}";
                var command = new NpgsqlCommand(deleteOld, conn);
                command.ExecuteNonQuery();
            }
        }
    }
}
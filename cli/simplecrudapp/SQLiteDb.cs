using simplecrudapp.Data;
using SQLite;

namespace simplecrudapp
{
    public class SQLiteDb
    {
        private static string _folder = Environment.CurrentDirectory;
        private static string _path = Path.Combine(_folder, "simple-crud.db");

        private readonly object _lock = new object();

        /// <summary>
        /// Create all SQLite tables on the program, should be invoked on the initialization.
        /// </summary>
        public void CreateAllTable()
        {
            lock (_lock)
            {
                using var connection = OpenConnection();

                connection.CreateTable<EmployeeData>();
                connection.Close();
            }
        }

        /// <summary>
        /// Drop all SQLite tables on the program, should be invoked on the initialization.
        /// </summary>
        public void DropAllTable()
        {
            lock (_lock)
            {
                using var connection = OpenConnection();

                connection.DropTable<EmployeeData>();
                connection.Close();
            }
        }

        public void Insert<TData>(TData row)
        {
            lock (_lock)
            {
                using var connection = OpenConnection();

                connection.Insert(row);
                connection.Close();
            }
        }

        public void Update<TData>(TData row)
        {
            lock (_lock)
            {
                using var connection = OpenConnection();

                connection.Update(row);
                connection.Close();
            }
        }

        public void Delete<TData>(TData row)
        {
            lock (_lock)
            {
                using var connection = OpenConnection();

                connection.Delete(row);
                connection.Close();
            }
        }

        public TData? GetRow<TData>(Func<TData, bool> predicate) where TData : new()
        {
            lock (_lock)
            {
                using var connection = OpenConnection();

                try
                {
                    var rows = connection.Table<TData>().ToArray();
                    foreach (var row in rows)
                    {
                        if (predicate(row))
                        {
                            return row;
                        }    
                    }

                    return default;
                }
                catch (Exception ex)
                {
                    throw;
                }
                finally
                {
                    connection.Close();
                }
            }
        }

        public IEnumerable<TData> GetRows<TData>() where TData : new()
        {
            lock (_lock)
            {
                using var connection = OpenConnection();

                try
                {
                    return connection.Table<TData>().ToArray();
                }
                catch (Exception ex)
                {
                    throw;
                }
                finally
                {
                    connection.Close();
                }
            }
        }

        /// <summary>
        /// Open sql connection with 'using' keyword to dispose after used.
        /// </summary>
        /// <returns>
        /// <see cref="SQLiteConnection"/> object.
        /// </returns>
        private SQLiteConnection OpenConnection()
        {
            return new SQLiteConnection(_path);
        }
    }
}

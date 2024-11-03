
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;


namespace CallaghanDev.Common.MSSQL
{
    public class ForeignKeyInfo
    {
        public string ForeignKeyName { get; set; }
        public string ParentTable { get; set; }
        public string ParentColumn { get; set; }
        public string ReferencedTable { get; set; }
        public string ReferencedColumn { get; set; }

        public override string ToString()
        {
            return $"ForeignKeyName={ForeignKeyName}, ParentTable={ParentTable}, ParentColumn={ParentColumn}, ReferencedTable={ReferencedTable}, ReferencedColumn={ReferencedColumn}";
        }
    }

    public class DatabaseHelper
    {
        /// <summary>
        /// Retrieves foreign key dependencies from the database.
        /// </summary>
        /// <param name="connectionString">The connection string to the SQL Server database.</param>
        /// <returns>A list of ForeignKeyInfo objects representing the foreign key dependencies.</returns>
        public static List<ForeignKeyInfo> GetForeignKeyDependencies(string connectionString)
        {
            var dependencies = new List<ForeignKeyInfo>();

            string query = @"
            SELECT 
                fk.name AS ForeignKeyName,
                tp.name AS ParentTable,
                cp.name AS ParentColumn,
                tr.name AS ReferencedTable,
                cr.name AS ReferencedColumn
            FROM 
                sys.foreign_keys fk
            INNER JOIN 
                sys.foreign_key_columns fkc ON fk.object_id = fkc.constraint_object_id
            INNER JOIN 
                sys.tables tp ON fk.parent_object_id = tp.object_id
            INNER JOIN 
                sys.columns cp ON fkc.parent_column_id = cp.column_id AND fkc.parent_object_id = cp.object_id
            INNER JOIN 
                sys.tables tr ON fk.referenced_object_id = tr.object_id
            INNER JOIN 
                sys.columns cr ON fkc.referenced_column_id = cr.column_id AND fkc.referenced_object_id = cr.object_id
            ORDER BY 
                tp.name, fk.name;
        ";

            using (var connection = new SqlConnection(connectionString))
            using (var command = new SqlCommand(query, connection))
            {
                connection.Open();
                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        var foreignKeyInfo = new ForeignKeyInfo
                        {
                            ForeignKeyName = reader["ForeignKeyName"].ToString(),
                            ParentTable = reader["ParentTable"].ToString(),
                            ParentColumn = reader["ParentColumn"].ToString(),
                            ReferencedTable = reader["ReferencedTable"].ToString(),
                            ReferencedColumn = reader["ReferencedColumn"].ToString()
                        };

                        dependencies.Add(foreignKeyInfo);
                    }
                }
            }

            return dependencies;
        }



    }

}


using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CallaghanDev.Utilities.Code
{
    public class MSToMySQL
    {
        public string GenScript(string connectionString)
        {
            StringBuilder script = new StringBuilder();

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();

                DataTable tables = connection.GetSchema("Tables");

                foreach (DataRow table in tables.Rows)
                {
                    string tableName = (string)table["TABLE_NAME"];
                    script.AppendLine($"CREATE TABLE {tableName}(");

                    DataTable columns = connection.GetSchema("Columns", new string[] { null, null, tableName });

                    foreach (DataRow column in columns.Rows)
                    {
                        string columnName = (string)column["COLUMN_NAME"];
                        string dataType = (string)column["DATA_TYPE"];
                        script.Append($"    {columnName} {dataType}");

                        if (column != columns.Rows[columns.Rows.Count - 1])
                        {
                            script.Append(",");
                        }

                        script.AppendLine();
                    }

                    // Add foreign key constraints
                    DataTable foreignKeys = connection.GetSchema("ForeignKeys", new string[] { null, null, tableName });
                    foreach (DataRow row in foreignKeys.Rows)
                    {
                        string constraintName = (string)row["CONSTRAINT_NAME"];
                        string referencedColumnName = (string)row["REFERENCED_COLUMN_NAME"];
                        string referencedTableName = (string)row["REFERENCED_TABLE_NAME"];
                        string fkColumnName = (string)row["COLUMN_NAME"];

                        script.AppendLine($"    ,CONSTRAINT {constraintName} FOREIGN KEY ({fkColumnName}) REFERENCES {referencedTableName}({referencedColumnName})");
                    }

                    script.AppendLine(");");
                    script.AppendLine();
                }
            }
            return script.ToString();
        }
    }
}

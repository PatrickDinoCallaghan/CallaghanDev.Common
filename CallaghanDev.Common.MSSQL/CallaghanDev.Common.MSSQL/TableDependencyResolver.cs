using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CallaghanDev.Common.MSSQL
{
    internal class TableDependencyResolver
    {  /// <summary>
       /// Determines the order in which tables should be deleted to avoid foreign key constraint errors,
       /// ignoring self-referential (self-referencing) tables.
       /// </summary>
       /// <param name="foreignKeyInfos">A list of ForeignKeyInfo objects representing foreign key dependencies.</param>
       /// <returns>A list of table names in the order they should be deleted.</returns>
        public static List<string> GetDeletionOrder(List<ForeignKeyInfo> foreignKeyInfos)
        {
            // Build the graph
            var graph = new Dictionary<string, List<string>>();
            var visited = new HashSet<string>();
            var stack = new HashSet<string>();
            var sortedTables = new List<string>();
            var tables = new HashSet<string>();

            foreach (var fkInfo in foreignKeyInfos)
            {
                string child = fkInfo.ParentTable;       // Table containing the foreign key (child table)
                string parent = fkInfo.ReferencedTable;  // Table being referenced (parent table)

                // Ignore self-referential foreign keys
                if (child.Equals(parent, StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                // Add the child to the parent's adjacency list
                if (!graph.ContainsKey(parent))
                    graph[parent] = new List<string>();
                graph[parent].Add(child);

                // Add tables to the set
                tables.Add(child);
                tables.Add(parent);
            }

            // Include tables that might not have any dependencies
            foreach (var fkInfo in foreignKeyInfos)
            {
                tables.Add(fkInfo.ParentTable);
                tables.Add(fkInfo.ReferencedTable);
            }

            // Perform DFS-based topological sort to detect cycles
            foreach (var table in tables)
            {
                if (!visited.Contains(table))
                {
                    if (!TopologicalSortUtil(table, graph, visited, stack, sortedTables))
                    {
                        // Cycle detected
                        //throw new InvalidOperationException("A cycle was detected in the table dependencies.");
                    }
                }
            }

            // Reverse the list for deletion order (delete from child to parent)
            sortedTables.Reverse();

            return sortedTables;
        }

        private static bool TopologicalSortUtil(
            string table,
            Dictionary<string, List<string>> graph,
            HashSet<string> visited,
            HashSet<string> stack,
            List<string> sortedTables)
        {
            visited.Add(table);
            stack.Add(table);

            if (graph.ContainsKey(table))
            {
                foreach (var neighbor in graph[table])
                {
                    if (!visited.Contains(neighbor))
                    {
                        if (!TopologicalSortUtil(neighbor, graph, visited, stack, sortedTables))
                            return false;
                    }
                    else if (stack.Contains(neighbor))
                    {
                        // Cycle detected
                        Console.WriteLine($"Cycle detected: {string.Join(" -> ", stack)} -> {neighbor}");
                        return false;
                    }
                }
            }

            stack.Remove(table);
            sortedTables.Add(table);
            return true;
        }

    }
}

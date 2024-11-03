using CallaghanDev.Common.MSSQL;
using System;

public static class Program
{
    public static void Main(string[] args)
    {
        var deps = DatabaseHelper.GetForeignKeyDependencies(AppConfig.ConnectionString);
        // var GetDeletionOrder = TableDependencyResolver.GetDeletionOrder(deps);
        foreach (var item in deps)
        {
            Console.WriteLine(item);
        }
    }
}
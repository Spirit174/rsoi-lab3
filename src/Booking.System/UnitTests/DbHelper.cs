using Microsoft.EntityFrameworkCore;

namespace UnitTests;

public static class DbHelper
{
    public static T CreateContext<T>() where T : DbContext
    {
        var builder = new DbContextOptionsBuilder<T>();
        builder.UseInMemoryDatabase(Guid.NewGuid().ToString()); 

        return (T)Activator.CreateInstance(typeof(T), builder.Options)!;
    }

    public static void ClearDatabase<T>(T context) where T : DbContext
    {
        context.Database.EnsureDeleted();
        context.Database.EnsureCreated();
    }
}
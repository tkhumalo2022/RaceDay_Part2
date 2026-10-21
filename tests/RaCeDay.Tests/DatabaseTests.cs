using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using RaCeDay.Data;

namespace RaCeDay.Tests;

public class DatabaseTests
{
    [Fact]
    public void SqlServerMigrationCreatesIdentityAndCustomProfileColumns()
    {
        using var db = new ApplicationDbContext(new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseSqlServer("Server=(localdb)\\MSSQLLocalDB;Database=UnusedScriptOnly;Trusted_Connection=True").Options);
        Assert.NotEmpty(db.Database.GetMigrations());
        var sql = db.GetService<IMigrator>().GenerateScript();
        Assert.Contains("CREATE TABLE [AspNetUsers]", sql);
        Assert.Contains("[FirstName] nvarchar(60) NOT NULL", sql);
        Assert.Contains("[LastName] nvarchar(60) NOT NULL", sql);
        Assert.Contains("CREATE TABLE [AspNetUserRoles]", sql);
        Assert.DoesNotContain("DROP TABLE", sql);
    }
}

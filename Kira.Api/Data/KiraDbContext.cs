using Kira.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace Kira.Api.Data;

public class KiraDbContext : DbContext
{
    public KiraDbContext(DbContextOptions<KiraDbContext> options)
        : base(options)
    {
    }

    public DbSet<Project> Projects => Set<Project>();
}
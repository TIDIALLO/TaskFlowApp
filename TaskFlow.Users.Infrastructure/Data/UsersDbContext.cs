using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Reflection.Emit;
using TaskFlow.Users.Domain.Entities;

namespace TaskFlow.Users.Infrastructure.Data;

public class UsersDbContext : DbContext
{
    public DbSet<User> Users => Set<User>();

    public UsersDbContext(DbContextOptions<UsersDbContext> options) : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(UsersDbContext).Assembly);
    }
}
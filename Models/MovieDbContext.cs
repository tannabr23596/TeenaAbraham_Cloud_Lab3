using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using _301222912_abraham_mehta_Lab3.Models;

namespace _301222912_abraham_mehta_Lab3.Models;

public partial class MovieDbContext : DbContext
{
    public MovieDbContext()
    {
    }

    public MovieDbContext(DbContextOptions<MovieDbContext> options)
        : base(options)
    {
    }

   /* protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see http://go.microsoft.com/fwlink/?LinkId=723263.
        => optionsBuilder.UseSqlServer("Data Source=mssqlserver4api.cbavqb8qhxfj.ca-central-1.rds.amazonaws.com,1433;Database=MovieDB;User ID=teenaabraham;Password=Teena-2396;TrustServerCertificate=True");*/

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);

    public DbSet<_301222912_abraham_mehta_Lab3.Models.User>? User { get; set; }
}

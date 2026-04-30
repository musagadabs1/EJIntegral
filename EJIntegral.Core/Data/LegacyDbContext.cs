using EJIntegral.Core.Models.Legacy;
using Microsoft.EntityFrameworkCore;

namespace EJIntegral.Core.Data;

public class LegacyDbContext(DbContextOptions<LegacyDbContext> options) : DbContext(options)
{
    public DbSet<StaffDetails> StaffDetails => Set<StaffDetails>();
    public DbSet<ServiceDetails> ServiceDetails => Set<ServiceDetails>();
    public DbSet<Document> Documents => Set<Document>();
    public DbSet<StaffImage> StaffImages => Set<StaffImage>();
    public DbSet<DevelopmentArea> DevelopmentAreas => Set<DevelopmentArea>();
    public DbSet<Designation> Designations => Set<Designation>();
    public DbSet<Department> Departments => Set<Department>();
    public DbSet<LocalGovernment> LocalGovernments => Set<LocalGovernment>();
    public DbSet<EmpSalary> EmpSalaries => Set<EmpSalary>();
    public DbSet<EmployeeInfoWithPhoto> EmployeeInfoWithPhotos => Set<EmployeeInfoWithPhoto>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<StaffDetails>().ToTable("staff_details");
        modelBuilder.Entity<ServiceDetails>().ToTable("service_details");
        modelBuilder.Entity<Document>().ToTable("documents");
        modelBuilder.Entity<StaffImage>().ToTable("staff_images");
        modelBuilder.Entity<DevelopmentArea>().ToTable("development_area");
        modelBuilder.Entity<Designation>().ToTable("designation");
        modelBuilder.Entity<Department>().ToTable("department");
        modelBuilder.Entity<LocalGovernment>().ToTable("local_government");
        modelBuilder.Entity<EmpSalary>().ToTable("emp_salaries");

        modelBuilder.Entity<EmployeeInfoWithPhoto>().HasNoKey();
    }
}

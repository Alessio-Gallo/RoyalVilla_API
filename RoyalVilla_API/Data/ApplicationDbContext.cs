using System.Security.Cryptography.X509Certificates;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using RoyalVilla_API.Models; 

namespace RoyalVilla_API.Data;

public class ApplicationDbContext(DbContextOptions options) : IdentityDbContext(options)
{
    public DbSet<Villa> Villa { get; set; }
    public DbSet<User> User { get; set; }
    public DbSet<VillaAmenities> VillaAmenities { get; set; }
}


using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System;

namespace GCPTestAPI
{
    public class ContactContext : DbContext
    {
        public ContactContext(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public DbSet<Contact> Contacts { get; set; }
        public IConfiguration Configuration { get; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            builder.Entity<Contact>()
                .Property(x => x.Id)
                .HasDefaultValueSql("NEWID()");
            builder.Entity<Contact>()
                .HasKey(c => c.Id);
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlServer(Configuration.GetConnectionString("TestDB"));
        }

        internal void Where()
        {
            throw new NotImplementedException();
        }
    }
}

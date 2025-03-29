using Microsoft.EntityFrameworkCore;

namespace peopleapi
{
    public class PeopleDb : DbContext
    {
        public PeopleDb(DbContextOptions<PeopleDb> options) : base(options)
        {

        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            var personEntity = modelBuilder.Entity<PersonEntity>();
            personEntity.ToTable("Person");
            personEntity.HasKey(i => i.Id);

            personEntity.Property(p => p.FirstName).HasMaxLength(250);
            personEntity.Property(p => p.LastName).HasMaxLength(250);
            personEntity.Property(p => p.PhoneNumber).HasMaxLength(250);
        }

        public DbSet<PersonEntity> People { get; set; }
    }
}
using DBfirst.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DBfirst.Configurations.DataConfig
{
    public class StudentConfiguration : IEntityTypeConfiguration<Student>
    {
        public void Configure(EntityTypeBuilder<Student> builder)
        {
            // Ensure the "Student" table is mapped to the Student entity
            builder.ToTable("Student");

            // Define the primary key for the Student entity
            builder.HasKey(x => x.StudentId);

            builder.Property(e => e.Name).HasMaxLength(30);
            builder.HasOne(d => d.User)
                    .WithMany(p => p.Students)
                    .HasForeignKey(d => d.AccountId);
        }
    }
}

using DBfirst.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DBfirst.Configurations.DataConfig
{
    public class StudentSubjectConfiguration : IEntityTypeConfiguration<StudentSubject>
    {
        public void Configure(EntityTypeBuilder<StudentSubject> builder)
        {
            // Rename the table to something appropriate like "StudentSubjects"
            builder.ToTable("StudentSubjects");

            // Define the composite primary key
            builder.HasKey(x => new { x.StudentId, x.SubjectId });

            // Define the foreign key relationships
            builder.HasOne(x => x.Student)
                   .WithMany(x => x.StudentSubjects)
                   .HasForeignKey(x => x.StudentId)
                   .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(x => x.Subject)
                   .WithMany(x => x.StudentSubjects)
                   .HasForeignKey(x => x.SubjectId)
                   .OnDelete(DeleteBehavior.Cascade);
        }
    }
}

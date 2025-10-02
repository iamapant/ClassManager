using DBfirst.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System.Security.Claims;

namespace DBfirst.Configurations.DataConfig
{
    public class ClassConfiguration : IEntityTypeConfiguration<Class>
    {
        public void Configure(EntityTypeBuilder<Class> builder)
        {
            builder.HasKey(x => x.ClassId);
            builder.ToTable("Class");
            builder.Property(e => e.ClassName).HasMaxLength(20);
            builder.HasOne(d => d.Subject)
                    .WithMany(p => p.Classes)
                    .HasForeignKey(d => d.SubjectId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK__Class__SubjectId__3A81B327");
            builder.HasOne(d => d.Teacher)
                    .WithMany(p => p.Classes)
                    .HasForeignKey(d => d.TeacherId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK__Class__TeacherId__398D8EEE");
            builder.HasMany(d => d.Students)
                    .WithMany(p => p.Classes)
                    .UsingEntity<Dictionary<string, object>>(
                        "ClassStudent",
                        l => l.HasOne<Student>().WithMany().HasForeignKey("StudentId").OnDelete(DeleteBehavior.ClientSetNull).HasConstraintName("FK__ClassStud__Stude__3E52440B"),
                        r => r.HasOne<Class>().WithMany().HasForeignKey("ClassId").OnDelete(DeleteBehavior.ClientSetNull).HasConstraintName("FK__ClassStud__Class__3D5E1FD2"),
                        j =>
                        {
                            j.HasKey("ClassId", "StudentId").HasName("PK__ClassStu__48357579D7EF40EA");

                            j.ToTable("ClassStudent");
                        });
        }
    }
}

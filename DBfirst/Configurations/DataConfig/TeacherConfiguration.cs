using DBfirst.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DBfirst.Configurations.DataConfig
{
    public class TeacherConfiguration : IEntityTypeConfiguration<Teacher>
    {
        public void Configure(EntityTypeBuilder<Teacher> builder)
        {
            builder.HasKey(x => x.TeacherId);
            builder.ToTable("Teacher");
            builder.Property(e => e.AccountId).HasMaxLength(450);
            builder.Property(e => e.Name).HasMaxLength(30);
            builder.HasOne(d => d.User)
                    .WithMany(p => p.Teachers)
                    .HasForeignKey(d => d.AccountId);
            builder.HasOne(d => d.Subject)
                .WithMany(p => p.Teachers)
                .HasForeignKey(d => d.SubjectId);
        }
    }
}

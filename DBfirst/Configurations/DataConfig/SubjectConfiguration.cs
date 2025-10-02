using DBfirst.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DBfirst.Configurations.DataConfig
{
    public class SubjectConfiguration : IEntityTypeConfiguration<Subject>
    {
        public void Configure(EntityTypeBuilder<Subject> builder)
        {
            builder.HasKey(x => x.SubjectId);
            builder.ToTable("Subject");
            builder.Property(e => e.SubjectName).HasMaxLength(30);
        }
    }
}

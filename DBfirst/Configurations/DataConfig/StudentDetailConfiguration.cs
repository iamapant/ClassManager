using DBfirst.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DBfirst.Configurations.DataConfig
{
    public class StudentDetailConfiguration : IEntityTypeConfiguration<StudentDetail>
    {
        public void Configure(EntityTypeBuilder<StudentDetail> builder)
        {
            builder.HasKey(e => e.StudentDetailsId);
            builder.Property(e => e.AdditionalInformation).HasMaxLength(50);
            builder.Property(e => e.Address).HasMaxLength(50);
            builder.HasOne(d => d.Student)
                   .WithMany(p => p.StudentDetails)
                   .HasForeignKey(d => d.StudentId);
        }
    }
}

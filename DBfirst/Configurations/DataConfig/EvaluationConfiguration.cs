using DBfirst.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DBfirst.Configurations.DataConfig
{
    public class EvaluationConfiguration : IEntityTypeConfiguration<Evaluation>
    {
        public void Configure(EntityTypeBuilder<Evaluation> builder)
        {
            builder.HasKey(x => x.EvaluationId);
            builder.ToTable("Evaluation");
            builder.Property(e => e.AdditionExplanation).HasMaxLength(100);
            builder.HasOne(d => d.Student)
                   .WithMany(p => p.Evaluations)
                   .HasForeignKey(d => d.StudentId);
        }
    }
}

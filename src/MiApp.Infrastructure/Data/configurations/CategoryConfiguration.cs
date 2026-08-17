using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MiApp.Domain.Entities;

public class CategoryConfiguration : IEntityTypeConfiguration<Category>
{
    public void Configure(EntityTypeBuilder<Category> builder)
    {
        builder.ToTable("Categories");
        builder.HasKey(c => c.Id);
        builder.Property(c => c.Id).ValueGeneratedOnAdd();
        builder.Property(c => c.Name).IsRequired().HasMaxLength(100);
        builder.Property(c => c.Description).HasMaxLength(500);
        builder.Property(c => c.IsActive).HasDefaultValue(true);
        builder.HasOne(c => c.User).WithMany(u => u.Categories).HasForeignKey(c => c.UserId);

    }
}

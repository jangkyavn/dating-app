using DatingApp.Data.Entities;
using DatingApp.Data.Extentions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DatingApp.Data.Configurations
{
    public class PhotoConfiguration : DbEntityConfiguration<Photo>
    {
        public override void Configure(EntityTypeBuilder<Photo> entity)
        {
            entity.ToTable("Photos");

            entity.HasOne(x => x.User)
                .WithMany(y => y.Photos)
                .HasForeignKey(z => z.UserId);
        }
    }
}

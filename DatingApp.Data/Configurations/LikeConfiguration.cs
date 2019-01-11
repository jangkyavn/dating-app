using DatingApp.Data.Entities;
using DatingApp.Data.Extentions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DatingApp.Data.Configurations
{
    public class LikeConfiguration : DbEntityConfiguration<Like>
    {
        public override void Configure(EntityTypeBuilder<Like> entity)
        {
            entity.ToTable("Likes");

            entity.HasKey(x => new { x.LikerId, x.LikeeId });

            entity.HasOne(x => x.Likee)
                .WithMany(x => x.Likers)
                .HasForeignKey(x => x.LikeeId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(x => x.Liker)
                .WithMany(x => x.Likees)
                .HasForeignKey(x => x.LikerId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}

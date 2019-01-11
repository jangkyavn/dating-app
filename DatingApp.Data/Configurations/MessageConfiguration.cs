using DatingApp.Data.Entities;
using DatingApp.Data.Extentions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DatingApp.Data.Configurations
{
    public class MessageConfiguration : DbEntityConfiguration<Message>
    {
        public override void Configure(EntityTypeBuilder<Message> entity)
        {
            entity.ToTable("Messages");

            entity.HasOne(x => x.Sender)
                .WithMany(x => x.MessagesSend)
                .HasForeignKey(x => x.SenderId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(x => x.Recipient)
                .WithMany(x => x.MessagesReceived)
                .HasForeignKey(x => x.RecipientId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}

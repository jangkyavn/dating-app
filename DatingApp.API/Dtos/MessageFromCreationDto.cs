using System;

namespace DatingApp.API.Dtos
{
    public class MessageFromCreationDto
    {
        public MessageFromCreationDto()
        {
            MessageSent = DateTime.Now;
        }

        public int SenderId { get; set; }
        public int RecipientId { get; set; }
        public string Content { get; set; }
        public DateTime MessageSent { get; set; }
    }
}

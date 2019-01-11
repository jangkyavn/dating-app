using AutoMapper;
using DatingApp.API.Dtos;
using DatingApp.API.Helpers;
using DatingApp.Application.IRepositories;
using DatingApp.Data.Entities;
using DatingApp.Utilities.Helpers;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;

namespace DatingApp.API.Controllers
{
    [ServiceFilter(typeof(LogUserActivity))]
    [Route("api/users/{userId}/[controller]")]
    public class MessagesController : BaseController
    {
        private readonly IDatingRepository _datingRepository;
        private readonly IMapper _mapper;

        public MessagesController(IDatingRepository datingRepository,
            IMapper mapper)
        {
            _datingRepository = datingRepository;
            _mapper = mapper;
        }

        [HttpGet("{id}", Name = "GetMessage")]
        public async Task<IActionResult> GetMessage(int userId, int id)
        {
            if (userId != int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value))
            {
                return Unauthorized();
            }

            var messageForRepository = await _datingRepository.GetMessage(id);
            if (messageForRepository == null)
            {
                return NotFound();
            }

            return Ok(messageForRepository);
        }

        [HttpGet("thread/{recipientID}")]
        public async Task<IActionResult> GetMessageThread(int userId, int recipientId)
        {
            if (userId != int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value))
            {
                return Unauthorized();
            }

            var messageFromRepository = await _datingRepository.GetMessageThread(userId, recipientId);
            var messageThread = _mapper.Map<IEnumerable<MessageToReturnDto>>(messageFromRepository);

            return Ok(messageThread);
        }

        [HttpGet]
        public async Task<IActionResult> GetMessagesFromUser(int userId, [FromQuery]MessageParams messageParams)
        {
            if (userId != int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value))
            {
                return Unauthorized();
            }

            messageParams.UserId = userId;

            var messagesFromRepository = await _datingRepository.GetMessagesForUser(messageParams);

            var messages = _mapper.Map<IEnumerable<MessageToReturnDto>>(messagesFromRepository);

            Response.AddPagination(messagesFromRepository.CurrentPage, messagesFromRepository.PageSize, messagesFromRepository.TotalCount, messagesFromRepository.TotalPages);

            return Ok(messages);
        }

        [HttpPost]
        public async Task<IActionResult> CreateMessage(int userId, MessageFromCreationDto messageFromCreationDto)
        {
            var sender = await _datingRepository.GetUser(userId);

            if (sender.Id != int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value))
            {
                return Unauthorized();
            }

            messageFromCreationDto.SenderId = userId;
            var recipient = await _datingRepository.GetUser(messageFromCreationDto.RecipientId);

            if (recipient == null)
            {
                return BadRequest("Không tìm thấy người dùng");
            }

            var message = _mapper.Map<Message>(messageFromCreationDto);

            _datingRepository.Add(message);

            if (await _datingRepository.SaveAll())
            {
                var messageToReturn = _mapper.Map<MessageToReturnDto>(message);
                return CreatedAtRoute("GetMessage", new { id = message.Id }, messageToReturn);
            }

            throw new Exception("Lỗi khi tạo tin nhắn");
        }

        [HttpPut("{id}/read")]
        public async Task<IActionResult> MarkMessageAsRead(int id, int userId)
        {
            if (userId != int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value))
            {
                return Unauthorized();
            }

            var messageFromRepo = await _datingRepository.GetMessage(id);

            if (messageFromRepo.RecipientId != userId)
            {
                return Unauthorized();
            }

            messageFromRepo.IsRead = true;
            messageFromRepo.DateRead = DateTime.Now;

            if (await _datingRepository.SaveAll())
            {
                return NoContent();
            }

            throw new Exception("Lỗi khi đánh dấu tin nhắn");
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteMessage(int id, int userId)
        {
            if (userId != int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value))
            {
                return Unauthorized();
            }

            var messageFromRepo = await _datingRepository.GetMessage(id);

            if (messageFromRepo.SenderId == userId)
            {
                messageFromRepo.SenderDeleted = true;
            }

            if (messageFromRepo.RecipientId == userId)
            {
                messageFromRepo.RecipientDeleted = true;
            }

            if (messageFromRepo.SenderDeleted && messageFromRepo.RecipientDeleted)
            {
                _datingRepository.Delete(messageFromRepo);
            }

            if (await _datingRepository.SaveAll()) 
            {
                return NoContent();
            }

            throw new Exception("Có lỗi xảy ra khi xóa tin nhắn");
        }
    }
}
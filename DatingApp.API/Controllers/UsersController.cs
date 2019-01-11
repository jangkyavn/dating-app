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
    public class UsersController : BaseController
    {
        private readonly IDatingRepository _datingRepository;
        private readonly IMapper _mapper;

        public UsersController(IDatingRepository datingRepository, IMapper mapper)
        {
            _datingRepository = datingRepository;
            _mapper = mapper;
        }

        [HttpGet]
        public async Task<IActionResult> GetUsers([FromQuery]UserParams userParams)
        {
            var currentUserId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value);

            var userFromRepository = await _datingRepository.GetUser(currentUserId);

            userParams.UserId = currentUserId;

            if (string.IsNullOrEmpty(userParams.Gender))
            {
                userParams.Gender = userFromRepository.Gender == "male" ? "female" : "male";
            }

            var users = await _datingRepository.GetUsers(userParams);

            var usersToReturn = _mapper.Map<IEnumerable<UserForListDto>>(users);

            Response.AddPagination(users.CurrentPage, users.PageSize, users.TotalCount, users.TotalPages);

            return Ok(usersToReturn);
        }

        [HttpGet("{id}", Name = "GetUser")]
        public async Task<IActionResult> GetUser(int id)
        {
            var user = await _datingRepository.GetUser(id);

            var userToReturn = _mapper.Map<UserForDetailedDto>(user);

            return Ok(userToReturn);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateUser(int id, UserForUpdateDto userForUpdateDto)
        {
            if (id != int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value))
            {
                return Unauthorized();
            }

            var userForRespotiry = await _datingRepository.GetUser(id);
            _mapper.Map(userForUpdateDto, userForRespotiry);

            if (await _datingRepository.SaveAll())
            {
                return NoContent();
            }

            throw new Exception($"Cập nhật người dùng {id} bị lỗi");
        }

        [HttpPost("{id}/like/{recipientId}")]
        public async Task<IActionResult> LikeUser(int id, int recipientId)
        {
            if (id != int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value))
            {
                return Unauthorized();
            }

            var like = await _datingRepository.GetLike(id, recipientId);

            if (like != null)
            {
                return BadRequest("Bạn đã like người này rồi");
            }

            if (await _datingRepository.GetUser(recipientId) == null)
            {
                return NotFound();
            }

            like = new Like()
            {
                LikerId = id,
                LikeeId = recipientId
            };

            _datingRepository.Add(like);

            if (await _datingRepository.SaveAll())
            {
                return Ok();
            }

            return BadRequest("Lỗi khi thích");
        }
    }
}
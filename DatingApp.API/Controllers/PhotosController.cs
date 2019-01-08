using AutoMapper;
using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using DatingApp.API.Dtos;
using DatingApp.API.Helpers;
using DatingApp.Application.Repositories;
using DatingApp.Data.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace DatingApp.API.Controllers
{
    [Route("api/users/{userId}/photos")]
    public class PhotosController : BaseController
    {
        private readonly IDatingRepository _datingRepository;
        private readonly IMapper _mapper;
        private readonly IOptions<CloudinarySettings> _cloudinaryConfig;
        private Cloudinary _cloudinary;

        public PhotosController(IDatingRepository datingRepository,
            IMapper mapper,
            IOptions<CloudinarySettings> cloudinaryConfig)
        {
            _datingRepository = datingRepository;
            _mapper = mapper;
            _cloudinaryConfig = cloudinaryConfig;

            Account account = new Account()
            {
                Cloud = _cloudinaryConfig.Value.CloudName,
                ApiKey = _cloudinaryConfig.Value.ApiKey,
                ApiSecret = _cloudinaryConfig.Value.ApiSecret
            };

            _cloudinary = new Cloudinary(account);
        }

        [HttpGet("{id}", Name = "GetPhoto")]
        public async Task<IActionResult> GetPhoto(int id)
        {
            var photoForRepository = await _datingRepository.GetPhoto(id);

            var photo = _mapper.Map<PhotoForReturnDto>(photoForRepository);

            return Ok(photo);
        }

        [HttpPost]
        public async Task<IActionResult> AddPhotoForUser(int userId, [FromForm] PhotoForCreationDto photoForCreationDto)
        {
            if (userId != int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value))
            {
                return Unauthorized();
            }

            var userForRepository = await _datingRepository.GetUser(userId);

            var file = photoForCreationDto.File;

            var uploadResult = new ImageUploadResult();

            if (file.Length > 0)
            {
                using (var stream = file.OpenReadStream())
                {
                    var uploadParams = new ImageUploadParams()
                    {
                        File = new FileDescription(file.Name, stream),
                        Transformation = new Transformation().Width(500).Height(500).Crop("fill").Gravity("face")
                    };

                    uploadResult = _cloudinary.Upload(uploadParams);
                }
            }

            photoForCreationDto.Url = uploadResult.Uri.ToString();
            photoForCreationDto.PublicId = uploadResult.PublicId;

            var photo = _mapper.Map<Photo>(photoForCreationDto);

            if (!userForRepository.Photos.Any(x => x.IsMain))
            {
                photo.IsMain = true;
            }

            userForRepository.Photos.Add(photo);

            if (await _datingRepository.SaveAll())
            {
                var photoToReturn = _mapper.Map<PhotoForReturnDto>(photo);
                return CreatedAtRoute("GetPhoto", new { id = photo.Id }, photoToReturn);
            }

            return BadRequest("Lỗi không thể thêm ảnh");
        }

        [HttpPost("{id}/setMain")]
        public async Task<IActionResult> SetMainPhoto(int userId, int id)
        {
            if (userId != int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value))
            {
                return Unauthorized();
            }

            var userForRepository = await _datingRepository.GetUser(userId);

            if (!userForRepository.Photos.Any(x => x.Id == id))
            {
                return Unauthorized();
            }

            var photoForRepository = await _datingRepository.GetPhoto(id);

            if (photoForRepository.IsMain)
            {
                return BadRequest("Ảnh này đã được làm đại diện");
            }

            var currentMainPhoto = await _datingRepository.GetMainPhotoForUser(userId);
            currentMainPhoto.IsMain = false;

            photoForRepository.IsMain = true;

            if (await _datingRepository.SaveAll())
            {
                return NoContent();
            }

            return BadRequest("Không thể đặt ảnh làm đại diện");
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeletePhoto(int userId, int id)
        {
            if (userId != int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value))
            {
                return Unauthorized();
            }

            var userForRepository = await _datingRepository.GetUser(userId);

            if (!userForRepository.Photos.Any(x => x.Id == id))
            {
                return Unauthorized();
            }

            var photoForRepository = await _datingRepository.GetPhoto(id);

            if (photoForRepository.IsMain)
            {
                return BadRequest("Bạn không thể xóa ảnh đại diện được");
            }

            if (photoForRepository.PublicId != null)
            {
                var deleteParams = new DeletionParams(photoForRepository.PublicId);

                var result = _cloudinary.Destroy(deleteParams);

                if (result.Result == "ok")
                {
                    _datingRepository.Delete(photoForRepository);
                }
            }

            if (photoForRepository.PublicId == null)
            {
                _datingRepository.Delete(photoForRepository);
            }
            
            if (await _datingRepository.SaveAll())
            {
                return Ok();
            }

            return BadRequest("Lỗi khi xóa ảnh");
        }
    }
}
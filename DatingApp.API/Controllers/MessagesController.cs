using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using AutoMapper;
using DatingApp.API.Data;
using DatingApp.API.Dtos;
using DatingApp.API.Helpers;
using DatingApp.API.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DatingApp.API.Controllers
{
    [ServiceFilter(typeof(LogUserActivity))]
    [Authorize]
    [Route("api/users/{userId}/[controller]")]
    [ApiController]
    public class MessagesController : ControllerBase
    {
        private readonly IDatingRepository repo;
        private readonly IMapper mapper;

        public MessagesController(IDatingRepository _repo, IMapper _mapper)
        {
            repo = _repo;
            mapper = _mapper;
        }

        [HttpGet("{id}", Name = "GetMessage")]
        public async Task<IActionResult> GetMessage(int userId, int id)
        {
            if(userId != int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value))
                return Unauthorized();

            var messageFromRepo = await repo.GetMessage(id);

            if(messageFromRepo == null)
                return NotFound();
            
            return Ok(messageFromRepo);
        }

        [HttpGet]
        public async Task<IActionResult> GetMessageForUser(int userId, [FromQuery]MessageParams messageParams)
        {
            if(userId != int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value))
                return Unauthorized();

            messageParams.UserId = userId;

            var messagesFromRepo = await repo.GetMessageForUser(messageParams);

            var messages = mapper.Map<IEnumerable<MessageToReturnDto>>(messagesFromRepo);

            Response.AddPagination(messagesFromRepo.CurrentPage, messagesFromRepo.PageSize,
            messagesFromRepo.TotalCount, messagesFromRepo.TotalPages);

            return Ok(messages);
        }

        [HttpGet("thread/{recipientId}")]
        public async Task<IActionResult> GetMessageThread(int userId, int recipientId)
        {
            if(userId != int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value))
                return Unauthorized();

            var messageFromRepo = await repo.GetMessageThread(userId, recipientId);

            var messageThread = mapper.Map<IEnumerable<MessageToReturnDto>>(messageFromRepo);

            return Ok(messageThread);
        }


        [HttpPost]
        public async Task<IActionResult> CreateMessage(int userId, MessageForCreationDto messageForCreation)
        {
            var sender = await repo.GetUser(userId);
            if(sender.Id != int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value))
                return Unauthorized();

            messageForCreation.SenderId = userId;

            var recipient = await repo.GetUser(messageForCreation.RecipientId);

            if(recipient == null)
                return BadRequest("Could not find user");

            var message = mapper.Map<Message>(messageForCreation);

            repo.Add(message);

            if(await repo.SaveAll()){
                var messageToReturn = mapper.Map<MessageToReturnDto>(message);
                return CreatedAtRoute("GetMessage", new{id = message.Id}, messageToReturn);
            }
            

            throw new Exception("Creating on message failed on save");
        }
    }
}
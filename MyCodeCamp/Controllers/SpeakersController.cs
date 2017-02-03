﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using MyCodeCamp.Data;
using MyCodeCamp.Data.Entities;
using MyCodeCamp.Filters;
using MyCodeCamp.Models;

namespace MyCodeCamp.Controllers
{
    [Route("api/camps/{moniker}/speakers")]
    [ValidateModel]
    [ApiVersion("1.0")]
    [ApiVersion("1.1")]
    public class SpeakersController : BaseController
    {
        private ILogger<SpeakersController> _logger;
        private IMapper _mapper;
        private ICampRepository _repo;
        private UserManager<CampUser> _userManager;

        public SpeakersController(
            ICampRepository repo,
            ILogger<SpeakersController> logger,
            IMapper mapper,
            UserManager<CampUser> userManager)
        {
            _repo = repo;
            _logger = logger;
            _mapper = mapper;
            _userManager = userManager;
        }

        [HttpGet]
        [MapToApiVersion("1.0")]
        public IActionResult Get(string moniker,bool includeTalks = false)
        {
            var speakers = includeTalks ? _repo.GetSpeakersByMonikerWithTalks(moniker) : _repo.GetSpeakersByMoniker(moniker);

            return Ok(_mapper.Map<IEnumerable<SpeakerModel>>(speakers));
        }

        [HttpGet]
        [MapToApiVersion("1.1")]
        public IActionResult GetV11(string moniker,bool includeTalks = false)
        {
            var speakers = includeTalks ? _repo.GetSpeakersByMonikerWithTalks(moniker) : _repo.GetSpeakersByMoniker(moniker);

            return Ok(new { count = speakers.Count(),speakers = _mapper.Map<IEnumerable<SpeakerModel>>(speakers) });
        }

        [HttpGet("{id}",Name = "SpeakerGet")]
        public IActionResult Get(string moniker,int id,bool includeTalks = false)
        {
            var speaker = includeTalks ? _repo.GetSpeakerWithTalks(id) : _repo.GetSpeaker(id);


            if (speaker == null) return NotFound();
            if (speaker.Camp.Moniker != moniker) return BadRequest("Speaker not in specified Camp");

            return Ok(_mapper.Map<SpeakerModel>(speaker));

        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> Post(string moniker,[FromBody]SpeakerModel model)
        {
            try
            {
                var camp = _repo.GetCampByMoniker(moniker);
                if (camp == null) return BadRequest("Could not find camp");

                var speaker = _mapper.Map<Speaker>(model);
                speaker.Camp = camp;

                var campUser = await _userManager.FindByNameAsync(this.User.Identity.Name);
                if (campUser != null)
                {
                    speaker.User = campUser;

                    _repo.Add(speaker);
                    if (await _repo.SaveAllAsync())
                    {
                        var url = Url.Link("SpeakerGet",new { moniker = camp.Moniker,id = speaker.Id });
                        return Created(url,_mapper.Map<SpeakerModel>(speaker));
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Exception thrown while adding speaker: {ex}");

            }

            return BadRequest("Could not add new speaker");
        }

        [HttpPut("{id}")]
        [Authorize]
        public async Task<IActionResult> Put(string moniker,int id,[FromBody]SpeakerModel model)
        {
            try
            {
                var speaker = _repo.GetSpeaker(id);
                if (speaker == null) return NotFound();
                if (speaker.Camp.Moniker != moniker) return BadRequest("Speaker and Camp do not match");

                if (speaker.User.UserName != this.User.Identity.Name) return Forbid();

                _mapper.Map(model,speaker);

                if (await _repo.SaveAllAsync())
                {
                    return Ok(_mapper.Map<SpeakerModel>(speaker));
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Exception thrown while updating speaker: {ex}");
            }

            return BadRequest("Could not update Speaker");
        }

        [HttpDelete("{id}")]
        [Authorize]
        public async Task<IActionResult> Delete(string moniker,int id)
        {
            try
            {
                var speaker = _repo.GetSpeaker(id);
                if (speaker == null) return NotFound();
                if (speaker.Camp.Moniker != moniker) return BadRequest("Speaker and Camp do not match");

                if (speaker.User.UserName != this.User.Identity.Name) return Forbid();

                _repo.Delete(speaker);

                if (await _repo.SaveAllAsync())
                {
                    return NoContent();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Exception thrown while deleting speaker: {ex}");
            }

            return BadRequest("Could not delete Speaker");
        }
    }
}

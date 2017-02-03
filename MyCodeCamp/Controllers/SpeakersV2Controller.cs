using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
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
    [ApiVersion("2.0")]
    public class SpeakersV2Controller : SpeakersController
    {
        public SpeakersV2Controller(ICampRepository repo,
            ILogger<SpeakersController> logger,
            IMapper mapper,
            UserManager<CampUser> userManager)
            :base(repo,logger,mapper,userManager)
        {
        }

        public override IActionResult GetWithCount(string moniker,bool includeTalks = false)
        {
            var speakers = includeTalks ? _repo.GetSpeakersByMonikerWithTalks(moniker) : _repo.GetSpeakersByMoniker(moniker);
            return Ok(new
            {
                currentTime = DateTime.UtcNow,
                count = speakers.Count(),
                speakers = _mapper.Map<IEnumerable<SpeakerV2Model>>(speakers)
            });
        }

        [MapToApiVersion("0.1")]
        public override IActionResult Get(string moniker,bool includeTalks = false)
        {
            return base.Get(moniker,true);
        }
    }
}

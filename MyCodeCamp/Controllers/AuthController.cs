﻿using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using MyCodeCamp.Data;
using MyCodeCamp.Data.Entities;
using MyCodeCamp.Filters;

namespace MyCodeCamp.Controllers
{
    public class AuthController : Controller
    {
        private CampContext _context;
        private ILogger<AuthController> _logger;
        private SignInManager<CampUser> _signInManager;

        public AuthController(CampContext context,
            SignInManager<CampUser> signInManager,
            ILogger<AuthController> logger)
        {
            _context = context;
            _signInManager = signInManager;
            _logger = logger;
        }

        [HttpPost("api/auth/login")]
        [ValidateModel]
        public async Task<IActionResult> Login([FromBody]CredentialModel model)
        {
            try
            {
                var result = await _signInManager.PasswordSignInAsync(model.UserName,model.Password,false,false);
                if (result.Succeeded)
                {
                    return Ok();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Exception thrown while logging in: {ex}");
            }

            return BadRequest("Failed to login");
        }
    }
}
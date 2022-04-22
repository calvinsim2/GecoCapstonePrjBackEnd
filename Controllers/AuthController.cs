using CapstoneProjectBlog.Data.Context;
using CapstoneProjectBlog.Dtos;
using CapstoneProjectBlog.Helpers;
using CapstoneProjectBlog.Models;
using CapstoneProjectBlog.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Threading.Tasks;

namespace CapstoneProjectBlog.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class AuthController : ControllerBase
    {
        private readonly BlogContext _context;
        public readonly IConfiguration _config;
        private readonly IMailService _mailService;

        public AuthController(BlogContext context, IConfiguration config, IMailService mailService)
        {
            this._context = context;
            this._config = config;
            this._mailService = mailService;
        }

        
        [HttpPost("login")]
        [AllowAnonymous]

        // In the ActionResult, if we are using DTO, we need to change to the appropriate DTO
        // DTO is what we want to send it back to frontend.
        // LoginDto consists of Email and Password only.
        public async Task<ActionResult<LoginDto>> LoginEmployee([FromBody] LoginDto loginDtoObj)
        {
            if (loginDtoObj == null)
            {
                return BadRequest(new
                {
                    StatusCode = 404,
                    Message = "Empty value detected."
                });
            }

            // check if user is in database, first one that I find. 
            // the fetched user record, compare its Email with the Dto Email that was sent in.
            var user = await _context.UserModels.FirstOrDefaultAsync(a => a.Email == loginDtoObj.Email);
            if (user == null)
            {
                return NotFound(new
                {
                    StatusCode = 404,
                    Message = "No such user with Email exist"
                }) ;
            }
            // takes in user inputted password, passwordHash and PasswordSalt from database.
            // if this verifyHashPassword returns false, return 404
            if (!EncDescPassword.VerifyHashPassword(loginDtoObj.Password, user.PasswordHash, user.PasswordSalt))
            {
                return NotFound(new
                {
                    StatusCode = 404,
                    Message = "Incorrect Password"
                });
            }

            // once user log in is successful, we want to give this user a jwt token. 
            // and since we created CreateJwtToken to accept a UserModel, we have to put user in it.
            string token = CreateJwtToken(user);
            return Ok(new
            {
                StatusCode = 200,
                Message = "Login Success",
                Token = token
            });
        }

        [HttpPut("changepassword")]
        public async Task<ActionResult<ChangePasswordDto>> ChangePassword([FromBody] ChangePasswordDto changePasswordDtoObj)
        {
            if (changePasswordDtoObj == null)
            {
                return BadRequest();
            }

            var user = await _context.UserModels.FirstOrDefaultAsync(a => a.Email == changePasswordDtoObj.Email);
            if (user == null)
            {
                return NotFound(new
                {
                    StatusCode = 404,
                    Message = "No Such Email found"
                });
            }

            if (!EncDescPassword.VerifyHashPassword(changePasswordDtoObj.OldPassword, user.PasswordHash, user.PasswordSalt))
            {
                return NotFound(new
                {
                    StatusCode = 404,
                    Message = "Incorrect Password"
                });
            }
            EncDescPassword.CreateHashPassword(changePasswordDtoObj.NewPassword, out byte[] passwordHash, out byte[] passwordSalt);
            user.PasswordHash = passwordHash;
            user.PasswordSalt = passwordSalt;
            // put into database, need model, NOT DTO
            _context.Entry(user).State = EntityState.Modified; // updates 
            await _context.SaveChangesAsync();
            return Ok(new
            {
                StatusCode = 200,
                Message = "Success - Password Changed."
            });
        }

        // RESET PASSWORD ====================================================================================================================
        [HttpPut("resetpassword")]
        [AllowAnonymous]
        public async Task<ActionResult<ResetPasswordDto>> ResetPassword([FromBody] ResetPasswordDto resetPasswordDtoObj)
        {
            if (resetPasswordDtoObj == null)
            {
                return BadRequest(new
                {
                    StatusCode = 400,
                    Message = "Are you even sending something?"
                });
            }

            var user = await _context.UserModels.FirstOrDefaultAsync(a => a.Email == resetPasswordDtoObj.Email);
            if (user == null)
            {
                return NotFound(new
                {
                    StatusCode = 404,
                    Message = "Nope, no such email exists, did you even forget your own email?!"
                });
            }

            Random r = new Random();
            int getRandomNumber = r.Next(100001, 999999);
            string randomNumberInString = getRandomNumber.ToString();

            EncDescPassword.CreateHashPassword(randomNumberInString, out byte[] passwordHash, out byte[] passwordSalt);
            user.PasswordHash = passwordHash;
            user.PasswordSalt = passwordSalt;
            // put into database, need model, NOT DTO
            _context.Entry(user).State = EntityState.Modified; // updates 
            await _context.SaveChangesAsync();

            MailModel mailModel = new MailModel();
            mailModel.ToEmail = resetPasswordDtoObj.Email;
            mailModel.Subject = "Password Resetted!";
            mailModel.Body = $"Hi {resetPasswordDtoObj.Email}," + "<br>" +
                                $"<p> Your Password has successfully been resetted!</p>" + "<br>" +
                                $"<p> Your new Password is : {randomNumberInString}</p>";

            await _mailService.SendEmailAsync(mailModel);
            return Ok(new
            {
                StatusCode = 200,
                Message = "Success - Password Has Been Resetted."
            });
        }

        // 5 parts - payload, key, credentials, expirytime, token
        // we need to pass in the UserModel, because our UserModel have all the properties we need.
        // we need to install a nuGet package - Microsoft.AspNetCore.Authentication.JwtBearer - version 5.x.x since
        // we are using .net 5
        private string CreateJwtToken(UserModel user)
        {
            // payload is called as claims in .net 
            // we are creating a List of claims in this case. 
            List<Claim> claimsList = new List<Claim>()
            {
                new Claim("FullName", user.FullName),
                new Claim("UserID", user.UserID.ToString()),
                new Claim("RoleID", user.RoleID.ToString()),
            };

            // Key creation.
            // import SSK from IdentityModel.token, SSK takes in a byte of key. 
            // thus we encode / convert our secret string into byte, for SSK to use.
            var key = new SymmetricSecurityKey(System.Text.Encoding.UTF8.GetBytes(_config["SecretKey:Key"]));

            // Credentials creation
            // Signing credentials need a key, and the algorithm we want to use, in this case it is HmacSha256. 
            var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            // import from IdentityModel.Tokens.Jwt
            // JwtSecurityToken takes in an object
            // token, will consist of the payload, key, credentials and expiry time
            var token = new JwtSecurityToken(
                claims: claimsList,
                expires: DateTime.Now.AddHours(1),
                signingCredentials: credentials
                );

            // we use this to help us encrypt the token which is to be returned to user upon sign in.
            // we need to pass in our created token. 
            var myJwt = new JwtSecurityTokenHandler().WriteToken(token);
            return myJwt;
        }
    }
}

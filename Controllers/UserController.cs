using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using CapstoneProjectBlog.Data.Context;
using CapstoneProjectBlog.Models;
using AutoMapper;
using CapstoneProjectBlog.Dtos;
using CapstoneProjectBlog.Helpers;
using Newtonsoft.Json;
using System.IO;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Authorization;
using CapstoneProjectBlog.Services;

namespace CapstoneProjectBlog.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class UserController : ControllerBase
    {
        private readonly BlogContext _context;
        private readonly IMapper _mapper;
        private IWebHostEnvironment _env;
        private readonly IMailService _mailService;

        public UserController(BlogContext context, IMapper mapper, IWebHostEnvironment env, IMailService mailService)
        {
            _context = context;
            _mapper = mapper;
            _env = env;
            _mailService = mailService;
        }

        // GET: api/User
        [HttpGet]
        public async Task<ActionResult<UserModel>> GetAllUser()
        {
            var userList = await _context.UserModels.Include(a => a.Comments).Include(b => b.Blogs).Include(c=>c.Role).ToListAsync();

            if (userList.Count < 1)
            {
                return Ok(new
                {
                    StatusCode = 200,
                    Message = "Success - No User Recorded.",

                });
            }

            return Ok(new
            {
                StatusCode = 200,
                Message = "Success",
                Result = userList,
            });
        }

        // GET: api/User/5
        [HttpGet("{id}")]
        public async Task<ActionResult<UserModel>> GetIndividualUser(int id)
        {
            var user = await _context.UserModels.Include(a => a.Comments).Include(b=>b.Blogs).Include(c => c.Role).AsNoTracking().FirstOrDefaultAsync(a => a.UserID == id);

            if (user == null)
            {
                return NotFound(new
                {
                    StatusCode = 404,
                    Message = "No User with given ID Found.",

                });
            }

            return Ok(new
            {
                StatusCode = 200,
                Message = "Success",
                Result = user,

            });
        }

        // POST: api/Role

        [HttpPost]
        [AllowAnonymous]
        public async Task<ActionResult<UserAddDto>> AddUser()
        {
            IFormFileCollection req = Request.Form.Files;
            var files = req;
            var userDtoString = Request.Form["UserDetails"];
            // remember, when we send it over from front end, data is in a string
            var userAddDtoObj = JsonConvert.DeserializeObject<UserAddDto>(userDtoString);
            // need www.root folder.
            // wwwroot/UserImages/<whatever your .jpg or png name is>
            var uploads = Path.Combine(_env.WebRootPath, "UserImages");
            if (userAddDtoObj == null)
            {
                return BadRequest(new
                {
                    StatusCode = 400,
                    Message = "Please input data",

                });
            }

            // check if RoleID matches existing roles
            if (userAddDtoObj.RoleID != 1 && userAddDtoObj.RoleID != 2)
            {
                return BadRequest(new
                {
                    StatusCode = 400,
                    Message = "RoleID assigned to unknown, please ensure correct RoleID is inserted.",

                });
            }

            // check if email already exists
            var isEmailExist = await _context.UserModels.AsNoTracking().FirstOrDefaultAsync(a => a.Email == userAddDtoObj.Email);
            if (isEmailExist != null)
            {
                return BadRequest(new
                {
                    StatusCode = 400,
                    Message = "Sorry, Email has been taken!",

                });
            }

            // if there are multiple images, we can use this. in future
            foreach (var file in files)
            {
                if (file.Length > 0)
                {
                    // if directory doesn't exist, we create a new directory.
                    if (!Directory.Exists(uploads))
                    {
                        Directory.CreateDirectory(uploads);
                    }
                    var urls = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT")?.Split(";");

                    var filepath = Path.Combine(uploads, file.FileName);
                    // a filestream class helps copy our stream data into our folder.
                    using (var fileStream = new FileStream(filepath, FileMode.Create))
                    {
                        file.CopyTo(fileStream);
                    }
                    userAddDtoObj.ProfileImgUrl = $"UserImages/{file.FileName}";
                }
            }

            // mapper has a method called Map. 
            // .Map<(desired data type)>(Source);
            var userModelObj = _mapper.Map<UserModel>(userAddDtoObj);
            userModelObj.JoinedDate = DateTime.Now;
            // static class, to call it, we need to call it by calling the class Name.
            // of course, to use this class, we have to import it.
            EncDescPassword.CreateHashPassword(userAddDtoObj.Password, out byte[] passwordHash, out byte[] passwordSalt);
            // passwordHash and passwordSalt, are returned by this CreateHashpassword. 
            userModelObj.PasswordHash = passwordHash;
            userModelObj.PasswordSalt = passwordSalt;
            await _context.UserModels.AddAsync(userModelObj);
            await _context.SaveChangesAsync();

            MailModel mailModel = new MailModel();
            mailModel.ToEmail = userModelObj.Email;
            mailModel.Subject = "New User Created!";
            mailModel.Body = $"Hi {userModelObj.FullName}," + "<br>" +
                                $"<p> Your Profile has been successfully created using email : {userModelObj.Email} !</p>" + "<br>" +
                                "Your Login credentials to InvokerBlog is: " + "<br>" +
                                $"Email: <strong> {userModelObj.Email} </strong>" + "<br>" +
                                $"Password: <strong> {userAddDtoObj.Password} </strong>" +
                                $"<p> Your Role ID as you've picked is : {userModelObj.RoleID} !</p>" + "<br>" +
                                "<p>Please refer to the following to confirm your Role </p>" + "<br>" +
                                "1 - Blogger" + "<br>" +
                                "2 - User";

            // SendEmailAsync, is inbuilt into IMailService
            await _mailService.SendEmailAsync(mailModel);

            return Ok(new
            {
                StatusCode = 200,
                Message = "Success - User added.",
                Result = userModelObj,
            });
        }

        [HttpPost("add/admin")]
        [AllowAnonymous]

        // FromBody means it will come from UI
        public async Task<ActionResult<UserAddDto>> AddDepartment([FromBody] UserAddDto adminUserAddDtoObj)
        {
            if (adminUserAddDtoObj == null)
            {
                return BadRequest();
            }


            var userModelObj = _mapper.Map<UserModel>(adminUserAddDtoObj);
            userModelObj.RoleID = 3;
            userModelObj.JoinedDate = DateTime.Now;
            // static class, to call it, we need to call it by calling the class Name.
            // of course, to use this class, we have to import it.
            EncDescPassword.CreateHashPassword(adminUserAddDtoObj.Password, out byte[] passwordHash, out byte[] passwordSalt);
            // passwordHash and passwordSalt, are returned by this CreateHashpassword. 
            userModelObj.PasswordHash = passwordHash;
            userModelObj.PasswordSalt = passwordSalt;
            await _context.UserModels.AddAsync(userModelObj); // Adding record to db
            await _context.SaveChangesAsync(); // saving the db

            return Ok(userModelObj);
        }

        // PUT: api/User/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("update")]
        public async Task<ActionResult<UserEditDto>> UpdateUser()
        {
            IFormFileCollection req = Request.Form.Files;
            var files = req;
            var userDtoString = Request.Form["UserDetails"];
            // remember, when we send it over from front end, data is in a string
            var userEditDtoObj = JsonConvert.DeserializeObject<UserEditDto>(userDtoString);
            // need www.root folder.
            // wwwroot/UserImages/<whatever your .jpg or png name is>
            var uploads = Path.Combine(_env.WebRootPath, "UserImages");
            if (userEditDtoObj == null)
            {
                return BadRequest(new
                {
                    StatusCode = 400,
                    Message = "Please input data",

                });
            }

            // if Id exists in the database.
            // check if Id Has NOT Been tracked by anywhere else. 
            // AsNoTracking -> won't track the same instance of the same Entity . 
            var isUserExist = await _context.UserModels.AsNoTracking().FirstOrDefaultAsync(a => a.UserID == userEditDtoObj.UserID);

            if (isUserExist == null)
            {
                return NotFound(new
                {
                    StatusCode = 404,
                    Message = "No such User found",

                });

            }

            // if there are multiple images, we can use this. in future
            foreach (var file in files)
            {
                if (file.Length > 0)
                {
                    // if directory doesn't exist, we create a new directory.
                    if (!Directory.Exists(uploads))
                    {
                        Directory.CreateDirectory(uploads);
                    }
                    var urls = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT")?.Split(";");

                    var filepath = Path.Combine(uploads, file.FileName);
                    // a filestream class helps copy our stream data into our folder.
                    using (var fileStream = new FileStream(filepath, FileMode.Create))
                    {
                        file.CopyTo(fileStream);
                    }
                    userEditDtoObj.ProfileImgUrl = $"UserImages/{file.FileName}";
                }
             
            }

            

            var userModelObj = _mapper.Map<UserModel>(userEditDtoObj);
            if (userModelObj.ProfileImgUrl == null)
            {
                userModelObj.ProfileImgUrl = isUserExist.ProfileImgUrl;
            }
            userModelObj.JoinedDate = isUserExist.JoinedDate;
            userModelObj.PasswordHash = isUserExist.PasswordHash;
            userModelObj.PasswordSalt = isUserExist.PasswordSalt;

            _context.Entry(userModelObj).State = EntityState.Modified; // updates 
            await _context.SaveChangesAsync();

            return Ok(new
            {
                StatusCode = 200,
                Message = "User details Updated",
                Result = userModelObj
            });
        }


        // DELETE: api/User/5
        [HttpDelete("{id}")]
        public async Task<ActionResult<UserModel>> DeleteUser(int id)
        {
            var user = await _context.UserModels.FindAsync(id);
            if (user == null)
            {
                return NotFound(new
                {
                    StatusCode = 404,
                    Message = "User NOT found"
                });
            }

            _context.UserModels.Remove(user);
            await _context.SaveChangesAsync();

            return Ok(new
            {
                StatusCode = 200,
                Message = "User Record has been deleted"
            });
        }
    }
}

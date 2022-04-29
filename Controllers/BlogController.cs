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
using Newtonsoft.Json;
using System.IO;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Authorization;

namespace CapstoneProjectBlog.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class BlogController : ControllerBase
    {
        private readonly BlogContext _context;
        private readonly IMapper _mapper;
        private IWebHostEnvironment _env;

        public BlogController(BlogContext context, IMapper mapper,IWebHostEnvironment env)
        {
            _context = context;
            _mapper = mapper;
            _env = env;
        }

        // GET: api/Blog
        [HttpGet]
        public async Task<ActionResult<BlogModel>> GetAllBlog()
        {
            var blogList = await _context.BlogModels.OrderByDescending(c => c.PublishDate).Include(a=>a.User).Include(a=>a.Comments).ThenInclude(b=>b.User).ToListAsync();

            if (blogList.Count < 1)
            {
                return Ok(new
                {
                    StatusCode = 200,
                    Message = "Success - No Blog Recorded to date.",

                });
            }

            return Ok(new
            {
                StatusCode = 200,
                Message = "Success",
                Result = blogList,
            });
        }

        // GET: api/Blog/5
        [HttpGet("{id}")]
        public async Task<ActionResult<BlogModel>> GetIndividualBlog(int id)
        {
            var blog = await _context.BlogModels.Include(a => a.User).Include(a => a.Comments).ThenInclude(a => a.User).AsNoTracking()
                .FirstOrDefaultAsync(a => a.BlogID == id);

            if (blog == null)
            {
                return NotFound(new
                {
                    StatusCode = 404,
                    Message = "No Blog with given ID Found.",

                });
            }

            return Ok(new
            {
                StatusCode = 200,
                Message = "Success",
                Result = blog,

            });
        }

        [HttpGet("blogger/{id}")]
        public async Task<ActionResult<BlogModel>> GetBlogByBlogger(int id)
        {
            var bloggerBlog = from blog in _context.BlogModels where blog.UserID == id orderby blog.PublishDate descending select blog;

            if (bloggerBlog == null)
            {
                return NotFound(new
                {
                    StatusCode = 404,
                    Message = "No Blog with given ID Found.",

                });
            }

            return Ok(new
            {
                StatusCode = 200,
                Message = "Success",
                Result = bloggerBlog,

            });
        }

        // POST: api/Blog
        [HttpPost("add")]

        public async Task<ActionResult<BlogAddDto>> AddBlog()
        {
            IFormFileCollection req = Request.Form.Files;
            var files = req;
            var blogDtoString = Request.Form["BlogDetails"];
            // remember, when we send it over from front end, data is in a string
            var blogAddDtoObj = JsonConvert.DeserializeObject<BlogAddDto>(blogDtoString);
            // need www.root folder.
            // wwwroot/UserImages/<whatever your .jpg or png name is>
            var uploads = Path.Combine(_env.WebRootPath, "BlogImages");
            if (blogAddDtoObj == null)
            {
                return BadRequest(new
                {
                    StatusCode = 400,
                    Message = "Please input data",

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
                    blogAddDtoObj.BlogImg = $"BlogImages/{file.FileName}";
                }
            }

            // mapper has a method called Map. 
            // .Map<(desired data type)>(Source);
            var BlogModelObj = _mapper.Map<BlogModel>(blogAddDtoObj);
            BlogModelObj.PublishDate = DateTime.Now;
            await _context.BlogModels.AddAsync(BlogModelObj);
            await _context.SaveChangesAsync();
            return Ok(new
            {
                StatusCode = 200,
                Message = "Success - Blog added.",
                Result = BlogModelObj,
            });
        }

        

        // PUT: api/Blog/5
        [HttpPut("update")]
        public async Task<ActionResult<BlogEditDto>> UpdateBlog()
        {
            IFormFileCollection req = Request.Form.Files;
            var files = req;
            var blogDtoString = Request.Form["BlogDetails"];
            // remember, when we send it over from front end, data is in a string
            var blogEditDtoObj = JsonConvert.DeserializeObject<BlogEditDto>(blogDtoString);
            // need www.root folder.
            // wwwroot/UserImages/<whatever your .jpg or png name is>
            var uploads = Path.Combine(_env.WebRootPath, "BlogImages");
            if (blogEditDtoObj == null)
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
            var isBlogExist = await _context.BlogModels.AsNoTracking().FirstOrDefaultAsync(a => a.BlogID == blogEditDtoObj.BlogID);

            if (isBlogExist == null)
            {
                return NotFound(new
                {
                    StatusCode = 404,
                    Message = "No such Blog found",

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
                    blogEditDtoObj.BlogImg = $"BlogImages/{file.FileName}";
                }
            }



            var blogModelObj = _mapper.Map<BlogModel>(blogEditDtoObj);
            if (blogModelObj.BlogImg == null)
            {
                blogModelObj.BlogImg = isBlogExist.BlogImg;
            }
            blogModelObj.PublishDate = isBlogExist.PublishDate;
            blogModelObj.UpdatedDate = DateTime.Now;
            _context.Entry(blogModelObj).State = EntityState.Modified; // updates 
            await _context.SaveChangesAsync();

            return Ok(new
            {
                StatusCode = 200,
                Message = "Blog details Updated",
                Result = blogModelObj
            });
        }


        // DELETE: api/Blog/5
        [HttpDelete("{id}")]
        public async Task<ActionResult<BlogModel>> DeleteBlog(int id)
        {
            var blog = await _context.BlogModels.FindAsync(id);
            if (blog == null)
            {
                return NotFound(new
                {
                    StatusCode = 404,
                    Message = "Blog NOT found"
                });
            }

            _context.BlogModels.Remove(blog);
            await _context.SaveChangesAsync();

            return Ok(new
            {
                StatusCode = 200,
                Message = "Blog Record has been deleted"
            });
        }
    }
}

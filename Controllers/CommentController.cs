using AutoMapper;
using CapstoneProjectBlog.Data.Context;
using CapstoneProjectBlog.Dtos;
using CapstoneProjectBlog.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace CapstoneProjectBlog.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class CommentController : ControllerBase
    {
        private readonly BlogContext _context;
        private readonly IMapper _mapper;

        public CommentController(BlogContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        // GET: api/Comment
        [HttpGet]
        public async Task<ActionResult<CommentModel>> GetAllComment()
        {
            var commentList = await _context.CommentModels.Include(a=>a.User).ToListAsync();

            if (commentList.Count < 1)
            {
                return Ok(new
                {
                    StatusCode = 200,
                    Message = "Success - No Comment Recorded to date.",

                });
            }

            return Ok(new
            {
                StatusCode = 200,
                Message = "Success",
                Result = commentList,
            });
        }

        // GET: api/Comment/5
        [HttpGet("{id}")]
        public async Task<ActionResult<CommentModel>> GetIndividualComment(int id)
        {
            var comment = await _context.CommentModels.Include(a => a.User).OrderByDescending(c => c.CreatedDate).AsNoTracking().FirstOrDefaultAsync(a => a.CommentID == id);

            if (comment == null)
            {
                return NotFound(new
                {
                    StatusCode = 404,
                    Message = "No Comment with given ID Found.",

                });
            }

            return Ok(new
            {
                StatusCode = 200,
                Message = "Success",
                Result = comment,

            });
        }

        // GET BY PARTICULAR USER
        [HttpGet("user/{id}")]
  
        
        public async Task<ActionResult<CommentModel>> GetCommentFromIndividualUser(int id)
        {
            var comment = await _context.CommentModels.Include(a => a.User).Include(b=>b.Blog).Where(a => a.UserID == id).ToListAsync();
            // .Find(a => a.UserID == id);

            if (comment == null)
            {
                return Ok(new
                {
                    StatusCode = 200,
                    Message = "User has not posted comments on any blog yet.",
                });
            }

            return Ok(new
            {
                StatusCode = 200,
                Message = "Success",
                Result = comment,

            });
        }

        // POST: api/Comment
        [HttpPost("add")]

        public async Task<ActionResult<CommentAddDto>> AddComment([FromBody] CommentAddDto commentAddDtoObj)
        {
            if (commentAddDtoObj == null)
            {
                return BadRequest(new
                {
                    StatusCode = 400,
                    Message = "Please input data",

                });
            }

            // mapper has a method called Map. 
            // .Map<(desired data type)>(Source);
            var commentModelObj = _mapper.Map<CommentModel>(commentAddDtoObj);
            commentModelObj.CreatedDate = DateTime.Now;
            await _context.CommentModels.AddAsync(commentModelObj);
            await _context.SaveChangesAsync();
            return Ok(new
            {
                StatusCode = 200,
                Message = "Success - Comment added.",
                Result = commentModelObj,
            });
        }

        // PUT: api/Comment/5
        [HttpPut("update")]
        public async Task<ActionResult<CommentEditDto>> UpdateComment([FromBody] CommentEditDto commentEditDtoObj)
        {
            if (commentEditDtoObj == null)
            {
                return BadRequest(new
                {
                    Status = 400,
                    Message = "Please send data to update."
                });
            }

            // if Id exists in the database.
            // check if Id Has NOT Been tracked by anywhere else. 
            // AsNoTracking -> won't track the same instance of the same Entity . 
            var isCommentExist = await _context.CommentModels.AsNoTracking().FirstOrDefaultAsync(a => a.CommentID == commentEditDtoObj.CommentID);

            if (isCommentExist == null)
            {
                return NotFound(new
                {
                    StatusCode = 404,
                    Message = "No such Comment found",

                });

            }

            var commentModelObj = _mapper.Map<CommentModel>(commentEditDtoObj);
            commentModelObj.CreatedDate = isCommentExist.CreatedDate;
            commentModelObj.UpdatedDate = DateTime.Now;

            _context.Entry(commentModelObj).State = EntityState.Modified; // updates 
            await _context.SaveChangesAsync();

            return Ok(new
            {
                StatusCode = 200,
                Message = "Comment details Updated",
                Result = commentModelObj
            });
        }

        // DELETE: api/Comment/5
        [HttpDelete("{id}")]
        public async Task<ActionResult<CommentModel>> DeleteComment(int id)
        {
            var comment = await _context.CommentModels.FindAsync(id);
            if (comment == null)
            {
                return NotFound(new
                {
                    StatusCode = 404,
                    Message = "Comment NOT found"
                });
            }

            _context.CommentModels.Remove(comment);
            await _context.SaveChangesAsync();

            return Ok(new
            {
                StatusCode = 200,
                Message = "Comment Record has been deleted"
            });
        }
    }
}

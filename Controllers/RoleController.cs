using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using CapstoneProjectBlog.Data.Context;
using CapstoneProjectBlog.Models;
using CapstoneProjectBlog.Dtos;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;

namespace CapstoneProjectBlog.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class RoleController : ControllerBase
    {
        private readonly BlogContext _context;
        private readonly IMapper _mapper;

        public RoleController(BlogContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        // GET: api/Role
        [HttpGet]
        public async Task<ActionResult<RoleModel>> GetAllRoles()
        {
            var RoleList = await _context.RoleModels.Include(a => a.User).ToListAsync();

            if (RoleList.Count < 1)
            {
                return Ok(new
                {
                    StatusCode = 200,
                    Message = "Success - No Roles Recorded.",

                });
            }

            return Ok(new
            {
                StatusCode = 200,
                Message = "Success",
                Result = RoleList,
            });
        }

        // GET: api/Role/5
        [HttpGet("{id}")]
        public async Task<ActionResult<RoleModel>> GetIndividualRole(int id)
        {
            var role = await _context.RoleModels.Include(a => a.User).AsNoTracking().FirstOrDefaultAsync(a => a.RoleID == id);

            if (role == null)
            {
                return NotFound(new
                {
                    StatusCode = 404,
                    Message = "No Role with such ID Found.",

                });
            }

            return Ok(new
            {
                StatusCode = 200,
                Message = "Success",
                Result = role,

            });
        }

        // POST: api/Role

        [HttpPost]
        public async Task<ActionResult<RoleAddDto>> AddRole([FromBody] RoleAddDto roleAddDtoObj)
        {
            if (roleAddDtoObj == null)
            {
                return BadRequest(new
                {
                    StatusCode = 400,
                    Message = "Please input data",

                });
            }

            // mapper has a method called Map. 
            // .Map<(desired data type)>(Source);
            var RoleModelObj = _mapper.Map<RoleModel>(roleAddDtoObj);
            await _context.RoleModels.AddAsync(RoleModelObj);
            await _context.SaveChangesAsync();
            return Ok(new
            {
                StatusCode = 200,
                Message = "Success - Role added.",
                Result = RoleModelObj,
            });
        }

        // PUT: api/Role/5

        [HttpPut("update")]
        public async Task<ActionResult<RoleEditDto>> UpdateRole([FromBody] RoleEditDto roleEditDtoObj)
        {
            if (roleEditDtoObj == null)
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
            var isRoleExist = await _context.RoleModels.AsNoTracking().FirstOrDefaultAsync(a => a.RoleID == roleEditDtoObj.RoleID);

            if (isRoleExist == null)
            {
                return NotFound(new
                {
                    StatusCode = 404,
                    Message = "No such Role found",

                });

            }

            var roleModelObj = _mapper.Map<RoleModel>(roleEditDtoObj);

            _context.Entry(roleModelObj).State = EntityState.Modified; // updates 
            await _context.SaveChangesAsync();

            return Ok(new
            {
                StatusCode = 200,
                Message = "Role details Updated",
                Result = roleModelObj
            });
        }

        

        // DELETE: api/Role/5
        [HttpDelete("{id}")]
        public async Task<ActionResult<RoleModel>> DeleteRole(int id)
        {
            var deleteRole = await _context.RoleModels.FindAsync(id);
            if (deleteRole == null)
            {
                return NotFound(new
                {
                    StatusCode = 404,
                    Message = "Role NOT found"
                });
            }

            _context.RoleModels.Remove(deleteRole);
            await _context.SaveChangesAsync();

            return Ok(new
            {
                StatusCode = 200,
                Message = "Role Record has been deleted"
            });
        }

        
    }
}

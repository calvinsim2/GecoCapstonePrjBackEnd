using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace CapstoneProjectBlog.Models
{
    public class RoleModel
    {
        [Key]
        public int RoleID { get; set; }

        public string RoleName { get; set; }

        public ICollection<UserModel> User { get; set; }
    }
}

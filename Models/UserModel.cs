using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace CapstoneProjectBlog.Models
{
    public class UserModel
    {
        [Key]
        public int UserID { get; set; }

        public string Email { get; set; }
        public string FullName { get; set; }

        public DateTime JoinedDate { get; set; }

        public string? ProfileImgUrl { get; set; }
        public byte [] PasswordHash { get; set; }
        public byte [] PasswordSalt { get; set; }

        // navigation property
        public int RoleID { get; set; }

        public RoleModel Role { get; set; }

        public ICollection<CommentModel> Comments { get; set; }

        public ICollection<BlogModel> Blogs { get; set; }
    }
}

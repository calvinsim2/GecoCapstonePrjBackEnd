using System;
using System.ComponentModel.DataAnnotations;

namespace CapstoneProjectBlog.Models
{
    public class CommentModel
    {
        [Key]
        public int CommentID { get; set; }

        public string Comment { get; set; }
        public DateTime CreatedDate { get; set; }

        public DateTime? UpdatedDate { get; set; }
        // navigation properties
        public int? UserID { get; set; }

        public UserModel User { get; set; }
        public int BlogID { get; set; }

        public BlogModel Blog { get; set; }
    }
}

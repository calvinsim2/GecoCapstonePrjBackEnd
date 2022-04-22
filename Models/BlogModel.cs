using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace CapstoneProjectBlog.Models
{
    public class BlogModel
    {
        [Key]
        public int BlogID { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }

        public string Content { get; set; }
        public string BlogImg { get; set; }
        public DateTime PublishDate { get; set; }

        public DateTime? UpdatedDate { get; set; }
        public bool IsVisible  { get; set; }

        // navigation property
        public int UserID { get; set; }

        public UserModel User { get; set; }

        public ICollection<CommentModel> Comments { get; set; }
    }
}

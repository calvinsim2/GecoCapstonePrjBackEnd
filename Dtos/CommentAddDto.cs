using System;

namespace CapstoneProjectBlog.Dtos
{
    public class CommentAddDto
    {
        public string Comment { get; set; }

        // navigation property
        public int UserID { get; set; }

        public int BlogID { get; set; }
    }
}

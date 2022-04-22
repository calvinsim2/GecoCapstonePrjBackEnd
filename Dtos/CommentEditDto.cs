using System;

namespace CapstoneProjectBlog.Dtos
{
    public class CommentEditDto
    {
        public int CommentID { get; set; }

        public string Comment { get; set; }
        // navigation property
        public int UserID { get; set; }

        public int BlogID { get; set; }
    }
}

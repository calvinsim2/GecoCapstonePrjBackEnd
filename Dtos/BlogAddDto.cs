using System;

namespace CapstoneProjectBlog.Dtos
{
    public class BlogAddDto
    {
        public string Title { get; set; }
        public string Description { get; set; }
        public string Content { get; set; }
        public string BlogImg { get; set; }
        public bool IsVisible { get; set; }

        // navigation property
        public int UserID { get; set; }


    }
}

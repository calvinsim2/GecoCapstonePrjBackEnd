namespace CapstoneProjectBlog.Dtos
{
    public class UserEditDto
    {
        public int UserID { get; set; }

        public string Email { get; set; }

        public string FullName { get; set; }

        public string? ProfileImgUrl { get; set; }

        // navigation property
        public int RoleID { get; set; }
    }
}

namespace CapstoneProjectBlog.Dtos
{
    public class UserAddDto
    {
        public string Email { get; set; }
        public string FullName { get; set; }
        public string Password { get; set; }
        public string? ProfileImgUrl { get; set; }

        // navigation property
        public int RoleID { get; set; }
    }
}

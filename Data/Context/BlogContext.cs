using CapstoneProjectBlog.Models;
using Microsoft.EntityFrameworkCore;

namespace CapstoneProjectBlog.Data.Context
{
    public class BlogContext : DbContext
    {
        public BlogContext(DbContextOptions<BlogContext> options) : base(options)
        {

        }

        // import the Models, which we created, and we create a DBSet for it.
        public DbSet<UserModel> UserModels { get; set; }
        public DbSet<RoleModel> RoleModels { get; set; }
        public DbSet<CommentModel> CommentModels { get; set; }
        public DbSet<BlogModel> BlogModels { get; set; }

        // ModelBuilder - it helps us to build the tables. It takes our data and helps us to shape it into a 
        // table format. It does this by using his Entity Method. 

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<UserModel>().ToTable("tbl_User")
                                            .HasOne(a => a.Role)
                                            .WithMany(a => a.User)
                                            .HasForeignKey(a => a.RoleID)
                                            .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<RoleModel>().ToTable("tbl_Role");

            modelBuilder.Entity<CommentModel>(entity =>
            {
                entity.ToTable("tbl_Comments").HasOne(a => a.Blog)
                                              .WithMany(a => a.Comments)
                                              .HasForeignKey(a => a.BlogID)
                                              .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(a => a.User)
                      .WithMany(a => a.Comments)
                      .HasForeignKey(a => a.UserID)
                      .OnDelete(DeleteBehavior.NoAction);

                

            });



            modelBuilder.Entity<BlogModel>()
                .ToTable("tbl_Blog")
                .HasOne(a => a.User)
                .WithMany(a => a.Blogs)
                .HasForeignKey(a => a.UserID)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}

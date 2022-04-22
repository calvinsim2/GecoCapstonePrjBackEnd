using CapstoneProjectBlog.Models;
using System.Threading.Tasks;

namespace CapstoneProjectBlog.Services
{
    public interface IMailService
    {
        Task SendEmailAsync(MailModel mailModel);
    }
}

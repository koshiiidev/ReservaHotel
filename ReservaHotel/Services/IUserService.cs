using ReservaHotel.Areas.Identity.Data;
using Microsoft.AspNetCore.Identity;

namespace ReservaHotel.Services
{
    public interface IUserService
    {

        Task<IdentityResult> RegisterUserAsync(Usuario user, string password);
    }

    public class UserService : IUserService 
    {
        private readonly UserManager<Usuario> _userManager;

        public UserService(UserManager<Usuario> userManager)
        {
            _userManager = userManager;
        }

        public async Task<IdentityResult> RegisterUserAsync(Usuario user, string password) 
        {
            var result = await _userManager.CreateAsync(user, password);
            if (result.Succeeded) 
            {
                await _userManager.AddToRoleAsync(user, "Cliente");
            }

            return result;
        }
    }
}

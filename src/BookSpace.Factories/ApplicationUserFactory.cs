using BookSpace.Factories.DTO;
using BookSpace.Models;

namespace BookSpace.Factories
{
    public class ApplicationUserFactory : IFactory<ApplicationUser, UserCreateDto>
    {
        public ApplicationUser Create(UserCreateDto model)
        {
            return new ApplicationUser
            {
                UserName = model.Username,
                Email = model.Email
            };
        }
    }
}

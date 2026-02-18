using Happenings.Model.Entities;
using Happenings.Model.Requests;
using Happenings.Model.Responses;
using Happenings.Services.Database;
using Happenings.Services.Interfaces;

namespace Happenings.Services.Services
{
    public class UserService : IUserService
    {
        private readonly HappeningsContext _context;

        public UserService(HappeningsContext context)
        {
            _context = context;
        }

        public List<UserDto> Get()
        {
            return _context.Users
                .Select(u => new UserDto
                {
                    Id = u.Id,
                    Username = u.Username,
                    Email = u.Email,
                    CreatedAt = u.CreatedAt
                })
                .ToList();
        }

        public UserDto GetById(int id)
        {
            var user = _context.Users.Find(id);
            if (user == null)
                throw new Exception("User not found");

            return new UserDto
            {
                Id = user.Id,
                Username = user.Username,
                Email = user.Email,
                CreatedAt = user.CreatedAt
            };
        }

        public UserDto Insert(UserInsertRequest request)
        {
            var entity = new User
            {
                Username = request.Username,
                Email = request.Email,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password)
            };

            _context.Users.Add(entity);
            _context.SaveChanges();

            return GetById(entity.Id);
        }

        public UserDto Update(int id, UserUpdateRequest request)
        {
            var entity = _context.Users.Find(id);
            if (entity == null)
                throw new Exception("User not found");

            entity.Username = request.Username;
            entity.Email = request.Email;

            _context.SaveChanges();
            return GetById(id);
        }

        public void Delete(int id)
        {
            var entity = _context.Users.Find(id);
            if (entity == null)
                throw new Exception("User not found");

            _context.Users.Remove(entity);
            _context.SaveChanges();
        }
    }
}

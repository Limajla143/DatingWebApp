using System;
using System.Linq;
using System.Threading.Tasks;
using DatingApp.API.Models;
using Microsoft.EntityFrameworkCore;


namespace DatingApp.API.Data
{
    public class AuthRepository : IAuthRepository
    {
        private readonly DataContext context;

        public AuthRepository(DataContext _context)
        {
            context = _context;
        }
        public async Task<bool> IsUserExist(string usermame)
        {
            if (await context.Users.Where(x => x.UserName == usermame).AnyAsync())
            {
                return true;
            }
            return false;
        }

        public async Task<User> Login(string username, string password)
        {
            var user = await context.Users.Include(p => p.Photos).Where(u => u.UserName == username).FirstOrDefaultAsync();

            if (user == null)
                return null;

            // if (!VerifyPasswordHash(password, user.PasswordHash, user.PasswordSalt))
            // {
            //     return null;
            // }

            return user;
        }

        private bool VerifyPasswordHash(string password, byte[] passwordHash, byte[] passwordSalt)
        {
            using (var hmac = new System.Security.Cryptography.HMACSHA512(passwordSalt))
            {
                var computedHash = hmac.ComputeHash(System.Text.Encoding.UTF8.GetBytes(password));

                for (int i = 0; i < computedHash.Length; i++)
                {
                    if (computedHash[i] != passwordHash[i]) return false;
                }

                return true;
            }
        }

        public async Task<User> Register(User user, string password)
        {
            byte[] passwordHash, passwordSalt;
            CreatePasswordHash(out passwordHash, out passwordSalt, password);

            // user.PasswordHash = passwordHash;
            // user.PasswordSalt = passwordSalt;

            await context.Users.AddAsync(user);
            await context.SaveChangesAsync();

            return user;
        }

        private void CreatePasswordHash(out byte[] passwordHash, out byte[] passwordSalt, string password)
        {
            using (var hmac = new System.Security.Cryptography.HMACSHA512())
            {
                passwordSalt = hmac.Key;
                passwordHash = hmac.ComputeHash(System.Text.Encoding.UTF8.GetBytes(password));
            }
        }
    }
}
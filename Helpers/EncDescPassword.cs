using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace CapstoneProjectBlog.Helpers
{
    // static class is a class that contains static variables, static methods.
    // we cannot create objects of a static class. 
    // so, how do we use it? 
    // to use static class, we need to call the class Name. etc. EncDESCPassword.CreateHashPassword
    public static class EncDescPassword
    {
        // initially, out args are empty, but after method execution, it will act as a return which it contains the value we assign.
        public static void CreateHashPassword(string password, out byte[] passwordHash, out byte[] passwordSalt)
        {
            // HMACSHA512 - Encryption model created by .Net team.
            // we need to import Security.Cryptogaphy to use this.. 
            // in this HMACSHA512 class, there is a prop called Key.
            // ComputeHash is a method in HMACSHA512 class. 
            using (var hmac = new HMACSHA512())
            {
                passwordSalt = hmac.Key;
                // hashing our password
                // Encoding.UTF8.GetBytes --> convert a string into bytes
                passwordHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(password));
            }

        }

        // if password match, return true, else return false. 
        public static bool VerifyHashPassword(string password, byte[] passwordHash, byte[] passwordSalt)
        {
            // to verify, we need to pass the passwordSalt
            // passwordSalt has our Key, which returns a computed hash.
            using (var hmac = new HMACSHA512(passwordSalt))
            {
                // if computedHash is equal to passwordHash, it means the password is correct. 
                var computedHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(password));
                return computedHash.SequenceEqual(passwordHash);
            }
        }
    }
}

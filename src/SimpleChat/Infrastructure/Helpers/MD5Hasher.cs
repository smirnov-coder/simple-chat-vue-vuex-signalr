using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace SimpleChat.Infrastructure.Helpers
{
    /// <inheritdoc cref="IMD5Hasher"/>
    public class MD5Hasher : IMD5Hasher
    {
        private IGuard _guard;

        public MD5Hasher() : this(null)
        {
        }

        public MD5Hasher(IGuard guard) => _guard = guard ?? new Guard();

        public string ComputeHash(string source)
        {
            _guard.EnsureStringParamIsNotNullOrEmpty(source, nameof(source));

            using (var md5 = MD5.Create())
            {
                byte[] bytes = md5.ComputeHash(Encoding.UTF8.GetBytes(source));
                StringBuilder builder = new StringBuilder();
                // Loop through each byte of the hashed data and format each one as a hexadecimal string.
                for (int i = 0; i < bytes.Length; i++)
                {
                    builder.Append(bytes[i].ToString("x2"));
                }
                string result = builder.ToString();
                return result;
            }
        }
    }
}

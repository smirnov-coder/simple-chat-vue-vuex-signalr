using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace SimpleChat.Infrastructure.Extensions
{
    public static class HashAlgorithmExtensions
    {
        public static string ComputeHash(this HashAlgorithm md5Hasher, string source)
        {
            byte[] bytes = md5Hasher.ComputeHash(Encoding.UTF8.GetBytes(source));
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

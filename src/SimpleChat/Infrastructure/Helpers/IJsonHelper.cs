using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SimpleChat.Infrastructure.Helpers
{
    public interface IJsonHelper
    {
        JObject Parse(string json);

        string SerializeObject<T>(T value);

        T DeserializeObject<T>(string json);
    }
}

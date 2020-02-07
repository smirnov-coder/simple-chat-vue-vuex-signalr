using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace SimpleChat.Infrastructure.Helpers
{
    public class JsonHelper : IJsonHelper
    {
        private IGuard _guard;

        public JsonHelper() : this(null)
        {
        }

        public JsonHelper(IGuard guard) => _guard = guard ?? new Guard();

        public T DeserializeObject<T>(string json)
        {
            _guard.EnsureStringParamIsNotNullOrEmpty(json, nameof(json));
            return JsonConvert.DeserializeObject<T>(json);
        }

        public JObject Parse(string json)
        {
            _guard.EnsureStringParamIsNotNullOrEmpty(json, nameof(json));
            return JObject.Parse(json);
        }

        public string SerializeObject<T>(T value)
        {
            _guard.EnsureObjectParamIsNotNull(value, nameof(value));
            return JsonConvert.SerializeObject(value);
        }


    }
}

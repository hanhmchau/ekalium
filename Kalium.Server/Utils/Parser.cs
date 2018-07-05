using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Kalium.Server.Utils
{
    public class Parser
    {
        private readonly JObject _obj;

        public Parser(string json)
        {
            this._obj = Parse(json);
        }
        private static JObject Parse(string json)
        {
            object dec = JsonConvert.DeserializeObject(json);
            return JObject.Parse(dec.ToString());
        }

        public string AsString(string property) => _obj[property].ToString();
        public int AsInt(string property) => int.Parse(_obj[property].ToString());
        public bool AsBool(string property) => bool.Parse(_obj[property].ToString());
        public long AsLong(string property) => long.Parse(_obj[property].ToString());
        public double AsDouble(string property) => double.Parse(_obj[property].ToString());
        public T AsObject<T>(string property) => JsonConvert.DeserializeObject<T>(_obj[property].ToString());
    }
}

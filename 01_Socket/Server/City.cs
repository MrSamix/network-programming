using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Server
{
    public class City
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int ZipCode { get; set; }

        public City() { }

        public byte[] ToBytesArray()
        {
            string json = JsonConvert.SerializeObject(this);
            byte[] data = Encoding.Unicode.GetBytes(json);
            return data;
        }
    }
}

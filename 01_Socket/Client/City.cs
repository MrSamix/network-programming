using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Client
{
    public class City
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int ZipCode { get; set; }

        public City() { }

        public City(byte[] data)
        {
            string json = Encoding.Unicode.GetString(data);
            City city = JsonConvert.DeserializeObject<City>(json);
            if (city is not null)
            {
                this.Id = city.Id;
                this.Name = city.Name;
                this.ZipCode = city.ZipCode;
            }
        }

    }
}

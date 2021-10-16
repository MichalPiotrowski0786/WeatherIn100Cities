namespace XLE_Task_MichałPiotrowski.Models {
    public class FinalModel {
        //public string Country { get; set; } // from country/cities api
        public string City { get; set; } // from country/cities api
        public float Lat { get; set; }
        public float Lon { get; set; }
        public string Description { get; set; }
        public float Temperature { get; set; }
        public float Pressure { get; set; }
        public float Humidity { get; set; }
        public float Wind { get; set; }

        public FinalModel(
            //string country, 
            string city, 
            float lat, 
            float lon, 
            string description, 
            float temperature, 
            float pressure, 
            float humidity, 
            float wind) {

            //Country = country;
            City = city;
            Lat = lat;
            Lon = lon;
            Description = description;
            Temperature = temperature;
            Pressure = pressure;
            Humidity = humidity;
            Wind = wind;
        }
    }
}

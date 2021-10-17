namespace XLE_Task_MichałPiotrowski.Models {
    public class FinalModel {
        public string City { get; set; }
        public string CountryCode { get; set; }
        public float Lat { get; set; }
        public float Lon { get; set; }
        public string Description { get; set; }
        public float Temperature { get; set; }
        public float Pressure { get; set; }
        public float Humidity { get; set; }
        public float Wind { get; set; }

        public FinalModel(
            string city,
            string countrycode,
            float lat, 
            float lon, 
            string description, 
            float temperature, 
            float pressure, 
            float humidity, 
            float wind) {

            City = city;
            CountryCode = countrycode;
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

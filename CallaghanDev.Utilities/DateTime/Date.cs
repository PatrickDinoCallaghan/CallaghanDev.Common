using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CallaghanDev.Utilities.DateTimeTools
{
    public class UkHollidayDate
    {
        private HttpClient client = new HttpClient();

        public HollidayData hollidayData;

        public UkHollidayDate()
        {
            // Wait for result. :/
            HollidayData result = HolidayData().GetAwaiter().GetResult();
            hollidayData = result;
        }
        private async Task<HollidayData> HolidayData()
        {
            string url = "";

            HttpResponseMessage response = await client.GetAsync(url);
            response.EnsureSuccessStatusCode();
            string responseBody = await response.Content.ReadAsStringAsync();

            hollidayData = JsonConvert.DeserializeObject<HollidayData>(responseBody);

            return hollidayData;
        }

        public DateTime NextWorkingDay(DateTime date)
        {
            do
            {
                date = date.AddDays(1);
            }
            while (date.DayOfWeek == DayOfWeek.Saturday || date.DayOfWeek == DayOfWeek.Sunday
            || hollidayData.EnglandAndWales.events.Where(r => r.date == date.ToString("yyyy-MM-dd"))?.Count() > 0); // TODO: Find string format

            return date;
        }
    }
    public class Event
    {
        public string title { get; set; }
        public string date { get; set; }
        public string notes { get; set; }
        public bool bunting { get; set; }
    }

    public class EnglandAndWales
    {
        public string division { get; set; }
        public List<Event> events { get; set; }
    }

    public class Scotland
    {
        public string division { get; set; }
        public List<Event> events { get; set; }
    }

    public class NorthernIreland
    {
        public string division { get; set; }
        public List<Event> events { get; set; }
    }

    public class HollidayData
    {
        [JsonProperty(PropertyName = "england-and-wales")]
        public EnglandAndWales EnglandAndWales { get; set; }
        public Scotland scotland { get; set; }
        [JsonProperty(PropertyName = "northern-ireland")]
        public NorthernIreland NorthernIreland { get; set; }
    }
}

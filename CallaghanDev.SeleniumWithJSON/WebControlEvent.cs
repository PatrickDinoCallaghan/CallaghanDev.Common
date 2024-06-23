using CallaghanDev.SeleniumWithJSON.Extensions;
using CallaghanDev.Utilities.ConsoleHelper;
using CallaghanDev.Utilities.Web;
using CallaghanDev.Utilities.Web.Enums;
using CallaghanDev.Utilities.Web.Extensions;
using DocumentFormat.OpenXml.Wordprocessing;
using Newtonsoft.Json;
using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;
using System.Collections;

namespace CallaghanDev.SeleniumWithJSON
{

    public class WebControlEvent
    {        
        [JsonProperty]
        public string Name { get; set; }

        [JsonProperty]
        public string BaseUrl { get; set; }


        [JsonProperty]
        private List<WebControlAction> _actions = new List<WebControlAction>();

        [JsonProperty]
        public WebControlAction ReturnAction { get; set; }

        [JsonIgnore]
        IWebDriver _webDriver;

        public class WebControlTask() { }

        public WebControlAction Add(WebControlAction action = null, bool IsReturn = false)
        {
            WebControlAction act = action ?? new WebControlAction();
            if (IsReturn)
            {
                ReturnAction = act;
            }
            else
            {
                _actions.Add(act);
            }

            return act;
        }

        public void RemoveActionAt(int index)
        {
            if (index < 0 || index >= _actions.Count)
                throw new IndexOutOfRangeException("Index out of range");

            _actions.RemoveAt(index);
        }
        public void Clear()
        {
            _actions.Clear();
        }
        public object Run()
        {
            if (_webDriver == null)
            {
                throw new Exception("Base url not set.");
            }
            _webDriver.Url = _webDriver.ConvertToValidUrl(BaseUrl);
            foreach (var item in _actions)
            {
                item.Run();
            }
            return ReturnAction?.Run();
        }
        public void Build(string EventName, string BaseUrl)
        {
            this.Name = EventName;
            this.BaseUrl = BaseUrl;
            WebControlAction ActionAdded =  Add();
            ActionAdded.SetWebDriver(_webDriver);
            ActionAdded.Run();
            while (cnsl.AskYesNoQuestion("Do you want to add another action?"))
            {
                ActionAdded = Add();
                ActionAdded.SetWebDriver(_webDriver);
                ActionAdded.Run();
            }

            if (cnsl.AskYesNoQuestion("Do you want a return action?"))
            {
                ReturnAction = Add();
                ReturnAction.SetWebDriver(_webDriver);
                Console.WriteLine("-=Returned Object=-");
                Console.WriteLine(ReturnAction.Run().ToString());
            }
            Console.WriteLine("Finished...");
        }
        public void InjectCookies()
        {

        }
        public void SetWebDriver(IWebDriver webDriver)
        {
            _webDriver = webDriver;
            foreach (var item in _actions)
            {
                item.SetWebDriver(_webDriver);
            }

            ReturnAction?.SetWebDriver(_webDriver);

        }
        public void Save(string filePath = null)
        {
            // Determine the file path
            if (string.IsNullOrEmpty(filePath))
            {
                string fileName = "data.json"; // or any default file name you want to use
                filePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, fileName);
            }

            // Serialize the object to JSON
            string json = JsonConvert.SerializeObject(this, Formatting.Indented);

            // Write the JSON data to the file
            File.WriteAllText(filePath, json);

            // Notify the user
            Console.WriteLine($"JSON data has been written to {filePath}");
        }

        public static WebControlEvent Load(string filePath)
        {
            filePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, filePath);
            string json = File.ReadAllText(filePath);
            return JsonConvert.DeserializeObject<WebControlEvent>(json);
        }
    }
}

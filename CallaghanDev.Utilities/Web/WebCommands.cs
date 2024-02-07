using CallaghanDev.Utilities.String;
using Newtonsoft.Json;
using OpenQA.Selenium;
using OpenQA.Selenium.Firefox;
using OpenQA.Selenium.Support.UI;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Xml.Linq;

namespace CallaghanDev.Utilities.Web
{
    /*
    public class WebCommands
    {
        Dictionary<string, WebCommand> WebCommandList = new Dictionary<string, WebCommand>();

        private int pageCount = 0;
        private IWebDriver _driver;
        [JsonIgnore]
        string _base_window_handle;

        public WebCommands(IWebDriver webDriver, string BaseURL, string[] Args)
        {
            _driver = webDriver;
            Init(BaseURL, Args);
        }
        public WebCommands(IWebDriver webDriver, string BaseURL)
        {
            _driver = webDriver;
            Init(BaseURL, null);
        }
        public void Run(string Name)
        {
            if (WebCommandList.ContainsKey(Name))
            {
                WebCommandList[Name].Run();
            }
            else 
            { 
                throw new ArgumentOutOfRangeException($"WebCommand not found :{Name}");
            }
        }
        public void Save(string path)
        {
            // Serialize the dictionary to a JSON string
            string jsonString = JsonConvert.SerializeObject(WebCommandList, Formatting.Indented);

            File.WriteAllText(path, jsonString);
        }
        public void Load(string path)
        {
            // Read the JSON string from the file
            string jsonString = File.ReadAllText(path);

            // Deserialize the JSON string back to a Dictionary
            WebCommandList = JsonConvert.DeserializeObject<Dictionary<string, WebCommand>>(jsonString);

            foreach (WebCommand webcmd in WebCommandList.Values)
            {
                webcmd.SetIWebDriver(_driver);
            }
        }
        public bool Add(string Name, WebCommand webCommand)
        {
            if (WebCommandList.ContainsKey(Name))
            {
                return false;
            }
            else
            {
                WebCommandList.Add(Name, webCommand);
                return false;
            }
        }
        public bool Add(string Name, List<WebTask> WebTasks)
        {
            WebCommand webCommand = new WebCommand(_driver);


            foreach (WebTask item in WebTasks)
            {
                webCommand.Add(item);
            }

            return Add(Name, webCommand);
        }
        public void Init(string BaseUrl, string[] Args)
        {

            bool ImportCookiesExist = File.Exists("cookies.json");

            _driver.Navigate().GoToUrl(BaseUrl);

            _base_window_handle = _driver.CurrentWindowHandle;

            InitCookies(BaseUrl);
            
        }
        private void InitCookies(string BaseUrl)
        {
            WebDriverWait wait = new WebDriverWait(_driver, TimeSpan.FromSeconds(10));
            wait.Until(d => ((IJavaScriptExecutor)d).ExecuteScript("return document.readyState").Equals("complete"));

            bool ImportCookiesExist = File.Exists("cookies.json");
            try
            {

                if (ImportCookiesExist)
                {
                    var cookiesDTOList = JsonConvert.DeserializeObject<List<SeleniumCookieDTO>>(File.ReadAllText("cookies.json"));
                    List<Cookie> cookies = cookiesDTOList.Select(r => r.ToCookie()).ToList();
                    foreach (var cookie in cookies)
                    {
                        if (cookie.Domain.Contains("chat.openai.com"))
                        {
                            //_driver.Manage().Cookies.AddCookie(cookie);
                        }
                    }
                }
                else
                {
                    Console.WriteLine("No cookies found");
                }
                _driver.Navigate().GoToUrl(BaseUrl);
                wait = new WebDriverWait(_driver, TimeSpan.FromSeconds(10));
                wait.Until(d => ((IJavaScriptExecutor)d).ExecuteScript("return document.readyState").Equals("complete"));

                var allCookies = _driver.Manage().Cookies.AllCookies;

                File.WriteAllText("cookies.json", JsonConvert.SerializeObject(allCookies));
            }
            catch (Exception ex)
            {
                Console.WriteLine("Unable to load Cookies. ERROR:" +ex.Message);
            }
        }
        public bool CloseAllWindowsExceptBase()
        {
            IList<string> windowHandles = _driver.WindowHandles;

            foreach (string handle in windowHandles)
            {
                if (handle != _base_window_handle)
                {
                    _driver.SwitchTo().Window(handle);
                    return true;
                }
            }
            return false;
        }
    }

    [JsonObject]
    public class WebCommand
    {
        [JsonIgnore]
        private IWebDriver _driver;
        [JsonProperty]
        public string Name { get; set; }
        [JsonProperty]
        private List<WebTask> WebTasks;
        public WebCommand(IWebDriver webDriver) 
        {
            _driver = webDriver;
            WebTasks = new List<WebTask>();
        }
        public void Run()
        {
            foreach (var command in WebTasks)
            {
                bool sucess = command.Run();
                if (!sucess)
                {
                    Console.WriteLine($"{command.Text} : failed!");
                }
                else
                {
                    Console.WriteLine($"{command.Text} : success!");
                }
            }
        }
        public void Add(WebTask webTask)
        {
            WebTasks.Add(webTask);
        }
        public void Add(WebEventType webEventType, ElementIdentifer element, string text = "")
        {
            if (string.IsNullOrEmpty(text))
            {
                text = string.Empty;
            }

            WebTasks.Add(new WebTask(_driver, element, webEventType, text) );
        }
        public void Clear()
        {
            WebTasks.Clear();
        }
        public void SetIWebDriver(IWebDriver driver)
        {
            _driver = driver;
            foreach (WebTask tsk in WebTasks)
            {
                tsk.SetIWebDriver(driver);
            }
        }
    }

    [JsonObject]
    public class WebTask
    {
        [JsonProperty]
        private ElementIdentifer _Element;
        [JsonProperty]
        private WebEventType _webEventType;
        [JsonIgnore]
        private IWebDriver _driver;
        [JsonIgnore]
        private IWebElementsHandler _webElementsHandler;
        public WebTask(IWebDriver driver, ElementIdentifer ElementIdentification, WebEventType webEventType, string text ="")
        {
            _Element = ElementIdentification;
            _webEventType = webEventType;
            _driver = driver;
            _webElementsHandler = new WebElementsHandler(_driver);
            if (text != null) 
            {
                Text = text;
            }
            else
            {
                Text = string.Empty;
            }
        }
        public WebTask(IWebDriver driver, string ElementIdentification, WebEventType webEventType, string text = "")
        {
            _Element = new ElementIdentifer(ElementIdentification);
            _webEventType = webEventType;
            _driver = driver;
            _webElementsHandler = new WebElementsHandler(_driver);
            if (text != null)
            {
                Text = text;
            }
            else
            {
                Text = string.Empty;
            }
        }
        [JsonProperty]
        public string Text { get; set; } = string.Empty;
        public bool Run()
        {
            if (_Element == null)
                throw new Exception();

            FireEvent fireEvent = GetEvent();

            bool EventFiredSuccess = fireEvent(new object[] { _Element, Text });
            return EventFiredSuccess;
        }
        public delegate bool FireEvent(params object[] args);
        public FireEvent GetEvent()
        {
            switch (_webEventType)
            {
                case WebEventType.ChangeText:
                    return TrySendKeys;
                case WebEventType.Click:
                    return TryClickElement;
                case WebEventType.ReadText:
                    return TryReadinglement;
                default:
                    throw new NotImplementedException();
            }

        }
        private bool TrySendKeys(string[] args)
        {
            if (args.Count() == 1)
            {
                return TrySendKeysElement(args[0], "");
            }
            else if (args.Length == 2)
            {
                return TrySendKeysElement(args[0], args[1]);
            }
            else
            {
                throw new NotImplementedException();
            }

        }
        private bool TryClickElement(string[] args)
        {
            return TryClickElement(args);
        }
        private bool TryReadinglement(string[] args)
        {
            return TryReadinglement(args);
        }
        public void SetIWebDriver(IWebDriver driver)
        {
            _driver = driver;
        }
        private bool TrySendKeysElement(ElementIdentifer InElement, string textToSend)
        {
            try
            {
                IWebElement element = _webElementsHandler.FindTextField(InElement.IdentifyingText);
                if (element != null)
                {
                    element.SendKeys(textToSend);
                    return true; // Element was found and text was sent
                }
            }
            catch (Exception ex) { }

            if (ConsoleHelper.Console.AskYesNoQuestion($"The web element you searched for by the element tag '{InElement.IdentifyingText}' wasn't found, Do you want to select a web element to take its place?"))
            {
                List<IWebElement> Allelements = _webElementsHandler.FindAllElements(InElement.IdentifyingText, ElementType.TextField).ToList();
                int Answer = ConsoleHelper.Console.SelectOptionFromList(Allelements.Select(r => $"TagName:{r.TagName}, ShownText:{r.Text}, Id:{r.GetAttribute("id")}").ToList());
                InElement.IdentifyingText = Allelements[Answer].TagName;
                return TrySendKeysElement(InElement, textToSend);
            }

            return false; // Element was not found
        }
        private bool TryClickElement(ElementIdentifer InElement)
        {
            try
            {
                IWebElement element = _webElementsHandler.FindButton(InElement.IdentifyingText);
                if (element != null)
                {
                    element.Click();
                    return true; // Element was found and text was sent
                }
                else
                {
                    Console.WriteLine($"element {InElement.IdentifyingText} not found");
                }
            }
            catch (Exception ex) { }

            if (ConsoleHelper.Console.AskYesNoQuestion($"The web element you searched for by the element tag '{ElementSearchText}' wasn't found, Do you want to select a web element to take its place?"))
            {
                List<IWebElement> Allelements = _webElementsHandler.FindAllElements(ElementSearchText, ElementType.Button).ToList();
                int Answer = ConsoleHelper.Console.SelectOptionFromList(Allelements.Select(r => $"TagName:{r.TagName}, ShownText:{r.Text}, Id:{r.GetAttribute("id")}").ToList());
                _ElementIdentificationText = Allelements[Answer].TagName;
                return TryClickElement(_ElementIdentificationText);
            }
            return false; // Element was not found
        }
        private bool TryReadinglement(ElementIdentifer InElement)
        {
            try
            {
                IWebElement element = _webElementsHandler.FindTextField(ElementSearchText);
                if (element != null)
                {
                    Console.WriteLine(element.Text);
                    return true; // Element was found and text was sent
                }
                else
                {
                    Console.WriteLine($"element {ElementSearchText} not found");
                }
            }
            catch (Exception ex) { }
            if (ConsoleHelper.Console.AskYesNoQuestion($"The web element you searched for by the element tag '{ElementSearchText}' wasn't found, Do you want to select a web element to take its place?"))
            {
                List<IWebElement> Allelements = _webElementsHandler.FindAllElements(ElementSearchText, ElementType.TextField).ToList();
                int Answer = ConsoleHelper.Console.SelectOptionFromList(Allelements.Select(r => $"TagName:{r.TagName}, ShownText:{r.Text}, Id:{r.GetAttribute("id")}").ToList());
                _ElementIdentificationText = Allelements[Answer].TagName;
                return TryReadinglement(_ElementIdentificationText);
            }
            return false; // Element was not found
        }
    }
    public interface IWebElementsHandler
    {
        public IReadOnlyCollection<IWebElement> FindAllElements(string SearchText, ElementType elementType);
        public IWebElement FindButton(string SearchText);
        public IWebElement FindTextField(string SearchText);
        public IWebElement FindCheckBoxes(string SearchText);
        public IWebElement RadioButtonsBoxes(string SearchText);
        public IWebElement DropdownsBoxes(string SearchText);
    }
    public class WebElementsHandler : IWebElementsHandler
    {
        IWebDriver _driver;
        Similarity _similarity;
        public WebElementsHandler(IWebDriver driver)
        {
            _driver = driver;

            _similarity = new Similarity();
        }
        public IReadOnlyCollection<IWebElement> FindAllElements(string SearchText, ElementType elementType)
        {
            if (string.IsNullOrEmpty(SearchText))
            {
                return GetElements(elementType);
            }
            else
            {
                List<IWebElement> elements = GetElements(elementType).ToList();

                List<Tuple<IWebElement, string>> Elements_ByID = elements.Where(r => IsStale(r) == false).Select(r => new Tuple<IWebElement, string>(r, r.GetAttribute("id"))).ToList();
                List<Tuple<IWebElement, string>> Elements_Text = elements.Where(r => IsStale(r) == false).Select(r => new Tuple<IWebElement, string>(r, r.Text)).ToList();
                List<Tuple<IWebElement, string>> Elements_TagName = elements.Where(r => IsStale(r) == false).Select(r => new Tuple<IWebElement, string>(r, r.TagName)).ToList();

                List<Tuple<IWebElement, string>> filteredItems_ByType = new List<Tuple<IWebElement, string>>();

                filteredItems_ByType.AddRange(Elements_ByID);
                filteredItems_ByType.AddRange(Elements_Text);
                filteredItems_ByType.AddRange(Elements_TagName);

               return filteredItems_ByType.Where(r => IsStale(r.Item1) == false && !string.IsNullOrEmpty( r.Item2) )
                    .OrderByDescending(x => _similarity.JaroWinklerDistance(x.Item2, SearchText)).Select(r => r.Item1).ToList();
            }
        }


        private bool IsStale(IWebElement webElement)
        {
            try
            {
                // Accessing the properties to determine if the element is stale
                var dummy = webElement.Text;
                var dummyTag = webElement.TagName;

                return false;
            }
            catch (StaleElementReferenceException)
            {
                return true;
            }
        }
        public IWebElement FindButton(ElementIdentifer SearchText)
        {
            IReadOnlyCollection<IWebElement> buttons = GetElements(ElementType.Button);

            return FindClose(buttons, SearchText);
        }
        public IWebElement FindTextField(ElementIdentifer SearchText)
        {
            IReadOnlyCollection<IWebElement> textFields = GetElements(ElementType.TextField);


            return FindClose(textFields, SearchText);
        }
        public IWebElement FindCheckBoxes(ElementIdentifer SearchText)
        {
            IReadOnlyCollection<IWebElement> checkBoxes = GetElements(ElementType.CheckBoxes);


            return FindClose(checkBoxes, SearchText);
        }
        public IWebElement RadioButtonsBoxes(ElementIdentifer SearchText)
        {
            IReadOnlyCollection<IWebElement> radioButtons = GetElements(ElementType.CheckBoxes);


            return FindClose(radioButtons, SearchText);
        }
        public IWebElement DropdownsBoxes(ElementIdentifer SearchText)
        {
            IReadOnlyCollection<IWebElement> dropdowns = GetElements(ElementType.ButtonsBoxes);

            return FindClose(dropdowns, SearchText);
        }

        private IReadOnlyCollection<IWebElement> GetElements(ElementType type)
        {
            switch (type)
            {
                case ElementType.Button:
                    return _driver.FindElements(By.CssSelector("button, input[type='button'], input[type='submit']"));
                case ElementType.TextField:
                    return _driver.FindElements(By.CssSelector("input[type='text'], textarea"));
                case ElementType.CheckBoxes:
                    return _driver.FindElements(By.CssSelector("input[type='checkbox']"));
                case ElementType.ButtonsBoxes:
                    return _driver.FindElements(By.CssSelector("input[type='radio']"));
                case ElementType.Dropdowns:
                    return _driver.FindElements(By.TagName("select")); 
                default:
                    return _driver.FindElements(By.XPath("//*"));
            }
        }
        private IWebElement FindClose(IReadOnlyCollection<IWebElement> filteredItems, ElementIdentifer SearchText)
        {

            List<Tuple<IWebElement, string>> Elements_ByID = filteredItems.Where(r => IsStale(r) == false).Select(r => new Tuple<IWebElement, string>(r, r.GetAttribute("Id"))).ToList();
            List<Tuple<IWebElement, string>> Elements_Text = filteredItems.Where(r => IsStale(r) == false).Select(r => new Tuple<IWebElement, string>(r, r.Text)).ToList();
            List<Tuple<IWebElement, string>> Elements_TagName = filteredItems.Where(r => IsStale(r) == false).Select(r => new Tuple<IWebElement, string>(r, r.TagName)).ToList();

            List<Tuple<IWebElement, string>> filteredItems_ByType = new List<Tuple<IWebElement, string>>();

            filteredItems_ByType.AddRange(Elements_ByID);
            filteredItems_ByType.AddRange(Elements_Text);
            filteredItems_ByType.AddRange(Elements_TagName);

            List<Tuple<IWebElement, string>> Found = filteredItems_ByType.Where(r => IsStale(r.Item1) == false).ToList();

            if (Found.Count() > 0)
            {
                Found = Found.OrderByDescending(x => _similarity.JaroWinklerDistance(x.Item2, SearchText)).ToList();

                if (_similarity.JaroWinklerDistance(Found[0].Item1.TagName, SearchText) > 0.95)
                {
                    return Found[0].Item1;
                }
            }
            return null;

        }

        public IWebElement FindButton(string SearchText)
        {
            throw new NotImplementedException();
        }

        public IWebElement FindTextField(string SearchText)
        {
            throw new NotImplementedException();
        }

        public IWebElement FindCheckBoxes(string SearchText)
        {
            throw new NotImplementedException();
        }

        public IWebElement RadioButtonsBoxes(string SearchText)
        {
            throw new NotImplementedException();
        }

        public IWebElement DropdownsBoxes(string SearchText)
        {
            throw new NotImplementedException();
        }
    }

    public enum ElementType
    {
        All,
        Button,
        TextField,
        CheckBoxes,
        ButtonsBoxes,
        Dropdowns
    }
    public enum WebEventType
    {
        None,
        ChangeText,
        Click,
        ReadText,
    }
    public class ElementIdentifer
    {
        public ElementIdentifer(string tagName, string shownText, string id)
        {
            _TagName = tagName;
            _ShownText = shownText;
            _Id = id;
        }

        public ElementIdentifer(string identifyingText)
        {
            IdentifyingText = identifyingText;
        }


        [JsonProperty]
        private string _TagName;
        [JsonProperty]
        private string _ShownText;
        [JsonProperty]
        private string _Id;
        [JsonProperty]
        private string _IdentifyingText;

        [JsonIgnore]
        public string IdentifyingText 
        { 
            get 
            {
                if (_TagName == string.Empty && _ShownText == string.Empty && _Id == string.Empty)
                {
                    return _IdentifyingText;
                }
                else
                {
                    throw new Exception("A ");
                }
            } 
            set 
            { 
                _IdentifyingText = value; 
                _TagName = string.Empty; 
                _ShownText = string.Empty;
                _Id = string.Empty; 
            } 
        }
        [JsonIgnore]
        public string TagName { get { return _TagName; } set{ _TagName = value; IdentifyingText = string.Empty; } }
        [JsonIgnore] 
        public string ShownText { get { return _ShownText; } set { _ShownText = value; IdentifyingText = string.Empty; } }
        [JsonIgnore]
        public string Id { get { return _Id; } set { _Id = value; IdentifyingText = string.Empty; } }

        
        public override string ToString()
        {
            return $"TagName:{TagName}, ShownText:{ShownText}, Id:{Id}";
        }
    }
    */
}

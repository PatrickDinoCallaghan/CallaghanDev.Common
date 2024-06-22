using CallaghanDev.SeleniumWithJSON.Extensions;
using CallaghanDev.Utilities.ConsoleHelper;
using CallaghanDev.Utilities.Web.Enums;
using CallaghanDev.Utilities.Web.Extensions;
using Newtonsoft.Json;
using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;
using System.Collections;

namespace CallaghanDev.SeleniumWithJSON
{

    public class WebControlEvent : IEnumerable<WebControlAction>
    {
        [JsonProperty]
        public string Name { get; set; }

        [JsonProperty]
        public string BaseUrl { get; set; }
        [JsonProperty]
        private List<WebControlAction> _actions = new List<WebControlAction>();

        [JsonProperty]
        public WebControlAction ReturnAction { get; set; }
        public class WebControlTask()
        {

        }

        public void Add(WebControlAction action)
        {
            _actions.Add(action);
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
        public IEnumerator<WebControlAction> GetEnumerator()
        {
            return _actions.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
        public object Run()
        {
            foreach (var item in _actions)
            {
                item.Run();
            }

            return ReturnAction;

        }

        public void InjectCookies()
        {

        }

        public void Save()
        {

        }
    }
    public class WebControlAction
    {
        [JsonProperty]
        WebControlElement _webControlElement;

        [JsonProperty]
        public WebElementActionEnum webElementAction;
        [JsonProperty]
        public string Name { get; set; }

        public WebControlAction(WebControlElement InWebControlElement)
        {
            _webControlElement = InWebControlElement;
        }

        public object Run(int Index )
        {
            if (_webControlElement == null)
            {
                _webControlElement = new WebControlElement();
            }
          return  FireEvent("", Index);
        }
        public object Run(string sendText)
        {

          return FireEvent(sendText, 0);
        }
        public object Run()
        {
            return FireEvent("",0);
        }
        private object FireEvent(string SendText, int Index)
        {
            switch (webElementAction)
            {
                case WebElementActionEnum.CheckIfElementDisplayed:
                   return _webControlElement.IsElementDisplayed();
                 case WebElementActionEnum.Click:
                    _webControlElement.Click();
                    return null;
                case WebElementActionEnum.SelectItemInList:
                    _webControlElement.SelectItemInList(Index);
                    return null;
                case WebElementActionEnum.SelectedItemValue:
                    return _webControlElement.SelectedItemValue();
                case WebElementActionEnum.ElementText:
                    return _webControlElement.ElementText();
                case WebElementActionEnum.IsCheckboxChecked:
                    return _webControlElement.IsCheckboxChecked();
                case WebElementActionEnum.SendKeys:
                    _webControlElement.SendKeys(SendText);
                    return null;
                case WebElementActionEnum.FillTextBox:
                    _webControlElement.FillTextBox(SendText);
                    return null;
                default:
                    throw new Exception("No action set.");

            }
        }
    }
    public class WebControlElement
    {
        [JsonProperty]
        public LocatorTypeEnum locatorType = LocatorTypeEnum.None;

        [JsonProperty]
        public WebControlElementType elementType = WebControlElementType.None;

        [JsonProperty]
        public LocatorTypeEnum locatorTypeEnum = LocatorTypeEnum.None;

        [JsonProperty]
        public string Name { get; set; }

        [JsonProperty]
        public string Identifier { get; set; }

        [JsonIgnore]
        private IWebDriver _webdriver;

        public WebControlElement() { }

        public void SetWebDriver(IWebDriver webdriver)
        {
            _webdriver = webdriver;
        }
        private IWebElement GetElement()
        {
            switch (locatorTypeEnum)
            {
                case LocatorTypeEnum.CssSelector:
                    return _webdriver.FindElement(By.CssSelector(Identifier));
                case LocatorTypeEnum.XPath:
                    return _webdriver.FindElement(By.XPath(Identifier));
                case LocatorTypeEnum.ClassName:
                    return _webdriver.FindElement(By.ClassName(Identifier));
                case LocatorTypeEnum.TagName:
                    return _webdriver.FindElement(By.TagName(Identifier));
                case LocatorTypeEnum.LinkText:
                    return _webdriver.FindElement(By.LinkText(Identifier));
                case LocatorTypeEnum.PartialLinkText:
                    return _webdriver.FindElement(By.PartialLinkText(Identifier));
                default:
                    Tuple<IWebElement, List<IWebElement>> elementOptions = _webdriver.ItemSelection(elementType, Identifier);
                    IWebElement selectedWebElement = ItemSelectionScreen(elementOptions);
                    if (selectedWebElement == null)
                    {
                        throw new Exception("No element selected for this action");
                    }
                    return selectedWebElement;
            }
        }

        private IWebElement ItemSelectionScreen(Tuple<IWebElement, List<IWebElement>> elementOptions)
        {
            Application.Init();
            var top = Application.Top;

            // Show the yes/no message screen
            IWebElement result = ShowYesNoMessage(elementOptions);


            Application.Shutdown();

            return result;
        }

        private IWebElement ShowYesNoMessage(Tuple<IWebElement, List<IWebElement>> data)
        {
            IWebElement selectedElement = data.Item1;
            var confirmationDialog = new Dialog("Confirm Selection", 60, 7);
            bool confirmed = false;

            var label = new Label($"Is this the correct selection?\n\n{selectedElement}")
            {
                X = Pos.Center(),
                Y = Pos.Center() - 1
            };

            var yesButton = new Button("Yes");

            yesButton.Clicked += () =>
            {
                confirmed = true;
               
                Application.RequestStop();
            };

            var noButton = new Button("No");

            noButton.Clicked += () =>
            {
                confirmed = false;
                selectedElement = null;
                Application.RequestStop();
            };

            confirmationDialog.Add(label);
            confirmationDialog.AddButton(yesButton);
            confirmationDialog.AddButton(noButton);

            Application.Run(confirmationDialog);

            if (confirmed) // Yes button is clicked
            {
                return selectedElement;
            }
            else
            {
                if (data.Item2 == null || data.Item2?.Count == 0)
                {
                    return null;
                }

                List<IWebElement> options = new List<IWebElement>();
                options.Add(data.Item1);
                options.AddRange(data.Item2);
                return ShowSelectionScreen(options);
            }
        }

        private IWebElement ShowSelectionScreen(List<IWebElement> options)
        {
            IWebElement selectedOption = null;

            var selectionDialog = new Dialog("Select an Option", 80, 24, new Button("Ok"));
            bool okClicked = false;

            var listView = new ListView(options.Select(r=>r.Text).ToList())
            {
                Width = Dim.Percent(50),
                Height = Dim.Fill(),
                AllowsMarking = false
            };

            var textView = new TextView
            {
                X = Pos.Percent(50),
                Width = Dim.Fill(),
                Height = Dim.Fill(),
                ReadOnly = true
            };

            listView.SelectedItemChanged += (args) =>
            {
                selectedOption = options[args.Item];
                textView.Text = selectedOption.SelectorsToString();
            };

            var okButton = new Button("Ok");

            okButton.Clicked += () =>
            {
                okClicked = true;
                Application.RequestStop();
            };

            selectionDialog.Add(listView);
            selectionDialog.Add(textView);
            selectionDialog.AddButton(okButton);

            Application.Run(selectionDialog);

            return okClicked ? selectedOption : null;
        }

        public bool IsElementDisplayed( int timeoutInSeconds = 5)
        {
            WebDriverWait wait = new WebDriverWait(_webdriver, TimeSpan.FromSeconds(timeoutInSeconds));
            return wait.Until<bool>(x => GetElement().Displayed);
        }
        public void Click()
        {
            GetElement().Click();
        }
        public void SelectItemInList( int itemIndex)
        {
            SelectElement select =  (SelectElement)GetElement();
            select.SelectByIndex(itemIndex);
        }
        public string SelectedItemValue()
        {
            SelectElement select = (SelectElement)GetElement();
            if (string.IsNullOrEmpty(select.SelectedOption.Text))
            {

                return select.SelectedOption.GetAttribute("value");
            }
            else
            {
                return select.SelectedOption.Text;
            }
        }
        public string ElementText()
        {
            return GetElement().Text;
        }
        public bool IsCheckboxChecked()
        {
            return GetElement().Selected;
        }
        public void SendKeys( string value)
        {
            GetElement().SendKeys(value);
        }
        public void FillTextBox(string value)
        {
            GetElement().SendKeys(value);
        }
    }

   
}

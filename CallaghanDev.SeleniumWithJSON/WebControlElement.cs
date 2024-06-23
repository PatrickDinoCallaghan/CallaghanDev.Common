using CallaghanDev.Utilities.ConsoleHelper;
using CallaghanDev.Utilities.Web.Enums;
using Newtonsoft.Json;
using OpenQA.Selenium.Support.UI;
using OpenQA.Selenium;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CallaghanDev.SeleniumWithJSON.Extensions;
using CallaghanDev.Utilities.Web.Extensions;
using DocumentFormat.OpenXml.Bibliography;
using System.Xml.Linq;

namespace CallaghanDev.SeleniumWithJSON
{
    public class WebControlElement
    {
        [JsonProperty]
        public LocatorTypeEnum locatorType { get; set; }
            

        [JsonProperty]
        public WebControlElementType elementType { get; set; }

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
            if (string.IsNullOrEmpty(Name))
            {
                Tuple<string, WebControlElementType> NameType = ShowInputBox();
                Name = NameType.Item1;
                elementType = NameType.Item2;
            }
            switch (locatorType)
            {
                case LocatorTypeEnum.Text:
                    return _webdriver.ItemSelection(elementType, Identifier).Item2.Where(r => r.Text == Identifier).First();
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
                    else
                    {
                        Identifier = selectedWebElement.BestIdentifier();
                        locatorType = selectedWebElement.GetLocatorUsed(Identifier);
                    }
                    return selectedWebElement;
            }
        }

        private Tuple<string, WebControlElementType> ShowInputBox()
        {
            Application.Init();
            var inputBox = new Dialog("Give web element a name", 50, 20);
            var okButton = new Button("OK", is_default: true)
            {
                X = Pos.Center(),
                Y = Pos.Bottom(inputBox) - 3,
                Enabled = false
            };

            var inputField = new TextField("")
            {
                X = Pos.Center(),
                Y = Pos.Center() - 2,
                Width = 40
            };

            var dropdownLabel = new Label("Select Element Type:")
            {
                X = Pos.Center(),
                Y = Pos.Center(),
            };

            var dropdown = new ComboBox()
            {
                X = Pos.Center(),
                Y = Pos.Center() + 1,
                Width = 40,

                Height = 6, 
            };

            // Populate the dropdown with the enum values
            List<string> strings = Enum.GetNames(typeof(WebControlElementType)).Select(r => r.ToString()).ToList();
            dropdown.SetSource(strings);

            inputField.TextChanged += (text) =>
            {
                okButton.Enabled = !string.IsNullOrWhiteSpace(inputField.Text.ToString()) && dropdown.SelectedItem != null && dropdown.SelectedItem >= 0;
            };

            dropdown.SelectedItemChanged += (args) =>
            {
                okButton.Enabled = !string.IsNullOrWhiteSpace(inputField.Text.ToString()) && dropdown.SelectedItem != null && dropdown.SelectedItem >= 0;
            };

            okButton.Clicked += () =>
            {
                var inputValue = inputField.Text.ToString();
                var selectedElementType = (WebControlElementType)Enum.Parse(typeof(WebControlElementType), dropdown.Text.ToString());
                MessageBox.Query(50, 7, "Input Received", $"You entered: {inputValue}\nSelected Element: {selectedElementType}", "OK");
                Application.RequestStop(inputBox);
            };

            inputBox.Add(inputField);
            inputBox.Add(dropdownLabel);
            inputBox.Add(dropdown);
            inputBox.AddButton(okButton);

            Application.Run(inputBox);
            Application.Shutdown();

            return new Tuple<string, WebControlElementType>(inputField.Text.ToString(), (WebControlElementType)dropdown.SelectedItem);
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

            var label = new Label($"Is this the correct selection?\n\n{selectedElement.BestIdentifier()}")
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
                options.AddRange(data.Item2);
                selectedElement = ShowSelectionScreen(options);
                return selectedElement;
            }
        }

        private IWebElement ShowSelectionScreen(List<IWebElement> options)
        {
            IWebElement selectedOption = null;

            var selectionDialog = new Dialog("Select an Option", 80, 24, new Button("Ok"));
            bool okClicked = false;

            var listView = new ListView(options.Select(r => r.BestIdentifier()).ToList())
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
                ReadOnly = true,
                WordWrap = true
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

        #region Event
        public bool IsElementDisplayed(int timeoutInSeconds = 5)
        {
            WebDriverWait wait = new WebDriverWait(_webdriver, TimeSpan.FromSeconds(timeoutInSeconds));
            return wait.Until<bool>(x => GetElement().Displayed);
        }
        public void Click()
        {

            GetElement().Click();

            WebDriverWait wait = new WebDriverWait(_webdriver, TimeSpan.FromSeconds(1));

            wait.Until(d => ((IJavaScriptExecutor)d).ExecuteScript("return document.readyState").Equals("complete"));
        }
        public void SelectItemInList(int itemIndex)
        {
            SelectElement select = (SelectElement)GetElement();
            select.SelectByIndex(itemIndex);

            WebDriverWait wait = new WebDriverWait(_webdriver, TimeSpan.FromSeconds(1));

            wait.Until(d => ((IJavaScriptExecutor)d).ExecuteScript("return document.readyState").Equals("complete"));
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
        public void SendKeys(string value)
        {
            GetElement().SendKeys(value);
        }
        public void FillTextBox(string value)
        {
            GetElement().SendKeys(value);
        }
        #endregion
    }
}

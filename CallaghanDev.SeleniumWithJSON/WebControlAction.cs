using CallaghanDev.Utilities.ConsoleHelper;
using CallaghanDev.Utilities.Web.Enums;
using Newtonsoft.Json;
using OpenQA.Selenium;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CallaghanDev.SeleniumWithJSON
{
    public class WebControlAction
    {
        [JsonProperty]
        WebControlElement _webControlElement;

        [JsonProperty]
        public WebElementActionEnum webElementAction;
        [JsonProperty]
        public string Name { get; set; }


        [JsonIgnore]
        public LocatorTypeEnum locatorType
        {
            get { return _webControlElement.locatorType; }
            set { _webControlElement.locatorType = value; }
        }

        [JsonIgnore]
        public WebControlElementType elementType
        {
            get { return _webControlElement.elementType; }
            set { _webControlElement.elementType = value; }
        }

        [JsonIgnore]
        public LocatorTypeEnum locatorTypeEnum
        {
            get { return _webControlElement.locatorType; }
            set { _webControlElement.locatorType = value; }
        }
        [JsonIgnore]
        public string ElementName
        {
            get { return _webControlElement.Name; }
            set { _webControlElement.Name = value; }
        }
        [JsonIgnore]
        public string Identifier
        {
            get { return _webControlElement.Identifier; }
            set { _webControlElement.Identifier = value; }
        }
       

        public WebControlAction()
        {
            _webControlElement = new WebControlElement();
        }
        public object Run(int Index)
        {
            if (_webControlElement == null)
            {
                _webControlElement = new WebControlElement();
            }
            return FireEvent("", Index);
        }
        public object Run(string sendText)
        {

            return FireEvent(sendText, 0);
        }
        public object Run()
        {
            return FireEvent("", 0);
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
                    webElementAction = ShowInputBox();
                    return FireEvent(SendText, Index);
            }
        }

        public void SetWebDriver(IWebDriver webdriver)
        {
            _webControlElement.SetWebDriver(webdriver);
        }

        private WebElementActionEnum ShowInputBox()
        {
            Application.Init();
            var inputBox = new Dialog("Give web element a name", 50, 20);
            var okButton = new Button("OK", is_default: true)
            {
                X = Pos.Center(),
                Y = Pos.Bottom(inputBox) - 3,
                Enabled = false
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
            List<string> strings = Enum.GetNames(typeof(WebElementActionEnum)).Select(r => r.ToString()).ToList();
            dropdown.SetSource(strings);


            dropdown.SelectedItemChanged += (args) =>
            {
                okButton.Enabled = dropdown.SelectedItem != null && dropdown.SelectedItem >= 0;
            };

            okButton.Clicked += () =>
            {
                var selectedElementType = (WebElementActionEnum)Enum.Parse(typeof(WebElementActionEnum), dropdown.Text.ToString());
                MessageBox.Query(50, 7, "Input Received", $"Selected Element: {selectedElementType}", "OK");
                Application.RequestStop(inputBox);
            };

            inputBox.Add(dropdownLabel);
            inputBox.Add(dropdown);
            inputBox.AddButton(okButton);

            Application.Run(inputBox);
            Application.Shutdown();

            return  (WebElementActionEnum)dropdown.SelectedItem;
        }

    }
}

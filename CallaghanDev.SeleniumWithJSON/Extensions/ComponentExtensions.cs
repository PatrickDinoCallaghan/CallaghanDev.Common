using CallaghanDev.Utilities.String;
using CallaghanDev.Utilities.Web.Enums;
using CallaghanDev.Utilities.Web.Extensions;
using OpenQA.Selenium;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CallaghanDev.SeleniumWithJSON.Extensions
{
    public delegate By ByLocatorFunction(string locator);
    public static class ComponentExtensions
    {
        private readonly static Similarity similarity = new Similarity();

        public static Tuple<IWebElement, List<IWebElement>> ItemSelection(this IWebDriver webdriver, WebControlElementType elementType, string Identifier)
        {
            WaitForPageLoad(webdriver);
            List<ByLocatorFunction> functions = new List<ByLocatorFunction>
            {
                By.CssSelector,
                By.XPath,
                By.ClassName,
                By.TagName,
                By.LinkText,
                By.PartialLinkText
            };
            foreach (var function in functions)
            {
                By byLocator = function(Identifier);
                IWebElement webElement = webdriver.FindElement(byLocator);

                if (GetElementType(webElement) == elementType)
                {
                    return new Tuple<IWebElement, List<IWebElement>>(webElement, null) ;
                }
            }
            List<IWebElement> webElementsByType = GetAllElements(webdriver).Where(r => GetElementType(r) == elementType).ToList();

            if (webElementsByType.Any())
            {
                if (webElementsByType.Count() == 1)
                {
                    return new Tuple<IWebElement, List<IWebElement>>(webElementsByType.First(), null);
                }
                else
                {
                    return new Tuple<IWebElement, List<IWebElement>>(GetMostSimilarLocator(webdriver, Identifier, webElementsByType), webElementsByType);
                }
            }
            else
            {
                throw new Exception("No controls of this type exist within this page...");
            }
        }
        private static IWebElement GetMostSimilarLocator(IWebDriver webdriver, string identifier, List<IWebElement> elements)
        {
            var allLocators = elements.Select(r => r.GetElementLocators());
            var allLocatorValues = allLocators
                .SelectMany(dict => dict.Values)
                .Where(value => !string.IsNullOrEmpty(value))
                .ToList();

            var mostSimilarValue = similarity.StringMostSimilar(identifier, allLocatorValues);

            foreach (var dict in allLocators)
            {
                foreach (var kvp in dict)
                {
                    if (kvp.Value == mostSimilarValue)
                    {

                        ByLocatorFunction func = kvp.Key;
                        By byLocator = func(kvp.Value);
                        IWebElement webElement = webdriver.FindElement(byLocator);
                        return webElement;
                    }
                }
            }

            throw new Exception("No similar locator found.");
        }
        private static WebControlElementType GetElementType(IWebElement element)
        {
            string tagName = element.TagName.ToLower();
            string type = element.GetAttribute("type")?.ToLower();

            return tagName switch
            {
                "input" => type switch
                {
                    "text" => WebControlElementType.InputText,
                    "password" => WebControlElementType.InputPassword,
                    "checkbox" => WebControlElementType.InputCheckbox,
                    "radio" => WebControlElementType.InputRadio,
                    "submit" => WebControlElementType.InputSubmit,
                    "button" => WebControlElementType.InputButton,
                    "file" => WebControlElementType.InputFile,
                    "hidden" => WebControlElementType.InputHidden,
                    "email" => WebControlElementType.InputEmail,
                    "number" => WebControlElementType.InputNumber,
                    "date" => WebControlElementType.InputDate,
                    "range" => WebControlElementType.InputRange,
                    "search" => WebControlElementType.InputSearch,
                    "tel" => WebControlElementType.InputTel,
                    "url" => WebControlElementType.InputUrl,
                    "color" => WebControlElementType.InputColor,
                    _ => throw new Exception("Unknown input type")
                },
                "button" => WebControlElementType.Button,
                "select" => WebControlElementType.Select,
                "option" => WebControlElementType.Option,
                "textarea" => WebControlElementType.TextArea,
                "label" => WebControlElementType.Label,
                "fieldset" => WebControlElementType.Fieldset,
                "legend" => WebControlElementType.Legend,
                "form" => WebControlElementType.Form,
                "output" => WebControlElementType.Output,
                "datalist" => WebControlElementType.Datalist,
                "keygen" => WebControlElementType.Keygen,
                "progress" => WebControlElementType.Progress,
                "meter" => WebControlElementType.Meter,
                _ => throw new Exception("Unknown element type")
            };
        }
        private static void WaitForPageLoad(IWebDriver driver)
        {
            var wait = new OpenQA.Selenium.Support.UI.WebDriverWait(driver, TimeSpan.FromSeconds(10));
            wait.Until(d => ((IJavaScriptExecutor)d).ExecuteScript("return document.readyState").Equals("complete"));
        }
        private static List<IWebElement> GetAllElements(this IWebDriver webdriver)
        {
            WaitForPageLoad(webdriver);
            return new List<IWebElement>(webdriver.FindElements(By.CssSelector("*")));
        }
    }
}

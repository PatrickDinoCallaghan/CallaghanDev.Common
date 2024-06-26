﻿using CallaghanDev.Utilities.String;
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
            webdriver.WaitForPageLoad();
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
                try
                {
                    By byLocator = function(Identifier);
                    IWebElement webElement = webdriver.FindElement(byLocator);

                    if (webElement.GetElementType() == elementType)
                    {
                        return new Tuple<IWebElement, List<IWebElement>>(webElement, null);
                    }
                }
                catch (OpenQA.Selenium.NoSuchElementException Notfound) { }
                catch (Exception ex) { }
            }
            List<IWebElement> webElementsByType = GetAllElements(webdriver).Where(r => r.GetElementType() == elementType && !string.IsNullOrEmpty(r.BestIdentifier())).ToList();

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

        private static void WaitForPageLoad(this IWebDriver driver)
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

using CallaghanDev.SeleniumWithJSON.Extensions;
using OpenQA.Selenium;
using OpenQA.Selenium.Remote;
using System;
using System.Text;

namespace CallaghanDev.Utilities.Web.Extensions
{
    public static class WebElementExtensions
    {
        public static Dictionary<ByLocatorFunction, string> GetElementLocators(this IWebElement element)
        {
            var locators = new Dictionary<ByLocatorFunction, string>
            {
                {  By.CssSelector,  element.GetCssSelector() },
                {   By.XPath, element.GetXPath() },
                { By.ClassName, element.GetClassName() },
                { By.TagName, element.GetTagName() },
                {  By.LinkText, element.TagName.ToLower() == "a" ? element.GetLinkText() : string.Empty },
                { By.PartialLinkText, element.TagName.ToLower() == "a" ? element.GetPartialLinkText() : string.Empty }
            };

            return locators;
        }
        public static string SelectorsToString(this IWebElement element)
        {
            var builder = new StringBuilder();

            try
            {
                builder.AppendLine($"CssSelector: {element.GetCssSelector()}");
            }
            catch (Exception ex)
            {
                builder.AppendLine($"CssSelector: {ex.Message}");
            }

            try
            {
                builder.AppendLine($"XPath: {element.GetXPath()}");
            }
            catch (Exception ex)
            {
                builder.AppendLine($"XPath: {ex.Message}");
            }

            try
            {
                builder.AppendLine($"ClassName: {element.GetClassName()}");
            }
            catch (Exception ex)
            {
                builder.AppendLine($"ClassName: {ex.Message}");
            }

            try
            {
                builder.AppendLine($"TagName: {element.GetTagName()}");
            }
            catch (Exception ex)
            {
                builder.AppendLine($"TagName: {ex.Message}");
            }

            try
            {
                builder.AppendLine($"LinkText: {element.GetLinkText()}");
            }
            catch (Exception ex)
            {
                builder.AppendLine($"LinkText: {ex.Message}");
            }

            try
            {
                builder.AppendLine($"PartialLinkText: {element.GetPartialLinkText()}");
            }
            catch (Exception ex)
            {
                builder.AppendLine($"PartialLinkText: {ex.Message}");
            }

            return builder.ToString();
        }

        private static string GetCssSelector(this IWebElement element)
        {
            string id = element.GetAttribute("id");
            if (!string.IsNullOrEmpty(id))
            {
                return $"#{id}";
            }

            string className = element.GetAttribute("class");
            if (!string.IsNullOrEmpty(className))
            {
                return $"{element.TagName.ToLower()}.{className.Replace(" ", ".")}";
            }

            return element.TagName.ToLower();
        }

        private static string GetXPath(this IWebElement element)
        {
            string id = element.GetAttribute("id");
            if (!string.IsNullOrEmpty(id))
            {
                return $"//*[@id='{id}']";
            }

            string className = element.GetAttribute("class");
            if (!string.IsNullOrEmpty(className))
            {
                return $"//{element.TagName}[contains(@class, '{className}')]";
            }

            string text = element.Text;
            if (!string.IsNullOrEmpty(text))
            {
                return $"//{element.TagName}[contains(text(), '{text}')]";
            }

            return $"//{element.TagName}";
        }

        private static string GetClassName(this IWebElement element)
        {
            return element.GetAttribute("class");
        }

        private static string GetTagName(this IWebElement element)
        {
            return element.TagName;
        }

        private static string GetLinkText(this IWebElement element)
        {
            if (element.TagName.ToLower() == "a")
            {
                return element.Text;
            }

            throw new Exception("Element is not a link");
        }

        private static string GetPartialLinkText(this IWebElement element)
        {
            if (element.TagName.ToLower() == "a")
            {
                return element.Text.Substring(0, System.Math.Min(element.Text.Length, 10));
            }

            throw new Exception("Element is not a link");
        }
    }
}

using CallaghanDev.SeleniumWithJSON.Extensions;
using CallaghanDev.Utilities.Web.Enums;
using OpenQA.Selenium;
using OpenQA.Selenium.Remote;
using System;
using System.Diagnostics;
using System.Text;

namespace CallaghanDev.Utilities.Web.Extensions
{
    public static class WebElementExtensions
    {
        public static string BestIdentifier(this IWebElement element)
        {
            if (!string.IsNullOrEmpty(element.Text))
            {
                return element.Text;
            }
            if (!string.IsNullOrEmpty(element.GetCssSelector()))
            {
                return element.GetCssSelector();
            }
            if (!string.IsNullOrEmpty(element.GetXPath()))
            {
                return element.GetXPath();
            }
            if (!string.IsNullOrEmpty(element.GetClassName()))
            {
                return element.GetClassName();
            }
            if (!string.IsNullOrEmpty(element.GetTagName()))
            {
                return element.GetTagName();
            }
            if (!string.IsNullOrEmpty(element.GetLinkText()))
            {
                return element.GetLinkText();
            }
            if (!string.IsNullOrEmpty(element.GetPartialLinkText()))
            {
                return element.GetPartialLinkText();
            }
            return "";
        }


        public static LocatorTypeEnum GetLocatorUsed(this IWebElement element,string Identifier)
        {
            if (!string.IsNullOrEmpty(Identifier))
            {
                try
                {
                    if (!string.IsNullOrEmpty(element.Text))
                    {
                        return LocatorTypeEnum.Text;
                    }
                }
                catch (NoSuchElementException) { }

                try
                {
                    if (element.GetCssSelector() == Identifier)
                    {
                        return LocatorTypeEnum.CssSelector;
                    }
                }
                catch (NoSuchElementException) { }
                
                try
                {
                    if (element.GetXPath() == Identifier)
                    {
                        return LocatorTypeEnum.XPath;
                    }
                }
                catch (NoSuchElementException) { }

                try
                {
                    if (element.GetClassName() == Identifier)
                    {
                        return LocatorTypeEnum.ClassName;
                    }
                }
                catch (NoSuchElementException) { }

                try
                {
                    if (element.GetTagName() == Identifier)
                    {
                        return LocatorTypeEnum.TagName;
                    }
                }
                catch (NoSuchElementException) { }

                try
                {
                    if (element.GetLinkText() == Identifier)
                    {
                        return LocatorTypeEnum.LinkText;
                    }
                }
                catch (NoSuchElementException) { }

                try
                {
                    if (element.GetPartialLinkText() == Identifier)
                    {
                        return LocatorTypeEnum.PartialLinkText;
                    }
                }
                catch (NoSuchElementException) { }
            }

            return LocatorTypeEnum.None;
        }

        public static Dictionary<ByLocatorFunction, string> GetElementLocators(this IWebElement element)
        {
            var locators = new Dictionary<ByLocatorFunction, string>
            {
                {   By.CssSelector,  element.GetCssSelector() },
                {   By.XPath, element.GetXPath() },
                {   By.ClassName, element.GetClassName() },
                {   By.TagName, element.GetTagName() },
                {   By.LinkText, element.TagName.ToLower() == "a" ? element.GetLinkText() : string.Empty },
                {   By.PartialLinkText, element.TagName.ToLower() == "a" ? element.GetPartialLinkText() : string.Empty }
            };

            return locators;
        }
        public static string SelectorsToString(this IWebElement element)
        {
            var builder = new StringBuilder();
            try
            {
                builder.AppendLine($"Text: {element.Text}");
            }
            catch (Exception ex)
            {
                builder.AppendLine($"Text: {ex.Message}");
            }
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
            else
            {
                return "";
            }
        }

        private static string GetPartialLinkText(this IWebElement element)
        {
            if (element.TagName.ToLower() == "a")
            {
                return element.Text.Substring(0, System.Math.Min(element.Text.Length, 10));
            }

            return "";
        }


        private static readonly Dictionary<string, WebControlElementType> InputTypeMapping = new()
        {
            { "text", WebControlElementType.TextBox },
            { "password", WebControlElementType.TextBox },
            { "checkbox", WebControlElementType.CheckBox },
            { "radio", WebControlElementType.RadioButton },
            { "submit", WebControlElementType.Button },
            { "button", WebControlElementType.Button },
        };

        private static readonly Dictionary<string, WebControlElementType> TagNameMapping = new()
        {
            { "button", WebControlElementType.Button },
            { "select", WebControlElementType.DropDownList },
            { "textarea", WebControlElementType.TextArea },
            { "label", WebControlElementType.Label },
            { "form", WebControlElementType.Unknown }, // Assuming 'form' could be categorized as Unknown
            { "a", WebControlElementType.Link }, // Assuming anchor tags for links
            { "img", WebControlElementType.Image }, // Assuming img tags for images
            { "table", WebControlElementType.Table } // Assuming table tags
        };

        public static WebControlElementType GetElementType(this IWebElement element)
        {
            string tagName = element.TagName.ToLower();
            if (tagName == "input")
            {
                string type = element.GetAttribute("type")?.ToLower();
                if (type != null && InputTypeMapping.TryGetValue(type, out var inputElementType))
                {
                    return inputElementType;
                }
            }

            if (TagNameMapping.TryGetValue(tagName, out var elementType))
            {
                return elementType;
            }

            return WebControlElementType.Unknown;
        }
    }
}

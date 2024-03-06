
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;
using System.Threading;
using Newtonsoft.Json;
using CallaghanDev.Utilities.String;

namespace CallaghanDev.Utilities.Web
{
    public static class WebDriverExtensions
    {
        public static Cookie ToCookie(this SeleniumCookieDTO dto)
        {
            return new Cookie(dto.Name, dto.Value, dto.Domain, dto.Path, dto.Expires, dto.Secure, dto.HttpOnly, dto.SameSite);
        }
        // Make sure to close the driver when done
        public static void CloseDriver(this IWebDriver driver)
        {
            driver.Close();
            driver.Quit();
        }

        public static void ExtractCookies(this IWebDriver webdriver, browserType  browser, string CookiesFileJson = "cookies.json")
        {
            if (browser == browserType.Firefox)
            {
                new FireFoxCookies().ExtractCookies(CookiesFileJson);
            }
            else if (browser == browserType.w3m)
            {

                new w3mCookies().ExtractCookies(CookiesFileJson);
            }
            else
            {
                throw new NotImplementedException();
            }
        }
        /// <summary>
        /// Imports cookies from the local machine to use them within the webdriver.
        /// </summary>
        /// <param name="webdriver"></param>
        /// <param name="BaseUrl"></param>
        /// <param name="CookiesFileJson"></param>
        public static void ImportCookies(this IWebDriver webdriver, string BaseUrl, string CookiesFileJson = "cookies.json")
        {
            WebDriverWait wait = new WebDriverWait(webdriver, TimeSpan.FromSeconds(10));

            wait.Until(d => ((IJavaScriptExecutor)d).ExecuteScript("return document.readyState").Equals("complete"));

            bool ImportCookiesExist = File.Exists(CookiesFileJson);
            try
            {
                if (ImportCookiesExist)
                {
                    bool CookieFound = false;
                    var cookiesDTOList = JsonConvert.DeserializeObject<List<SeleniumCookieDTO>>(File.ReadAllText(CookiesFileJson));
                    List<Cookie> cookies = cookiesDTOList.Select(r => r.ToCookie()).ToList();
                    foreach (var cookie in cookies)
                    {
                        //"chat.openai.com" example for OpenAi
                        if (cookie.Domain.Contains(BaseUrl))
                        {
                            try
                            {
                                webdriver.Manage().Cookies.AddCookie(cookie);

                            Console.WriteLine($"Cookie added for domain:{ConvertToValidUrl(cookie.Domain)}");
                            CookieFound = true;
                            }
                            catch (Exception ex)
                            {
                                Console.WriteLine("Cookie cannot be added:" + ex.Message);
                            }
                        }
                    }
                    if (!CookieFound)
                    {
                        if (cookies.Count > 0)
                        {
                            string CookieNameClosest = new Similarity().StringMostSimilar(BaseUrl, cookies.Select(r => r.Domain).ToList());
                           Cookie cookie = cookies.Where(r => r.Domain == CookieNameClosest).FirstOrDefault();
                        if (cookie != null)
                        {
                            string correctedDomian = ConvertToValidUrl(cookie.Domain);
                            webdriver.Navigate().GoToUrl(correctedDomian);
                            webdriver.Manage().Cookies.AddCookie(cookie);
                            Console.WriteLine($"Cookie added for domain:{correctedDomian}");
                        }
                        }
                    }
                }
                else
                {
                    Console.WriteLine("No cookies found");
                }
                webdriver.Navigate().GoToUrl(BaseUrl);
                wait = new WebDriverWait(webdriver, TimeSpan.FromSeconds(10));
                wait.Until(d => ((IJavaScriptExecutor)d).ExecuteScript("return document.readyState").Equals("complete"));

                var allCookies = webdriver.Manage().Cookies.AllCookies;

                File.WriteAllText("cookies.json", JsonConvert.SerializeObject(allCookies));
            }
            catch (Exception ex)
            {
                Console.WriteLine("Unable to load Cookies. ERROR:" + ex.Message);
            }
        }
        #region A set of CSS and form based extension methods for <see cref="IWebDriver"/>.
        public static string ConvertToValidUrl(string domain)
        {
            if (Uri.TryCreate(domain, UriKind.Absolute, out Uri result) && (result.Scheme == Uri.UriSchemeHttp || result.Scheme == Uri.UriSchemeHttps))
            {
                return domain;
            }
            else
            {
                return $"https://{domain}";
            }
        }
        /// <summary>
        /// Clicks a button that has the given value.
        /// </summary>
        /// <param name="buttonValue">The button's value (input[value=])</param>
        /// <param name="webdriver">A <see cref="IWebDriver"/> instance.</param>
        /// <exception cref="OpenQA.Selenium.NoSuchElementException">No element was found.</exception>
        public static void ClickButtonWithValue(this IWebDriver webdriver, string buttonValue)
        {
            webdriver.FindElement(By.CssSelector("input[value='" + buttonValue + "']")).Click();
        }

        /// <summary>
        /// Clicks a button with the id ending with the provided value.
        /// </summary>
        /// <param name="idEndsWith">A CSS id.</param>
        /// <param name="webdriver">A <see cref="IWebDriver"/> instance.</param>
        /// <exception cref="OpenQA.Selenium.NoSuchElementException">No element was found.</exception>
        public static void ClickButtonWithId(this IWebDriver webdriver, string idEndsWith)
        {
            webdriver.FindElement(By.CssSelector("input[id$='" + idEndsWith + "']")).Click();
        }

        /// <summary>
        /// Selects an item from a drop down box using the given CSS id and the itemIndex.
        /// </summary>
        /// <param name="selector">A valid CSS selector.</param>
        /// <param name="itemIndex">A zero-based index that determines which drop down box to target from the CSS selector (assuming
        /// the CSS selector returns more than one drop down box).</param>
        /// <param name="webdriver">A <see cref="IWebDriver"/> instance.</param>
        /// <exception cref="OpenQA.Selenium.NoSuchElementException">No element was found.</exception>
        public static void SelectItemInList(this IWebDriver webdriver, string selector, int itemIndex)
        {
            SelectElement element = new SelectElement(webdriver.FindElement(By.CssSelector(selector)));
            element.SelectByIndex(itemIndex);
        }

        /// <summary>
        /// Selects an item from the nth drop down box (based on the elementIndex argument), using the given CSS id and the itemIndex.
        /// </summary>
        /// <param name="selector">A valid CSS selector.</param>
        /// <param name="itemIndex">A zero-based index that determines which drop down box to target from the CSS selector (assuming
        /// the CSS selector returns more than one drop down box).</param>
        /// <param name="elementIndex">The element in the drop down list to select.</param>
        /// <param name="webdriver">A <see cref="IWebDriver"/> instance.</param>
        /// <exception cref="OpenQA.Selenium.NoSuchElementException">No element was found.</exception>
        public static void SelectItemInList(this IWebDriver webdriver, string selector, int itemIndex, int elementIndex)
        {
            SelectElement element = new SelectElement(webdriver.FindElements(By.CssSelector(selector))[elementIndex]);
            element.SelectByIndex(itemIndex);
        }

        /// <summary>
        /// Returns the number of elements that match the given CSS selector.
        /// </summary>
        /// <param name="selector">A valid CSS selector.</param>
        /// <param name="webdriver">A <see cref="IWebDriver"/> instance.</param>
        /// <returns>The number of elements found.</returns>
        /// <exception cref="OpenQA.Selenium.NoSuchElementException">No element was found.</exception>
        public static int ElementCount(this IWebDriver webdriver, string selector)
        {
            return webdriver.FindElements(By.CssSelector(selector)).Count;
        }

        /// <summary>
        /// Gets the selected index from a drop down box using the given CSS selector.
        /// </summary>
        /// <param name="selector">A valid CSS selector.</param>
        /// <param name="webdriver">A <see cref="IWebDriver"/> instance.</param>
        /// <returns>The index of the selected item in the drop down box</returns>
        /// <exception cref="OpenQA.Selenium.NoSuchElementException">No element was found.</exception>
        public static int SelectedIndex(this IWebDriver webdriver, string selector)
        {
            SelectElement element = new SelectElement(webdriver.FindElement(By.CssSelector(selector)));

            for (int i = 0; i < element.Options.Count; i++)
            {
                if (element.Options[i].Selected)
                    return i;
            }

            return -1;
        }

        /// <summary>
        /// Gets the selected index from the nth drop down box (based on the elementIndex argument), using the given CSS selector.
        /// </summary>
        /// <param name="selector">A valid CSS selector.</param>
        /// <param name="itemIndex">A zero-based index that determines which drop down box to target from the CSS selector (assuming
        /// the CSS selector returns more than one drop down box).</param>
        /// <param name="webdriver">A <see cref="IWebDriver"/> instance.</param>
        /// <returns>The index of the selected item in the drop down box</returns>
        /// <exception cref="OpenQA.Selenium.NoSuchElementException">No element was found.</exception>
        public static int SelectedIndex(this IWebDriver webdriver, string selector, int itemIndex)
        {
            SelectElement element = new SelectElement(webdriver.FindElements(By.CssSelector(selector))[itemIndex]);

            for (int i = 0; i < element.Options.Count; i++)
            {
                if (element.Options[i].Selected)
                    return i;
            }

            return -1;
        }

        /// <summary>
        /// Gets the selected value from a drop down box using the given CSS selector.
        /// </summary>
        /// <param name="selector">A valid CSS selector.</param>
        /// <param name="webdriver">A <see cref="IWebDriver"/> instance.</param>
        /// <returns>The value of the selected item.</returns>
        /// <exception cref="OpenQA.Selenium.NoSuchElementException">No element was found.</exception>
        public static string SelectedItemValue(this IWebDriver webdriver, string selector)
        {
            SelectElement element = (SelectElement)webdriver.FindElement(By.CssSelector(selector));
            return element.SelectedOption.GetAttribute("value");
        }

        /// <summary>
        /// Clicks a link with the text provided. This is case sensitive and searches using an Xpath contains() search.
        /// </summary>
        /// <param name="linkContainsText">The link text to search for.</param>
        /// <param name="webdriver">A <see cref="IWebDriver"/> instance.</param>
        /// <exception cref="OpenQA.Selenium.NoSuchElementException">No element was found.</exception>
        public static void ClickLinkWithText(this IWebDriver webdriver, string linkContainsText)
        {
            webdriver.FindElement(By.XPath("//a[contains(text(),'" + linkContainsText + "')]")).Click();
        }

        /// <summary>
        /// Clicks a link with the id ending with the provided value.
        /// </summary>
        /// <param name="idEndsWith">A CSS id.</param>
        /// <param name="webdriver">A <see cref="IWebDriver"/> instance.</param>
        /// <exception cref="OpenQA.Selenium.NoSuchElementException">No element was found.</exception>
        public static void ClickLinkWithId(this IWebDriver webdriver, string idEndsWith)
        {
            webdriver.FindElement(By.CssSelector("a[id$='" + idEndsWith + "']")).Click();
        }

        /// <summary>
        /// Clicks an element using the given CSS selector.
        /// </summary>
        /// <param name="selector">A valid CSS selector.</param>
        /// <param name="webdriver">A <see cref="IWebDriver"/> instance.</param>
        /// <exception cref="OpenQA.Selenium.NoSuchElementException">No element was found.</exception>
        public static void Click(this IWebDriver webdriver, string selector)
        {
            webdriver.FindElement(By.CssSelector(selector)).Click();
        }

        /// <summary>
        /// Clicks an element using the given CSS selector.
        /// </summary>
        /// <param name="selector">A valid CSS selector.</param>
        /// <param name="itemIndex">A zero-based index that determines which element to target from the CSS selector (assuming
        /// the CSS selector returns more than one element).</param>
        /// <param name="webdriver">A <see cref="IWebDriver"/> instance.</param>
        /// <exception cref="OpenQA.Selenium.NoSuchElementException">No element was found.</exception>
        public static void Click(this IWebDriver webdriver, string selector, int itemIndex)
        {
            webdriver.FindElements(By.CssSelector(selector))[itemIndex].Click();
        }

        /// <summary>
        /// Gets an input element with the id ending with the provided value.
        /// </summary>
        /// <param name="idEndsWith">A CSS id.</param>
        /// <param name="webdriver">A <see cref="IWebDriver"/> instance.</param>
        /// <returns>An <see cref="IWebElement"/> for the item matched.</returns>
        /// <exception cref="OpenQA.Selenium.NoSuchElementException">No element was found.</exception>
        public static IWebElement InputWithId(this IWebDriver webdriver, string idEndsWith)
        {
            return webdriver.FindElement(By.CssSelector("input[id$='" + idEndsWith + "']"));
        }

        /// <summary>
        /// Gets an element's value with the id ending with the provided value.
        /// </summary>
        /// <param name="idEndsWith">A CSS id.</param>
        /// <param name="webdriver">A <see cref="IWebDriver"/> instance.</param>
        /// <returns>The element's value.</returns>
        /// <exception cref="OpenQA.Selenium.NoSuchElementException">No element was found.</exception>
        public static string ElementValueWithId(this IWebDriver webdriver, string idEndsWith)
        {
            return webdriver.FindElement(By.CssSelector("input[id$='" + idEndsWith + "']")).GetAttribute("value");
        }

        /// <summary>
        /// Gets an element's value using the given CSS selector.
        /// </summary>
        /// <param name="selector">A valid CSS selector.</param>
        /// <param name="webdriver">A <see cref="IWebDriver"/> instance.</param>
        /// <returns>The element's value.</returns>
        /// <exception cref="OpenQA.Selenium.NoSuchElementException">No element was found.</exception>
        public static string ElementValue(this IWebDriver webdriver, string selector)
        {
            return webdriver.FindElement(By.CssSelector(selector)).GetAttribute("value");
        }

        /// <summary>
        /// Gets an element's value using the given CSS selector.
        /// </summary>
        /// <param name="selector">A valid CSS selector.</param>
        /// <param name="itemIndex">A zero-based index that determines which element to target from the CSS selector (assuming
        /// the CSS selector returns more than one element).</param>
        /// <param name="webdriver">A <see cref="IWebDriver"/> instance.</param>
        /// <returns>The element's value.</returns>
        /// <exception cref="OpenQA.Selenium.NoSuchElementException">No element was found.</exception>
        public static string ElementValue(this IWebDriver webdriver, string selector, int itemIndex)
        {
            return webdriver.FindElements(By.CssSelector(selector))[itemIndex].GetAttribute("value");
        }

        /// <summary>
        /// Gets an element's text using the given CSS selector.
        /// </summary>
        /// <param name="selector">A valid CSS selector.</param>
        /// <param name="itemIndex">A zero-based index that determines which element to target from the CSS selector (assuming
        /// the CSS selector returns more than one element).</param>
        /// <param name="webdriver">A <see cref="IWebDriver"/> instance.</param>
        /// <returns>The element's text.</returns>
        /// <exception cref="OpenQA.Selenium.NoSuchElementException">No element was found.</exception>
        public static string ElementText(this IWebDriver webdriver, string selector, int itemIndex)
        {
            return webdriver.FindElements(By.CssSelector(selector))[itemIndex].Text;
        }

        /// <summary>
        /// Return true if the checkbox with the id ending with the provided value is checked/selected.
        /// </summary>
        /// <param name="idEndsWith">A CSS id.</param>
        /// <param name="webdriver">A <see cref="IWebDriver"/> instance.</param>
        /// <returns>True if the checkbox is checked.</returns>
        /// <exception cref="OpenQA.Selenium.NoSuchElementException">No element was found.</exception>
        public static bool IsCheckboxChecked(this IWebDriver webdriver, string idEndsWith)
        {
            return webdriver.FindElement(By.CssSelector("input[id$='" + idEndsWith + "']")).Selected;
        }

        /// <summary>
        /// Clicks the checkbox with the id ending with the provided value.
        /// </summary>
        /// <param name="idEndsWith">A CSS id.</param>
        /// <param name="webdriver">A <see cref="IWebDriver"/> instance.</param>
        /// <exception cref="OpenQA.Selenium.NoSuchElementException">No element was found.</exception>
        public static void ClickCheckbox(this IWebDriver webdriver, string idEndsWith)
        {
            webdriver.FindElement(By.CssSelector("input[id$='" + idEndsWith + "']")).Click();
        }

        /// <summary>
        /// Sets an element's (an input field) value to the provided text by using SendKeys().
        /// </summary>
        /// <param name="value">The text to type.</param>
        /// <param name="element">A <see cref="IWebElement"/> instance.</param>
        /// <exception cref="OpenQA.Selenium.NoSuchElementException">No element was found.</exception>
        public static void SetValue(this IWebElement element, string value)
        {
            element.SendKeys(value);
        }

        /// <summary>
        /// Sets an element's (an input field) value to the provided text, using the given CSS selector and using SendKeys().
        /// </summary>
        /// <param name="selector">A valid CSS selector.</param>
        /// <param name="value">The text to type.</param>
        /// <param name="webdriver">A <see cref="IWebDriver"/> instance.</param>
        /// <exception cref="OpenQA.Selenium.NoSuchElementException">No element was found.</exception>
        public static void SetValue(this IWebDriver webdriver, string selector, string value)
        {
            webdriver.FindElement(By.CssSelector(selector)).Clear();
            webdriver.FindElement(By.CssSelector(selector)).SendKeys(value);
        }

        /// <summary>
        /// Sets an element's (an input field) value to the provided text, using the given CSS selector and using SendKeys().
        /// </summary>
        /// <param name="selector">A valid CSS selector.</param>
        /// <param name="value">The text to type.</param>
        /// <param name="itemIndex">A zero-based index that determines which element to target from the CSS selector (assuming
        /// the CSS selector returns more than one element).</param>
        /// <param name="webdriver">A <see cref="IWebDriver"/> instance.</param>
        /// <exception cref="OpenQA.Selenium.NoSuchElementException">No element was found.</exception>
        public static void SetValue(this IWebDriver webdriver, string selector, string value, int itemIndex)
        {
            webdriver.FindElements(By.CssSelector(selector))[itemIndex].Clear();
            webdriver.FindElements(By.CssSelector(selector))[itemIndex].SendKeys(value);
        }

        /// <summary>
        /// Sets the textbox with the given CSS id to the provided value.
        /// </summary>
        /// <param name="idEndsWith">A CSS id.</param>
        /// <param name="value">The text to type.</param>
        /// <param name="webdriver">A <see cref="IWebDriver"/> instance.</param>
        /// <exception cref="OpenQA.Selenium.NoSuchElementException">No element was found.</exception>
        public static void FillTextBox(this IWebDriver webdriver, string idEndsWith, string value)
        {
            webdriver.SetValue("input[id$='" + idEndsWith + "']", value);
        }

        /// <summary>
        /// Sets the textarea with the given CSS id to the provided value.
        /// </summary>
        /// <param name="value">The text to set the value to.</param>
        /// <param name="idEndsWith">A CSS id.</param>
        /// <param name="webdriver">A <see cref="IWebDriver"/> instance.</param>
        /// <exception cref="OpenQA.Selenium.NoSuchElementException">No element was found.</exception>
        public static void FillTextArea(this IWebDriver webdriver, string idEndsWith, string value)
        {
            webdriver.SetValue("textarea[id$='" + idEndsWith + "']", value);
        }

        /// <summary>
        /// Waits the specified time in second (using a thread sleep)
        /// </summary>
        /// <param name="seconds">The number of seconds to wait (this uses TimeSpan.FromSeconds)</param>
        /// <param name="webdriver">A <see cref="IWebDriver"/> instance.</param>
        [Obsolete("Use WaitForElementDisplayed instead, as Wait uses Thread.Sleep")]
        public static void Wait(this IWebDriver webdriver, double seconds)
        {
            Thread.Sleep(TimeSpan.FromSeconds(seconds));
        }

        /// <summary>
        /// Waits 2 seconds, which is *usually* the maximum time needed for all Javascript execution to finish on the page.
        /// </summary>
        /// <param name="webdriver">A <see cref="IWebDriver"/> instance.</param>
        [Obsolete("Use WaitForElementDisplayed instead, as WaitForPageLoad uses Thread.Sleep")]
        public static void WaitForPageLoad(this IWebDriver webdriver)
        {
            webdriver.Wait(2);
        }

        /// <summary>
        /// Waits for an elements to be displayed on the page for the time specified.
        /// </summary>
        /// <param name="driver">A <see cref="IWebDriver"/> instance.</param>
        /// <param name="by">The selector to find the element with.</param>
        /// <param name="timeoutInSeconds">The number of seconds to wait</param>
        /// <returns></returns>
        public static IWebElement WaitForElementDisplayed(this IWebDriver driver, By by, int timeoutInSeconds = 10)
        {
            WebDriverWait wait = new WebDriverWait(driver, TimeSpan.FromSeconds(timeoutInSeconds));
            if (wait.Until<bool>(x => x.FindElement(by).Displayed))
            {
                return driver.FindElement(by);
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// Determines whether the element provided by the selector is displayed or not, waiting a 
        /// certain amount of time for it to be displayed.
        /// </summary>
        /// <param name="driver">A <see cref="IWebDriver"/> instance.</param>
        /// <param name="by">The selector to find the element with.</param>
        /// <param name="timeoutInSeconds">The number of seconds to wait.</param>
        /// <returns>True if the element is displayed, false otherwise.</returns>
        public static bool IsElementDisplayed(this IWebDriver driver, By by, int timeoutInSeconds = 10)
        {
            WebDriverWait wait = new WebDriverWait(driver, TimeSpan.FromSeconds(timeoutInSeconds));
            return wait.Until<bool>(x => x.FindElement(by).Displayed);
        }

        #endregion
    }
}
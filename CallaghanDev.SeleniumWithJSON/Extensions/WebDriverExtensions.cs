
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;
using System.Threading;
using Newtonsoft.Json;
using CallaghanDev.Utilities.String;
using CallaghanDev.Utilities.Web.Enums;

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

        public static void ExtractCookies(this IWebDriver webdriver, BrowserTypeEnum  browser, string CookiesFileJson = "cookies.json")
        {
            if (browser == BrowserTypeEnum.Firefox)
            {
                new FireFoxCookies().ExtractCookies(CookiesFileJson);
            }
            else if (browser == BrowserTypeEnum.w3m)
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

                            Console.WriteLine($"Cookie added for domain:{webdriver.ConvertToValidUrl(cookie.Domain)}");
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
                            string correctedDomian = webdriver.ConvertToValidUrl(cookie.Domain);
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

        #region A set of CSS and form based extension methods for <see cref="IWebDriver"/>.
        public static string ConvertToValidUrl(this IWebDriver webdriver, string domain)
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
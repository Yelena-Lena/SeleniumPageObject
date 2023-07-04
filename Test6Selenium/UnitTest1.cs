using System;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.DevTools.V112.HeapProfiler;
using OpenQA.Selenium.Support.UI;
using System.Collections.Generic;
using SeleniumExtras.WaitHelpers;
using System.Configuration;
using System.Xml.Linq;
using NUnit.Framework.Internal;
using OpenQA.Selenium.Interactions;
using System.Collections.ObjectModel;

namespace Test6Selenium
{
    public class Tests
    {
        private IWebDriver? _driver;
        private By acceptCoookies = By.Id("onetrust-accept-btn-handler");

        //for test #1
        private By careersLink = By.LinkText("Careers");
        private By keywords = By.XPath("//*[@id='new_form_job_search-keyword']");
        private By remoteCheck = By.XPath("//label[contains(.,'Remote')]");
        private By findButton = By.XPath("//*[@id='jobSearchFilterForm']/button");
        private By bylocationSelect = By.CssSelector(".recruiting-search__location");
        private By viewMore = By.XPath("//*[@id='main']/div[1]/div[2]/section/div[2]/div/div/section/a");
       
        //for test #2
        private By searchIcon = By.CssSelector(".search-icon");
        private By searchString = By.XPath("//*[@id='new_form_search']");
        private By searchButton = By.ClassName("header-search__submit");

        [SetUp]
        public void Setup()
        {
            string configFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "App.config");
            ExeConfigurationFileMap configFileMap = new ExeConfigurationFileMap();
            configFileMap.ExeConfigFilename = configFilePath;
            Configuration config = ConfigurationManager.OpenMappedExeConfiguration(configFileMap, ConfigurationUserLevel.None);

            this._driver = new ChromeDriver();
            this._driver.Navigate().GoToUrl(config.AppSettings.Settings["baseUrl"].Value);
            this._driver.Manage().Window.Maximize();
        }

        [TestCase("C#", "All Locations")]
        [TestCase("Java", "All Locations")]
        public void SearchForPosition(string progrLang, string optionText)
        {
            IsElementVisible(acceptCoookies);
            _driver.FindElement(acceptCoookies).Click();
            // Find a link “Carriers” and click on it
            _driver.FindElement(careersLink).Click();
            // Write name of any programming language in the field “Keywords” 
            IsElementVisible(keywords);
            _driver.FindElement(keywords).SendKeys(progrLang);

            // Find the locations element
            var dropdownContainer = _driver.FindElement(bylocationSelect);
            var dropdown = dropdownContainer.FindElement(By.CssSelector("select"));
            var dropdownOptions = dropdown.FindElements(By.TagName("option"));

            foreach (var option in dropdownOptions)
            {
                if (option.Text == optionText)
                {
                    option.Click();
                    break;
                }
            }
            // Select option “Remote”
            IsElementVisible(remoteCheck);
            _driver.FindElement(remoteCheck).Click();
            // Click on the button “Find” 
            _driver.FindElement(findButton).Click();

            // implcit wait for downloading searching elements
            _driver.Manage().Timeouts().ImplicitWait = TimeSpan.FromSeconds(60);
            IJavaScriptExecutor jsExecutor = (IJavaScriptExecutor)_driver;
            // Scroll to the end of the page
            jsExecutor.ExecuteScript("window.scrollTo(0, document.body.scrollHeight);");
            _driver.Manage().Timeouts().ImplicitWait = TimeSpan.FromSeconds(60);
            ((IJavaScriptExecutor)_driver).ExecuteScript("window.scrollTo(0, document.body.scrollHeight)");

            var wait2 = new WebDriverWait(_driver, TimeSpan.FromSeconds(20));  // Increase the timeout to 20 seconds
            try
            {
                var showMoreButton =
                    wait2.Until(ExpectedConditions.ElementIsVisible(
                        viewMore));
                showMoreButton = wait2.Until(ExpectedConditions.ElementToBeClickable(showMoreButton));
                showMoreButton.Click();
            }
            catch (WebDriverTimeoutException)
            {
                //there is no such element on page
            }

            _driver.Manage().Timeouts().ImplicitWait = TimeSpan.FromSeconds(30);
            var listOfResults = _driver.FindElement(By.ClassName("search-result__list"));
            List<IWebElement> allElements =
                    listOfResults.FindElements(By.ClassName("search-result__item")).ToList();
            IWebElement lastElement = allElements.ElementAt(allElements.Count - 1);
            var viewApplyButton = lastElement.FindElement(By.CssSelector(
                    "a.button-text.primary-button-preset.white-background-preset.search-result__item-apply-23"));
            ((IJavaScriptExecutor)_driver).ExecuteScript("arguments[0].scrollIntoView(true);", viewApplyButton);
            ((IJavaScriptExecutor)_driver).ExecuteScript("arguments[0].click();", viewApplyButton);
            _driver.Manage().Timeouts().ImplicitWait = TimeSpan.FromSeconds(30);

            ReadOnlyCollection<IWebElement> list =
                _driver.FindElements(By.XPath("//*[contains(text(),'" + progrLang + "')]"));
            Assert.IsTrue(list.Count > 0, "Text not found!");

        }

        [TestCase("Blockchain")]
        [TestCase("Cloud")]
        [TestCase("Automation")]
        public void ValidateGlobalSearch(string textForSearch)
        {
            IsElementVisible(acceptCoookies);
            _driver.FindElement(acceptCoookies).Click();
            //3.	Find a magnifier icon and click on it
            IsElementVisible(searchIcon);
            _driver.FindElement(searchIcon).Click();
            //4.	Find a search string and put there “Blockchain”/”Cloud”/”Automation” (use as a parameter for a test)
            IsElementVisible(searchString);
            _driver.FindElement(searchString).SendKeys(textForSearch);
            // 5.Click “Find” button
            _driver.FindElement(searchButton).Click();
            // implcit wait for downloading searching elements
            _driver.Manage().Timeouts().ImplicitWait = TimeSpan.FromSeconds(30);
            // Create an instance of the JavaScript executor
            IJavaScriptExecutor jsExecutor = (IJavaScriptExecutor)_driver;
            // Scroll to the end of the page
            jsExecutor.ExecuteScript("window.scrollTo(0, document.body.scrollHeight);");
            // Find the specific <div> element containing the links
            IWebElement searchResultsDiv = _driver.FindElement(By.ClassName("search-results__items"));
            // Find all the links within the <div> element
            List<IWebElement> links = searchResultsDiv.FindElements(By.TagName("a")).ToList();
            // Use LINQ to filter the links that contain the word in their text
            List<IWebElement> linksWithText = links.Where(link =>
                link.Text.IndexOf(textForSearch, StringComparison.OrdinalIgnoreCase) >= 0 ||
                link.GetAttribute("href").IndexOf(textForSearch, StringComparison.OrdinalIgnoreCase) >= 0
            ).ToList();
            List<IWebElement> descriptions = searchResultsDiv.FindElements(By.TagName("p")).ToList();
            List<IWebElement> descriptionsWithText = descriptions.Where(link =>
                link.Text.IndexOf(textForSearch, StringComparison.OrdinalIgnoreCase) >= 0).ToList();
            // Check if all links contain the word
            bool allDescriptionsContainCloud = links.Count == descriptionsWithText.Count;

            Assert.IsTrue(allDescriptionsContainCloud);

            // Assert.That(linksWithText.Count, Is.EqualTo(links.Count));


        }

        public void IsElementVisible(By element, int timeoutSec = 10)
        {
            new WebDriverWait(this._driver, TimeSpan.FromSeconds(timeoutSec)).Until(
                ExpectedConditions.ElementIsVisible(element));
        }

        [TearDown]
        public void TearDown()
        {
            this._driver.Close();
            this._driver.Quit();
        }
    }
}
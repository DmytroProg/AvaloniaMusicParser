using HtmlAgilityPack;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Interactions;
using OpenQA.Selenium.Support.UI;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessLogicLayer.Interfaces
{
    public abstract class ParserService<T> : IDisposable
    {
        protected ChromeDriver driver;

        public virtual void Dispose()
        {
            try
            {
                driver?.Dispose();
            }
            catch (Exception) { }
        }

        protected async Task OpenPage(string url)
        {
            await Task.Run(() =>
            {
                var chromeOptions = new ChromeOptions();
                chromeOptions.AddArgument("--window-position=-32000,-32000");
                var chromeDriveService = ChromeDriverService.CreateDefaultService();
                chromeDriveService.HideCommandPromptWindow = true;
                driver = new ChromeDriver(chromeDriveService, chromeOptions);

                driver.Navigate().GoToUrl(url);
            });
        }

        protected async Task<string> ReadHtmlDocument()
        {
            await Task.Delay(1000);
            string dynamicHtml = driver.PageSource;

            return dynamicHtml;
        }

        protected async Task<HtmlDocument> LoadHtmlDocument()
        {
            var htmlString = await ReadHtmlDocument();

            var doc = new HtmlDocument();
            doc.LoadHtml(htmlString);

            return doc;
        }

        protected void MoveDownPage()
        {
            new Actions(driver)
                .ScrollByAmount(0, 1500)
                .Perform();
        }

        protected bool IsEndOfSite()
        {
            IJavaScriptExecutor js = (IJavaScriptExecutor)driver;
            long currentScrollPos = (long)js.ExecuteScript("return window.scrollY+1500;");
            long pageHeight = (long)js.ExecuteScript("return document.body.scrollHeight;");

            return currentScrollPos >= pageHeight;
        }

        public abstract Task<T> GetAll(string url);

        public void CloseDriver()
        {
            driver?.Quit();
        }
    }
}

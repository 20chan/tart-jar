using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using OpenQA.Selenium.Chrome;

namespace tart_jar {
    public class FBScrapper : IDisposable {
        private const string FB = "https://www.facebook.com/";
        private readonly ChromeDriver driver;

        public FBScrapper(double implicitWait, double pageLoad) {
            driver = new ChromeDriver();
            var timeouts = driver.Manage().Timeouts();
            timeouts.ImplicitWait = TimeSpan.FromSeconds(implicitWait);
            timeouts.PageLoad = TimeSpan.FromSeconds(pageLoad);
        }

        public void Dispose() {
            driver.Close();
            driver.Dispose();
        }

        public Task<bool> LoginAsync(string email, string password) {
            return Task.Run(() => {
                driver.Url = $"{FB}/login.php?next=https%3A%2F%2Fwww.facebook.com%2Fanalytics";
                driver.FindElementById("email").SendKeys(email);
                driver.FindElementById("pass").SendKeys(password);
                driver.FindElementById("loginbutton").Click();

                return driver.Url.StartsWith("https://www.facebook.com/analytics");
            });
        }

        public Task WaitForLoginManuallyAsync() {
            return Task.Run(async () => {
                driver.Url = $"{FB}/login.php?next=https%3A%2F%2Fwww.facebook.com%2Fanalytics";
                while (true) {
                    if (driver.Url.StartsWith("https://www.facebook.com/analytics")) {
                        return;
                    }

                    await Task.Delay(1000);
                }
            });
        }

        public async IAsyncEnumerable<EventData> ScrapEventDataAsync(string appId, DateTime since, DateTime until, Task wait) {
            await Task.Run(() => {
                var s = TimeStamp(since);
                var u = TimeStamp(until);
                driver.Url = $"{FB}/analytics/{appId}/BreakdownTable?since={s}&until={u}&view=CREATE&";
            });

            await wait;

            // wow such not implemented yeet
            yield return new EventData();

            long TimeStamp(DateTime date) {
                return (long)date.Subtract(new DateTime(1970, 1, 1)).TotalMilliseconds;
            }
        }
    }
}
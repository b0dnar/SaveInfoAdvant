using System;
using System.Collections.Generic;
using System.Threading;
using System.Windows;
using OpenQA.Selenium;
using OpenQA.Selenium.Firefox;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.IO;


namespace SaveFotosAdvant
{
    public static class BaseClasse
    {
        public static string GetCookies()
        {
            FirefoxDriver driver;

            FirefoxDriverService service = FirefoxDriverService.CreateDefaultService();
            service.HideCommandPromptWindow = true;

            FirefoxOptions options = new FirefoxOptions();
            options.AddArgument("--headless");
            string rez = "";

            try
            {

                driver = new FirefoxDriver(service, options);


                driver.Navigate().GoToUrl("http://advant.club/ua/search/");
                Thread.Sleep(1000);
                driver.FindElement(By.Id("id_login_or_email")).SendKeys("testlogin");
                driver.FindElement(By.Id("id_password")).SendKeys("qwerty12");
                driver.FindElement(By.Id("submit")).Click();

                var cookies = driver.Manage().Cookies.AllCookies;

                foreach (var a in cookies)
                {
                    if (a.Name.Equals("sessionid"))
                        rez = a.Value;
                }

                driver.Close();
                driver.Quit();
                driver.Dispose();
            }
            catch (Exception e)
            {
                MessageBox.Show(String.Format("Ошибка с получением кукі!!! Проблема с мозила\n{0}", e.ToString()));
            }

            return rez;
        }

        public static List<string> CreateFilters(string nameCountry, string cookie)
        {
            var listAllUrl = new List<string>();

            string idCountry = GetIdCountry(nameCountry, cookie);
            if (idCountry.Equals(""))
                return null;

            MethodGET("https://advant.club/search/departure-city/change/ua/2014/", cookie);//set city Kyiv

            string dateF = DateTime.Now.ToString("dd.MM.yyyy");
            string dateT = DateTime.Now.AddDays(12).ToString("dd.MM.yyyy");

            for (var nightCount = 6; nightCount <= 21; nightCount++)
            {
                for (var stars = 3; stars <= 5; stars++)
                {
                    listAllUrl.Add("https://advant.club/ua/search/?country=" + idCountry + "&date_from=" + dateF + "&date_till=" + dateT + "&night_from=" + nightCount + "&night_till=" + nightCount + "&adult_amount=2&child_amount=0&child1_age=0&child2_age=0&child3_age=0&hotel_ratings=3&hotel_ratings=4&hotel_ratings=5&price_from=100&price_till=100000");
                    listAllUrl.Add("https://advant.club/ua/search/?country=" + idCountry + "&date_from=" + dateF + "&date_till=" + dateT + "&night_from=" + nightCount + "&night_till=" + nightCount + "&adult_amount=2&child_amount=1&child1_age=2&child2_age=0&child3_age=0&hotel_ratings=3&hotel_ratings=4&hotel_ratings=5&price_from=100&price_till=100000");
                    listAllUrl.Add("https://advant.club/ua/search/?country=" + idCountry + "&date_from=" + dateF + "&date_till=" + dateT + "&night_from=" + nightCount + "&night_till=" + nightCount + "&adult_amount=2&child_amount=1&child1_age=12&child2_age=0&child3_age=0&hotel_ratings=3&hotel_ratings=4&hotel_ratings=5&price_from=100&price_till=100000");
                }
            }

            return listAllUrl;
        }

        private static string GetIdCountry(string name, string cookie)
        {
            Regex reg = new Regex(@"<option value=.(?<val>.*?).>" + name);
            Regex regNum = new Regex(@"[0-9]+");
            string url = "https://advant.club/ua/search/", rez = "";

            try
            {
                string kod = MethodGET(url, cookie);
                rez = reg.Match(kod).Groups["val"].Value;
                rez = regNum.Match(rez).Value;
            }
            catch (Exception e)
            {
                MessageBox.Show(String.Format("Ошибка с получением id страны!!!\n{0}", e.ToString()));
            }

            return rez;
        }

        public static long GetTimeRequest()
        {
            DateTime foo = DateTime.UtcNow;
            long unixTime = ((DateTimeOffset)foo).ToUnixTimeMilliseconds();

            return unixTime;
        }

        public static string MethodGET(string url, string cookies)
        {
            if (!url.Contains("http") || url.Equals(""))
                return "";

            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
            HttpWebResponse response = null;
            string rez = "";

            request.Method = WebRequestMethods.Http.Get;
            request.AllowAutoRedirect = false;
            request.Headers.Add("Cookie", "sessionid=" + cookies);

            request.KeepAlive = false;
            request.ContentType = "application/x-www-form-urlencoded";
            //request.UserAgent = "Mozilla/5.0 (X11; Linux x86_64; rv:45.0) Gecko/20100101 Thunderbird/45.3.0";
            try
            {
                response = (HttpWebResponse)request.GetResponse();
                if (response.StatusCode == HttpStatusCode.Redirect)
                {
                    string newUrl = response.Headers["Location"];
                    return newUrl;
                }

                if (response == null)
                    return rez;

                Stream receiveStream = response.GetResponseStream();
                StreamReader readStream = new StreamReader(receiveStream, Encoding.UTF8);
                rez = readStream.ReadToEnd();
            }
            catch (Exception e)
            {
                MessageBox.Show(String.Format("Ошибка в запросе\nurl = {0}\n{1}", url, e.ToString()));
            }


            return rez;
        }
    }
}

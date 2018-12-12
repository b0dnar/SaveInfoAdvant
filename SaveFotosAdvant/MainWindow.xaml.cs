using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Text.RegularExpressions;
using System.Threading;
using System.IO;
//using System.Net;
using OpenQA.Selenium;
using OpenQA.Selenium.Firefox;
using HtmlAgilityPack;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace SaveFotosAdvant
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        public MainWindow()
        {
            InitializeComponent();
            Initial();
        }

        private void Initial()
        {
            comboNameCountry.Items.Add("Выберете страну");
            comboNameCountry.Items.Add("Австрия");
            comboNameCountry.Items.Add("Азербайджан");
            comboNameCountry.Items.Add("Албания");
            comboNameCountry.Items.Add("Андорра");
            comboNameCountry.Items.Add("Болгария");
            comboNameCountry.Items.Add("Бразилия");
            comboNameCountry.Items.Add("Венгрия");
            comboNameCountry.Items.Add("Вьетнам");
            comboNameCountry.Items.Add("Греция");
            comboNameCountry.Items.Add("Грузия");
            comboNameCountry.Items.Add("Доминикана");
            comboNameCountry.Items.Add("Египет");
            comboNameCountry.Items.Add("Израиль");
            comboNameCountry.Items.Add("Индия");
            comboNameCountry.Items.Add("Индонезия");
            comboNameCountry.Items.Add("Иордания");
            comboNameCountry.Items.Add("Испания");
            comboNameCountry.Items.Add("Италия");
            comboNameCountry.Items.Add("Катар");
            comboNameCountry.Items.Add("Кения");
            comboNameCountry.Items.Add("Кипр");
            comboNameCountry.Items.Add("Китай");
            comboNameCountry.Items.Add("Куба");
            comboNameCountry.Items.Add("Маврикий");
            comboNameCountry.Items.Add("Малайзия");
            comboNameCountry.Items.Add("Мальдивы");
            comboNameCountry.Items.Add("Мальта");
            comboNameCountry.Items.Add("Марокко");
            comboNameCountry.Items.Add("Мексика");
            comboNameCountry.Items.Add("ОАЭ");
            comboNameCountry.Items.Add("Оман");
            comboNameCountry.Items.Add("Польша");
            comboNameCountry.Items.Add("Португалия");
            comboNameCountry.Items.Add("Сейшельские острова");
            comboNameCountry.Items.Add("Сингапур");
            comboNameCountry.Items.Add("Словакия");
            comboNameCountry.Items.Add("Словения");
            comboNameCountry.Items.Add("США");
            comboNameCountry.Items.Add("Таиланд");
            comboNameCountry.Items.Add("Танзания");
            comboNameCountry.Items.Add("Тунис");
            comboNameCountry.Items.Add("Турция");
            comboNameCountry.Items.Add("Франция");
            comboNameCountry.Items.Add("Хорватия");
            comboNameCountry.Items.Add("Черногория");
            comboNameCountry.Items.Add("Чехия");
            comboNameCountry.Items.Add("Шри Ланка");
            comboNameCountry.Items.Add("Япония");

            comboNameCountry.SelectedIndex = 0;

            comboTypeParser.Items.Add("Выберете тип");
            comboTypeParser.Items.Add("Парсинг фото");
            comboTypeParser.Items.Add("Парсинг описания");

            comboTypeParser.SelectedIndex = 0;
        }

        private void buttonChoiceFolder_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                using (var dlg = new System.Windows.Forms.FolderBrowserDialog())
                {
                    dlg.ShowNewFolderButton = true;
                    System.Windows.Forms.DialogResult result = dlg.ShowDialog();

                    if (result == System.Windows.Forms.DialogResult.OK)
                        textPatchFolder.Text = dlg.SelectedPath;
                }
            }
            catch
            {
                MessageBox.Show("Ошибка при выборе местоположение папки!");
            }
        }

        private void buttonCancel_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void buttonStart_Click(object sender, RoutedEventArgs e)
        {
            if (comboTypeParser.Text.Equals("Выберете тип") || comboNameCountry.Text.Equals("Выберете страну") || textPatchFolder.Text.Equals(""))
            {
                MessageBox.Show("Вы не заполнили все поля!");
                return;
            }
            else if (comboTypeParser.Text.Equals("Парсинг фото"))
            {
                if (textNameFolder.Text.Equals(""))
                {
                    MessageBox.Show("Вы не заполнили название папки!");
                    return;
                }

                Run(TypeParser.ParserFoto);
            }
            else if (comboTypeParser.Text.Equals("Парсинг описания"))
            {
                if (textNameFile.Text.Equals(""))
                {
                    MessageBox.Show("Вы не заполнили название файла!");
                    return;
                }

                Run(TypeParser.ParserDescription);
            }

            MessageBox.Show("Идет парсинг. Подождите!");
        }

        private async void Run(TypeParser type)
        {
            string nameCountry = comboNameCountry.Text, pathResult = "";

            pathResult = textPatchFolder.Text[textPatchFolder.Text.Length - 1].Equals(@"\") ? textPatchFolder.Text : textPatchFolder.Text + @"\";
            if (type == TypeParser.ParserFoto)
                pathResult += textNameFolder.Text;
            else
                pathResult += textNameFile.Text;

            await Task.Run(() =>
            {
                if (!Directory.Exists(pathResult))
                    Directory.CreateDirectory(pathResult);

                var cookies = BaseClasse.GetCookies();
                if (cookies.Equals(""))
                    return;

                List<string> filters = BaseClasse.CreateFilters(nameCountry, cookies);
                if (filters == null)
                    return;

                Dictionary<string, string> listNameAndUrl = GetUrl(filters, type, cookies);
                if (listNameAndUrl == null)
                    return;

                if (type == TypeParser.ParserFoto)
                    DownloadImg(listNameAndUrl, pathResult);
                else
                    WriteDescription(listNameAndUrl, pathResult, cookies);

                MessageBox.Show("Скачивание завершено!!!");
            });
        }

        private Dictionary<string, string> GetUrl(List<string> filters, TypeParser type, string cookie)
        {
            HtmlDocument html = new HtmlDocument();
            var listRez = new Dictionary<string, string>();

            try
            {
                foreach (var f in filters)
                {
                    string url = BaseClasse.MethodGET(f, cookie);

                    if (url.Equals(""))
                        continue;

                    if (!url.Contains("http"))
                        continue;

                    while (true)
                    {
                        string s = BaseClasse.MethodGET(url + "load/state/?tours=0&hotels=0&_=" + BaseClasse.GetTimeRequest(), cookie);
                        if (s.Contains("percent\": 100"))
                            break;
                    }

                    string rez = BaseClasse.MethodGET(url + "results/?_=" + BaseClasse.GetTimeRequest(), cookie);
                    if (rez.Contains("По Вашему запросу нет актуальных предложений"))
                        continue;

                    html.LoadHtml(rez);
                    var collection = html.DocumentNode.SelectNodes("//div[@class='row js-hotel hotel-inf-list']");
                    foreach (var item in collection)
                    {
                        string temp = item.SelectSingleNode(".//div[@class='col-md-8']/div/div/h4").InnerText.Replace("\t", "");
                        string[] arr = temp.Split('\n');
                        arr = arr.Where(x => !string.IsNullOrEmpty(x)).ToArray();
                        string name = arr[0].Replace(" Hotel", "").Replace(" ", "_").Replace(" & ", "_");

                        if (listRez.Keys.Contains(name))
                            continue;

                        if (type == TypeParser.ParserFoto)
                        {
                            temp = item.SelectSingleNode(".//div/div/img").Attributes["src"].Value;
                            if (!temp.Equals("/static/img/no-hotel-img.gif"))
                                listRez.Add(name, temp.Substring(0, temp.IndexOf('?')));
                        }
                        else
                            listRez.Add(name, "https://advant.club/hotel/" + item.Attributes["data-id"].Value + "/");
                    }
                    //break;    for test
                }
            }
            catch (Exception e)
            {
                MessageBox.Show(String.Format("Ошибка в получении ссылок на фото!\n{0}", e.ToString()));
                return null;
            }

            return listRez;
        }

        private void DownloadImg(Dictionary<string, string> list, string path)
        {
            using (var webClient = new System.Net.WebClient())
            {
                foreach (var item in list)
                {
                    try
                    {
                        webClient.DownloadFile(item.Value, path + "\\" + item.Key + ".jpg");
                    }
                    catch
                    { }
                }
            }
        }

        private void WriteDescription(Dictionary<string, string> list, string path, string cookie)
        {
            Dictionary<string, string> listRez = new Dictionary<string, string>();
            Cookie ck = new Cookie("sessionid", cookie);

            FirefoxDriverService service = FirefoxDriverService.CreateDefaultService();
            service.HideCommandPromptWindow = true;

            FirefoxOptions options = new FirefoxOptions();
            options.AddArgument("--headless");

            var driver = new FirefoxDriver(service, options);

            driver.Navigate().GoToUrl("http://advant.club/ua/search/");
            Thread.Sleep(1000);
            driver.FindElement(By.Id("id_login_or_email")).SendKeys("testlogin");
            driver.FindElement(By.Id("id_password")).SendKeys("qwerty12");
            driver.FindElement(By.Id("submit")).Click();

            using (StreamWriter sw = new StreamWriter(path, true, Encoding.UTF8))
            {
                foreach (var item in list)
                {
                    driver.Navigate().GoToUrl(item.Value);
                    Thread.Sleep(1000);
                    bool exit = false;
                    string description = "";
                    int count = 0;

                    while(!exit)
                    {
                        try
                        {
                            description = driver.FindElementByClassName("os-tour-description").Text;
                            exit = true;
                        }
                        catch
                        {
                            count++;
                            Thread.Sleep(2000);
                        }
                        finally
                        {
                            if (count == 7)
                                exit = true;
                        }
                    }

                    if (description.Equals(""))
                        continue;

                    sw.WriteLine(String.Format("\t\t\t\t{0}", item.Key));
                    sw.WriteLine(description);
                    sw.WriteLine("\n");
                }
            }
        }
    }
}

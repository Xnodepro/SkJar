using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using System.Windows.Forms;
using System.Threading;
using HtmlAgilityPack;
using System.Drawing;
using System.Data;
using System.Net.Http;
using Newtonsoft.Json;
using System.Net;
using WebSocketSharp;
using System.IO;

namespace CSMONEY
{
    class Work
    {
        IWebDriver driver;
        int ID = 0;

        List<string> me = new List<string>();
        List<ForItemsUSer> them = new List<ForItemsUSer>();
        DatUser ItemsUser;
        Dat ItemsBot;
        string apiKey = Properties.Settings.Default.ApiKey;

        CookieContainer cookies = new CookieContainer();
        HttpClientHandler handler = new HttpClientHandler();
        #region botInventoryStruc
        public struct Dat
        {
            public string cacheKey { get; set; }
            public List<ite> items { get; set; }
        }
        public struct ite
        {

            public string name { get; set; }
            public int bot { get; set; }
            public double price { get; set; }
            public List<it> items { get; set; }
            public string id { get; set; }
        }
        public struct it
        {
            public string id { get; set; }

        }

        #endregion
        #region ForItems
        public struct ForItemsUSer
        {
            public string id { get; set; }
            public int count { get; set; }
        }
        #endregion
        #region UserInventoryStruc
        public struct DatUser
        {
            public List<iteUser> items { get; set; }
        }
        public struct iteUser
        {

            public string name { get; set; }
            public double price { get; set; }
            public List<itUser> items { get; set; }
            public string id { get; set; }
        }
        public struct itUser
        {
            public string id { get; set; }

        }
        #endregion
        //public void start()
        //{
        //    var firstFull = Convert.ToInt32(DateTime.Now.ToString("HHmmss"));
        //    while (true)
        //    {
        //        try
        //        {
        //            while (Program.pauseMoney==true)
        //                Thread.Sleep(200);

        //            var res = ClickItem(ITEMS);
        //            if (res == true)
        //            {
        //                var first = Convert.ToInt32(DateTime.Now.ToString("HHmmss"));
        //            }
        //            if (Convert.ToInt32(DateTime.Now.ToString("HHmmss")) - firstFull > 200)
        //            {
        //                bool cheek = false;
        //                var dt = DataTa.GetTable();
        //                foreach (DataRow item in dt.Rows)
        //                {
        //                    var s = item.ItemArray;
        //                    if (s[0].ToString() == Properties.Settings.Default.name && s[1].ToString() == Properties.Settings.Default.csmoney)
        //                    {
        //                        cheek = true;
        //                        if (s[3].ToString() != Properties.Settings.Default.csmoneyVersion)
        //                        {
        //                            MessageBox.Show("Версия ПО устарела");
        //                            Application.Exit();
        //                        }
        //                        Program.sleepIMONEY = Convert.ToInt32(s[2].ToString());
        //                    }
        //                }
        //                if (cheek == false)
        //                {
        //                    MessageBox.Show("Лицензия не активная.");
        //                    Application.Exit();
        //                }
        //                firstFull = Convert.ToInt32(DateTime.Now.ToString("HHmmss"));
        //                RefreshBotInventory();
        //                Program.Mess.Enqueue("БОТ[" + ID + "] " + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss:fff") + "|" + "{Правило 200сек.}Обновил инвентари!");

        //            }
        //            Thread.Sleep(Program.sleepMSecond);
        //        }
        //        catch (Exception ex) { }
        //    }
        //}
        public Work(int id)
        {
            ID = id;
        }

        public void INI()
        {
            try
            {
                var driverService = ChromeDriverService.CreateDefaultService();  //скрытие 
                driverService.HideCommandPromptWindow = true;                    //консоли
                driver = new ChromeDriver(driverService);
                driver.Navigate().GoToUrl("https://skinsjar.com/ru");
                MessageBox.Show("Введите все данные , после этого программа продолжит работу!");

               
                ////////////////////////////////////Проверка треййд ссылки/////////////////////////////////////////
                driver.Manage().Window.Position = new Point(5000, 5000);
                driver.Navigate().GoToUrl("https://steamcommunity.com/id/me/tradeoffers/privacy#trade_offer_access_url");
                IWebElement offer = driver.FindElement(By.Id("trade_offer_access_url"));
                if (offer.GetAttribute("value") != Properties.Settings.Default.csmoney)
                {
                    MessageBox.Show("Выберите аккаунт к которому привязана программа!");
                    driver.Quit();
                    return;
                }
                ////////////////////////////////////////////////////////////////////////////////////////////////////

                driver.Navigate().GoToUrl("https://skinsjar.com/ru");
                driver.Manage().Window.Position = new Point(0, 0);
                MessageBox.Show("Введите все данные , после этого программа продолжит работу!");
                var _cookies = driver.Manage().Cookies.AllCookies;
                foreach (var item in _cookies)
                {
                    handler.CookieContainer.Add(new System.Net.Cookie(item.Name, item.Value) { Domain = item.Domain });
                }
                //Запуск запросов на отслеживания инвентаря ботов
                for (int i = 0; i < Program.threadCount; i++)
                {
                    new System.Threading.Thread(delegate ()
                    {
                        try
                        {
                            Get(handler);
                        }
                        catch (Exception ex) { }
                    }).Start();
                }
                //запуск отслуживания инвентаря юзера
                for (int i = 0; i < 1; i++)
                {
                    new System.Threading.Thread(delegate () {
                        try
                        {
                            GetInvUser(handler);
                        }
                        catch (Exception ex) { }
                    }).Start();
                }

                //////////////////////////////////////////

                    new System.Threading.Thread(delegate () {
                        while (true)
                        {
                            try
                            {
                                Thread.Sleep(2000);
                                driver.Navigate().Refresh();
                                Thread.Sleep(300000);
                            }
                            catch (Exception ex) { }

                        }
                    }).Start();
                
            }
            catch (Exception ex) { Program.Mess.Enqueue(ex.Message); }

        }
        private void Get(HttpClientHandler handler)
        {
            HttpClientHandler handler1 = handler;
            HttpClient client = null;

            while (true)
            {
                try
                {
                    client = new HttpClient(handler1);
                    client.Timeout = TimeSpan.FromSeconds(30);
                    client.DefaultRequestHeaders.Add("User-Agent",
                    "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/63.0.3239.84 Safari/537.36");
                    client.DefaultRequestHeaders.Add("Accept", "application/json, text/plain, */*");
                    client.DefaultRequestHeaders.Add("Accept-Language", "ru-RU,ru;q=0.8,en-US;q=0.6,en;q=0.4");


                    var response = client.GetAsync("https://skinsjar.com/api/v3/load/bots?refresh=1&v=2").Result;
                    if (response.IsSuccessStatusCode)
                    {
                        var responseContent = response.Content;
                        string responseString = responseContent.ReadAsStringAsync().Result;
                        var ITEMS = JsonConvert.DeserializeObject<Dat>(responseString);
                        ItemsBot = ITEMS;
                  //      ClickItem(ITEMS);
                        Program.Mess.Enqueue("" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss:fff") + "|" + "Завершил загрузку предметов:" + ITEMS.items.Count);
                    }

                    Thread.Sleep(1000);
                }
                catch (Exception ex) { }
            }
            // return new Data();
        }
        private void GetInvUser(HttpClientHandler handler)
        {

            HttpClient client = null;
            HttpClientHandler handler2 = new HttpClientHandler();
            var _cookies = driver.Manage().Cookies.AllCookies;
            foreach (var item in _cookies)
            {
                handler2.CookieContainer.Add(new System.Net.Cookie(item.Name, item.Value) { Domain = item.Domain });
            }
            while (true)
            {
                try
                {
                    client = new HttpClient(handler2);
                    client.Timeout = TimeSpan.FromSeconds(30);
                    client.DefaultRequestHeaders.Add("User-Agent",
                    "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/63.0.3239.84 Safari/537.36");
                    client.DefaultRequestHeaders.Add("Accept", "application/json, text/plain, */*");
                    client.DefaultRequestHeaders.Add("Accept-Language", "ru-RU,ru;q=0.8,en-US;q=0.6,en;q=0.4");


                    var response = client.GetAsync("https://skinsjar.com/api/v3/load/inventory?refresh=1&v=2").Result;
                    if (response.IsSuccessStatusCode)
                    {
                        var responseContent = response.Content;
                        string responseString = responseContent.ReadAsStringAsync().Result;
                        ItemsUser = JsonConvert.DeserializeObject<DatUser>(responseString);
                           Program.Mess.Enqueue("" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss:fff") + "|" + "Завершил загрузку предметов:" + ItemsUser.items.Count);
                    }

                    Thread.Sleep(5000);
                }
                catch (Exception ex) { }
            }
            // return new Data();
        }
        private void AutoSelect( string botId,string itemsId)
        {
            HttpClientHandler handler = new HttpClientHandler();
            var _cookies = driver.Manage().Cookies.AllCookies;
            foreach (var item in _cookies)
            {
                handler.CookieContainer.Add(new System.Net.Cookie(item.Name, item.Value) { Domain = item.Domain });
            }
                try
                {
                    HttpClient client = null;
                    client = new HttpClient(handler);
                    client.Timeout = TimeSpan.FromSeconds(60);
                    client.DefaultRequestHeaders.Add("User-Agent",
                                                     "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/63.0.3239.84 Safari/537.36");
                    client.DefaultRequestHeaders.Add("Accept", "application/json, text/plain, */*");
                    client.DefaultRequestHeaders.Add("accept-language", "ru-RU,ru;q=0.9,en-US;q=0.8,en;q=0.7");
                    client.DefaultRequestHeaders.Add("x-requested-with", "XMLHttpRequest");
                    StringContent content = new StringContent("{\"itemsFromMe\":[\"" +itemsId+ "\"],\"itemsFromThem\":[],\"bot\":" +botId +",\"isSocketConnected\":true,\"resetSession\":true,\"overstockBots\":[]}", Encoding.UTF8, "application/json");
                    
                     var response = client.PostAsync("https://skinsjar.com/api/autoSelect?v=2", content).Result;
                  
                     if (response.IsSuccessStatusCode)
                     {
                        var responseContent = response.Content;
                        string responseString = responseContent.ReadAsStringAsync().Result;
                        var ITEMS = JsonConvert.DeserializeObject<dynamic>(responseString);
                        GetItemsAfterAutoSelect(ITEMS);
                     }
                }
                catch (Exception ex) { }

        }
        private void SendOffer(string ContentPost)
        {
            HttpClientHandler handler = new HttpClientHandler();
            var _cookies = driver.Manage().Cookies.AllCookies;
            foreach (var item in _cookies)
            {
                handler.CookieContainer.Add(new System.Net.Cookie(item.Name, item.Value) { Domain = item.Domain });
            }
            try
            {
                HttpClient client = null;
                client = new HttpClient(handler);
                client.Timeout = TimeSpan.FromSeconds(60);
                client.DefaultRequestHeaders.Add("User-Agent",
                                                 "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/63.0.3239.84 Safari/537.36");
                client.DefaultRequestHeaders.Add("Accept", "application/json, text/plain, */*");
                client.DefaultRequestHeaders.Add("accept-language", "ru-RU,ru;q=0.9,en-US;q=0.8,en;q=0.7");
                client.DefaultRequestHeaders.Add("x-requested-with", "XMLHttpRequest");
                StringContent content = new StringContent(ContentPost, Encoding.UTF8, "application/json");
                
                var response = client.PostAsync("https://skinsjar.com/api/items/trade?v=2", content).Result;

                if (response.IsSuccessStatusCode)
                {
                    var responseContent = response.Content;
                    string responseString = responseContent.ReadAsStringAsync().Result;
                    var ITEMS = JsonConvert.DeserializeObject<dynamic>(responseString);
                }
            }
            catch (Exception ex) { }

        }
        private string GetContentToOffer(string botId)
        {
            string meStr = "";
            string themStr = "";
            foreach (var item in me)
            {
                try
                {
                    var ss = ItemsBot.items.Find(N => (N.id == item)).items[0].id;
                    meStr += "\\\"" + ss + "\\\",";
                }
                catch (Exception ex) { }
            }
            var a = meStr;
            meStr = meStr.Remove(meStr.Length - 1, 1);
            foreach (var item in them)
            {
                try
                {
                    for (int i = 0; i < item.count; i++)
                    {
                        var ss = ItemsUser.items.Find(N => (N.id == item.id)).items[i].id;
                        themStr += "\\\"" + ss + "\\\",";
                    }
                   
                }catch(Exception ex) {
                    string ss = "";
                }
            }
            themStr = themStr.Remove(themStr.Length - 1, 1);
            
            return "{\\\"itemsFromThem\\\":["+ themStr + "],\\\"bot\\\":"+ botId + ",\\\"itemsFromMe\\\":["+ meStr + "],\\\"isSocketConnected\\\":true}";

        }
        private void GetItemsAfterAutoSelect(dynamic itm)
        {
            foreach (var item in itm.me)
            {

                    for (int i = 0; i < (int)item.Value; i++)
                    {
                        me.Add(item.Name);
                    }

            }
            foreach (var item in itm.them)
            {

                    ForItemsUSer FIU = new ForItemsUSer {
                        id = item.Name,
                        count = (int)item.Value
                    };
                    them.Add(FIU);
                
            }
        }
        private bool ClickItem(Dat json)
        {

            try
            {
                var da = json;
                foreach (var item in da.items)
                {

                    foreach (var name in Program.Data)
                    {
                        if (item.name.Replace(" ", "") == (name.Name).Replace(" ", ""))
                        {
                            Program.Mess.Enqueue("БОТ[" + ID + "] " + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss:fff") + "| Нашел предмет :" + item.name + "|Цена_Сайта:" + item.price + "|Цена_Наша:" + name.Price);
                            if (item.price <= name.Price)
                            {
                                me.Clear();
                                them.Clear();
                                Program.Mess.Enqueue("БОТ[" + ID + "] " + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss:fff") + "| Оправил Запрос |" + "|IDItems:" + item.items[0].id.ToString() + "|wallet:" + item.price.ToString());
                                AutoSelect(item.bot.ToString(), item.items[0].id.ToString());
                                Program.Mess.Enqueue(  DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss:fff") + "Сделал Автоподбор!:" );
                                string ContentUs = GetContentToOffer(item.bot.ToString());
                                // SendOffer(ContentUs);
                                IJavaScriptExecutor js = driver as IJavaScriptExecutor;
                                //string querySelect = "var xhr = new XMLHttpRequest(); var body = \"" + "{\\\"itemsFromMe\\\":[\\\"" + item.items[0].id.ToString() + "\\\"],\\\"itemsFromThem\\\":[],\\\"bot\\\":" + item.bot.ToString() + ",\\\"isSocketConnected\\\":true,\\\"resetSession\\\":true,\\\"overstockBots\\\":[]}" + "\"; xhr.open(\"POST\", 'https://skinsjar.com/api/autoSelect?v=2', true); xhr.setRequestHeader('Content-Type', 'application/json;charset=UTF-8'); xhr.setRequestHeader('x-requested-with', 'XMLHttpRequest'); xhr.setRequestHeader('accept', 'application/json, text/plain, */*');xhr.setRequestHeader('Custom-Params', '');  xhr.send(body); ";
                                //string titleSelect = (string)js.ExecuteScript(querySelect);
                                //Thread.Sleep(2000);
                                //string queryLocked = "var xhr = new XMLHttpRequest(); xhr.open(\"GET\", 'https://skinsjar.com/api/sockets/getLocked?full=true&v=2', true);  xhr.setRequestHeader('x-requested-with', 'XMLHttpRequest'); xhr.setRequestHeader('accept', 'application/json, text/plain, */*'); xhr.setRequestHeader('Custom-Params', ''); xhr.send(); ";
                                //string titleLocked = (string)js.ExecuteScript(queryLocked);
                                //Thread.Sleep(2000);
                                //string queryBalance = "var xhr = new XMLHttpRequest(); xhr.open(\"GET\", 'https://skinsjar.com/api/accounts/balance?v=2', true); xhr.setRequestHeader('Content-Type', 'application/json;charset=UTF-8'); xhr.setRequestHeader('x-requested-with', 'XMLHttpRequest'); xhr.setRequestHeader('accept', 'application/json, text/plain, */*');xhr.setRequestHeader('Custom-Params', '');  xhr.send(); ";
                                //string titleBalance = (string)js.ExecuteScript(queryBalance);
                                //Thread.Sleep(2000);
                                string query = "var xhr = new XMLHttpRequest();var body = \"" + ContentUs + "\"; xhr.open(\"POST\", 'https://skinsjar.com/api/items/trade?v=2', true); xhr.setRequestHeader('Content-Type', 'application/json;charset=UTF-8'); xhr.setRequestHeader('x-requested-with', 'XMLHttpRequest'); xhr.setRequestHeader('accept', 'application/json, text/plain, */*'); xhr.setRequestHeader('Custom-Params', 'eyJib3RzSW52ZW50b3J5VGltZVN0YW1wIjoxNTEzMzc2MTA2LCJjYWNoZUtleSI6ImRlYnVnOnE0bDFLZlRhSnlZckE1bktoWmMrNVE9PX5+fjp+fn5vZVNmcnFHeGVTSGxYd0xlU0ZGNVE3bklGbjZGL0FTMkdobG02U2ptK01FL3BhWnF0QURnTkFlRVl6ajNaWnEvY1pwNUc4ckJaNTB0ZnUvMjNRVktrVUpzUENuYm04STVjSndvaHllRnFDNW43TTNpbzA1L2RVUEN1VFd4S3pCdiIsImFjY291bnRCYWxhbmNlQW1vdW50IjowLCJhY2NvdW50QmFsYW5jZUxvY2tlZEFtb3VudCI6MCwiYWNjb3VudEJhbGFuY2VUb3RhbEFtb3VudCI6MCwiY3VycmVuY3lTeW1ib2wiOiJVU0QiLCJjdXJyZW5jeU11bHRpcGxpZXIiOjF9'); xhr.send(body); ";
                                string title = (string)js.ExecuteScript(query);
                                Program.Mess.Enqueue(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss:fff") + "Завершил все запросы!");
                                Thread.Sleep(3000);
                                return false;

                            }
                            else {
                                SetListBadPrice(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss:fff"), "cs.money", item.name.ToString() , name.Price.ToString(), item.price.ToString());
                            }

                        }
                    }

                }
            }
            catch (Exception ex) { Program.Mess.Enqueue("БОТ[" + ID + "] " + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss:fff") + "|Ошибка2 :" + ex.Message); }
            return false;
        }
       
     
        private void SetListBadPrice(string _Data, string _Site, string _Name, string _OldPrice, string _NewPrice)
        {
            Program.DataViewBadPrice item = new Program.DataViewBadPrice()
            {
                Date = _Data,
                Site = _Site,
                Name =_Name,
                OldPrice = _OldPrice,
                NewPrice =_NewPrice
            };
            Program.BadPrice.Add(item);
        }
    }
}

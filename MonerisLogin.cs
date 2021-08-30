using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace MonerisLogin
{
    class MonerisLogin
    {
        // Merchant Resource Center
        private static string mrcDomain = "www3.moneris.com";
        private static string mrcUrl = "https://" + mrcDomain;
        private static string mrcUsername = Settings.Default.mrcUsername; // use you Username for Merchant Resource Center
        private static string mrcPassword = Settings.Default.mrcPassword; // use your Password for Merchant Resource Center 
        private static string mrcStoreId = Settings.Default.storeId; // use your Store ID for Merchant Resource Center

        private const string mrcLoginRelativePath = "/mpg/index.php";
        private const string mrcTransactionReportPageRelativePath = "/mpg/reports/transaction/index.php";
        private const string mrcTransactionReportDownloadRelativePath = "/mpg/include/export_txn.php";

        // Merchant Direct
        private static string mdDomain = "www1.moneris.com";
        private static string mdUrl = "https://" + mdDomain;
        private static string mdUsername = Settings.Default.mdUsername; // use User ID for Merchant Direct
        private static string mdPassword = Settings.Default.mdPassword; // use Password for Merchant Direct
        private static string mdAccountNumber = Settings.Default.mdAccount;

        private const string mdLoginRelativePath = "/cgi-bin/rbaccess/rbunxcgi?F6=1&F7=L8&F21=PB&F22=L8&REQUEST=ClientSignin&LANGUAGE=ENGLISH";
        private const string mdTransactionReportDownloadRelativePath = "/cgi-bin/rbaccess/rbunxcgi";


        static async System.Threading.Tasks.Task Main(string[] args)
        {
            // Create HttpClient with cookies of the account logged into Merchant Resource Center
            HttpClient mrcHttpClient = MrcLogin().Result;
            // TODO other requests using mdHttpClient, e.g. download transactions

            // Create HttpClient with cookies of the account logged into Merchant Direct
            HttpClient mdHttpClient = MdLogin().Result;
            // TODO other requests using mdHttpClient, e.g. download transactions

        }



        private static async Task<HttpClient> MrcLogin()
        {
            string loginPageUrl = mrcUrl + mrcLoginRelativePath;
            CookieContainer cookieContainer = new CookieContainer();
            HttpClientHandler httpClientHandler = new HttpClientHandler
            {
                UseCookies = true,
                CookieContainer = cookieContainer,
                AllowAutoRedirect = false // Note: We do not need follow redirects (302) after successful login. We can use it to varify successful login.
            };
            HttpClient httpClient = new HttpClient(httpClientHandler);

            // GET login page
            HttpRequestMessage loginPageRequestMessage = new HttpRequestMessage
            {
                Method = HttpMethod.Get,
                RequestUri = new Uri(loginPageUrl),
                Headers = {
                    { HttpRequestHeader.Connection.ToString(), "keep-alive" },
                    { HttpRequestHeader.Pragma.ToString(), "no-cache" },
                    { HttpRequestHeader.CacheControl.ToString(), "no-cache" },
                    { "sec-ch-ua", "\"Chromium\";v=\"92\", \" Not A; Brand\";v=\"99\", \"Google Chrome\";v=\"92\"" },
                    { "sec-ch-ua-mobile", "?0" },
                    { "Upgrade-Insecure-Requests", "1" },
                    { HttpRequestHeader.UserAgent.ToString(), "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/92.0.4515.131 Safari/537.36" },
                    { HttpRequestHeader.Accept.ToString(), "text/html,application/xhtml+xml,application/xml;q=0.9,image/avif,image/webp,image/apng,*/*;q=0.8,application/signed-exchange;v=b3;q=0.9" },
                    { "Sec-Fetch-Site", "same-origin" },
                    { "Sec-Fetch-Mode", "navigate" },
                    { "Sec-Fetch-User", "?1" },
                    { "Sec-Fetch-Dest", "document" },
                    { HttpRequestHeader.Referer.ToString(), loginPageUrl },
                    { HttpRequestHeader.AcceptLanguage.ToString(), "en-US,en;q=0.9" },
                },
            };
            HttpResponseMessage loginPageResponse = await httpClient.SendAsync(loginPageRequestMessage);

            string loginFormContent = await loginPageResponse.Content.ReadAsStringAsync();
            HtmlDocument loginHtmlDoc = new HtmlDocument();
            loginHtmlDoc.LoadHtml(loginFormContent);
            HtmlNodeCollection loginFormNodes = loginHtmlDoc.DocumentNode.SelectNodes("//form[@name='login']");
            if (loginFormNodes.Count != 1)
            {
                Console.Error.WriteLine("The structure of the MRC login page changed.");
                return null;
            }
            HtmlNode loginFormNode = loginFormNodes.ElementAt(0);
            HtmlNodeCollection loginInputnodes = loginFormNode.SelectNodes(".//input[@class='textbox']");
            if (loginInputnodes.Count != 3)
            {
                Console.Error.WriteLine("The structure of the MRC login page changed.");
                return null;
            }
            HtmlNode usernameInput = loginInputnodes.ElementAt(0);
            HtmlNode storeIdInput = loginInputnodes.ElementAt(1);
            HtmlNode passwordInput = loginInputnodes.ElementAt(2);

            // POST authentication request
            FormUrlEncodedContent loginFormRequestBody = new FormUrlEncodedContent(new[]
            {
                new KeyValuePair<string, string>(usernameInput.Attributes["name"].Value, mrcUsername),
                new KeyValuePair<string, string>(storeIdInput.Attributes["name"].Value, mrcStoreId),
                new KeyValuePair<string, string>(passwordInput.Attributes["name"].Value, mrcPassword),
                new KeyValuePair<string, string>("do_login", "Submit")
            });
            HttpRequestMessage loginFormRequestMessage = new HttpRequestMessage
            {
                Method = HttpMethod.Post,
                RequestUri = new Uri(loginPageUrl),
                Headers = {
                    { HttpRequestHeader.Connection.ToString(), "keep-alive" },
                    { HttpRequestHeader.Pragma.ToString(), "no-cache" },
                    { HttpRequestHeader.CacheControl.ToString(), "no-cache" },
                    { "sec-ch-ua", "\"Chromium\";v=\"92\", \" Not A; Brand\";v=\"99\", \"Google Chrome\";v=\"92\"" },
                    { "sec-ch-ua-mobile", "?0" },
                    { "Upgrade-Insecure-Requests", "1" },
                    { "Origin", mrcUrl },
                    { HttpRequestHeader.ContentType.ToString(), "application/x-www-form-urlencoded" },
                    { HttpRequestHeader.UserAgent.ToString(), "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/92.0.4515.131 Safari/537.36" },
                    { HttpRequestHeader.Accept.ToString(), "text/html,application/xhtml+xml,application/xml;q=0.9,image/avif,image/webp,image/apng,*/*;q=0.8,application/signed-exchange;v=b3;q=0.9" },
                    { "Sec-Fetch-Site", "same-origin" },
                    { "Sec-Fetch-Mode", "navigate" },
                    { "Sec-Fetch-User", "?1" },
                    { "Sec-Fetch-Dest", "document" },
                    { HttpRequestHeader.Referer.ToString(), loginPageUrl },
                    { HttpRequestHeader.AcceptLanguage.ToString(), "en-US,en;q=0.9" },
                },
                Content = loginFormRequestBody
            };
            HttpResponseMessage loginFormResponse = await httpClient.SendAsync(loginFormRequestMessage);
            if (loginFormResponse.StatusCode != HttpStatusCode.Found /*302*/)
            {
                Console.Error.WriteLine("Cannot login. Wrong credentials?");
                //Console.Error.WriteLine(await loginFormResponse.Content.ReadAsStringAsync());
                return null;
            }
            return httpClient;
        }


        private static async Task<HttpClient> MdLogin()
        {
            string loginPageUrl = mdUrl + mdLoginRelativePath;
            CookieContainer cookieContainer = new CookieContainer();
            HttpClientHandler httpClientHandler = new HttpClientHandler
            {
                UseCookies = true,
                CookieContainer = cookieContainer,
                AllowAutoRedirect = false // Note: We do not need follow redirects (302) after successful login. We can use it to varify successful login.
            };
            HttpClient httpClient = new HttpClient(httpClientHandler);

            // GET login page
            HttpRequestMessage loginPageRequestMessage = new HttpRequestMessage
            {
                Method = HttpMethod.Get,
                RequestUri = new Uri(loginPageUrl),
                Headers = {
                    { HttpRequestHeader.Connection.ToString(), "keep-alive" },
                    { HttpRequestHeader.Pragma.ToString(), "no-cache" },
                    { HttpRequestHeader.CacheControl.ToString(), "no-cache" },
                    { "sec-ch-ua", "\"Chromium\";v=\"92\", \" Not A; Brand\";v=\"99\", \"Google Chrome\";v=\"92\"" },
                    { "sec-ch-ua-mobile", "?0" },
                    { "Upgrade-Insecure-Requests", "1" },
                    { HttpRequestHeader.UserAgent.ToString(), "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/92.0.4515.131 Safari/537.36" },
                    { HttpRequestHeader.Accept.ToString(), "text/html,application/xhtml+xml,application/xml;q=0.9,image/avif,image/webp,image/apng,*/*;q=0.8,application/signed-exchange;v=b3;q=0.9" },
                    { "Sec-Fetch-Site", "same-origin" },
                    { "Sec-Fetch-Mode", "navigate" },
                    { "Sec-Fetch-User", "?1" },
                    { "Sec-Fetch-Dest", "document" },
                    { HttpRequestHeader.Referer.ToString(), loginPageUrl },
                    { HttpRequestHeader.AcceptLanguage.ToString(), "en-US,en;q=0.9" },
                },
            };
            HttpResponseMessage loginPageResponse = await httpClient.SendAsync(loginPageRequestMessage);
            string loginPageContent = await loginPageResponse.Content.ReadAsStringAsync();

            HtmlDocument loginHtmlDoc = new HtmlDocument();
            loginHtmlDoc.LoadHtml(loginPageContent);
            HtmlNodeCollection loginFormNodes = loginHtmlDoc.DocumentNode.SelectNodes("//form[@id='signIn']");
            if (loginFormNodes.Count != 1)
            {
                Console.Error.WriteLine("The structure of the MD login page changed.");
                return null;
            }
            HtmlNode loginFormNode = loginFormNodes.ElementAt(0);
            HtmlNodeCollection loginInputNodes = loginFormNode.SelectNodes(".//input[@type='HIDDEN']");
            List<KeyValuePair<string, string>> authRequestInputs = new List<KeyValuePair<string, string>>();
            bool containsSST = false;
            foreach (HtmlNode inputNode in loginInputNodes)
            {
                string inputName = inputNode.Attributes["name"].Value;
                string inputValue = inputNode.Attributes["value"].Value;
                authRequestInputs.Add(new KeyValuePair<string, string>(inputName, inputValue));
                containsSST = "SST".Equals(inputName) ? true : containsSST;
            }
            if (!containsSST)
            {
                Console.Error.WriteLine("The structure of the MD login page changed but it may work...");
            }
            authRequestInputs.Add(new KeyValuePair<string, string>("USERID", mdUsername));
            authRequestInputs.Add(new KeyValuePair<string, string>("PASSWORD", mdPassword));

            // POST authentication request
            FormUrlEncodedContent loginFormRequestBody = new FormUrlEncodedContent(authRequestInputs);
            HttpRequestMessage loginFormRequestMessage = new HttpRequestMessage
            {
                Method = HttpMethod.Post,
                RequestUri = new Uri(loginPageUrl),
                Headers = {
                    { HttpRequestHeader.Connection.ToString(), "keep-alive" },
                    { HttpRequestHeader.CacheControl.ToString(), "no-cache" },
                    { HttpRequestHeader.Pragma.ToString(), "no-cache" },
                    { "authority", mdDomain },
                    //{ "sec-ch-ua", "\"Chromium\";v=\"92\", \" Not A; Brand\";v=\"99\", \"Google Chrome\";v=\"92\"" },
                    //{ "sec-ch-ua-mobile", "?0" },
                    { "Upgrade-Insecure-Requests", "1" },
                    { "Origin", mdUrl },
                    { HttpRequestHeader.ContentType.ToString(), "application/x-www-form-urlencoded" },
                    { HttpRequestHeader.UserAgent.ToString(), "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/92.0.4515.131 Safari/537.36" },
                    { HttpRequestHeader.Accept.ToString(), "text/html,application/xhtml+xml,application/xml;q=0.9,image/avif,image/webp,image/apng,*/*;q=0.8,application/signed-exchange;v=b3;q=0.9" },
                    { HttpRequestHeader.AcceptEncoding.ToString(), "gzip, deflate, br" },
                    { "Sec-Fetch-Site", "same-origin" },
                    { "Sec-Fetch-Mode", "navigate" },
                    { "Sec-Fetch-User", "?1" },
                    { "Sec-Fetch-Dest", "document" },
                    { HttpRequestHeader.Referer.ToString(), loginPageUrl }, // or https://www1.moneris.com/cgi-bin/rbaccess/rbunxcgi?F6=1&F7=L8&F21=PB&F22=L8&REQUEST=ClientSignin&LANGUAGE=ENGLISH
                    { HttpRequestHeader.AcceptLanguage.ToString(), "en-US,en;q=0.9" },
                },
                Content = loginFormRequestBody
            };
            HttpResponseMessage loginFormResponse = await httpClient.SendAsync(loginFormRequestMessage);
            if (loginFormNodes.Count != 1)
            {
                Console.Error.WriteLine("The structure of the MD login page changed.");
                return null;
            }
            return httpClient;
        }
    }
}

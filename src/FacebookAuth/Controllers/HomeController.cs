using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Net;
using System.Text;
using System.IO;
using System.Collections.Specialized;

namespace FacebookAuth.Controllers
{
    public class HomeController : Controller
    {
        /// <summary>
        /// 你申請的 client_id
        /// </summary>
        private const string client_id = "{client_id}";
        /// <summary>
        /// 你申請的 client_secret
        /// </summary>
        private const string client_secret = "{client_id}";
        /// <summary>
        /// 申請時候設定的回傳網址
        /// </summary>
        private const string redirect_uri = "{redirect_uri}";

        public ActionResult Index()
        {
            // 省略了 response_type 參數
            // 就不會授權過還會在出現授權畫面
            string Url = "https://www.facebook.com/dialog/oauth?scope={0}&redirect_uri={1}&client_id={2}&state={3}";
            // email 是取得信箱的權限
            // read_stream 是讀取動態牆的權限
            // 權限之間用逗號(,)做分隔
            string scope = "email,read_stream";
            string redirect_uri_encode = Utitity.UrlEncode(redirect_uri);
            string state = "";

            Response.Redirect(string.Format(Url, scope, redirect_uri_encode, client_id, state));

            return null;
        }

        public ActionResult CallBack(string Code)
        {
            // 沒有接收到參數
            if (string.IsNullOrEmpty(Code))
                return Content("沒有收到 Code");

            // 沒有 grant_type 參數
            string Url = "https://graph.facebook.com/oauth/access_token?code={0}&client_id={1}&client_secret={2}&redirect_uri={3}";
            string redirect_uri_encode = Utitity.UrlEncode(redirect_uri);

            HttpWebRequest request = HttpWebRequest.Create(string.Format(Url,Code, client_id, client_secret, redirect_uri_encode)) as HttpWebRequest;
            string result = null;
            request.Method = "Get";    // 方法
            request.KeepAlive = true; //是否保持連線

            using (WebResponse response = request.GetResponse())
            {
                StreamReader sr = new StreamReader(response.GetResponseStream());
                result = sr.ReadToEnd();
                sr.Close();
            }
            // 回傳的網頁格式會是 QueryString 格式
            // access_token={access_token}&expires={expires}
            NameValueCollection qscoll = HttpUtility.ParseQueryString(result);

            Session["token"] = qscoll["access_token"];  // 記錄 access_token

            // 這邊不建議直接把 Token 當做參數傳給 CallAPI 可以避免 Token 洩漏
            return RedirectToAction("CallAPI");
        }

        public ActionResult CallAPI()
        {
            if (Session["token"] == null)
                return Content("請先取得授權！");

            string token = Session["token"] as string;
            // 取得個人資料的 API 網址
            string Url = "https://graph.facebook.com/me?access_token=" + token;
            HttpWebRequest request = HttpWebRequest.Create(Url) as HttpWebRequest;
            string result = null;
            request.Method = "GET";    // 方法
            request.KeepAlive = true; //是否保持連線

            using (WebResponse response = request.GetResponse())
            {
                StreamReader sr = new StreamReader(response.GetResponseStream());
                result = sr.ReadToEnd();
                sr.Close();
            }

            Response.Write(result);

            return null;
        }
    }
}

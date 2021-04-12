using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using VimeoDotNet.Models;
using VimeoDotNet.Net;

namespace VimeoTest.Controllers
{
    public class HomeController : Controller
    {
        // GET: Home
        public ActionResult UploadVideo()
        {
           return View();
        }

        [HttpPost]
        public void SaveVideoToVimeo(HttpPostedFileBase file)
        {

            try
            {
               var fileName = Path.GetFileName(file.FileName);
               var path = Path.Combine(Server.MapPath("~/UploadVideos"), fileName);
               file.SaveAs(path);

                string vVimeURL = "https://api.vimeo.com/me/videos";
                WebClient wc = new WebClient();
                wc.Headers.Clear();
                wc.Headers.Add("Authorization", "bearer TokenGeneratedFromYourAccount");
                wc.Headers.Add("Content-Type", "application/json");
                wc.Headers.Add("Accept", "application/vnd.vimeo.*+json;version=3.4");
                wc.Encoding = System.Text.Encoding.UTF8;
                string vData = "{\"upload\": {" + "\"approach\":\"tus\"," + "\"size\":\"" + file.InputStream.Length + "\"}}";
                var result = wc.UploadString(vVimeURL, "POST", vData);
                var vimeoTicket = JsonConvert.DeserializeObject<JObject>(result,
                        new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore });
                var LocationLink = vimeoTicket["upload"];
                wc.Headers.Clear();
                wc.Headers.Add("Content-Type", "application/offset+octet-stream");
                wc.Headers.Add("Accept", "application/vnd.vimeo.*+json;version=3.4");
                wc.Headers.Add("Tus-Resumable", "1.0.0");
                wc.Headers.Add("Upload-Offset", file.InputStream.Length.ToString());

                string vupload_link = LocationLink["upload_link"].ToString(); ;
                byte[] vResponse = wc.UploadFile(vupload_link, "PATCH", path);

            }
            catch (Exception h)
            {
                throw;
            }
        }

    }
}

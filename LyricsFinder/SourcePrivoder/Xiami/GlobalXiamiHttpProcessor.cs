using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace LyricsFinder.SourcePrivoder.Xiami
{
    public static class GlobalXiamiHttpProcessor
    { 
        private static Task updatingTask;
        private static string currentXmSgTk;
        private static CookieContainer cookieContainer = new CookieContainer();
        private const string xmsgtkCookieKey = "xm_sg_tk=";

        private static MD5 md5 = MD5.Create();

        public static async ValueTask<JObject> ProcessRequestAsync(string apiPath, string paramQ = "", string apiBasePath = "https://www.xiami.com", CancellationToken cancellationToken = default, int retry = 5)
        {
            if (retry == 0)
                return default;

            if (updatingTask is Task pending)
                await pending;

            var str = $"{await GetXmSgTk()}_xmMain_{apiPath}_{paramQ}";
            if (cancellationToken.IsCancellationRequested)
                return default;

            var paramS = BitConverter.ToString(md5.ComputeHash(Encoding.UTF8.GetBytes(str))).Replace("-", "").ToLowerInvariant(); ;
            var fullUrl = $"{apiBasePath}{apiPath}?_q={WebUtility.UrlEncode(paramQ)}&_s={paramS}";
            Utils.Output("paramQ : " + paramQ);
            Utils.Output("paramS : " + paramS);
            Utils.Output("fullUrl : " + fullUrl);

            var req = WebRequest.CreateHttp(fullUrl);
            req.CookieContainer = cookieContainer;
            req.AutomaticDecompression = DecompressionMethods.GZip;
            req.Accept = "text/html,application/xhtml+xml,application/xml;q=0.9,image/webp,*/*;q=0.8";
            req.Headers["Accept-Language"] = "text/html,application/xhtml+xml,application/xml;q=0.9,image/webp,*/*;q=0.8";
            //req.Connection = "keep-alive";
            req.Host = "www.xiami.com";
            req.Headers["TE"] = "Trailers";
            req.Headers["Upgrade-Insecure-Requests"] = "1";
            req.UserAgent = "Mozilla/5.0 (Windows NT 6.1; Win64; x64; rv:74.0) Gecko/20100101 Firefox/74.0";
            req.Headers["Accept-Encoding"] = "gzip";

            if (GlobalSetting.SearchAndDownloadTimeout > 0)
                req.Timeout = GlobalSetting.SearchAndDownloadTimeout;

            var response = await req.GetResponseAsync();
            if (cancellationToken.IsCancellationRequested)
                return default;

            using var reader = new StreamReader(response.GetResponseStream());
            var content = await reader.CancelableReadToEndAsync(cancellationToken);
            if (cancellationToken.IsCancellationRequested)
                return default;

            var json = JObject.Parse(content);
            var code = json["code"].ToString().ToUpper();
            if (code == "SUCCESS")
                return json;

            if (code == "SG_TOKEN_EXPIRED")
            {
                //令牌过期 , 更新一下
                Utils.Output("xiami response SG_TOKEN_EXPIRED");
                await UpdateXmSgTk();
                return await ProcessRequestAsync(apiPath, paramQ, apiBasePath, cancellationToken,retry--);
            }

            return default;
        }

        private static async ValueTask<string> GetXmSgTk()
        {
            if (currentXmSgTk is null)
                updatingTask = UpdateXmSgTk();

            if (updatingTask is Task pending)
                await pending;

            return currentXmSgTk;
        }

        private static async Task UpdateXmSgTk()
        {
            Utils.Output("trying to get new XmSgTk for xiami provider...");

            var req = WebRequest.CreateHttp("https://www.xiami.com/");
            req.CookieContainer = cookieContainer;
            var resp = await req.GetResponseAsync();

            currentXmSgTk = resp.Headers.GetValues("Set-Cookie")
                .Where(x => x.StartsWith(xmsgtkCookieKey))
                .Select(x => x.Substring(xmsgtkCookieKey.Length).Split("_").FirstOrDefault())
                .FirstOrDefault()?.ToLowerInvariant();

            Debug.Assert(!string.IsNullOrWhiteSpace(currentXmSgTk), "currentXmSgTk is invaild after XmSgTk request.");
            Utils.Output("currentXmSgTk : " + currentXmSgTk);

            updatingTask = null;
        }
    }
}

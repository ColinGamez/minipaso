using System;
using System.Diagnostics;
using System.IO;
using System.Management;
using System.Net;
using System.Text;
using System.Xml;

namespace Minipaso
{
    /// <summary>
    /// Submits scores to the website's leaderboard worker.
    /// Sends the six WEI subscores + manufacturer/model + nickname.
    /// Deliberately NOT the drive serials the 2007 gadget uploaded.
    /// </summary>
    public static class Reporter
    {
        // TODO: point this at your deployed worker, e.g.
        // https://colingamez-r2-worker.yourname.workers.dev
        private const string WorkerUrl = "https://colingamez-r2-worker.colingames.workers.dev";

        public static string RankingPageUrl()
        {
            return "https://ColinGamez.github.io/ColinGamez-website/minipaso/";
        }

        public static void OpenRankingPage()
        {
            Process.Start(RankingPageUrl());
        }

        public static string MachineModel()
        {
            try
            {
                ManagementObjectSearcher searcher = new ManagementObjectSearcher(
                    "SELECT Manufacturer, Model FROM Win32_ComputerSystem");
                foreach (ManagementObject mo in searcher.Get())
                {
                    object maker = mo["Manufacturer"];
                    object model = mo["Model"];
                    string a = (maker == null) ? "" : maker.ToString().Trim();
                    string b = (model == null) ? "" : model.ToString().Trim();
                    if (a.Length > 0 || b.Length > 0)
                    {
                        return (a + " " + b).Trim();
                    }
                }
            }
            catch (ManagementException)
            {
            }
            return "Unknown PC";
        }

        /// <summary>
        /// POSTs the vote XML. Returns the server's vote id on success.
        /// </summary>
        public static string Vote(WinSatScores scores, string nickname)
        {
            XmlDocument xml = new XmlDocument();
            XmlElement root = xml.CreateElement("MiniPasoVote");
            xml.AppendChild(root);

            AddText(xml, root, "Nickname", nickname);
            AddText(xml, root, "Model", MachineModel());
            AddText(xml, root, "SystemScore", scores.SystemScore.ToString("0.0"));
            AddText(xml, root, "CpuScore", scores.CpuScore.ToString("0.0"));
            AddText(xml, root, "MemoryScore", scores.MemoryScore.ToString("0.0"));
            AddText(xml, root, "GraphicsScore", scores.GraphicsScore.ToString("0.0"));
            AddText(xml, root, "GamingScore", scores.GamingScore.ToString("0.0"));
            AddText(xml, root, "DiskScore", scores.DiskScore.ToString("0.0"));
            AddText(xml, root, "MeasuredAt", scores.MeasuredAt.ToString("o"));

            byte[] body = Encoding.UTF8.GetBytes(xml.OuterXml);
            HttpWebRequest req = (HttpWebRequest)WebRequest.Create(WorkerUrl + "/api/minipaso/vote");
            req.Method = "POST";
            req.ContentType = "text/xml; charset=utf-8";
            req.ContentLength = body.Length;
            using (Stream stream = req.GetRequestStream())
            {
                stream.Write(body, 0, body.Length);
            }
            using (HttpWebResponse res = (HttpWebResponse)req.GetResponse())
            {
                using (StreamReader reader = new StreamReader(res.GetResponseStream(), Encoding.UTF8))
                {
                    return reader.ReadToEnd().Trim();
                }
            }
        }

        private static void AddText(XmlDocument xml, XmlElement parent, string tag, string text)
        {
            XmlElement el = xml.CreateElement(tag);
            el.AppendChild(xml.CreateTextNode(text == null ? "" : text));
            parent.AppendChild(el);
        }
    }
}

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Xml;

namespace Minipaso
{
    /// <summary>
    /// Reads Windows Experience Index scores from the WinSAT DataStore,
    /// exactly like the 2007 gadget's onVote() did (minus the drive serials).
    /// </summary>
    public class WinSatScores
    {
        public double SystemScore;
        public double CpuScore;
        public double MemoryScore;
        public double GraphicsScore;
        public double GamingScore;
        public double DiskScore;
        public DateTime MeasuredAt;
    }

    public static class WinSat
    {
        public static string DataStorePath()
        {
            string windir = Environment.GetEnvironmentVariable("SystemRoot");
            if (string.IsNullOrEmpty(windir))
            {
                windir = @"C:\Windows";
            }
            return Path.Combine(windir, @"Performance\WinSAT\DataStore");
        }

        /// <summary>
        /// Finds the newest official Formal assessment and returns its WinSPR scores.
        /// Returns null when no assessment exists yet (user should press Measure first).
        /// </summary>
        public static WinSatScores ReadLatest()
        {
            string dir = DataStorePath();
            if (!Directory.Exists(dir))
            {
                return null;
            }

            string[] files = Directory.GetFiles(dir, "*.WinSAT.xml");
            string best = null;
            DateTime bestDate = DateTime.MinValue;

            foreach (string file in files)
            {
                string name = Path.GetFileName(file);
                if (name.IndexOf("Formal") < 0)
                {
                    continue;
                }
                try
                {
                    XmlDocument xml = new XmlDocument();
                    xml.Load(file);
                    XmlNode root = xml.SelectSingleNode("/WinSAT");
                    if (root == null)
                    {
                        continue;
                    }
                    XmlNode official = root.SelectSingleNode("IsOfficial");
                    if (official == null || official.InnerText != "1")
                    {
                        continue;
                    }
                    XmlNode exec = root.SelectSingleNode("ExecDateTOD");
                    DateTime when = File.GetLastWriteTime(file);
                    if (exec != null)
                    {
                        DateTime parsed;
                        if (DateTime.TryParse(exec.InnerText, out parsed))
                        {
                            when = parsed;
                        }
                    }
                    if (best == null || when > bestDate)
                    {
                        best = file;
                        bestDate = when;
                    }
                }
                catch (XmlException)
                {
                    continue;
                }
                catch (IOException)
                {
                    continue;
                }
            }

            if (best == null)
            {
                return null;
            }

            XmlDocument doc = new XmlDocument();
            doc.Load(best);
            XmlNode winspr = doc.SelectSingleNode("/WinSAT/WinSPR");
            if (winspr == null)
            {
                return null;
            }

            WinSatScores scores = new WinSatScores();
            scores.SystemScore = ScoreOf(winspr, "SystemScore");
            scores.CpuScore = ScoreOf(winspr, "CpuScore");
            scores.MemoryScore = ScoreOf(winspr, "MemoryScore");
            scores.GraphicsScore = ScoreOf(winspr, "GraphicsScore");
            scores.GamingScore = ScoreOf(winspr, "GamingScore");
            scores.DiskScore = ScoreOf(winspr, "DiskScore");
            scores.MeasuredAt = bestDate;
            return scores;
        }

        private static double ScoreOf(XmlNode winspr, string tag)
        {
            XmlNode node = winspr.SelectSingleNode(tag);
            if (node == null)
            {
                return 0.0;
            }
            double value;
            if (double.TryParse(node.InnerText, out value))
            {
                return value;
            }
            return 0.0;
        }

        /// <summary>
        /// Launches the official assessment. Needs admin; winsat shows its own window.
        /// </summary>
        public static void RunFormalAssessment()
        {
            ProcessStartInfo psi = new ProcessStartInfo("winsat", "formal");
            psi.UseShellExecute = true;
            psi.Verb = "runas";
            Process.Start(psi);
        }
    }
}

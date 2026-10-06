using System;
using System.Drawing;
using System.Windows.Forms;

namespace Minipaso
{
    /// <summary>
    /// The whole UI, hand-built in code (VS2010-designer-friendly, no resx needed):
    /// nickname row, 測定 Measure / 投票 Vote / ランキング Ranking buttons,
    /// and the six WEI subscores. Bilingual labels like the original gadget.
    /// </summary>
    public class MainForm : Form
    {
        private TextBox nicknameBox;
        private Button measureButton;
        private Button voteButton;
        private Button rankingButton;
        private Label statusLabel;
        private Label systemValue;
        private Label cpuValue;
        private Label memoryValue;
        private Label graphicsValue;
        private Label gamingValue;
        private Label diskValue;

        private WinSatScores current;

        public MainForm()
        {
            this.Text = "Minipaso \u2014 \u307f\u3093\u306a\u306e\u30d1\u30bd\u30b3\u30f3 (tribute)";
            this.ClientSize = new Size(360, 340);
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.StartPosition = FormStartPosition.CenterScreen;
            this.Font = new Font("Segoe UI", 9f);

            int y = 12;

            Label nickLabel = new Label();
            nickLabel.Text = "Nickname:";
            nickLabel.Location = new Point(12, y + 3);
            nickLabel.AutoSize = true;
            this.Controls.Add(nickLabel);

            this.nicknameBox = new TextBox();
            this.nicknameBox.Location = new Point(90, y);
            this.nicknameBox.Width = 258;
            this.nicknameBox.Text = Environment.UserName;
            this.Controls.Add(this.nicknameBox);

            y += 34;

            this.measureButton = new Button();
            this.measureButton.Text = "\u6e2c\u5b9a Measure";
            this.measureButton.Location = new Point(12, y);
            this.measureButton.Size = new Size(106, 30);
            this.measureButton.Click += new EventHandler(this.OnMeasure);
            this.Controls.Add(this.measureButton);

            this.voteButton = new Button();
            this.voteButton.Text = "\u6295\u7968 Vote";
            this.voteButton.Location = new Point(127, y);
            this.voteButton.Size = new Size(106, 30);
            this.voteButton.Click += new EventHandler(this.OnVote);
            this.Controls.Add(this.voteButton);

            this.rankingButton = new Button();
            this.rankingButton.Text = "\u30e9\u30f3\u30ad\u30f3\u30b0 Ranking";
            this.rankingButton.Location = new Point(242, y);
            this.rankingButton.Size = new Size(106, 30);
            this.rankingButton.Click += new EventHandler(this.OnRanking);
            this.Controls.Add(this.rankingButton);

            y += 44;

            this.systemValue = this.AddScoreRow("System (base):", y);
            y += 26;
            this.cpuValue = this.AddScoreRow("Processor:", y);
            y += 26;
            this.memoryValue = this.AddScoreRow("Memory:", y);
            y += 26;
            this.graphicsValue = this.AddScoreRow("Graphics:", y);
            y += 26;
            this.gamingValue = this.AddScoreRow("Gaming graphics:", y);
            y += 26;
            this.diskValue = this.AddScoreRow("Primary disk:", y);
            y += 32;

            this.statusLabel = new Label();
            this.statusLabel.Location = new Point(12, y);
            this.statusLabel.Size = new Size(336, 40);
            this.statusLabel.Text = "Press Measure to run winsat (needs admin), or Vote to submit your latest scores.";
            this.Controls.Add(this.statusLabel);

            this.RefreshScores();
        }

        private Label AddScoreRow(string caption, int y)
        {
            Label cap = new Label();
            cap.Text = caption;
            cap.Location = new Point(12, y + 3);
            cap.AutoSize = true;
            this.Controls.Add(cap);

            Label val = new Label();
            val.Text = "-";
            val.Location = new Point(180, y + 3);
            val.AutoSize = true;
            val.Font = new Font("Segoe UI", 9f, FontStyle.Bold);
            this.Controls.Add(val);
            return val;
        }

        private void RefreshScores()
        {
            try
            {
                this.current = WinSat.ReadLatest();
            }
            catch (Exception ex)
            {
                this.statusLabel.Text = "Could not read WinSAT data: " + ex.Message;
                return;
            }
            if (this.current == null)
            {
                this.statusLabel.Text = "No WinSAT assessment found. Press Measure first.";
                return;
            }
            this.systemValue.Text = this.current.SystemScore.ToString("0.0");
            this.cpuValue.Text = this.current.CpuScore.ToString("0.0");
            this.memoryValue.Text = this.current.MemoryScore.ToString("0.0");
            this.graphicsValue.Text = this.current.GraphicsScore.ToString("0.0");
            this.gamingValue.Text = this.current.GamingScore.ToString("0.0");
            this.diskValue.Text = this.current.DiskScore.ToString("0.0");
            this.statusLabel.Text = "Scores from " + this.current.MeasuredAt.ToString("g") + ".";
        }

        private void OnMeasure(object sender, EventArgs e)
        {
            try
            {
                WinSat.RunFormalAssessment();
                this.statusLabel.Text = "winsat is running in its own window. Come back and press Vote when it finishes.";
            }
            catch (Exception ex)
            {
                this.statusLabel.Text = "Could not start winsat: " + ex.Message;
            }
        }

        private void OnVote(object sender, EventArgs e)
        {
            if (this.current == null)
            {
                this.RefreshScores();
                if (this.current == null)
                {
                    return;
                }
            }
            string nick = this.nicknameBox.Text.Trim();
            if (nick.Length == 0)
            {
                this.statusLabel.Text = "Type a nickname first.";
                return;
            }
            this.voteButton.Enabled = false;
            try
            {
                string id = Reporter.Vote(this.current, nick);
                this.statusLabel.Text = "Voted! Server id: " + id;
            }
            catch (Exception ex)
            {
                this.statusLabel.Text = "Vote failed: " + ex.Message;
            }
            this.voteButton.Enabled = true;
        }

        private void OnRanking(object sender, EventArgs e)
        {
            try
            {
                Reporter.OpenRankingPage();
            }
            catch (Exception ex)
            {
                this.statusLabel.Text = "Could not open the ranking page: " + ex.Message;
            }
        }
    }
}

using System;
using Microsoft.Win32;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Numerics;
using System.Globalization;
using System.Xml;
using System.IO;
using Prism.Commands;
using Prism.Mvvm;


namespace SD2CircleTool.ViewModels
{
    public class ClassicinatorViewModel : BindableBase
    {
        private String _filePath; public String FilePath { get => _filePath; set => SetProperty(ref _filePath, value); }
        private bool _isPathValid = false;

        public DelegateCommand OpenFileCommand => new(() =>
        {
            _isPathValid = false;
            OpenFileDialog ofd = new()
            {
                Filter = "Soundodger2 Level Files (*.xml)|*.xml"
            };

            if (ofd.ShowDialog() == true)
            { 
                FilePath = ofd.FileName;
                _isPathValid = true;
            }
        });

        public DelegateCommand ConvertCommand => new(() =>
        {
            if (_isPathValid)
            {
                // Load xml, split by line
                String[] sd2lvl = File.ReadAllText(FilePath).Split(Environment.NewLine, StringSplitOptions.RemoveEmptyEntries);
                String[] sd2meta = FilePath.Split('/', '.', '\\');
                String nick = sd2meta[sd2meta.Length - 2];
                String newFilePath = FilePath.Replace(".xml", "_Classic.xml");
                String headFilePath = FilePath.Replace(nick + ".xml", "header.txt");

                //MessageBox.Show(String.Format("{0}\n{1}\n{2}\n{3}", sd2lvl[0], sd2lvl[1], sd2lvl[2], nick)); // debug shit

                // Metadata Fields
                String title = "TITLE (header not found)";
                String artist = "ARTIST (header not found)";
                String designer = "";
                String mp3name = nick + ".mp3";
                String subtitle = "";
                String bgBlack = "false";
                String containsHeart = "false";
                String length = "1000";
                nick = nick + "_Classic";

                int colorBg1 = 0;
                int colorBg2 = 0;
                int colorOut = 0;
                int colorScr = 0;
                int colorAr1 = 0;
                int colorAr2 = 0;
                int colorHom = 0;
                int colorBub = 0;
                int colorHug = 0;

                int enemies = 1;
                int difficulty = 0;

                int totalNoEvents = 3;

                double preview = 50;

                char[] separators = new char[] { ' ', '"', '=' };

                // SD+ level
                List<string> sdlvl = new List<string>();
                sdlvl.Add("<Song>");
                sdlvl.Add("  <Script time=\"0\" enemies=\"0\" warpType=\"spinRate\" val=\"0\"/>");
                sdlvl.Add("  <Script time=\"0\" enemies=\"0\" warpType=\"timeWarp\" val=\"1\"/>");

                foreach (String line in sd2lvl)
                {

                    #region Metadata

                    // Decode Colours
                    if (line.StartsWith("  <Colors "))
                    {
                        String[] colorLine = line.Split(' ', StringSplitOptions.RemoveEmptyEntries);

                        foreach (String entry in colorLine)
                        {
                            if (entry.StartsWith("bg1="))
                            {
                                String hex = entry.Split(separators, StringSplitOptions.RemoveEmptyEntries)[1];
                                colorBg1 = int.Parse(hex, NumberStyles.HexNumber);
                                if (colorBg1 == 0) bgBlack = "true";
                            }
                            else if (entry.StartsWith("bg2="))
                            {
                                String hex = entry.Split(separators, StringSplitOptions.RemoveEmptyEntries)[1];
                                colorBg2 = int.Parse(hex, NumberStyles.HexNumber);
                            }
                            else if (entry.StartsWith("outline="))
                            {
                                String hex = entry.Split(separators, StringSplitOptions.RemoveEmptyEntries)[1];
                                colorOut = int.Parse(hex, NumberStyles.HexNumber);
                            }
                            else if (entry.StartsWith("score="))
                            {
                                String hex = entry.Split(separators, StringSplitOptions.RemoveEmptyEntries)[1];
                                colorScr = int.Parse(hex, NumberStyles.HexNumber);
                            }
                            else if (entry.StartsWith("arrow="))
                            {
                                String hex = entry.Split(separators, StringSplitOptions.RemoveEmptyEntries)[1];
                                colorAr1 = int.Parse(hex, NumberStyles.HexNumber);
                            }
                            else if (entry.StartsWith("arrow_2="))
                            {
                                String hex = entry.Split(separators, StringSplitOptions.RemoveEmptyEntries)[1];
                                colorAr2 = int.Parse(hex, NumberStyles.HexNumber);
                            }
                            else if (entry.StartsWith("homing="))
                            {
                                String hex = entry.Split(separators, StringSplitOptions.RemoveEmptyEntries)[1];
                                colorHom = int.Parse(hex, NumberStyles.HexNumber);
                            }
                            else if (entry.StartsWith("bubble="))
                            {
                                String hex = entry.Split(separators, StringSplitOptions.RemoveEmptyEntries)[1];
                                colorBub = int.Parse(hex, NumberStyles.HexNumber);
                            }
                            else if (entry.StartsWith("hug="))
                            {
                                String hex = entry.Split(separators, StringSplitOptions.RemoveEmptyEntries)[1];
                                colorHug = int.Parse(hex, NumberStyles.HexNumber);
                            }
                        }
                        int brk = 1 + 1;
                    }

                    // Decode Settings
                    else if (line.StartsWith("  <Settings "))
                    {
                        String[] settingsLine = line.Split('\"', StringSplitOptions.RemoveEmptyEntries);

                        int iAuth = 0, iEn = 0, iDiff = 0, iHeart = 0, i = 0;

                        foreach (String entry in settingsLine)
                        {
                            if (entry.EndsWith("by=")) { iAuth = i + 1; }
                            else if (entry.EndsWith("en=")) { iEn = i + 1; }
                            else if (entry.EndsWith("diff=")) { iDiff = i + 1; }
                            else if (entry.EndsWith("hearts=")) { iHeart = i + 1; }
                            i++;
                        }

                        designer = settingsLine[iAuth];
                        enemies = int.Parse(settingsLine[iEn]);
                        bool isDiffNum = int.TryParse(settingsLine[iDiff], out difficulty);
                        if (!isDiffNum) subtitle = settingsLine[iDiff];
                        containsHeart = settingsLine[iHeart];
                    }

                    // Decode Music
                    else if (line.StartsWith("  <Music "))
                    {
                        String[] musicLine = line.Split(' ', StringSplitOptions.RemoveEmptyEntries);

                        double previewTime = 0;
                        double endTime = 1;

                        foreach (String entry in musicLine)
                        {
                            if (entry.StartsWith("preview="))
                            {
                                String temp = entry.Split(separators, StringSplitOptions.RemoveEmptyEntries)[1];
                                previewTime = double.Parse(temp);
                            }

                            else if (entry.StartsWith("endAt="))
                            {
                                String temp = entry.Split(separators, StringSplitOptions.RemoveEmptyEntries)[1];
                                endTime = double.Parse(temp);
                            }
                        }

                        if (previewTime != -1)
                        {
                            preview = previewTime / endTime;
                        }

                    }

                    #endregion

                    #region Events

                    // Spinrate
                    else if (line.StartsWith("  <Event type=\"spin\" "))
                    {
                        String tempTime = "0";
                        String tempVal = "0";

                        String[] eventLine = line.Split(' ', StringSplitOptions.RemoveEmptyEntries);

                        foreach (String entry in eventLine)
                        {
                            String[] temp = entry.Split(separators, StringSplitOptions.RemoveEmptyEntries);

                            if (temp[0] == "time")
                            {
                                
                                tempTime = temp[1];
                            }

                            else if (temp[0] == "val")
                            {
                                tempVal = temp[1];
                            }
                        }

                        sdlvl.Add(SpinEvent.makeString(tempTime, tempVal));
                        totalNoEvents++;
                    }

                    // Timewarp
                    else if (line.StartsWith("  <Event type=\"time\" "))
                    {
                        String tempTime = "0";
                        String tempVal = "0";

                        String[] eventLine = line.Split(' ', StringSplitOptions.RemoveEmptyEntries);

                        foreach (String entry in eventLine)
                        {
                            String[] temp = entry.Split(separators, StringSplitOptions.RemoveEmptyEntries);

                            if (temp[0] == "time")
                            {

                                tempTime = temp[1];
                            }

                            else if (temp[0] == "val")
                            {
                                tempVal = temp[1];
                            }
                        }

                        sdlvl.Add(TimeEvent.makeString(tempTime, tempVal));
                        totalNoEvents++;
                    }

                    #endregion

                    #region Bullets
                    else if (line.StartsWith("  <Bullet "))
                    {
                        BulletEvent bullet = new BulletEvent();

                        String[] bulletLine = line.Split(' ', StringSplitOptions.RemoveEmptyEntries);

                        foreach (String entry in bulletLine)
                        {
                            String[] temp = entry.Split(separators, StringSplitOptions.RemoveEmptyEntries);

                            switch (temp[0])
                            {
                                // set time
                                case "time":
                                    bullet.time = temp[1];
                                    break;

                                // decode and set enemies
                                case "en":
                                    String[] en = temp[1].Split(',');
                                    foreach (String enGroup in en)
                                    {
                                        int e;

                                        // if singular enemy
                                        if (int.TryParse(enGroup, out e))
                                        {
                                            bullet.enemies.Add(e);
                                        }

                                        // if enemy range
                                        else
                                        {
                                            int eMin = int.Parse(enGroup.Split('-')[0]);
                                            int eMax = int.Parse(enGroup.Split('-')[1]);
                                            for (int i = eMin; i <= eMax; i++)
                                            {
                                                bullet.enemies.Add(i);
                                            }
                                        }
                                    }
                                    break;

                                case "patt":
                                    bullet.shotType = temp[1];
                                    break;

                                case "type":
                                    if (temp[1] == "arrow") { bullet.bulletType = "nrm"; }
                                    else { bullet.bulletType = temp[1]; }
                                    break;

                                case "col":
                                    bullet.col = temp[1];
                                    break;

                                case "aim":
                                    if (temp[1] == "player") { bullet.aim = "pl"; }
                                    else { bullet.aim = "mid"; }
                                    break;

                                case "life":
                                    double life = double.Parse(temp[1]);
                                    life *= 30;
                                    bullet.lifespan = String.Format(new CultureInfo("en-US"), "{0:N3}", life);
                                    break;

                                case "offset":
                                    bullet.offset0 = temp[1].Split(',')[0];
                                    bullet.offset1 = temp[1].Split(',')[1];
                                    break;

                                case "amt":
                                    bullet.amount0 = temp[1].Split(',')[0];
                                    bullet.amount1 = temp[1].Split(',')[1];
                                    break;

                                case "speed":
                                    bullet.speed0 = temp[1].Split(',')[0];
                                    bullet.speed1 = temp[1].Split(',')[1];
                                    break;

                                case "spread":
                                    bullet.angle0 = temp[1].Split(',')[0];
                                    bullet.angle1 = temp[1].Split(',')[1];
                                    break;

                                case "dur":
                                    bullet.duration = temp[1];
                                    break;

                                case "rows":
                                    int.TryParse(temp[1], out bullet.rows);
                                    break;

                                default:
                                    break;
                            }
                        }

                        sdlvl.Add(bullet.makeString());
                    }

                    #endregion
                }

                if (File.Exists(headFilePath))
                {
                    String[] sd2header = File.ReadAllText(headFilePath).Split(new String[] { "\n", "\r" }, StringSplitOptions.RemoveEmptyEntries);
                    foreach (String line in sd2header)
                    {
                        String[] temp = line.Split('|');
                        if (temp[0] == "title") { title = temp[1]; }
                        else if (temp[0] == "artist") { artist = temp[1]; }
                        else if (temp[0] == "length") { length = temp[1]; }
                    }
                }

                sdlvl.Insert(totalNoEvents, String.Format("  <Script time=\"{0}\" enemies=\"0\" warpType=\"spinRate\" val=\"0\"/>", length));
                sdlvl.Insert(totalNoEvents + 1, String.Format("  <Script time=\"{0}\" enemies=\"0\" warpType=\"timeWarp\" val=\"1\"/>", length));

                // Write Metadata
                sdlvl.Insert(1, String.Format(new CultureInfo("en-US"), "  <Info nick=\"{0}\" enemies=\"{1}\" " +
                    "color1=\"{2}\" color2=\"{3}\" color3=\"{4}\" color4=\"{5}\" color5=\"{6}\" color6=\"{7}\" color7=\"{8}\" color8=\"{9}\" color9=\"{10}\" " +
                    "title=\"{11}\" artist=\"{12}\" difficulty=\"{13}\" designer=\"{14}\" MP3Name=\"{15}\" bgBlack=\"{16}\" audioPreview=\"{17:N3}\" subtitle=\"{18}\" containsHeart=\"{19}\"/>",
                    nick, enemies, colorAr1, colorAr2, colorHom, colorBub, colorOut, colorBg2, colorBg1, colorScr, colorHug,
                    title, artist, difficulty, designer, mp3name, bgBlack, preview, subtitle, containsHeart));

                sdlvl.Add("</Song>");

                File.WriteAllLines(newFilePath, sdlvl);
            }
        });
    }

    public class SpinEvent
    {
        public double time;
        public double value;
        public SpinEvent(double time, double value)
        {
            this.time = time;
            this.value = value;
        }

        public override String ToString()
        {
            return String.Format(new CultureInfo("en-US"), "  <Script time=\"{0:N3}\" enemies=\"0\" warpType=\"spinRate\" val=\"{1:N3}\"/>",
                time, value);
        }

        public static String makeString(double Time, double Value)
        {
            return String.Format(new CultureInfo("en-US"), "  <Script time=\"{0:N3}\" enemies=\"0\" warpType=\"spinRate\" val=\"{1:N3}\"/>",
                Time, Value);
        }
        public static String makeString(String Time, String Value)
        {
            return String.Format(new CultureInfo("en-US"), "  <Script time=\"{0}\" enemies=\"0\" warpType=\"spinRate\" val=\"{1}\"/>",
                Time, Value);
        }
    }

    public class TimeEvent
    {
        public double time;
        public double value;
        public TimeEvent(double time, double value)
        {
            this.time = time;
            this.value = value;
        }

        public override String ToString()
        {
            return String.Format(new CultureInfo("en-US"), "  <Script time=\"{0:N3}\" enemies=\"0\" warpType=\"timeWarp\" val=\"{1:N3}\"/>",
                time, value);
        }

        public static String makeString(double Time, double Value)
        {
            return String.Format(new CultureInfo("en-US"), "  <Script time=\"{0:N3}\" enemies=\"0\" warpType=\"timeWarp\" val=\"{1:N3}\"/>",
                Time, Value);
        }
        public static String makeString(String Time, String Value)
        {
            return String.Format(new CultureInfo("en-US"), "  <Script time=\"{0}\" enemies=\"0\" warpType=\"timeWarp\" val=\"{1}\"/>",
                Time, Value);
        }
    }

    public class BulletEvent
    {
        public String time = "0";
        public List<int> enemies = new List<int>();
        public String shotType = "normal";
        public String bulletType = "nrm";
        public String col = "";
        public String lifespan = "90";
        public String aim = "mid";
        public String offset0 = "0";
        public String amount0 = "1";
        public String speed0 = "5";
        public String angle0 = "0";
        public String offset1 = "0";
        public String amount1 = "1";
        public String speed1 = "5";
        public String angle1 = "0";
        public String duration = "1";
        public int rows = 1;

        public String makeString()
        {
            String tenemies = "";
            String tlifespan = "";
            String trows = "";

            // convert enemies to string
            tenemies += enemies[0].ToString();
            enemies.RemoveAt(0);

            foreach (int enemy in enemies)
            {
                tenemies += ",";
                tenemies += enemy.ToString();
            }

            // convert col=2 arrows to nrm2

            if (bulletType == "nrm")
            {
                if (col == "2") { bulletType = "nrm2";  }
            }

            // convert homing lifespan
            if (bulletType == "homing")
            {
                tlifespan += String.Format(new CultureInfo("en-US"), "lifespan=\"{0}\" ", lifespan);
            }

            // rows
            if (rows >= 2)
            {
                shotType = "wave";
                trows += String.Format(new CultureInfo("en-US"), "rows=\"{0}\" ", rows);
            }

            switch (shotType)
            {
                case "normal":
                    return String.Format(new CultureInfo("en-US"), "  <Script time=\"{0}\" enemies=\"{1}\" shotType=\"normal\" bulletType=\"{2}\" {8}aim=\"{3}\" " +
                        "offset0=\"{4}\" amount0=\"{5}\" speed0=\"{6}\" angle0=\"{7}\"/>",
                        time, tenemies, bulletType, aim,
                        offset0, amount0, speed0, angle0, tlifespan);

                case "wave":
                    return String.Format(new CultureInfo("en-US"), "  <Script time=\"{0}\" enemies=\"{1}\" shotType=\"wave\" bulletType=\"{2}\" {8}{13}aim=\"{3}\" " +
                        "offset0=\"{4}\" amount0=\"{5}\" speed0=\"{6}\" angle0=\"{7}\" " +
                        "offset1=\"{9}\" amount1=\"{10}\" speed1=\"{11}\" angle1=\"{12}\"/>",
                        time, tenemies, bulletType, aim,
                        offset0, amount0, speed0, angle0, tlifespan,
                        offset1, amount1, speed1, angle1, trows);

                case "stream":
                    return String.Format(new CultureInfo("en-US"), "  <Script time=\"{0}\" enemies=\"{1}\" shotType=\"stream\" bulletType=\"{2}\" {8}aim=\"{3}\" " +
                        "amount=\"{4}\" offset0=\"{5}\" speed0=\"{6}\" angle0=\"{7}\" " +
                        "offset1=\"{9}\" speed1=\"{10}\" angle1=\"{11}\" duration=\"{12}\"/>",
                        time, tenemies, bulletType, aim,
                        amount0, offset0, speed0, angle0, tlifespan,
                        offset1, speed1, angle1, duration);

                case "burst":
                    return String.Format(new CultureInfo("en-US"), "  <Script time=\"{0}\" enemies=\"{1}\" shotType=\"burst\" bulletType=\"{2}\" {8}aim=\"{3}\" " +
                        "offset0=\"{4}\" amount0=\"{5}\" speed0=\"{6}\" angle0=\"{7}\" " +
                        "speed1=\"{9}\"/>",
                        time, tenemies, bulletType, aim,
                        offset0, amount0, speed0, angle0, tlifespan,
                        speed1);

                default:
                    return String.Format(new CultureInfo("en-US"), "  <Script time=\"{0}\" enemies=\"{1}\" shotType=\"normal\" bulletType=\"{2}\" aim=\"{3}\" " +
                        "offset0=\"{4}\" amount0=\"{5}\" speed0=\"{6}\" angle0=\"{7}\"/>",
                        time, tenemies, bulletType, aim,
                        offset0, amount0, speed0, angle0);
            }
        }
    }
}

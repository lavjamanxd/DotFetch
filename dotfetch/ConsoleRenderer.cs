using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace dotfetch
{
    public class ConsoleRenderer
    {
        private SysInformationProvider sysInfo;

        public ConsoleRenderer(SysInformationProvider info)
        {
            sysInfo = info;
        }

        public void Render()
        {
            DrawWindowsLogo();
            WriteSysInfo(2);
        }

        private async void CreateScreenShot()
        {
            Console.Write("Creating Screenshot: ");
            for (var i = 0; i < 3; i++)
            {
                await Task.Delay(new TimeSpan(0, 0, 0, 1));
                Console.Write(".");
            }

            using (var bmpScreenCapture = new Bitmap(Screen.PrimaryScreen.Bounds.Width,
                                            Screen.PrimaryScreen.Bounds.Height))
            {
                using (var g = Graphics.FromImage(bmpScreenCapture))
                {
                    g.CopyFromScreen(Screen.PrimaryScreen.Bounds.X,
                                     Screen.PrimaryScreen.Bounds.Y,
                                     0, 0,
                                     bmpScreenCapture.Size,
                                     CopyPixelOperation.SourceCopy);
                    var filename = "Screenshot" + DateTime.Now.ToString().Replace('.', '_').Replace(":", string.Empty) + ".png";
                    bmpScreenCapture.Save(filename, ImageFormat.Png);
                }
            }

            Console.Write(" done :3");
        }

        private void WriteSysInfo(int globalOffset)
        {
            Console.SetCursorPosition(50, globalOffset + 1);
            WriteUserInfo();

            Console.SetCursorPosition(50, globalOffset + 2);
            WriteManufacturer();

            Console.SetCursorPosition(50, globalOffset + 3);
            WriteModelNumber();

            Console.SetCursorPosition(50, globalOffset + 4);
            WriteOsVersion();

            Console.SetCursorPosition(50, globalOffset + 5);
            WriteOsBuildNumber();

            Console.SetCursorPosition(50, globalOffset + 6);
            WriteUptime();

            Console.SetCursorPosition(50, globalOffset + 7);
            WriteShell();

            Console.SetCursorPosition(50, globalOffset + 8);
            WriteResolution();

            Console.SetCursorPosition(50, globalOffset + 9);
            WriteFont();

            Console.SetCursorPosition(50, globalOffset + 10);
            WriteCpuName();

            Console.SetCursorPosition(50, globalOffset + 11);
            WriteCpuCores();

            Console.SetCursorPosition(50, globalOffset + 12);
            WriteCpuLoad();

            Console.SetCursorPosition(50, globalOffset + 13);
            WriteRam();

            Console.SetCursorPosition(50, globalOffset + 14);
            var offset = WriteGpus(globalOffset + 14);

            Console.SetCursorPosition(50, offset);

            offset = WriteDisks(offset);

            if (sysInfo.BatteryAvailable)
            {
                Console.SetCursorPosition(50, offset);
                WriteBattery();
            }
            Console.SetCursorPosition(0, 27);
            Console.ForegroundColor = ConsoleColor.White;
            CreateScreenShot();
        }

        private void WriteBattery()
        {
            Console.ForegroundColor = ConsoleColor.DarkCyan;
            Console.Write("Battery: ");
            Console.ForegroundColor = ConsoleColor.White;
            RenderBattery(int.Parse(sysInfo.BatteryChargeState));
        }

        private void SetBatteryColor(int progress)
        {
            if (progress < 33)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                return;
            }

            if (progress > 33 && progress < 66)
            {
                Console.ForegroundColor = ConsoleColor.Yellow;
                return;
            }
            Console.ForegroundColor = ConsoleColor.Green;
        }

        private void RenderBattery(int percentage)
        {
            Console.Write("(" + percentage + ")" + "%  ");
            Console.ForegroundColor = ConsoleColor.DarkGray;
            Console.Write("[");
            for (var i = 0; i < 10; i++)
            {
                if (i > percentage)
                {
                    Console.ForegroundColor = ConsoleColor.Gray;
                }
                else
                {
                    SetBatteryColor(percentage);
                }
                Console.Write("|");
            }

            Console.ForegroundColor = ConsoleColor.DarkGray;
            Console.Write("]=");
            if (!sysInfo.IsBatteryCharging) return;
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.Write(" ~");
        }

        private int WriteDisks(int offset)
        {
            Console.ForegroundColor = ConsoleColor.DarkCyan;
            Console.Write("Disks: ");
            Console.ForegroundColor = ConsoleColor.White;
            var i = 0;
            foreach (var hdd in sysInfo.Hdds)
            {
                Console.SetCursorPosition(58, offset + i);

                var conv = (Math.Pow(10, 9));
                var total = hdd.Capacity / conv;
                var free = hdd.FreeSpace / conv;
                var used = total - free;

                var percentage = ((double)used / (double)total) * 100;
                Console.Write(hdd.Name + "  ");
                RenderProgressBar((int)percentage);
                Console.ForegroundColor = ConsoleColor.White;
                Console.Write(Math.Round(used, 2) + "GB" + " / " + Math.Round(total, 2) + "GB");
                i++;
            }
            return offset + i;
        }

        private void WriteModelNumber()
        {
            Console.ForegroundColor = ConsoleColor.DarkCyan;
            Console.Write("Model: ");
            Console.ForegroundColor = ConsoleColor.White;
            Console.Write(sysInfo.ModelNumber);
        }

        private void WriteManufacturer()
        {
            Console.ForegroundColor = ConsoleColor.DarkCyan;
            Console.Write("Manufacturer: ");
            Console.ForegroundColor = ConsoleColor.White;
            Console.Write(sysInfo.Manufacturer);
        }

        private int WriteGpus(int renderPos)
        {
            Console.ForegroundColor = ConsoleColor.DarkCyan;
            Console.Write("GPU: ");
            Console.ForegroundColor = ConsoleColor.White;
            var i = 0;
            foreach (var gpu in sysInfo.GpuList)
            {
                Console.SetCursorPosition(55, renderPos + i);
                Console.Write(gpu);
                i++;
            }
            return renderPos + i;
        }

        private void WriteRam()
        {
            Console.ForegroundColor = ConsoleColor.DarkCyan;
            Console.Write("RAM: ");
            Console.ForegroundColor = ConsoleColor.White;
            Console.Write(sysInfo.UsedRamSize + "MB" + " / " + sysInfo.MaxRamSize + "MB  ");
            var percentage = double.Parse(sysInfo.UsedRamSize) / double.Parse(sysInfo.MaxRamSize) * 100;
            RenderProgressBar((int)percentage);
        }

        private void WriteCpuLoad()
        {
            Console.ForegroundColor = ConsoleColor.DarkCyan;
            Console.Write("CPU Usage: ");
            RenderProgressBar(int.Parse(sysInfo.CpuLoad));
        }

        private void SetLoadColor(int progress)
        {
            if (progress < 33)
            {
                Console.ForegroundColor = ConsoleColor.Green;
                return;
            }

            if (progress > 33 && progress < 66)
            {
                Console.ForegroundColor = ConsoleColor.Yellow;
                return;
            }
            Console.ForegroundColor = ConsoleColor.Red;
        }

        private void RenderProgressBar(int progress)
        {
            SetLoadColor(progress);
            Console.Write("(" + progress + ")" + "%  ");
            Console.ForegroundColor = ConsoleColor.DarkGray;
            Console.Write("[");
            SetLoadColor(progress);
            for (var i = 0; i < 10; i++)
            {
                if (i < progress / 10)
                {
                    Console.Write("=");
                }
                else
                {
                    Console.ForegroundColor = ConsoleColor.Gray;
                    Console.Write("-");
                }
            }

            Console.ForegroundColor = ConsoleColor.DarkGray;
            Console.Write("]");
        }

        private void WriteCpuCores()
        {
            Console.ForegroundColor = ConsoleColor.DarkCyan;
            Console.Write("Cores: ");
            Console.ForegroundColor = ConsoleColor.White;
            Console.Write(sysInfo.CpuCore + " Physical " + sysInfo.CpuVirtualCore + " Logical");
        }

        private void WriteCpuName()
        {
            Console.ForegroundColor = ConsoleColor.DarkCyan;
            Console.Write("CPU: ");
            Console.ForegroundColor = ConsoleColor.White;
            Console.Write(sysInfo.CpuName);
        }

        private void WriteFont()
        {
            Console.ForegroundColor = ConsoleColor.DarkCyan;
            Console.Write("Font: ");
            Console.ForegroundColor = ConsoleColor.White;
            Console.Write(sysInfo.Font);
        }

        private void WriteResolution()
        {
            Console.ForegroundColor = ConsoleColor.DarkCyan;
            Console.Write("Resolution: ");
            Console.ForegroundColor = ConsoleColor.White;
            Console.Write(sysInfo.ScreenWidth + " x " + sysInfo.ScreenHeight + " " + sysInfo.Bits + "bit" + " " + sysInfo.RefreshRate + "Hz");
        }

        private void WriteShell()
        {
            Console.ForegroundColor = ConsoleColor.DarkCyan;
            Console.Write("Shell: ");
            Console.ForegroundColor = ConsoleColor.White;
            Console.Write(sysInfo.Shell);
        }

        private void WriteUptime()
        {
            Console.ForegroundColor = ConsoleColor.DarkCyan;
            Console.Write("Uptime: ");
            Console.ForegroundColor = ConsoleColor.White;
            Console.Write(WriteTime(sysInfo.Uptime.Days, "d") + WriteTime(sysInfo.Uptime.Hours, "h") +
                          WriteTime(sysInfo.Uptime.Minutes, "M") + WriteTime(sysInfo.Uptime.Seconds, "s"));
        }

        public string WriteTime(int value, string marker)
        {
            return value + marker + " ";
        }

        private void WriteOsBuildNumber()
        {
            Console.ForegroundColor = ConsoleColor.DarkCyan;
            Console.Write("Build Number: ");
            Console.ForegroundColor = ConsoleColor.White;
            Console.Write(sysInfo.OsBuildNumber);
        }

        private void WriteOsVersion()
        {
            Console.ForegroundColor = ConsoleColor.DarkCyan;
            Console.Write("OS: ");
            Console.ForegroundColor = ConsoleColor.White;
            Console.Write(sysInfo.OperatingSystem + " " + sysInfo.OsArchitecture);
        }

        private void WriteUserInfo()
        {
            Console.ForegroundColor = ConsoleColor.DarkCyan;
            Console.Write(sysInfo.UserName);
            Console.ForegroundColor = ConsoleColor.White;
            Console.Write("@");
            Console.ForegroundColor = ConsoleColor.DarkCyan;
            Console.Write(sysInfo.PcName);
        }

        private void DrawWindowsLogo()
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("                                           ..");
            Console.WriteLine("                                 ...---++++-");
            Console.WriteLine("                       ...-:////+++++ooooooo-");
            Console.ForegroundColor = ConsoleColor.Red;
            Console.Write("             .....");

            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine(" -/++++ooooooooooooooooooo-");
            Console.ForegroundColor = ConsoleColor.Red;
            Console.Write("    .------/+oooo/ ");
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("/oooooooooooooooooooooooo-");
            Console.ForegroundColor = ConsoleColor.Red;
            Console.Write("  /+ooooooooooooo/ ");
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("/oooooooooooooooooooooooo-");
            Console.ForegroundColor = ConsoleColor.Red;
            Console.Write("  ooooooooooooooo/ ");
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("/oooooooooooooooooooooooo-");
            Console.ForegroundColor = ConsoleColor.Red;
            Console.Write("  ooooooooooooooo/ ");
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("/oooooooooooooooooooooooo-");
            Console.ForegroundColor = ConsoleColor.Red;
            Console.Write("  ooooooooooooooo/ ");
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("/oooooooooooooooooooooooo-");
            Console.ForegroundColor = ConsoleColor.Red;
            Console.Write("  ooooooooooooooo/ ");
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("/oooooooooooooooooooooooo-");
            Console.ForegroundColor = ConsoleColor.Red;
            Console.Write("  ooooooooooooooo/ ");
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("/oooooooooooooooooooooooo-");
            Console.ForegroundColor = ConsoleColor.Red;
            Console.Write("  +++++++++++++++: ");
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("/++++++++++++++++++++++++-");
            Console.ForegroundColor = ConsoleColor.Red;
            Console.Write("  ```````````````` ");
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("``````````````````````````");
            Console.ForegroundColor = ConsoleColor.Blue;
            Console.Write("  +++++++++++++++: ");
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("-------------------------.");
            Console.ForegroundColor = ConsoleColor.Blue;
            Console.Write("  ooooooooooooooo/ ");
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("-::::::::::::::::::::::::.");
            Console.ForegroundColor = ConsoleColor.Blue;
            Console.Write("  ooooooooooooooo/ ");
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("-::::::::::::::::::::::::.");
            Console.ForegroundColor = ConsoleColor.Blue;
            Console.Write("  ooooooooooooooo/ ");
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("-::::::::::::::::::::::::.");
            Console.ForegroundColor = ConsoleColor.Blue;
            Console.Write("  ooooooooooooooo/ ");
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("-::::::::::::::::::::::::.");
            Console.ForegroundColor = ConsoleColor.Blue;
            Console.Write("  ooooooooooooooo/ ");
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("-::::::::::::::::::::::::.");
            Console.ForegroundColor = ConsoleColor.Blue;
            Console.Write("  ooooooooooooooo/ ");
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("-::::::::::::::::::::::::.");
            Console.ForegroundColor = ConsoleColor.Blue;
            Console.Write("  .:+oooooooooooo/ ");
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("-::::::::::::::::::::::::.");
            Console.ForegroundColor = ConsoleColor.Blue;
            Console.Write("     ````````-::+: ");
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("-::::::::::::::::::::::::.");
            Console.WriteLine("                    ```---::::::::::::::::::.");
            Console.WriteLine("                               ```---:::::::.");
            Console.WriteLine("                                       ``````");
            Console.ResetColor();
        }
    }
}

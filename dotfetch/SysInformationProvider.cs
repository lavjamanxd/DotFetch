using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Management;
using System.Runtime.InteropServices;
using Microsoft.VisualBasic.Devices;

namespace dotfetch
{
    public class SysInformationProvider
    {
        //User Related
        public string UserName { get; set; }
        public string PcName { get; set; }

        //OS related
        public string OperatingSystem { get; set; }
        public string OsArchitecture { get; set; }
        public string OsBuildNumber { get; set; }

        public TimeSpan Uptime { get; set; }
        public string ScreenWidth { get; set; }
        public string ScreenHeight { get; set; }
        public string Bits { get; set; }
        public string RefreshRate { get; set; }

        public string Font { get; set; }
        public string Shell { get; set; }

        //CPU related
        public string CpuName { get; set; }
        public string CpuCore { get; set; }
        public string CpuVirtualCore { get; set; }
        public string CpuLoad { get; set; }

        //Memory related
        public string MaxRamSize { get; set; }
        public string UsedRamSize { get; set; }

        //GPU
        public List<string> GpuList { get; set; }

        //Model
        public string Manufacturer { get; set; }
        public string ModelNumber { get; set; }

        //Battery
        public bool BatteryAvailable { get; set; }
        public bool IsBatteryCharging { get; set; }
        public string BatteryChargeState { get; set; }

        //HDD
        public List<HddModel> Hdds { get; set; }

        public SysInformationProvider()
        {
            InitializeVariables();
        }


        public TimeSpan GetUpTime()
        {
            return TimeSpan.FromMilliseconds(GetTickCount64());
        }

        [DllImport("kernel32")]
        private static extern ulong GetTickCount64();

        public IEnumerable<IEnumerable<PropertyData>> GetManagementClassProperties(string managementClassName)
        {
            var mc = new ManagementClass(managementClassName);
            var moc = mc.GetInstances();
            var mocs = moc.OfType<ManagementObject>();

            return mocs.Select(managementObject => managementObject.Properties.OfType<PropertyData>());
        }


        private void InitializeVariables()
        {
            SetUpUserInfo();
            Uptime = GetUpTime();
            SetUpOsVariables();
            SetUpEnvironmentVariables();
            SetUpGpuVariables();
            SetUpCpuVariables();
            SetUpRamVaribles();
            SetUpSystemInfo();
            SetUpBatteryInfo();
            SetUpHddModels();
        }

        private void SetUpHddModels()
        {
            Hdds = new List<HddModel>();

            foreach (var label in DriveInfo.GetDrives().Where(x => x.DriveType == DriveType.Fixed))
            {
                Hdds.Add(new HddModel() { Capacity = label.TotalSize, Name = label.Name, FreeSpace = label.TotalFreeSpace });
            }
        }

        private void SetUpBatteryInfo()
        {
            var batteryInfo = GetManagementClassProperties("Win32_Battery").FirstOrDefault().ToList();

            BatteryAvailable = true;
            IsBatteryCharging =
                batteryInfo.FirstOrDefault(x => x.Name.Equals("BatteryStatus")).Value.ToString().Equals(1)
                    ? false
                    : true;
            BatteryChargeState =
                batteryInfo.FirstOrDefault(x => x.Name.Equals("EstimatedChargeRemaining")).Value.ToString();
        }

        private void SetUpSystemInfo()
        {
            var computerSystem = GetManagementClassProperties("Win32_ComputerSystem").FirstOrDefault();
            Manufacturer = computerSystem.FirstOrDefault(x => x.Name.Equals("Manufacturer")).Value.ToString();
            ModelNumber = computerSystem.FirstOrDefault(x => x.Name.Equals("Model")).Value.ToString();
        }

        private void SetUpRamVaribles()
        {
            var computerInfo = new ComputerInfo();

            MaxRamSize = (computerInfo.TotalPhysicalMemory / (1024 * 1024)).ToString();
            UsedRamSize =
                ((computerInfo.TotalPhysicalMemory - computerInfo.AvailablePhysicalMemory) / (1024 * 1024)).ToString();
        }

        private void SetUpCpuVariables()
        {
            var cpu = GetManagementClassProperties("Win32_Processor").FirstOrDefault();

            CpuName = cpu.FirstOrDefault(x => x.Name.Equals("Name")).Value.ToString();
            CpuLoad = cpu.FirstOrDefault(x => x.Name.Equals("LoadPercentage")).Value.ToString();
            CpuCore = cpu.FirstOrDefault(x => x.Name.Equals("NumberOfCores")).Value.ToString();
            CpuVirtualCore = cpu.FirstOrDefault(x => x.Name.Equals("NumberOfLogicalProcessors")).Value.ToString();
        }

        private void SetUpGpuVariables()
        {
            var firstvideoController = GetManagementClassProperties("Win32_VideoController").FirstOrDefault();

            ScreenHeight =
                firstvideoController.FirstOrDefault(x => x.Name.Equals("CurrentVerticalResolution")).Value.ToString();
            ScreenWidth =
                firstvideoController.FirstOrDefault(x => x.Name.Equals("CurrentHorizontalResolution")).Value.ToString();
            Bits = firstvideoController.FirstOrDefault(x => x.Name.Equals("CurrentBitsPerPixel")).Value.ToString();
            RefreshRate =
                firstvideoController.FirstOrDefault(x => x.Name.Equals("CurrentRefreshRate")).Value.ToString();

            var videoControllers = GetManagementClassProperties("Win32_VideoController");
            GpuList = new List<string>();

            for (var i = 0; i < videoControllers.Count(); i++)
            {
                GpuList.Add(
                    videoControllers.ElementAt(i).FirstOrDefault(x => x.Name.Equals("Description")).Value.ToString());
            }
        }

        private void SetUpEnvironmentVariables()
        {
            var screen = GetManagementClassProperties("Win32_Desktop").FirstOrDefault();
            Font = screen.FirstOrDefault(x => x.Name.Equals("IconTitleFaceName")).Value.ToString();

            var environment = GetManagementClassProperties("Win32_Environment").FirstOrDefault();
            Shell =
                environment.FirstOrDefault(x => x.Name.Equals("VariableValue"))
                    .Value.ToString()
                    .Split('\\')
                    .LastOrDefault();
        }

        private void SetUpOsVariables()
        {
            var operatingSystemProperties = GetManagementClassProperties("Win32_OperatingSystem").FirstOrDefault();

            OperatingSystem = operatingSystemProperties.FirstOrDefault(x => x.Name.Equals("Caption")).Value.ToString();
            OsBuildNumber = operatingSystemProperties.FirstOrDefault(x => x.Name.Equals("BuildNumber")).Value.ToString();
            OsArchitecture =
                operatingSystemProperties.FirstOrDefault(x => x.Name.Equals("OSArchitecture")).Value.ToString();
        }

        private void SetUpUserInfo()
        {
            var userInfo = System.Security.Principal.WindowsIdentity.GetCurrent()?.Name;
            if (userInfo != null)
            {
                var trimmedInfo = userInfo.Split('\\');

                PcName = trimmedInfo[0];
                UserName = trimmedInfo[1];
            }
        }
    }
}

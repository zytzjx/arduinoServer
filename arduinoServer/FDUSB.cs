using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace arduinoServer
{
    class FDUSB
    {
        #region structures
        [StructLayout(LayoutKind.Sequential)]
        private struct STORAGE_DEVICE_NUMBER
        {
            public int DeviceType;
            public int DeviceNumber;
            public int PartitionNumber;
        }

        private enum USB_HUB_NODE
        {
            UsbHub,
            UsbMIParent
        }

        private enum USB_CONNECTION_STATUS
        {
            NoDeviceConnected,
            DeviceConnected,
            DeviceFailedEnumeration,
            DeviceGeneralFailure,
            DeviceCausedOvercurrent,
            DeviceNotEnoughPower,
            DeviceNotEnoughBandwidth,
            DeviceHubNestedTooDeeply,
            DeviceInLegacyHub
        }

        private enum USB_DEVICE_SPEED : byte
        {
            UsbLowSpeed,
            UsbFullSpeed,
            UsbHighSpeed
        }

        private struct SP_DEVINFO_DATA
        {
            public int cbSize;

            public Guid ClassGuid;

            public int DevInst;

            public IntPtr Reserved;
        }

        private struct SP_DEVICE_INTERFACE_DATA
        {
            public int cbSize;

            public Guid InterfaceClassGuid;

            public int Flags;

            public IntPtr Reserved;
        }

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
        private struct SP_DEVICE_INTERFACE_DETAIL_DATA
        {
            public int cbSize;

            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = BUFFER_SIZE)]
            public string DevicePath;
        }

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
        private struct USB_HCD_DRIVERKEY_NAME
        {
            public int ActualLength;

            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = BUFFER_SIZE)]
            public string DriverKeyName;
        }

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
        private struct USB_ROOT_HUB_NAME
        {
            public int ActualLength;

            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = BUFFER_SIZE)]
            public string RootHubName;
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        private struct USB_HUB_DESCRIPTOR
        {
            public byte bDescriptorLength;

            public byte bDescriptorType;

            public byte bNumberOfPorts;

            public short wHubCharacteristics;

            public byte bPowerOnToPowerGood;

            public byte bHubControlCurrent;

            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 64)]
            public byte[] bRemoveAndPowerMask;
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        private struct USB_HUB_INFORMATION
        {
            public USB_HUB_DESCRIPTOR HubDescriptor;

            public byte HubIsBusPowered;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct USB_NODE_INFORMATION
        {
            public int NodeType;

            public USB_HUB_INFORMATION HubInformation;
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        private struct USB_NODE_CONNECTION_INFORMATION_EX
        {
            public int ConnectionIndex;

            public USB_DEVICE_DESCRIPTOR DeviceDescriptor;

            public byte CurrentConfigurationValue;

            public byte Speed;

            public byte DeviceIsHub;

            public short DeviceAddress;

            public int NumberOfOpenPipes;

            public int ConnectionStatus;
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        internal struct USB_DEVICE_DESCRIPTOR
        {
            public byte bLength;

            public byte bDescriptorType;

            public short bcdUSB;

            public byte bDeviceClass;

            public byte bDeviceSubClass;

            public byte bDeviceProtocol;

            public byte bMaxPacketSize0;

            public short idVendor;

            public short idProduct;

            public short bcdDevice;

            public byte iManufacturer;

            public byte iProduct;

            public byte iSerialNumber;

            public byte bNumConfigurations;
        }

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
        private struct USB_STRING_DESCRIPTOR
        {
            public byte bLength;

            public byte bDescriptorType;

            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = MAXIMUM_USB_STRING_LENGTH)]
            public string bString;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct USB_SETUP_PACKET
        {
            public byte bmRequest;
            public byte bRequest;
            public short wValue;
            public short wIndex;
            public short wLength;
        }

        private struct USB_DESCRIPTOR_REQUEST
        {
            public int ConnectionIndex;

            public USB_SETUP_PACKET SetupPacket;
        }

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
        private struct USB_NODE_CONNECTION_NAME
        {
            public int ConnectionIndex;

            public int ActualLength;

            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = BUFFER_SIZE)]
            public string NodeName;
        }

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
        private struct USB_NODE_CONNECTION_DRIVERKEY_NAME
        {
            public int ConnectionIndex;

            public int ActualLength;

            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = BUFFER_SIZE)]
            public string DriverKeyName;
        }
        #endregion structures

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static bool IsValidHandle(IntPtr handle)
        {
            // if (?:) will be eliminated by jit
            return IntPtr.Size == 4
                ? handle.ToInt32() > 0
                : handle.ToInt64() > 0;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static Int64 HandleToInt64(IntPtr handle)
        {
            // if (?:) will be eliminated by jit
            return IntPtr.Size == 4
                ? handle.ToInt32() 
                : handle.ToInt64();
        }

        public class USBController
        {
            internal int ControllerIndex;

            internal string ControllerDriverKeyName;

            internal string ControllerDevicePath;

            internal string ControllerDeviceDesc;

            public int Index => ControllerIndex;

            public string DevicePath => ControllerDevicePath;

            public string DriverKeyName => ControllerDriverKeyName;

            public string Name => ControllerDeviceDesc;

            public USBController()
            {
                ControllerIndex = 0;
                ControllerDevicePath = "";
                ControllerDeviceDesc = "";
                ControllerDriverKeyName = "";
            }

            public USBHub GetRootHub()
            {
                USBHub uSBHub = new USBHub();
                uSBHub.HubIsRootHub = true;
                uSBHub.HubDeviceDesc = "Root Hub";
                IntPtr intPtr = CreateFile(ControllerDevicePath, GENERIC_WRITE, FILE_SHARE_WRITE, IntPtr.Zero, OPEN_EXISTING, 0, IntPtr.Zero);
                if (IsValidHandle(intPtr))
                {
                    int num = Marshal.SizeOf((object)default(USB_ROOT_HUB_NAME));
                    IntPtr intPtr2 = Marshal.AllocHGlobal(num);
                    if (DeviceIoControl(intPtr, IOCTL_USB_GET_ROOT_HUB_NAME, intPtr2, num, intPtr2, num, out var lpBytesReturned, IntPtr.Zero))
                    {
                        uSBHub.HubDevicePath = "\\\\.\\" + ((USB_ROOT_HUB_NAME)Marshal.PtrToStructure(intPtr2, typeof(USB_ROOT_HUB_NAME))).RootHubName;
                    }
                    IntPtr intPtr3 = CreateFile(uSBHub.HubDevicePath, GENERIC_WRITE, FILE_SHARE_WRITE, IntPtr.Zero, OPEN_EXISTING, 0, IntPtr.Zero);
                    if (IsValidHandle(intPtr3))
                    {
                        USB_NODE_INFORMATION uSB_NODE_INFORMATION = default(USB_NODE_INFORMATION);
                        uSB_NODE_INFORMATION.NodeType = 0;
                        int num2 = Marshal.SizeOf((object)uSB_NODE_INFORMATION);
                        IntPtr intPtr4 = Marshal.AllocHGlobal(num2);
                        Marshal.StructureToPtr((object)uSB_NODE_INFORMATION, intPtr4, fDeleteOld: true);
                        if (DeviceIoControl(intPtr3, IOCTL_USB_GET_NODE_INFORMATION, intPtr4, num2, intPtr4, num2, out lpBytesReturned, IntPtr.Zero))
                        {
                            USB_NODE_INFORMATION uSB_NODE_INFORMATION2 = (USB_NODE_INFORMATION)Marshal.PtrToStructure(intPtr4, typeof(USB_NODE_INFORMATION));
                            uSBHub.HubIsBusPowered = Convert.ToBoolean(uSB_NODE_INFORMATION2.HubInformation.HubIsBusPowered);
                            uSBHub.HubPortCount = uSB_NODE_INFORMATION2.HubInformation.HubDescriptor.bNumberOfPorts;
                        }
                        Marshal.FreeHGlobal(intPtr4);
                        CloseHandle(intPtr3);
                    }
                    Marshal.FreeHGlobal(intPtr2);
                    CloseHandle(intPtr);
                }
                return uSBHub;
            }
        }

        public class USBHub
        {
            internal int HubPortCount;

            internal string HubDriverKey;

            internal string HubDevicePath;

            internal string HubDeviceDesc;

            internal string HubManufacturer;

            internal string HubProduct;

            internal string HubSerialNumber;

            internal string HubInstanceID;

            internal bool HubIsBusPowered;

            internal bool HubIsRootHub;

            public int PortCount => HubPortCount;

            public string DevicePath => HubDevicePath;

            public string DriverKey => HubDriverKey;

            public string Name => HubDeviceDesc;

            public string InstanceID => HubInstanceID;

            public bool IsBusPowered => HubIsBusPowered;

            public bool IsRootHub => HubIsRootHub;

            public string Manufacturer => HubManufacturer;

            public string Product => HubProduct;

            public string SerialNumber => HubSerialNumber;

            public USBHub()
            {
                HubPortCount = 0;
                HubDevicePath = "";
                HubDeviceDesc = "";
                HubDriverKey = "";
                HubIsBusPowered = false;
                HubIsRootHub = false;
                HubManufacturer = "";
                HubProduct = "";
                HubSerialNumber = "";
                HubInstanceID = "";
            }

            public ReadOnlyCollection<USBPort> GetUSBPorts()
            {
                List<USBPort> list = new List<USBPort>();
                IntPtr intPtr = CreateFile(HubDevicePath, GENERIC_WRITE, FILE_SHARE_WRITE, IntPtr.Zero, OPEN_EXISTING, 0, IntPtr.Zero);
                if (IsValidHandle(intPtr))
                {
                    int num = Marshal.SizeOf(typeof(USB_NODE_CONNECTION_INFORMATION_EX));
                    IntPtr intPtr2 = Marshal.AllocHGlobal(num);
                    for (int i = 1; i <= HubPortCount; i++)
                    {
                        USB_NODE_CONNECTION_INFORMATION_EX uSB_NODE_CONNECTION_INFORMATION_EX = default(USB_NODE_CONNECTION_INFORMATION_EX);
                        uSB_NODE_CONNECTION_INFORMATION_EX.ConnectionIndex = i;
                        Marshal.StructureToPtr((object)uSB_NODE_CONNECTION_INFORMATION_EX, intPtr2, fDeleteOld: true);
                        if (DeviceIoControl(intPtr, IOCTL_USB_GET_NODE_CONNECTION_INFORMATION_EX, intPtr2, num, intPtr2, num, out var _, IntPtr.Zero))
                        {
                            uSB_NODE_CONNECTION_INFORMATION_EX = (USB_NODE_CONNECTION_INFORMATION_EX)Marshal.PtrToStructure(intPtr2, typeof(USB_NODE_CONNECTION_INFORMATION_EX));
                            USBPort uSBPort = new USBPort();
                            uSBPort.PortPortNumber = i;
                            uSBPort.PortHubDevicePath = HubDevicePath;
                            USB_CONNECTION_STATUS connectionStatus = (USB_CONNECTION_STATUS)uSB_NODE_CONNECTION_INFORMATION_EX.ConnectionStatus;
                            uSBPort.PortStatus = connectionStatus.ToString();
                            USB_DEVICE_SPEED speed = (USB_DEVICE_SPEED)uSB_NODE_CONNECTION_INFORMATION_EX.Speed;
                            uSBPort.PortSpeed = speed.ToString();
                            uSBPort.PortIsDeviceConnected = uSB_NODE_CONNECTION_INFORMATION_EX.ConnectionStatus == 1;
                            uSBPort.PortIsHub = Convert.ToBoolean(uSB_NODE_CONNECTION_INFORMATION_EX.DeviceIsHub);
                            uSBPort.PortDeviceDescriptor = uSB_NODE_CONNECTION_INFORMATION_EX.DeviceDescriptor;
                            list.Add(uSBPort);
                        }
                    }
                    Marshal.FreeHGlobal(intPtr2);
                    CloseHandle(intPtr);
                }
                return new ReadOnlyCollection<USBPort>(list);
            }
        }

        public class USBPort
        {
            internal int PortPortNumber;

            internal string PortStatus;

            internal string PortHubDevicePath;

            internal string PortSpeed;

            internal bool PortIsHub;

            internal bool PortIsDeviceConnected;

            internal USB_DEVICE_DESCRIPTOR PortDeviceDescriptor;

            public int PortNumber => PortPortNumber;

            public string HubDevicePath => PortHubDevicePath;

            public string Status => PortStatus;

            public string Speed => PortSpeed;

            public bool IsHub => PortIsHub;

            public bool IsDeviceConnected => PortIsDeviceConnected;

            public USBPort()
            {
                PortPortNumber = 0;
                PortStatus = "";
                PortHubDevicePath = "";
                PortSpeed = "";
                PortIsHub = false;
                PortIsDeviceConnected = false;
            }

            public USBDevice GetDevice()
            {
                if (!PortIsDeviceConnected)
                {
                    return null;
                }
                USBDevice pCUSBDevice = new USBDevice();
                pCUSBDevice.DevicePortNumber = PortPortNumber;
                pCUSBDevice.DeviceHubDevicePath = PortHubDevicePath;
                pCUSBDevice.DeviceDescriptor = PortDeviceDescriptor;
                IntPtr intPtr = CreateFile(PortHubDevicePath, GENERIC_WRITE, FILE_SHARE_WRITE, IntPtr.Zero, OPEN_EXISTING, 0, IntPtr.Zero);
                if (IsValidHandle(intPtr))
                {
                    int num = BUFFER_SIZE;
                    string s = new string('\0', BUFFER_SIZE / Marshal.SystemDefaultCharSize);
                    int lpBytesReturned;
                    if (PortDeviceDescriptor.iManufacturer > 0)
                    {
                        USB_DESCRIPTOR_REQUEST uSB_DESCRIPTOR_REQUEST = default(USB_DESCRIPTOR_REQUEST);
                        uSB_DESCRIPTOR_REQUEST.ConnectionIndex = PortPortNumber;
                        uSB_DESCRIPTOR_REQUEST.SetupPacket.wValue = (short)(768 + PortDeviceDescriptor.iManufacturer);
                        USB_DESCRIPTOR_REQUEST uSB_DESCRIPTOR_REQUEST2 = uSB_DESCRIPTOR_REQUEST;
                        uSB_DESCRIPTOR_REQUEST2.SetupPacket.wLength = (short)(num - Marshal.SizeOf((object)uSB_DESCRIPTOR_REQUEST2));
                        uSB_DESCRIPTOR_REQUEST2.SetupPacket.wIndex = 1033;
                        IntPtr intPtr2 = Marshal.StringToHGlobalAuto(s);
                        Marshal.StructureToPtr((object)uSB_DESCRIPTOR_REQUEST2, intPtr2, fDeleteOld: true);
                        if (DeviceIoControl(intPtr, IOCTL_USB_GET_DESCRIPTOR_FROM_NODE_CONNECTION, intPtr2, num, intPtr2, num, out lpBytesReturned, IntPtr.Zero))
                        {
                            pCUSBDevice.DeviceManufacturer = ((USB_STRING_DESCRIPTOR)Marshal.PtrToStructure(new IntPtr(HandleToInt64(intPtr2) + Marshal.SizeOf((object)uSB_DESCRIPTOR_REQUEST2)), typeof(USB_STRING_DESCRIPTOR))).bString;
                        }
                        Marshal.FreeHGlobal(intPtr2);
                    }
                    if (PortDeviceDescriptor.iProduct > 0)
                    {
                        USB_DESCRIPTOR_REQUEST uSB_DESCRIPTOR_REQUEST = default(USB_DESCRIPTOR_REQUEST);
                        uSB_DESCRIPTOR_REQUEST.ConnectionIndex = PortPortNumber;
                        uSB_DESCRIPTOR_REQUEST.SetupPacket.wValue = (short)(768 + PortDeviceDescriptor.iProduct);
                        USB_DESCRIPTOR_REQUEST uSB_DESCRIPTOR_REQUEST3 = uSB_DESCRIPTOR_REQUEST;
                        uSB_DESCRIPTOR_REQUEST3.SetupPacket.wLength = (short)(num - Marshal.SizeOf((object)uSB_DESCRIPTOR_REQUEST3));
                        uSB_DESCRIPTOR_REQUEST3.SetupPacket.wIndex = 1033;
                        IntPtr intPtr3 = Marshal.StringToHGlobalAuto(s);
                        Marshal.StructureToPtr((object)uSB_DESCRIPTOR_REQUEST3, intPtr3, fDeleteOld: true);
                        if (DeviceIoControl(intPtr, IOCTL_USB_GET_DESCRIPTOR_FROM_NODE_CONNECTION, intPtr3, num, intPtr3, num, out lpBytesReturned, IntPtr.Zero))
                        {
                            pCUSBDevice.DeviceProduct = ((USB_STRING_DESCRIPTOR)Marshal.PtrToStructure(new IntPtr(HandleToInt64(intPtr3) + Marshal.SizeOf((object)uSB_DESCRIPTOR_REQUEST3)), typeof(USB_STRING_DESCRIPTOR))).bString;
                        }
                        Marshal.FreeHGlobal(intPtr3);
                    }
                    if (PortDeviceDescriptor.iSerialNumber > 0)
                    {
                        USB_DESCRIPTOR_REQUEST uSB_DESCRIPTOR_REQUEST = default(USB_DESCRIPTOR_REQUEST);
                        uSB_DESCRIPTOR_REQUEST.ConnectionIndex = PortPortNumber;
                        uSB_DESCRIPTOR_REQUEST.SetupPacket.wValue = (short)(768 + PortDeviceDescriptor.iSerialNumber);
                        USB_DESCRIPTOR_REQUEST uSB_DESCRIPTOR_REQUEST4 = uSB_DESCRIPTOR_REQUEST;
                        uSB_DESCRIPTOR_REQUEST4.SetupPacket.wLength = (short)(num - Marshal.SizeOf((object)uSB_DESCRIPTOR_REQUEST4));
                        uSB_DESCRIPTOR_REQUEST4.SetupPacket.wIndex = 1033;
                        IntPtr intPtr4 = Marshal.StringToHGlobalAuto(s);
                        Marshal.StructureToPtr((object)uSB_DESCRIPTOR_REQUEST4, intPtr4, fDeleteOld: true);
                        if (DeviceIoControl(intPtr, IOCTL_USB_GET_DESCRIPTOR_FROM_NODE_CONNECTION, intPtr4, num, intPtr4, num, out lpBytesReturned, IntPtr.Zero))
                        {
                            pCUSBDevice.DeviceSerialNumber = ((USB_STRING_DESCRIPTOR)Marshal.PtrToStructure(new IntPtr(HandleToInt64(intPtr4) + Marshal.SizeOf((object)uSB_DESCRIPTOR_REQUEST4)), typeof(USB_STRING_DESCRIPTOR))).bString;
                        }
                        Marshal.FreeHGlobal(intPtr4);
                    }
                    if (PortDeviceDescriptor.idVendor > 0)
                    {
                        pCUSBDevice.DeviceVendorId = PortDeviceDescriptor.idVendor.ToString("x4");
                    }
                    USB_NODE_CONNECTION_DRIVERKEY_NAME uSB_NODE_CONNECTION_DRIVERKEY_NAME = default(USB_NODE_CONNECTION_DRIVERKEY_NAME);
                    uSB_NODE_CONNECTION_DRIVERKEY_NAME.ConnectionIndex = PortPortNumber;
                    int num2 = Marshal.SizeOf((object)uSB_NODE_CONNECTION_DRIVERKEY_NAME);
                    IntPtr intPtr5 = Marshal.AllocHGlobal(num2);
                    Marshal.StructureToPtr((object)uSB_NODE_CONNECTION_DRIVERKEY_NAME, intPtr5, fDeleteOld: true);
                    if (DeviceIoControl(intPtr, IOCTL_USB_GET_NODE_CONNECTION_DRIVERKEY_NAME, intPtr5, num2, intPtr5, num2, out lpBytesReturned, IntPtr.Zero))
                    {
                        pCUSBDevice.DeviceDriverKey = ((USB_NODE_CONNECTION_DRIVERKEY_NAME)Marshal.PtrToStructure(intPtr5, typeof(USB_NODE_CONNECTION_DRIVERKEY_NAME))).DriverKeyName;
                        StringBuilder sb = new StringBuilder();
                        pCUSBDevice.DeviceName = GetDescriptionByKeyName(pCUSBDevice.DeviceDriverKey, sb);
                        if (sb.Length > 0)
                        {
                            pCUSBDevice.DeviceLocationpaths = sb.ToString();
                        }
                        pCUSBDevice.DeviceInstanceID = GetInstanceIDByKeyName(pCUSBDevice.DeviceDriverKey);
                    }
                    Marshal.FreeHGlobal(intPtr5);
                    CloseHandle(intPtr);
                }
                return pCUSBDevice;
            }

            public USBHub GetHub()
            {
                if (!PortIsHub)
                {
                    return null;
                }
                USBHub uSBHub = new USBHub();
                uSBHub.HubIsRootHub = false;
                uSBHub.HubDeviceDesc = "External Hub";
                IntPtr intPtr = CreateFile(PortHubDevicePath, GENERIC_WRITE, FILE_SHARE_WRITE, IntPtr.Zero, OPEN_EXISTING, 0, IntPtr.Zero);
                if (intPtr.ToInt32() != -1)
                {
                    USB_NODE_CONNECTION_NAME uSB_NODE_CONNECTION_NAME = default(USB_NODE_CONNECTION_NAME);
                    uSB_NODE_CONNECTION_NAME.ConnectionIndex = PortPortNumber;
                    int num = Marshal.SizeOf((object)uSB_NODE_CONNECTION_NAME);
                    IntPtr intPtr2 = Marshal.AllocHGlobal(num);
                    Marshal.StructureToPtr((object)uSB_NODE_CONNECTION_NAME, intPtr2, fDeleteOld: true);
                    if (DeviceIoControl(intPtr, IOCTL_USB_GET_NODE_CONNECTION_NAME, intPtr2, num, intPtr2, num, out var lpBytesReturned, IntPtr.Zero))
                    {
                        uSBHub.HubDevicePath = "\\\\.\\" + ((USB_NODE_CONNECTION_NAME)Marshal.PtrToStructure(intPtr2, typeof(USB_NODE_CONNECTION_NAME))).NodeName;
                    }
                    IntPtr intPtr3 = CreateFile(uSBHub.HubDevicePath, GENERIC_WRITE, FILE_SHARE_WRITE, IntPtr.Zero, OPEN_EXISTING, 0, IntPtr.Zero);
                    if (IsValidHandle(intPtr3))
                    {
                        USB_NODE_INFORMATION uSB_NODE_INFORMATION = default(USB_NODE_INFORMATION);
                        uSB_NODE_INFORMATION.NodeType = 0;
                        int num2 = Marshal.SizeOf((object)uSB_NODE_INFORMATION);
                        IntPtr intPtr4 = Marshal.AllocHGlobal(num2);
                        Marshal.StructureToPtr((object)uSB_NODE_INFORMATION, intPtr4, fDeleteOld: true);
                        if (DeviceIoControl(intPtr3, IOCTL_USB_GET_NODE_INFORMATION, intPtr4, num2, intPtr4, num2, out lpBytesReturned, IntPtr.Zero))
                        {
                            uSB_NODE_INFORMATION = (USB_NODE_INFORMATION)Marshal.PtrToStructure(intPtr4, typeof(USB_NODE_INFORMATION));
                            uSBHub.HubIsBusPowered = Convert.ToBoolean(uSB_NODE_INFORMATION.HubInformation.HubIsBusPowered);
                            uSBHub.HubPortCount = uSB_NODE_INFORMATION.HubInformation.HubDescriptor.bNumberOfPorts;
                        }
                        Marshal.FreeHGlobal(intPtr4);
                        CloseHandle(intPtr3);
                    }
                    USBDevice device = GetDevice();
                    uSBHub.HubInstanceID = device.DeviceInstanceID;
                    uSBHub.HubManufacturer = device.Manufacturer;
                    uSBHub.HubProduct = device.Product;
                    uSBHub.HubSerialNumber = device.SerialNumber;
                    uSBHub.HubDriverKey = device.DriverKey;
                    Marshal.FreeHGlobal(intPtr2);
                    CloseHandle(intPtr);
                }
                return uSBHub;
            }
        }

        public class USBDevice
        {
            internal int DevicePortNumber;

            internal string DeviceDriverKey;

            internal string DeviceHubDevicePath;

            internal string DeviceInstanceID;

            internal string DeviceName;

            internal string DeviceManufacturer;

            internal string DeviceProduct;

            internal string DeviceSerialNumber;

            internal string DeviceVendorId;

            internal USB_DEVICE_DESCRIPTOR DeviceDescriptor;

            internal string DeviceLocationpaths;

            public int PortNumber => DevicePortNumber;

            public string HubDevicePath => DeviceHubDevicePath;

            public string DriverKey => DeviceDriverKey;

            public string InstanceID => DeviceInstanceID;

            public string Name => DeviceName;

            public string Manufacturer => DeviceManufacturer;

            public string Product => DeviceProduct;

            public string SerialNumber => DeviceSerialNumber;

            public string VendorId => DeviceVendorId;

            public string Locationpaths => DeviceLocationpaths;

            public USBDevice()
            {
                DevicePortNumber = 0;
                DeviceHubDevicePath = "";
                DeviceDriverKey = "";
                DeviceManufacturer = "";
                DeviceProduct = "Unknown USB Device";
                DeviceSerialNumber = "";
                DeviceName = "";
                DeviceInstanceID = "";
                DeviceVendorId = "";
                DeviceLocationpaths = "";
            }

            override public String ToString()
            {
                StringBuilder sb = new StringBuilder();
                sb.Append("{").Append("PortNumber=").Append("\"").Append(DevicePortNumber).Append("\"").Append(",")
                    .Append("HubDevicePath=").Append("\"").Append(HubDevicePath).Append("\"").Append(",")
                    .Append("DriverKey=").Append("\"").Append(DriverKey).Append("\"").Append(",")
                    .Append("InstanceID=").Append("\"").Append(InstanceID).Append("\"").Append(",")
                    .Append("Name=").Append("\"").Append(Name).Append("\"").Append(",")
                    .Append("Manufacturer=").Append("\"").Append(Manufacturer).Append("\"").Append(",")
                    .Append("Product=").Append("\"").Append(Product).Append("\"").Append(",")
                    .Append("SerialNumber=").Append("\"").Append(SerialNumber).Append("\"").Append(",")
                    .Append("Locationpaths=").Append("\"").Append(Locationpaths).Append("\"").Append(",")
                    .Append("VendorId=").Append("\"").Append(VendorId).Append("\"").Append("}");

                return sb.ToString();
            }
        }
        #region api const
        private const int IOCTL_STORAGE_GET_DEVICE_NUMBER = 2953344;
        private const string GUID_DEVINTERFACE_DISK = "53f56307-b6bf-11d0-94f2-00a0c91efb8b";
        private const int GENERIC_WRITE = 1073741824;
        private const int FILE_SHARE_READ = 1;
        private const int FILE_SHARE_WRITE = 2;
        private const int OPEN_EXISTING = 3;
        private const int INVALID_HANDLE_VALUE = -1;
        private const int IOCTL_GET_HCD_DRIVERKEY_NAME = 2229284;
        private const int IOCTL_USB_GET_ROOT_HUB_NAME = 2229256;
        private const int IOCTL_USB_GET_NODE_INFORMATION = 2229256;
        private const int IOCTL_USB_GET_NODE_CONNECTION_INFORMATION_EX = 2229320;
        private const int IOCTL_USB_GET_DESCRIPTOR_FROM_NODE_CONNECTION = 2229264;
        private const int IOCTL_USB_GET_NODE_CONNECTION_NAME = 2229268;
        private const int IOCTL_USB_GET_NODE_CONNECTION_DRIVERKEY_NAME = 2229280;
        private const int USB_DEVICE_DESCRIPTOR_TYPE = 1;
        private const int USB_STRING_DESCRIPTOR_TYPE = 3;
        private const int BUFFER_SIZE = 2048;
        private const int MAXIMUM_USB_STRING_LENGTH = 255;
        private const string GUID_DEVINTERFACE_HUBCONTROLLER = "3abf6f2d-71c4-462a-8a92-1e6861e6af27";
        private const string REGSTR_KEY_USB = "USB";
        private const int DIGCF_PRESENT = 2;
        private const int DIGCF_ALLCLASSES = 4;
        private const int DIGCF_DEVICEINTERFACE = 16;
        private const int SPDRP_DRIVER = 9;
        private const int SPDRP_DEVICEDESC = 0;
        private const int SPDRP_LOCATION_PATHS = 0x00000023;
        private const int REG_SZ = 1;
        #endregion api const
        public static List<USBDevice> ListConnectedDevices()
        {
            List<USBDevice> list = new List<USBDevice>();
            foreach (USBController uSBHostController in GetUSBHostControllers())
            {
                USBListHub(uSBHostController.GetRootHub(), list);
            }
            return list;
        }

        private static void USBListHub(USBHub Hub, List<USBDevice> DevList)
        {
            foreach (USBPort uSBPort in Hub.GetUSBPorts())
            {
                if (uSBPort.IsHub)
                {
                    USBListHub(uSBPort.GetHub(), DevList);
                }
                else if (uSBPort.IsDeviceConnected)
                {
                    DevList.Add(uSBPort.GetDevice());
                }
            }
        }
        #region setupapi
        [DllImport("setupapi.dll")]
        private static extern int CM_Get_Parent(out IntPtr pdnDevInst, int dnDevInst, int ulFlags);

        [DllImport("setupapi.dll", CharSet = CharSet.Auto)]
        private static extern int CM_Get_Device_ID(IntPtr dnDevInst, IntPtr Buffer, int BufferLen, int ulFlags);

        [DllImport("setupapi.dll", CharSet = CharSet.Auto)]
        private static extern IntPtr SetupDiGetClassDevs(ref Guid ClassGuid, int Enumerator, IntPtr hwndParent, int Flags);

        [DllImport("setupapi.dll", CharSet = CharSet.Auto)]
        private static extern IntPtr SetupDiGetClassDevs(int ClassGuid, string Enumerator, IntPtr hwndParent, int Flags);

        [DllImport("setupapi.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern bool SetupDiEnumDeviceInterfaces(IntPtr DeviceInfoSet, IntPtr DeviceInfoData, ref Guid InterfaceClassGuid, int MemberIndex, ref SP_DEVICE_INTERFACE_DATA DeviceInterfaceData);

        [DllImport("setupapi.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern bool SetupDiGetDeviceInterfaceDetail(IntPtr DeviceInfoSet, ref SP_DEVICE_INTERFACE_DATA DeviceInterfaceData, ref SP_DEVICE_INTERFACE_DETAIL_DATA DeviceInterfaceDetailData, int DeviceInterfaceDetailDataSize, ref int RequiredSize, ref SP_DEVINFO_DATA DeviceInfoData);

        [DllImport("setupapi.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern bool SetupDiGetDeviceRegistryProperty(IntPtr DeviceInfoSet, ref SP_DEVINFO_DATA DeviceInfoData, int iProperty, ref int PropertyRegDataType, IntPtr PropertyBuffer, int PropertyBufferSize, ref int RequiredSize);

        [DllImport("setupapi.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern bool SetupDiEnumDeviceInfo(IntPtr DeviceInfoSet, int MemberIndex, ref SP_DEVINFO_DATA DeviceInfoData);

        [DllImport("setupapi.dll", SetLastError = true)]
        private static extern bool SetupDiDestroyDeviceInfoList(IntPtr DeviceInfoSet);

        [DllImport("setupapi.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern bool SetupDiGetDeviceInstanceId(IntPtr DeviceInfoSet, ref SP_DEVINFO_DATA DeviceInfoData, StringBuilder DeviceInstanceId, int DeviceInstanceIdSize, out int RequiredSize);

        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern bool DeviceIoControl(IntPtr hDevice, int dwIoControlCode, IntPtr lpInBuffer, int nInBufferSize, IntPtr lpOutBuffer, int nOutBufferSize, out int lpBytesReturned, IntPtr lpOverlapped);

        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr CreateFile(string lpFileName, int dwDesiredAccess, int dwShareMode, IntPtr lpSecurityAttributes, int dwCreationDisposition, int dwFlagsAndAttributes, IntPtr hTemplateFile);

        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern bool CloseHandle(IntPtr hObject);
        #endregion setupapi
        public static ReadOnlyCollection<USBController> GetUSBHostControllers()
        {
            List<USBController> list = new List<USBController>();
            Guid ClassGuid = new Guid(GUID_DEVINTERFACE_HUBCONTROLLER);
            IntPtr deviceInfoSet = SetupDiGetClassDevs(ref ClassGuid, 0, IntPtr.Zero, DIGCF_PRESENT | DIGCF_DEVICEINTERFACE);
            if (deviceInfoSet != IntPtr.Zero)
            {
                IntPtr intPtr = Marshal.AllocHGlobal(BUFFER_SIZE);
                int num = 0;
                bool flag;
                do
                {
                    USBController uSBController = new USBController();
                    uSBController.ControllerIndex = num;
                    SP_DEVICE_INTERFACE_DATA DeviceInterfaceData = default(SP_DEVICE_INTERFACE_DATA);
                    DeviceInterfaceData.cbSize = Marshal.SizeOf((object)DeviceInterfaceData);
                    flag = SetupDiEnumDeviceInterfaces(deviceInfoSet, IntPtr.Zero, ref ClassGuid, num, ref DeviceInterfaceData);
                    if (flag)
                    {
                        SP_DEVINFO_DATA DeviceInfoData = default(SP_DEVINFO_DATA);
                        DeviceInfoData.cbSize = Marshal.SizeOf((object)DeviceInfoData);
                        SP_DEVICE_INTERFACE_DETAIL_DATA DeviceInterfaceDetailData = default(SP_DEVICE_INTERFACE_DETAIL_DATA);
                        DeviceInterfaceDetailData.cbSize = ((IntPtr.Size == 4) ? (4 + Marshal.SystemDefaultCharSize) : 8);
                        int RequiredSize = 0;
                        int deviceInterfaceDetailDataSize = BUFFER_SIZE;
                        if (SetupDiGetDeviceInterfaceDetail(deviceInfoSet, ref DeviceInterfaceData, ref DeviceInterfaceDetailData, deviceInterfaceDetailDataSize, ref RequiredSize, ref DeviceInfoData))
                        {
                            uSBController.ControllerDevicePath = DeviceInterfaceDetailData.DevicePath;
                            int RequiredSize2 = 0;
                            int PropertyRegDataType = 1;
                            if (SetupDiGetDeviceRegistryProperty(deviceInfoSet, ref DeviceInfoData, SPDRP_DEVICEDESC, ref PropertyRegDataType, intPtr, BUFFER_SIZE, ref RequiredSize2))
                            {
                                uSBController.ControllerDeviceDesc = Marshal.PtrToStringAuto(intPtr);
                            }
                            if (SetupDiGetDeviceRegistryProperty(deviceInfoSet, ref DeviceInfoData, SPDRP_DRIVER, ref PropertyRegDataType, intPtr, BUFFER_SIZE, ref RequiredSize2))
                            {
                                uSBController.ControllerDriverKeyName = Marshal.PtrToStringAuto(intPtr);
                            }
                        }
                        list.Add(uSBController);
                    }
                    num++;
                }
                while (flag);
                Marshal.FreeHGlobal(intPtr);
                SetupDiDestroyDeviceInfoList(deviceInfoSet);
            }
            return new ReadOnlyCollection<USBController>(list);
        }

        private static string GetDescriptionByKeyName(string DriverKeyName, StringBuilder sblocationpaths)
        {
            string result = "";
            IntPtr deviceInfoSet = SetupDiGetClassDevs(0, "USB", IntPtr.Zero, DIGCF_PRESENT | DIGCF_ALLCLASSES);
            if (deviceInfoSet != IntPtr.Zero)
            {
                IntPtr intPtr = Marshal.AllocHGlobal(BUFFER_SIZE);
                int num = 0;
                bool flag;
                do
                {
                    SP_DEVINFO_DATA DeviceInfoData = default(SP_DEVINFO_DATA);
                    DeviceInfoData.cbSize = Marshal.SizeOf((object)DeviceInfoData);
                    flag = SetupDiEnumDeviceInfo(deviceInfoSet, num, ref DeviceInfoData);
                    if (flag)
                    {
                        int RequiredSize = 0;
                        int PropertyRegDataType = 1;
                        string text = "";
                        if (SetupDiGetDeviceRegistryProperty(deviceInfoSet, ref DeviceInfoData, SPDRP_DRIVER, ref PropertyRegDataType, intPtr, BUFFER_SIZE, ref RequiredSize))
                        {
                            text = Marshal.PtrToStringAuto(intPtr);
                        }
                        if (text == DriverKeyName)
                        {
                            if (SetupDiGetDeviceRegistryProperty(deviceInfoSet, ref DeviceInfoData, SPDRP_DEVICEDESC, ref PropertyRegDataType, intPtr, BUFFER_SIZE, ref RequiredSize))
                            {
                                result = Marshal.PtrToStringAuto(intPtr);
                            }
                            if (SetupDiGetDeviceRegistryProperty(deviceInfoSet, ref DeviceInfoData, SPDRP_LOCATION_PATHS, ref PropertyRegDataType, intPtr, BUFFER_SIZE, ref RequiredSize))
                            {
                                sblocationpaths.Append(Marshal.PtrToStringAuto(intPtr));
                            }
                            break;
                        }
                    }
                    num++;
                }
                while (flag);
                Marshal.FreeHGlobal(intPtr);
                SetupDiDestroyDeviceInfoList(deviceInfoSet);
            }
            return result;
        }

        private static string GetInstanceIDByKeyName(string DriverKeyName)
        {
            string result = "";
            IntPtr deviceInfoSet = SetupDiGetClassDevs(0, "USB", IntPtr.Zero, DIGCF_PRESENT | DIGCF_ALLCLASSES);
            if (deviceInfoSet != IntPtr.Zero)
            {
                IntPtr intPtr = Marshal.AllocHGlobal(BUFFER_SIZE);
                int num = 0;
                bool flag;
                do
                {
                    SP_DEVINFO_DATA DeviceInfoData = default(SP_DEVINFO_DATA);
                    DeviceInfoData.cbSize = Marshal.SizeOf((object)DeviceInfoData);
                    flag = SetupDiEnumDeviceInfo(deviceInfoSet, num, ref DeviceInfoData);
                    if (flag)
                    {
                        int RequiredSize = 0;
                        int PropertyRegDataType = 1;
                        string text = "";
                        if (SetupDiGetDeviceRegistryProperty(deviceInfoSet, ref DeviceInfoData, SPDRP_DRIVER, ref PropertyRegDataType, intPtr, BUFFER_SIZE, ref RequiredSize))
                        {
                            text = Marshal.PtrToStringAuto(intPtr);
                        }
                        if (text == DriverKeyName)
                        {
                            int num2 = BUFFER_SIZE;
                            StringBuilder stringBuilder = new StringBuilder(num2);
                            SetupDiGetDeviceInstanceId(deviceInfoSet, ref DeviceInfoData, stringBuilder, num2, out RequiredSize);
                            result = stringBuilder.ToString();
                            break;
                        }
                    }
                    num++;
                }
                while (flag);
                Marshal.FreeHGlobal(intPtr);
                SetupDiDestroyDeviceInfoList(deviceInfoSet);
            }
            return result;
        }

        public static string GetComFromInstanceID(string sinstanceid)
        {
            RegistryKey myKey = Registry.LocalMachine.OpenSubKey($@"SYSTEM\CurrentControlSet\Enum\{sinstanceid}\Device Parameters", false);
            String value = (String)myKey.GetValue("PortName");
            myKey.Close();
            return value;
        }

    }
}

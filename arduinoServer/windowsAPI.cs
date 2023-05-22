using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace arduinoServer
{
    class windowsAPI
    {
        #region DEVPROPKEY and DEVPROP_TYPE definitions

        [StructLayout(LayoutKind.Sequential)]
        internal struct DEVPROPKEY
        {
            public Guid guid;
            public int pid;

            public DEVPROPKEY(Guid guid, int pid)
            {
                this.guid = guid;
                this.pid = pid;
            }

            public DEVPROPKEY(uint a, ushort b, ushort c, byte d, byte e, byte f, byte g, byte h, byte i, byte j, byte k, int pid)
            {
                guid = new Guid(a, b, c, d, e, f, g, h, i, j, k);
                this.pid = pid;
            }
        }

        private const int DEVPROP_TYPEMOD_ARRAY = 0x00001000; // Array of fixed sized data elements
        private const int DEVPROP_TYPEMOD_LIST = 0x00002000; // List of variable-sized data elements

        internal enum DEVPROP_TYPE : int
        {
            EMPTY = 0x0000_0000, // Nothing, no property data
            NULL = 0x0000_0001, // Null property data
            SBYTE = 0x0000_0002, // 8-bit signed int (SBYTE)
            BYTE = 0x0000_0003, // 8-bit unsigned int (BYTE)
            INT16 = 0x0000_0004, // 16-bit signed int (SHORT)
            UINT16 = 0x0000_0005, // 16-bit unsigned int (USHORT)
            INT32 = 0x0000_0006, // 32-bit signed int (LONG)
            UINT32 = 0x0000_0007, // 32-bit unsigned int (ULONG)
            INT64 = 0x0000_0008, // 64-bit signed int (LONG64)
            UINT64 = 0x0000_0009, // 64-bit unsigned int (ULONG64)
            FLOAT = 0x0000_000A, // 32-bit floating point (FLOAT)
            DOUBLE = 0x0000_000B, // 64-bit floating point (DOUBLE)
            DECIMAL = 0x0000_000C, // 128-bit floating point (DECIMAL)
            GUID = 0x0000_000D, // 128-bit unique identifier (GUID)
            CURRENCY = 0x0000_000E, // 64 bit signed int currency value (CURRENCY)
            DATE = 0x0000_000F, // Date (DATE)
            FILETIME = 0x0000_0010, // File time (FILETIME)
            BOOLEAN = 0x0000_0011, // 8-bit boolean (DEVPROP_BOOLEAN)
            STRING = 0x0000_0012, // Null-terminated-string
            STRING_LIST = STRING | DEVPROP_TYPEMOD_LIST, // Multi-sz-string list
            SECURITY_DESCRIPTOR = 0x0000_0013, // Self-relative binary SECURITY_DESCRIPTOR
            SECURITY_DESCRIPTOR_STRING = 0x0000_0014, // Security descriptor string (SDDL format)
            DEVPROPKEY = 0x0000_0015, // Device property key (DEVPROPKEY)
            DEVPROPTYPE = 0x0000_0016, // Device property type (DEVPROPTYPE)
            BINARY = BYTE | DEVPROP_TYPEMOD_ARRAY, // Custom binary data
            ERROR = 0x0000_0017, // 32-bit Win32 system error code
            NTSTATUS = 0x0000_0018, // 32-bit NTSTATUS code
            STRING_INDIRECT = 0x0000_0019, // String resource (@[path\]<dllname>,-<strId>)
        }

        #endregion
        #region DEVPROPKEY_DEVICE Define
        //
        // Device properties
        //
        // These DEVPKEYs corespond to the SetupAPI SPDRP_XXX device properties
        //
        internal static  DEVPROPKEY DEVPKEY_Device_DeviceDesc = new DEVPROPKEY(0xa45c254e, 0xdf1c, 0x4efd, 0x80, 0x20, 0x67, 0xd1, 0x46, 0xa8, 0x50, 0xe0, 2); // DEVPROP_TYPE.STRING
        internal static  DEVPROPKEY DEVPKEY_Device_HardwareIds = new DEVPROPKEY(0xa45c254e, 0xdf1c, 0x4efd, 0x80, 0x20, 0x67, 0xd1, 0x46, 0xa8, 0x50, 0xe0, 3); // DEVPROP_TYPE.STRING_LIST
        internal static  DEVPROPKEY DEVPKEY_Device_CompatibleIds = new DEVPROPKEY(0xa45c254e, 0xdf1c, 0x4efd, 0x80, 0x20, 0x67, 0xd1, 0x46, 0xa8, 0x50, 0xe0, 4); // DEVPROP_TYPE.STRING_LIST
        internal static  DEVPROPKEY DEVPKEY_Device_Service = new DEVPROPKEY(0xa45c254e, 0xdf1c, 0x4efd, 0x80, 0x20, 0x67, 0xd1, 0x46, 0xa8, 0x50, 0xe0, 6); // DEVPROP_TYPE.STRING
        internal static  DEVPROPKEY DEVPKEY_Device_Class = new DEVPROPKEY(0xa45c254e, 0xdf1c, 0x4efd, 0x80, 0x20, 0x67, 0xd1, 0x46, 0xa8, 0x50, 0xe0, 9); // DEVPROP_TYPE.STRING
        internal static  DEVPROPKEY DEVPKEY_Device_ClassGuid = new DEVPROPKEY(0xa45c254e, 0xdf1c, 0x4efd, 0x80, 0x20, 0x67, 0xd1, 0x46, 0xa8, 0x50, 0xe0, 10); // DEVPROP_TYPE.GUID
        internal static  DEVPROPKEY DEVPKEY_Device_Driver = new DEVPROPKEY(0xa45c254e, 0xdf1c, 0x4efd, 0x80, 0x20, 0x67, 0xd1, 0x46, 0xa8, 0x50, 0xe0, 11); // DEVPROP_TYPE.STRING
        internal static  DEVPROPKEY DEVPKEY_Device_Manufacturer = new DEVPROPKEY(0xa45c254e, 0xdf1c, 0x4efd, 0x80, 0x20, 0x67, 0xd1, 0x46, 0xa8, 0x50, 0xe0, 13); // DEVPROP_TYPE.STRING
        internal static  DEVPROPKEY DEVPKEY_Device_FriendlyName = new DEVPROPKEY(0xa45c254e, 0xdf1c, 0x4efd, 0x80, 0x20, 0x67, 0xd1, 0x46, 0xa8, 0x50, 0xe0, 14); // DEVPROP_TYPE.STRING
        internal static  DEVPROPKEY DEVPKEY_Device_LocationInfo = new DEVPROPKEY(0xa45c254e, 0xdf1c, 0x4efd, 0x80, 0x20, 0x67, 0xd1, 0x46, 0xa8, 0x50, 0xe0, 15); // DEVPROP_TYPE.STRING
        internal static  DEVPROPKEY DEVPKEY_Device_PDOName = new DEVPROPKEY(0xa45c254e, 0xdf1c, 0x4efd, 0x80, 0x20, 0x67, 0xd1, 0x46, 0xa8, 0x50, 0xe0, 16); // DEVPROP_TYPE.STRING
        internal static  DEVPROPKEY DEVPKEY_Device_LocationPaths = new DEVPROPKEY(0xa45c254e, 0xdf1c, 0x4efd, 0x80, 0x20, 0x67, 0xd1, 0x46, 0xa8, 0x50, 0xe0, 37); // DEVPROP_TYPE.STRING_LIST
        internal static  DEVPROPKEY DEVPKEY_Device_InstanceId = new DEVPROPKEY(0x78c34fc8, 0x104a, 0x4aca, 0x9e, 0xa4, 0x52, 0x4d, 0x52, 0x99, 0x6e, 0x57, 256); // DEVPROP_TYPE.STRING

        //
        // Device properties
        //
        internal static  DEVPROPKEY DEVPKEY_Device_DevNodeStatus = new DEVPROPKEY(0x4340a6c5, 0x93fa, 0x4706, 0x97, 0x2c, 0x7b, 0x64, 0x80, 0x08, 0xa5, 0xa7, 2); // DEVPROP_TYPE.UINT32
        internal static  DEVPROPKEY DEVPKEY_Device_ProblemCode = new DEVPROPKEY(0x4340a6c5, 0x93fa, 0x4706, 0x97, 0x2c, 0x7b, 0x64, 0x80, 0x08, 0xa5, 0xa7, 3); // DEVPROP_TYPE.UINT32
        internal static  DEVPROPKEY DEVPKEY_Device_Parent = new DEVPROPKEY(0x4340a6c5, 0x93fa, 0x4706, 0x97, 0x2c, 0x7b, 0x64, 0x80, 0x08, 0xa5, 0xa7, 8); // DEVPROP_TYPE.STRING
        internal static  DEVPROPKEY DEVPKEY_Device_Children = new DEVPROPKEY(0x4340a6c5, 0x93fa, 0x4706, 0x97, 0x2c, 0x7b, 0x64, 0x80, 0x08, 0xa5, 0xa7, 9); // DEVPROP_TYPE.STRING_LIST
        internal static  DEVPROPKEY DEVPKEY_Device_Siblings = new DEVPROPKEY(0x4340a6c5, 0x93fa, 0x4706, 0x97, 0x2c, 0x7b, 0x64, 0x80, 0x08, 0xa5, 0xa7, 10); // DEVPROP_TYPE.STRING_LIST
        internal static  DEVPROPKEY DEVPKEY_Device_Model = new DEVPROPKEY(0x78c34fc8, 0x104a, 0x4aca, 0x9e, 0xa4, 0x52, 0x4d, 0x52, 0x99, 0x6e, 0x57, 39); // DEVPROP_TYPE.STRING
        internal static  DEVPROPKEY DEVPKEY_Device_ContainerId = new DEVPROPKEY(0x8c7ed206, 0x3f8a, 0x4827, 0xb3, 0xab, 0xae, 0x9e, 0x1f, 0xae, 0xfc, 0x6c, 2);     // DEVPROP_TYPE_GUID

        //
        // HID specific
        //
        internal static  DEVPROPKEY DEVPKEY_DeviceInterface_HID_UsagePage = new DEVPROPKEY(0xcbf38310, 0x4a17, 0x4310, 0xa1, 0xeb, 0x24, 0x7f, 0xb, 0x67, 0x59, 0x3b, 2); // DEVPROP_TYPE.UINT16
        internal static  DEVPROPKEY DEVPKEY_DeviceInterface_HID_UsageId = new DEVPROPKEY(0xcbf38310, 0x4a17, 0x4310, 0xa1, 0xeb, 0x24, 0x7f, 0xb, 0x67, 0x59, 0x3b, 3); // DEVPROP_TYPE.UINT16
        internal static  DEVPROPKEY DEVPKEY_DeviceInterface_HID_IsReadOnly = new DEVPROPKEY(0xcbf38310, 0x4a17, 0x4310, 0xa1, 0xeb, 0x24, 0x7f, 0xb, 0x67, 0x59, 0x3b, 4); // DEVPROP_TYPE.BOOLEAN
        internal static  DEVPROPKEY DEVPKEY_DeviceInterface_HID_VendorId = new DEVPROPKEY(0xcbf38310, 0x4a17, 0x4310, 0xa1, 0xeb, 0x24, 0x7f, 0xb, 0x67, 0x59, 0x3b, 5); // DEVPROP_TYPE.UINT16
        internal static  DEVPROPKEY DEVPKEY_DeviceInterface_HID_ProductId = new DEVPROPKEY(0xcbf38310, 0x4a17, 0x4310, 0xa1, 0xeb, 0x24, 0x7f, 0xb, 0x67, 0x59, 0x3b, 6); // DEVPROP_TYPE.UINT16
        internal static  DEVPROPKEY DEVPKEY_DeviceInterface_HID_VersionNumber = new DEVPROPKEY(0xcbf38310, 0x4a17, 0x4310, 0xa1, 0xeb, 0x24, 0x7f, 0xb, 0x67, 0x59, 0x3b, 7); // DEVPROP_TYPE.UINT16
        internal static  DEVPROPKEY DEVPKEY_DeviceInterface_HID_BackgroundAccess = new DEVPROPKEY(0xcbf38310, 0x4a17, 0x4310, 0xa1, 0xeb, 0x24, 0x7f, 0xb, 0x67, 0x59, 0x3b, 8); // DEVPROP_TYPE.BOOLEAN
        #endregion
        #region "Win32API"

        #region constants
        public const int NO_ERROR = 0;
        public const int ERROR_FILE_NOT_FOUND = 2;

        public const uint STANDARD_RIGHTS_REQUIRED = 0x000F0000;
        public const uint SYNCHRONIZE = 0x00100000;
        public const uint EVENT_ALL_ACCESS = (STANDARD_RIGHTS_REQUIRED | SYNCHRONIZE | 0x3);
        public const uint EVENT_MODIFY_STATE = 0x0002;

        public const uint GENERIC_READ = 0x80000000;
        public const uint GENERIC_WRITE = 0x40000000;
        public const uint GENERIC_EXECUTE = 0x20000000;
        public const uint GENERIC_ALL = 0x10000000;
        public const uint FILE_FLAG_NO_BUFFERING = 0x20000000;
        public const Int32 FILE_ATTRIBUTE_NORMAL = 0X80;
        public const Int32 FILE_FLAG_OVERLAPPED = 0X40000000;

        public const int FILE_SHARE_READ = 0x1;
        public const int FILE_SHARE_WRITE = 0x2;
        public const int OPEN_EXISTING = 0x3;
        public const int INVALID_HANDLE_VALUE = -1;

        public const int USBUSER_GET_CONTROLLER_INFO_0 = 0x00000001;
        public const int USBUSER_GET_CONTROLLER_DRIVER_KEY = 0x00000002;

        public const int IOCTL_GET_HCD_DRIVERKEY_NAME = 0x220424;
        public const int IOCTL_USB_GET_ROOT_HUB_NAME = 0x220408;
        public const int IOCTL_USB_GET_NODE_INFORMATION = 0x220408;
        public const int IOCTL_USB_GET_NODE_CONNECTION_INFORMATION_EX = 0x220448;
        public const int IOCTL_USB_GET_DESCRIPTOR_FROM_NODE_CONNECTION = 0x220410;
        public const int IOCTL_USB_GET_NODE_CONNECTION_NAME = 0x220414;
        public const int IOCTL_USB_GET_NODE_CONNECTION_DRIVERKEY_NAME = 0x220420;
        public const int IOCTL_STORAGE_GET_DEVICE_NUMBER = 0x2D1080;

        public const int USB_DEVICE_DESCRIPTOR_TYPE = 0x1;
        public const int USB_CONFIGURATION_DESCRIPTOR_TYPE = 0x2;
        public const int USB_STRING_DESCRIPTOR_TYPE = 0x3;
        public const int USB_INTERFACE_DESCRIPTOR_TYPE = 0x4;
        public const int USB_ENDPOINT_DESCRIPTOR_TYPE = 0x5;

        public const string GUID_DEVINTERFACE_HUBCONTROLLER = "3abf6f2d-71c4-462a-8a92-1e6861e6af27";
        public const int MAX_BUFFER_SIZE = 2048;
        public const int MAXIMUM_USB_STRING_LENGTH = 255;
        public const string REGSTR_KEY_USB = "USB";
        public const int REG_SZ = 1;
        public const int DIF_PROPERTYCHANGE = 0x00000012;
        public const int DICS_FLAG_GLOBAL = 0x00000001;

        public const int DIGCF_DEFAULT = 0x00000001;  // only valid with DIGCF_DEVICEINTERFACE
        public const int DIGCF_PRESENT = 0x00000002;
        public const int DIGCF_ALLCLASSES = 0x00000004;
        public const int DIGCF_PROFILE = 0x00000008;
        public const int DIGCF_DEVICEINTERFACE = 0x00000010;

        public enum SetupDiGetDeviceRegistryPropertyEnum : uint
        {
            SPDRP_DEVICEDESC = 0x00000000, // DeviceDesc (R/W)
            SPDRP_HARDWAREID = 0x00000001, // HardwareID (R/W)
            SPDRP_COMPATIBLEIDS = 0x00000002, // CompatibleIDs (R/W)
            SPDRP_UNUSED0 = 0x00000003, // unused
            SPDRP_SERVICE = 0x00000004, // Service (R/W)
            SPDRP_UNUSED1 = 0x00000005, // unused
            SPDRP_UNUSED2 = 0x00000006, // unused
            SPDRP_CLASS = 0x00000007, // Class (R--tied to ClassGUID)
            SPDRP_CLASSGUID = 0x00000008, // ClassGUID (R/W)
            SPDRP_DRIVER = 0x00000009, // Driver (R/W)
            SPDRP_CONFIGFLAGS = 0x0000000A, // ConfigFlags (R/W)
            SPDRP_MFG = 0x0000000B, // Mfg (R/W)
            SPDRP_FRIENDLYNAME = 0x0000000C, // FriendlyName (R/W)
            SPDRP_LOCATION_INFORMATION = 0x0000000D, // LocationInformation (R/W)
            SPDRP_PHYSICAL_DEVICE_OBJECT_NAME = 0x0000000E, // PhysicalDeviceObjectName (R)
            SPDRP_CAPABILITIES = 0x0000000F, // Capabilities (R)
            SPDRP_UI_NUMBER = 0x00000010, // UiNumber (R)
            SPDRP_UPPERFILTERS = 0x00000011, // UpperFilters (R/W)
            SPDRP_LOWERFILTERS = 0x00000012, // LowerFilters (R/W)
            SPDRP_BUSTYPEGUID = 0x00000013, // BusTypeGUID (R)
            SPDRP_LEGACYBUSTYPE = 0x00000014, // LegacyBusType (R)
            SPDRP_BUSNUMBER = 0x00000015, // BusNumber (R)
            SPDRP_ENUMERATOR_NAME = 0x00000016, // Enumerator Name (R)
            SPDRP_SECURITY = 0x00000017, // Security (R/W, binary form)
            SPDRP_SECURITY_SDS = 0x00000018, // Security (W, SDS form)
            SPDRP_DEVTYPE = 0x00000019, // Device Type (R/W)
            SPDRP_EXCLUSIVE = 0x0000001A, // Device is exclusive-access (R/W)
            SPDRP_CHARACTERISTICS = 0x0000001B, // Device Characteristics (R/W)
            SPDRP_ADDRESS = 0x0000001C, // Device Address (R)
            SPDRP_UI_NUMBER_DESC_FORMAT = 0X0000001D, // UiNumberDescFormat (R/W)
            SPDRP_DEVICE_POWER_DATA = 0x0000001E, // Device Power Data (R)
            SPDRP_REMOVAL_POLICY = 0x0000001F, // Removal Policy (R)
            SPDRP_REMOVAL_POLICY_HW_DEFAULT = 0x00000020, // Hardware Removal Policy (R)
            SPDRP_REMOVAL_POLICY_OVERRIDE = 0x00000021, // Removal Policy Override (RW)
            SPDRP_INSTALL_STATE = 0x00000022, // Device Install State (R)
            SPDRP_LOCATION_PATHS = 0x00000023, // Device Location Paths (R)
            SPDRP_BASE_CONTAINERID = 0x00000024  // Base ContainerID (R)
        }
        public enum CMGetDevNodeRegistryPropertyEnum : uint
        {
            CM_DRP_DEVICEDESC = 0x00000001, // DeviceDesc REG_SZ property =RW,
            CM_DRP_HARDWAREID = 0x00000002, // HardwareID REG_MULTI_SZ property =RW,
            CM_DRP_COMPATIBLEIDS = 0x00000003, // CompatibleIDs REG_MULTI_SZ property =RW,
            CM_DRP_UNUSED0 = 0x00000004, // unused
            CM_DRP_SERVICE = 0x00000005, // Service REG_SZ property =RW,
            CM_DRP_UNUSED1 = 0x00000006, // unused
            CM_DRP_UNUSED2 = 0x00000007, // unused
            CM_DRP_CLASS = 0x00000008, // Class REG_SZ property =RW,
            CM_DRP_CLASSGUID = 0x00000009, // ClassGUID REG_SZ property =RW,
            CM_DRP_DRIVER = 0x0000000A, // Driver REG_SZ property =RW,
            CM_DRP_CONFIGFLAGS = 0x0000000B, // ConfigFlags REG_DWORD property =RW,
            CM_DRP_MFG = 0x0000000C, // Mfg REG_SZ property =RW,
            CM_DRP_FRIENDLYNAME = 0x0000000D, // FriendlyName REG_SZ property =RW,
            CM_DRP_LOCATION_INFORMATION = 0x0000000E, // LocationInformation REG_SZ property =RW,
            CM_DRP_PHYSICAL_DEVICE_OBJECT_NAME = 0x0000000F, // PhysicalDeviceObjectName REG_SZ property =R,
            CM_DRP_CAPABILITIES = 0x00000010, // Capabilities REG_DWORD property =R,
            CM_DRP_UI_NUMBER = 0x00000011, // UiNumber REG_DWORD property =R,
            CM_DRP_UPPERFILTERS = 0x00000012, // UpperFilters REG_MULTI_SZ property =RW,
            CM_DRP_LOWERFILTERS = 0x00000013, // LowerFilters REG_MULTI_SZ property =RW,
            CM_DRP_BUSTYPEGUID = 0x00000014, // Bus Type Guid, GUID, =R,
            CM_DRP_LEGACYBUSTYPE = 0x00000015, // Legacy bus type, INTERFACE_TYPE, =R,
            CM_DRP_BUSNUMBER = 0x00000016, // Bus Number, DWORD, =R,
            CM_DRP_ENUMERATOR_NAME = 0x00000017, // Enumerator Name REG_SZ property =R,
            CM_DRP_SECURITY = 0x00000018, // Security - Device override =RW,
            CM_DRP_SECURITY_SDS = 0x00000019, // Security - Device override =RW,
            CM_DRP_DEVTYPE = 0x0000001A, // Device Type - Device override =RW,
            CM_DRP_EXCLUSIVE = 0x0000001B, // Exclusivity - Device override =RW,
            CM_DRP_CHARACTERISTICS = 0x0000001C, // Characteristics - Device Override =RW,
            CM_DRP_ADDRESS = 0x0000001D, // Device Address =R,
            CM_DRP_UI_NUMBER_DESC_FORMAT = 0x0000001E, // UINumberDescFormat REG_SZ property =RW,
            CM_DRP_DEVICE_POWER_DATA = 0x0000001F, // CM_POWER_DATA REG_BINARY property =R,
            CM_DRP_REMOVAL_POLICY = 0x00000020, // CM_DEVICE_REMOVAL_POLICY REG_DWORD =R,
            CM_DRP_REMOVAL_POLICY_HW_DEFAULT = 0x00000021, // CM_DRP_REMOVAL_POLICY_HW_DEFAULT REG_DWORD =R,
            CM_DRP_REMOVAL_POLICY_OVERRIDE = 0x00000022, // CM_DRP_REMOVAL_POLICY_OVERRIDE REG_DWORD =RW,
            CM_DRP_INSTALL_STATE = 0x00000023 // CM_DRP_INSTALL_STATE REG_DWORD =R,
        }

        public const int SPDRP_DRIVER = 0x9;
        public const int SPDRP_DEVICEDESC = 0x0;

        public const int DICS_ENABLE = 0x00000001;
        public const int DICS_DISABLE = 0x00000002;

        public const UInt32 INFINITE = 0xFFFFFFFF;

        public static readonly Guid GUID_DEVCLASS_USB = new Guid("{0x36fc9e60, 0xc465, 0x11cf, {0x80, 0x56, 0x44, 0x45, 0x53, 0x54, 0x00, 0x00}}");
        #endregion

        #region enumerations

        public enum UsbDeviceClass : byte
        {
            UnspecifiedDevice = 0x00,
            AudioInterface = 0x01,
            CommunicationsAndCDCControlBoth = 0x02,
            HIDInterface = 0x03,
            PhysicalInterfaceDevice = 0x5,
            ImageInterface = 0x06,
            PrinterInterface = 0x07,
            MassStorageInterface = 0x08,
            HubDevice = 0x09,
            CDCDataInterface = 0x0A,
            SmartCardInterface = 0x0B,
            ContentSecurityInterface = 0x0D,
            VidioInterface = 0x0E,
            PersonalHeathcareInterface = 0x0F,
            DiagnosticDeviceBoth = 0xDC,
            WirelessControllerInterface = 0xE0,
            MiscellaneousBoth = 0xEF,
            ApplicationSpecificInterface = 0xFE,
            VendorSpecificBoth = 0xFF
        }

        public enum HubCharacteristics : byte
        {
            GangedPowerSwitching = 0x00,
            IndividualPotPowerSwitching = 0x01,
            // to do

        }

        //original winapi USB_HUB_NODE enumeration
        //typedef enum _USB_HUB_NODE
        //{
        //    UsbHub,
        //    UsbMIParent
        //} USB_HUB_NODE;

        public enum USB_HUB_NODE
        {
            UsbHub,
            UsbMIParent
        }

        public enum USB_DESCRIPTOR_TYPE : byte
        {
            DeviceDescriptorType = 0x1,
            ConfigurationDescriptorType = 0x2,
            StringDescriptorType = 0x3,
            InterfaceDescriptorType = 0x4,
            EndpointDescriptorType = 0x5,
            HubDescriptor = 0x29
        }

        public enum USB_CONFIGURATION : byte
        {
            RemoteWakeUp = 32,
            SelfPowered = 64,
            BusPowered = 128,
            RemoteWakeUp_BusPowered = 160,
            RemoteWakeUp_SelfPowered = 96
        }

        public enum USB_TRANSFER : byte
        {
            Control = 0x0,
            Isochronous = 0x1,
            Bulk = 0x2,
            Interrupt = 0x3
        }

        //original winapi USB_CONNECTION_STATUS enumeration
        //typedef enum _USB_CONNECTION_STATUS
        //{
        //    NoDeviceConnected,
        //    DeviceConnected,
        //    DeviceFailedEnumeration,
        //    DeviceGeneralFailure,
        //    DeviceCausedOvercurrent,
        //    DeviceNotEnoughPower,
        //    DeviceNotEnoughBandwidth,
        //    DeviceHubNestedTooDeeply,
        //    DeviceInLegacyHub
        //} USB_CONNECTION_STATUS, *PUSB_CONNECTION_STATUS;

        public enum USB_CONNECTION_STATUS : int
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

        //original winapi USB_DEVICE_SPEED enumeration
        //typedef enum _USB_DEVICE_SPEED
        //{
        //    UsbLowSpeed = 0,
        //    UsbFullSpeed,
        //    UsbHighSpeed
        //} USB_DEVICE_SPEED;

        public enum USB_DEVICE_SPEED : byte
        {
            UsbLowSpeed,
            UsbFullSpeed,
            UsbHighSpeed
        }

        public enum DeviceInterfaceDataFlags : uint
        {
            Unknown = 0x00000000,
            Active = 0x00000001,
            Default = 0x00000002,
            Removed = 0x00000004
        }

        public enum HubPortStatus : short
        {
            Connection = 0x0001,
            Enabled = 0x0002,
            Suspend = 0x0004,
            OverCurrent = 0x0008,
            BeingReset = 0x0010,
            Power = 0x0100,
            LowSpeed = 0x0200,
            HighSpeed = 0x0400,
            TestMode = 0x0800,
            Indicator = 0x1000,
            // these are the bits which cause the hub port state machine to keep moving 
            //kHubPortStateChangeMask = kHubPortConnection | kHubPortEnabled | kHubPortSuspend | kHubPortOverCurrent | kHubPortBeingReset 
        }

        public enum HubStatus : byte
        {
            LocalPowerStatus = 1,
            OverCurrentIndicator = 2,
            LocalPowerStatusChange = 1,
            OverCurrentIndicatorChange = 2
        }

        public enum PortIndicatorSlectors : byte
        {
            IndicatorAutomatic = 0,
            IndicatorAmber,
            IndicatorGreen,
            IndicatorOff
        }

        public enum PowerSwitching : byte
        {
            SupportsGangPower = 0,
            SupportsIndividualPortPower = 1,
            SetPowerOff = 0,
            SetPowerOn = 1
        }

        /// <summary>
        /// Device registry property codes
        /// </summary>
        public enum SPDRP : int
        {
            /// <summary>
            /// DeviceDesc (R/W)
            /// </summary>
            SPDRP_DEVICEDESC = 0x00000000,

            /// <summary>
            /// HardwareID (R/W)
            /// </summary>
            SPDRP_HARDWAREID = 0x00000001,

            /// <summary>
            /// CompatibleIDs (R/W)
            /// </summary>
            SPDRP_COMPATIBLEIDS = 0x00000002,

            /// <summary>
            /// unused
            /// </summary>
            SPDRP_UNUSED0 = 0x00000003,

            /// <summary>
            /// Service (R/W)
            /// </summary>
            SPDRP_SERVICE = 0x00000004,

            /// <summary>
            /// unused
            /// </summary>
            SPDRP_UNUSED1 = 0x00000005,

            /// <summary>
            /// unused
            /// </summary>
            SPDRP_UNUSED2 = 0x00000006,

            /// <summary>
            /// Class (R--tied to ClassGUID)
            /// </summary>
            SPDRP_CLASS = 0x00000007,

            /// <summary>
            /// ClassGUID (R/W)
            /// </summary>
            SPDRP_CLASSGUID = 0x00000008,

            /// <summary>
            /// Driver (R/W)
            /// </summary>
            SPDRP_DRIVER = 0x00000009,

            /// <summary>
            /// ConfigFlags (R/W)
            /// </summary>
            SPDRP_CONFIGFLAGS = 0x0000000A,

            /// <summary>
            /// Mfg (R/W)
            /// </summary>
            SPDRP_MFG = 0x0000000B,

            /// <summary>
            /// FriendlyName (R/W)
            /// </summary>
            SPDRP_FRIENDLYNAME = 0x0000000C,

            /// <summary>
            /// LocationInformation (R/W)
            /// </summary>
            SPDRP_LOCATION_INFORMATION = 0x0000000D,

            /// <summary>
            /// PhysicalDeviceObjectName (R)
            /// </summary>
            SPDRP_PHYSICAL_DEVICE_OBJECT_NAME = 0x0000000E,

            /// <summary>
            /// Capabilities (R)
            /// </summary>
            SPDRP_CAPABILITIES = 0x0000000F,

            /// <summary>
            /// UiNumber (R)
            /// </summary>
            SPDRP_UI_NUMBER = 0x00000010,

            /// <summary>
            /// UpperFilters (R/W)
            /// </summary>
            SPDRP_UPPERFILTERS = 0x00000011,

            /// <summary>
            /// LowerFilters (R/W)
            /// </summary>
            SPDRP_LOWERFILTERS = 0x00000012,

            /// <summary>
            /// BusTypeGUID (R)
            /// </summary>
            SPDRP_BUSTYPEGUID = 0x00000013,

            /// <summary>
            /// LegacyBusType (R)
            /// </summary>
            SPDRP_LEGACYBUSTYPE = 0x00000014,

            /// <summary>
            /// BusNumber (R)
            /// </summary>
            SPDRP_BUSNUMBER = 0x00000015,

            /// <summary>
            /// Enumerator Name (R)
            /// </summary>
            SPDRP_ENUMERATOR_NAME = 0x00000016,

            /// <summary>
            /// Security (R/W, binary form)
            /// </summary>
            SPDRP_SECURITY = 0x00000017,

            /// <summary>
            /// Security (W, SDS form)
            /// </summary>
            SPDRP_SECURITY_SDS = 0x00000018,

            /// <summary>
            /// Device Type (R/W)
            /// </summary>
            SPDRP_DEVTYPE = 0x00000019,

            /// <summary>
            /// Device is exclusive-access (R/W)
            /// </summary>
            SPDRP_EXCLUSIVE = 0x0000001A,

            /// <summary>
            /// Device Characteristics (R/W)
            /// </summary>
            SPDRP_CHARACTERISTICS = 0x0000001B,

            /// <summary>
            /// Device Address (R)
            /// </summary>
            SPDRP_ADDRESS = 0x0000001C,

            /// <summary>
            /// UiNumberDescFormat (R/W)
            /// </summary>
            SPDRP_UI_NUMBER_DESC_FORMAT = 0X0000001D,

            /// <summary>
            /// Device Power Data (R)
            /// </summary>
            SPDRP_DEVICE_POWER_DATA = 0x0000001E,

            /// <summary>
            /// Removal Policy (R)
            /// </summary>
            SPDRP_REMOVAL_POLICY = 0x0000001F,

            /// <summary>
            /// Hardware Removal Policy (R)
            /// </summary>
            SPDRP_REMOVAL_POLICY_HW_DEFAULT = 0x00000020,

            /// <summary>
            /// Removal Policy Override (RW)
            /// </summary>
            SPDRP_REMOVAL_POLICY_OVERRIDE = 0x00000021,

            /// <summary>
            /// Device Install State (R)
            /// </summary>
            SPDRP_INSTALL_STATE = 0x00000022,

            /// <summary>
            /// Device Location Paths (R)
            /// </summary>
            SPDRP_LOCATION_PATHS = 0x00000023,
        }

        [Flags]
        internal enum DeviceControlOptions : uint
        {
            DIGCF_DEFAULT = 0x1,
            DIGCF_PRESENT = 0x2,
            DIGCF_ALLCLASSES = 0x4,
            DIGCF_PROFILE = 0x8,
            DIGCF_DEVICEINTERFACE = 0x10
        }

        [Flags]
        internal enum ClassInstallerFunctionCode : uint
        {
            DIF_SELECTDEVICE = 0x1,
            DIF_INSTALLDEVICE = 0x2,
            DIF_ASSIGNRESOURCES = 0x3,
            DIF_PROPERTIES = 0x4,
            DIF_REMOVE = 0x5,
            DIF_FIRSTTIMESETUP = 0x6,
            DIF_FOUNDDEVICE = 0x7,
            DIF_SELECTCLASSDRIVERS = 0x8,
            DIF_VALIDATECLASSDRIVERS = 0x9,
            DIF_INSTALLCLASSDRIVERS = 0xA,
            DIF_CALCDISKSPACE = 0xB,
            DIF_DESTROYPRIVATEDATA = 0xC,
            DIF_VALIDATEDRIVER = 0xD,
            DIF_DETECT = 0xF,
            DIF_INSTALLWIZARD = 0x10,
            DIF_DESTROYWIZARDDATA = 0x11,
            DIF_PROPERTYCHANGE = 0x12,
            DIF_ENABLECLASS = 0x13,
            DIF_DETECTVERIFY = 0x14,
            DIF_INSTALLDEVICEFILES = 0x15,
            DIF_UNREMOVE = 0x16,
            DIF_SELECTBESTCOMPATDRV = 0x17,
            DIF_ALLOW_INSTALL = 0x18,
            DIF_REGISTERDEVICE = 0x19,
            DIF_NEWDEVICEWIZARD_PRESELECT = 0x1A,
            DIF_NEWDEVICEWIZARD_SELECT = 0x1B,
            DIF_NEWDEVICEWIZARD_PREANALYZE = 0x1C,
            DIF_NEWDEVICEWIZARD_POSTANALYZE = 0x1D,
            DIF_NEWDEVICEWIZARD_FINISHINSTALL = 0x1E,
            DIF_UNUSED1 = 0x1F,
            DIF_INSTALLINTERFACES = 0x20,
            DIF_DETECTCANCEL = 0x21,
            DIF_REGISTER_COINSTALLERS = 0x22,
            DIF_ADDPROPERTYPAGE_ADVANCED = 0x23,
            DIF_ADDPROPERTYPAGE_BASIC = 0x24,
            DIF_RESERVED1 = 0x25,
            DIF_TROUBLESHOOTER = 0x26,
            DIF_POWERMESSAGEWAKE = 0x27,
            DIF_ADDREMOTEPROPERTYPAGE_ADVANCED = 0x28,
            DIF_UPDATEDRIVER_UI = 0x29,
            DIF_FINISHINSTALL_ACTION = 0x2A,
            DIF_RESERVED2 = 0x30
        }
        #endregion


        #region structures

        //original winapi SP_CLASSINSTALL_HEADER structure
        //typedef struct _SP_CLASSINSTALL_HEADER
        //{
        //  DWORD  cbSize;
        //  DI_FUNCTION  InstallFunction;
        //} SP_CLASSINSTALL_HEADER, *PSP_CLASSINSTALL_HEADER;

        [StructLayout(LayoutKind.Sequential)]
        public struct SP_CLASSINSTALL_HEADER
        {
            public int cbSize;
            public int InstallFunction;
        }

        //original winapi SP_PROPCHANGE_PARAMS structure
        //typedef struct _SP_PROPCHANGE_PARAMS
        //{
        //  SP_CLASSINSTALL_HEADER  ClassInstallHeader;
        //  DWORD  StateChange;
        //  DWORD  Scope;
        //  DWORD  HwProfile;
        //} SP_PROPCHANGE_PARAMS, *PSP_PROPCHANGE_PARAMS;

        [StructLayout(LayoutKind.Sequential)]
        public struct SP_PROPCHANGE_PARAMS
        {
            public SP_CLASSINSTALL_HEADER ClassInstallHeader;
            public int StateChange;
            public int Scope;
            public int HwProfile;
            public void Init()
            {
                ClassInstallHeader = new SP_CLASSINSTALL_HEADER();
            }
        }

        //original winapi SP_DEVINFO_DATA structure
        //typedef struct _SP_DEVINFO_DATA
        //{
        //  DWORD cbSize;
        //  GUID ClassGuid;
        //  DWORD DevInst;
        //  ULONG_PTR Reserved;
        //} SP_DEVINFO_DATA,  *PSP_DEVINFO_DATA;

        [StructLayout(LayoutKind.Sequential)]
        public struct SP_DEVINFO_DATA
        {
            public UInt32 cbSize;
            public Guid ClassGuid;
            public UInt32 DevInst;
            public IntPtr Reserved;
        }

        //original winapi SP_DEVICE_INTERFACE_DATA structure
        //typedef struct _SP_DEVICE_INTERFACE_DATA
        //{
        //  DWORD cbSize;
        //  GUID InterfaceClassGuid;
        //  DWORD Flags;
        //  ULONG_PTR Reserved;
        //} SP_DEVICE_INTERFACE_DATA,  *PSP_DEVICE_INTERFACE_DATA;

        [StructLayout(LayoutKind.Sequential)]
        public struct SP_DEVICE_INTERFACE_DATA
        {
            public int cbSize;
            public Guid InterfaceClassGuid;
            public DeviceInterfaceDataFlags Flags;
            public IntPtr Reserved;
        }

        //original winapi SP_DEVICE_INTERFACE_DETAIL_DATA structure
        //typedef struct _SP_DEVICE_INTERFACE_DETAIL_DATA
        //{
        //  DWORD cbSize;
        //  TCHAR DevicePath[ANYSIZE_ARRAY];
        //} SP_DEVICE_INTERFACE_DETAIL_DATA,  *PSP_DEVICE_INTERFACE_DETAIL_DATA;

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
        public struct SP_DEVICE_INTERFACE_DETAIL_DATA
        {
            public int cbSize;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = MAX_BUFFER_SIZE)]
            public string DevicePath;
        }

        //original winapi USB_HCD_DRIVERKEY_NAME structure
        //typedef struct _USB_HCD_DRIVERKEY_NAME
        //{
        //    ULONG ActualLength;
        //    WCHAR DriverKeyName[1];
        //} USB_HCD_DRIVERKEY_NAME, *PUSB_HCD_DRIVERKEY_NAME;

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
        public struct USB_HCD_DRIVERKEY_NAME
        {
            public int ActualLength;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = MAX_BUFFER_SIZE)]
            public string DriverKeyName;
        }

        //original winapi USB_ROOT_HUB_NAME structrue
        //typedef struct _USB_ROOT_HUB_NAME
        //{
        //    ULONG  ActualLength;
        //    WCHAR  RootHubName[1];
        //} USB_ROOT_HUB_NAME, *PUSB_ROOT_HUB_NAME;

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
        public struct USB_ROOT_HUB_NAME
        {
            public int ActualLength;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = MAX_BUFFER_SIZE)]
            public string RootHubName;
        }

        //original winapi USB_HUB_DESCRIPTOR structure
        //typedef struct _USB_HUB_DESCRIPTOR
        //{
        //    UCHAR  bDescriptorLength;
        //    UCHAR  bDescriptorType;
        //    UCHAR  bNumberOfPorts;
        //    USHORT  wHubCharacteristics;
        //    UCHAR  bPowerOnToPowerGood;
        //    UCHAR  bHubControlCurrent;
        //    UCHAR  bRemoveAndPowerMask[64];
        //} USB_HUB_DESCRIPTOR, *PUSB_HUB_DESCRIPTOR;

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public struct USB_HUB_DESCRIPTOR
        {
            public byte bDescriptorLength;
            public USB_DESCRIPTOR_TYPE bDescriptorType;
            public byte bNumberOfPorts;
            public short wHubCharacteristics;
            public byte bPowerOnToPowerGood;
            public byte bHubControlCurrent;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 64)]
            public byte[] bRemoveAndPowerMask;
        }

        //original winapi USB_HUB_INFORMATION structrure
        //typedef struct _USB_HUB_INFORMATION
        //{
        //    USB_HUB_DESCRIPTOR HubDescriptor;
        //    BOOLEAN HubIsBusPowered;
        //} USB_HUB_INFORMATION, *PUSB_HUB_INFORMATION;

        [StructLayout(LayoutKind.Sequential)]
        public struct USB_HUB_INFORMATION
        {
            public USB_HUB_DESCRIPTOR HubDescriptor;
            public bool HubIsBusPowered;
        }

        //original winapi USB_NODE_INFORMATION structure
        //typedef struct _USB_NODE_INFORMATION
        //{
        //    USB_HUB_NODE  NodeType;
        //    union
        //    {
        //        USB_HUB_INFORMATION  HubInformation;
        //        USB_MI_PARENT_INFORMATION  MiParentInformation;
        //    } u;
        //} USB_NODE_INFORMATION, *PUSB_NODE_INFORMATION;

        [StructLayout(LayoutKind.Sequential)]
        public struct USB_NODE_INFORMATION
        {
            //public int NodeType;
            public USB_HUB_NODE NodeType;
            public USB_HUB_INFORMATION HubInformation;
        }

        //original winapi USB_NODE_CONNECTION_INFORMATION_EX structrue
        //typedef struct _USB_NODE_CONNECTION_INFORMATION_EX
        //{
        //    ULONG  ConnectionIndex;
        //    USB_DEVICE_DESCRIPTOR  DeviceDescriptor;
        //    UCHAR  CurrentConfigurationValue;
        //    UCHAR  Speed;
        //    BOOLEAN  DeviceIsHub;
        //    USHORT  DeviceAddress;
        //    ULONG  NumberOfOpenPipes;
        //    USB_CONNECTION_STATUS  ConnectionStatus;
        //    USB_PIPE_INFO  PipeList[0];
        //} USB_NODE_CONNECTION_INFORMATION_EX, *PUSB_NODE_CONNECTION_INFORMATION_EX;

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public struct USB_NODE_CONNECTION_INFORMATION_EX
        {
            public int ConnectionIndex;
            public USB_DEVICE_DESCRIPTOR DeviceDescriptor;
            public byte CurrentConfigurationValue;
            public USB_DEVICE_SPEED Speed;
            public byte DeviceIsHub;
            public short DeviceAddress;
            public int NumberOfOpenPipes;
            public USB_CONNECTION_STATUS ConnectionStatus;
            //add by steven for usb 3.0
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = MAXIMUM_USB_STRING_LENGTH)]
            public string Reserved;
            //public IntPtr PipeList;
        }

        //original winapi USB_DEVICE_DESCRIPTOR structrure
        //typedef struct _USB_DEVICE_DESCRIPTOR
        //{
        //    UCHAR  bLength;
        //    UCHAR  bDescriptorType;
        //    USHORT  bcdUSB;
        //    UCHAR  bDeviceClass;
        //    UCHAR  bDeviceSubClass;
        //    UCHAR  bDeviceProtocol;
        //    UCHAR  bMaxPacketSize0;
        //    USHORT  idVendor;
        //    USHORT  idProduct;
        //    USHORT  bcdDevice;
        //    UCHAR  iManufacturer;
        //    UCHAR  iProduct;
        //    UCHAR  iSerialNumber;
        //    UCHAR  bNumConfigurations;
        //} USB_DEVICE_DESCRIPTOR, *PUSB_DEVICE_DESCRIPTOR ;

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public class USB_DEVICE_DESCRIPTOR
        {
            public byte bLength;
            public USB_DESCRIPTOR_TYPE bDescriptorType;
            public short bcdUSB;
            public UsbDeviceClass bDeviceClass;
            public byte bDeviceSubClass;
            public byte bDeviceProtocol;
            public byte bMaxPacketSize0;
            public ushort idVendor;
            public ushort idProduct;
            public short bcdDevice;
            public byte iManufacturer;
            public byte iProduct;
            public byte iSerialNumber;
            public byte bNumConfigurations;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct USB_ENDPOINT_DESCRIPTOR
        {
            public byte bLength;
            public USB_DESCRIPTOR_TYPE bDescriptorType;
            public byte bEndpointAddress;
            public USB_TRANSFER bmAttributes;
            public short wMaxPacketSize;
            public byte bInterval;
        }


        //original winapi USB_STRING_DESCRIPTOR structrue
        //typedef struct _USB_STRING_DESCRIPTOR
        //{
        //    UCHAR bLength;
        //    UCHAR bDescriptorType;
        //    WCHAR bString[1];
        //} USB_STRING_DESCRIPTOR, *PUSB_STRING_DESCRIPTOR;

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
        public struct USB_STRING_DESCRIPTOR
        {
            public byte bLength;
            public USB_DESCRIPTOR_TYPE bDescriptorType;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = MAXIMUM_USB_STRING_LENGTH)]
            public string bString;
        }

        //original winapi USB_DESCRIPTOR_REQUEST structrue
        //typedef struct _USB_DESCRIPTOR_REQUEST
        //{
        //  ULONG ConnectionIndex;
        //  struct
        //  {
        //    UCHAR  bmRequest;
        //    UCHAR  bRequest;
        //    USHORT  wValue;
        //    USHORT  wIndex;
        //    USHORT  wLength;
        //  } SetupPacket;
        //  UCHAR  Data[0];
        //} USB_DESCRIPTOR_REQUEST, *PUSB_DESCRIPTOR_REQUEST

        [StructLayout(LayoutKind.Sequential)]
        public struct USB_SETUP_PACKET
        {
            public byte bmRequest;
            public byte bRequest;
            public short wValue;
            public short wIndex;
            public short wLength;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct USB_DESCRIPTOR_REQUEST
        {
            public int ConnectionIndex;
            public USB_SETUP_PACKET SetupPacket;
            //public byte[] Data;
        }

        //original winapi USB_NODE_CONNECTION_NAME structure
        //typedef struct _USB_NODE_CONNECTION_NAME
        //{
        //    ULONG  ConnectionIndex;
        //    ULONG  ActualLength;
        //    WCHAR  NodeName[1];
        //} USB_NODE_CONNECTION_NAME, *PUSB_NODE_CONNECTION_NAME;

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
        public struct USB_NODE_CONNECTION_NAME
        {
            public int ConnectionIndex;
            public int ActualLength;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = MAX_BUFFER_SIZE)]
            public string NodeName;
        }

        //original winapi USB_NODE_CONNECTION_DRIVERKEY_NAME structrue
        //typedef struct _USB_NODE_CONNECTION_DRIVERKEY_NAME
        //{
        //    ULONG  ConnectionIndex;
        //    ULONG  ActualLength;
        //    WCHAR  DriverKeyName[1];
        //} USB_NODE_CONNECTION_DRIVERKEY_NAME, *PUSB_NODE_CONNECTION_DRIVERKEY_NAME;

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
        public struct USB_NODE_CONNECTION_DRIVERKEY_NAME               // Yes, this is the same as the structure above...
        {
            public int ConnectionIndex;
            public int ActualLength;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = MAX_BUFFER_SIZE)]
            public string DriverKeyName;
        }

        //typedef struct _STORAGE_DEVICE_NUMBER
        //{
        //  DEVICE_TYPE  DeviceType;
        //  ULONG  DeviceNumber;
        //  ULONG  PartitionNumber;
        //} STORAGE_DEVICE_NUMBER, *PSTORAGE_DEVICE_NUMBER;

        [StructLayout(LayoutKind.Sequential)]
        public struct STORAGE_DEVICE_NUMBER
        {
            public int DeviceType;
            public int DeviceNumber;
            public int PartitionNumber;
        }


        //typedef struct _USB_INTERFACE_DESCRIPTOR { 
        //  UCHAR  bLength ;
        //  UCHAR  bDescriptorType ;    
        //  UCHAR  bInterfaceNumber ;
        //  UCHAR  bAlternateSetting ;
        //  UCHAR  bNumEndpoints ;
        //  UCHAR  bInterfaceClass ;
        //  UCHAR  bInterfaceSubClass ;
        //  UCHAR  bInterfaceProtocol ;
        //  UCHAR  iInterface ;
        //} USB_INTERFACE_DESCRIPTOR, *PUSB_INTERFACE_DESCRIPTOR ;

        [StructLayout(LayoutKind.Sequential)]
        public struct USB_INTERFACE_DESCRIPTOR
        {
            public byte bLength;
            public USB_DESCRIPTOR_TYPE bDescriptorType;
            public byte bInterfaceNumber;
            public byte bAlternateSetting;
            public byte bNumEndpoints;
            public byte bInterfaceClass;
            public byte bInterfaceSubClass;
            public byte bInterfaceProtocol;
            public byte Interface;
        }


        //typedef struct _USB_CONFIGURATION_DESCRIPTOR { 
        //  UCHAR  bLength ;
        //  UCHAR  bDescriptorType ;
        //  USHORT  wTotalLength ;
        //  UCHAR  bNumInterfaces ;
        //  UCHAR  bConfigurationValue;
        //  UCHAR  iConfiguration ;
        //  UCHAR  bmAttributes ;
        //  UCHAR  MaxPower ;
        //} USB_CONFIGURATION_DESCRIPTOR, *PUSB_CONFIGURATION_DESCRIPTOR ;

        [StructLayout(LayoutKind.Sequential)]
        public struct USB_CONFIGURATION_DESCRIPTOR
        {
            public byte bLength;
            public USB_DESCRIPTOR_TYPE bDescriptorType;
            public short wTotalLength;
            public byte bNumInterface;
            public byte bConfigurationsValue;
            public byte iConfiguration;
            public USB_CONFIGURATION bmAttributes;
            public byte MaxPower;
        }

        //typedef struct _HID_DESCRIPTOR
        //{
        //UCHAR  bLength;
        //UCHAR  bDescriptorType;
        //USHORT  bcdHID;
        //UCHAR  bCountry;
        //UCHAR  bNumDescriptors;
        //struct _HID_DESCRIPTOR_DESC_LIST
        //{
        //UCHAR  bReportType;
        //USHORT  wReportLength;
        //} DescriptorList [1];
        //} HID_DESCRIPTOR, *PHID_DESCRIPTOR;

        [StructLayout(LayoutKind.Sequential)]
        public struct HID_DESCRIPTOR_DESC_LIST
        {
            public byte bReportType;
            public short wReportLength;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct HID_DESCRIPTOR
        {
            public byte bLength;
            public USB_DESCRIPTOR_TYPE bDescriptorType;
            public short bcdHID;
            public byte bCountry;
            public byte bNumDescriptors;
            public HID_DESCRIPTOR_DESC_LIST hid_desclist;
        }

        [StructLayout(LayoutKind.Sequential)]
        public class SP_DEVINFO_DATA1
        {
            public int cbSize;
            public Guid ClassGuid;
            public int DevInst;
            public ulong Reserved;
        };

        //typedef struct RAW_ROOTPORT_PARAMETERS
        //{
        //USHORT  PortNumber;
        //USHORT  PortStatus;
        //} RAW_ROOTPORT_PARAMETERS, *PRAW_ROOTPORT_PARAMETERS;

        [StructLayout(LayoutKind.Sequential)]
        public class RAW_ROOTPORT_PARAMETERS
        {
            public ushort PortNumber;
            public ushort PortStatus;
        }

        //typedef struct USB_UNICODE_NAME
        //{
        //ULONG  Length;
        //WCHAR  String[1];
        //} USB_UNICODE_NAME, *PUSB_UNICODE_NAME;

        [StructLayout(LayoutKind.Sequential)]
        public class USB_UNICODE_NAME
        {
            public ulong Length;
            public string str;
        }

        #endregion
        #region kernel32
        [DllImport("kernel32.dll", SetLastError = true)]
        internal static extern uint GetLastError();
        #endregion kernel32
        #region SetUpAPI
        [DllImport("setupapi.dll", CharSet = CharSet.Auto, SetLastError = true)]
        internal static extern IntPtr SetupDiGetClassDevs(
            [In] IntPtr classGuid,
            [In] [MarshalAs(UnmanagedType.LPWStr)] string enumerator,
            [In] IntPtr hwndParent,
            DeviceControlOptions Flags
        );

        [DllImport("setupapi.dll", CharSet = CharSet.Auto, SetLastError = true)]
        internal static extern bool SetupDiEnumDeviceInfo(
            IntPtr DeviceInfoSet,
            uint MemberIndex,
            ref SP_DEVINFO_DATA DeviceInfoData
        );

        [DllImport("setupapi.dll", CharSet = CharSet.Auto, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static extern bool SetupDiSetClassInstallParams(
            [In] IntPtr DeviceInfoSet,
            [In] ref SP_DEVINFO_DATA DeviceInfoData,
            [In] ref SP_PROPCHANGE_PARAMS ClassInstallParams,
            uint ClassInstallParamsSize
        );


        [DllImport("setupapi.dll", CharSet = CharSet.Auto, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static extern bool SetupDiCallClassInstaller(
            ClassInstallerFunctionCode InstallFunction,
            [In] IntPtr DeviceInfoSet,
            [In] ref SP_DEVINFO_DATA DeviceInfoData
        );

        [DllImport("setupapi.dll", CharSet = CharSet.Auto, SetLastError = true)]
        internal static extern bool SetupDiDestroyDeviceInfoList(IntPtr DeviceInfoSet);

        [DllImport("setupapi.dll", CharSet = CharSet.Auto, SetLastError = true)]
        internal static extern bool SetupDiGetDevicePropertyKeys(IntPtr DeviceInfoSet, ref SP_DEVINFO_DATA DeviceInfoData, IntPtr PropertyKeyArray, uint PropertyKeyCount, ref uint RequiredPropertyKeyCount, uint Flags);

        [DllImport("setupapi.dll", CharSet = CharSet.Auto, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static extern bool SetupDiGetDeviceProperty(IntPtr DeviceInfoSet, [In] ref SP_DEVINFO_DATA DeviceInfoData, [In] ref DEVPROPKEY PropertyKey, out DEVPROP_TYPE PropertyType, byte[] PropertyBuffer, uint PropertyBufferSize, ref uint RequiredSize, uint Flags);
        [DllImport("setupapi.dll", SetLastError = true, CharSet = CharSet.Auto)]
        internal static extern Boolean SetupDiOpenDeviceInfo(IntPtr DeviceInfoSet, String DeviceInstanceId, IntPtr hwndParent, Int32 Flags, ref SP_DEVINFO_DATA DeviceInfoData);


        // Declare the external functions from the Windows API
        [DllImport("setupapi.dll", SetLastError = true)]
        internal static extern IntPtr SetupDiCreateDeviceInfoList(IntPtr ClassGuid, IntPtr hwndParent);

        [DllImport("setupapi.dll", SetLastError = true)]
        internal static extern IntPtr SetupDiGetClassDevs(IntPtr ClassGuid, [MarshalAs(UnmanagedType.LPTStr)] string Enumerator, IntPtr hwndParent, uint Flags);

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

        //[DllImport("setupapi.dll", SetLastError = true)]
        //private static extern bool SetupDiDestroyDeviceInfoList(IntPtr DeviceInfoSet);

        [DllImport("setupapi.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern bool SetupDiGetDeviceInstanceId(IntPtr DeviceInfoSet, ref SP_DEVINFO_DATA DeviceInfoData, StringBuilder DeviceInstanceId, int DeviceInstanceIdSize, out int RequiredSize);

        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern bool DeviceIoControl(IntPtr hDevice, int dwIoControlCode, IntPtr lpInBuffer, int nInBufferSize, IntPtr lpOutBuffer, int nOutBufferSize, out int lpBytesReturned, IntPtr lpOverlapped);

        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr CreateFile(string lpFileName, int dwDesiredAccess, int dwShareMode, IntPtr lpSecurityAttributes, int dwCreationDisposition, int dwFlagsAndAttributes, IntPtr hTemplateFile);

        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern bool CloseHandle(IntPtr hObject);


        #endregion SetUpAPI

        #endregion

    }
}

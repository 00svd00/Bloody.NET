using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading;
using HidSharp;
using HidSharp.Reports.Encodings;

namespace Bloody.NET
{

    public sealed class BloodyKeyboard : IDisposable
    {
        private const int VendorId = 0x09DA;
        private const uint LedUsagePage = 0x000C; //(ff52,0001,000C) //List of UsagePage+UsageID I found from Windows Device Manager(labed there as Uxxxx&&UPxxxx)
        private const uint LedUsage = 0x0001; //(0244,/0080,0001)
        private static readonly byte[] ColorPacketHeader = new byte[3] { 0x07, 0x03, 0x06 }; 
        private static readonly byte[] ColorPacketNumber = new byte[7] { 0x01, 0x07, 0x08, 0x09, 0x0a, 0x0b, 0x0c }; //byte after 070306
        private readonly byte[] _keyColors = new byte[348];//(64-4-2)*2*3 = 58*6 = 348

        private readonly HidStream _ledStream;
        private readonly HidDevice _ctrlDevice;
        private readonly HidStream _ctrlStream;

        static List<int?> products = new List<int?> 
        { 
            0xFA10,  //B810r
            0xFA44   //B930
        };
        private BloodyKeyboard(HidDevice ledDevice, HidStream ledStream, HidDevice ctrlDevice, HidStream ctrlStream)
        {
            _ledStream = ledStream;
            _ctrlDevice = ctrlDevice;
            _ctrlStream = ctrlStream;
        }


        public static BloodyKeyboard Initialize()
        {   
            IEnumerable<HidDevice> devices = null;
            foreach (int? id in BloodyKeyboard.products)
            {
                devices = DeviceList.Local.GetHidDevices(vendorID: VendorId, productID: id); //Try to find Bloody B930 with given VID PID

                if (devices.Any())
                {
                    break;
                }
            }
            if (!devices.Any())
            {
                return null;
            }
            try
            {
                HidDevice ledDevice = GetFromUsages(devices, LedUsagePage, LedUsage);
                HidDevice ctrlDevice = devices.First(d => d.GetMaxFeatureReportLength() > 50);
                HidStream ledStream = null;
                HidStream ctrlStream = null;
                if ((ctrlDevice?.TryOpen(out ctrlStream) ?? false) && (ledDevice?.TryOpen(out ledStream) ?? false))
                {
                    BloodyKeyboard kb = new BloodyKeyboard(ledDevice, ledStream, ctrlDevice, ctrlStream);
                    if (kb.SendCtrlInitSequence())
                        return kb;
                }
                else
                {
                    ctrlStream?.Close();
                    ledStream?.Close();
                }
            }
            catch
            { }
            return null;
        }

        #region Initial Reports
        private bool SendCtrlInitSequence() //Sends initial sequence of packets to initialise keyboard, as seen in Wireshark dump
        {
            var result =

                SetCtrlReport(CtrlReports._0x1f) && 

                GetCtrlReport(0x07) && // wValue = 0307, set 07 as it is the ReportID (See USD Hid documentation for more info)

                SetCtrlReport(CtrlReports._0x1f) &&

                GetCtrlReport(0x07) &&

                SetCtrlReport(CtrlReports._0x05) &&

                GetCtrlReport(0x07) &&

                SetCtrlReport(CtrlReports._0x1f) &&

                GetCtrlReport(0x07) &&

                SetCtrlReport(CtrlReports._0x1f) &&

                GetCtrlReport(0x07) &&

                SetCtrlReport(CtrlReports._0x1f) &&

                GetCtrlReport(0x07) &&

                SetCtrlReport(CtrlReports._0x29) &&

                GetCtrlReport(0x07) &&

                SetCtrlReport(CtrlReports._0x1f) &&

                GetCtrlReport(0x07) &&

                SetCtrlReport(CtrlReports._0x1f) &&

                GetCtrlReport(0x07) &&

                SetCtrlReport(CtrlReports._0x1f) &&

                GetCtrlReport(0x07) &&

                SetCtrlReport(CtrlReports._0x05) &&

                GetCtrlReport(0x07) &&

                SetCtrlReport(CtrlReports._0x07) &&

                GetCtrlReport(0x07) &&

                SetCtrlReport(CtrlReports._0x1f) &&

                GetCtrlReport(0x07) &&

                SetCtrlReport(CtrlReports._0x1f) &&

                GetCtrlReport(0x07) &&

                SetCtrlReport(CtrlReports._0x2a) &&

                GetCtrlReport(0x07) &&

                SetCtrlReport(CtrlReports._0x1f) &&

                GetCtrlReport(0x07) &&

                SetCtrlReport(CtrlReports._0x1f) &&

                GetCtrlReport(0x07) &&

                SetCtrlReport(CtrlReports._0x2a) &&

                GetCtrlReport(0x07) &&

                SetCtrlReport(CtrlReports._0x1f) &&

                GetCtrlReport(0x07) &&

                SetCtrlReport(CtrlReports._0x1f) &&

                GetCtrlReport(0x07) &&

                SetCtrlReport(CtrlReports._0x29) &&

                GetCtrlReport(0x07) &&

                SetCtrlReport(CtrlReports._0x1e01) &&

                SetCtrlReport(CtrlReports._0x09) &&

                SetCtrlReport(CtrlReports._0x05) &&

                GetCtrlReport(0x07) &&

                SetCtrlReport(CtrlReports._0x2f002d) &&

                GetCtrlReport(0x07) &&

                SetCtrlReport(CtrlReports._0x0c) &&

                SetCtrlReport(CtrlReports._0x0c) &&

                SetCtrlReport(CtrlReports._0x030605) &&

                GetCtrlReport(0x07) &&

                SetCtrlReport(CtrlReports._0x06) &&

                SetCtrlReport(CtrlReports._0x030601) &&

                SetCtrlReport(CtrlReports._0x030605) &&

                GetCtrlReport(0x07) &&

                SetCtrlReport(CtrlReports._0x030601) &&

                SetCtrlReport(CtrlReports._0x030607) &&

                SetCtrlReport(CtrlReports._0x030608) &&

                SetCtrlReport(CtrlReports._0x030609) &&

                SetCtrlReport(CtrlReports._0x03060a) &&

                SetCtrlReport(CtrlReports._0x03060b) &&

                SetCtrlReport(CtrlReports._0x03060c) &&

                SetCtrlReport(CtrlReports._0x030605) &&

                GetCtrlReport(0x07) &&

                SetCtrlReport(CtrlReports._0x030606) &&

                GetCtrlReport(0x07);

            return result;
        }

        private bool GetCtrlReport(byte report_id) //GetReport
        {
            int size = _ctrlDevice.GetMaxFeatureReportLength();
            var buf = new byte[size];
            buf[0] = report_id;
            try
            {
                _ctrlStream.GetFeature(buf);
                return true;
            }
            catch
            {
                return false;
            }
        }

        private bool SetCtrlReport(byte[] reportBuffer) //SetReport, send packet with given name from CtrlReports.cs (Packets Copied Data fragment from the ones captured in Wireshark)
        {
            try
            {
                _ctrlStream.SetFeature(reportBuffer);
                return true;
            }
            catch
            {
                return false;
            }
        }
        #endregion
        #region Set Colors
        /// <summary>
        /// All packets sent to the keyboard which invole anything to do with rgb are 64 bytes long.
        /// First Packet of rgb data transfer starts with 07030601 and next bytes are empty.
        /// Next Packets contain actual rgb data and start with 070306xx where xx is 07->0C hex range, basically giving packets order where 07 packet is first and 0C packet is last.
        /// 07, 08 packets send Red data; 09,0A packets send Green data; 0B,0C send Blue data.
        /// The 07,09,0B packets contain colors for first half of keyboard, and 08,0A,0C cointain data for second half of keyboard. 
        /// First half are keys in Key.cs with number less than 57 and vice versa. Meaning that each packet contains data for 58 keys.
        /// First 4 bytes in each packets are used for header and next 2 packets are always empty, so the first key to appear at this offset in first packet is labled as key = 0 (ESC=0)
        /// </summary>

        public void SetColors(Dictionary<Key, Color> keyColors) 
        {
            foreach (var key in keyColors)
                SetKeyColor(key.Key, key.Value);
        }

        public void SetColor(Color clr) //Set Color to every key on keyboard
        {
            foreach (Key key in (Key[])Enum.GetValues(typeof(Key))) 
                SetKeyColor(key, clr);
        }

        public void SetKeyColor(Key key, Color clr) //Puts data for each key into the byte array _keyColors
        {
            int offset = (int)key;
            _keyColors[offset + 0] = clr.R; //First 2 packets of rgb data are red there for they take up 58*2=116 bytes, so the data for green color for next 2 packets should start from 116 byte (as byte arrays start from byte 0 as first one). 
            _keyColors[offset + 116] = clr.G;
            _keyColors[offset + 232] = clr.B;
        }
        public bool Update() => WriteColorBuffer();      
        private bool WriteColorBuffer() 
        {
            byte[] packet = new byte[64];
            ColorPacketHeader.CopyTo(packet, 0); //header 07030601 + next 61 bytes are empty
            Array.Copy(new byte[61], 0, packet, ColorPacketHeader.Length, 61);
            Array.Copy(ColorPacketNumber, 0, packet, 3, 1);
            try
            {
                _ctrlStream.SetFeature(packet);
                for (int i = 1; i <= 6; i++) //header 0703060xx + next 61 bytes are rgb data
                {
                    ColorPacketHeader.CopyTo(packet, 0); //Copies Color Header to packet
                    Array.Copy(ColorPacketNumber, i, packet, 3, 1); //Copies header number to packet (07-> 0C)
                    Array.Copy(_keyColors, (i * 58) - 58, packet, 6, 58); //Copies rgb bytes to the packet
                    _ctrlStream.SetFeature(packet); //Sends packet as additional data in SetReport USBHID packet
                }
                return true;
            }
            catch
            {
               Disconnect();
               return false;
            }
        }
        #endregion
        private static HidDevice GetFromUsages(IEnumerable<HidDevice> devices, uint usagePage, uint usage)
        {
            foreach (var dev in devices) //For each dev in devices under Vid Pid get dev with right UsageID and UsagePage, if found return it.
            {
                try
                {
                    var raw = dev.GetRawReportDescriptor();
                    var usages = EncodedItem.DecodeItems(raw, 0, raw.Length).Where(t => t.TagForGlobal == GlobalItemTag.UsagePage);
                    if (usages.Any(g => g.ItemType == ItemType.Global && g.DataValue == usagePage))
                    {
                        if (usages.Any(l => l.ItemType == ItemType.Local && l.DataValue == usage))
                        {
                            return dev; 
                        }
                    }
                }
                catch
                {
                    //failed to get the report descriptor, skip
                }
            }
            return null;
        }

        public void Disconnect()
        {
            _ctrlStream?.Close();
            _ledStream?.Close();
        }

        #region IDisposable Support
        /// <summary>
        /// Disconnects the keyboard when disposing
        /// </summary>
        public void Dispose() => Disconnect();
        #endregion
    }

}

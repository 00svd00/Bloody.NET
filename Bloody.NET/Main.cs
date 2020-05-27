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
        private const int ProductId = 0xFA44;
        private const uint LedUsagePage = 0x000C; //ff52 //0001 //000C
        private const uint LedUsage = 0x0001; //0244 //0080 //0001
        private static readonly byte[] ColorPacketHeader = new byte[3] { 0x07, 0x03, 0x06 };
        private static readonly byte[] ColorPacketNumber = new byte[7] { 0x01, 0x07, 0x08, 0x09, 0x0a, 0x0b, 0x0c };
        private readonly byte[] _keyColors = new byte[348];//(64-4-2)*2*3

        private readonly HidStream _ledStream;
        private readonly HidDevice _ctrlDevice;
        private readonly HidStream _ctrlStream;



        private BloodyKeyboard(HidDevice ledDevice, HidStream ledStream, HidDevice ctrlDevice, HidStream ctrlStream)
        {
            _ledStream = ledStream;
            _ctrlDevice = ctrlDevice;
            _ctrlStream = ctrlStream;
        }


        public static BloodyKeyboard Initialize()
        {
            var devices = DeviceList.Local.GetHidDevices(vendorID: VendorId, productID: ProductId);

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
        private bool SendCtrlInitSequence()
        {
            var result =

                SetCtrlReport(CtrlReports._0x1f) &&

                GetCtrlReport(0x07) &&

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

        private bool GetCtrlReport(byte report_id)
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

        private bool SetCtrlReport(byte[] reportBuffer)
        {
            try
            {
                _ctrlStream.SetFeature(reportBuffer);
                //Console.WriteLine(BitConverter.ToString(reportBuffer).Replace("-", " "));
                return true;
            }
            catch
            {
                return false;
            }
        }
        #endregion
        #region Set Colors
        public void SetColors(Dictionary<Key, Color> keyColors) 
        {
            foreach (var key in keyColors)
                SetKeyColor(key.Key, key.Value);
        }

        public void SetColor(Color clr)
        {
            foreach (Key key in (Key[])Enum.GetValues(typeof(Key))) 
                SetKeyColor(key, clr);
        }

        public void SetKeyColor(Key key, Color clr) 
        {
            int offset = (int)key;
            _keyColors[offset + 0] = clr.R;
            _keyColors[offset + 116] = clr.G;
            _keyColors[offset + 232] = clr.B;
        }
        public bool Update() => WriteColorBuffer();

        private bool WriteColorBuffer() //TBD
        {
            byte[] packet = new byte[64];
            ColorPacketHeader.CopyTo(packet, 0);//header at the beginning of the first packet
            Array.Copy(new byte[61], 0, packet, ColorPacketHeader.Length, 61);
            Array.Copy(ColorPacketNumber, 0, packet, 3, 1);
            try
            {
                //Console.WriteLine(BitConverter.ToString(packet).Replace("-", " "));
                _ctrlStream.SetFeature(packet);
                for (int i = 1; i <= 6; i++)//each chunk consists of the byte 0x00 and 64 bytes of data after that
                {
                    ColorPacketHeader.CopyTo(packet, 0);
                    Array.Copy(ColorPacketNumber, i, packet, 3, 1);
                    Array.Copy(_keyColors, (i * 58) - 58, packet, 6, 58);
                    //Console.WriteLine(BitConverter.ToString(packet).Replace("-", " "));
                    _ctrlStream.SetFeature(packet);
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
            foreach (var dev in devices) //For each dev in devices under Vid Pid
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

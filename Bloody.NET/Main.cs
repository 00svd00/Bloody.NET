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
        private const int MaxTries = 100;
        private const int VendorId = 0x09DA;
        private const int ProductId = 0xFA44;
        private const uint LedUsagePage = 0x000C; //ff52 //0001 //000C
        private const uint LedUsage = 0x0001; //0244 //0080 //0001
        private static readonly byte[] ColorPacketHeader = new byte[3] { 0x07, 0x03, 0x06 };
        private static readonly byte[] ColorPacketNumber = new byte[7] { 0x01, 0x07, 0x08, 0x09, 0x0a, 0x0b, 0x0c };
        private readonly byte[] _keyColors = new byte[348];//(64-4-2)*2*3

        private readonly HidDevice _ledDevice;
        private readonly HidStream _ledStream;
        private readonly HidDevice _ctrlDevice;
        private readonly HidStream _ctrlStream;



        private BloodyKeyboard(HidDevice ledDevice, HidStream ledStream, HidDevice ctrlDevice, HidStream ctrlStream)
        {
            _ledDevice = ledDevice;
            _ledStream = ledStream;
            _ctrlDevice = ctrlDevice;
            _ctrlStream = ctrlStream;
        }

        private static void Main()
        {
            Console.WriteLine("Hello");
        }

        public static BloodyKeyboard Initialize()
        {
            var devices = DeviceList.Local.GetHidDevices(vendorID: VendorId, productID: ProductId);

            if (!devices.Any())
            {
                Console.WriteLine("N1");
                return null;
            }
            try
            {
                HidDevice ledDevice = GetFromUsages(devices, LedUsagePage, LedUsage);
                HidDevice ctrlDevice = devices.First(d => d.GetMaxFeatureReportLength() > 50);
                HidStream ledStream = null;
                HidStream ctrlStream = null;
                Console.WriteLine(ledDevice + "led");
                Console.WriteLine(ctrlDevice + "ctrl");
                if (ctrlDevice?.TryOpen(out ctrlStream) ?? false)
                {
                    Console.WriteLine("Y5");
                }
                if (ledDevice?.TryOpen(out ledStream) ?? false)
                {
                    Console.WriteLine("Success");
                }

                if ((ctrlDevice?.TryOpen(out ctrlStream) ?? false) && (ledDevice?.TryOpen(out ledStream) ?? false))
                {
                    BloodyKeyboard kb = new BloodyKeyboard(ledDevice, ledStream, ctrlDevice, ctrlStream);
                    Console.WriteLine(kb);
                    Console.WriteLine("Connection Established");
                    if (kb.SendCtrlInitSequence())
                        return kb;
                }
                else
                {
                    ctrlStream?.Close();
                    ledStream?.Close();
                    Console.WriteLine("N2");
                }
            }
            catch
            { }
            return null;
        }
        #region Colour setting ToBeChanged

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

                GetCtrlReport(0x07)




                ;


            //_ctrlStream?.Close();

            return result;
        }

        private bool GetCtrlReport(byte report_id)
        {
            int size = _ctrlDevice.GetMaxFeatureReportLength();
            //Console.WriteLine(size);
            var buf = new byte[size];
            buf[0] = report_id;
            try
            {
                _ctrlStream.GetFeature(buf);
                Console.WriteLine("GetClrlReport");
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
                Console.WriteLine("SetClrlReport");
                Console.WriteLine(BitConverter.ToString(reportBuffer).Replace("-", " "));
                return true;
            }
            catch
            {
                return false;
            }
        }

        //Waits for Success packet to be recived from Keyboard
        private bool WaitCtrlDevice()
        {
            int size = _ctrlDevice.GetMaxFeatureReportLength();
            byte[] buf = new byte[size];
            buf[0] = 0x04;
            for (int i = 0; i < MaxTries; i++)
            {
                Thread.Sleep(150);
                try
                {
                    _ctrlStream.GetFeature(buf);
                    if (buf[1] == 0x01)
                        return true;
                }
                catch
                {
                    return false;
                }
            }
            return false;
        }

        public void SetColors(Dictionary<Key, Color> keyColors) //TBC
        {
            foreach (var key in keyColors)
                SetKeyColor(key.Key, key.Value);
        }

        public void SetColor(Color clr)
        {
            foreach (Key key in (Key[])Enum.GetValues(typeof(Key))) //TBC
                SetKeyColor(key, clr);
        }

        public void SetKeyColor(Key key, Color clr) //TBC
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
            Array.Copy(new byte[61], 0,
                        packet, ColorPacketHeader.Length,
                        61);//copy the first 60 bytes of color data to the packet
                            //so 60 data + 5 header fits in a packet
            Array.Copy(ColorPacketNumber, 0, packet, 3, 1);
            try
            {
                Console.WriteLine("1.");
                Console.WriteLine(BitConverter.ToString(packet).Replace("-", " "));
                _ctrlStream.SetFeature(packet);
                Console.WriteLine("Test2");
                for (int i = 1; i <= 6; i++)//each chunk consists of the byte 0x00 and 64 bytes of data after that
                {
                    Console.WriteLine("Test3");
                    ColorPacketHeader.CopyTo(packet, 0);
                    Array.Copy(ColorPacketNumber, i, packet, 3, 1);
                    //  packet[0] = 0x00;
                    Array.Copy(_keyColors, (i * 58) - 58, packet, 6, 58);
                    Console.WriteLine($"{i + 1}.");
                    Console.WriteLine(BitConverter.ToString(packet).Replace("-", " "));
                    _ctrlStream.SetFeature(packet);
                }
                return true;
            }
            catch
            {
               Console.WriteLine("False");
               Disconnect();
               return false;
            }
        }
#endregion
        public void Disconnect()
        {
            _ctrlStream?.Close();
            _ledStream?.Close();
        } 

        private static HidDevice GetFromUsages(IEnumerable<HidDevice> devices, uint usagePage, uint usage)
        {
            foreach (var dev in devices) //For each dev in devices under Vid Pid
            {
                try
                {
                    var raw = dev.GetRawReportDescriptor();
                    // Console.WriteLine(raw);
                    var usages = EncodedItem.DecodeItems(raw, 0, raw.Length).Where(t => t.TagForGlobal == GlobalItemTag.UsagePage);
                    // Console.WriteLine(usages);
                    if (usages.Any(g => g.ItemType == ItemType.Global && g.DataValue == usagePage))
                    {
                        Console.WriteLine("Y3");
                        if (usages.Any(l => l.ItemType == ItemType.Local && l.DataValue == usage))
                        {
                            Console.WriteLine("Y4");
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
        #region IDisposable Support
        /// <summary>
        /// Disconnects the keyboard when disposing
        /// </summary>
        public void Dispose() => Disconnect();
        #endregion
    }

}

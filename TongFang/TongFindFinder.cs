﻿using HidSharp;
using HidSharp.Reports.Encodings;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TongFang
{
    public static class TongFindFinder
    {
        private const int VID = 0x048D;
        private const int PID = 0xCE00;
        private const int PID2 = 0x6004;
        private const uint USAGE_PAGE = 0xFF03;
        private const uint USAGE = 0x001;

        public static IEnumerable<ITongFangKeyboard> Find()
        {
            var devices = DeviceList.Local.GetHidDevices(VID).Where(d => d.ProductID == PID || d.ProductID == PID2);

            if (!devices.Any())
               yield break;

            foreach (var device in devices)
            {
                if (!device.VerifyUsageAndUsagePage(USAGE_PAGE, USAGE))
                    continue;

                yield return new TongFangKeyboard(device, 7, Layout.ANSI);
            }
        }

        public static bool TryFind(int brightness, Layout layout, out ITongFangKeyboard keyboard)
        {
            var devices = DeviceList.Local.GetHidDevices(VID).Where(d => d.ProductID == PID || d.ProductID == PID2);

            if (!devices.Any())
            {
                keyboard = null;
                return false;
            }

            try
            {
                foreach (var device in devices)
                {
                    if (!device.VerifyUsageAndUsagePage(USAGE_PAGE, USAGE))
                        continue;

                    keyboard = new TongFangKeyboard(device, brightness, layout);
                    return true;
                }
            }
            catch
            { }

            keyboard = null;
            return false;
        }

        private static bool VerifyUsageAndUsagePage(this HidDevice device, uint usagePage, uint usage)
        {
            try
            {
                var rawReportDescriptor = device.GetRawReportDescriptor();

                var items = EncodedItem.DecodeItems(rawReportDescriptor, 0, rawReportDescriptor.Length).Where(t => t.TagForGlobal == GlobalItemTag.UsagePage);

                return items.Any(item => item.ItemType == ItemType.Global && item.DataValue == usagePage)
                    && items.Any(item => item.ItemType == ItemType.Local && item.DataValue == usage);
            }
            catch
            {
                return false;
            }
        }
    }
}

﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

namespace HidApiAdapter
{
    public class HidDevice
    {

        private readonly hid_device_info m_DeviceInfo;
        private IntPtr m_DevicePtr = IntPtr.Zero;

        private HidDevice() { }

        internal HidDevice(hid_device_info deviceInfo, IntPtr devicePtr)
        {
            m_DeviceInfo = deviceInfo;
            m_DevicePtr = devicePtr;
        }

        public bool WasConnected => m_DevicePtr != IntPtr.Zero;

        public int VendorId => m_DeviceInfo.vendor_id;
        public int ProductId => m_DeviceInfo.product_id;


        public string Path() => 
            Marshal.PtrToStringAnsi(m_DeviceInfo.path);

        public bool Connect()
        {
            if (m_DevicePtr == IntPtr.Zero)
                return false;

            m_DevicePtr = HidApi.hid_open_path(m_DeviceInfo.path);
           
            return true;
        }

        public bool Disconnect()
        {
            if (m_DevicePtr == IntPtr.Zero)
                return false;

            HidApi.hid_close(m_DevicePtr);

            return true;
        }

        private byte[] m_WriteBuffer = new byte[1024];

        public int Write(byte[] bytes)
        {
            if (m_DevicePtr == IntPtr.Zero)
                return 0;

            if (bytes == null || bytes.Length == 0)
                return 0;

            if (m_WriteBuffer.Length <= bytes.Length)
                Array.Resize(ref m_WriteBuffer, bytes.Length + 2);

            //TODO fix this for other OSs
            //hidapi for windows has problem - first byte must be 0 also array length shuold be increased for 1
            Array.Copy(bytes, 0, m_WriteBuffer, 1, bytes.Length);

            return HidApi.hid_write(m_DevicePtr, m_WriteBuffer, Convert.ToUInt32(bytes.Length + 1));
        }
        
        public int Read(byte[] buff, int len)
        {
            if (m_DevicePtr == IntPtr.Zero)
                return 0;

            return HidApi.hid_read(m_DevicePtr, buff, Convert.ToUInt32(len));
        }

        #region device info

        StringBuilder m_DeviceInfoBuffer = new StringBuilder(1024);

        public string SerialNumber()
        {
            m_DeviceInfoBuffer.Clear();
            HidApi.hid_get_serial_number_string(m_DevicePtr, m_DeviceInfoBuffer, 1024 / 4);

            return m_DeviceInfoBuffer.ToString();
        }

        public string Manufacturer()
        {
            m_DeviceInfoBuffer.Clear();
            HidApi.hid_get_manufacturer_string(m_DevicePtr, m_DeviceInfoBuffer, 1024 / 4);

            return m_DeviceInfoBuffer.ToString();
        }

        public string Product()
        {
            m_DeviceInfoBuffer.Clear();
            HidApi.hid_get_product_string(m_DevicePtr, m_DeviceInfoBuffer, 1024 / 4);

            return m_DeviceInfoBuffer.ToString();
        }


        #endregion

        public override string ToString()
        {
            if (WasConnected)
                return $"manufacturer: {Manufacturer()}, serial_number:{SerialNumber()}, product:{Product()}";
            else
                return "unknown";
        }

    }
}
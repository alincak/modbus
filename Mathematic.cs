using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Globalization;
using System.Collections;

namespace modbus
{
    public class Mathematic
    {
        static public UInt16 CalculateCRC(byte[] Buffer, int Len)
        {
            UInt16 CRC = 0xFFFF;

            for (int pos = 0; pos < Len; pos++)
            {
                CRC ^= (UInt16)Buffer[pos];

                for (int i = 8; i != 0; i--)
                {
                    if ((CRC & 0x0001) != 0)
                    {
                        CRC >>= 1;
                        CRC ^= 0xA001;
                    }
                    else
                    {
                        CRC >>= 1;
                    }
                }
            }
            return CRC;
        }

        static public byte MSB(int X)
        {
            return (byte)((X & 0xff00) >> 8);
        }

        static public byte LSB(int X)
        {
            return (byte)(X & 0x00ff);
        }

        static private byte Highest(Int32 X)
        {
            return (byte)((X & 0xff000000) >> 24);
        }

        static private byte Higher(Int32 X)
        {
            return (byte)((X & 0x00ff0000) >> 16);
        }

        static private byte High(Int32 X)
        {
            return (byte)((X & 0x0000ff00) >> 8);
        }

        static private byte Low(Int32 X)
        {
            return (byte)(X & 0x000000ff);
        }

        static public float ToFloat(byte A, byte B, byte C, byte D)
        {
            return BitConverter.ToSingle(new byte[] { B, A, D, C }, 0);
        }

        static public Int16 ToInt16(byte MSB, byte LSB)
        {
            return (Int16)(MSB * 256 + LSB);
        }

        static public decimal ToDecimal(byte A, byte B, byte C, byte D)
        {
            return Convert.ToDecimal((long)C * 16777216 + (long)D * 65536 + (long)A * 256 + (long)B);
        }

        static public uint ToUInt(byte A, byte B, byte C, byte D)
        {
            return Convert.ToUInt32((uint)C * 16777216 + (uint)D * 65536 + (uint)A * 256 + (uint)B);
        }

        static public List<byte> ToBytes(Int16 X)
        {
            List<byte> Bytes = new List<byte>();
            Bytes.Add(MSB(Convert.ToInt32(X)));
            Bytes.Add(LSB(Convert.ToInt32(X)));

            return Bytes;
        }

        static public List<byte> ToBytes(Int32 X)
        {
            List<byte> Bytes = new List<byte>();
            Bytes.Add(High(Convert.ToInt32(X)));
            Bytes.Add(Low(Convert.ToInt32(X)));
            Bytes.Add(Highest(Convert.ToInt32(X)));
            Bytes.Add(Higher(Convert.ToInt32(X)));

            return Bytes;
        }

        static public List<byte> ToBytes(float X)
        {
            List<byte> Bytes = new List<byte>();
            byte[] FloatBytes = BitConverter.GetBytes(X);
            Bytes.Add(FloatBytes[1]);
            Bytes.Add(FloatBytes[0]);
            Bytes.Add(FloatBytes[3]);
            Bytes.Add(FloatBytes[2]);

            return Bytes;
        }
    }
}
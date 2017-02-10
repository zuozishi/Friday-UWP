/// Copyright (c) 2011 Brian Bonnett
/// Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files (the "Software"), to deal in the Software without restriction, including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, subject to the following conditions:
/// The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.

using System;

namespace xmedianet.socketserver.TLS
{
    public enum Endianess
    {
        Little,
        Big
    }

    public class ByteHelper
    {
        public static byte [] GetBytesForInt32(int value, Endianess endian)
        {
            byte[] bRet = new byte[4];
            if (endian == Endianess.Big)
            {
                bRet[0] = (byte)((value & 0xFF000000) >> 24);
                bRet[1] = (byte)((value & 0xFF0000) >> 16);
                bRet[2] = (byte)((value & 0x00FF00) >> 8);
                bRet[3] = (byte)((value & 0x0000FF));
            }
            else
            {
                bRet[3] = (byte)((value & 0xFF000000) >> 24);
                bRet[2] = (byte)((value & 0xFF0000) >> 16);
                bRet[1] = (byte)((value & 0x00FF00) >> 8);
                bRet[0] = (byte)((value & 0x0000FF));
            }
            return bRet;
        }

        public static byte[] GetBytesForUint64(ulong value, Endianess endian)
        {
            byte[] bRet = new byte[8];
            if (endian == Endianess.Big)
            {
                bRet[0] = (byte)((value & 0xFF00000000000000) >> 56);
                bRet[1] = (byte)((value & 0x00FF000000000000) >> 48);
                bRet[2] = (byte)((value & 0x0000FF0000000000) >> 40);
                bRet[3] = (byte)((value & 0x000000FF00000000) >> 32);
                bRet[4] = (byte)((value & 0x00000000FF000000) >> 24);
                bRet[5] = (byte)((value & 0x0000000000FF0000) >> 16);
                bRet[6] = (byte)((value & 0x000000000000FF00) >> 8);
                bRet[7] = (byte)((value & 0x00000000000000FF));
            }
            else
            {
                bRet[7] = (byte)((value & 0xFF00000000000000) >> 56);
                bRet[6] = (byte)((value & 0x00FF000000000000) >> 48);
                bRet[5] = (byte)((value & 0x0000FF0000000000) >> 40);
                bRet[4] = (byte)((value & 0x000000FF00000000) >> 32);
                bRet[3] = (byte)((value & 0x00000000FF000000) >> 24);
                bRet[2] = (byte)((value & 0x0000000000FF0000) >> 16);
                bRet[1] = (byte)((value & 0x000000000000FF00) >> 8);
                bRet[0] = (byte)((value & 0x00000000000000FF));
            }
            return bRet;
        }

        public static int WriteUInt32BigEndian(byte[] bArrayWriteTo, int nIndex, uint value)
        {
            return WriteUInt32BigEndian(bArrayWriteTo, nIndex, value, 4);
        }

        public static int WriteUInt32BigEndian(byte[] bArrayWriteTo, int nIndex, uint value, int nLength)
        {
            if (nLength == 4)
            {
                bArrayWriteTo[nIndex + 0] = (byte)((value & 0xFF000000) >> 24);
                bArrayWriteTo[nIndex + 1] = (byte)((value & 0xFF0000) >> 16);
                bArrayWriteTo[nIndex + 2] = (byte)((value & 0x00FF00) >> 8);
                bArrayWriteTo[nIndex + 3] = (byte)((value & 0x0000FF));
                return 4;
            }
            else if (nLength == 3)
            {
                bArrayWriteTo[nIndex + 0] = (byte)((value & 0xFF0000) >> 16);
                bArrayWriteTo[nIndex + 1] = (byte)((value & 0x00FF00) >> 8);
                bArrayWriteTo[nIndex + 2] = (byte)((value & 0x0000FF));
                return 3;
            }
            else if (nLength == 2)
            {
                bArrayWriteTo[nIndex + 0] = (byte)((value & 0x00FF00) >> 8);
                bArrayWriteTo[nIndex + 1] = (byte)((value & 0x0000FF));
                return 2;
            }
            else if (nLength == 1)
            {
                bArrayWriteTo[nIndex + 0] = (byte)((value & 0x0000FF));
                return 1;
            }

            return 0;
        }

        public static int WriteULongBigEndian(byte[] bArrayWriteTo, int nIndex, ulong value)
        {
            bArrayWriteTo[nIndex + 0] = (byte)((value & 0xFF00000000000000) >> 56);
            bArrayWriteTo[nIndex + 1] = (byte)((value & 0xFF000000000000) >> 48);
            bArrayWriteTo[nIndex + 2] = (byte)((value & 0xFF0000000000) >> 40);
            bArrayWriteTo[nIndex + 3] = (byte)((value & 0xFF00000000) >> 32);
            bArrayWriteTo[nIndex + 4] = (byte)((value & 0xFF000000) >> 24);
            bArrayWriteTo[nIndex + 5] = (byte)((value & 0xFF0000) >> 16);
            bArrayWriteTo[nIndex + 6] = (byte)((value & 0x00FF00) >> 8);
            bArrayWriteTo[nIndex + 7] = (byte)((value & 0x0000FF));
            return 8;
        }

        public static int WriteUShortBigEndian(byte[] bArrayWriteTo, int nIndex, ushort value)
        {
            bArrayWriteTo[nIndex + 0] = (byte)((value & 0x00FF00) >> 8);
            bArrayWriteTo[nIndex + 1] = (byte)((value & 0x0000FF));
            return 2;
        }


        public static int WriteByte(byte[] bArrayWriteTo, int nIndex, byte value)
        {
            bArrayWriteTo[nIndex + 0] = value;
            return 1;
        }

        public static int WriteByteArray(byte[] bArrayWriteTo, int nIndex, byte [] bValue)
        {
            Array.Copy(bValue, 0, bArrayWriteTo, nIndex, (int)bValue.Length);
            return bValue.Length;
        }

        public static uint ReadUintBigEndian(byte[] bRet, int nIndex)
        {
            return ReadUintBigEndian(bRet, nIndex, 4);
        }

        public static uint ReadUintBigEndian(byte[] bData, int nIndex, int nBytes)
        {
            if (nBytes == 4)
                return (uint)((bData[nIndex + 0] << 24) | (bData[nIndex + 1] << 16) | (bData[nIndex + 2] << 8) | (bData[nIndex + 3]));
            else if (nBytes == 3)
                return (uint)((bData[nIndex + 0] << 16) | (bData[nIndex + 1] << 8) | (bData[nIndex + 2]));
            else if (nBytes == 2)
                return (uint)((bData[nIndex + 0] << 8) | (bData[nIndex + 1]));
            else if (nBytes == 1)
                return (uint)((bData[nIndex + 0]));

            return 0;
        }

        public static ulong ReadULongBigEndian(byte[] bData, int nIndex)
        {
            return (ulong)(((long)bData[nIndex + 0] << 56) | ((long)bData[nIndex + 1] << 48) | ((long)bData[nIndex + 2] << 40) | ((long)bData[nIndex + 3] << 32) | ((long)bData[nIndex + 4] << 24) | ((long)bData[nIndex + 5] << 16) | ((long)bData[nIndex + 6] << 8) | ((long)bData[nIndex + 7]));
        }

        public static ushort ReadUshortBigEndian(byte[] bRet, int nIndex)
        {
            return (ushort) ReadUintBigEndian(bRet, nIndex, 2);
        }


        public static byte ReadByte(byte[] bRet, int nIndex)
        {
            return (byte)bRet[nIndex];
        }

        public static byte [] ReadByteArray(byte[] bSource, int nIndex, int nLength)
        {
            byte[] bRet = new byte[nLength];
            Array.Copy(bSource, nIndex, bRet, 0, nLength);
            return bRet;
        }

        public static string HexStringFromByte(byte[] aBytes, bool Spaces, int LineBreakAt)
        {
            if (aBytes == null)
                return "null";
            System.Text.StringBuilder builder = new System.Text.StringBuilder(aBytes.Length * 3 + 10);
            int nByte = 1;
            foreach (byte b in aBytes)
            {
                string strHex = Convert.ToString(b, 16);
                if (strHex.Length == 1)
                    strHex = "0" + strHex;
                if (Spaces)
                    strHex += " ";
                builder.Append(strHex);

                if (nByte == LineBreakAt)
                {
                    nByte = 0;
                    builder.Append("\r\n");
                }
                //builder.AppendFormat("{X}", b);

                nByte++;
            }

            string strRet = builder.ToString();
            strRet = strRet.TrimEnd();
            strRet = strRet.ToUpper();
            return strRet;
        }

        public static bool CompareArrays(byte[] bA, byte[] bB)
        {
            if ((bA == null) && (bB == null))
                return true;
            if ((bA == null) || (bB == null))
                return false;
            if (bA.Length != bB.Length)
                return false;

            for (int i = 0; i < bA.Length; i++)
            {
                if (bA[i] != bB[i])
                    return false;
            }
            return true;
        }

        /// <summary>
        /// converts a hex character to decimal equivalent
        /// </summary>
        /// <param name="HexChar"></param>
        /// <returns></returns>
        public static int ToDecimal(char HexChar)
        {
            switch (HexChar)
            {
                case '0':
                    return 0;
                case '1':
                    return 1;
                case '2':
                    return 2;
                case '3':
                    return 3;
                case '4':
                    return 4;
                case '5':
                    return 5;
                case '6':
                    return 6;
                case '7':
                    return 7;
                case '8':
                    return 8;
                case '9':
                    return 9;
                case 'A':
                    return 10;
                case 'a':
                    return 10;
                case 'B':
                    return 11;
                case 'b':
                    return 11;
                case 'C':
                    return 12;
                case 'c':
                    return 12;
                case 'D':
                    return 13;
                case 'd':
                    return 13;
                case 'E':
                    return 14;
                case 'e':
                    return 14;
                case 'F':
                    return 15;
                case 'f':
                    return 15;
                default:
                    return Convert.ToInt32(HexChar);
            }
        }

        public static char ToHex(int nDecimal)
        {
            switch (nDecimal)
            {
                case 0:
                    return '0';
                case 1:
                    return '1';
                case 2:
                    return '2';
                case 3:
                    return '3';
                case 4:
                    return '4';
                case 5:
                    return '5';
                case 6:
                    return '6';
                case 7:
                    return '7';
                case 8:
                    return '8';
                case 9:
                    return '9';
                case 10:
                    return 'A';
                case 11:
                    return 'B';
                case 12:
                    return 'C';
                case 13:
                    return 'D';
                case 14:
                    return 'E';
                case 15:
                    return 'F';
                default:
                    return Convert.ToChar(nDecimal);
            }
        }

        /// <summary>
        ///  converts a hex string with spaces to a byte array
        /// </summary>
        /// <param name="strHexString"></param>
        /// <returns></returns>
        public static byte[] ByteFromHexString(string strHexString)
        {
            /// see if the hex string has spaces, if so, split it, if not split differently
            strHexString = strHexString.Replace(" ", "");  /// Remove all spaces

            if (strHexString.Length == 0)
                return new byte[] { };
            if (strHexString.Length % 2 != 0)
                strHexString = strHexString + "0";


            byte[] bRet = new byte[strHexString.Length / 2];
            for (int i = 0; i < strHexString.Length / 2; i++)
            {
                bRet[i] = (byte)(ToDecimal(strHexString[i * 2]) * 16 + ToDecimal(strHexString[i * 2 + 1]));
            }

            return bRet;
        }
    }
}


using System;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;

namespace Kishore.X509.Parser
{
    /// <summary>
    /// Summary description for AbstractAsn1Container.
    /// </summary>
    internal abstract class AbstractAsn1Container
    {
        private int offset;
        private byte[] data;
        private byte tag;

        internal protected AbstractAsn1Container(byte[] abyte, int i, byte tag)
        {
            this.tag = tag;
            if (abyte[i] != tag)
            {
                throw new Exception("Invalid data. The tag byte is not valid");
            }
            int length = DetermineLength(abyte, i + 1);
            int bytesInLengthField = DetermineLengthLen(abyte, i + 1);
            int start = i + bytesInLengthField + 1;
            this.offset = start + length;
            data = new byte[length];
            Array.Copy(abyte, start, data, 0, length);
        }

        internal int Offset
        {
            get
            {
                return offset;
            }
        }

        internal byte[] Bytes
        {
            get
            {
                return this.data;
            }
        }

        internal protected virtual int DetermineLengthLen(byte[] abyte0, int i)
        {
            int j = abyte0[i] & 0xff;
            switch (j)
            {
                case 129:
                    return 2;

                case 130:
                    return 3;

                case 131:
                    return 4;

                case 132:
                    return 5;

                case 128:
                default:
                    return 1;
            }
        }

        internal protected virtual int DetermineLength(byte[] abyte0, int i)
        {
            int j = abyte0[i] & 0xff;
            switch (j)
            {
                case 128:
                    return DetermineIndefiniteLength(abyte0, i);

                case 129:
                    return abyte0[i + 1] & 0xff;

                case 130:
                    int k = (abyte0[i + 1] & 0xff) << 8;
                    k |= abyte0[i + 2] & 0xff;
                    return k;

                case 131:
                    int l = (abyte0[i + 1] & 0xff) << 16;
                    l |= (abyte0[i + 2] & 0xff) << 8;
                    l |= abyte0[i + 3] & 0xff;
                    return l;
            }
            return j;
        }

        internal protected virtual int DetermineIndefiniteLength(byte[] abyte0, int i)
        {
            if ((abyte0[i - 1] & 0xff & 0x20) == 0)
                throw new Exception("Invalid indefinite length.");
            int j = 0;
            int k;
            int l;
            for (i++; abyte0[i] != 0 && abyte0[i + 1] != 0; i += 1 + k + l)
            {
                j++;
                k = DetermineLengthLen(abyte0, i + 1);
                j += k;
                l = DetermineLength(abyte0, i + 1);
                j += l;
            }

            return j;
        }


    }

    internal class IntegerContainer : AbstractAsn1Container
    {
        internal IntegerContainer(byte[] abyte, int i)
            : base(abyte, i, 0x2)
        {
        }
    }

    internal class SequenceContainer : AbstractAsn1Container
    {
        internal SequenceContainer(byte[] abyte, int i)
            : base(abyte, i, 0x30)
        {
        }
    }

    public class X509PublicKeyParser
    {
        public static RSAParameters GetRSAPublicKeyParameters(byte[] bytes)
        {
            return GetRSAPublicKeyParameters(bytes, 0);
        }

        public static RSAParameters GetRSAPublicKeyParameters(byte[] bytes, int i)
        {
            SequenceContainer seq = new SequenceContainer(bytes, i);
            IntegerContainer modContainer = new IntegerContainer(seq.Bytes, 0);
            IntegerContainer expContainer = new IntegerContainer(seq.Bytes, modContainer.Offset);
            return LoadKeyData(modContainer.Bytes, 0, modContainer.Bytes.Length, expContainer.Bytes, 0, expContainer.Bytes.Length);
        }

        public static RSAParameters GetRSAPublicKeyParameters(X509Certificate cert)
        {
            return GetRSAPublicKeyParameters(cert.GetPublicKey(), 0);
        }

        private static RSAParameters LoadKeyData(byte[] abyte0, int i, int j, byte[] abyte1, int k, int l)
        {
            byte[] modulus = null;
            byte[] publicExponent = null;
            for (; abyte0[i] == 0; i++)
                j--;

            modulus = new byte[j];
            Array.Copy(abyte0, i, modulus, 0, j);
            int i1 = modulus.Length * 8;
            int j1 = modulus[0] & 0xff;
            for (int k1 = j1 & 0x80; k1 == 0; k1 = j1 << 1 & 0xff)
                i1--;

            if (i1 < 256 || i1 > 4096)
                throw new Exception("Invalid RSA modulus size.");
            for (; abyte1[k] == 0; k++)
                l--;

            publicExponent = new byte[l];
            Array.Copy(abyte1, k, publicExponent, 0, l);
            RSAParameters p = new RSAParameters();
            p.Modulus = modulus;
            p.Exponent = publicExponent;
            return p;
        }
    }
}



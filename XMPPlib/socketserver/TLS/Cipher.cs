/// Copyright (c) 2011 Brian Bonnett
/// Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files (the "Software"), to deal in the Software without restriction, including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, subject to the following conditions:
/// The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.

using System.Collections.Generic;


namespace xmedianet.socketserver.TLS
{

    public enum CipherSuite : ushort
    {
        TLS_NULL_WITH_NULL_NULL = 0x0000,

        // RSA Certificate suites
        TLS_RSA_WITH_NULL_MD5 = 0x0001,
        TLS_RSA_WITH_NULL_SHA = 0x0002,
        TLS_RSA_WITH_RC4_128_MD5 = 0x0004,
        TLS_RSA_WITH_RC4_128_SHA = 0x0005,
        TLS_RSA_WITH_IDEA_CBC_SHA = 0x0007,
        TLS_RSA_WITH_DES_CBC_SHA = 0x0009,
        TLS_RSA_WITH_3DES_EDE_CBC_SHA = 0x000A,

        // Diffie-Hellman cert suites
        TLS_DH_DSS_WITH_DES_CBC_SHA = 0x000C,
        TLS_DH_DSS_WITH_3DES_EDE_CBC_SHA = 0x000D,
        TLS_DH_RSA_WITH_DES_CBC_SHA = 0x000F,
        TLS_DH_RSA_WITH_3DES_EDE_CBC_SHA = 0x0010,
        TLS_DHE_DSS_WITH_DES_CBC_SHA = 0x0012,
        TLS_DHE_DSS_WITH_3DES_EDE_CBC_SHA = 0x0013,
        TLS_DHE_RSA_WITH_DES_CBC_SHA = 0x0015,
        TLS_DHE_RSA_WITH_3DES_EDE_CBC_SHA = 0x0016,

        // Diffie-Hellman anonymous (deprecated)
        TLS_DH_anon_WITH_RC4_128_MD5 = 0x0018,
        TLS_DH_anon_WITH_DES_CBC_SHA = 0x001A,
        TLS_DH_anon_WITH_3DES_EDE_CBC_SHA = 0x001B,


        // Weak ciphers before export laws were loosened
        TLS_RSA_EXPORT_WITH_RC4_40_MD5 = 0x0003,
        TLS_RSA_EXPORT_WITH_RC2_CBC_40_MD5 = 0x0006,
        TLS_RSA_EXPORT_WITH_DES40_CBC_SHA = 0x0008,
        TLS_DH_DSS_EXPORT_WITH_DES40_CBC_SHA = 0x000B,
        TLS_DH_RSA_EXPORT_WITH_DES40_CBC_SHA = 0x000E,
        TLS_DHE_DSS_EXPORT_WITH_DES40_CBC_SHA = 0x0011,
        TLS_DHE_RSA_EXPORT_WITH_DES40_CBC_SHA = 0x0014,
        TLS_DH_anon_EXPORT_WITH_RC4_40_MD5 = 0x0017,
        TLS_DH_anon_EXPORT_WITH_DES40_CBC_SHA = 0x0019,

        // others
        TLS_KRB5_WITH_DES_CBC_SHA = 0x001E,
        TLS_KRB5_WITH_3DES_EDE_CBC_SHA = 0x001F,
        TLS_KRB5_WITH_RC4_128_SHA = 0x0020,
        TLS_KRB5_WITH_IDEA_CBC_SHA = 0x0021,
        TLS_KRB5_WITH_DES_CBC_MD5 = 0x0022,
        TLS_KRB5_WITH_3DES_EDE_CBC_MD5 = 0x0023,
        TLS_KRB5_WITH_RC4_128_MD5 = 0x0024,
        TLS_KRB5_WITH_IDEA_CBC_MD5 = 0x0025,

        // don't use with 1.1
        TLS_KRB5_EXPORT_WITH_DES_CBC_40_SHA = 0x0026,
        TLS_KRB5_EXPORT_WITH_RC2_CBC_40_SHA = 0x0027,
        TLS_KRB5_EXPORT_WITH_RC4_40_SHA = 0x0028,
        TLS_KRB5_EXPORT_WITH_DES_CBC_40_MD5 = 0x0029,
        TLS_KRB5_EXPORT_WITH_RC2_CBC_40_MD5 = 0x002A,
        TLS_KRB5_EXPORT_WITH_RC4_40_MD5 = 0x002B,

        // other again
        TLS_RSA_WITH_AES_128_CBC_SHA = 0x002F,
        TLS_DH_DSS_WITH_AES_128_CBC_SHA = 0x0030,
        TLS_DH_RSA_WITH_AES_128_CBC_SHA = 0x0031,
        TLS_DHE_DSS_WITH_AES_128_CBC_SHA = 0x0032,
        TLS_DHE_RSA_WITH_AES_128_CBC_SHA = 0x0033,
        TLS_DH_anon_WITH_AES_128_CBC_SHA = 0x0034,
        TLS_RSA_WITH_AES_256_CBC_SHA = 0x0035,
        TLS_DH_DSS_WITH_AES_256_CBC_SHA = 0x0036,
        TLS_DH_RSA_WITH_AES_256_CBC_SHA = 0x0037,
        TLS_DHE_DSS_WITH_AES_256_CBC_SHA = 0x0038,
        TLS_DHE_RSA_WITH_AES_256_CBC_SHA = 0x0039,
        TLS_DH_anon_WITH_AES_256_CBC_SHA = 0x003A
    }

    public class Cipher
    {
        public Cipher(CipherSuite suite, BulkEncryptionAlgorithm encrypalgo, MACAlgorithm macalgo,
            byte nBlockSize, byte nIVSize, int nKeyBits, byte nKeyMaterialSize)
        {
            CipherSuite = suite;
            BulkEncryptionAlgorithm = encrypalgo;
            MACAlgorithm = macalgo;
            BlockSizeBytes = nBlockSize;
            IVSize = nIVSize;
            KeySizeBits= nKeyBits;
            KeyMaterialLength = nKeyMaterialSize;
            KeyBlockSize = (this.KeyMaterialLength + this.HashSize + this.IVSize) << 1;
        }

        private int m_nkeyBlockSize = 0;
        public int KeyBlockSize
        {
            get 
            {
                return m_nkeyBlockSize;
            }
            protected set
            {
                m_nkeyBlockSize = value;
            }
        }

        public override string ToString()
        {
            return CipherSuite.ToString();
        }

        private CipherType m_eCipherType = CipherType.block;
        public CipherType CipherType
        {
            get { return m_eCipherType; }
            set { m_eCipherType = value; }
        }

        CipherSuite m_eCipherSuite = CipherSuite.TLS_NULL_WITH_NULL_NULL;
        public CipherSuite CipherSuite
        {
          get { return m_eCipherSuite; }
          set { m_eCipherSuite = value; }
        }

        private BulkEncryptionAlgorithm m_eBulkEncryptionAlgorithm = BulkEncryptionAlgorithm.nul;
        public BulkEncryptionAlgorithm BulkEncryptionAlgorithm
        {
            get { return m_eBulkEncryptionAlgorithm; }
            set { m_eBulkEncryptionAlgorithm = value; }
        }

        private MACAlgorithm m_eMACAlgorithm = MACAlgorithm.nul;
        public MACAlgorithm MACAlgorithm
        {
            get { return m_eMACAlgorithm; }
            set 
            { 
                m_eMACAlgorithm = value;
                if (m_eMACAlgorithm == TLS.MACAlgorithm.md5)
                    HashSize = 16;
                else if (m_eMACAlgorithm == TLS.MACAlgorithm.sha)
                    HashSize = 20;
            }
        }

        private byte m_bHashSize = 0;
        public byte HashSize
        {
            get { return m_bHashSize; }
            set { m_bHashSize = value; }
        }


        private byte m_nBlockSize = 0;
        public byte BlockSizeBytes
        {
            get { return m_nBlockSize; }
            set { m_nBlockSize = value; }
        }

        private byte m_nIVSize = 0;
        public byte IVSize
        {
            get { return m_nIVSize; }
            set { m_nIVSize = value; }
        }

        private int m_bKeySize = 0;
        public int KeySizeBits
        {
            get { return m_bKeySize; }
            set { m_bKeySize = value; }
        }

        private byte m_bKeyMaterialLength = 0;
        public byte KeyMaterialLength
        {
            get { return m_bKeyMaterialLength; }
            set { m_bKeyMaterialLength = value; }
        }

   

        public static Dictionary<CipherSuite, Cipher> CommonCiphers = new Dictionary<CipherSuite, Cipher>();
        static Cipher()
        {
            //CommonCiphers.Add(CipherSuite.TLS_NULL_WITH_NULL_NULL, new Cipher(CipherSuite.TLS_NULL_WITH_NULL_NULL, BulkEncryptionAlgorithm.nul, MACAlgorithm.nul, 0, 0, 0, 0));
            CommonCiphers.Add(CipherSuite.TLS_RSA_WITH_AES_256_CBC_SHA, new Cipher(CipherSuite.TLS_RSA_WITH_AES_256_CBC_SHA, BulkEncryptionAlgorithm.aes, MACAlgorithm.sha, 16, 16, 256, 32));
            CommonCiphers.Add(CipherSuite.TLS_RSA_WITH_AES_128_CBC_SHA, new Cipher(CipherSuite.TLS_RSA_WITH_AES_128_CBC_SHA, BulkEncryptionAlgorithm.aes, MACAlgorithm.sha, 16, 16, 128, 16));
        }

        public static Cipher FindCipher(CipherSuite suite)
        {
            if (CommonCiphers.ContainsKey(suite) == true)
                return CommonCiphers[suite];

            return null;
        }
    }
}

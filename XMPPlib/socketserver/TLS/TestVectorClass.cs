/// Copyright (c) 2011 Brian Bonnett
/// Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files (the "Software"), to deal in the Software without restriction, including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, subject to the following conditions:
/// The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.

using System;
using System.Net;
using System.Windows;

using System.Security.Cryptography;

/// Classes used to test our TLS algorithms by using known public/private keys and wireshark captures
/// 
#if DEBUG

namespace xmedianet.socketserver.TLS
{
    public class TestVectorClass
    {
        /// <summary>
        ///  These appear to be working - get the same results a wikipedia http://en.wikipedia.org/wiki/HMAC
        /// </summary>
        public static void TestHMACs()
        {
            byte[] bKey = System.Text.UTF8Encoding.UTF8.GetBytes("key");
            byte[] bData = System.Text.UTF8Encoding.UTF8.GetBytes("The quick brown fox jumps over the lazy dog");

            HMACSHA1 sha = new HMACSHA1(bKey);
            byte[] bRethmacsha1 = sha.ComputeHash(bData);
            System.Diagnostics.Debug.WriteLine("SHA1 HMAC is: ");
            System.Diagnostics.Debug.WriteLine(xmedianet.socketserver.TLS.ByteHelper.HexStringFromByte(bRethmacsha1, true, 8));
            //should be this for HMACSHA1 - DE 7C 9B 85 B8 B7 8A A6 BC 8A 7A 36 F7 0A 90 70 1C 9D B4 D9

            xmedianet.socketserver.TLS.HMACMD5 md5 = new xmedianet.socketserver.TLS.HMACMD5(bKey);
            byte[] bRethmacmd5 = md5.ComputeHash(bData);
            System.Diagnostics.Debug.WriteLine("MD5 HMAC is: ");
            System.Diagnostics.Debug.WriteLine(xmedianet.socketserver.TLS.ByteHelper.HexStringFromByte(bRethmacmd5, true, 8));
            /// should be this for HMAC MD5 - 80 07 07 13 46 3E 77 49 B9 0C 2D C2 49 11 E2 75
        }

        public static void GenerateStuff()
        {
            string strPreMasterSecret = "03 01 7B 4C D6 CC FA A3 B6 52 1B 8B 46 92 25 3D F3 C1 1A F7 9E 99 95 B3 3E 96 B3 F7 90 B4 8D 6E B4 7A 46 C2 F8 7D 71 FE 32 19 E6 96 58 19 3E 78";
            string strClientRandom = "4f 1b a0 34 2b f8 ad ba f4 97 4c 39 0f e1 e1 dd 4a a9 b1 af 39 f2 e4 25 ad 27 b0 ce a1 43 c4 49";
            string strServerRandom = "4f 1b a0 6b 88 c1 1a 57 96 dd 83 79 aa ab 4f 99 0c c8 e2 31 74 9a af 16 a1 46 c7 da 88 d6 27 b5";

            byte[] bPreMasterSecret = ByteHelper.ByteFromHexString(strPreMasterSecret);
            byte[] bClientRandom = ByteHelper.ByteFromHexString(strClientRandom);
            byte[] bServerRandom = ByteHelper.ByteFromHexString(strServerRandom);

            xmedianet.socketserver.TLS.ConnectionState trans = new xmedianet.socketserver.TLS.ConnectionState();

            ByteBuffer buf = new ByteBuffer();
            buf.AppendData(bClientRandom);
            buf.AppendData(bServerRandom);
            byte[] bCSRandom = buf.GetAllSamples();

            // Do it in reverse order for different algorithms
            buf.AppendData(bServerRandom);
            buf.AppendData(bClientRandom);
            byte[] bSCRandom = buf.GetAllSamples();


            byte[] MasterSecret = trans.PRF(bPreMasterSecret, "master secret", bCSRandom, 48);
            System.Diagnostics.Debug.WriteLine("Master Secret found is:\r\n{0}", ByteHelper.HexStringFromByte(MasterSecret, true, 16));
/// check... our master secret calulation matches what we see in wireshark, so all is well up to this point

            trans.SecurityParameters.Cipher = Cipher.FindCipher(CipherSuite.TLS_RSA_WITH_AES_128_CBC_SHA);
            trans.ComputeKeys(MasterSecret, bSCRandom);


            string strClientHello = "0100003103014f1ba0342bf8adbaf4974c390fe1e1dd4aa9b1af39f2e425ad27b0cea143c449000004002f00ff0100000400230000";
            string strServerHello = "0200003103014f1ba06b88c11a5796dd8379aaab4f990cc8e231749aaf16a146c7da88d627b500002f000009ff0100010000230000";
            string strServerCert = "0b00029a00029700029430820290308201f9a00302010202090086c664416cd7cd31300d06092a864886f70d01010505003061310b3009060355040613025553310b300906035504080c025458310f300d06035504070c0644616c6c61733121301f060355040a0c18496e7465726e6574205769646769747320507479204c74643111300f06035504030c086d656c6c6c766172301e170d3132303132313138343235385a170d3132303232303138343235385a3061310b3009060355040613025553310b300906035504080c025458310f300d06035504070c0644616c6c61733121301f060355040a0c18496e7465726e6574205769646769747320507479204c74643111300f06035504030c086d656c6c6c76617230819f300d06092a864886f70d010101050003818d0030818902818100b393188b70c857fc0f7511f7322e6f91d2761f335de29c488416d82a091b428d315321cb44c7970a7618f5d6e84b0cdd5dbda847cd819ed98dae5c5dec79994b1ad76989150ca551dcbf6a9876e417558eceb78aa094f8c645a6898270a6402a038ad5e7e90303632455b7a1adf07d02633cb56bada5a1a22a0a8f3d842f40f30203010001a350304e301d0603551d0e0416041456326c3cb4f1d784c483fe7e167b80a67e421b63301f0603551d2304183016801456326c3cb4f1d784c483fe7e167b80a67e421b63300c0603551d13040530030101ff300d06092a864886f70d0101050500038181000e38924f5b9f57562bf95b163d0611b512d458866d4de9db94ce56efab9f198ef81247277612b4758728f4aececb54a122641fa0396792fe3f945d99dbc75de378f381071ae3bb11b53d18235f36cedab94670816b1c0b53f0a62daa0850cc368adadb3335d4bebff111e9fbba76ac5be7175279680ebb62e640927e3f6074fe";
            string strServerDone = "0e000000";
            string strClientKE = "1000008200809f6af68a96205955bfa0a8b793cb78720f063ef7c3ef82aa10a4f306bfdd8ace898eb5546b873e7b80a7ec95a817b3acf7cf64b45058deaf498bc270ad2901fd921a566c0ddc68c59225350b89f4289aa6f30cb5944cdebc62ed75a9907aee36687b5882fc4bbe9a571137ea2142a465f5278168319cca1646e33706be614091";
            
            string strEncryptedHS = "59 09 86 f1 79 de 71 f4 b4 14 b1 ad 1a 70 c0 81 d5 24 32 8b 18 1a c4 a6 63 3a 27 d8 31 ad 54 5f 0d 85 37 a2 0f 6c 7b 9c 4b fc 83 18 e6 88 a6 67";

            ByteBuffer objHandShakeBuffer = new ByteBuffer();
            objHandShakeBuffer.AppendData(ByteHelper.ByteFromHexString(strClientHello));
            objHandShakeBuffer.AppendData(ByteHelper.ByteFromHexString(strServerHello));
            objHandShakeBuffer.AppendData(ByteHelper.ByteFromHexString(strServerCert));
            objHandShakeBuffer.AppendData(ByteHelper.ByteFromHexString(strServerDone));
            objHandShakeBuffer.AppendData(ByteHelper.ByteFromHexString(strClientKE));

            /// Example says handshake has read 976 bytes of data and written 256... let's check with wireshark
            /// Sent: 53 + 134            170
            /// Recv: 53 + 670 + 4        48

            SHA1Managed sha1 = new SHA1Managed();
            byte[] bAllHandShakeData = objHandShakeBuffer.GetAllSamples();
            byte[] bmd5OfHandshakeMessages = MD5Core.GetHash(bAllHandShakeData);
            byte[] bsha1OfHandshakeMessages = sha1.ComputeHash(bAllHandShakeData);
            ByteBuffer bSum = new ByteBuffer();
            bSum.AppendData(bmd5OfHandshakeMessages);
            bSum.AppendData(bsha1OfHandshakeMessages);

            byte[] bCombinedHashes = bSum.GetAllSamples();

            /// No use in writing out this debug, only a few of the characters get seen, the rest are dropped
            ///System.Diagnostics.Debug.WriteLine("**** Start all handshake data ****\r\n{0}\r\n**** End all handshake data ****", ByteHelper.HexStringFromByte(bAllHandshakeData, true, 32));
            /// verify_data
            ///     PRF(master_secret, finished_label, MD5(handshake_messages) + SHA-1(handshake_messages)) [0..11];

            TLSHandShakeMessage msgFinished = new TLSHandShakeMessage();
            msgFinished.HandShakeMessageType = HandShakeMessageType.Finished;
            msgFinished.HandShakeFinished.verify_data = trans.PRF(MasterSecret, "client finished", bCombinedHashes, 12);

            TLSRecord recordFinished = new TLSRecord();
            recordFinished.MajorVersion = 3;
            recordFinished.MinorVersion = 1;
            recordFinished.ContentType = TLSContentType.Handshake;
            recordFinished.Messages.Add(msgFinished);

            trans.WriteSequenceNumber = 0;
            byte[] bEncryptedGenericBlockCipher = trans.CompressEncryptOutgoingData(recordFinished);
            
            /// Check.  The value here is the same as strEncryptedHS above
            System.Diagnostics.Debug.WriteLine("Encrypted Client Done is:\r\n{0}", ByteHelper.HexStringFromByte(bEncryptedGenericBlockCipher, true, 16));
        }

        public static void GenerateStuff2()
        {
            string strPreMasterSecret = "03 01 9A 04 0E DA 89 8C 97 01 53 0D DE 42 EE CF 69 09 8A D1 99 D2 B8 93 1B DA 4C 01 BA 1C A0 B6 01 B5 5F 5B 7A A0 F5 5B 3C B1 7D 72 96 33 BC DC";
            string strClientRandom = "4f1c89f895eb17b8419927796ce5b6f20401a6e96262031f0b37a7872665f9ab";
            string strServerRandom = "4f1c89cd4170867e7a5a2d6daa4e8d01237096c70bf891d7ff650554433b7ba5";

            byte[] bPreMasterSecret = ByteHelper.ByteFromHexString(strPreMasterSecret);
            byte[] bClientRandom = ByteHelper.ByteFromHexString(strClientRandom);
            byte[] bServerRandom = ByteHelper.ByteFromHexString(strServerRandom);

            xmedianet.socketserver.TLS.ConnectionState trans = new xmedianet.socketserver.TLS.ConnectionState();

            ByteBuffer buf = new ByteBuffer();
            buf.AppendData(bClientRandom);
            buf.AppendData(bServerRandom);
            byte[] bCSRandom = buf.GetAllSamples();

            // Do it in reverse order for different algorithms
            buf.AppendData(bServerRandom);
            buf.AppendData(bClientRandom);
            byte[] bSCRandom = buf.GetAllSamples();


            byte[] MasterSecret = trans.PRF(bPreMasterSecret, "master secret", bCSRandom, 48);
            System.Diagnostics.Debug.WriteLine("Master Secret found is:\r\n{0}", ByteHelper.HexStringFromByte(MasterSecret, true, 16));
            /// check... our master secret calulation matches what we see in wireshark, so all is well up to this point

            trans.SecurityParameters.Cipher = Cipher.FindCipher(CipherSuite.TLS_RSA_WITH_AES_128_CBC_SHA);
            trans.ComputeKeys(MasterSecret, bSCRandom);


            string strClientHello = "0100002903014f1c89f895eb17b8419927796ce5b6f20401a6e96262031f0b37a7872665f9ab000002002f0100";
            string strServerHello = "0200004603014f1c89cd4170867e7a5a2d6daa4e8d01237096c70bf891d7ff650554433b7ba520880100006ddf3cb09c0eb6e40d0167000f03ca81ac58eec4c3c760a70143524d002f00";
            string strServerCert = "0b0004ba0004b70004b4308204b030820298a003020102021062be99f31b21039a47bcad3b019383c0300d06092a864886f70d0101050500301431123010060355040313096c6f63616c686f7374301e170d3132303130383036303030305a170d3232303131353036303030305a301431123010060355040313096c6f63616c686f737430820222300d06092a864886f70d01010105000382020f003082020a0282020100a0c647622204207c3948269a4338eba9b86e9494e5f18f3f024ef6850da59045f09ec8f55ed13d0407adb0f0eef2ea99c5ff6f2078a47b27a64eab77b4a1a709af8255d2b2bfd859149296dc5d93c91728165d484ec55ded72d34828ad43d782bcb81b4e3966b7cdccc88bb41355b42e0fd3ed8b24dbc99903ffb73d27d2f26bce18dd2712a84105eb6a0f4152e7a01ad7e8f1ad6d32c3b21ca3a0a6206398fbb1ac7e85f4586f5535ff35e693b5094bf7360a7a33fc9c2693087ae5777aa761a1fa016f02c320eacb51ea879f6a1dc078daab3e134bf0ae5bba774c6bdb95efe35ea7f8d8b0736ec2e28369b457b3d1c1522b810c2e7340a61b6a8c830af7c8de8047eb5f4b624d782a41e330d983515118182c6709ff41373f74d601aa5c9cb8c6a940549ab6f1f8ca11c945d17d2625acb7fe8a10109afddb9cd495da1f3e88261a063273344efb865e6d86fb0f6384f97685fb992af866616da81499a3223a2a557849c0507f27932063b73d9d2d47a2725c28dbce7b32f7f85d568142cad7c1a9cb9132a977b6e6c164a16fa0b77af027afc6dfb28a5d9e8a4babccac66e569f74a4689158a299100540230c9fdfcddc4649232e0af348d4f833f0a9344eb8f64b48f572cf9653280c2b26e6ea3fecb214ace7643e61f4c74dc79373505352f22408ed883252628b557ad749c80445d8acbe97cf9395c1897deb30dea850203010001300d06092a864886f70d010105050003820201004089ae88bd71245d1863b96505aa1342047088680f2078ca79b733eb542a4c1c267055de24210adc682f48f4d18d27d06100179a85bba5b21749cb8e71b200fb97c37b3e9ad64c197a4d4b7befdfceb67a69a80a7be24ac4cc6307ea3241f55b6800ecedee86b5fe8a21869cf8653cc0b653b71b5132c51ad2e334c16c1247155e82d744d0fdb9546d5c14bdbd9b8952d575e34335d85bd2c6079cf2adba910f98dc48a51217b617cf78ca91f57fe2b7ff88e2e78f5882530d55d63ec466a9d02e72f2a4269779880c66136a3dac96a26f334771e59cc89f3c66ddf5d21492b5d40565dbfdc0a9833d57c68728e969088612dbe655dc98a4d2449b2cf06208a4f26d831fa4d90eb79ea59d1934e61029a81c4795f28ec91df7b82b8f2ab85783f12e334d98e2313a7829f73630edd47a891de3b241ecf6aa3afd1d6c6e43db1d177a31b306bf7b05fb4fd56f462179e0efc439fa10391ce010c12607336e6379aa192bb106e86eb875a66919cc5a08692aefaebddbd92736f21b2410889530b8ceeded969624e1f9ab08957254d0029b20881f820276ede7987e86d705f88715e21e950e45624add2ee6cb682f2e9ecd37839ccda82290c5b6638981c45270f75a6c786fa734877a560e7d48c8abc122070ae0792a213de33ccfe0456f31e91c3be4833e50463e2a97ce828bbaae8324482bc07181ba7ee50fb87c65be66bb56";
            string strServerDone = "0e000000";
            string strClientKE = "1000020202000fab10111b3822b4fd32d0a73046b8d4f605eadf98bbf89bbd6924fea2e545e6ecbf2844f98de744edda195b73a4f10f9d473808697d03b3d34b947256f0f9395d914eb6a570091c559d27100c5897b8d66f4572955bc242f01d9328fe052f1e240aad72dece482aadb5440931d8271db0f59d558392013522f83c2cc9b11db4f654997f0293decd9a1a2ec950cfb2f2264158a7112ad3626fb51b368497fa4d5ef93b637b95b93b533ae9decd26e22d85a5f5d8b8ac1d151e7b11be23ff1cf341d78c42e281a536da9be3e4560a78fa428d9e44ee200504ba4d4146cbd64b5fb0179f8e015e64ccb13c5581119eb953b44f9d7081266d88fcfed08d632297fe2a94a98f2f930c5f8e564b89598e805bc3b143ea0acc1faa0e7d1d0799cb925b0f6d04f3c68b5d0cff0838ec881575b7aece5b481b17f193e9ad559c1426fa5f7407ba80e81b50ab6c5e589335fdb0861d6b351715cde0b13746b664928d640f1883760623ef867c4dd0568ffd08f03837f511abbc2821fe2d676bf2f742eeb50f27fadacbd211649a7889f0b38aedaab331747c424add3a13fe4575fffb120cbdd544b5d353b1dfbf5dd5be16545d70b436fac29fe12942b63a9dae438e0a5d3f3b116b39317161943474c477371001b3be844908799ec44aeab0f82607cf0e0ba86561c1d8a49298a17b6f2733c1340d185d337e349c58526d8407d3b38498";

            string strEncryptedHS = "35275a481265bd929ede8056ed23ad42bebd2da9d8bc841b813ce8fae921b044fb37f03604fec9de92e1873155212143";

            ByteBuffer objHandShakeBuffer = new ByteBuffer();
            objHandShakeBuffer.AppendData(ByteHelper.ByteFromHexString(strClientHello));
            objHandShakeBuffer.AppendData(ByteHelper.ByteFromHexString(strServerHello));
            objHandShakeBuffer.AppendData(ByteHelper.ByteFromHexString(strServerCert));
            objHandShakeBuffer.AppendData(ByteHelper.ByteFromHexString(strServerDone));
            objHandShakeBuffer.AppendData(ByteHelper.ByteFromHexString(strClientKE));

            /// Example says handshake has read 976 bytes of data and written 256... let's check with wireshark
            /// Sent: 53 + 134            170
            /// Recv: 53 + 670 + 4        48

            SHA1Managed sha1 = new SHA1Managed();
            byte[] bAllHandShakeData = objHandShakeBuffer.GetAllSamples();
            byte[] bmd5OfHandshakeMessages = MD5Core.GetHash(bAllHandShakeData);
            byte[] bsha1OfHandshakeMessages = sha1.ComputeHash(bAllHandShakeData);
            ByteBuffer bSum = new ByteBuffer();
            bSum.AppendData(bmd5OfHandshakeMessages);
            bSum.AppendData(bsha1OfHandshakeMessages);

            byte[] bCombinedHashes = bSum.GetAllSamples();

            /// No use in writing out this debug, only a few of the characters get seen, the rest are dropped
            ///System.Diagnostics.Debug.WriteLine("**** Start all handshake data ****\r\n{0}\r\n**** End all handshake data ****", ByteHelper.HexStringFromByte(bAllHandshakeData, true, 32));
            /// verify_data
            ///     PRF(master_secret, finished_label, MD5(handshake_messages) + SHA-1(handshake_messages)) [0..11];

            TLSHandShakeMessage msgFinished = new TLSHandShakeMessage();
            msgFinished.HandShakeMessageType = HandShakeMessageType.Finished;
            msgFinished.HandShakeFinished.verify_data = trans.PRF(MasterSecret, "client finished", bCombinedHashes, 12);

            TLSRecord recordFinished = new TLSRecord();
            recordFinished.MajorVersion = 3;
            recordFinished.MinorVersion = 1;
            recordFinished.ContentType = TLSContentType.Handshake;
            recordFinished.Messages.Add(msgFinished);

            trans.WriteSequenceNumber = 0;
            byte[] bEncryptedGenericBlockCipher = trans.CompressEncryptOutgoingData(recordFinished);

            /// Check.  The value here is the same as strEncryptedHS above
            System.Diagnostics.Debug.WriteLine("Encrypted Client Done is:\r\n{0}", ByteHelper.HexStringFromByte(bEncryptedGenericBlockCipher, true, 16));
        }
        /// <summary>
        /// Found java example on the web that gave the following inputs and outputs.
        /// Tested ours and the PRF output matches theirs, so we should be good
        /// </summary>
        public static void TestPRF()
        {
            xmedianet.socketserver.TLS.ConnectionState trans = new xmedianet.socketserver.TLS.ConnectionState();
            byte[] bSecret = new byte[48];
            int i = 0;
            for (i = 0; i < bSecret.Length; i++) bSecret[i] = 0xab;
            byte[] bSeed = new byte[64];
            for (i = 0; i < bSeed.Length; i++) bSeed[i] = 0xcd;

            byte[] bRet = trans.PRF(bSecret, "PRF Testvector", bSeed, 104);
            System.Diagnostics.Debug.WriteLine("PRF Testvector output is...(should be: D3 D4 D1 E3 49 B5 D5 15..)");
            System.Diagnostics.Debug.WriteLine(xmedianet.socketserver.TLS.ByteHelper.HexStringFromByte(bRet, true, 8));

            //"D3 D4 D1 E3 49 B5 D5 15 04 46 66 D5 1D E3 2B AB" + "25 8C B5 21 B6 B0 53 46 3E 35 48 32 FD 97 67 54" + "44 3B CF 9A 29 65 19 BC 28 9A BC BC 11 87 E4 EB" + "D3 1E 60 23 53 77 6C 40 8A AF B7 4C BC 85 EF F6" + "92 55 F9 78 8F AA 18 4C BB 95 7A 98 19 D8 4A 5D" + "7E B0 06 EB 45 9D 3A E8 DE 98 10 45 4B 8B 2D 8F" + "1A FB C6 55 A8 C9 A0 13";  

        }

        public static void TestRSACertificate()
        {
            byte [] bPublicKeyModulus = new byte [] 
            {
                0xB3, 0x93, 0x18, 0x8B, 0x70, 0xC8, 0x57, 0xFC, 0x0F, 0x75, 0x11, 0xF7, 0x32, 0x2E, 0x6F, 0x91, 
                0xD2, 0x76, 0x1F, 0x33, 0x5D, 0xE2, 0x9C, 0x48, 0x84, 0x16, 0xD8, 0x2A, 0x09, 0x1B, 0x42, 0x8D,
                0x31, 0x53, 0x21, 0xCB, 0x44, 0xC7, 0x97, 0x0A, 0x76, 0x18, 0xF5, 0xD6, 0xE8, 0x4B, 0x0C, 0xDD, 
                0x5D, 0xBD, 0xA8, 0x47, 0xCD, 0x81, 0x9E, 0xD9, 0x8D, 0xAE, 0x5C, 0x5D, 0xEC, 0x79, 0x99, 0x4B, 
                0x1A, 0xD7, 0x69, 0x89, 0x15, 0x0C, 0xA5, 0x51, 0xDC, 0xBF, 0x6A, 0x98, 0x76, 0xE4, 0x17, 0x55, 
                0x8E, 0xCE, 0xB7, 0x8A, 0xA0, 0x94, 0xF8, 0xC6, 0x45, 0xA6, 0x89, 0x82, 0x70, 0xA6, 0x40, 0x2A, 
                0x03, 0x8A, 0xD5, 0xE7, 0xE9, 0x03, 0x03, 0x63, 0x24, 0x55, 0xB7, 0xA1, 0xAD, 0xF0, 0x7D, 0x02, 
                0x63, 0x3C, 0xB5, 0x6B, 0xAD, 0xA5, 0xA1, 0xA2, 0x2A, 0x0A, 0x8F, 0x3D, 0x84, 0x2F, 0x40, 0xF3 
            };

            // Taken from openssl on our test certificate
            byte[] bPrivateExponent = new byte[] 
            {
                0x42, 0x7e, 0x26, 0x29, 0x83, 0xd2, 0x7b, 0x59, 0xd7, 0x33, 0x67, 0x3a, 0x9c, 0x37, 0x3b,
                0x92, 0xc8, 0x56, 0x7a, 0xc9, 0x1f, 0x6b, 0x88, 0xa9, 0x05, 0x58, 0x1c, 0x24, 0xbc, 0x88,
                0x7e, 0x85, 0x1f, 0x8d, 0x83, 0xc6, 0xeb, 0xa9, 0xe8, 0x10, 0xb4, 0x98, 0x1b, 0x77, 0xbf,
                0x3e, 0x02, 0xfe, 0x78, 0xf6, 0x80, 0x38, 0x4e, 0x2d, 0x3f, 0xef, 0x98, 0x99, 0xc6, 0x93,
                0xf4, 0xbb, 0x35, 0xfa, 0x4d, 0x25, 0x93, 0x3b, 0xfa, 0xf0, 0x35, 0x21, 0x18, 0xe7, 0x16,
                0x5a, 0x21, 0x2a, 0xd1, 0x8e, 0xe4, 0x27, 0xcc, 0xca, 0x84, 0xe5, 0xc6, 0x31, 0x1d, 0x6f,
                0x2f, 0x3c, 0xc6, 0x17, 0x6d, 0x55, 0x2c, 0x1a, 0x95, 0xbe, 0x90, 0x18, 0xf8, 0x44, 0xc0,
                0x8a, 0xdd, 0x1e, 0x11, 0x68, 0x67, 0x42, 0xad, 0x6e, 0xb0, 0x41, 0x68, 0x9d, 0xa8, 0xe6,
                0xf0, 0x81, 0x36, 0xd3, 0x7f, 0x34, 0x16, 0x41
            };

            byte[] bPublicKeyExponent = new byte[] { 0x01, 0x00, 0x01 };

            string strTextToBeEncrypted = "12345";
            string strEncryptedWithPublicKey = "5A D9 BF B9 44 4C C5 F2 0E 9C F5 61 D7 D2 BA B0 54 7A 46 74 BF 3A A3 5E 1D 45 23 9A A0 C8 A0 AF F5 11 16 25 03 3D 77 04 C4 85 80 05 52 FB 83 22 06 63 6C 65 82 A9 75 12 72 E2 82 07 B8 72 EA 30 66 0E 75 6E D2 D4 B6 2F DD B2 61 15 4C D3 53 465B 68 36 F6 C1 3E 90 74 52 07 4E 32 EC 8D 7F A2 F1 71 64 33 F3 2D 99 9A 12 D1 ED ED AB 9D B0 D3 18 A9 B2 E2 99 C0 C7 C3 BA AF EF 51 5C 05 0F 4D";
            string strEncryptedWithPrivateKey= "62 D9 64 6E C2 57 BA 58 92 E0 13 2D EA 36 71 49 83 B6 7D 32 32 EE A6 89 FF 71 8F 60 94 43 6E 85 5D AB FF 52 61 92 A5 EE 8A 9E AE 27 51 3B A3 65 22 C3 EC AB 29 E2 2D EB 29 04 51 D9 6B D4 5F 6C 36 8C 62 80 F7 D3 69 8B A2 51 43 CF B3 9D 03 50A4 FB 93 BB CC 42 5F 20 FF 9B 51 9B 4D 56 30 10 35 14 CE EF E6 52 5F 07 52 AC 98 BB 54 2A 79 0F 08 E8 20 45 6A 77 78 85 0D BF 7B E2 6F F9 43 F8";


            byte[] bTextToBeEncrypted = System.Text.UTF8Encoding.UTF8.GetBytes(strTextToBeEncrypted);
            byte[] bEncryptedWithPublicKey = ByteHelper.ByteFromHexString(strEncryptedWithPublicKey);
            byte[] bEncryptedWithPrivateKey = ByteHelper.ByteFromHexString(strEncryptedWithPrivateKey);

                
                /// This RSA certificate was generated with OpenSSL.  A file is encrypted with OpenSSL using the private key to make sure we
            /// can decrypt it with the public key
            /// 


            RSACryptoServiceProvider provider = new RSACryptoServiceProvider();
            
            RSAParameters rsaparams = new RSAParameters();
            rsaparams.Modulus = bPublicKeyModulus;
            rsaparams.Exponent = bPublicKeyExponent;
            provider.ImportParameters(rsaparams);
            
            /// Verified that both of these methods work, they encrypt the string to a form that can be unencrypted (to reproduce "12345")
            /// by using the private key with openssl  
            /// Steps taken
            /// Save the debug output hex string produced below as a ascii hex file (384 bytes long)
            /// convert the ascii hex file to a binary file (128 bytes long)
            ///     (I wrote a program called ba.exe to do this)
            /// run openssl against the binary file:
            /// C:\openssl\bin>openssl rsautl -decrypt -in hex2.bin -inkey test_key.pem -out hex2.decrypt
            /// verify that file hex2.decrypt has the ascii contents "12345"  (it did with both outputs below)
            /// 
            /// (Note the output is different every time because of PKCS padding applied to make it different every time, this is stripped once it's decrypted)
            /// 

            byte[] bEncryptedText1 = provider.Encrypt(bTextToBeEncrypted, false);
            System.Diagnostics.Debug.WriteLine(ByteHelper.HexStringFromByte(bEncryptedText1, true, 16));
            System.Diagnostics.Debug.WriteLine("");

         
            RSAPKCS1KeyExchangeFormatter formm = new RSAPKCS1KeyExchangeFormatter(provider);
            byte[] bEncryptedText = formm.CreateKeyExchange(bTextToBeEncrypted);
            System.Diagnostics.Debug.WriteLine(ByteHelper.HexStringFromByte(bEncryptedText, true, 16));
            System.Diagnostics.Debug.WriteLine("");
            System.Diagnostics.Debug.WriteLine("");


            provider.Dispose();
        }

    }
}
#endif
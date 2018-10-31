using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Text;

namespace AmigaOsBuilder.Tests.EncodingConverterTests
{
    [TestClass]
    public class EncodingConverter_ConvertIsoBytesToUtf8String
    {
        //español.ct
        private byte[] _espanolIsoBytes = {
              0x65, 0x73, 0x70, 0x61, 0xF1, 0x6F, 0x6C, 0x2E, 0x63, 0x74
            };
        private byte[] _espanolSubIsoBytes = {
                0x00, 0x1E, 0x4A, 0x61, 0x6E, 0x6F, 0x5F, 0x76, 0x31, 0x2E, 0x30, 0x31,
                0x5C, 0x43, 0x61, 0x74, 0x61, 0x6C, 0x6F, 0x67, 0x73, 0x5C, 0x65, 0x73,
                0x70, 0x61, 0xF1, 0x6F, 0x6C, 0x2E, 0x63, 0x74, 0x22, 0x2D, 0x08, 0x18,
                0x73, 0x96, 0xDA, 0x37, 0x1B, 0x72, 0x6C, 0x73, 0xE0, 0x0E, 0xF0, 0x91,
                0xD6, 0xDD, 0x12, 0xA8, 0xEC, 0xB8, 0x7D, 0x60, 0x90, 0x14, 0x51, 0xE4,
                0xCB, 0x89, 0x12, 0x99, 0x25, 0x3E, 0x7C, 0x24, 0x83, 0xAE, 0xA2, 0x77,
                0x92, 0xD7
            };

        [TestMethod]
        public void EncodingConverter_ConvertIsoBytesToUtf8String_Espanol()
        {
            var ec = new EncodingConverter();
            var result = ec.ConvertIsoBytesToUtf8String(_espanolIsoBytes);
            Assert.AreEqual("español.ct", result);
        }
        
        [TestMethod]
        public void EncodingConverter_ConvertIsoBytesToUtf8String_EspanolSubstring()
        {
            var ec = new EncodingConverter();
            var result = ec.ConvertIsoBytesToUtf8String(_espanolSubIsoBytes, 22, 10);
            Assert.AreEqual("español.ct", result);
        }

        [TestMethod]
        public void EncodingConverter_ConvertUtf8StringToIsoBytes_Espanol()
        {          
            var ec = new EncodingConverter();
            var result = ec.ConvertUtf8StringToIsoBytes("español.ct");
            Assert.AreEqual(_espanolIsoBytes.Length, result.Length);
            Assert.AreEqual(_espanolIsoBytes[0], result[0]);
            Assert.AreEqual(_espanolIsoBytes[1], result[1]);
            Assert.AreEqual(_espanolIsoBytes[2], result[2]);
            Assert.AreEqual(_espanolIsoBytes[3], result[3]);
            Assert.AreEqual(_espanolIsoBytes[4], result[4]);
            Assert.AreEqual(_espanolIsoBytes[5], result[5]);
            Assert.AreEqual(_espanolIsoBytes[6], result[6]);
            Assert.AreEqual(_espanolIsoBytes[7], result[7]);
            Assert.AreEqual(_espanolIsoBytes[8], result[8]);
            Assert.AreEqual(_espanolIsoBytes[9], result[9]);
        }

        [TestMethod]
        public void EncodingConverter_OldUtf8GetStringWasntWorking()
        {
            var result2 = Encoding.UTF8.GetString(_espanolIsoBytes, 0, _espanolIsoBytes.Length);
            Assert.AreNotEqual("español.ct", result2);
        }

        [TestMethod]
        public void EncodingConverter_OldUtf8GetBytesWasntWorking()
        {
            var result = Encoding.UTF8.GetBytes("español.ct");

            Assert.AreNotEqual(_espanolIsoBytes.Length, result.Length);
            Assert.AreEqual(_espanolIsoBytes[0], result[0]);
            Assert.AreEqual(_espanolIsoBytes[1], result[1]);
            Assert.AreEqual(_espanolIsoBytes[2], result[2]);
            Assert.AreEqual(_espanolIsoBytes[3], result[3]);
            Assert.AreNotEqual(_espanolIsoBytes[4], result[4]);
            Assert.AreNotEqual(_espanolIsoBytes[5], result[5]);
            Assert.AreNotEqual(_espanolIsoBytes[6], result[6]);
            Assert.AreNotEqual(_espanolIsoBytes[7], result[7]);
            Assert.AreNotEqual(_espanolIsoBytes[8], result[8]);
            Assert.AreNotEqual(_espanolIsoBytes[9], result[9]);
        }
        
        [TestMethod]
        public void EncodingConverter_OldTwoErrorsDoesntMakeOneRight()
        {
            var espanolString = Encoding.UTF8.GetString(_espanolIsoBytes, 0, _espanolIsoBytes.Length);

            var result = Encoding.UTF8.GetBytes(espanolString);

            Assert.AreNotEqual(_espanolIsoBytes.Length, result.Length);
            Assert.AreEqual(_espanolIsoBytes[0], result[0]);
            Assert.AreEqual(_espanolIsoBytes[1], result[1]);
            Assert.AreEqual(_espanolIsoBytes[2], result[2]);
            Assert.AreEqual(_espanolIsoBytes[3], result[3]);
            Assert.AreNotEqual(_espanolIsoBytes[4], result[4]);
            Assert.AreNotEqual(_espanolIsoBytes[5], result[5]);
            Assert.AreNotEqual(_espanolIsoBytes[6], result[6]);
            Assert.AreNotEqual(_espanolIsoBytes[7], result[7]);
            Assert.AreNotEqual(_espanolIsoBytes[8], result[8]);
            Assert.AreNotEqual(_espanolIsoBytes[9], result[9]);
        }
    }
}

using AmeisenBotUtilities;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace AmeisenBotUtilities.Tests
{
    [TestClass()]
    public class UtilsTests
    {
        [TestMethod()]
        public void FirstCharToUpperTest()
        {
            Assert.AreEqual("AyyLMAO", Utils.FirstCharToUpper("ayyLMAO"));
        }

        [TestMethod()]
        public void GenerateRandonStringTest()
        {
            Assert.AreEqual(8, Utils.GenerateRandonString(8, "ABCDEFGHIJKLMNOPQRSTUVWXYZ").Length);
        }

        [TestMethod()]
        public void GetDistanceTest()
        {
            Vector3 a = new Vector3(0, 0, 0);
            Vector3 b = new Vector3(0, 100, 0);

            Assert.AreEqual(100.0, Utils.GetDistance(a, b));
        }

        [TestMethod()]
        public void ByteArrayToStringTest()
        {
            Assert.AreEqual("0102030405060708", Utils.ByteArrayToString(new byte[] {01, 02, 03, 04, 05, 06, 07, 08}));
        }
    }
}
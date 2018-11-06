using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace AmigaOsBuilder.Tests.WinPathFixerTests
{
    [TestClass]
    public class WinPathFixer_FixPath
    {
        [TestMethod]
        public void WinPathFixer_FixPath_FileNameConExe_Fixed()
        {
            var fixer = new WinPathFixer();
            var result = fixer.FixPath(@"C:\somedrive\CON.exe");

            Assert.AreEqual(@"C:\somedrive\_amigafixed_CON.exe", result);
        }

        [TestMethod]
        public void WinPathFixer_FixPath_FileNameCon_Fixed()
        {
            var fixer = new WinPathFixer();
            var result = fixer.FixPath(@"C:\somedrive\CON");

            Assert.AreEqual(@"C:\somedrive\_amigafixed_CON", result);
        }

        [TestMethod]
        public void WinPathFixer_FixPath_FileNameContExe_NotFixed()
        {
            var fixer = new WinPathFixer();
            var result = fixer.FixPath(@"C:\somedrive\CONT.exe");

            Assert.AreEqual(@"C:\somedrive\CONT.exe", result);
        }

        [TestMethod]
        public void WinPathFixer_FixPath_FileNameCont_NotFixed()
        {
            var fixer = new WinPathFixer();
            var result = fixer.FixPath(@"C:\somedrive\CONT");

            Assert.AreEqual(@"C:\somedrive\CONT", result);
        }

        [TestMethod]
        public void WinPathFixer_FixPath_FileNameContCon_NotFixed()
        {
            var fixer = new WinPathFixer();
            var result = fixer.FixPath(@"C:\somedrive\CONT.CON");

            Assert.AreEqual(@"C:\somedrive\CONT.CON", result);
        }


        [TestMethod]
        public void WinPathFixer_FixPath_FolderNameConExe_Fixed()
        {
            var fixer = new WinPathFixer();
            var result = fixer.FixPath(@"C:\somedrive\CON.exe\file");

            Assert.AreEqual(@"C:\somedrive\_amigafixed_CON.exe\file", result);
        }

        [TestMethod]
        public void WinPathFixer_FixPath_FolderNameCon_Fixed()
        {
            var fixer = new WinPathFixer();
            var result = fixer.FixPath(@"C:\somedrive\CON\file");

            Assert.AreEqual(@"C:\somedrive\_amigafixed_CON\file", result);
        }

        [TestMethod]
        public void WinPathFixer_FixPath_FolderNameContExe_NotFixed()
        {
            var fixer = new WinPathFixer();
            var result = fixer.FixPath(@"C:\somedrive\CONT.exe\file");

            Assert.AreEqual(@"C:\somedrive\CONT.exe\file", result);
        }

        [TestMethod]
        public void WinPathFixer_FixPath_FolderNameCont_NotFixed()
        {
            var fixer = new WinPathFixer();
            var result = fixer.FixPath(@"C:\somedrive\CONT\file");

            Assert.AreEqual(@"C:\somedrive\CONT\file", result);
        }

        [TestMethod]
        public void WinPathFixer_FixPath_FolderNameContCon_NotFixed()
        {
            var fixer = new WinPathFixer();
            var result = fixer.FixPath(@"C:\somedrive\CONT.CON\file");

            Assert.AreEqual(@"C:\somedrive\CONT.CON\file", result);
        }

        [TestMethod]
        public void WinPathFixer_FixPath_EmptyPath()
        {
            var fixer = new WinPathFixer();
            var result = fixer.FixPath(@"");

            Assert.AreEqual(@"", result);
        }

        [TestMethod]
        public void WinPathFixer_FixPath_Spaces_NotRemoved()
        {
            var fixer = new WinPathFixer();
            var result = fixer.FixPath(@"C:\somedrive\CON CON\file");

            Assert.AreEqual(@"C:\somedrive\CON CON\file", result);
        }
    }
}

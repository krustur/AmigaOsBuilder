using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace AmigaOsBuilder.Tests.WinPathFixerTests
{
    [TestClass]
    public class WinPathFixer_DefixPath
    {
        [TestMethod]
        public void WinPathFixer_DefixPath_FileNameConExe_Defixed()
        {
            var fixer = new WinPathFixer();
            var result = fixer.DefixPath(@"C:\somedrive\_amigafixed_CON.exe");

            Assert.AreEqual(@"C:\somedrive\CON.exe", result);
        }

        [TestMethod]
        public void WinPathFixer_DefixPath_FileNameCon_Defixed()
        {
            var fixer = new WinPathFixer();
            var result = fixer.DefixPath(@"C:\somedrive\_amigafixed_CON");

            Assert.AreEqual(@"C:\somedrive\CON", result);
        }

        [TestMethod]
        public void WinPathFixer_DefixPath_FolderNameConExe_Defixed()
        {
            var fixer = new WinPathFixer();
            var result = fixer.DefixPath(@"C:\somedrive\_amigafixed_CON.exe\file");

            Assert.AreEqual(@"C:\somedrive\CON.exe\file", result);
        }

        [TestMethod]
        public void WinPathFixer_DefixPath_FolderNameCon_Defixed()
        {
            var fixer = new WinPathFixer();
            var result = fixer.DefixPath(@"C:\somedrive\_amigafixed_CON\file");

            Assert.AreEqual(@"C:\somedrive\CON\file", result);
        }        
    }
}

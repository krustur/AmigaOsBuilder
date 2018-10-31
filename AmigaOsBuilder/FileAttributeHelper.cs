//using System;
//using System.IO;

//namespace AmigaOsBuilder
//{
//    /*
       
//        H = Hold    H_______ 0x80 = Hidden
//        S = Script  _S______ 0x40 = ?
//        P = Pure    __P_____ 0x20 = System
//        A = Archive ___A____ 0x10 = !Archive
//        R = Read    ____R___ 0x08 = ?
//        W = Write   _____W__ 0x04 = Readonly
//        E = Execute ______E_ 0x02 = ?
//        D = Delete  _______D 0x01 = Readonly

//     */
//    public class FileAttributeHelper
//    {
//        public static byte ToAmigaAttributes(FileAttributes winAttributes)
//        {
//            byte amigaAttributes = 0x00;

//            if (winAttributes.HasFlag(FileAttributes.Hidden)){ amigaAttributes |= 0x80; }
//            if (winAttributes.HasFlag(FileAttributes.System)) { amigaAttributes |= 0x20; }
//            if (!winAttributes.HasFlag(FileAttributes.Archive)) { amigaAttributes |= 0x10; }
//            if (winAttributes.HasFlag(FileAttributes.ReadOnly)) { amigaAttributes |= 0x04; }
//            //if (winAttributes.HasFlag(FileAttributes.ReadOnly)) { amigaAttributes |= 0x01; }

//            return amigaAttributes;
//        }

//        internal static FileAttributes ToWinAttributes(byte amigaAttributes)
//        {
//            FileAttributes winAttributes = FileAttributes.Archive;

//            if ((amigaAttributes & 0x80) != 0) { winAttributes |= FileAttributes.Hidden; }
//            if ((amigaAttributes & 0x20) != 0) { winAttributes |= FileAttributes.System; }
//            if ((amigaAttributes & 0x10) != 0) { winAttributes &= ~FileAttributes.Archive; }
//            if ((amigaAttributes & 0x04) != 0) { winAttributes |= FileAttributes.ReadOnly; }

//            return winAttributes;
//        }
//    }
//}
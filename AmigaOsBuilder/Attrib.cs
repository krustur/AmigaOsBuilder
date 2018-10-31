using System;
using System.IO;

namespace AmigaOsBuilder
{
    public class Attributes
    {
        private byte _amigaAttributes;
        //private FileAttributes _winAttributes;

        public Attributes(byte amigaAttributes)
        {
            _amigaAttributes = amigaAttributes;
            //_winAttributes = FileAttributeHelper.ToWinAttributes(amigaAttributes);
        }

        //public Attributes(FileAttributes winAttributes)
        //{
        //    _amigaAttributes = FileAttributeHelper.ToAmigaAttributes(winAttributes);
        //    _winAttributes = winAttributes;
        //}

        public byte GetAmigaAttributes()
        {
            return _amigaAttributes;
        }

        //public FileAttributes GetWinAttributes()
        //{
        //    return _winAttributes;
        //}

        public override string ToString()
        {
            return $"{_amigaAttributes}";
        }

        // override object.Equals
        public override bool Equals(object obj)
        {
            //       
            // See the full list of guidelines at
            //   http://go.microsoft.com/fwlink/?LinkID=85237  
            // and also the guidance for operator== at
            //   http://go.microsoft.com/fwlink/?LinkId=85238
            //

            if (obj == null || GetType() != obj.GetType())
            {
                return false;
            }

            // TODO: write your implementation of Equals() here
            return /*this._winAttributes == ((Attributes)obj)._winAttributes &&*/ this._amigaAttributes == ((Attributes)obj)._amigaAttributes;
            //return base.Equals(obj);
        }

        // override object.GetHashCode
        public override int GetHashCode()
        {
            // TODO: write your implementation of GetHashCode() here
            return this._amigaAttributes /** 0x00010000 + (int)this._winAttributes*/;
            //throw new System.NotImplementedException();
            //return base.GetHashCode();
        }

        public static bool operator ==(Attributes lhs, Attributes rhs)
        {
            // Check for null on left side.
            if (Object.ReferenceEquals(lhs, null))
            {
                if (Object.ReferenceEquals(rhs, null))
                {
                    // null == null = true.
                    return true;
                }

                // Only the left side is null.
                return false;
            }
            // Equals handles case of null on right side.
            return lhs.Equals(rhs);
        }

        public static bool operator !=(Attributes lhs, Attributes rhs)
        {
            return !(lhs == rhs);
        }
    }
}
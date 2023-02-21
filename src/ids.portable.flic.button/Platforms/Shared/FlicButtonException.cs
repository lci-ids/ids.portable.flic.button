using System;
using System.Collections.Generic;
using System.Text;

namespace IDS.Portable.Flic.Button.Platforms.Shared
{
    public class FlicButtonException : Exception
    {
        public FlicButtonException(string message, Exception? innerException = null) : base(message, innerException)
        {
        }
    }

    public class FlicButtonManagerNullException : FlicButtonException
    {
        public FlicButtonManagerNullException(Exception? innerException = null) : base("Flic Button manager is null, make sure it's initialized.", innerException)
        {
        }
    }

    public class FlicButtonNullException : FlicButtonException
    {
        public FlicButtonNullException(string message, Exception? innerException = null) : base(message, innerException)
        {
        }
    }
}

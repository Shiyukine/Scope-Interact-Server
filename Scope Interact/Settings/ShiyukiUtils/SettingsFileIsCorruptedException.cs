using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShiyukiUtils.Exceptions
{
    class SettingsFileIsCorruptedException : Exception
    {
        public SettingsFileIsCorruptedException()
        {
        }

        public SettingsFileIsCorruptedException(string message)
            : base(message)
        {
        }

        public SettingsFileIsCorruptedException(string message, Exception inner)
            : base(message, inner)
        {
        }
    }
}

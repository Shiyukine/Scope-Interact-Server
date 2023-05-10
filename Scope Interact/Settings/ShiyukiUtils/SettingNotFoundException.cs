using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShiyukiUtils.Exceptions
{
    public class SettingNotFoundException : Exception
    {
        public SettingNotFoundException()
        {
        }

        public SettingNotFoundException(string message)
            : base(message)
        {
        }

        public SettingNotFoundException(string message, Exception inner)
            : base(message, inner)
        {
        }
    }
}

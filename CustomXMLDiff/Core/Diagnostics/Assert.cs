using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CustomXMLDiff.Core.Diagnostics
{
    public static class Assert
    {
        public static void ArgumentNotNull(object ob, string message="Argument is null")
        {
            if (ob == null)
            {
                throw new NullReferenceException(message);
            }
        }
        public static void StringIsNullOrEmpty(string ob, string message="Argument is null")
        {
            if (string.IsNullOrEmpty(ob))
            {
                throw new Exception(message);
            }
        }
    }
}

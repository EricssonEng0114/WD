using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;

namespace UBPC.Web.Common
{
    public static class ExceptionManager
    {
        public static String Format(Exception e)
        {
            if (e == null)
                return String.Empty;

            Exception ex = e;
            StringBuilder sb = new StringBuilder();

            while (ex != null)
            {
                if (sb.Length > 0)
                    sb.Append("|");

                sb.AppendLine("Message:");
                sb.AppendFormat("   {0}", ex.Message);
                sb.AppendLine(Environment.NewLine);

                sb.AppendLine("Exception Type:");
                sb.AppendFormat("   {0}", ex.GetType().ToString());
                sb.AppendLine(Environment.NewLine);

                sb.AppendLine("Source:");
                sb.AppendFormat("   {0}", ex.Source);
                sb.AppendLine(Environment.NewLine);

                if (ex.Data.Count > 0)
                {
                    sb.AppendLine("Data:");

                    foreach (DictionaryEntry de in ex.Data)
                    {
                        sb.AppendFormat("   {0} = {1}", de.Key, de.Value);
                    }

                    sb.AppendLine(Environment.NewLine);
                }

                sb.AppendLine("Stack Trace:");
                sb.Append(ex.StackTrace);

                ex = ex.InnerException;
            }

            return sb.ToString();
        }

        public static byte[] Serialize(Exception exception)
        {
            using (MemoryStream stream = new MemoryStream())
            {
                IFormatter formatter = new BinaryFormatter();
                formatter.Serialize(stream, exception);
                return stream.GetBuffer();
            }
        }

        public static Exception Deserialize(byte[] buffer)
        {
            using (MemoryStream stream = new MemoryStream(buffer))
            {
                IFormatter fomatter = new BinaryFormatter();
                return (fomatter.Deserialize(stream) as Exception);
            }
        }
    }
}

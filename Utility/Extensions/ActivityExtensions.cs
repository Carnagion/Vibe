using System.IO;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;

namespace Vibe.Utility.Extensions
{
    internal static class ActivityExtensions
    {
        internal static string? ReadFromFile(this Activity activity, string? filePath)
        {
            Stream? input = activity.OpenFileInput(filePath);
            if (input is null)
            {
                return null;
            }
            StringBuilder builder = new();
            while (true)
            {
                int read = input.ReadByte();
                if (read is -1)
                {
                    break;
                }
                builder.Append((char)read);
            }
            return builder.ToString();
        }

        internal static void WriteToFile(this Activity activity, string? filePath, string text)
        {
            Stream? output = activity.OpenFileOutput(filePath, FileCreationMode.Private);
            if (output is null)
            {
                return;
            }
            text.Execute(character => output.WriteByte((byte)character));
            output.Flush();
            output.Close();
        }
    }
}
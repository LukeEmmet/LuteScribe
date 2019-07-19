using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace LuteScribe.Serialization.Commandline
{
    public class TabOutputParser
    {
        public string Messages { get; }
        public bool HasErrors { get; }

        public TabOutputParser(int exitCode, string stdOut, string stdErr)
        {
            HasErrors = false;
            Messages = "";

            //this parser assumes that TAB is called with a -q argument
            //which reduces all unecessary output messages. But the first line
            //always still seems to be "setting filename to XYZ", so we see what is
            //after that...
            var findErrors = "setting filename to [^\\r\\n]+([\\s\\S]+)$";

            //if TAB has errors, they will be in the std output, 
            //and exit code will probably be -1, or at least not 0
            var match = (new Regex(findErrors)).Match(stdOut);
            if (match.Success)
            {
                var message = match.Groups[1].ToString();
                if (!string.IsNullOrWhiteSpace(message))
                {
                    //non-empty messages 
                    HasErrors = true;
                    Messages = message;
                }

            }
            else if (exitCode != 0)
            {
                //tab returned non zero exit code - could indicate an error
                HasErrors = true;
                Messages = "exit code: " + exitCode;

                if (stdErr.Length > 0) { Messages += "\n" + stdErr; }
                if (stdOut.Length > 0) { Messages += "\n" + stdOut; }
            }


        }
    }
}

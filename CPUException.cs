using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CPUFramework
{
    public class CPUException : Exception
    {
        public CPUException(string message = "", string sprocnameval = "") : base(message)
        {
            this.SprocName = sprocnameval;
        }
        public string SprocName { get; internal set; }

        public string FriendlyMessage
        {
            get
            {
                string msg = this.Message;

                    string[] substrings = msg.Split("_");
                    string[] laststring = substrings[substrings.Count()-1].Split("\".");
                    string[] lastcolumn = substrings[substrings.Count() - 1].Split("\'.");
                if (this.Message.Contains("f_") && this.Message.Contains("DELETE"))
                {
                    msg = "This record from " + substrings[1] + " cannot be deleted because it has a record from " + laststring[0] + " associated with it.";
                }
                if(this.Message.Contains("f_") && this.Message.Contains("INSERT") || this.Message.Contains("UPDATE"))
                {
                    msg = "This " + laststring[0] + " record cannot be saved as attempted because the value for " + substrings[1] + " is not valid.";
                }
                if (this.Message.Contains("ck_"))
                {
                    msg = "This record cannot be saved as attempted because";

                    for (int i = 1;  i < substrings.Count() - 1; i++)
                    {
                        msg += " " + substrings[i];
                    }

                    msg += " " + laststring[0] + ".";
                }
                if (this.Message.Contains("u_") && substrings.Count() == 3)
                {
                    
                    msg = "The " + lastcolumn[0] + " in the " + substrings[1] + " table must be unique.";
                }
                if (this.Message.Contains("u_") && substrings.Count() > 3)
                {
                    msg = "The columns";
                    for (int i = 2; i < substrings.Count() - 2; i++)
                    {
                        msg += " " + substrings[i] + ",";
                    }
                    msg += " and " + lastcolumn[0] + " from the " + substrings[1] + " table must be uique.";
                }
                    return msg;
            }
        }
    }
}


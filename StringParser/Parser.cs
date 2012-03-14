using System;
using System.Text;
using System.Collections;
using Microsoft.SPOT;

namespace StringParser
{
    public class Parser
    {
        Hashtable commands = null;
        public delegate void ParserHandler(object[] args, int argNum);
    
        public Parser()
        {
            commands = new Hashtable();
        }

        public void addCommand(string com, ParserHandler handler)
        {
            commands.Add(com, handler);
        }

        public void parse(byte[] b)
        {

            string[] rx_dataString = (new string(Encoding.UTF8.GetChars(b))).TrimEnd('\r','\n').Split(',');
            string command = rx_dataString[0];

            string[] par = new string[rx_dataString.Length - 1];
            Array.Copy(rx_dataString, 1, par, 0, par.Length);

            if (commands.Contains(command))
            {
                ParserHandler handler = commands[command] as ParserHandler;

                if (handler != null)
                {
                    handler(par, par.Length);
                }
            }

            else Debug.Print("Unknown command!");
        }
    }
}

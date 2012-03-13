using System;
using System.Text;
using System.Collections;
using Microsoft.SPOT;

namespace StringParser
{
    public class Parser
    {
        Hashtable commands = null;
        public delegate void ParserHandler(object[] args);
    
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
            string[] rx_dataString = (new string(Encoding.UTF8.GetChars(b))).Split(',');

            if (commands.Contains(rx_dataString[0]))
            {
                ParserHandler handler = commands[rx_dataString[0]] as ParserHandler;

                if (handler != null)
                {
                    handler(rx_dataString);
                }
            }

            else Debug.Print("Unknown command!");
        }
    }
}

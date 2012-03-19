using System;
using System.Text;
using System.Collections;
using Microsoft.SPOT;

namespace edobezLib
{
    public class StringParser
    {
        Hashtable commands = null;
        public delegate void ParserHandler(string[] args, int argNum);
    
        public StringParser()
        {
            commands = new Hashtable();
        }

        public void addCommand(string com, ParserHandler handler)
        {
            commands.Add(com.ToLower(), handler);
        }

        public bool parse(byte[] b)
        {
            // Array di char -> Stringa codificata in UTF8 -> Tutto lowercase -> Tolte terminazioni -> Diviso in un array
            string[] rx_dataString = (new string(Encoding.UTF8.GetChars(b))).ToLower().TrimEnd('\r','\n').Split(','); 
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
                return true;
            }

            else
            {
                //Debug.Print("Unknown command!");
                return false;
            }
        }
    }
}

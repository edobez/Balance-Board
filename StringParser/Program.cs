using System;
using System.Threading;
using System.IO.Ports;
using System.Text;

using Microsoft.SPOT;

namespace StringParser
{
    public class Program
    {
        static SerialPort uart;
        static Parser p;
        public static void Main()
        {
            uart = new SerialPort("COM2", 115200);
            p = new Parser();

            uart.Open();
            uart.DataReceived += new SerialDataReceivedEventHandler(uart_DataReceived);
            p.addCommand("CIAO", onCiao);

            Thread.Sleep(Timeout.Infinite);
        }

        static void uart_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            Thread.Sleep(5);
            byte[] rxData = new byte[16];
            uart.Read(rxData, 0, rxData.Length);

            Debug.Print(new String(Encoding.UTF8.GetChars(rxData)));

            p.parse(rxData);
        }

        private static void onCiao(object[] args)
        {
            Debug.Print(Resources.GetString(Resources.StringResources.String1));
        }

    }
}

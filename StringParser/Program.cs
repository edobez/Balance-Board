using System;
using System.Threading;
using System.IO.Ports;
using System.Text;

using Microsoft.SPOT;

namespace StringParser
{
    public class Program
    {
        static SerialPort UART;
        static Parser p;
        public static void Main()
        {
            UART = new SerialPort("COM2", 57600);
            p = new Parser();

            UART.ReadTimeout = 500;
            UART.Open();
            UART.DataReceived += new SerialDataReceivedEventHandler(uart_DataReceived);

            p.addCommand("CIAO", onCiao);

            Thread.Sleep(Timeout.Infinite);
        }

        static void uart_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            Debug.Print("Data recieved!");

            Thread.Sleep(5);
            byte[] rxData = new byte[32];
            UART.Read(rxData, 0, rxData.Length);

            Debug.Print("Received: " + new String(Encoding.UTF8.GetChars(rxData)));

            if (rxData.Length != 0) p.parse(rxData);
        }

        private static void onCiao(object[] args, int argNum)
        {
            Debug.Print("Called onCiao");
            foreach (string arg in args)
            {
                Debug.Print(arg);
            }
        }

    }
}

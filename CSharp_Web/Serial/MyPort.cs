using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.IO.Ports;
using CSharp_Web.Models;

namespace CSharp_Web.Serial
{
    public class MyPort
    {
        public SerialPort port = new SerialPort("COM3", 9600);
        public int id = 1;
        public string name = "COM3";
        public int rate = 9600;
        public double temperature = 0;
        public double light = 0;
        public double red = 0;
        public double green = 0;
        public double yellow = 0;
        public double blue = 0;
        public double white = 0;
        public string sent = "";
        public string recv = "";

        public MyPort()
        {

        }

        public void Close()
        {
            port.Close();
        }

        public void WriteLED(double rV, double gV, double yV, double bV, double wV)
        {
            if (!port.IsOpen)
            {
                port.Open();
            }
            red = rV;
            green = gV;
            yellow = yV;
            blue = bV;
            white = wV;

            int[] LED = new int[5];
            LED[0] = (int)rV;
            LED[1] = (int)gV;
            LED[2] = (int)yV;
            LED[3] = (int)bV;
            LED[4] = (int)wV;
            string[] sLED = new string[5];
            string LED_Write = "";
            for (int j = 0; j < 5; j++)
            {
                if (LED[j] >= 10)
                {
                    sLED[j] = "00" + LED[j].ToString();
                }
                else
                {
                    sLED[j] = "000" + LED[j].ToString();
                }
                LED_Write += sLED[j];
            }
            List<byte> bWrite = new List<byte>();
            List<string> sWrite = new List<string>();
            for (int j = 0; j < LED_Write.Length; j += 2)
            {
                sWrite.Add(LED_Write.Substring(j, 2));
            }
            for (int j = 0; j < sWrite.Count; j++)
            {
                bWrite.Add(Convert.ToByte(sWrite[j], 16));
            }
            byte[] myWrite = bWrite.ToArray();

            MyModbus modbus = new MyModbus();
            byte[] text = modbus.GetReadFrame(0x01, 0x10, 0x06, 0x05, 8);
            List<byte> bText = new List<byte>();
            int i = 0;
            for (int j = 0; j < 6; j++)
            {
                bText.Add(text[i]);
                i++;
                if (i == 6)
                {
                    bText.Add(0x0A);
                    foreach (byte wb in myWrite)
                    {
                        bText.Add(wb);
                    }
                }
            }
            byte[] LED_Text = bText.ToArray();
            ushort crc = MyModbus.CRC16(LED_Text, 0, LED_Text.Length - 3);
            bText.Add(MyModbus.WORD_HI(crc));
            bText.Add(MyModbus.WORD_LO(crc));
            byte[] myText = bText.ToArray();
            port.Write(myText, 0, myText.Length);
            port.Write(";");
            port.Close();
        }

        public Data GetData(int id)
        {
            if (!port.IsOpen)
            {
                port.Open();
            }
            MyModbus modbus = new MyModbus();
            byte[] tByte = modbus.GetReadFrame((byte)0x01, (byte)0x03, (byte)0x02, (byte)0x01, 8);
            port.Write(tByte, 0, 8);
            port.Write(";");
            byte[] tRecvBytes = new byte[32];
            port.Read(tRecvBytes, 0, 32);
            string tRecvText = OnGetData(tRecvBytes, 2);
            UInt32 tData = Convert.ToUInt32(tRecvText, 16);

            byte[] lByte = modbus.GetReadFrame((byte)0x01, (byte)0x03, (byte)0x05, (byte)0x01, 8);
            port.Write(lByte, 0, 8);
            port.Write(";");
            byte[] lRecvBytes = new byte[32];
            port.Read(tRecvBytes, 0, 32);
            string lRecvText = OnGetData(tRecvBytes, 2);
            UInt32 lData = Convert.ToUInt32(lRecvText, 16);

            double t = (double)tData;
            temperature = (1.0 / (1.0 / (25 + 273.15) + 1.0 / 3435.0 * (Math.Log(1024.0 / t) - 1.0)) - 273.15);
            light = (double)lData;
            id++;

            Data data = new Data(id, "COM3", 9600, temperature, light, 0, 0, 0, 0, 0, "", "");
            return data;
        }

        private string OnGetData(byte[] buffer, int count)
        {
            string result = "";
            for (int i = 0; i < count; i++)
            {
                byte[] tmpByte = new byte[1];
                tmpByte[0] = buffer[i + 3];
                result += BitConverter.ToString(tmpByte).Replace('-', '\0');
            }
            return result;
        }
    }
}

using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace modbus
{

    public enum DataType
    {
        _INT16 = 1,
        _FLOAT = 2
    }

    public class Modbus
    {
        internal bool isProcessState = false;

        internal SerialPort sp;
        public string modbusStatus;

        #region Constructor / Deconstructor
        public Modbus(SerialPort serialport)
        {
            this.sp = serialport;
        }

        ~Modbus()
        {
        }
        #endregion

        #region CRC Computation
        private void GetCRC(byte[] message, ref byte[] CRC)
        {
            //Function expects a modbus message of any length as well as a 2 byte CRC array in which to 
            //return the CRC values:

            ushort CRCFull = 0xFFFF;
            byte CRCHigh = 0xFF, CRCLow = 0xFF;
            char CRCLSB;

            for (int i = 0; i < (message.Length) - 2; i++)
            {
                CRCFull = (ushort)(CRCFull ^ message[i]);

                for (int j = 0; j < 8; j++)
                {
                    CRCLSB = (char)(CRCFull & 0x0001);
                    CRCFull = (ushort)((CRCFull >> 1) & 0x7FFF);

                    if (CRCLSB == 1)
                        CRCFull = (ushort)(CRCFull ^ 0xA001);
                }
            }

            CRC[1] = CRCHigh = (byte)((CRCFull >> 8) & 0xFF);
            CRC[0] = CRCLow = (byte)(CRCFull & 0xFF);
        }
        #endregion

        #region Build Message
        private void BuildMessage(byte address, byte type, ushort start, ushort registers, ref byte[] message)
        {
            //Array to receive CRC bytes:
            byte[] CRC = new byte[2];

            message[0] = address;
            message[1] = type;
            message[2] = (byte)(start >> 8);
            message[3] = (byte)start;
            message[4] = (byte)(registers >> 8);
            message[5] = (byte)registers;

            GetCRC(message, ref CRC);
            message[message.Length - 2] = CRC[0];
            message[message.Length - 1] = CRC[1];

            //Console.WriteLine("[{0}][{1}][{2}][{3}][{4}][{5}][{6}][{7}]", message[0], message[1], message[2], message[3], message[4], message[5], message[message.Length - 2], message[message.Length - 1]);
        }
        #endregion

        #region Check Response
        private bool CheckResponse(byte[] response)
        {
            //Perform a basic CRC check:
            byte[] CRC = new byte[2];
            GetCRC(response, ref CRC);

            if (CRC[0] == response[response.Length - 2] && CRC[1] == response[response.Length - 1])
                return true;
            else
                return false;
        }
        #endregion

        #region Get Response
        private void GetResponse(ref byte[] response)
        {
            //There is a bug in .Net 2.0 DataReceived Event that prevents people from using this
            //event as an interrupt to handle data (it doesn't fire all of the time). Therefore
            //we have to use the ReadByte command for a fixed length as it's been shown to be reliable.

            for (int i = 0; i < response.Length; i++)
            {
                response[i] = (byte)(sp.ReadByte());
            }

        }
        #endregion

        #region Function 16 - Write Multiple Registers
        public bool SendFc16(byte address, ushort start, ushort registers, short[] values, int timeOut)
        {
            //isProcessState = true;

            //Ensure port is open:
            if (sp.IsOpen)
            {
                //Clear in/out buffers:
                sp.DiscardOutBuffer();
                sp.DiscardInBuffer();

                sp.WriteTimeout = timeOut;

                //Message is 1 addr + 1 fcn + 2 start + 2 reg + 1 count + 2 * reg vals + 2 CRC
                byte[] message = new byte[9 + 2 * registers];
                //Function 16 response is fixed at 8 bytes
                byte[] response = new byte[8];

                //Add bytecount to message:
                message[6] = (byte)(registers * 2);
                //Put write values into message prior to sending:
                for (int i = 0; i < registers; i++)
                {
                    message[7 + 2 * i] = (byte)(values[i] >> 8);
                    message[8 + 2 * i] = (byte)(values[i]);
                }
                //Build outgoing message:
                BuildMessage(address, (byte)16, start, registers, ref message);

                //Send Modbus message to Serial Port:
                try
                {
                    sp.Write(message, 0, message.Length);
                    GetResponse(ref response);
                }
                catch (Exception err)
                {
                    Console.WriteLine("modbus-SendFc16 : {0}", err.Message);
                    return false;
                }
                //Evaluate message:
                if (CheckResponse(response))
                {
                    Console.WriteLine("Write successful");
                    //modbusStatus = "Write successful";
                    return true;
                }
                else
                {
                    Console.WriteLine("CRC error");
                    //modbusStatus = "CRC error";
                    return false;
                }
            }
            else
            {
                Console.WriteLine("Serial port not open");
                //modbusStatus = "Serial port not open";
                return false;
            }
        }

        public bool SetParams(byte SlaveAddress, int Offset, int Quantity, byte[] Bytes, int timeOut)
        {
            try
            {
                byte slave = SlaveAddress;
                byte function = (byte)16;

                byte[] response = new byte[8];
                sp.WriteTimeout = timeOut;

                List<byte> ByteToWrite = new List<byte>();

                ByteToWrite.Add(SlaveAddress);
                ByteToWrite.Add((byte)16);
                ByteToWrite.Add(Mathematic.MSB(Offset - 1));
                ByteToWrite.Add(Mathematic.LSB(Offset - 1));
                ByteToWrite.Add(Mathematic.MSB(Quantity));
                ByteToWrite.Add(Mathematic.LSB(Quantity));
                ByteToWrite.Add((byte)Bytes.Length);
                ByteToWrite.AddRange(Bytes);


                int CRC = Mathematic.CalculateCRC(ByteToWrite.ToArray(), ByteToWrite.Count);

                ByteToWrite.Add((byte)(CRC & 0x00FF));
                ByteToWrite.Add((byte)((CRC & 0xFF00) >> 8));

                //[01][10][00][02][00][08][10][CC][CD][3E][4C][00][00][40][00][00][00][40][00][00][00][00][00]

                for (int i = 0; i < 2; ++i)
                {
                    byte _b = WriteModbusFunction(ByteToWrite.ToArray(), ref response, slave, function);
                    if (_b == 1)
                        break;
                    else
                    {
                        if (i == 1)
                            return false;
                    }
                }

                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine("modbus-SendFc16 : {0}", ex.Message);
                return false;
            }
        }

        private byte WriteModbusFunction(byte[] byteData, ref byte[] response, byte slaveID, byte function)
        {
            sp.Write(byteData, 0, byteData.Length);
            GetResponse(ref response);

            if (CheckResponse(response))
            {
                //modbusStatus = "Write successful";
                if (response[0] == slaveID)
                {
                    if (response[1] == function)
                        return 1;
                    else
                        return 2;
                }
                else
                    return 3;
            }
            else
                return 4;
        }

        #endregion

        #region Function 3 - Read Registers
        public bool SendFc3(byte address, ushort start, ushort registers, DataType datatype, ref short[] values, int timeOut)
        {
            int _bayt = (int)datatype;
            isProcessState = true;

            //Ensure port is open:
            if (sp.IsOpen)
            {
                //Clear in/out buffers:
                sp.DiscardOutBuffer();
                sp.DiscardInBuffer();

                sp.ReadTimeout = timeOut;

                //Function 3 request is always 8 bytes:
                byte[] message = new byte[8];
                //Function 3 response buffer:
                byte[] response = new byte[5 + 2 * registers];
                //Build outgoing modbus message:
                BuildMessage(address, (byte)3, start, registers, ref message);
                //Send modbus message to Serial Port:
                try
                {
                    sp.Write(message, 0, message.Length);
                    GetResponse(ref response);
                }
                catch (Exception err)
                {
                    Console.WriteLine("modbus-SendFc3 : {0}", err.Message);
                    isProcessState = false;
                    //modbusStatus = "Error in read event: " + err.Message;
                    return false;
                }

                //Evaluate message:
                if (CheckResponse(response))
                {
                    //Return requested register values:
                    for (int i = 0; i < (response.Length - 5) / 2; i++)
                    {
                        //Console.WriteLine(response[2 * i + 3]);
                        values[i] = response[2 * i + 3];
                        values[i] <<= 8;
                        values[i] += response[2 * i + 4];
                    }
                    Console.WriteLine("modbus-SendFc3 : Read successful");
                    //modbusStatus = "Read successful";

                    return true;
                }
                else
                {
                    Console.WriteLine("modbus-SendFc3 : CRC error");
                    isProcessState = false;
                    //modbusStatus = "CRC error";
                    return false;
                }
            }
            else
            {
                Console.WriteLine("modbus-SendFc3 : Serial port not open");
                isProcessState = false;
                //modbusStatus = "Serial port not open";
                return false;
            }



        }
        #endregion

    }
}

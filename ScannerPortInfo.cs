using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace modbus
{
    public class ScannerPortInfo
    {
        private SerialPort m_serialPort;
        private Modbus _modbus;


        int m_dataBit = 3;
        int m_baudrate = 5;

        //PortCluster m_processCluster;

        private Label m_label;
        private Func<bool> m_ProcessEvent;

        private short[] m_ComInfo;

        public ScannerPortInfo(string portname, Func<bool> ProcessEvent, Label label)
        {
            PortOpen(portname);

            m_label = label;
            m_ProcessEvent = ProcessEvent;
        }

        ~ScannerPortInfo()
        {
            try
            {
                if (m_serialPort.IsOpen)
                {
                    m_serialPort.Close();
                }
            }
            catch (Exception)
            {
            }
        }

        private void PortOpen(string _portName)
        {
            try
            {
                m_serialPort = new SerialPort();
                m_serialPort.PortName = _portName;
                m_serialPort.Open();

                if (m_serialPort.IsOpen)
                {

                    m_serialPort.BaudRate = DefaultSetting.GBaudRate;
                    m_serialPort.DataBits = DefaultSetting.Gdatabits;
                    m_serialPort.StopBits = (StopBits)1;
                    m_serialPort.Parity = (Parity)0;

                    //m_processCluster = new PortCluster();

                    //TaskStart(m_processCluster);
                }
            }
            catch (Exception)
            {
            }
        }

        private void PortClose()
        {
            try
            {
                if (m_serialPort.IsOpen)
                {
                    m_serialPort.Close();
                }
            }
            catch (Exception)
            {

            }
        }

        private void ComVariableReset()
        {
            m_ComInfo = null;
            m_ComInfo = new short[10];
        }

        public void Start()
        {
            TaskStart(/*m_processCluster*/);

            ComVariableReset();

            if (!DefaultScanner())
                FullScanner();

            if (!_modbus.isProcessState)
                isDefaultProcessState();

            m_ProcessEvent.Invoke();
        }

        private void TaskStart(/*PortCluster PC*/)
        {
            _modbus = new Modbus(m_serialPort);

            //new Thread(() => PC.ProcessStart(_modbus)).Start();
            //await Task.Run(() => PC.ProcessStart(_modbus));
        }

        private bool DefaultScanner()
        {

            _modbus.SendFc3(DefaultSetting.GSlaveID, 175, 10, DataType._FLOAT, ref m_ComInfo, 1000);

            if (_modbus.isProcessState)
            {
                isDefaultProcessState();

                m_ProcessEvent.Invoke();

                return true;
            }

            return false;
        }

        private void FullScanner()
        {
            _modbus = new Modbus(m_serialPort);
            m_serialPort.ReadTimeout = 500;

            m_serialPort.DataBits = 8;
            for (int baud = 0; baud < GlobalInfo.GetBauds.Length; baud++)
            {   
                m_serialPort.BaudRate = (int)GlobalInfo.GetBauds[baud];
                //m_serialPort.DataBits = (int)GlobalInfo.GetDataBit[bits];

                for (int forParity = 0; forParity < GlobalInfo.GetParity.Length; forParity++)
                {
                    m_serialPort.Parity = GlobalInfo.GetParity[forParity];
                    for (int forStopBits = 0; forStopBits < GlobalInfo.GetStopBits.Length; forStopBits++)
                    {
                        m_serialPort.StopBits = GlobalInfo.GetStopBits[forStopBits];

                        ComVariableReset();

                        _modbus.sp = m_serialPort;

                        LabelWrite(baud, 3, GlobalInfo.GetStopBits[forStopBits].ToString(), GlobalInfo.GetParity[forParity].ToString());

                        _modbus.SendFc3(DefaultSetting.GSlaveID, 175, 10, DataType._FLOAT, ref m_ComInfo, 250);

                        if (_modbus.isProcessState)
                        {
                            isFullProcessState();
                            return;
                        }
                    }
                }
            }
        }

        private void LabelWrite(int bauds, int bit, string _stopbit, string _parity)
        {
            string text = "Baudrate : " + (int)GlobalInfo.GetBauds[bauds] + " DataBits : " + (int)GlobalInfo.GetDataBit[bit] + " Parity : " + _parity + " StopBits : " + _stopbit;

            m_dataBit = bit;
            m_baudrate = bauds;
            //Console.WriteLine(text);

            if (m_label.InvokeRequired)
            {
                m_label.Invoke(new Action(() =>
                {
                    m_label.Text = text;
                }));
            }
            else
            {
                m_label.Text = text;
            }
        }

        private bool isDefaultProcessState()
        {
            //if (_modbus.isProcessState)
            //{
            //m_processCluster.ProcessStop();
            //m_processCluster = null;


            GlobalInfo.DefaultToGlobal(ToFloat(m_ComInfo, 0), m_serialPort.PortName);
            //Console.WriteLine("GlobalSlaveID : {0}", GlobalInfo.SlaveID);

            PortClose();


            m_ProcessEvent.Invoke();
            //}

            return _modbus.isProcessState;
        }

        private bool isFullProcessState()
        {
            //m_processCluster.ProcessStop();
            //m_processCluster = null;

            GlobalInfo.PortName = m_serialPort.PortName;
            GlobalInfo.SlaveID = (byte)ToFloat(m_ComInfo, 0);
            GlobalInfo.BaudRate = m_baudrate; //(int)ToFloat(m_ComInfo, 1);
            GlobalInfo.DataBits = m_dataBit; //(int)ToFloat(m_ComInfo, 2);

            PortClose();

            m_ProcessEvent.Invoke();


            return _modbus.isProcessState;
        }

        private bool isProcessEnded()
        {


            return true;
        }

        private float ToFloat(short[] value, int i)
        {
            int intValue = (int)value[2 * i];
            intValue <<= 16;
            intValue += (int)value[2 * i + 1];

            return BitConverter.ToSingle(BitConverter.GetBytes(intValue), 0);
        }
    }
}

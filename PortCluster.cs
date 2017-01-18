using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO.Ports;
using System.Linq;
using System.Security.Permissions;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace modbus
{
    public class PortCluster
    {
        /// <summary>
        /// Port durumu.
        /// </summary>
        public bool PortIsOpen { get; private set; }

        private static SerialPort m_SerialPort;
        internal static SerialPort GetSerialPort
        {
            get { return m_SerialPort; }
        }

        public PortCluster()
        {

        }

        public PortCluster(string portName, int baudRate, int databits, Parity parity, StopBits stopBits)
        {

            m_SerialPort = new SerialPort();
            m_SerialPort.PortName = portName;
            m_SerialPort.BaudRate = baudRate;
            m_SerialPort.DataBits = databits;
            m_SerialPort.Parity = parity;

            //Console.WriteLine("Port oluşturuldu.");

            PortIsOpen = PortOpen();
        }

        private bool PortOpen()
        {
            try
            {
                if (!m_SerialPort.IsOpen)
                {
                    m_SerialPort.Open();
                }

                return true;
            }
            catch (Exception)
            {
                PortClose();
                return false;
            }
        }

        public bool PortClose()
        {
            try
            {
                if (m_SerialPort.IsOpen)
                {
                    m_SerialPort.Close();
                }

                m_SerialPort = null;

                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        private Modbus ModbusSettingChange(Modbus _modbus)
        {
            Modbus temp = _modbus;
            try
            {
                _modbus.sp.BaudRate = (int)GlobalInfo.GetBauds[GlobalInfo.BaudRate];
                _modbus.sp.DataBits = (int)GlobalInfo.GetDataBit[GlobalInfo.DataBits];

                return _modbus;
            }
            catch (Exception)
            {
                return temp;
            }
        }

        /// <summary>
        /// Modbus okuma/yazma işlemlerinin başlatılması.
        /// </summary>
        /// <param name="modbus">İşlem yapılacak modbus sınıfı.</param>
        /// <param name="scan">Scanner yapılıp yapılmadığı. Default FALSE yani scan pasif.</param>
        public void ProcessStart(Modbus modbus, bool scan = false)
        {
            bool txState = false;
            try
            {
                GlobalInfo.ProcessState = true;

                if (modbus == null)
                    modbus = new Modbus(GetSerialPort);

                Console.WriteLine("System start...");

                while (GlobalInfo.ProcessState)
                {
                    txState = false;
                    if (!scan)
                    {
                        modbus.sp.BaudRate = (int)GlobalInfo.GetBauds[GlobalInfo.BaudRate];
                        modbus.sp.DataBits = (int)GlobalInfo.GetDataBit[GlobalInfo.DataBits];
                        modbus.sp.StopBits = GlobalInfo.stopBits;
                        modbus.sp.Parity = GlobalInfo.parity;
                    }

                    if (ProcessCluster.GetHighPriorityProcess != null)
                    {
                        foreach (ProcessInfo PI in ProcessCluster.GetHighPriorityProcess[0].Item1)
                        {
                            ReceiveTransmitProcess(ReceiveTransmit.RX, true);
                            if (PI.state == ProcessState.READ)
                            {
                                txState = modbus.SendFc3(PI.SlaveID, PI.PollStart, PI.PollLength, PI.dataType, ref PI.Values, 2000);
                            }
                            else
                            {
                                txState = modbus.SetParams(PI.SlaveID, PI.PollStart, PI.Bytes.Length / 2, PI.Bytes, 2000);
                                if (!txState)
                                {
                                    if (GlobalControl.StatusLabel != null)
                                        GlobalControl.StatusLabel.Text = "Error Write";
                                }
                                Thread.Sleep(150);
                                //modbus.SendFc16(PI.SlaveID, 40083, (ushort)1, PI.Values, 2000);
                            }
                            ReceiveTransmitProcess(ReceiveTransmit.RX, false);
                        }
                        ReceiveTransmitProcess(ReceiveTransmit.TX, txState);

                        Func<bool> function = ProcessCluster.GetHighPriorityProcess[0].Item2;
                        ProcessCluster.SetHighPriorityProcess = null;

                        function.Invoke();
                    }
                    else if (ProcessCluster.GetLowPriorityProcess != null)
                    {
                        foreach (ProcessInfo PI in ProcessCluster.GetLowPriorityProcess[0].Item1)
                        {
                            ReceiveTransmitProcess(ReceiveTransmit.RX, true);
                            if (PI.state == ProcessState.READ)
                            {
                                txState = modbus.SendFc3(PI.SlaveID, PI.PollStart, PI.PollLength, PI.dataType, ref PI.Values, 2000);
                            }
                            else
                            {
                                txState = modbus.SetParams(PI.SlaveID, PI.PollStart, PI.Bytes.Length / 2, PI.Bytes, 2000);
                                //Thread.Sleep(150);
                            }
                            ReceiveTransmitProcess(ReceiveTransmit.RX, false);
                        }
                        ReceiveTransmitProcess(ReceiveTransmit.TX, txState);

                        Func<bool> function = ProcessCluster.GetLowPriorityProcess[0].Item2;
                        ProcessCluster.SetLowPriorityProcess = null;

                        function.Invoke();
                    }
                    else
                    {
                        //Console.WriteLine("SlaveID : {0}", 0);
                    }

                    Thread.Sleep(250);
                    ReceiveTransmitProcess(ReceiveTransmit.TX, false);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("PortCluster-ProcessStart : {0}", ex.Message);
            }
        }

        public void ProcessStop()
        {
            GlobalInfo.ProcessState = false;
        }

        private void ReceiveTransmitProcess(ReceiveTransmit rt, bool process)
        {
            if (GlobalFunctions.FunctionRX != null && GlobalFunctions.FunctionTX != null)
            {
                if (rt == ReceiveTransmit.RX)
                {
                    GlobalFunctions.FunctionRX(process);
                }
                else
                {
                    GlobalFunctions.FunctionTX(process);
                }
            }
        }

    }
}

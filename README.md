# modbus
Simple communication

use of.

<h3>STEP 1:</h3>

        ```C#
        private void Dashboard_Load(object sender, EventArgs e)
        {
            initProcessCluster();
            
            TimerDisplay = new System.Windows.Forms.Timer();
            TimerDisplay.Tick += TimerDisplay_Tick;
            TimerDisplay.Interval = 5000;
            
            TimerDisplay.Start();

            GlobalFunctions.FunctionRX = this.RX;
            GlobalFunctions.FunctionTX = this.TX;
        }
        ```

<h3>STEP 2:</h3>

```C#
        private void initProcessCluster()
        {
            int _baud;
            int _bit;

            if (GlobalInfo.BaudRate == -1 && GlobalInfo.DataBits == -1)
            {
                _baud = 19200;
                _bit = 8;
            }
            else
            {
                _baud = (int)GlobalInfo.GetBauds[GlobalInfo.BaudRate];
                _bit = (int)GlobalInfo.GetDataBit[GlobalInfo.DataBits];
            }

            m_processCluster = new PortCluster(GlobalInfo.PortName, _baud, _bit, GlobalInfo.parity, GlobalInfo.stopBits);

            if (m_processCluster.PortIsOpen)
            {
                PortStart(m_processCluster);
            }
            else
            {
                //...Mesaj verilecek.
            }
        }
        ```

<h3>STEP 3:</h3>

```C#
        public async void PortStart(PortCluster PC)
        {
            await Task.Run(() => PC.ProcessStart(null));
        }
        ```
        
<h3>STEP 4:</h3>

```C#
        public short[] DashboardValues = new short[18];
        private void TimerDisplay_Tick(object sender, EventArgs e)
        {
            initProcessRefresh();
        }

        private void initProcessRefresh()
        {
            try
            {
                Tuple<ProcessInfo[], Func<bool>> tuple = new Tuple<ProcessInfo[], Func<bool>>(
               new ProcessInfo[] {
                                new ProcessInfo() { SlaveID = GlobalInfo.SlaveID, PollStart= 2051, PollLength = 18, Values = DashboardValues, state = ProcessState.READ },
                               }, ObjectsValueSet);

                ProcessCluster.SetLowPriorityProcess = tuple;
            }
            catch (Exception)
            { }
        }
        ```
        
 <h3>STEP 5:</h3>
 
 ```C#
        private bool ObjectsValueSet()
        {
            return DisplayProcess();
        }
        
        private bool DisplayProcess()
        {
            try
            {
                for (int i = 0; i < (18 / 2); i++)
                {

                    int intValue = GetValue(i, DashboardValues);

                    string val;
                    if (i < 4)
                    {
                        string format = GetFormatDisplay(i);
                        val = (BitConverter.ToSingle(BitConverter.GetBytes(intValue), 0)).ToString(format).Replace(',', '.');
                    }
                    else
                        val = (BitConverter.ToSingle(BitConverter.GetBytes(intValue), 0)).ToString();

                    //Console.WriteLine("{0} => {1}", i, val);

                    SetValue(i, val);
                }

                //string[] LogValues = new string[4];

                return true;
            }
            catch (Exception)
            {
                return false;
            }
            finally
            {
                TX(false);
            }
        }
        
        private int GetValue(int i, short[] values)
        {
            int intValue = (int)values[2 * i];
            intValue <<= 16;
            intValue += (int)values[2 * i + 1];

            return intValue;
        }
        ```

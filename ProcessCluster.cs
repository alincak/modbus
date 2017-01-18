using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace modbus
{
    public enum ProcessPriority
    {
        High = 0,
        Low = 1
    }

    public enum ProcessState
    {
        READ = 0,
        WRITE = 1
    }

    public static class ProcessCluster
    {
        private static List<Tuple<ProcessInfo[], Func<bool>>> m_HighPriorityProcess;
        private static List<Tuple<ProcessInfo[], Func<bool>>> m_LowPriorityProcess;

        internal static List<Tuple<ProcessInfo[], Func<bool>>> GetHighPriorityProcess
        {
            get { return m_HighPriorityProcess; }
        }

        internal static List<Tuple<ProcessInfo[], Func<bool>>> GetLowPriorityProcess
        {
            get { return m_LowPriorityProcess; }
        }

        public static Tuple<ProcessInfo[], Func<bool>> SetHighPriorityProcess
        {
            set
            {
                //Console.WriteLine(m_HighPriorityProcess);
                if (value == null)
                {
                    m_HighPriorityProcess = null;
                }
                else
                {
                    if (m_HighPriorityProcess == null)
                    {
                        m_HighPriorityProcess = new List<Tuple<ProcessInfo[], Func<bool>>>();
                        m_HighPriorityProcess.Add(value);
                    }
                }
            }
        }

        public static bool GetStateHighPriorityProcess
        {
            get
            {
                if (m_HighPriorityProcess == null)
                    return false;
                else
                    return true;
            }
        }

        public static Tuple<ProcessInfo[], Func<bool>> SetLowPriorityProcess
        {
            set
            {
                if (value == null)
                {
                    m_LowPriorityProcess = null;
                }
                else
                {
                    if (m_LowPriorityProcess == null)
                    {
                        m_LowPriorityProcess = new List<Tuple<ProcessInfo[], Func<bool>>>();
                        m_LowPriorityProcess.Add(value);
                    }
                }
            }
        }
    }

    public class ProcessInfo
    {
        public byte SlaveID;
        public ushort PollStart;
        public ushort PollLength;
        public short[] Values;
        public DataType dataType;
        public byte[] Bytes;
        public ProcessState state;
    }
}

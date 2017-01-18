using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO.Ports;
using System.Windows.Forms;

namespace modbus
{
    /// <summary>
    /// RX ve TX tercihleri.
    /// </summary>
    enum ReceiveTransmit
    {
        RX,
        TX
    }

    /// <summary>
    /// Uygulama içinde kullanılan temel değişkenler burada barındırılır.
    /// </summary>
    public static class GlobalInfo
    {

        public static byte tempDP = 2;
        public static byte NTCtempDP = 2;
        public static byte HumidityDP = 2;

        /// <summary>
        /// Modbus okuma/yazma process kontrolü. TRUE aktif, FALSE pasif.
        /// </summary>
        public static bool ProcessState = true;

        private static object[] m_Bauds = { 1200, 2400, 4800, 9600, 14400, 19200, 28800, 38400, 56000, 57600, 115200, 128000, 256000, 460800, 921600 };
        private static object[] m_DataBit = { 5, 6, 7, 8 };
        private static StopBits[] m_ArrayStopBits = { StopBits.One, StopBits.Two };
        private static Parity[] m_ArrayParity = { Parity.None, Parity.Odd, Parity.Even };

        private static string m_PortName;
        private static int m_BaudRate = -1;
        private static int m_databits = -1;
        private static Parity m_parity = Parity.None;
        private static StopBits m_stopBits = StopBits.One;

        private static byte m_slaveID = 0;

        /// <summary>
        /// Baudrade listesini verir.
        /// </summary>
        public static object[] GetBauds
        {
            get { return m_Bauds; }
        }

        /// <summary>
        /// DataBits listesini verir.
        /// </summary>
        public static object[] GetDataBit
        {
            get { return m_DataBit; }
        }

        /// <summary>
        /// StopBits listesini verir.
        /// </summary>
        public static StopBits[] GetStopBits
        {
            get { return m_ArrayStopBits; }
        }

        /// <summary>
        /// Parity listesini verir.
        /// </summary>
        public static Parity[] GetParity
        {
            get { return m_ArrayParity; }
        }

        public static string PortName
        {
            get { return m_PortName; }
            set { m_PortName = value; }
        }

        public static int BaudRate
        {
            get { return m_BaudRate; }
            set { m_BaudRate = value; }
        }

        public static int DataBits
        {
            get { return m_databits; }
            set { m_databits = value; }
        }

        public static Parity parity
        {
            get { return m_parity; }
            set { m_parity = value; }
        }

        public static StopBits stopBits
        {
            get { return m_stopBits; }
            set { m_stopBits = value; }
        }

        public static byte SlaveID
        {
            get { return m_slaveID; }
            set { m_slaveID = value; }
        }

        /// <summary>
        /// Scanner ederken, default değerler uygun ise bu değerler Global değişkenlere atılır.
        /// </summary>
        /// <param name="id">Scanner zamanında sabit bir adres ile tarama yapılır. Değerlere ulaşıldığında asıl adres alınır. Adresi temsil eder.</param>
        /// <param name="portname">Bağımlı olduğu com'un adı.</param>
        public static void DefaultToGlobal(float id, string portname)
        {
            PortName = portname;
            BaudRate = DefaultSetting.GBaudRate;
            SlaveID = (byte)id;
            DataBits = DefaultSetting.Gdatabits;
            parity = DefaultSetting.Gparity;
            stopBits = DefaultSetting.GstopBits;
        }
    }

    /// <summary>
    /// İlk adımda en çok kullanılan, sabit olması olası değerler buradadır.
    /// </summary>
    public class DefaultSetting
    {
        public static byte GSlaveID = 253;
        public static int GBaudRate = 5;
        public static int Gdatabits = 3;
        public static Parity Gparity = Parity.None;
        public static StopBits GstopBits = StopBits.One;
    }

    /// <summary>
    /// Global fonksiyonlar. Yani uygulamanın her hangi bir noktasında invoke edebilme durumu için.
    /// </summary>
    public class GlobalFunctions
    {
        /// <summary>
        /// Bu fonksiyon splash form da ki, lable'ı kontrol etmek için kullanılır.
        /// </summary>
        public static Func<bool, string> FunctionSplashLable = null;

        /// <summary>
        /// Bu fonksiyon setting alanında dinamik değişilik sonrasında, eğer yapılan değişiklik kayıt edilmez ise her ihtimale karşı settting formu kapandığında çalışır.
        /// </summary>
        public static Func<bool> FunctionDashBoardDP = null;


        /// <summary>
        /// RX için kullanılacak fonksiyon referansıdır. Fonksiyon imzası, bool dönüş ve bool parametresi tipidir.
        /// </summary>
        public static Func<bool, bool> FunctionRX = null;

        /// <summary>
        /// TX için kullanılacak fonksiyon referansıdır. Fonksiyon imzası, bool dönüş ve bool parametresi tipidir.
        /// </summary>
        public static Func<bool, bool> FunctionTX = null;

        /// <summary>
        /// Verilen string değeri T tipi Enum'a dönüştürür.
        /// </summary>
        /// <typeparam name="T">Dönüştürülecek Enum tip.</typeparam>
        /// <param name="value">Enum içinde ki değerin string değeri.</param>
        /// <returns>Verilen değiri istenilen Enum tipi ile döner.</returns>
        public static T ParseEnum<T>(string value)
        {
            return (T)Enum.Parse(typeof(T), value, true);
        }
    }

    public class GlobalControl
    {
        public static ToolStripStatusLabel StatusLabel = null;
    }
}

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading;
using System.Net.Sockets;
using System.Net;
using System.IO;

namespace ServisZaMonitoring
{
    public partial class EnMonitoring : ServiceBase
    {
        Timer tm;
        static TcpClient tcpclnt = new TcpClient();
        static string sSource;
        static string sLog;
        static string sEvent; //spremenljivke za pisati napake v enentlo
        //static StreamWriter sw;
        static ElektrikaEntities ent = new ElektrikaEntities();
        public EnMonitoring()
        {
            InitializeComponent();
        }

        protected override void OnStart(string[] args)
        {
            tcpclnt.Connect("193.2.139.235", 5333);
            tm = new Timer(new TimerCallback(VsakTik), null, 0, 60000); //na 1 minuto
        }

        protected override void OnStop()
        {
        }
        
        //v metodi Receive damo zadnji parameter še fazo, ki prihaja
        static void VsakTik(object state)
        {     
            
                //sw = new StreamWriter("C:\\barbara\\Podatki.txt", true);
                //prva faza
                Socket socket = tcpclnt.Client;
                byte[] vprašanje = new byte[] { 0x01, 0x04, 0x00, 0x00, 0x00, 0x06, 0x70, 0x08 };
                try
                {
                   
                    Send(socket, vprašanje, 0, vprašanje.Length, 10000);
                }
                catch (Exception e)
               {
                sSource = "Energetski monitring";
                sLog = "Application";
                sEvent = e.Message;
                if (!EventLog.SourceExists(sSource))
                    EventLog.CreateEventSource(sSource, sLog);
                EventLog.WriteEntry(sSource, sEvent, EventLogEntryType.Error, 234);
                }
                Socket socket1 = tcpclnt.Client;
                byte[] bb = new byte[25];  // length of the text "Hello world!"
                try
                { 
                    Receive(socket1, bb, 0, 17, 5000,1);
                }
                catch (Exception ex) 
                {
                    sSource = "Energetski monitring";
                    sLog = "Application";
                    sEvent = ex.Message;
                    if (!EventLog.SourceExists(sSource))
                        EventLog.CreateEventSource(sSource, sLog);
                    EventLog.WriteEntry(sSource, sEvent, EventLogEntryType.Error, 234);
                }
                //druga faza
                Socket socket2 = tcpclnt.Client;
                byte[] vprašanje1 = new byte[] { 0x01, 0x04, 0x00, 0x0a, 0x00, 0x06, 0x50, 0x0a };
                try
                {
                    
                    Send(socket2, vprašanje1, 0, vprašanje1.Length, 10000);
                }
                catch (Exception ex) 
                {
                    sSource = "Energetski monitring";
                    sLog = "Application";
                    sEvent = ex.Message;
                    if (!EventLog.SourceExists(sSource))
                        EventLog.CreateEventSource(sSource, sLog);
                    EventLog.WriteEntry(sSource, sEvent, EventLogEntryType.Error, 234); 
                }
               
                Socket socket3 = tcpclnt.Client;
                byte[] bb1 = new byte[25];  
                try
                { 
                    Receive(socket3, bb1, 0, 17, 5000,2);
                }
                catch (Exception ex) 
                {
                    sSource = "Energetski monitring";
                    sLog = "Application";
                    sEvent = ex.Message;
                    if (!EventLog.SourceExists(sSource))
                        EventLog.CreateEventSource(sSource, sLog);
                    EventLog.WriteEntry(sSource, sEvent, EventLogEntryType.Error, 234);
                }
               //tretja faza
                Socket socket4 = tcpclnt.Client;
                byte[] vprašanje2 = new byte[] { 0x01, 0x04, 0x00, 0x14, 0x00, 0x06, 0x30, 0x0c };
                try
                {
                    
                    Send(socket4, vprašanje2, 0, vprašanje2.Length, 10000);
                }
                catch (Exception ex) 
                {
                    sSource = "Energetski monitring";
                    sLog = "Application";
                    sEvent = ex.Message;
                    if (!EventLog.SourceExists(sSource))
                        EventLog.CreateEventSource(sSource, sLog);
                    EventLog.WriteEntry(sSource, sEvent, EventLogEntryType.Error, 234);
                }
                
                Socket socket5 = tcpclnt.Client;
                byte[] bb2 = new byte[25];  
                try
                { 
                    Receive(socket5, bb2, 0, 17, 5000,3);
                }
                catch (Exception ex) 
                {
                    sSource = "Energetski monitring";
                    sLog = "Application";
                    sEvent = ex.Message;
                    if (!EventLog.SourceExists(sSource))
                        EventLog.CreateEventSource(sSource, sLog);
                    EventLog.WriteEntry(sSource, sEvent, EventLogEntryType.Error, 234);
                }
                //sw.Close();
                
        }
        public static void Send(Socket socket, byte[] buffer, int offset, int size, int timeout)
        {
            int startTickCount = Environment.TickCount;
            int sent = 0;  
            do
            {
                if (Environment.TickCount > startTickCount + timeout)
                    throw new Exception("Timeout.");
                try
                {
                    sent += socket.Send(buffer, offset + sent, size - sent, SocketFlags.None);
                }
                catch (SocketException ex)
                {
                    if (ex.SocketErrorCode == SocketError.WouldBlock ||
                        ex.SocketErrorCode == SocketError.IOPending ||
                        ex.SocketErrorCode == SocketError.NoBufferSpaceAvailable)
                    {
                        // socket buffer is probably full, wait and try again
                        Thread.Sleep(30);
                    }
                    else
                        throw ex;  // any serious error occurr
                }
            } while (sent < size);
        }
        public static void Receive(Socket socket, byte[] buffer, int offset, int size, int timeout,int faza)
        {
            int startTickCount = Environment.TickCount;
            int received = 0;  // how many bytes is already received
            do
            {

                try
                {
                    received += socket.Receive(buffer, offset + received, size - received, SocketFlags.None);
                }
                catch (SocketException ex)
                {
                    if (ex.SocketErrorCode == SocketError.WouldBlock ||
                        ex.SocketErrorCode == SocketError.IOPending ||
                        ex.SocketErrorCode == SocketError.NoBufferSpaceAvailable)
                    {
                        // socket buffer is probably empty, wait and try again
                        Thread.Sleep(10);
                    }
                    else
                        throw ex;  // any serious error occurr
                }

            } while ((received < size) & (Environment.TickCount < startTickCount + timeout));
            double v = (buffer[6] + buffer[5] * Math.Pow(2, 8) + buffer[4] * Math.Pow(2, 16) + buffer[3] * Math.Pow(2, 24)) / 10.0;
            double a=(buffer[7] * Math.Pow(2, 24) + buffer[8] * Math.Pow(2, 16) + buffer[9] * Math.Pow(2, 8) + buffer[10]) / 1000.0;
            double w=(buffer[11] * Math.Pow(2, 24) + buffer[12] * Math.Pow(2, 16) + buffer[13] * Math.Pow(2, 8) + buffer[14]) / 1000.0;
            
            Meritve mer = new Meritve();
            if (faza == 1)
            {
                mer.A1 = (float)a;
                mer.V1 = (float)v;
                mer.kW1 = (float)w;
            }
            if (faza == 2)
            {
                mer.A2 = (float)a;
                mer.V2 = (float)v;
                mer.kW2 = (float)w;
            }
            if (faza == 3)
            {           
                  mer.A3 = (float)a;
                  mer.V3 = (float)v;
                  mer.kW3 = (float)w;
            }
            ent.Meritve.Add(mer);
            ent.SaveChanges();
            //sw.Write(DateTime.Now.ToLongTimeString() + " VL " + (buffer[6] + buffer[5] * Math.Pow(2, 8) + buffer[4] * Math.Pow(2, 16) + buffer[3] * Math.Pow(2, 24)) / 10.0 + " V " + "\t");
            //sw.Write(" AL " + (buffer[7] * Math.Pow(2, 24) + buffer[8] * Math.Pow(2, 16) + buffer[9] * Math.Pow(2, 8) + buffer[10]) / 1000.0 + " A " + "\t");
            //sw.WriteLine(" kW " + (buffer[11] * Math.Pow(2, 24) + buffer[12] * Math.Pow(2, 16) + buffer[13] * Math.Pow(2, 8) + buffer[14]) / 1000.0 + " kW ");
            byte[] zaCrc = new byte[15];
            for (int j = 0; j < 15; j++)
                zaCrc[j] = buffer[j];
            uint r = Crc16.ComputeCrc(zaCrc);
            byte[] re = new byte[2];
            re[0] = (byte)(r % 256);
            re[1] = (byte)(r / 256);
            if (re[0] != buffer[15] | re[1] != buffer[16])
            {
                sSource = "Energetski monitring";
                sLog = "Application";
                sEvent = "Napačen CRC "+DateTime.Now;
                if (!EventLog.SourceExists(sSource))
                    EventLog.CreateEventSource(sSource, sLog);
                EventLog.WriteEntry(sSource, sEvent, EventLogEntryType.Error, 234);
            }
        }
    }
}

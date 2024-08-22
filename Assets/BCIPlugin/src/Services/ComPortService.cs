using UnityEngine;
using System.Collections;
using System.IO.Ports;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Text;


public class ComPortService
{
    public string portName = "COM4";                //串口名
    public int baudRate = 9600;                     //波特率
    public Parity parity = Parity.None;             //效验位
    public int dataBits = 8;                        //数据位
    public StopBits stopBits = StopBits.One;        //停止位
    SerialPort sp = null;
    Thread dataReceiveThread;

    public List<byte> listReceive = new List<byte>();
    char[] strchar = new char[100];                 //接收的字符信息转换为字符数组信息
    string str;

    static ComPortService instance;

    public static ComPortService Instance
    {
        get
        {
            if (instance == null)
            {
                instance = new ComPortService();
            }
            return instance;
        }
    }

    public ComPortService()
    {
        OpenPort();
    }

    ~ComPortService()
    {
        ClosePort();
    }

    public void OpenPort()
    {
        //创建串口
        sp = new SerialPort(portName, baudRate, parity, dataBits, stopBits);
        sp.ReadTimeout = 1000;
        try
        {
            sp.Open();
        }
        catch (Exception ex)
        {
            Debug.Log(ex.Message);
        }
    }

    public void ClosePort()
    {
        try
        {
            sp.Close();
            dataReceiveThread.Abort();
        }
        catch (Exception ex)
        {
            Debug.Log(ex.Message);
        }
    }


    public void WriteData(string dataStr)
    {
        if (sp.IsOpen)
        {
            byte[] buffer = { 0x00 };
            //      string zero = "0";
            sp.Write(dataStr);
            //      System.Threading.Thread.Sleep(500);
            sp.Write(buffer, 0, buffer.Length);
        }
    }

}

using Microsoft.Azure.Devices.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IndustrialIoT
{
    public class Program
    {
        public static int deviceNumber;

        static async Task Main(string[] args)
        {
            OpcDevice opcDevice = new OpcDevice();
            opcDevice.Start();
            Console.WriteLine("choose device number:");
            deviceNumber = Convert.ToInt32(Console.ReadLine());
            string deviceConnectionString = File.ReadAllLines($"../../../../Config.txt")[1 + deviceNumber];
            Console.WriteLine(deviceConnectionString);
            using var deviceClient = DeviceClient.CreateFromConnectionString(deviceConnectionString, TransportType.Mqtt);
            var device = new IoTDevice(deviceClient);

            Console.WriteLine($"Connection success!");
            while (true)
            {
                device.SendMessages(OpcDevice.client, deviceNumber);
                Thread.Sleep(1000);
            }
        }
    }
}
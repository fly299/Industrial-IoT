using Microsoft.Azure.Devices.Client;
using Newtonsoft.Json;
using Opc.UaFx.Client;
using Opc.UaFx;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mime;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Azure.Devices.Shared;
using System.Net.Sockets;

namespace IndustrialIoT
{
    public class IoTDevice
    {
        private readonly DeviceClient client;

        public IoTDevice(DeviceClient deviceClient)
        {
            this.client = deviceClient;
        }

        #region Sending Messages

        public async Task SendMessages(OpcClient opcClient, int deviceNumber)
        {
            var data = new
            {
                productionStatus = opcClient.ReadNode($"ns=2;s=Device {deviceNumber}/ProductionStatus").Value,
                workorderId = opcClient.ReadNode($"ns=2;s=Device {deviceNumber}/WorkorderId").Value,
                goodCount = opcClient.ReadNode($"ns=2;s=Device {deviceNumber}/GoodCount").Value,
                badCount = opcClient.ReadNode($"ns=2;s=Device {deviceNumber}/BadCount").Value,
                temperature = opcClient.ReadNode($"ns=2;s=Device {deviceNumber}/Temperature").Value
            };

            var dataString = JsonConvert.SerializeObject(data);
            Message eventMessage = new Message(Encoding.UTF8.GetBytes(dataString));
            eventMessage.ContentType = MediaTypeNames.Application.Json;
            eventMessage.ContentEncoding = "utf-8";
            await client.SendEventAsync(eventMessage);
        }

        #endregion

        #region Direct Methods
        private async Task<MethodResponse> EmergencyStopHandler(MethodRequest methodRequest, object userContext)
        {
            Console.WriteLine($"\t METHOD EXECUTED: {methodRequest.Name}");
            await OpcDevice.EmergencyStop();
            return new MethodResponse(0);
        }

        private async Task<MethodResponse> ResetErrorStatusHandler(MethodRequest methodRequest, object userContext)
        {
            Console.WriteLine($"\t METHOD EXECUTED: {methodRequest.Name}");
            await OpcDevice.ResetErrorStatus();
            return new MethodResponse(0);
        }
        #endregion

        public async Task InitializeHandlers()
        {
            await client.SetMethodHandlerAsync("EmergencyStop", EmergencyStopHandler, client);
            await client.SetMethodHandlerAsync("ResetErrorStatus", ResetErrorStatusHandler, client);
        }
    }
}

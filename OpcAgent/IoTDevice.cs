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
using Microsoft.Azure.Amqp.Framing;

namespace IndustrialIoT
{
    enum Errors
    {
        EmergencyStop = 1,
        PowerFailure = 2,
        SensorFailue = 4,
        Unknown = 8
    }



    public class IoTDevice
    {
        bool started = true;
        bool update = false;

        private readonly DeviceClient client;

        public IoTDevice(DeviceClient deviceClient)
        {
            this.client = deviceClient;
        }

        #region UpdateTwinOnChange
        public async void updateCheck(OpcClient opcClient)
        {
            if (started)
            {
                await UpdateTwinAsync(OpcDevice.client);
                started = false;
                Console.Write("Device twin Start-Up update.");
            }
            else
            {
                Twin twin = await client.GetTwinAsync();
                int value = (int)opcClient.ReadNode($"ns=2;s=Device {Program.deviceNumber}/ProductionRate").Value;
                int twinValue = twin.Properties.Reported["productionRate"];
                StringBuilder errorBuilder = new StringBuilder();
                int errors = (int)opcClient.ReadNode($"ns=2;s=Device {Program.deviceNumber}/DeviceError").Value;
                if ((errors & Convert.ToInt32(Errors.Unknown)) != 0)
                {
                    errorBuilder.Append("Unknown ");
                }
                if ((errors & Convert.ToInt32(Errors.SensorFailue)) != 0)
                {
                    errorBuilder.Append("SensorFailure ");
                }
                if ((errors & Convert.ToInt32(Errors.PowerFailure)) != 0)
                {
                    errorBuilder.Append("PowerFailure ");
                }
                if ((errors & Convert.ToInt32(Errors.EmergencyStop)) != 0)
                {
                    errorBuilder.Append("Emergency stop ");
                }

                string errorsString = errorBuilder.ToString();
                string twinError = twin.Properties.Reported["deviceErrorsString"];

                if(twinValue != value)
                {
                    await UpdateTwinAsync(OpcDevice.client);
                    Console.WriteLine("ProductionRate Update");
                }
                
                if(twinError != errorsString)
                {
                    await UpdateTwinAsync(OpcDevice.client);
                    Console.WriteLine("Error Update");
                }

            }
        }
        #endregion

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
            updateCheck(opcClient);
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
        
        private async Task<MethodResponse> DefaultServiceHandler(MethodRequest methodRequest, object userContext)
        {
            Console.WriteLine($"\t DEFAULT METHOD EXECUTED: {methodRequest.Name}");
            await Task.Delay(1000);
            return new MethodResponse(0);
        }
        #endregion

        #region Device Twin

        public async Task UpdateTwinAsync(OpcClient opcClient)
        {
            StringBuilder errorBuilder = new StringBuilder();
            int errors = (int)opcClient.ReadNode($"ns=2;s=Device {Program.deviceNumber}/DeviceError").Value;
            if ((errors & Convert.ToInt32(Errors.Unknown)) != 0)
            {
                errorBuilder.Append("Unknown ");
            }
            if ((errors & Convert.ToInt32(Errors.SensorFailue)) != 0)
            {
                errorBuilder.Append("SensorFailure ");
            }
            if ((errors & Convert.ToInt32(Errors.PowerFailure)) != 0)
            {
                errorBuilder.Append("PowerFailure ");
            }
            if ((errors & Convert.ToInt32(Errors.EmergencyStop)) != 0)
            {
                errorBuilder.Append("Emergency stop ");
            }

            string errorsString = errorBuilder.ToString();

            var reportedProperties = new TwinCollection();

            reportedProperties["productionRate"] = opcClient.ReadNode($"ns=2;s=Device {Program.deviceNumber}/ProductionRate").Value;
            reportedProperties["deviceErrorsString"] = errorsString;
            reportedProperties["deviceErrorsCode"] = errors;

            await client.UpdateReportedPropertiesAsync(reportedProperties);
        }

        private async Task OnDesiredPropertyChanged(TwinCollection desiredProperties, object userContext)
        {
            int value = desiredProperties["productionRate"];
            OpcDevice.SetProductionRate(value);
            TwinCollection reportedProperties = new TwinCollection();
            reportedProperties["productionRate"] = value;

            await client.UpdateReportedPropertiesAsync(reportedProperties).ConfigureAwait(false);
        }

        #endregion

        public async Task InitializeHandlers()
        {
            await client.SetMethodHandlerAsync("EmergencyStop", EmergencyStopHandler, client);
            await client.SetMethodHandlerAsync("ResetErrorStatus", ResetErrorStatusHandler, client);
            
            await client.SetMethodDefaultHandlerAsync(DefaultServiceHandler, client);

            await client.SetDesiredPropertyUpdateCallbackAsync(OnDesiredPropertyChanged, client);
        }
    }
}

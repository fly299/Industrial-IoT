using Opc.UaFx;
using Opc.UaFx.Client;
using Microsoft.Azure.Devices.Client;
using System.Text;

var deviceConnectionString = "HostName=FirstDevice.azure-devices.net;DeviceId=test_device2;SharedAccessKey=4BvjMPCe0NeqAQ6CJljg299021g5sL78SAIoTJy01tk=";
using var deviceClient = DeviceClient.CreateFromConnectionString(deviceConnectionString, TransportType.Mqtt);



using (var client = new OpcClient("opc.tcp://localhost:4840/"))
{
    client.Connect();

    OpcReadNode[] commands = new OpcReadNode[] {
    new OpcReadNode("ns=2;s=Device 1/ProductionStatus", OpcAttribute.DisplayName),
    new OpcReadNode("ns=2;s=Device 1/ProductionStatus"),
    new OpcReadNode("ns=2;s=Device 1/ProductionRate", OpcAttribute.DisplayName),
    new OpcReadNode("ns=2;s=Device 1/ProductionRate"),
    new OpcReadNode("ns=2;s=Device 1/WorkorderId", OpcAttribute.DisplayName),
    new OpcReadNode("ns=2;s=Device 1/WorkorderId"),
    new OpcReadNode("ns=2;s=Device 1/Temperature", OpcAttribute.DisplayName),
    new OpcReadNode("ns=2;s=Device 1/Temperature"),
    new OpcReadNode("ns=2;s=Device 1/GoodCount", OpcAttribute.DisplayName),
    new OpcReadNode("ns=2;s=Device 1/GoodCount"),
    new OpcReadNode("ns=2;s=Device 1/BadCount", OpcAttribute.DisplayName),
    new OpcReadNode("ns=2;s=Device 1/BadCount"),
    new OpcReadNode("ns=2;s=Device 1/DeviceError", OpcAttribute.DisplayName),
    new OpcReadNode("ns=2;s=Device 1/DeviceError"),
};

    IEnumerable<OpcValue> job = client.ReadNodes(commands);

    foreach (var item in job)
    {
        Console.WriteLine(item.Value);
    }


    var productionStatus = client.ReadNode("ns=2;s=Device 1/ProductionStatus").Value.ToString();
    var workorderId = client.ReadNode("ns=2;s=Device 1/WorkorderId").Value.ToString();
    var goodCount = client.ReadNode("ns=2;s=Device 1/GoodCount").Value.ToString();
    var badCount = client.ReadNode("ns=2;s=Device 1/BadCount").Value.ToString();
    var temperature = client.ReadNode("ns=2;s=Device 1/Temperature").Value.ToString();

    var messagePayload = $"{{ \"ProductionStatus\": \"{productionStatus}\", \"WorkorderId\": \"{workorderId}\", \"GoodCount\": \"{goodCount}\", \"BadCount\": \"{badCount}\", \"Temperature\": \"{temperature}\" }}";
    var telemetryMessage = new Message(Encoding.UTF8.GetBytes(messagePayload));

    await deviceClient.SendEventAsync(telemetryMessage);

}

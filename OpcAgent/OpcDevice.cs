using Opc.UaFx;
using Opc.UaFx.Client;
using Microsoft.Azure.Devices.Client;
using System.Text;
using Opc.Ua;
using Microsoft.Azure.Amqp.Framing;

namespace IndustrialIoT
{
    public class OpcDevice
    {
        public static OpcClient client = new OpcClient(File.ReadAllLines($"../../../../Config.txt")[1]);

        public void Start()
        {
            Console.WriteLine(File.ReadAllLines($"../../../../Config.txt")[1].ToString());
            client.Connect();

            Console.WriteLine("Device list:");
            var node = client.BrowseNode(OpcObjectTypes.ObjectsFolder);
            foreach (var childNode in node.Children())
                if (!childNode.DisplayName.Value.Contains("Server"))
                {
                    Console.WriteLine($"{childNode.Name}");
                }
        }

        public static async Task EmergencyStop()
        {
            client.CallMethod($"ns=2;s=Device {Program.deviceNumber}", $"ns=2;s=Device {Program.deviceNumber}/EmergencyStop");
            await Task.Delay(1000);
        }

        public static async Task ResetErrorStatus()
        {
            client.CallMethod($"ns=2;s=Device {Program.deviceNumber}", $"ns=2;s=Device {Program.deviceNumber}/ResetErrorStatus");
            await Task.Delay(1000);
        }
    }
}
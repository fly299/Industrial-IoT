using Opc.UaFx;
using Opc.UaFx.Client;
using Microsoft.Azure.Devices.Client;
using System.Text;
using Opc.Ua;

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
    }
}
using Opc.UaFx;
using Opc.UaFx.Client;
using Microsoft.Azure.Devices.Client;
using System.Text;

public class OpcDevice
{
    OpcClient client = new OpcClient(File.ReadAllLines($"../../../../Config.txt")[1]);

    public void Start()
    {
        Console.WriteLine(File.ReadAllLines($"../../../../Config.txt")[1].ToString());
        client.Connect();
    }
}
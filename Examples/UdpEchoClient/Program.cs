using System.Net; // For IPAddress
using System.Net.Sockets; // For UdpClient
using System.Text; // For Encoding (ASCII)

class UdpEchoClient {
	static void Main() {
		StartClient(50001);
	}

	static void StartClient(int remotePort) {
		// Start a UdpClient on an arbitrary port:
		UdpClient client = new UdpClient();
		Console.WriteLine($"Starting UDP client");
		// Note: client.Client.LocalEndPoint is null for now -
		// it will only be assigned when we send the first message!

		// Remote is the "server" port, at local host (=IP address 127.0.0.1):
		IPEndPoint remote = new IPEndPoint(IPAddress.Loopback,remotePort);
		// This variable will be used to keep track of where incoming packets are coming from:
		IPEndPoint receiveRemote = new IPEndPoint(IPAddress.Any, 0);

		while (true) {
			Console.WriteLine("Enter a message:");
			Console.WriteLine("(type 'close' to stop the client)");
			// Block until the user enters an input:
			string input = Console.ReadLine();
			if (input == "close") break;
			// Translate that input to a byte array, using ASCII encoding:
			byte[] packet = Encoding.ASCII.GetBytes(input);
			// Send it to the remote end point (=server):
			client.Send(packet, remote);

			// Block until a packet comes in:
			packet = client.Receive(ref receiveRemote);
			// Then, translate it to a string using ASCII encoding, and print it:
			string reply = Encoding.ASCII.GetString(packet);
			Console.WriteLine($"Received a reply packet from {receiveRemote}: {reply}");
		}
		// clean up resources:
		client.Close();
	}
}


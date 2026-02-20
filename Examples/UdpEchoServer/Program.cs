using System.Net; // For IPAddress
using System.Net.Sockets; // For UdpClient

class UdpServer {
	static void Main() {
		StartServer(50001);
	}

	static void StartServer(int port) {
		// Create a UdpClient on the given port.
		// Note that in UDP, since it's not connection based,
		// there is not much difference between clients and servers.
		// However, a UdpClient that plays the role of server should have a known port, such that
		//  other clients can reach it:
		UdpClient client = new UdpClient(port);
		Console.WriteLine($"Starting UDP server on port {port} - listening for incoming messages");

		IPEndPoint remote = new IPEndPoint(IPAddress.Any,0);
		while (true) {
			// The next call blocks until a packet comes in.
			// Packets can be received from anywhere!
			// The [remote] ref (=output parameter) tells us where this packet came from:
			byte[] packet = client.Receive(ref remote);
			Console.WriteLine($"Received a {packet.Length} packet from {remote}. Echoing...");
			// Don't do anything with the packet, just send it back to where it came from: 
			client.Send(packet, remote);
		}
	}
}


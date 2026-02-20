using System.Net.Sockets; // For TcpClient
using System.Text; // For Encoding (ASCII)

class TcpEchoClient {
	static void Main(string[] args) {
		int remotePort = 50001;
		// In this case, the first command line argument is the remote port (=server port):
		if (args.Length > 0 && int.TryParse(args[0], out int newRemotePort)) {
			remotePort = newRemotePort;
		}
		StartClient(remotePort);
	}

	static void StartClient(int remotePort) {
		// There is no error handling here. (Where) should it be added?

		// Start a TcpClient on an arbitrary local port, and connect it to the server:
		// (server IP address = local host = 127.0.0.1)
		TcpClient client = new TcpClient();
		client.Connect("127.0.0.1", remotePort);
		Console.WriteLine($"Starting TCP spam client on {client.Client.LocalEndPoint}, connected to {client.Client.RemoteEndPoint}");
		Console.WriteLine("Press 'S' to start/stop spamming the server, 'Q' to quit");

		NetworkStream stream = client.GetStream();
		bool spamming = false;
		int messageNumber = 0;
		// Create a bunch of long message packets to be sent:
		List<byte[]> messages = CreateMessages(26, 80);

		while (true) {
			if (spamming) {
				// Send the next message packet over the TcpClient's network stream:
				stream.Write(messages[messageNumber], 0, messages[messageNumber].Length);
				Console.ForegroundColor = ConsoleColor.Red;
				Console.WriteLine("Sending message:\n" + Encoding.ASCII.GetString(messages[messageNumber]));
				messageNumber = (messageNumber + 1) % messages.Count;
			}
			// Poll whether at least one incoming byte is available on the network stream:
			if (client.Available>0) {
				// ..if so, read *exactly* that many bytes from the stream:
				byte[] packet = new byte[client.Available];
				// Note that stream.Read is now not blocking, since we know at least one byte is available!
				stream.Read(packet,0, packet.Length);

				// Decode the incoming byte packet as an ASCII string and print it:
				string message = Encoding.ASCII.GetString(packet);
				Console.ForegroundColor = ConsoleColor.Green;
				Console.WriteLine("Received message:\n" + message);
			}
			// Console.KeyAvailable is true whenever a key is pressed (while the console window is in focus):
			if (Console.KeyAvailable) {
				// ..if so, read that key and get the corresponding character:
				char input = Console.ReadKey(true).KeyChar;
				if (input =='s') {
					spamming = !spamming;
				} else if (input == 'q') {
					break;
				}
			}
			// We are spamming, but not *that much*!
			// Five messages per second (=200ms delay) is enough:
			Thread.Sleep(200);
		}
		client.Close();
	}

	static List<byte[]> CreateMessages(int number, int length) {
		List<byte[]> messages = new List<byte[]>();

		for (int i=0;i<number;i++) {
			byte[] message = new byte[length];
			for (int j=0;j<length;j++) {
				// chars and ints can be added together! The result is an int, which
				// corresponds exactly to the ASCII code of the character.
				// We cast this to a byte and put it into the byte array:
				message[j] = (byte)('A' + i % 26);
			}
			messages.Add(message);
		}
		return messages;
	}
}


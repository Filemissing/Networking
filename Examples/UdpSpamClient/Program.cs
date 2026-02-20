using System.Net; // For IPAddress
using System.Net.Sockets; // For UdpClient
using System.Text; // For Encoding (ASCII)

class UdpSpamClient {
	/// <summary>
	/// (static void) Main is the "entry point" for any (Console) program - this is where the program starts
	/// </summary>
	static void Main() {
		StartClient(50001);
	}

	static void StartClient(int remotePort) {
		UdpClient client = new UdpClient();
		Console.WriteLine($"Starting UDP client");

		IPEndPoint remote = new IPEndPoint(IPAddress.Loopback,remotePort);
		IPEndPoint receiveRemote = new IPEndPoint(IPAddress.Any, 0);

		bool spamming = false;
		int messageNumber = 0;
		List<byte[]> messages = CreateMessages(26, 80);

		while (true) {
			if (spamming) {
				client.Send(messages[messageNumber], remote); 
				Console.ForegroundColor = ConsoleColor.Red;
				Console.WriteLine("Sending message:\n" + Encoding.ASCII.GetString(messages[messageNumber]));
				messageNumber = (messageNumber + 1) % messages.Count;
			}
			if (client.Available>0) {
				byte[] packet = client.Receive(ref receiveRemote);
				string message = Encoding.ASCII.GetString(packet);
				Console.ForegroundColor = ConsoleColor.Green;
				Console.WriteLine("Received message:\n" + message);
			}
			if (Console.KeyAvailable) {
				char input = Console.ReadKey(true).KeyChar;
				if (input == 's') {
					spamming = !spamming;
				} else if (input == 'q') {
					break;
				}
			}
			Thread.Sleep(200);
		}
		client.Close();
	}

	static List<byte[]> CreateMessages(int number, int length) {
		List<byte[]> messages = new List<byte[]>();

		for (int i = 0; i < number; i++) {
			byte[] message = new byte[length];
			for (int j = 0; j < length; j++) {
				message[j] = (byte)('A' + i % 26);
			}
			messages.Add(message);
		}
		return messages;
	}
}


using System.Net; // For IPAddress
using System.Net.Sockets; // For TcpClient
using System.Text; // For Encoding (ASCII)

class TcpEchoClient {
	/// <summary>
	/// (static void) Main is the "entry point" for any (Console) program - this is where the program starts.
	/// The string array [args] contains the command line arguments, when the program is started from a terminal window.
	/// </summary>
	static void Main(string[] args) {
		int remotePort = 50001;
		if (args.Length > 0) {
			// If there is at least one command line argument, we check if it's an integer. 
			// If so, that will be the local port of our TcpClient:
			if (int.TryParse(args[0], out int localPort)) {
				// If there's a second integer command line argument, that will be the remote port (=server port)
				if (args.Length > 1 && int.TryParse(args[1], out int newRemotePort)) {
					remotePort = newRemotePort;
				}
				StartClient(localPort, remotePort);
			} else {
				Console.WriteLine("Command line arguments: [localPort] [remotePort]\n");
			}
		} else {
			StartClient(0, remotePort);
		}
	}

	static void StartClient(int localPort, int remotePort) {
		// There is no error handling here. (Where) should it be added?

		// If localPort is zero, start a TcpClient on an arbitrary port.
		// Otherwise, start it on the given local port:
		TcpClient client = localPort>0?
			new TcpClient(new IPEndPoint(IPAddress.Any,localPort)):
			new TcpClient();
		// Connect the TcpClient to a server (TcpListener) on "localhost/loopback" (IP address 127.0.0.1),
		//  on the given remote port:
		client.Connect("127.0.0.1", remotePort);
		Console.WriteLine($"Starting TCP client on {client.Client.LocalEndPoint}, connected to {client.Client.RemoteEndPoint}");

		byte[] buffer = new byte[1024];
		NetworkStream stream = client.GetStream();

		while (true) {
			Console.WriteLine("Enter a message. Type 'close' to close the connection:");
			// This line blocks until user input is entered:
			string input = Console.ReadLine();
			if (input=="close") {
				break;
			}
			// Encoding.ASCII can be used to translate between strings and byte arrays:
			// (To support more exotic unicode characters, you could use e.g. Encoding.UTF8 instead.)
			byte[] packet = Encoding.ASCII.GetBytes(input);
			// Send the encoded string through the TcpClient's network stream:
			stream.Write(packet, 0, packet.Length);

			Console.WriteLine("Message sent. Waiting for reply.");
			
			// In this blocking call, we wait until a reply is received.
			// [bytesRead] is the number of bytes that were available (at most [buffer.Length])
			int bytesRead = stream.Read(buffer,0, buffer.Length);
			// Next, decode the incoming bytes using the same ASCII encoding, and print it:
			string reply = Encoding.ASCII.GetString(buffer, 0, bytesRead);
			Console.WriteLine($"{bytesRead} byte reply received: {reply}");
		}
		Console.WriteLine("Closing client");
		client.Close();
	}
}


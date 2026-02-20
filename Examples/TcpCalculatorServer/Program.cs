using System.Net; // For IPAddress
using System.Net.Sockets; // For TcpListener, TcpClient
using System.Text; // For Encoding (ASCII)

class TcpServer {
	static void Main() {
		StartServer(50001);
	}

	static void StartServer(int port) {
		TcpListener listener = new TcpListener(IPAddress.Any, port);
		listener.Start();
		Console.WriteLine($"Starting TCP calculator server on port {port} - listening for incoming connection requests");

		while (true) {
			TcpClient client = listener.AcceptTcpClient();
			Console.WriteLine($"Client connected from remote end point {client.Client.RemoteEndPoint}");
			NetworkStream stream = client.GetStream();
			// Still deal with clients iteratively (one by one), but
			// poll the connection to see if input is available:
			// (See the other TcpServers for details)
			//
			// Does this while loop ever end?
			// Should there be error handling? Where?
			while (client.Connected) {
				if (client.Available>0) {
					byte[] data = new byte[client.Available];
					int bytesRead = stream.Read(data,0, data.Length);
					Console.WriteLine("Bytes read: "+bytesRead);
					string request = Encoding.ASCII.GetString(data, 0, data.Length);

					// This is the first example where the server actually parses
					// the input and creates a reply:
					string reply = GetReply(request);

					byte[] packet = Encoding.ASCII.GetBytes(reply);
					stream.Write(packet,0,packet.Length);

				}
				Thread.Sleep(100);
			}
			Console.WriteLine("Client disconnected");
			client.Close();
		}
	}

	static string GetReply(string input) {
		// This method shows different ways of doing string parsing.
		// Note that there is no error handling whatsoever, so the server can crash in many ways!
		// *Try to find all the possible ways to break this, and do better than this in your own servers!*

		// Trim removes whitespace at the start and end.
		// ToLower replaces capital letters by lower case letters (Inverse: ToUpper).
		input = input.Trim().ToLower();
		if (input.StartsWith("add") || input.StartsWith("mul") || input.StartsWith("sub") || input.StartsWith("div")) {
			// Substring gets a substring. Parameters: startIndex, length.
			// Split splits a string into an array of strings, given a separator character.
			string[] args = input.Substring(4, input.Length - 4).Split(' ');
			// With int.Parse, strings (consisting of digits) can be translated to integer numbers:
			// (Similar useful methods: float.Parse, int.TryParse, etc.)
			int left = int.Parse(args[0]);
			int right = int.Parse(args[1]);

			int result = 0;
			switch (input.Substring(0, 3)) {
				case "add":
					result = left + right;
					break;
				case "mul":
					result = left * right;
					break;
				case "sub":
					result = left - right;
					break;
				case "div":
					result = left / right;
					break;
			}
			return result.ToString();
		} else {
			return $"Unknown command\nUse one of the following commands: add, mul, sub, div, followed by two numbers, separated by spaces.\nExample:\nadd 7 4";
		}
	}
}


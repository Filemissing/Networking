using System.Net; // For IPAddress
using System.Net.Sockets; // For TcpListener, TcpClient

class TcpServer {
	static void Main() {
		StartServer(50001);
	}

	static void StartServer(int port) {
		// Start listening for TCP connection requests, on the given port:
		TcpListener listener = new TcpListener(IPAddress.Any, port);
		listener.Start();
		Console.WriteLine($"Starting TCP server on port {port} - listening for incoming connection requests");
		Console.WriteLine("Press Q to stop the server");

		// Now we handle multiple connected clients simultaneously - 
		// we keep them in a list:
		List<TcpClient> clients = new List<TcpClient>();

		while (true) {
			// Note: there is no error handling in this server! Is it needed? If so, where?
			AcceptNewClients(listener, clients);
			HandleMessages(clients);
			// Clean up disconnected clients. Does this actually ever happen?!
			CleanupClients(clients);
			if (QuitPressed()) {
				Console.WriteLine("Stopping server");
				break;
			}
			// It's good to give the CPU a break - 10ms is enough, and still gives fast response times:
			Thread.Sleep(10);
		}
		// When stopping the server, properly clean up all resources:
		foreach (TcpClient client in clients) {
			client.Close();
		}
		listener.Stop();
		Console.WriteLine("Server stopped");
	}

	static void AcceptNewClients(TcpListener listener, List<TcpClient> clients) {
		// Pending will be true if there is an incoming connection request:
		if (listener.Pending()) {
			// ..if so, accept it and store the new TcpClient:
			// (Note that the AcceptTcpClient call is not blocking now, since we know there's a pending request!)
			TcpClient newClient = listener.AcceptTcpClient();
			clients.Add(newClient);
			Console.WriteLine($"Client connected from remote end point {newClient.Client.RemoteEndPoint}");
		}
	}
	static void HandleMessages(List<TcpClient> clients) {
		foreach (TcpClient client in clients) {
			// For each of the connected clients, we check whether there's an incoming message available:
			if (client.Available > 0) {
				// ..if so, we read exactly that many bytes into an array:
				NetworkStream stream = client.GetStream();
				int packetLength = client.Available;
				byte[] data = new byte[packetLength];
				stream.Read(data, 0, packetLength);
				Console.WriteLine($"Received a message of length {packetLength} from {client.Client.RemoteEndPoint} - echoing");
				// For now, we don't do anything special with the incoming message - 
				// just send it straight back to the sender:
				stream.Write(data);
			}
		}
	}
	static void CleanupClients(List<TcpClient> clients) {
		for (int i = clients.Count - 1; i >= 0; i--) {
			// If any of our current clients are disconnected, 
			// we close the TcpClient to clean up resources, and remove it from our list:
			// (Note that this type of for loop is needed since we're modifying the collection inside the loop!)
			if (!clients[i].Connected) {
				clients[i].Close();
				clients.RemoveAt(i);
				Console.WriteLine($"Removing client. Number of connected clients: {clients.Count}");
			}
		}
	}
	static bool QuitPressed() {
		if (Console.KeyAvailable) {
			char input = Console.ReadKey(true).KeyChar;
			if (input == 'q') {
				return true;
			}
		}
		return false;
	}
}


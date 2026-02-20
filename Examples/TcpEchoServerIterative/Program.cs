using System.Net; // For IPAddress
using System.Net.Sockets; // For TcpListener, TcpClient

class TcpServer {
	/// <summary>
	/// (static void) Main is the "entry point" for any (Console) program - this is where the program starts
	/// </summary>
	static void Main() {
		StartServer(50001);
	}

	static void StartServer(int port) {
		// Start listening for TCP connection requests, on the given port:
		TcpListener listener = new TcpListener(IPAddress.Any, port);
		listener.Start();
		Console.WriteLine($"Starting TCP server on port {port} - listening for incoming connection requests");

		byte[] buffer = new byte[1024];

		while (true) {
			// This is a blocking call: the program waits until a connection request comes in.
			// In that case, a new TcpClient is created which allows communication with that remote client:
			TcpClient client = listener.AcceptTcpClient();
			Console.WriteLine($"Client connected from remote end point {client.Client.RemoteEndPoint}");
			// Bidirectional communication (read/write) is done through the TcpClient's NetworkStream:
			NetworkStream stream = client.GetStream();

			while (client.Connected) {
				try {
					// Block until at least one byte is received on the current stream. 
					// Then, read all available bytes, or [buffer.Length] bytes if more
					//  than that many bytes are available.
					// [bytesRead] returns the number of bytes that were read into the buffer.
					int bytesRead = stream.Read(buffer, 0, buffer.Length);
					// This is one way in which TcpClients can indicate that the remote client has been closed:
					if (bytesRead==0) {
						Console.WriteLine("Client stream closed");
						client.Close();
						break;
					}

					Console.WriteLine($"Received a message of length {bytesRead} - echoing");

					// For now, we don't do anything with the incoming message, and just send it
					// back to where it came from:
					stream.Write(buffer, 0, bytesRead);
				} catch (Exception error) {
					// Many things can go wrong when working with network streams. 
					// If so, we catch the Exception, print it, and close the client:
					Console.WriteLine($"Error while handling client: {error.Message}");

					// Close the client to clean up resources:
					client.Close();
				}
			}
		}
		// This code is never reached since the server runs until you stop the program, but
		// listeners can (should) be stopped like this:
		//listener.Stop();
	}
}


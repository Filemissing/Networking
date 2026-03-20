using Rug.Osc;
using System.Net; // For IPAddress
using System.Net.Sockets;
using System.Text; // For TcpListener, TcpClient

class Server
{
    static void Main()
    {
        StartServer(50001);
    }

    static List<TcpClient> playerClients = new();
    public static Dictionary<TcpClient, string?> playerToGame = new();
    static Dictionary<string, Game> games = new();

    static void StartServer(int port)
    {
        // Start listening for TCP connection requests, on the given port:
        TcpListener listener = new TcpListener(IPAddress.Any, port);
        listener.Start();
        Console.WriteLine($"Starting TCP server on port {port} - listening for incoming connection requests");

        while (true)
        {
            AcceptNewClients(listener);
            
            HandleMessages();
            
            CleanupClients();

            Thread.Sleep(10);
        }

        // When stopping the server, properly clean up all resources
        foreach (TcpClient client in playerClients)
            client.Close();
        listener.Stop();
        Console.WriteLine("Server stopped");
    }

    static void OnClientMessage(TcpClient client, OscMessage message)
    {
        string[] addressParts = message.Address.Trim('/').Split('/');
        try
        {
            switch (addressParts[0])
            {
                case "echo":
                    if (addressParts[1] == "game")
                    {
                        string? lobbyName; 
                        playerToGame.TryGetValue(client, out lobbyName);
                        if (lobbyName != null)
                            games[lobbyName].Echo(message[0] as string);
                    }
                    else if (addressParts[1] == "global")
                        foreach (TcpClient reciever in playerClients)
                            SendOscMessage(reciever, message);

                    break;

                case "lobby":
                    HandleGameEvent(client, addressParts, message);
                    break;

                case "player":
                    HandlePlayerEvent(client, addressParts, message);
                    break;

                default:
                    Console.WriteLine($"Recieved unknown message {message.Address} from client at {client.Client.RemoteEndPoint}");
                    break;
            }
        } catch (Exception e)
        {
            Console.WriteLine($"Error handling message {message.ToString()}");
            Console.Write(e.ToString());
        }
    }

    static void HandleGameEvent(TcpClient client, string[] addressParts, OscMessage message)
    {
        // events relating to game creation and ending, rematches, etc.
        switch (addressParts[1])
        {
            case "list": // request a list of all active games available to join - no arguments
                OscMessage returnMessage = new("/lobby/list", games.Keys.Where(key => games[key].players.Count == 1).ToArray());
                SendOscMessage(client, returnMessage);
                break;

            case "create": // create a new game - 1 argument name
                string name = (string)message[0];
                Game game = new Game(name);
                game.Join(client);
                games[name] = game;
                break;

            case "join": // join a game - 1 argument name
                games[(string)message[0]].Join(client);
                break;

            case "leave":
                string? lobbyName;
                playerToGame.TryGetValue(client, out lobbyName);
                if (lobbyName != null)
                    games[lobbyName].Leave(client);
                break;

            default:
                Console.WriteLine($"Recieved unknown Game-event {message.Address} from client at {client.Client.RemoteEndPoint}");
                break;
        }
    }
    static void HandlePlayerEvent(TcpClient client, string[] addressParts, OscMessage message)
    {
        // events relating to player actions, move, resign, chat
        switch (addressParts[1])
        {
            case "move": // player moved their piece - 4 piece, color, from, to
                string? gameName = playerToGame[client];
                if (gameName != null)
                {
                    games[gameName].Move(client, (string)message[0], (string)message[1], (string)message[2], (string)message[3]);
                }
                break;

            default:
                Console.WriteLine($"Recieved unknown Player-event {message.Address} from client at {client.Client.RemoteEndPoint}");
                break;
        }
    }

    public static void SendOscMessage(TcpClient client, OscMessage message)
    {
        byte[] oscData = message.ToByteArray();
        byte[] lengthBytes = BitConverter.GetBytes(oscData.Length);

        NetworkStream stream = client.GetStream();
        stream.Write(lengthBytes, 0, 4);
        stream.Write(oscData, 0, oscData.Length);
    }

    static void AcceptNewClients(TcpListener listener)
    {
        if (listener.Pending())
        {
            TcpClient newClient = listener.AcceptTcpClient();
            playerClients.Add(newClient);
            Console.WriteLine($"Client connected from remote end point {newClient.Client.RemoteEndPoint}");
        }
    }
    static void HandleMessages()
    {
        foreach (TcpClient client in playerClients)
        {
            while (client.Available > 0)
            {
                NetworkStream stream = client.GetStream();

                // message length is denoted by a 32 bit integer -> 4 bytes
                byte[] lengthBytes = new byte[4];
                stream.Read(lengthBytes, 0, 4);
                int packetLength = BitConverter.ToInt32(lengthBytes, 0);

                // read the actual packet content
                byte[] bytes = new byte[packetLength];
                stream.Read(bytes, 0, packetLength);

                OscMessage message = OscMessage.Read(bytes, packetLength);

                OnClientMessage(client, message);
            }
        }
    }
    static void CleanupClients()
    {
        for (int i = playerClients.Count - 1; i >= 0; i--)
        {
            // If any of our current clients are disconnected, 
            // we close the TcpClient to clean up resources, and remove it from our list:
            // (Note that this type of for loop is needed since we're modifying the collection inside the loop!)
            if (!playerClients[i].Connected)
            {
                playerClients[i].Close();
                playerClients.RemoveAt(i);
                Console.WriteLine($"Removing client. Number of connected clients: {playerClients.Count}");
            }
        }
    }
}

// data structure classes
class Game
{
    public string Name { get; private set; }

    public List<Player> players { get; private set; } = new List<Player>();

    Piece?[,] board = new Piece?[8, 8]
    {
        { new(PieceType.Rook, Color.White),     new(PieceType.Pawn, Color.White), null, null, null, null, new(PieceType.Pawn, Color.Black), new(PieceType.Rook, Color.Black)    },
        { new(PieceType.Knight, Color.White),   new(PieceType.Pawn, Color.White), null, null, null, null, new(PieceType.Pawn, Color.Black), new(PieceType.Knight, Color.Black)  },
        { new(PieceType.Bishop, Color.White),   new(PieceType.Pawn, Color.White), null, null, null, null, new(PieceType.Pawn, Color.Black), new(PieceType.Bishop, Color.Black)  },
        { new(PieceType.Queen, Color.White),    new(PieceType.Pawn, Color.White), null, null, null, null, new(PieceType.Pawn, Color.Black), new(PieceType.Queen, Color.Black)   },
        { new(PieceType.King, Color.White),     new(PieceType.Pawn, Color.White), null, null, null, null, new(PieceType.Pawn, Color.Black), new(PieceType.King, Color.Black)    },
        { new(PieceType.Bishop, Color.White),   new(PieceType.Pawn, Color.White), null, null, null, null, new(PieceType.Pawn, Color.Black), new(PieceType.Bishop, Color.Black)  },
        { new(PieceType.Knight, Color.White),   new(PieceType.Pawn, Color.White), null, null, null, null, new(PieceType.Pawn, Color.Black), new(PieceType.Knight, Color.Black)  },
        { new(PieceType.Rook, Color.White),     new(PieceType.Pawn, Color.White), null, null, null, null, new(PieceType.Pawn, Color.Black), new(PieceType.Rook, Color.Black)    },
    };
    List<string> record = new(); // not yet used but would be a nice feature
    int turn = 0;

    public Game(string name)
    {
        this.Name = name;
    }

    public void Join(TcpClient client)
    {
        if (players.Count == 2)
            return;

        Server.playerToGame[client] = Name;

        Player newPlayer = new Player(client);
        players.Add(newPlayer);

        foreach (var player in players)
            Server.SendOscMessage(player.client, new OscMessage("/lobby/join", Name, players.Count));

        if (players.Count == 2)
            Start();
    }
    public void Leave(TcpClient client)
    {
        Player? player = GetPlayerByClient(client);
        if (player != null)
        {
            players.Remove(player);
            Server.playerToGame[client] = null;
        }

        if (players.Count == 0)
            Stop(EndState.Disconnected);
    }

    public void Start()
    {
        Random rng = new();
        int whitePlayer = rng.Next(players.Count);
        players[whitePlayer].color = Color.White; // black is default so that is already set for the other player

        foreach (Player player in players)
        {
            Server.SendOscMessage(player.client, new OscMessage("/game/start", player.color.ToString()));
        }
    }
    public void Stop(EndState state)
    {
        foreach (Player player in players)
        {
            if (player.client != null)
            {
                Server.SendOscMessage(player.client, new OscMessage("/game/stop", state.ToString())); // notify any remaining connections that this room is no longer available
            }
        }
    }

    public void Move(TcpClient client, string piece, string color, string oldPos, string newPos)
    {
        Player? player = GetPlayerByClient(client);

        if (player == null)
            return;

        bool isWhiteTurn = turn % 2 == 0;

        if (isWhiteTurn ? player.color == Color.Black : player.color == Color.White)
        {
            Server.SendOscMessage(client, new OscMessage("/player/move/invalid", BoardToString()));
            return; // wait your turn you cheater
        }

        PieceType pieceType = (PieceType)Enum.Parse(typeof(PieceType), piece);
        Color pieceColor = (Color)Enum.Parse(typeof(Color), color);

        (int, int) oldCoords = StringToCoords(oldPos);
        (int, int) newCoords = StringToCoords(newPos);

        Piece? originPiece = board[oldCoords.Item1, oldCoords.Item2];
        Piece? targetPiece = board[newCoords.Item1, newCoords.Item2];

        bool isValidColorPiece = isWhiteTurn ? pieceColor == Color.White : pieceColor == Color.Black;

        // very basic move validation
        if (isValidColorPiece && (originPiece != null && originPiece.type == pieceType) && (targetPiece == null || targetPiece.color != pieceColor)) 
        {
            board[newCoords.Item1, newCoords.Item2] = originPiece;
            board[oldCoords.Item1, oldCoords.Item2] = null;
            Server.SendOscMessage(client, new OscMessage("/player/move/valid", BoardToString()));
        }
        else
        {
            Server.SendOscMessage(client, new OscMessage("/player/move/invalid", BoardToString()));
            return;
        }

        turn++;

        Player otherPlayer = GetOtherPlayer(GetPlayerByClient(client));
        Server.SendOscMessage(otherPlayer.client, new OscMessage("/player/move", BoardToString()));
    }
    public void Echo(string message)
    {
        foreach (Player player in players)
            Server.SendOscMessage(player.client, new OscMessage("/echo/game", message));
    }

    public enum EndState
    {
        Disconnected,
        White,
        Black,
    }

    // helper methods
    Player GetOtherPlayer(Player player)
    {
        int index = players.IndexOf(player);
        return index == 0 ? players[1] : players[0];
    }
    Player? GetPlayerByClient(TcpClient client)
    {
        return players.Find(p => p.client == client);
    }

    (int, int) StringToCoords(string str)
    {
        return (int.Parse(str[0].ToString()), int.Parse(str[1].ToString())); // todo: error handling
    }

    public string BoardToString()
    {
        StringBuilder sb = new StringBuilder(64);
        for (int x = 0; x < 8; x++)
        {
            for (int y = 0; y < 8; y++)
            {
                Piece? piece = board[x, y];
                if (piece == null)
                {
                    sb.Append('0');
                }
                else
                {
                    int value = (int)piece.type + 1 + (piece.color == Color.White ? 0 : 6);
                    // For pawns that have already moved, add 12 as an offset
                    if (piece.type == PieceType.Pawn && !piece.firstMove)
                        value += 12;
                    sb.Append((char)('0' + value));
                }
            }
        }
        return sb.ToString();
    }
}

class Player
{
    public TcpClient client { get; private set; }

    public Color color = Color.Black;

    public Player(TcpClient client)
    {
        this.client = client;
    }
}

class Piece
{
    public Piece(PieceType type, Color color)
    {
        this.type = type;
        this.color = color;
    }
    public PieceType type { get; private set; }
    public Color color { get; private set; }

    public bool firstMove = true;
}

enum Color
{
    White,
    Black
}

enum PieceType
{
    King,
    Queen,
    Bishop,
    Knight,
    Rook,
    Pawn
}

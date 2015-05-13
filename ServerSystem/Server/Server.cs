using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Collections.Generic;
using MySql.Data.MySqlClient;

public class IP
{
    public const string Anthony = "169.234.17.166";//school ip
    public const string Anthony1 = "70.187.161.177";//my other public ip's
    public const string Anthony2 = "71.94.130.204";
    public const string Faye = "169.234.29.80";
    public const string mySQL = IP.Anthony;
}
// State object for reading client data asynchronously
public class StateObject
{
    // Client  socket.
    public Socket workSocket = null;
    // Size of receive buffer.
    public const int BufferSize = 1024;
    // Receive buffer.
    public byte[] buffer = new byte[BufferSize];
    // Received data string.
    public StringBuilder sb = new StringBuilder();
}

// Every four players is one game. More than one game may be allowed.
// An IP address can have multiple Games, or multiple players of the same Game.
public class Game
{
    public enum Status { NONE, LOBBY, PLAYING, PAUSED, ENDED }; // others to be added

    // Game details
    public Status status = Status.NONE;
    //public List<Player> allPlayers;

    //**need a list of possible locations for 4 players
    public List<Player> allPlayers;
    public int nextindex;
    public List<Bomb> allBombs;
    private int readycount;

    public Game()
    {
        allPlayers = new List<Player>();
        allBombs = new List<Bomb>();
        nextindex = 0;//starting position, when one player is added: next position is 1
        readycount = 0;
    }

    // After lobby details are finished and a game starts...
    public void GameLoop()
    {
        int timer = 0;
        int maxTimerValue = 40; // change this value
        // Continue looping until game ends and a winner is determined.
        while (status != Status.ENDED)
        {
            if (++timer != maxTimerValue)
            {
                continue;
            }
            //Implement game logic and stuffs here.

            sendPosition();
            sendBombs();
        }
    }

    private void sendToAll(string package)
    {
        AsynchronousSocketListener.sendALL(package);
        /*foreach (Player player in allPlayers)
        {
            if (player != null)
            {
                AsynchronousSocketListener.directedSend(player.clientSocket, package);
            }
        }*/
    }

    private void sendPosition() // Player;0;T;9;8;0end;1;T;8;9;1end;2;T;9;7;2end;3;F;9;6;3end;
    {
        string package = "Player;";
        for (int i = 0; i < getplayerCount(); i++)
        {
            string header = i.ToString() + MessageHandler.semicolon;
            bool isAlive = !(allPlayers[i].isDead());
            string trueFalse = isAlive.ToString();
            string deadOrAlive = trueFalse[0].ToString() + MessageHandler.semicolon;
            string x = allPlayers[i].x.ToString() + MessageHandler.semicolon;
            string z = allPlayers[i].z.ToString() + MessageHandler.semicolon;
            string footer = i.ToString() + "end" + MessageHandler.semicolon;
            package += header + deadOrAlive + x + z + footer;
        }
        sendToAll(package);
    }

    // If there are still bombs in the list, send them out and delete.
    private void sendBombs() // Bomb;x;z;strength;
    {
        string header = "Bomb;";
        while (allBombs.Count != 0)
        {
            Bomb bomb = allBombs[0];
            for (int i = 0; i < getplayerCount(); i++)
            {
                string x = bomb.x.ToString() + MessageHandler.semicolon;
                string z = bomb.z.ToString() + MessageHandler.semicolon;
                string strength = bomb.strength.ToString() + MessageHandler.semicolon;

                string package = header + x + z + strength;

                sendToAll(package);
            }
            allBombs.Remove(bomb);
        }
    }

    // A bit incomplete as I'm not sure how you guys want it
    public class Player
    {
        public enum Status { NONE, WAITING, READY, ALIVE, DEAD }; // others to be added
        public Status status = Status.NONE;
        public String username;
        public int index;//--added by Anthony for lobby logic
        //public Socket clientSocket;
        public Socket clientSocket;
        public float x, z;

        public Player(Socket clientSocket, float x, float z, int index, String username)
        {
            this.clientSocket = clientSocket;
            this.x = x;
            this.z = z;
            this.index = index;
            this.username = username;
            this.status = Status.ALIVE;
        }
        public void alive()
        {
            this.status = Status.ALIVE;
        }
        public void dead()
        {
            this.status = Status.DEAD;
        }
        public bool isDead()
        {
            return this.status == Status.DEAD;
        }

        public void inLobby()
        {
            this.status = Status.WAITING;
        }
        public void ready()
        {
            this.status = Status.READY;
        }
        public void startGame()
        {
            this.status = Status.ALIVE;
        }
        public void update(float x, float z)
        {
            this.x = x;
            this.z = z;
        }
    }

    // A bit incomplete as I'm not sure how you guys want it
    public class Bomb
    {
        public enum Status { NONE, PENDING, TICKING, DESTROYED };
        public Status status = Status.NONE;
        public int ACKs = 0;
        public float x, z;
        public int strength;

        public Bomb(float x, float z, int strength)
        {
            status = Status.PENDING;
            this.x = x;
            this.z = z;
            this.strength = strength;
        }

        // Add to the total number of players who have
        // gotten the signal to plant the bomb
        // then return the new incremented number
        public int ack()
        {
            return ++ACKs;
        }

        public bool isReady(int numOfPlayers)
        {
            if (ACKs == numOfPlayers)
            {
                status = Status.TICKING;
                return true;
            }
            return false;
        }

        public void blowUp()
        {
            status = Status.DESTROYED;
        }
    }

    public void lobby()//whenever theres anyone
    {
        for (int i = 0; i < getplayerCount(); i++)
        {
            //send to all
            if (allPlayers[i] == null)
            {
                break;
            }
            string package = "P" + (i + 1) + "L: " + allPlayers[i].username
                + "|" + allPlayers[i].x + "|" + allPlayers[i].z;
            AsynchronousSocketListener.sendALL(package);
        }
    }

    public void addBomb(float x, float z, int strength)
    {
        allBombs.Add(new Bomb(x, z, strength));
    }

    public Player getPlayer(int index)
    {
        return allPlayers[index];
    }

    public void addPlayer(Socket ip, string user)
    {
        //grab one of these default locations
        //9,9
        //9,-9
        //-9,-9
        //-9,9
        float x = 0; float z = 0;//filler for now
        switch (nextindex)
        {
            case 0:
                x = 9; z = 9;
                break;
            case 1:
                x = 9; z = -9;
                break;
            case 2:
                x = -9; z = -9;
                break;
            case 3:
                x = -9; z = 9;
                break;
            default:
                break;
        }
        addPlayer(ip, x, z, user);//-1 for now
    }

    public void addPlayer(Socket ip, float x, float z, string user)
    {
        if (nextindex != 4)//no more after position 3
        {
            allPlayers.Add(new Player(ip, x, z, nextindex, user));
            //Console.WriteLine(user);
            nextindex++;
            lobby();//have the server send player info back to all clients
            //return nextindex - 1;//send index to the client?
        }
    }

    public int getplayerCount()
    {
        return allPlayers.Count;// allPlayers.Count;
    }

    public void playerready(int index)
    {
        getPlayer(index).ready();
        readycount++;
        //send message to all players of who's ready
        for (int i = 0; i < getplayerCount() && allPlayers[i] != null; i++)
        {
            AsynchronousSocketListener.lazySend("P" + (i + 1) + "R: ready");
        }
        checkready();
    }
    public void playerNotready(int index)
    {
        getPlayer(index).inLobby();
        readycount = (readycount == 0 ? 0 : readycount-1) ;
        for (int i = 0; i < getplayerCount() && allPlayers[i] != null; i++)
        {
            AsynchronousSocketListener.lazySend("P" + (i + 1) + "R: not ready");
        }
    }
    public void checkready()
    {
        if (readycount >= 2 && readycount == getplayerCount())
        {
            //start countdown??
            Console.WriteLine("Game Starting...");
            GameLoop();
            //after countdown
            Console.WriteLine("Game has ended...????");
        }
    }

    public Player findPlayer(Socket client)
    {
        foreach (Player player in allPlayers)
        {
            if (player.clientSocket == client)
            {
                return player;
            }
        }
        Console.WriteLine("Your Player object cannot be found. So here is a null.");
        return null;
    }

    public void sendGameOver(int winner)
    {
        string package = "Game Over;";
        if (winner != -1)
        {
            package += (winner.ToString() + MessageHandler.semicolon);
        }
        sendToAll(package);
        status = Status.ENDED;
    }

    public void checkGameStatus()
    {
        int numOfPlayers = getplayerCount();
        int winner = -1;
        for (int i = 0; i < getplayerCount(); i++)
        {
            if (allPlayers[i].isDead())
            {
                numOfPlayers--;
                continue;
            }
            winner = i;
        }
        switch (numOfPlayers)
        {
            case 1: // we have a winner. not sure who it is though.
                Console.WriteLine("Player " + (winner + 1) + " won!");
                sendGameOver(winner);
                break;
            case 0: // everybody died lulz
                Console.WriteLine("Everybody died! Oh noes!");
                sendGameOver(-1);
                break;
            default: break;
        }
    }
}
//--Anthony--added the database connection
public class DatabaseHandler
{
    private static MySqlConnection connect;
    private string server;
    private string db;
    private string serveruser;
    private string serverpass;

    public DatabaseHandler()
    {
        server = IP.mySQL;
        //server = "169.234.20.168";
        //server = "127.0.0.1";
        db = "BombermanDB";
        serveruser = "root";
        serverpass = "master";
        string connectionstring = "SERVER=" + server + ";PORT = 3306;"/* DATABASE=" + db + ";"*/ + "user id="
                + serveruser + ";" + "PASSWORD=" + serverpass + ";" + "connection timeout=30;";
        connect = new MySqlConnection(connectionstring);
    }
    private static bool OpenConnection()
    {
        try
        {
            connect.Open();
            Console.WriteLine("Database Connected");
            return true;
        }
        catch (MySqlException x)
        {
            Console.WriteLine(x.Message);
            return false;
        }
    }
    private static bool CloseConnection()
    {
        try
        {
            connect.Close();
            return true;
        }
        catch (MySqlException x)
        {
            Console.WriteLine(x.Message);
            return false;
        }
    }
    public static bool adduser(string user, string pass)
    {
        if (OpenConnection())
        {
            string query = "INSERT INTO bmdb.main (username, pass) VALUES (@username,@password)";
            MySqlCommand cmd = new MySqlCommand(query, connect);
            cmd.Parameters.Add("@username", user);
            cmd.Parameters.Add("@password", pass);
            int result = cmd.ExecuteNonQuery();
            CloseConnection();
            return result > 0;
        }
        return false;
    }
    /*//don't need this yet maybe
    public void updateinfo(string user)
    {

    }*/


    //method checks the database if the user and password are correct
    //need to update once its correct so no more than one person can long on at once
    public static bool verify(string user, string pass)
    {
        if (OpenConnection())
        {
            string[] list = new string[2];
            string query = "select * from bmdb.main where username = '" + user + "'";
            MySqlCommand cmd = new MySqlCommand(query, connect);
            MySqlDataReader reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                list[0] = reader.GetString("username");
                list[1] = reader.GetString("pass");
            }
            reader.Close();
            Console.WriteLine(list[0] + "\n" + list[1]);
            CloseConnection();
            return (user == list[0] && pass == list[1]);
        }
        return false;
    }
}


// The class that handles messages. All methods are currently static.
public class MessageHandler
{
    // Currently all un-parsed messages
    public static List<Message> allMessages = new List<Message>();

    public static List<Game> games = new List<Game>();
    public static int count = 0;//every 4, create/add a new Game object

    public static string[] EOF = new string[] { "<EOF>" };
    public static char semicolon = ';';

    public class Message
    {
        public Socket client;
        public string message;
        public List<string> messageParts;

        public Message(Socket client, string message)
        {
            this.client = client;
            this.message = message;
            this.messageParts = new List<string>();
        }

        public List<string> split()
        {
            string[] strings = message.Split(semicolon);
            foreach (string s in strings)//Null reference exception here
            {
                if (s != "")
                {
                    messageParts.Add(s);
                }
            }
            return messageParts;
        }
    }

    // Call this to add a new message to the processing list
    public static void addMessage(Socket client, string message)
    {
        allMessages.Add(new Message(client, message));
    }

    private static void attemptingLogin(Message m)
    {
        //parse the string for user and pass
        //Attempting Login: username|password<EOF>
        string user = "";
        string pass = "";
        string msg = m.message.Substring(18);//username|password<EOF>
        int index = msg.IndexOf("|");
        user = msg.Substring(0, index);//username

        msg = msg.Substring(index + 1);//password<EOF> this doesnt
        //index = msg.IndexOf("<");//this works
        pass = msg; //.Substring(0);

        //Console.WriteLine("Username: "+user + "\n" +"Password: "+ pass);

        //check the database with the user and pass
        if (DatabaseHandler.verify(user, pass))
        {
            AsynchronousSocketListener.lazySend("Login Success " + user);
            //add player object here?
        }
        else
        {
            AsynchronousSocketListener.lazySend("Login Failed");
        }
        //AsynchronousSocketListener.lazySend("Login Success<EOF>");
    }

    private static void registering(Message m)
    {

        string msg = m.message.Substring(13);//username|password<EOF>
        int index = msg.IndexOf("|");
        string user = msg.Substring(0, index);
        msg = msg.Substring(index + 1);
        //index = msg.IndexOf("<");
        string pass = msg; //.Substring(0, index);
        Console.WriteLine(user + "\n" + pass);
        if (DatabaseHandler.adduser(user, pass))
        {
            AsynchronousSocketListener.lazySend("Registered " + user);
            //add player object here?
        }
        else
        {
            AsynchronousSocketListener.lazySend("Invalid Registery");
        }
    }

    private static void awaitingGame(Message m)
    {
        //move the game state to lobby
        //have the game add the player who sent the msg
        //need the username and the ip at minimum, need to probably make a list
        //of some sort of client object with ip/socket and username
        //game will send messages about the other players within the same lobby
        m.message = m.message.Substring(14);
        
        //IPAddress.Parse(((IPEndPoint)m.client.RemoteEndPoint).Address.ToString());
        games[games.Count - 1].addPlayer(m.client, m.message); //.Substring(0, m.message.IndexOf("<")));
        count++;
        if (count % 4 == 0)//reaches 4 players
        {
            games.Add(new Game());
            count = 0;
        }
    }
    static bool started = false; // DELETE
    private static void updatePlayerLocation(Message m) // for player location: Player;index;T/F;x;z;
    {
        try
        {
            string header = m.messageParts[0];
            //string playerNum = m.messageParts[1]; // discarded because Server knows who it came from
            string deadOrAlive = m.messageParts[2];
            float x = Convert.ToSingle(m.messageParts[3]);
            float z = Convert.ToSingle(m.messageParts[4]);
            Game game = games[0]; // #hardcoding lyfe
            Game.Player player = game.findPlayer(m.client);

            if (header != "Player")
            {
                return;
            }
            switch (deadOrAlive)
            {
                case "T":
                    player.alive();
                    break;
                case "F":
                    player.dead();
                    game.checkGameStatus();
                    break;
                default: return;
            }
            player.x = x;
            player.z = z;
        }
        catch (Exception e)
        {
            return;
        }
        if (started == false) // DELETE
        { games[0].GameLoop(); started = true; }
    }

    private static void bombProposal(Message m) // for bomb proposal: Bomb;x;z;strength;
    {
        try
        {
            string header = m.messageParts[0];
            float x = Convert.ToSingle(m.messageParts[1]);
            float z = Convert.ToSingle(m.messageParts[2]);
            int strength = Convert.ToInt32(m.messageParts[3]); // might break
            Game game = games[0]; // #hardcoding lyfe

            if (header != "Bomb")
            {
                return;
            }

            // Implement here. No need to receive ACKs for now.
            // The bomb will be sent out to other players soon, but not here.
            game.addBomb(x, z, strength);
        }
        catch (Exception e)
        {
            return;
        }
    }

    /*private static void bombACK(Message m) // for ACKing bomb: ACK;B;10;20;[strength];
    {

    }*/

    private static void playerReady(Message m)
    {
        int index = int.Parse(m.message.Substring(m.message.Length - 1));
        games[games.Count - 1].playerready(index);
    }
    private static void playerisnotready(Message m)
    {
        int index = int.Parse(m.message.Substring(m.message.Length - 1));
        games[games.Count - 1].playerNotready(index);
    }

    private static void cleanEOF(Message m)
    {
        // Separate all Message contents by <EOF> tag.
        string[] messages = m.message.Split(EOF, StringSplitOptions.None);
        if (messages.Length > 0)
        {
            foreach (string message in messages)
            {
                addMessage(m.client, message);
            }
        }
        allMessages.Remove(m);
    }

    // Process/parse all the messages received
    public static void processMessages()
    {
        games.Add(new Game());//hopefully this is only called once when the thread starts
        Console.WriteLine("A new game has been added. If you are seeing me for the second time, something went wrong.");
        while (true)
        {
            while (allMessages.Count > 0)
            {
                // Parse the first message that does not include an EOF tag.
                // If all messages have EOF tag, clean it.
                Message m = allMessages[0];
                /*foreach (Message message in allMessages)
                {
                    if ((message.message != "") && (!message.message.Contains("<EOF>")))
                    {
                        m = message;
                        break;
                    }
                }*/

                if (m != null)
                {
                    if (m.message.Contains("<EOF>"))
                    {
                        cleanEOF(m);
                        continue;
                    }
                    if (m.message == "")
                    {
                        allMessages.Remove(m);
                        continue;
                    }

                    // This is where you put if-statements for message contents, or calls
                    // to other soon-to-be-written-hopefully methods to keep it clean.
                    Console.WriteLine("Message to be parsed: " + m.message + "\n");

                    try
                    {
                        if (m.message.Contains("Attempting Login: "))
                        {
                            attemptingLogin(m);
                        }
                        else if (m.message.Contains("Registering: "))//Registering: username|password<EOF>
                        {
                            registering(m);
                        }
                        else if (m.message.Contains("Awaiting Game"))//Awaiting Game username<EOF>
                        {
                            awaitingGame(m);
                        }
                        else if (m.message.Contains("This player is ready "))
                        {
                            playerReady(m);
                        }
                        else if (m.message.Contains("This player is not ready "))
                        {
                            playerisnotready(m);
                        }
                        else if (m.message.Contains(";"))//happens on "This is a test" beginning messages, changing
                        {
                            m.split();//Null reference exception

                            if (m.messageParts[0] == "Player") // Player just sent you a location, server-senpai!
                            {
                                updatePlayerLocation(m);
                            }
                            else if (m.messageParts[0] == "Bomb") // Bomb proposal, be nice and approve asap
                            {
                                bombProposal(m);
                            }
                            /*else if (m.message.Substring(0, 5) == "ACK;B") // One more player placed the proposed bomb
                            {
                                bombACK(m);
                            }*/
                        }
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e.ToString());
                        Console.WriteLine("Message discarded without parsing: " + m.message);
                    }
                    finally
                    {
                        allMessages.Remove(m);
                    }
                }
            }
        }
    }

}

public class AsynchronousSocketListener
{
    // Thread signal.
    public static AutoResetEvent allDone = new AutoResetEvent(false);

    // List of all the clients, it should be changed into
    // a Dictionary<[IP address], [Sockets]> later on.
    public static List<Socket> allClients = new List<Socket>();

    private static Socket listener;
    private static IPHostEntry ipHostInfo;
    private static IPAddress ipAddress;
    private static IPEndPoint localEndPoint;
    private static IPEndPoint anyEndPoint;


    public AsynchronousSocketListener()
    {

    }

    // This is called only once. Don't look back. Keep moving.
    public static void StartListening()
    {
        // Data buffer for incoming data.
        byte[] bytes = new Byte[1024];

        // Create a TCP/IP socket.
        listener = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

        //Listen to external IP address
        ipHostInfo = Dns.GetHostEntry(Dns.GetHostName());
        ipAddress = ipHostInfo.AddressList[0];
        localEndPoint = new IPEndPoint(ipAddress, 11000);

        // Listen to any IP Address
        anyEndPoint = new IPEndPoint(IPAddress.Any, 11000);

        //setup a game, one at the beginning
        //allGames.Add(new Game());

        // Bind the socket to selected endpoint and listen/"wait" for incoming connections.
        try
        {
            listener.Bind(anyEndPoint);
            listener.Listen(100);

            while (true)
            {
                // Start an asynchronous socket to listen for connections.
                Console.WriteLine("Waiting for a connection..");

                // This technically means to wait for a client to connect.
                listener.BeginAccept(new AsyncCallback(AcceptCallback), listener);

                // Wait until a connection is made before continuing.
                allDone.WaitOne();

                // At this point, a first connection has been established.

            }

        }
        catch (Exception e)
        {
            Console.WriteLine(e.ToString());
        }

        Console.WriteLine("\nPress ENTER to continue...");
        Console.Read();

    }

    // When a new Client connects
    public static void AcceptCallback(IAsyncResult ar)
    {
        // Signal the main thread to continue.
        allDone.Set();

        // Get the socket that handles the client request.
        Socket listener = (Socket)ar.AsyncState;
        Socket handler = listener.EndAccept(ar);

        // Create the state object.
        StateObject state = new StateObject();
        state.workSocket = handler;

        // Games have bidirectional communication (as opposed to request/response)
        // So I need to store all clients sockets so I can send them messages later
        // TODO: store in meaningful way,such as Dictionary<string,Socket>
        allClients.Add(handler);
        //add to the Game instance, a new player object
        Console.WriteLine("New Client connected.");

        // Now, start "waiting" to receive messages.
        handler.BeginReceive(state.buffer, 0, StateObject.BufferSize, 0, new AsyncCallback(ReadCallback), state);
    }

    // When a message is received
    public static void ReadCallback(IAsyncResult ar)
    {
        String content = String.Empty;

        // Retrieve the state object and the handler socket from the asynchronous state object.
        StateObject state = (StateObject)ar.AsyncState;
        listener = state.workSocket;

        // Read data from the client socket.
        int bytesRead = listener.EndReceive(ar);

        if (bytesRead > 0)
        {
            // There  might be more data, so store the data received so far.
            state.sb.Append(Encoding.ASCII.GetString(state.buffer, 0, bytesRead));

            // Check for end-of-file tag. If it is not there, read more data.
            content = state.sb.ToString();
            if (content.IndexOf("<EOF>") > -1)
            {
                // All the data has been read from the client, add into the message processing list.
                Console.WriteLine("Read {0} bytes from socket." /*\n Data : {1}"*/, content.Length, content);
                /*IPAddress IP = IPAddress.Parse(((IPEndPoint)listener.RemoteEndPoint).Address.ToString());
                IPAddress IP2 = IPAddress.Parse(((IPEndPoint)listener.LocalEndPoint).Address.ToString());
                Console.WriteLine("\nRemoteIP: " + IP + ", LocalIP: " + IP2 + "\n");*/

                // I haven't figured out how to retrieve the IP yet, so it's "content, content" for now.
                // It should be "IP, content" later. #HalpIsNeeded.

                //Console.WriteLine("Message Received: " + content);

                MessageHandler.addMessage(listener, content);

                //Console.WriteLine("\n\n");
                //** Echo the data back to the client.**
                // (Probably not necessary, but play around with it.)
                //Send(listener, "From Server: "+content);

                // Setup a new state object
                StateObject newstate = new StateObject();
                newstate.workSocket = listener;
                // Call BeginReceive with a new state object,
                // pretty much means to continue receiving new messages and don't stop here.
                // There is no active while loop, so if this line is omitted,
                // server will stop being on stand-by for the messages.
                listener.BeginReceive(newstate.buffer, 0, StateObject.BufferSize, 0, new AsyncCallback(ReadCallback), newstate);
            }
            else
            {
                // Not all data received. Get more.
                listener.BeginReceive(state.buffer, 0, StateObject.BufferSize, 0, new AsyncCallback(ReadCallback), state);
            }
        }
    }

    // Self-explanatory. Send message. Although this is more of a helper function.
    private static void Send(Socket handler, String data)
    {
        // Convert the string data to byte data using ASCII encoding.
        byte[] byteData = Encoding.ASCII.GetBytes(data);

        // This call is the crucial moment...
        // Begin sending the data to the remote device.
        handler.BeginSend(byteData, 0, byteData.Length, 0, new AsyncCallback(SendCallback), handler);
    }

    // The actual sending is here.
    private static void SendCallback(IAsyncResult ar)
    {
        try
        {
            // Retrieve the socket from the state object.
            Socket handler = (Socket)ar.AsyncState;

            // Complete sending the data to the remote device.
            int bytesSent = handler.EndSend(ar);
            //Console.WriteLine("Sent {0} bytes to client.", bytesSent);
        }
        catch (Exception e)
        {
            Console.WriteLine(e.ToString());
        }
    }

    public static void lazySend(String content)
    {
        Send(listener, content + "<EOF>");
        Console.WriteLine("Sending: " + content + "<EOF>");
    }

    public static void directedSend(Socket target, string content)
    {
        Send(target, content);
        Console.WriteLine("Message: " + content + " was sent to " + IPAddress.Parse(((IPEndPoint)listener.RemoteEndPoint).Address.ToString()));
    }

    public static void sendALL(string content)
    {
        foreach (Socket s in allClients)
        {
            /*Console.WriteLine("Sending to a Client:");
            IPAddress IP = IPAddress.Parse(((IPEndPoint)s.RemoteEndPoint).Address.ToString());
            IPAddress IPl = IPAddress.Parse(((IPEndPoint)s.LocalEndPoint).Address.ToString());
            IPAddress IP2 = IPAddress.Parse(((IPEndPoint)listener.RemoteEndPoint).Address.ToString());
            IPAddress IP2l = IPAddress.Parse(((IPEndPoint)listener.LocalEndPoint).Address.ToString());
            Console.WriteLine("REMOTE\nIP: " + IP + "\nListener: " + IP2);
            Console.WriteLine("LOCAL\nIP: " + IPl + "\nListener: " + IP2l);
            Console.WriteLine();*/
            lazySend(content);
        }
    }

    public static int Main(String[] args)
    {
        // Thread #1 for the server to keep listening
        Thread listenThread = new Thread(StartListening);
        listenThread.Start();

        // Thread #2 for the message handler to start parsing messages
        Thread messageThread = new Thread(MessageHandler.processMessages);
        messageThread.Start();

        DatabaseHandler db = new DatabaseHandler();



        // This is for testing purposes only. Delete or comment out after done.
        /*Console.WriteLine("After you have established a connection...");
        int timesSent = 0;
        while (true)
        {
            Console.WriteLine("Please press enter to send a message to the client.");
            Console.ReadLine();

            // send test message.
            lazySend("Heyyos Client. This is my " + ++timesSent + "th time!");

            Console.WriteLine("You have sent " + timesSent + " messages.");

        }*/
        // END of timer implementation

        // Feel free to add more threads here to have parallel loops/tasks
        // or just to write more functions and other things.
        // But remember if you put it here you'll have to assume that
        // it works without needing a connection. In other words,
        // Threads #1 and #2 aren't dependent on each other since they
        // are separate threads!

        return 0;
    }
}

using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Collections.Generic;
using MySql.Data.MySqlClient;


public class IP
{
    public const string Anthony = "169.234.2.124";
    public const string Faye = "169.234.9.207";
    public const string Jeffrey = "169.234.22.25";
    public const string defaultIP4Anthony = "127.0.0.1";
    public const string mySQL = defaultIP4Anthony;
}

public class Settings
{
    // Turn off database here!
    public static bool database = true;
}


// State object for reading client data asynchronously
public class StateObject
{
    // Client socket.
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
    private bool full;

    public Game()
    {
        allPlayers = new List<Player>();
        allBombs = new List<Bomb>();
        nextindex = 0;//starting position, when one player is added: next position is 1
        readycount = 0;
        full = false;
    }
    public bool isFull()
    {
        full = (allPlayers.Count == 4);
        return full;
    }

    private void sendToAll(string package)
    {
        //AsynchronousSocketListener.sendALL(package);
        for (int i = 0; i < allPlayers.Count && allPlayers[i] != null; i++)
        {
            AsynchronousSocketListener.directedSend(allPlayers[i].clientSocket, package);
        }
    }

    public void sendPosition() // Player;0;T;9;8;0end;1;T;8;9;1end;2;T;9;7;2end;3;F;9;6;3end;
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
            //jeffrey velocity values
            string xv = allPlayers[i].xv.ToString() + MessageHandler.semicolon;
            string zv = allPlayers[i].zv.ToString() + MessageHandler.semicolon;
            string footer = i.ToString() + "end" + MessageHandler.semicolon;
            package += header + deadOrAlive + x + z + xv + zv+ footer;
        }
        sendToAll(package);
    }

    // If there are still bombs in the list, send them out and delete.
    public void sendBombs() // Bomb;x;z;strength;
    {
        string header = "Bomb;";
        while (allBombs.Count != 0)
        {
            Bomb bomb = allBombs[0];
            string x = bomb.x.ToString() + MessageHandler.semicolon;
            string z = bomb.z.ToString() + MessageHandler.semicolon;
            string strength = bomb.strength.ToString() + MessageHandler.semicolon;

            string package = header + x + z + strength;

            sendToAll(package);
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
        public float x, z, xv, zv;

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
    public void removePlayer(string user)
    {
       // Console.WriteLine(">>>\tDisconnect request received by: " + user);
        Player p = null;
        foreach(Player ps in allPlayers)
        {
            if (ps.username == user)
            {p = ps;}
        }

        if (p == null)
        {return;}
        Console.WriteLine("Player: " + user + " found, now removing");
        p.status = Player.Status.DEAD;

        allPlayers.Remove(p);//check the gameover scenario
        sendToAll("Lobby Refresh");
        lobby();
    }
    public void lobby()//whenever theres anyone
    {
        for (int i = 0; i < allPlayers.Count && i < 4; i++)
        {
            string package = "P" + (i + 1) + "L:...";//"P1L:..."
            //send to all
            if (allPlayers[i] != null)
            {   
                package = "P" + (i + 1) + "L: " + allPlayers[i].username
                + "|" + allPlayers[i].x + "|" + allPlayers[i].z;
            }
            sendToAll(package);
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
        float x = 0; float z = 0;//filler for now
        switch (nextindex)
        {
            case 0:
                x = -5; z = -7;
                break;
            case 1:
                x = 5; z = 7;
                break;
            case 2:
                x = 5; z = -7;
                break;
            case 3:
                x = -5; z = 7;
                break;
            default:
                break;
        }
        addPlayer(ip, x, z, user);
    }

    public void addPlayer(Socket ip, float x, float z, string user)
    {
        if (nextindex < 4)//no more after position 3
        {
            allPlayers.Add(new Player(ip, x, z, nextindex, user));
            nextindex++;
        }
    }

    public int getplayerCount()
    {
        return allPlayers.Count;// allPlayers.Count;
    }
    public void refresh()
    {
        lobby();
    }
    public void playerready(int index)
    {
        Player p = getPlayer(index);
        p.ready();
        readycount++;
        //send message to all players of who's ready
        sendToAll("P" + (index + 1) + "R: ready");
        checkready();
    }
    public void playerNotready(int index)
    {
        Player p = getPlayer(index);
        p.inLobby();
        readycount = (readycount == 0 ? 0 : readycount-1) ;
        sendToAll("P" + (index + 1) + "R: not ready");
    }
    public void checkready()
    {
        if (readycount >= 2 && readycount == getplayerCount())//game is ready to start
        {
            Console.WriteLine("\n-----------------------Game Starting----------------------------\n");
            this.status = Game.Status.PLAYING;
            //AsynchronousSocketListener.sendALL("Game Start" + MessageHandler.semicolon + allPlayers.Count);
            sendToAll("Game Start" + MessageHandler.semicolon + allPlayers.Count);
            MessageHandler.clearmessages();
        }
    }
    public bool playerExists(string user)
    {
        for (int i = 0; i < allPlayers.Count && allPlayers[i] != null; i++)
        {
            if (allPlayers[i].username == user)
            {
                return true;
            }
        }
        return false;
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
        readycount = 0;
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

        if (numOfPlayers <= 1) // there is one or fewer player standing, winner?
        {
            switch (numOfPlayers)
            {
                case 1: // we have a winner. not sure who it is though.
                    Console.WriteLine("------------------------Game Over!----------------------------");
                    Console.WriteLine("Player " + (winner /*+1*/) + " won!");
                    sendGameOver(winner);
                    break;
                case 0: // everybody died lulz
                    Console.WriteLine("Everybody died! Oh noes!");
                    sendGameOver(-1);
                    break;
                default: break;
            }

            // Update each player info in the database
            foreach(Player player in allPlayers)
            {
                bool isWinner = (player.index == winner);
                DatabaseHandler.updateinfo(player.username, isWinner);
            }
        }
    }
}
//--Anthony--added the database connection
public class DatabaseHandler
{
    private static MySqlConnection connect;
    private string server;
    private string serveruser;
    private string serverpass;

    public DatabaseHandler()
    {
        if (!Settings.database)
        {
            return;
        }

        //server = IP.mySQL;
        //server = "169.234.20.168";
        //server = "70.187.161.177";
        server = IP.mySQL;
        serveruser = "root";
        serverpass = "master";
        string connectionstring = "SERVER=" + server + ";PORT = 3306;"/* DATABASE=" + db + ";"*/ + "user id="
                + serveruser + ";" + "PASSWORD=" + serverpass + ";" + "connection timeout=30;";
        connect = new MySqlConnection(connectionstring);
    }
    private static bool OpenConnection()
    {
        if (!Settings.database)
        {
            return true;
        }

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
        if (!Settings.database)
        {
            return true;
        }

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
        if (!Settings.database)
        {
            return true;
        }

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
    //don't need this yet maybe
    public static bool updateinfo(string user, bool won)
    {
        if (!Settings.database)
        {
            return true;
        }

        if (OpenConnection())
        {
            int wins = 0;
            int games = 0;

            // Retrieve existing information from given user
            string query = String.Format("SELECT * from bmdb.main WHERE username = '{0}'", user);
            MySqlCommand cmd = new MySqlCommand(query, connect);

            // Load existing 'wins' and 'games' values into variables
            MySqlDataReader reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                int winsIndex = reader.GetOrdinal("wins");
                if (!reader.IsDBNull(winsIndex))
                {
                    wins = reader.GetInt32("wins");
                }

                int gamesIndex = reader.GetOrdinal("games");
                if (!reader.IsDBNull(gamesIndex))
                {
                    games = reader.GetInt32("games");
                }
            }
            reader.Close();

            //Console.WriteLine("JUST RETRIEVED DATA, wins={0}, games={1}", wins, games);

            // If this user won the game, increment 'wins',
            // also increment 'games' either way
            if (won)
            {
                wins++;
            }
            games++;

            //Console.WriteLine("BEFORE UPDATING, wins={0}, games={1}", wins, games);

            // Update back the new information through query
            query = String.Format("UPDATE bmdb.main SET wins={0}, games={1} WHERE username='{2}'", wins, games, user);
            cmd = new MySqlCommand(query, connect);

            // Check if the update was successful
            int result = cmd.ExecuteNonQuery();
            CloseConnection();

            //Console.WriteLine("Result > 0 ?  {0}", (result>0));
            return result > 0;
        }
        return false;
    }


    //method checks the database if the user and password are correct
    //need to update once its correct so no more than one person can long on at once
    public static bool verify(string user, string pass)
    {
        if (!Settings.database)
        {
            return true;
        }

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

    //public static List<Game> games = new List<Game>();
    public static Dictionary<string, Game> games = new Dictionary<string, Game>();
    //public static int count = 0;//every 4, create/add a new Game object

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
        //Console.WriteLine("New Message Added, Total: "+allMessages.Count + ", index: " + (allMessages.Count-1));
    }
    public static void clearmessages()
    {
        allMessages.Clear();
        //Console.WriteLine("Cleared: "+allMessages.Count);
    }
    private static void attemptingLogin(Message m)
    {
        //parse the string for user and pass
        //Attempting Login: username|password<EOF>
        Console.WriteLine("Trying to work");
        string user = "";
        string pass = "";
        string msg = m.message.Substring(18);//username|password<EOF>
        int index = msg.IndexOf("|");
        user = msg.Substring(0, index);//username

        msg = msg.Substring(index + 1);//password<EOF> this doesnt
        //index = msg.IndexOf("<");//this works
        pass = msg; //.Substring(0);

        AsynchronousSocketListener.Send(m.client, "Login Success " + user);

        //check the database with the user and pass
        
        if (DatabaseHandler.verify(user, pass))
        {
            AsynchronousSocketListener.Send(m.client,"Login Success " + user);//need this line for logins to work
            //add player object here?
        }
        else
        {
            AsynchronousSocketListener.Send(m.client,"Login Failed");
            AsynchronousSocketListener.DisconnectClient(m.client);
        }
         
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
            AsynchronousSocketListener.Send(m.client,"Registered " + user);
            //add player object here?
        }
        else
        {
            AsynchronousSocketListener.Send(m.client,"Invalid Registery");
        }
    }

    private static void awaitingGame(Message m)//"Awaiting Game "+ username + "|" + lobbyname
    {
        //move the game state to lobby
        //have the game add the player who sent the msg
        //need the username and the ip at minimum, need to probably make a list
        //of some sort of client object with ip/socket and username
        //game will send messages about the other players within the same lobby
        string username="";
        string lobbyname="";

        m.message = m.message.Substring(14);
        int index = m.message.IndexOf("|");
        username = m.message.Substring(0,index);
        lobbyname = m.message.Substring(index + 1);
        Console.WriteLine("LobbyName: " + lobbyname);
      /*  if (games[lobbyname].playerExists(username))
        {
            games[lobbyname].refresh();
            return;
        }*/
        games[lobbyname].lobby();
    }
    //static bool started = false; // DELETE
    private static void updatePlayerLocation(Message m) // for player location: Player;index;T/F;x;z;xvel;zvel;lobbyname
    {
        try
        {
            string header = m.messageParts[0];
            //string playerNum = m.messageParts[1]; // discarded because Server knows who it came from
            string deadOrAlive = m.messageParts[2];
            //location values
            float x = Convert.ToSingle(m.messageParts[3]);
            float z = Convert.ToSingle(m.messageParts[4]);
            
            //velocity values
            float xv = Convert.ToSingle(m.messageParts[5]);
            float zv = Convert.ToSingle(m.messageParts[6]);

            string lobbyname = m.messageParts[7];
            Game game = games[lobbyname]; // #hardcoding lyfe

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
            player.xv = xv;
            player.zv = zv;
            games[lobbyname].sendPosition();

         //   games[0].sendPosition();
        }
        catch (Exception e)
        {
            Console.WriteLine(e.ToString());
            return;
        }
    }

    private static void bombProposal(Message m) // for bomb proposal: Bomb;x;z;strength;lobbyname
    {
        try
        {
            string header = m.messageParts[0];
            float x = Convert.ToSingle(m.messageParts[1]);
            float z = Convert.ToSingle(m.messageParts[2]);
            int strength = Convert.ToInt32(m.messageParts[3]); // might break
            string lobbyname = m.messageParts[4];
            Game game = games[lobbyname]; // #hardcoding lyfe

            if (header != "Bomb")
            {
                return;
            }

            // Implement here. No need to receive ACKs for now.
            // The bomb will be sent out to other players soon, but not here.
            game.addBomb(x, z, strength);
            game.sendBombs();
        }
        catch (Exception e)
        {
            Console.WriteLine(e.ToString());
            return;
        }
    }

    private static void findgame(Message m)//"Find game: " + lobbyname
    {
        string lobbyname = m.message.Substring(11);
        Console.WriteLine("Trying to find game with name ... " + lobbyname);
        if (games.ContainsKey(lobbyname))
        {
            AsynchronousSocketListener.Send(m.client, "Game Found");
        }
        else
        {
            AsynchronousSocketListener.Send(m.client, "Game DNE");
        }
    }
    private static void makegame(Message m)//"New Game " + lobbyname + "|" + username
    {
        m.message = m.message.Substring(9);
        Console.WriteLine("\nmessage after substring " + m.message);
        int index = m.message.IndexOf("|");
        string lobbyname = m.message.Substring(0,index);
        string username = m.message.Substring(index + 1);

        if (games.ContainsKey(lobbyname))
        {
            AsynchronousSocketListener.Send(m.client, "Game Exists Already");
        }
        else
        {
            //create the lobby and add this player to it
            games.Add(lobbyname,new Game());
            games[lobbyname].addPlayer(m.client, username);
            AsynchronousSocketListener.Send(m.client, "Game Created " + lobbyname);
        }
    }
    private static void joingame(Message m)//"Joining make|anthony"
    {
        m.message = m.message.Substring(8);
        Console.WriteLine("\n'"+m.message+"'");//make|anthony
        int index = m.message.IndexOf("|");
        Console.WriteLine(index);
        string lobbyname = m.message.Substring(0,index);
        string username = m.message.Substring(index + 1);
        Console.WriteLine("'"+lobbyname+"'");
        if (games[lobbyname].isFull())
        {
            AsynchronousSocketListener.Send(m.client, "Game FULL");
        }
        else
        {
            games[lobbyname].addPlayer(m.client, username);
            AsynchronousSocketListener.Send(m.client, "Join Approved " + lobbyname);
        }
    }
    private static void disconnectrequest(Message m)
    {
        //lobby, if it exists find it and remove the user
        //username
        string lobbyname = "";
        string username = "";
        //14
        m.message = m.message.Substring(14);//username|lobbyname
        int index = m.message.IndexOf("|");
        username = m.message.Substring(0, index);
        lobbyname = m.message.Substring(index + 1);
        Console.WriteLine(">>>\tDisconnect request received by: " + username);
        if (lobbyname.Length != 0 || games.ContainsKey(lobbyname))
        {
            Console.WriteLine(">>>\tPlayer was in a game");
            games[lobbyname].removePlayer(username);
        }
        AsynchronousSocketListener.DisconnectClient(m.client);//disconnect the client from the server list
        Console.WriteLine(">>>\tDisconnect complete");
    }
    private static void removefromlobby(Message m)
    {
        //10
        m.message = m.message.Substring(10);
        int index = m.message.IndexOf("|");
        string username = m.message.Substring(0, index);
        string lobby = m.message.Substring(index + 1);
        games[lobby].removePlayer(username);
        Console.WriteLine("User: " + username + " requested to leave lobby: " + lobby);
        if (games[lobby].allPlayers.Count == 0)
        {
            Console.WriteLine("Deleting lobby: " + lobby);
            games.Remove(lobby);
        }
    }
    /*private static void bombACK(Message m) // for ACKing bomb: ACK;B;10;20;[strength];
    {

    }*/

    private static void playerReady(Message m)//"This player is ready " + playerindex + "|" + lobbyname
    {
        m.message = m.message.Substring(21);
        int i = m.message.IndexOf("|");
        int index = int.Parse(m.message.Substring(0,i));
        string lobbyname = m.message.Substring(i + 1);
        games[lobbyname].playerready(index);
    }
    private static void playerisnotready(Message m)//"This player is not ready " + playerindex + "|" + lobbyname
    {
        m.message = m.message.Substring(25);
        int i = m.message.IndexOf("|");
        int index = int.Parse(m.message.Substring(0, i));
        string lobbyname = m.message.Substring(i + 1);
        games[lobbyname].playerNotready(index);
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
        //games.Add(new Game());//hopefully this is only called once when the thread starts
        Console.WriteLine("A new game has been added. If you are seeing me for the second time, something went wrong.");
        while (true)
        {
            while (allMessages.Count > 0)
            {
                // Parse the first message that does not include an EOF tag.
                // If all messages have EOF tag, clean it.
                Message m = allMessages[0];
                
                if (m != null)
                {
                    //   Console.WriteLine("Message Received: " + m.message);
                    if (m.message.Contains("<EOF>"))
                    {
                        //Console.WriteLine(m.message);
                        cleanEOF(m);
                        continue;
                    }
                    if (m.message == "")
                    {
                        allMessages.Remove(m);
                        //Console.WriteLine("Message removed(EMPTY): " + allMessages.Count);
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
                        else if (m.message.Contains("Awaiting Game"))
                        {
                            awaitingGame(m);
                        }
                        else if (m.message.Contains(";"))//happens on "This is a test" beginning messages, changing
                        {
                            m.split();//Null reference exception

                            if (m.messageParts[0] == "Player") // Player just sent you a location, server-senpai!
                            {
                                Console.WriteLine("\nPlayer Update");
                                updatePlayerLocation(m);
                            }
                            else if (m.messageParts[0] == "Bomb") // Bomb proposal, be nice and approve asap
                            {
                                Console.WriteLine("\nBomb Update");
                                bombProposal(m);
                            }
                            /*else if (m.message.Substring(0, 5) == "ACK;B") // One more player placed the proposed bomb
                            {
                                bombACK(m);
                            }*/
                        }
                        else if (m.message.Contains("Disconnect Me "))
                        {
                            disconnectrequest(m);
                        }
                        else if (m.message.Contains("Find game: "))
                        {
                            findgame(m);
                        }
                        else if (m.message.Contains("New Game "))
                        {
                            makegame(m);
                        }
                        else if (m.message.Contains("Joining "))
                        {
                            joingame(m);
                        }
                        else if (m.message.Contains("Remove me "))
                        {
                            removefromlobby(m);
                        }
                        else if (m.message.Contains("Registering: "))//Registering: username|password<EOF>
                        {
                            registering(m);
                        }
                        else if (m.message.Contains("This player is ready "))
                        {
                            playerReady(m);
                        }
                        else if (m.message.Contains("This player is not ready "))
                        {
                            playerisnotready(m);
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
                        //Console.WriteLine("Message removed: " + allMessages.Count);
                    }
                }
                else
                {
                   // Console.WriteLine("Message removed(NULL): " + allMessages.Count);
                    allMessages.Remove(m);
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
        Socket listener = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);//SERVER socket?

        //Listen to external IP address
        ipHostInfo = Dns.GetHostEntry(Dns.GetHostName());
        ipAddress = ipHostInfo.AddressList[0];
        localEndPoint = new IPEndPoint(ipAddress, 11000);//listen at this specific point

        // Listen to any IP Address
        anyEndPoint = new IPEndPoint(IPAddress.Any, 11000);//listen for ANY REMOTE device

        //setup a game, one at the beginning
        //allGames.Add(new Game());

        // Bind the socket to selected endpoint and listen/"wait" for incoming connections.
        try
        {
            listener.Bind(anyEndPoint);
            listener.Listen(100);//100 connections max

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

    public static void DisconnectClient(Socket client)
    {
        allClients.Remove(client);
    }

    // When a new Client connects
    public static void AcceptCallback(IAsyncResult ar)
    {
        // Signal the main thread to continue.
        allDone.Set();

        // Get the socket that handles the client request. <---HANDLES the client
        Socket listener = (Socket)ar.AsyncState;
        Socket handler = listener.EndAccept(ar); //<---THIS is the CLIENT socket

        /*IPAddress IP = IPAddress.Parse(((IPEndPoint)listener.RemoteEndPoint).Address.ToString());
                IPAddress IP2 = IPAddress.Parse(((IPEndPoint)listener.LocalEndPoint).Address.ToString());
                Console.WriteLine("\nRemoteIP: " + IP + ", LocalIP: " + IP2 + "\n");*/
        
        /*
        Console.WriteLine("\nHandlerRemote): " + IPAddress.Parse(((IPEndPoint)handler.RemoteEndPoint).Address.ToString()));
        Console.WriteLine("Handler(Local): " + IPAddress.Parse(((IPEndPoint)handler.LocalEndPoint).Address.ToString()) + "\n");*/

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
        Socket listener = state.workSocket;

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
                //Console.WriteLine("Read {0} bytes from socket." /*\n Data : {1}"*/, content.Length, content);
                
                // I haven't figured out how to retrieve the IP yet, so it's "content, content" for now.
                // It should be "IP, content" later. #HalpIsNeeded.

                Console.WriteLine("\nMessage Received: " + content);

                MessageHandler.addMessage(listener, content);

                /*Console.WriteLine("\nListener(Remote): " + IPAddress.Parse(((IPEndPoint)listener.RemoteEndPoint).Address.ToString()));
                Console.WriteLine("Listener(Local): " + IPAddress.Parse(((IPEndPoint)listener.LocalEndPoint).Address.ToString()) + "\n");*/
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
    public static void Send(Socket handler, String data)
    {
        data += "<EOF>";
        // Convert the string data to byte data using ASCII encoding.
        byte[] byteData = Encoding.ASCII.GetBytes(data);
        Console.WriteLine("Message: " + data + " was sent to " + IPAddress.Parse(((IPEndPoint)handler.RemoteEndPoint).Address.ToString()));
        // This call is the crucial moment...
        // Begin sending the data to the remote device. <---------------------TO the REMOTE device
        handler.BeginSend(byteData, 0, byteData.Length, 0, new AsyncCallback(SendCallback), handler);
    }

    // The actual sending is here.
    private static void SendCallback(IAsyncResult ar)
    {
        try
        {
            // Retrieve the socket from the state object.
            Socket handler = (Socket)ar.AsyncState;//<----Client Socket
       
            /*IPAddress IP = IPAddress.Parse(((IPEndPoint)listener.RemoteEndPoint).Address.ToString());
                IPAddress IP2 = IPAddress.Parse(((IPEndPoint)listener.LocalEndPoint).Address.ToString());
                Console.WriteLine("\nRemoteIP: " + IP + ", LocalIP: " + IP2 + "\n");*/

            // Complete sending the data to the remote device.
            int bytesSent = handler.EndSend(ar);
            //Console.WriteLine("Sent {0} bytes to client.", bytesSent);
        }
        catch (Exception e)
        {
            Console.WriteLine(e.ToString());
        }
    }
    /*
    public static void lazySend(String content)
    {
        Send(listener, content + "<EOF>");
        Console.WriteLine("Sending: " + content + "<EOF>");
    }*/

    public static void directedSend(Socket target, string content)
    {
        Send(target, content);
        //Console.WriteLine("Message: " + content + " was sent to " + IPAddress.Parse(((IPEndPoint)target.RemoteEndPoint).Address.ToString()));
    }

    public static void sendALL(string content)
    {
        foreach (Socket s in allClients)
        {
            directedSend(s,content);
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

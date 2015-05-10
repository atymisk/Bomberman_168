using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Collections.Generic;
using MySql.Data.MySqlClient;

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
        server = "localhost";
        db = "BombermanDB";
        serveruser = "root";
        serverpass = "root";
        string connectionstring = "SERVER=" + server /*+ ";" + "DATABASE=" + db*/ + ";" + "user id=" 
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
    public static void adduser(string user, string pass)
    {
        if (OpenConnection())
        {
            string query = "INSERT INTO bmdb.main (username, pass) VALUES (@username,@password)";
            MySqlCommand cmd = new MySqlCommand(query, connect);
            cmd.Parameters.Add("@username", user);
            cmd.Parameters.Add("@password", pass);
            cmd.ExecuteNonQuery();
            CloseConnection();
        }

    }
    /*//don't need this yet maybe
    public void updateinfo(string user)
    {
        
    }*/
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

    public class Message
    {
        public string IP;
        public string message;

        public Message(string IP, string message)
        {
            this.IP = IP;
            this.message = message;
        }
    }

    // Call this to add a new message to the processing list
    public static void addMessage(string IP, string message)
    {
        allMessages.Add(new Message(IP, message));
    }

    // Process/parse all the messages received
    public static void processMessages()
    {
        while (true)
        {
            while (allMessages.Count > 0)
            {
                Message m = allMessages[0];
                if (m != null)
                {
                    // This is where you put if-statements for message contents, or calls
                    // to other soon-to-be-written-hopefully methods to keep it clean.
                    Console.WriteLine(m.message+"\n");
                    if (m.message.Contains("Attempting Login: "))
                    {
                        //parse the string for user and pass
                        //Attempting Login: username|password<EOF>
                        string user = "";
                        string pass = "";
                        string msg = m.message;

                        msg = msg.Substring(18);//username|password<EOF>
                        int index = msg.IndexOf("|");
                        user = msg.Substring(0, index);//username

                        msg = msg.Substring(index+1);//password<EOF> this doesnt
                        index = msg.IndexOf("<");//this works
                        pass = msg.Substring(0, index);
                        
                        //Console.WriteLine("Username: "+user + "\n" +"Password: "+ pass);

                        //check the database with the user and pass
                        if (DatabaseHandler.verify(user, pass))
                        {
                            AsynchronousSocketListener.lazySend("Login Success<EOF>");
                        }
                        else
                        {
                            AsynchronousSocketListener.lazySend("Login Failed<EOF>");
                        }
                        //AsynchronousSocketListener.lazySend("Login Success<EOF>");
                    }
                    else if (m.message.Contains("Registering: "))
                    {

                    }
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

    private static Socket listener;
    private static IPHostEntry ipHostInfo;
    private static IPAddress ipAddress;
    private static IPEndPoint localEndPoint;
    private static IPEndPoint anyEndPoint;


    public AsynchronousSocketListener() { }

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

                // I haven't figured out how to retrieve the IP yet, so it's "content, content" for now.
                // It should be "IP, content" later. #HalpIsNeeded.
                
                MessageHandler.addMessage(content, content);

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
            Console.WriteLine("Sent {0} bytes to client.", bytesSent);
        }
        catch (Exception e)
        {
            Console.WriteLine(e.ToString());
        }
    }

    public static void lazySend(String content)
    {
        Send(listener, content);
        Console.WriteLine(content);
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
        Console.WriteLine("After you have established a connection...");
        int timesSent = 0;
        while (true)
        {
            Console.WriteLine("Please press enter to send a message to the client.");
            Console.ReadLine();

            // send test message.
            lazySend("Heyyos Client. This is my " + ++timesSent + "th time!");

            Console.WriteLine("You have sent " + timesSent + " messages.");
            
        } // END of timer implementation

        // Feel free to add more threads here to have parallel loops/tasks
        // or just to write more functions and other things.
        // But remember if you put it here you'll have to assume that
        // it works without needing a connection. In other words,
        // Threads #1 and #2 aren't dependent on each other since they
        // are separate threads!

        return 0;
    }
}

using EXO.WebClient;
using System.Diagnostics;
using EXO.Networking.Common;
using System.Xml.Linq;

namespace ServerTestApp
{
    public partial class Form1 : Form
    {

        public const int CMD_SEND_NAME = 1;
        public const int CMD_SEND_MSG = 2;

        public const string URL = "wss://play.theboizgaming.com:8080/ws";
        private NetworkManager networkManager = new();

        public Dictionary<long, string> clientNames = new();

        public static Form1? form;

        public Form1()
        {
            form = this;
            InitializeComponent();
            networkManager.OnHostStart += HandleHostStart;
            networkManager.OnClientStart += HandleClientStart;
        }

        private void HandleClientStart(object? sender, NetworkManager e)
        {
            SetText(e.RoomName, room_name_txtbox);

            // Send my name to the server...
            using (var packet = Packet.CreateCustomPacket(CMD_SEND_NAME))
            {
                packet.Write(name_txtbox.Text);

                networkManager.ClientSend(packet);
            }

        }

        private void HandleHostStart(object? sender, NetworkManager e)
        {
            SetClientName(NetworkManager.LocalID, name_txtbox.Text);
            SetText(e.RoomKey, room_key_txtbox);
        }

        private async Task HandleIncoming()
        {

        }

        private void HandleSendPacket(Packet packet)
        {


        }

        private void host_btn_Click(object sender, EventArgs e)
        {
            if (networkManager.StartHost(room_name_txtbox.Text))
            {
                SetText(networkManager.RoomKey, room_key_txtbox);
            } else 
            {
                WriteToScreen("Could not connect to remote server!");  
            }

        }

        public void SetText(string text, TextBox txtBox)
        {
            if (txtBox.InvokeRequired)
            {
                // If called from a different thread, use Invoke to ensure it runs on the UI thread
                txtBox.Invoke(new Action(() => txtBox.Text = text));
            }
            else
            {
                // If already on the UI thread, directly set the text
                txtBox.Text = text;
            }
        }

        private void send_btn_Click(object sender, EventArgs e)
        {
            using (var packet = Packet.CreateCustomPacket(CMD_SEND_MSG))
            {
                if (NetworkManager.IsHost)
                {
                    packet.Write(NetworkManager.LocalID);
                    packet.Write(send_txtbox.Text);
                    networkManager.ServerBroadcast(packet);
                }
                else
                {
                    packet.Write(send_txtbox.Text);
                    networkManager.Send(packet);
                }
            }

            WriteToScreen($"ME : {send_txtbox.Text}");
            SetText("", send_txtbox);
        }

        private void join_btn_Click(object sender, EventArgs e)
        {
            if (networkManager.StartClient(room_key_txtbox.Text))
            {
                SetText(networkManager.RoomName, room_name_txtbox);
            }
            else 
            {
                WriteToScreen("Could not connect to remote server!");
            }

        }

        [ClientMessageHandler(CMD_SEND_NAME)]
        public static void Client_HandleNameRecieved(Packet packet)
        {
            var from = packet.ReadLong();
            var name = packet.ReadString();
            
            SetClientName(from, name);
        }

        [HostMessageHandler(CMD_SEND_NAME)]
        public static void Host_HandleNameRecieved(Packet packet, long from)
        {
            // We are recieving a name...
            var name = packet.ReadString();

            SetClientName(from, name);

            using (var bcPacket = Packet.CreateCustomPacket(CMD_SEND_NAME))
            {
                bcPacket.Write(from);
                bcPacket.Write(name);
                NetworkManager.Instance?.ServerBroadcast(packet, from);
            }


            // Send all the previously known player names...
            foreach (var client in form.clientNames)
            {
                using (var provideInfoPacket = Packet.CreateCustomPacket(CMD_SEND_NAME))
                {
                    provideInfoPacket.Write(client.Key);
                    provideInfoPacket.Write(client.Value);
                    NetworkManager.Instance?.ServerSend(from, provideInfoPacket);
                }
            }

        }

        [ClientMessageHandler(CMD_SEND_MSG)]
        public static void Client_HandleSendRecieved(Packet packet)
        {
            var from = packet.ReadLong();
            var msg = packet.ReadString();

            WriteToScreen($"{form.clientNames[from]} : {msg}");
        }

        [HostMessageHandler(CMD_SEND_MSG)]
        public static void Host_HandleSend(Packet packet, long from)
        {
            var msg = packet.ReadString();
            WriteToScreen($"{form.clientNames[from]} : {msg}");

            // Create the custom packet...
            using (var bcPacket = Packet.CreateCustomPacket(CMD_SEND_MSG))
            {
                // Write who the broadcast packet is from...
                bcPacket.Write(from);
                // Write the message...
                bcPacket.Write(msg);
                // Broadcast the packet!
                NetworkManager.Instance?.ServerBroadcast(packet, from);
            }
        }

        private static void SetClientName(long clientID, string name)
        {

            if (form == null)
            {
                Debug.WriteLine("Form is not set...");
                return;
            }

            if (form.clientNames.ContainsKey(clientID))
            {
                form.clientNames[clientID] = name;
            }
            else
            {
                form.clientNames.Add(clientID, name);
            }

            WriteToScreen($"{name} : Has Connected!");
        }

        private static void WriteToScreen(string toWrite)
        {
            var text = form.chat_txtbox.Text;
            text += (toWrite + Environment.NewLine);
            form.SetText(text, form.chat_txtbox);
        }
    }
}

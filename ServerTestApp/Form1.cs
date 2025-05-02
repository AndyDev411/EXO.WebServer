using EXO.WebClient;
using System.Diagnostics;
using EXO.Networking.Common;

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
            var packet = Packet.CreateCustomPacket(CMD_SEND_NAME);
            packet.Write(name_txtbox.Text);

            networkManager.ClientSend(packet);
        }

        private void HandleHostStart(object? sender, NetworkManager e)
        {
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
            networkManager.StartHost(room_name_txtbox.Text);
            SetText(networkManager.RoomKey, room_key_txtbox);
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
            var packet = Packet.CreateCustomPacket(CMD_SEND_MSG);
            packet.Write(send_txtbox.Text);

            if (NetworkManager.IsHost)
            {
                networkManager.ServerBroadcast(packet);
            }
            else
            {
                networkManager.Send(packet);
            }

        }

        private void join_btn_Click(object sender, EventArgs e)
        {
            networkManager.StartClient(room_key_txtbox.Text);
            SetText(networkManager.RoomName, room_name_txtbox);
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

            var bcPacket = Packet.CreateCustomPacket(CMD_SEND_NAME);
            bcPacket.Write(from);
            bcPacket.Write(name);
            NetworkManager.Instance?.ServerBroadcast(packet, from);

        }

        [ClientMessageHandler(CMD_SEND_MSG)]
        public static void Client_HandleSendRecieved(Packet packet)
        {
            var from = packet.ReadLong();
            var msg = packet.ReadString();

            WriteToScreen($"{form.clientNames[from]} : {msg}");
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
            text += (toWrite + "\n");
            form.SetText(text, form.chat_txtbox);
        }
    }
}

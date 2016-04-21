﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using WhiteNet.Server;

namespace Server.NS_Model
{
    class CCC_Server
    {
        #region Attributes
        // Server 
        private WhiteNet.Server.Server server;
        private int port;

        // Game
        private List<CCC_Player> players;
        private bool gameRunning;

        // Serverconfig
        public string Name { get; private set; }
        public int MaxPlayers { get; private set; }

        #endregion

        #region Delegates

        public delegate void PlayerEvent(CCC_Player player);

        #endregion

        #region Events

        public event PlayerEvent PlayerConnected = delegate { };

        #endregion

        #region Properties
        public List<CCC_Player> Players
        {
            get { return players; }
            set { players = value; }
        }

        public bool GameRunning
        {
            get { return gameRunning; }
        }
        #endregion

        #region Constructors
        public CCC_Server()
        {
            port = 63001;
            server = new WhiteNet.Server.Server();
            server.ClientConnected += OnClient;

            // TODO: Load config
            Name = "Test Server";
            MaxPlayers = 8;

            // Temp for testing
            gameRunning = false;
            players = new List<CCC_Player>();
        }
        #endregion

        #region Methodes
        public void Start()
        {
            server.StartListener(port);
        }

        public void Stop()
        {
            server.StopListener();
        }
        #endregion

        #region Eventhandlers
        private void OnClient(ServerClient client)
        {
            CCC_Packet response = client.Read();

            /**********************************************
             * Initial Handshake
             * Will check if client and server are using
             * the same protocol version.
             **********************************************/
            if (response.Flag == CCC_Packet.Type.HANDSHAKE)
            {
                if (response.Data.Length < 1 || response.Data[0] != CCC_Packet.Version)
                {
                    client.Send(new CCC_Packet(CCC_Packet.Type.PROTOCOL_NOT_SUPPORTED, CCC_Packet.Version));
                }
                else
                {
                    client.Send(new CCC_Packet(CCC_Packet.Type.HANDSHAKE_OK));
                }
            }
            /**********************************************
             * Info Request
             * Will return information about the server.
             * Clients can request this to prevent unnecessary requests.
             **********************************************/
            else if (response.Flag == CCC_Packet.Type.INFO)
            {
                StringBuilder builder = new StringBuilder();
                builder.AppendFormat("{0};{1};{2};", Name, GameRunning, MaxPlayers);

                foreach (CCC_Player player in players)
                {
                    builder.AppendFormat("{0},", player.Username);
                }

                byte[] encodedInfo = Encoding.Unicode.GetBytes(builder.ToString());
                client.Send(new CCC_Packet(CCC_Packet.Type.INFO_RESPONSE, encodedInfo));
            }
            /**********************************************
             * Login
             * Will check for:
             *  - Game is full
             *  - Username is taken by another player
             *  - Username is valid
             *    (Filter out inappropriate keywords/symbols/...)
             *  - Whitelist
             *  - Blacklist
             * 
             * If successfull, will add user to the player list.
             **********************************************/
            else if (response.Flag == CCC_Packet.Type.LOGIN)
            {
                // Check if game is full.
                if (players.Count == MaxPlayers)
                {
                    client.Send(new CCC_Packet(CCC_Packet.Type.GAME_FULL));
                    return;
                }

                string username = Encoding.Unicode.GetString(response.Data);

                // Check if username is taken.
                foreach (CCC_Player p in players)
                {
                    if (p.Username == username)
                    {
                        client.Send(new CCC_Packet(CCC_Packet.Type.USERNAME_TAKEN));
                        return;
                    }
                }

                // Check if username is valid.
                // TODO
                // Maybe some file or smth
                if (username.Contains("hacker"))
                {
                    client.Send(new CCC_Packet(CCC_Packet.Type.USERNAME_INVALID));
                    return;
                }

                // Check Whitelist.

                // Check Blacklist.

                client.Send(new CCC_Packet(CCC_Packet.Type.LOGIN_OK));
                CCC_Player player = new CCC_Player(client, username);
                players.Add(player);

                PlayerConnected(player);
                // Notify other players.
                foreach (CCC_Player p in players)
                {

                }
            }
            /**********************************************
             * Unknown Packet
             * Will return PROTOCOL_NOT_SUPPORTED error.
             **********************************************/
            else
            {
                Debug.WriteLine("UNKNOWN");
                // Unknown Packet Flag.
                client.Send(new CCC_Packet(CCC_Packet.Type.PROTOCOL_NOT_SUPPORTED, CCC_Packet.Version));
            }
        }
        #endregion
    }
}

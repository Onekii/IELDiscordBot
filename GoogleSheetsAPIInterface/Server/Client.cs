﻿using Shared;
using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace GoogleSheetsAPIInterface.Network
{
    class Client
    {
        internal readonly int Id;
        private TcpClient _client;
        private NetworkStream _stream;
        private byte[] _buffer;

        private Dictionary<int, Func<ByteBuffer, Task>> OpcodeHandlers;

        public Client(TcpClient socket, int clientId)
        {
            Id = clientId;
            _client = socket;
            _stream = _client.GetStream();
            _buffer = new byte[4096];

            OpcodeHandlers = new Dictionary<int, Func<ByteBuffer, Task>>()
            {
                { Opcodes.CMSG_IDENTIFY, HandleIdentifyOpcode},
                { Opcodes.CMSG_DSN_CALCULATION, HandleDSNCalculationOpcode },
                { Opcodes.CMSG_PLAYER_FA_ROLE, HandleFARoleOpcode },
                { Opcodes.CMSG_PLAYER_IN_DISCORD, HandleDiscordOpcode },
                { Opcodes.CMSG_PLAYER_SIGNUP_ACCEPTED, HandleSignupOpcode },
                { Opcodes.CMSG_REQUEST_LEAGUE, HandleRequestLeague },
                { Opcodes.CMSG_NEW_NICKNAME, HandleNewNameOpcode }
            };
        }

        private async Task HandleNewNameOpcode(ByteBuffer arg)
        {
            string discordName = arg.ReadString();
            string newName = arg.ReadString();

            var googleApi = GoogleAPI.Instance();
            int row = googleApi.GetRowNumber(discordName);
            
            if (row != -1)
            {
                string sectionToEdit = $"Player Data!O{row}";

                await googleApi.MakeRequest(sectionToEdit, new List<object>() { newName }).ConfigureAwait(false);
            }
        }

        private async Task HandleRequestLeague(ByteBuffer arg)
        {
            string discordTag = arg.ReadString();

            string league = GoogleAPI.Instance().GetLeague(discordTag);

            ByteBuffer buff = new ByteBuffer(Opcodes.SMSG_LEAGUE_RESPONSE, league.Length + 4);
            buff.WriteString(league);

            await SendPacketAsync(buff).ConfigureAwait(false);
        }

        private async Task HandleIdentifyOpcode(ByteBuffer arg)
        {
            ByteBuffer buff = new ByteBuffer(Opcodes.SMSG_IDENTIFY_ACK, 4);
            buff.WriteInt(Id);

            await SendPacketAsync(buff).ConfigureAwait(false);
        }

        private async Task HandleDiscordOpcode(ByteBuffer arg)
        {
            List<object> obj = new List<object>();
            obj.Add(true);

            int rowNumber = arg.ReadInt();

            string sectionToEdit = $"DSN Hub!H{rowNumber}";

            await GoogleAPI.Instance().MakeRequest(sectionToEdit, obj);
        }

        private async Task HandleSignupOpcode(ByteBuffer arg)
        {
            List<object> obj = new List<object>();

            obj.Add(true);
            obj.Add("");
            obj.Add(true);
            obj.Add(true);
            obj.Add(true);

            int rowNumber = arg.ReadInt();

            string sectionToEdit = $"DSN Hub!R{rowNumber}";

            await GoogleAPI.Instance().MakeRequest(sectionToEdit, obj);
        }

        private async Task HandleFARoleOpcode(ByteBuffer arg)
        {
            List<object> obj = new List<object>();
            obj.Add(true);
            obj.Add(true);

            int rowNumber = arg.ReadInt();

            string sectionToEdit = $"DSN Hub!Z{rowNumber}";

            await GoogleAPI.Instance().MakeRequest(sectionToEdit, obj);
        }

        private async Task HandleDSNCalculationOpcode(ByteBuffer arg)
        {
            List<object> obj = new List<object>();
            obj.Add(arg.ReadInt());
            obj.Add(arg.ReadInt());
            obj.Add(arg.ReadInt());
            obj.Add(arg.ReadInt());
            obj.Add(arg.ReadInt());
            obj.Add(arg.ReadInt());
            obj.Add(arg.ReadInt());

            int row = arg.ReadInt();
            obj[3] = $"=IFS(ISBLANK(K{row});;OR(K{row}<20;J{row}<20;I{row}<20);\"Investigate App\";OR(K{row}>=150;J{row}>=200;I{row}>= 350);\"Games Verified\";AND(K{row}<150;J{row}<200;I{row}<350); \"Min Games not reached\")";

            string sectionToEdit = $"DSN Hub!I{row}";

            await GoogleAPI.Instance().MakeRequest(sectionToEdit, obj);
        }

        public async Task SendPacketAsync(ByteBuffer buff)
        {
            byte[] b = buff.ToByteArray();
            await _stream.WriteAsync(b, 0, b.Length);
            await _stream.FlushAsync();

            Console.WriteLine($"Send {_buffer.Length} to client {Id}");
        }

        public async Task StartReadingAsync()
        {
            Console.WriteLine($"Client {Id} started reading for data.");

            while (_client.Connected)
            {
                int bytesRead = await _stream.ReadAsync(_buffer, 0, _buffer.Length);
                if (bytesRead > 0)
                {
                    Console.WriteLine($"Read {bytesRead} from client {Id}");
                    await ParseData().ConfigureAwait(false);

                    _buffer = new byte[4096];
                }
            }
        }

        private async Task ParseData()
        {
            byte[] arr = new byte[4096];
            Array.Copy(_buffer, arr, _buffer.Length);

            ByteBuffer buff = new ByteBuffer(arr);
            int opcode = buff.ReadInt();
            int length = buff.ReadInt();

            Console.WriteLine($"Read {opcode} from client {Id} with length {length}");

            if (OpcodeHandlers.ContainsKey(opcode))
            {
                Console.WriteLine($"Found handler for {opcode}");
                await OpcodeHandlers[opcode].Invoke(buff).ConfigureAwait(false);
            }
        }
    }
}

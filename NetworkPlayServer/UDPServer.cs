﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Text.Json;
using System.Numerics;
using System.Net.Http;
using System.Net.Http.Json;
using System.Net.Http.Headers;

namespace NetworkPlayServer
{
    internal class UDPServer
    {
        private int serverPort = 9100;
        private UdpClient client = null;
        private Dictionary<int,Player> players = new Dictionary<int, Player>();
        private Thread listenThread = null;
        private bool isServerOn = false;
        private GameManager instance = GameManager.Instance;

        private string InstanceId = "";
        private const string CRUD_URL = "https://obrwl6gf6vjdk3qwgdlartgumy0prkns.lambda-url.us-east-1.on.aws/";
        private const string EC2_URL = "https://o3zu5xaiso32noqdaw2qolit3u0sqhdb.lambda-url.us-east-1.on.aws/";

        private HttpClient httpClient;

        public UDPServer()
        {
            try
            {
                client = new UdpClient(serverPort);
                isServerOn = true;
            }
            catch (SocketException e)
            {
                Console.WriteLine("exception :"+e.ToString());
            }
            if(client != null)
            {
                listenThread = new Thread(() =>
                {
                    StartListen();
                });
                listenThread.Start();
                Console.WriteLine("Play Server Start");
            }
            httpClient = new HttpClient();
        }
        public void StartListen()
        {
            while (isServerOn)
            {
                try
                {
                    IPEndPoint targetEndPoint = new IPEndPoint(IPAddress.Any, 0);
                    byte[] readByte = client.Receive(ref targetEndPoint);
                    
                    string tmp = Encoding.UTF8.GetString(readByte);
                    DTO dto = JsonSerializer.Deserialize<DTO>(tmp);
                    //Console.WriteLine(targetEndPoint.Address.ToString() + ":"+targetEndPoint.Port.ToString()+" :" + tmp);
                    
                    if (dto != null) {
                        MessageType type = (MessageType)Enum.Parse(typeof(MessageType), dto.type);
                        switch (type)
                        {
                            case MessageType.CONNECT:
                                string[] clientAndInstance = dto.msg.Split(";");

                                Player player = new Player(targetEndPoint, clientAndInstance[0]);
                                InstanceId = clientAndInstance[1];

                                players.Add(dto.id, player);
                                SendToTarget(player.endPoint, MessageType.CONNECT, "SUCCESS");
                                if (instance.currCntUp())
                                {
                                    Console.WriteLine("인원이 다 참여하였습니다. 경기 시작");
                                    foreach(Player p in players.Values)
                                    {
                                        SendToTarget(p.endPoint, MessageType.CONNECT, "COMPLETE");
                                    }

                                    ChangeServerState();

                                }
                                else
                                {
                                    Console.WriteLine(player.getNickname()+"이(가) 참여했습니다. 현재원 : "+ instance.getCurrCnt());
                                    SendToTarget(player.endPoint, MessageType.SEND_PARTICIPANT, instance.getCurrCnt()+"");
                                }
                                break;
                            case MessageType.INSTANTIATE:
                                Console.WriteLine(players[dto.id].getNickname() + "생성되었습니다");
                                foreach(int key in players.Keys)
                                {
                                    if(dto.id == key) continue;
                                    string msg = dto.id + ";" + dto.msg;
                                    SendToTarget(players[key].endPoint, MessageType.INSTANTIATE,msg);  
                                }
                                break;
                            case MessageType.SEND_TRANSFORM:

                                //Console.WriteLine(dto.msg);
                                string[] posStr = dto.msg.Split(";");
                                float x = float.Parse(posStr[0]);
                                float y = float.Parse(posStr[1]);
                                float z = float.Parse(posStr[2]);
                                players[dto.id].setTransform(new Vector3(x,y,z));
                                SendTransform(readByte, dto.id);
                                break;
                            case MessageType.SEND_PARTICIPANT:
                                if (dto.msg.Equals("die"))
                                {
                                    Console.WriteLine("Dead" + players[dto.id].getNickname());
                                    players.Remove(dto.id);
                                    if (players.Count == 1)
                                    {
                                        foreach (Player p in players.Values) {
                                            SendToTarget(p.endPoint, MessageType.WIN, "win");
                                        }
                                    }
                                }
                                break;
                        }
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine("exception : "+e.ToString());
                }
                
            }
        }


        public void SendTransform(byte[] data, int clientId) { 
            foreach(int i in players.Keys)
            {
                if(i == clientId) continue;
                client.Send(data, data.Length, players[i].endPoint.Address.ToString(), players[i].endPoint.Port);
            }
        }

        public void SendDie(byte[] data ,int clientId)
        {
            foreach (int i in players.Keys)
            {
                if (i == clientId) continue;
                client.Send(data, data.Length, players[i].endPoint.Address.ToString(), players[i].endPoint.Port);
            }
        }

        public void SendToTarget(IPEndPoint target, MessageType type, string msg)
        {
            try
            {
                DTO sendDTO = new DTO(-1,type.ToString(), msg);
                string dtoToJson = JsonSerializer.Serialize(sendDTO);
                byte[] sendBuffer = Encoding.UTF8.GetBytes(dtoToJson);

                client.Send(sendBuffer, sendBuffer.Length, target.Address.ToString(), target.Port);
            }
            catch (Exception e)
            {
                Console.WriteLine("exception : "+e.ToString());
            }
        }

        public async void ChangeServerState()
        {
            try
            {
                HttpContent create_content = new StringContent("{\"action\":\"create\"}", Encoding.UTF8, "application/json");
                var create_response = await httpClient.PostAsync(EC2_URL, create_content);

                // 참여 불가로 변경
                HttpContent crud_content = new StringContent("{\"instance_id\":\"" + InstanceId + "\"}", Encoding.UTF8, "application/json");
                var update_response = await httpClient.PutAsync(CRUD_URL + "update_item", crud_content);
            }
            catch (Exception e) {
                Console.WriteLine(e.ToString());
            }
            
        }

    }

    
}
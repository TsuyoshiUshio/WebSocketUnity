using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;

namespace WebSocketServer
{
    public class ConferenceServer
    {
        private readonly AsyncLock _asyncLock = new AsyncLock();
        private readonly List<ConferenceClient> _clients = new List<ConferenceClient>();

        public void Map(IApplicationBuilder app)
        {
            app.UseWebSockets();
            app.Use(Acceptor);
        }


        private async Task Acceptor(HttpContext hc, Func<Task> n)
        {
            if (!hc.WebSockets.IsWebSocketRequest)
            {
                await n.Invoke();
                return;
            }
            var websocket = await hc.WebSockets.AcceptWebSocketAsync();
            var client = new ConferenceClient(websocket);

            await client.RecieveJoinAsync();

            if (!client.IsJoin)
                return;
            using (await _asyncLock.LockAsync())
            {
                _clients.AsParallel().ForAll(
                    x =>
                    {
                        var message = $"{client.UserName} さんが入室しました";
                        var messageBytes = Encoding.UTF8.GetBytes(message);
                        // 入室メッセージを送信
                        x.OnNext(new ChatMessage
                        {
                            UserName = "管理者",
                            Message = new ArraySegment<byte>(messageBytes),
                            RecieveTime = DateTimeOffset.Now
                        });

                        // ほかのクライアントと相互接続
                        x.Subscribe(client);
                        client.Subscribe(x);
                    });

                // エコーバック
                client.Subscribe(client);

                // 切断時動作
                client.Subscribe(s => { }, async () => await Close(client));

                // クライアント登録
                _clients.Add(client);
            }

            // 受信待機
            await client.ReceiveAsync();
        }

        private async Task Close(ConferenceClient client)
        {
            using (await _asyncLock.LockAsync())
            {
                _clients.Remove(client);



                // 退室メッセージを送信
                _clients.ForEach(
                    x =>
                    {
                        _clients.ForEach(
                            y =>
                            {
                                var message = $"{x.UserName} さんが退室しました";
                                var messageBytes = Encoding.UTF8.GetBytes(message);
                                y.OnNext(new ChatMessage
                                {
                                    UserName = "管理者",
                                    Message = new ArraySegment<byte>(messageBytes),
                                    RecieveTime = DateTimeOffset.Now
                                });

                            }

                        );
                    }
                    );
            }
        }
    }
}

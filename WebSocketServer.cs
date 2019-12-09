// Copyright 2019 Wildbook
// MIT License:
//   Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files (the "Software"), to deal in the Software without restriction, including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, subject to the following conditions:
//   The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.
//   THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.

using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading;

public class WebSocketServer
{
    public readonly List<WebSocketSession> Clients = new List<WebSocketSession>();

    public event EventHandler<WebSocketSession> ClientConnected;
    public event EventHandler<WebSocketSession> ClientDisconnected;
    
    private bool _listening;

    public void Close() => _listening = false;

    public void Listen(int port)
    {
        if (_listening) throw new Exception("Already listening!");
        _listening = true;

        var server = new TcpListener(IPAddress.Any, port);
        server.Start();

        Console.WriteLine("WS Server - UP");

        ThreadPool.QueueUserWorkItem(_ =>
        {
            while (_listening)
            {
                var session = new WebSocketSession(server.AcceptTcpClient());
                session.HandshakeCompleted += (__, ___) =>
                {
                    Console.WriteLine($"{session.Id}| Handshake Valid.");
                    Clients.Add(session);
                };

                session.Disconnected += (__, ___) =>
                {
                    Console.WriteLine($"{session.Id}| Disconnected.");
                    Clients.Remove(session);

                    ClientDisconnected?.Invoke(this, session);
                    session.Dispose();
                };

                Console.WriteLine($"{session.Id}| Connected.");
                ClientConnected?.Invoke(this, session);
                session.Start();
            }

            server.Stop();
        });
    }
}
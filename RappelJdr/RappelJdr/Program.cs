namespace DiscordBot
{
    using System;
    using System.IO;
    using System.Reflection;
    using System.Threading;
    using System.Threading.Channels;
    using System.Threading.Tasks;
    using Discord;
    using Discord.Net;
    using Discord.WebSocket;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;

    /// <summary>
    /// Main class of the program, contain the main method that is launching all the others.
    /// </summary>
    public class Program
    {
        /// <summary>
        /// WebSocket-based discord client.
        /// </summary>
        private DiscordSocketClient _client;

        /// <summary>
        ///
        /// </summary>
        /// <param name="args"></param>
        public static void Main(string[] args) => new Program().MainAsync().GetAwaiter().GetResult();

        public async void SendMessageSessions()
        {
            Thread.Sleep(10000);

            while (true)
            {
                var sessions = MessageHandler.SessionService.GetEntities();

                foreach (var session in sessions)
                {
                    try
                    {
                        await _client.GetGuild(session.ServerId).GetTextChannel(session.ChannelId).SendMessageAsync("Prochaine session le " + session.Date.ToString());

                        if (session.Date < DateTime.Now)
                        {
                            MessageHandler.DeleteSession(session.Id);
                        }
                    }
                    catch (Exception ex)
                    {
                        _ = Log(new LogMessage(LogSeverity.Error, "SendMessage", ex.Message));
                    }
                }

                Thread.Sleep(3600000);
            }
        }

        /// <summary>
        ///
        /// </summary>
        /// <returns></returns>
        public async Task MainAsync()
        {
            _client = new DiscordSocketClient();

            _client.Log += Log;

            string path = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), @"Token.json");
            string text = File.ReadAllText(path);
            Token token = (Token)JsonConvert.DeserializeObject(text, typeof(Token));
            await _client.LoginAsync(TokenType.Bot, token.Value);
            await _client.StartAsync();

            _client.MessageReceived += MessageReceived;

            Thread t = new Thread(new ThreadStart(SendMessageSessions));

            t.Start();

            // Block this task until the program is closed.
            await Task.Delay(-1);
        }

        /// <summary>
        /// Write a log message into the console.
        /// </summary>
        /// <param name="msg">Message from the logger.</param>
        /// <returns>Tell the task as been completed successfully.</returns>
        private Task Log(LogMessage msg)
        {
            Console.WriteLine(msg.ToString());
            return Task.CompletedTask;
        }

        /// <summary>
        /// Executed when a message is received.
        /// Used to manage messages sent by the users.
        /// </summary>
        /// <param name="message">Message sent on a channel where the bot is.</param>
        /// <returns></returns>
        private async Task MessageReceived(SocketMessage message)
        {
            string messageToSend = string.Empty;
            string request = message.Content;

            try
            {
                if (request != null)
                {
                    if (request.StartsWith("-"))
                    {
                        if (request.StartsWith("-set"))
                        {
                            var channel = (SocketGuildChannel)message.Channel;
                            var guild = (SocketGuild)channel.Guild;

                            messageToSend = MessageHandler.SetSession(request.Replace("-set ", string.Empty), guild.Id, channel.Id);
                        }
                        else if (request.StartsWith("-list"))
                        {
                            messageToSend = MessageHandler.ListSession();
                        }
                        else if (request.StartsWith("-delete"))
                        {
                            messageToSend = MessageHandler.DeleteSession(int.Parse(request.Replace("-delete ", string.Empty)));
                        }
                        else if (request.StartsWith("-help"))
                        {
                            messageToSend = MessageHandler.Help();
                        }
                    }
                }
            }
            catch
            {
                messageToSend = "Tag incorrect.";
            }

            if (!string.IsNullOrWhiteSpace(messageToSend))
            {
                try
                {
                    await message.Channel.SendMessageAsync(messageToSend);
                }
                catch (Exception ex)
                {
                    _ = Log(new LogMessage(LogSeverity.Error, "SendMessage", ex.Message));
                }
            }
        }
    }
}
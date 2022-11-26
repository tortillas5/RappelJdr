namespace RappelJdr
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Reflection;
    using System.Threading;
    using System.Threading.Tasks;
    using Discord;
    using Discord.WebSocket;
    using Newtonsoft.Json;
    using RappelJdr.Entities;

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
        /// Lance la tâche MainAsync en tant que programme.
        /// </summary>
        /// <param name="args"></param>
        public static void Main(string[] args) => new Program().MainAsync().GetAwaiter().GetResult();

        /// <summary>
        /// Asynchronous task launched at the begging of the app, handle everything.
        /// </summary>
        /// <returns>This async task.</returns>
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
            _client.ReactionAdded += ReactionAdded;
            _client.ReactionRemoved += ReactionRemoved;

            // Create and launch the task that send the messages saying there is sessions.
            Thread sessionsMessagesThread = new Thread(new ThreadStart(SendMessageSessions));
            sessionsMessagesThread.Start();

            // Block this task until the program is closed.
            await Task.Delay(-1);
        }

        /// <summary>
        /// Executed when a message is received.
        /// Used to manage messages sent by the users.
        /// </summary>
        /// <param name="message">Message sent on a channel where the bot is.</param>
        /// <returns>The current method as a Task.</returns>
        public async Task MessageReceived(SocketMessage message)
        {
            string messageToSend = string.Empty;
            string request = message.Content;

            try
            {
                if (request != null)
                {
                    if (request.StartsWith("-"))
                    {
                        SocketGuildChannel channel = (SocketGuildChannel)message.Channel;
                        SocketGuild guild = channel.Guild;

                        ulong serverId = guild.Id;
                        ulong channelId = channel.Id;

                        string command = GetCommand(request);

                        if (command == "-set")
                        {
                            List<string> args = GetArguments(request);

                            messageToSend = MessageHandler.SetSession(args[0] + " " + args[1], serverId, channelId);
                        }
                        else if (command == "-list")
                        {
                            messageToSend = MessageHandler.ListSession(serverId, channelId);
                        }
                        else if (command == "-delete")
                        {
                            List<string> args = GetArguments(request);

                            messageToSend = MessageHandler.DeleteSession(int.Parse(args[0]), serverId, channelId);
                        }
                        else if (command == "-help")
                        {
                            messageToSend = MessageHandler.Help();
                        }
                        else if (command == "-addRole")
                        {
                            List<string> args = GetArguments(request);
                            var userGuild = (SocketGuildUser)message.Author;
                            bool adminUser = userGuild.GuildPermissions.Has(GuildPermission.ManageRoles);

                            messageToSend = MessageHandler.AddRole(args[0], string.Join(" ", args.Skip(1)), GetUserName(message.Author), guild.Roles.Select(r => r.Name).ToList(), serverId, adminUser);
                        }
                        else if (command == "-removeRole")
                        {
                            List<string> args = GetArguments(request);
                            var userGuild = (SocketGuildUser)message.Author;
                            bool adminUser = userGuild.GuildPermissions.Has(GuildPermission.ManageRoles);

                            messageToSend = MessageHandler.RemoveRole(args[0], GetUserName(message.Author), serverId, adminUser);
                        }
                        else if (command == "-listRole")
                        {
                            messageToSend = MessageHandler.ListRole(serverId);
                        }
                        else if (command == "-react")
                        {
                            try
                            {
                                var sentMessage = await message.Channel.SendMessageAsync(MessageHandler.ReactTo(serverId));

                                ReactionHandler.MessageToFollow(sentMessage.Id, serverId);
                            }
                            catch (Exception ex)
                            {
                                _ = Log(new LogMessage(LogSeverity.Error, "MessageToFollow", ex.Message));
                            }

                            return;
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

        /// <summary>
        /// Method executed when an action is added to a message.
        /// Used for role gestion.
        /// </summary>
        /// <param name="message">Message wich was reacted to.</param>
        /// <param name="channel">Current channel.</param>
        /// <param name="reaction">Reaction wich someone reacted to.</param>
        /// <returns>The current method as a Task.</returns>
        public async Task ReactionAdded(Cacheable<IUserMessage, ulong> message, Cacheable<IMessageChannel, ulong> channel, SocketReaction reaction)
        {
            try
            {
                SocketGuild guild = ((SocketGuildChannel)channel.Value).Guild;
                ulong serverId = guild.Id;

                if (ReactionHandler.IsFollowedMessage(reaction.MessageId, serverId))
                {
                    var roles = ReactionHandler.GetRoles(serverId);

                    Role role = roles.FirstOrDefault(r => r.Emoji.Equals(reaction.Emote.Name));

                    if (role != null)
                    {
                        var user = reaction.User.Value;
                        var socketRole = ((SocketGuildChannel)reaction.Channel).Guild.Roles.FirstOrDefault(r => r.Name == role.Name);

                        await (user as IGuildUser).AddRoleAsync(socketRole);
                    }
                }
            }
            catch (Exception ex)
            {
                _ = Log(new LogMessage(LogSeverity.Error, "ReactionAdded", ex.Message));
            }
        }

        /// <summary>
        /// Method executed when an action is removed from a message.
        /// Used for role gestion.
        /// </summary>
        /// <param name="message">Message wich was reacted to.</param>
        /// <param name="channel">Current channel.</param>
        /// <param name="reaction">Reaction wich someone reacted to.</param>
        /// <returns>The current method as a Task.</returns>
        public async Task ReactionRemoved(Cacheable<IUserMessage, ulong> message, Cacheable<IMessageChannel, ulong> channel, SocketReaction reaction)
        {
            try
            {
                SocketGuild guild = ((SocketGuildChannel)channel.Value).Guild;
                ulong serverId = guild.Id;

                if (ReactionHandler.IsFollowedMessage(reaction.MessageId, serverId))
                {
                    var roles = ReactionHandler.GetRoles(serverId);

                    Role role = roles.FirstOrDefault(r => r.Emoji.Equals(reaction.Emote.Name));

                    if (role != null)
                    {
                        var user = reaction.User.Value;
                        var socketRole = ((SocketGuildChannel)reaction.Channel).Guild.Roles.FirstOrDefault(r => r.Name == role.Name);

                        await (user as IGuildUser).RemoveRoleAsync(socketRole);
                    }
                }
            }
            catch (Exception ex)
            {
                _ = Log(new LogMessage(LogSeverity.Error, "ReactionAdded", ex.Message));
            }
        }

        /// <summary>
        /// Tâche s'occupant d'envoyer les messages de rappel de sessions aux cannaux correspondants.
        /// </summary>
        public async void SendMessageSessions()
        {
            // Au lancement de l'application on attend une dizaine de secondes que le bot soit connecté à discord.
            Thread.Sleep(10000);

            // Tant que le programme tourne.
            while (true)
            {
                var sessions = MessageHandler.SessionService.GetEntities();

                foreach (var session in sessions)
                {
                    try
                    {
                        // Suppression si la session est passée.
                        if (session.Date < DateTime.Now)
                        {
                            MessageHandler.DeleteSession(session.Id, session.ServerId, session.ChannelId);
                        }
                        else
                        {
                            if (session.Date.Date == DateTime.Now.Date && session.Date.TimeOfDay <= DateTime.Now.TimeOfDay.Add(new TimeSpan(1, 0, 0)))
                            {
                                // Même jour & commence dans moins d'une heure.
                                await _client.GetGuild(session.ServerId).GetTextChannel(session.ChannelId).SendMessageAsync("@everyone la prochaine session commence bientôt ! " + session.Date.ToString());
                            }
                            else if (session.Date.TimeOfDay <= DateTime.Now.TimeOfDay.Add(new TimeSpan(1, 0, 0)) && session.Date.TimeOfDay >= DateTime.Now.TimeOfDay)
                            {
                                // Jour différent et commencera environ dans l'heure.
                                await _client.GetGuild(session.ServerId).GetTextChannel(session.ChannelId).SendMessageAsync("Prochaine session le " + session.Date.ToString());
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        _ = Log(new LogMessage(LogSeverity.Error, "SendMessageSessions", ex.Message));
                    }
                }

                Thread.Sleep(new TimeSpan(0, 59, 0));
            }
        }

        /// <summary>
        /// Return the arguments of a message.
        /// </summary>
        /// <param name="message">Message of an user.</param>
        /// <returns>List of arguments.</returns>
        private List<string> GetArguments(string message)
        {
            return message.Split(' ').Where(c => !string.IsNullOrWhiteSpace(c) && !c.StartsWith('-')).ToList();
        }

        /// <summary>
        /// Return the command (first part of the message) sent by an user.
        /// </summary>
        /// <param name="message">Message of an user.</param>
        /// <returns>A command.</returns>
        private string GetCommand(string message)
        {
            return message.Split(' ')[0];
        }

        /// <summary>
        /// Return the full username (name#1234) of a SockerUser.
        /// </summary>
        /// <param name="user">A socketUser.</param>
        /// <returns>A full username.</returns>
        private string GetUserName(SocketUser user)
        {
            return user.Username + "#" + user.Discriminator;
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
    }
}
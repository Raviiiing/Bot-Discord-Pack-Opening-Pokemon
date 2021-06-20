using Discord;
using Discord.Commands;
using Discord.WebSocket;
using System;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;

namespace Bot_Discord_Jeu_de_Carte
{
    class Program
    {
            static void Main(string[] args) => new Program().RunBotAsync().GetAwaiter().GetResult();

            private DiscordSocketClient _client;
            private CommandService _commands;
            private IServiceProvider _services;

            public async Task RunBotAsync()
            {
                _client = new DiscordSocketClient();
                _commands = new CommandService();

                _services = new ServiceCollection()
                    .AddSingleton(_client)
                    .AddSingleton(_commands)
                    .BuildServiceProvider();

                await _commands.AddModulesAsync(Assembly.GetEntryAssembly(), _services);

                string token = "token";

                _client.Log += _client_Log;

                _client.MessageReceived += HandleCommandAsync;

                await _client.LoginAsync(TokenType.Bot, token);

                await _client.StartAsync();

                await _client.SetGameAsync("!aide channel #elodie");

                await _client.SetStatusAsync(UserStatus.Online);

                await Task.Delay(-1);
            }

            private Task _client_Log(LogMessage arg)
            {
                Console.WriteLine(arg);
                return Task.CompletedTask;
            }

            private async Task HandleCommandAsync(SocketMessage arg)
            {
                var message = arg as SocketUserMessage;
                if (message is null) return;
                var context = new SocketCommandContext(_client, message);
                if (message.Author.IsBot || context.Channel.Name != "elodie") return;

                int argPos = 0;
                if (message.HasStringPrefix("!", ref argPos))
                {
                    var chnl = message.Channel as SocketGuildChannel;
                    var Guild = chnl.Guild.Name;
                    Console.ForegroundColor = ConsoleColor.Blue;
                    Console.WriteLine("{0}: USER: {1} Has issued a command in {2} in Server '{3}' : {4}", DateTime.Now, arg.Author.Id, arg.Channel, Guild, arg);
                    Console.ForegroundColor = ConsoleColor.White;
                    var result = await _commands.ExecuteAsync(context, argPos, _services);
                    if (!result.IsSuccess)
                        Console.WriteLine(result.ErrorReason);
                }
            }
        }
}

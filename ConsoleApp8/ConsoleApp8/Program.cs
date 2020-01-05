using System;
using System.Reflection;
using System.Threading;
using Microsoft.Extensions.DependencyInjection;
using Discord.Commands;
using Discord;
using Discord.Net;
using System.Threading.Tasks;
using Discord.Webhook;
using Discord.WebSocket;
using Discord.API;
using Discord.Audio;
using Discord.Rest;
using Discord.Net.Providers.WS4Net;
using System.Linq;
using NHentaiSharp.Core;
using NHentaiSharp;
using NHentaiSharp.Exception;
using NHentaiSharp.Search;
namespace Degen
{

    public class Program
    {
        // Program entry point
        static void Main(string[] args)
        {
            // Call the Program constructor, followed by the 
            // MainAsync method and wait until it finishes (which should be never).
            new Program().MainAsync().GetAwaiter().GetResult();
        }

        private  DiscordSocketClient _client;

        // Keep the CommandService and DI container around for use with commands.
        // These two types require you install the Discord.Net.Commands package.
        private readonly CommandService _commands;
        private readonly IServiceProvider _services;


        private Program()
        {
            _client = new DiscordSocketClient(new DiscordSocketConfig
            {
                // How much logging do you want to see?
                LogLevel = LogSeverity.Info,

                // If you or another service needs to do anything with messages
                // (eg. checking Reactions, checking the content of edited/deleted messages),
                // you must set the MessageCacheSize. You may adjust the number as needed.
                MessageCacheSize = 50,

                // If your platform doesn't have native WebSockets,
                // add Discord.Net.Providers.WS4Net from NuGet,
                // add the `using` at the top, and uncomment this line:
                WebSocketProvider = WS4NetProvider.Instance
            });

            _commands = new CommandService(new CommandServiceConfig
            {
                // Again, log level:
                LogLevel = LogSeverity.Info,

                // There's a few more properties you can set,
                // for example, case-insensitive commands.
                CaseSensitiveCommands = false,
            });

            // Subscribe the logging handler to both the client and the CommandService.
            _client.Log += Log;
            _commands.Log += Log;

            // Setup your DI container.
            _services = ConfigureServices();

        }

        // If any services require the client, or the CommandService, or something else you keep on hand,
        // pass them as parameters into this method as needed.
        // If this method is getting pretty long, you can seperate it out into another file using partials.
        private static IServiceProvider ConfigureServices()
        {
            var map = new ServiceCollection()
                // Repeat this for all the service classes
                // and other dependencies that your commands might need.
                .AddSingleton(new ServiceCollection());

            // When all your required services are in the collection, build the container.
            // Tip: There's an overload taking in a 'validateScopes' bool to make sure
            // you haven't made any mistakes in your dependency graph.
            return map.BuildServiceProvider();
        }

        // Example of a logging handler. This can be re-used by addons
        // that ask for a Func<LogMessage, Task>.
        private static Task Log(LogMessage message)
        {
            switch (message.Severity)
            {
                case LogSeverity.Critical:
                case LogSeverity.Error:
                    Console.ForegroundColor = ConsoleColor.Red;
                    break;
                case LogSeverity.Warning:
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    break;
                case LogSeverity.Info:
                    Console.ForegroundColor = ConsoleColor.White;
                    break;
                case LogSeverity.Verbose:
                case LogSeverity.Debug:
                    Console.ForegroundColor = ConsoleColor.DarkGray;
                    break;
            }
            Console.WriteLine($"{DateTime.Now,-19} [{message.Severity,8}] {message.Source}: {message.Message} {message.Exception}");
            Console.ResetColor();

            // If you get an error saying 'CompletedTask' doesn't exist,
            // your project is targeting .NET 4.5.2 or lower. You'll need
            // to adjust your project's target framework to 4.6 or higher
            // (instructions for this are easily Googled).
            // If you *need* to run on .NET 4.5 for compat/other reasons,
            // the alternative is to 'return Task.Delay(0);' instead.
            return Task.CompletedTask;
        }


        public class InfoModule : ModuleBase<SocketCommandContext>
        {
            [Command("nhentai")]
            [Summary("Searches nhentai")]
            public async Task NSearch()
            {
                var rand = new Random();
                await ReplyAsync($"https://nhentai.net/g/{rand.Next(1, 205953)}");
                
            }
           [Command("purge")]
           [RequireBotPermission(GuildPermission.ManageMessages)]
           public async Task PurgeMessages(uint amount)
            {
                var messages = await Context.Channel.GetMessagesAsync((int)amount+1).FlattenAsync();

                await (Context.Channel as ITextChannel).DeleteMessagesAsync(messages);
                const int delay = 5000;
                var m = await this.ReplyAsync($"Purge completed. _This message will be deleted in {delay / 1000} seconds._");
                await Task.Delay(delay);
                await m.DeleteAsync();
            }
          [Command("mute")]
          [RequireBotPermission(GuildPermission.ManageRoles)]
          public async Task MuteUser([Remainder]SocketGuildUser usertobemute)
            {
                
                var rUser = Context.User ;
                var role = Context.Guild.Roles.FirstOrDefault(x => x.Name == "Moderator Powers");
                var muterole = Context.Guild.Roles.FirstOrDefault(y => y.Name.ToString() == "Muted");
                if (rUser.Id == 358721793315438602 || rUser.Id == 294971449427623937 || rUser.Id == 365461162638442497 || rUser.Id == 268933457286004736 || rUser.Id == 249150534261538817 || rUser.Id == 300920616486436864)
                {
                    await ReplyAsync($"User {usertobemute.Mention} has been muted.");
                    await (usertobemute as IGuildUser).AddRoleAsync(muterole);
                }
                else
                {
                    await ReplyAsync("Admin only command. Sorry :heart");
                }
            }
            [Command("unmute")]
            [RequireBotPermission(GuildPermission.ManageRoles)]
            public async Task UnMuteUser([Remainder]SocketGuildUser usertobemute)
            {

                var rUser = Context.User;
                var role = Context.Guild.Roles.FirstOrDefault(x => x.Name == "Moderator Powers");
                var muterole = Context.Guild.Roles.FirstOrDefault(y => y.Name.ToString() == "Muted");
                if (rUser.Id == 358721793315438602 || rUser.Id == 294971449427623937 || rUser.Id == 365461162638442497 || rUser.Id == 268933457286004736 || rUser.Id == 249150534261538817 || rUser.Id == 300920616486436864)
                {
                    await ReplyAsync($"User {usertobemute.Mention} has been unmuted if they were muted before.");
                    await (usertobemute as IGuildUser).RemoveRoleAsync(muterole);
                }
                else
                {
                    await ReplyAsync("Admin only command. Sorry :heart");
                }
            }
            [Command("uptime")]
            public async Task Uptime()
            {
                var upt = DateTime.Now - System.Diagnostics.Process.GetCurrentProcess().StartTime;
                
                await ReplyAsync($"I've been running for the last {upt} without a restart.");
            }
            [Command("ban")]
           [RequireBotPermission(GuildPermission.BanMembers)]
           public async Task BanUder([Remainder]SocketGuildUser usertobebanned)
            {
                var rUser = Context.User as SocketGuildUser;
                var role = Context.Guild.Roles.FirstOrDefault(x => x.Name == "Moderator Powers");

                if (rUser.Id == 358721793315438602 || rUser.Id == 294971449427623937 || rUser.Id == 365461162638442497 || rUser.Id == 268933457286004736 || rUser.Id == 249150534261538817 || rUser.Id == 300920616486436864)
                {
                    if (usertobebanned.GuildPermissions.KickMembers == false)
                    {
                        await ReplyAsync($"User {usertobebanned.Mention} has been banned.");
                        await Context.Guild.AddBanAsync(usertobebanned, 0);
                    }
                    else
                    {
                        await ReplyAsync($"User {usertobebanned.Mention} is a moderator, ya rart.");
                    }
                }
                else
                {
                    await ReplyAsync("Moderator only command, sorry sweetie :heart:");
                }
            }
            [Command("kick")]
            [RequireBotPermission(GuildPermission.BanMembers)]
            public async Task KickUser([Remainder]SocketGuildUser usertobekicked)
            {

                var rUser = Context.User as SocketGuildUser;
                var role = Context.Guild.Roles.FirstOrDefault(x => x.Name == "Moderator Powers");
                var user = usertobekicked as IGuildUser;
                if (rUser.Id == 358721793315438602 || rUser.Id == 294971449427623937 || rUser.Id == 365461162638442497 || rUser.Id == 268933457286004736 || rUser.Id == 249150534261538817 || rUser.Id == 300920616486436864)
                {
                    if (user.GuildPermissions.KickMembers == false) { 
                        await ReplyAsync($"User {usertobekicked.Mention} has been kicked.");
                        await user.KickAsync();
                    }
                    else if (user.GuildPermissions.KickMembers == true)
                    {
                        await ReplyAsync($"User {usertobekicked.Mention} is a moderator, ya rart.");
                    }
                }
                else
                {
                    await ReplyAsync("Admin only command. Sorry  :heart:");
                }
            }

            [Command("commands")]
            [Summary("What do you think?")]
            public async Task Help()
            {
                await Context.Channel.SendMessageAsync("!commands,!hestia,!kurisu,!miku");

            }
            [Command("Koneko")]
            [Summary("Shows uptime")]
            public async Task Koneko([Summary("Number of deets to add")]string num)
            {
                int num1 = int.Parse(num);
                if(num == null)
                {
                    var rando = new Random();
                    var mikunumber = rando.Next(0, 78);
                    await Context.Channel.SendMessageAsync(Links.Koneko[mikunumber]);
                }
                for (int i = 0; i <= num1 - 1; i++)
                {
                    var rando = new Random();
                    var mikunumber = rando.Next(0, 78);
                    await Context.Channel.SendMessageAsync(Links.Koneko[mikunumber]);
                }
            }



            [Command("miku")]
            [Summary("Shows image of miku.")]
            public async Task Miku(
                [Summary("The number of images to link")] string num)
            {
                int num1 = int.Parse(num);
                if (num == null)
                {
                    var rando = new Random();
                    var mikunumber = rando.Next(0, 78);
                    await Context.Channel.SendMessageAsync(Links.BestGirl[mikunumber]);
                }
                for (int i = 0; i <= num1 - 1; i++)
                {
                    var rando = new Random();
                    var mikunumber = rando.Next(0, 16);
                    await Context.Channel.SendMessageAsync(Links.BestGirl[mikunumber]);
                }
            }
            [Command("hestia")]
            [Summary("Shows Image of Hestia.")]
            public async Task Hestia([Summary("Number of images")] string num)
            {
                int num1 = int.Parse(num);
                if (num == null)
                {
                    var rando = new Random();
                    var mikunumber = rando.Next(0, 78);
                    await Context.Channel.SendMessageAsync(Links.Bestia[mikunumber]);
                }

                for (int i = 0; i <= num1 - 1; i++)
                {
                    var rando = new Random();
                    var hestianumber = rando.Next(0, 8);
                    await Context.Channel.SendMessageAsync(Links.Bestia[hestianumber]);
                }
            }
            [Command("Kurisu")]
            [Summary("Shows Image of Kurisu")]
            public async Task Kurisu([Summary("Number of images")] string num)
            {
                bool b = string.IsNullOrEmpty(num);
                int num1 = int.Parse(num);
                if (b == false)
                {
                    var rando = new Random();
                    var mikunumber = rando.Next(0, 78);
                    await Context.Channel.SendMessageAsync(Links.Kurisu[mikunumber]);
                }
                else
                {
                    for (int i = 0; i <= num1 - 1; i++)
                    {
                        var rando = new Random();
                        var kurisunumber = rando.Next(0, 60);
                        await Context.Channel.SendMessageAsync(Links.Kurisu[kurisunumber]);
                    }
                }
                
            }
            [Command("die")]
            [RequireUserPermission(GuildPermission.KickMembers)]
            [Summary("Makes bot be sad then turn off")]
            public async Task Suicide()
            {
                var x = Context.User as SocketGuildUser;
                await ReplyAsync($"Oh good another comman- oh... I see how it is. I know I'm an awful bot but did you really need to tell me to die? I guess I'll grant your wish, {x.Mention}. Despite this, I still like you. :heart: :robot:");
                
                Environment.Exit(0);
            }
            [Command("bean")]
            [Summary("Fake ban's targeted user")]
            public async Task Bean([Remainder]SocketGuildUser usertobebanned)
            {
                string msg = "https://cdn.discordapp.com/attachments/561415828017381391/663216499288178731/uh-oh-you-friccin-moron-you-just-got-beaned-tag-2431452.png";
                var nonad = Context.User as SocketGuildUser;
                if (usertobebanned.GuildPermissions.BanMembers == false)
                {
                   
                    await ReplyAsync($"User {usertobebanned.Mention} has been beaned!");
                    await Discord.UserExtensions.SendMessageAsync(usertobebanned, msg);
                }
                else
                {
                    await ReplyAsync($"User {nonad.Mention} attempted to bean {usertobebanned.Mention} but it got reflected back onto them instead!");
                    await UserExtensions.SendMessageAsync(nonad, msg);
                }
            }
             
    }
        private async Task MainAsync()
        {
            // Centralize the logic for commands into a separate method.
            await InitCommands();
            string realtoken = "secret";
            // Login and connect.
            await _client.LoginAsync(TokenType.Bot,

                realtoken);
            await _client.StartAsync();
           
            // Wait infinitely so your bot actually stays connected.
            await Task.Delay(Timeout.Infinite);
        }

        private async Task InitCommands()
        {

            // Either search the program and add all Module classes that can be found.
            // Module classes MUST be marked 'public' or they will be ignored.
            // You also need to pass your 'IServiceProvider' instance now,
            // so make sure that's done before you get here.
            await _commands.AddModulesAsync(Assembly.GetEntryAssembly(), _services);
            // Or add Modules manually if you prefer to be a little more explicit:
            await _commands.AddModuleAsync<Module>(_services);
            // Note that the first one is 'Modules' (plural) and the second is 'Module' (singular).

            // Subscribe a handler to see if a message invokes a command.
            _client.MessageReceived += HandleCommandAsync;

            
        }

        

        private async Task HandleCommandAsync(SocketMessage arg)
        {
            // Bail out if it's a System Message.
            var msg = arg as SocketUserMessage;
            if (msg == null) return;

            // We don't want the bot to respond to itself or other bots.
            if (msg.Author.Id == _client.CurrentUser.Id || msg.Author.IsBot) return;
           
    
            // Create a number to track where the prefix ends and the command begins
            int pos = 0;
            // Replace the '!' with whatever character
            // you want to prefix your commands with.
            // Uncomment the second half if you also want
            // commands to be invoked by mentioning the bot instead.
            if (msg.HasCharPrefix('!', ref pos) /* || msg.HasMentionPrefix(_client.CurrentUser, ref pos) */)
            {
                // Create a Command Context.
                var context = new SocketCommandContext(_client, msg);
                
                // Execute the command. (result does not indicate a return value, 
                // rather an object stating if the command executed successfully).
                var result = await _commands.ExecuteAsync(context, pos, _services);

                // Uncomment the following lines if you want the bot
                // to send a message if it failed.
                // This does not catch errors from commands with 'RunMode.Async',
                // subscribe a handler for '_commands.CommandExecuted' to see those.
                //if (!result.IsSuccess && result.Error != CommandError.UnknownCommand)
                //    await msg.Channel.SendMessageAsync(result.ErrorReason);
            }
        }
    }
}

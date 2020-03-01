using System.Threading.Tasks;
using Discord.Commands;

namespace SysBot.Pokemon.Discord
{
    public class PingModule : ModuleBase<SocketCommandContext>
    {
        [Command("ping")]
        [Summary("Makes the bot respond, indicating that it is running.")]
        public async Task PingAsync()
        {
            await ReplyAsync("Pong!").ConfigureAwait(false);
        }

        [Command("hello"), Alias("hi")]
        [Summary("Informs users that they are adorable.")]
        [Priority(0)]
        public async Task GreetUser()
        {
            await ReplyAsync("hi ur qt").ConfigureAwait(false);
        }
    }
}
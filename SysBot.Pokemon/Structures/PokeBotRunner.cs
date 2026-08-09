using PKHeX.Core;
using SysBot.Base;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace SysBot.Pokemon;

public interface IPokeBotRunner
{
    PokeTradeHubConfig Config { get; }
    bool RunOnce { get; }
    bool IsRunning { get; }

    void StartAll();
    void StopAll();
    void InitializeStart();

    void Add(PokeRoutineExecutorBase newbot);
    void Remove(IConsoleBotConfig state, bool callStop);

    BotSource<PokeBotState>? GetBot(PokeBotState state);
    PokeRoutineExecutorBase CreateBotFromConfig(PokeBotState cfg);
    bool SupportsRoutine(PokeRoutineType pokeRoutineType);
}

public abstract class PokeBotRunner<T>(PokeTradeHub<T> hub, BotFactory<T> Factory)
    : BotRunner<PokeBotState>, IPokeBotRunner
    where T : PKM, new()
{
    public PokeTradeHub<T> Hub => hub;

    public PokeTradeHubConfig Config => Hub.Config;

    protected PokeBotRunner(PokeTradeHubConfig config, BotFactory<T> factory) : this(new PokeTradeHub<T>(config), factory)
    {
    }

    protected virtual void AddIntegrations() { }

    public override void Add(RoutineExecutor<PokeBotState> bot)
    {
        base.Add(bot);
    }

    public override bool Remove(IConsoleBotConfig cfg, bool callStop)
    {
        var bot = GetBot(cfg)?.Bot;
        return base.Remove(cfg, callStop);
    }

    public override void StartAll()
    {
        InitializeStart();

        if (!Hub.Config.SkipConsoleBotCreation)
            base.StartAll();
    }

    public override void InitializeStart()
    {
        if (RunOnce)
            return;

        AddIntegrations();
        AddTradeBotMonitors();

        base.InitializeStart();
    }

    public override void StopAll()
    {
        base.StopAll();
    }

    public override void PauseAll()
    {
        if (!Hub.Config.SkipConsoleBotCreation)
            base.PauseAll();
    }

    public override void ResumeAll()
    {
        if (!Hub.Config.SkipConsoleBotCreation)
            base.ResumeAll();
    }

    private void AddTradeBotMonitors()
    {
        var path = Hub.Config.Folder.DumpFolder;
        if (Hub.Config.Folder.Dump && !Directory.Exists(path))
            LogUtil.LogError("The program is configured to dump files, but the dump folder was not found. Please verify that it exists!", "Hub");
    }

    public PokeRoutineExecutorBase CreateBotFromConfig(PokeBotState cfg) => Factory.CreateBot(Hub, cfg);
    public BotSource<PokeBotState>? GetBot(PokeBotState state) => base.GetBot(state);
    void IPokeBotRunner.Remove(IConsoleBotConfig state, bool callStop) => Remove(state, callStop);
    public void Add(PokeRoutineExecutorBase newbot) => Add((RoutineExecutor<PokeBotState>)newbot);
    public bool SupportsRoutine(PokeRoutineType t) => Factory.SupportsRoutine(t);
}

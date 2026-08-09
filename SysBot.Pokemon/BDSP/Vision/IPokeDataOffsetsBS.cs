using System.Collections.Generic;

namespace SysBot.Pokemon;

public interface IPokeDataOffsetsBS
{
    public IReadOnlyList<long> BoxStartPokemonPointer { get; }
    public IReadOnlyList<long> SceneIDPointer { get; }
    public IReadOnlyList<long> MyStatusTrainerPointer { get; }
    public IReadOnlyList<long> MyStatusTIDPointer { get; }
    public IReadOnlyList<long> ConfigTextSpeedPointer { get; }
    public IReadOnlyList<long> ConfigLanguagePointer { get; }
    public IReadOnlyList<long> MainRNGPointer { get; }
    public IReadOnlyList<long> ZoneIDPointer { get; }
}

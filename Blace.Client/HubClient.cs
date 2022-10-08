using Blace.Shared;
using Blace.Shared.Models;

namespace Blace.Client;

public abstract class HubClient : IClient
{
    public virtual Task UpdateQuestion(Question question) => Task.CompletedTask;
    public virtual Task ShowAnswer() => Task.CompletedTask;
    public virtual Task UpdatePlayers(List<Player> players) => Task.CompletedTask;
    public virtual Task UpdatePixels(Pixel[] pixels) => Task.CompletedTask;
    public virtual Task UpdatePlace(Place place) => Task.CompletedTask;
    public virtual Task UpdateCooldown(uint cooldown) => Task.CompletedTask;
    public virtual Task UpdateState(State state) => Task.CompletedTask;
    public virtual Task ShowVoteResult(int[] result) => Task.CompletedTask;
}
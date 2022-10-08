using Blace.Shared.Models;

namespace Blace.Shared;

public interface IClient
{
    Task UpdateQuestion(Question question);
    Task ShowAnswer();
    Task UpdatePlayers(List<Player> players);
    Task UpdatePixels(Pixel[] pixels);
    Task UpdatePlace(Place place);
    Task UpdateCooldown(uint cooldown);
    Task UpdateState(State state);
    Task ShowVoteResult(int[] result);
}

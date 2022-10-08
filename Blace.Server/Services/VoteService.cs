using System.Collections.Concurrent;
using Blace.Shared;
using Microsoft.AspNetCore.SignalR;

namespace Blace.Server.Services;

public class VoteService
{
    private readonly IHubContext<Server, IClient> _hub;

    private readonly ConcurrentDictionary<Guid, byte> _votes = new();

    public VoteService(IHubContext<Server, IClient> hub)
    {
        _hub = hub;
    }

    public void Vote(Guid id, byte index)
    {
        _votes[id] = index;
    }

    public void ShowResult()
    {
        _hub.Clients.All.ShowVoteResult(GetResult());
        _votes.Clear();
    }

    private int[] GetResult()
    {
        ICollection<byte> values = _votes.Values;
        int[] result = new int[4];
        foreach (byte index in values)
            result[index]++;
        return result;
    }
}
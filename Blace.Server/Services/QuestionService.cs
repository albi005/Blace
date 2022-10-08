using System.Collections.Concurrent;
using Blace.Shared;
using Blace.Shared.Models;
using Microsoft.AspNetCore.SignalR;

namespace Blace.Server.Services;

public class QuestionService
{
    private readonly ScoreboardService _scoreboardService;
    private readonly QuestionRepository _questionRepository;
    private readonly PlayerService _playerService;
    private readonly IHubContext<Server, IClient> _hub;

    private readonly ConcurrentDictionary<Player, bool> _answers = new();

    private int _questionIndex = -1;
    
    public QuestionService(ScoreboardService scoreboardService, QuestionRepository questionRepository, IHubContext<Server, IClient> hub, PlayerService playerService)
    {
        _scoreboardService = scoreboardService;
        _questionRepository = questionRepository;
        _hub = hub;
        _playerService = playerService;
    }

    public bool CanShowNext => _questionIndex + 1 < _questionRepository.Questions.Length && Question == null;
    public Question? Question { get; private set; }

    public void ShowNext()
    {
        if (!CanShowNext) return;

        Question = _questionRepository.Questions[++_questionIndex];
        _hub.Clients.All.UpdateQuestion(Question);
    }

    public void AnswerQuestion(Player player, bool isCorrect)
    {
        _answers[player] = isCorrect;
    }

    public void ShowAnswer()
    {
        Question = null;
        foreach (KeyValuePair<Player,bool> answer in _answers)
        {
            if (answer.Value)
                answer.Key.Score++;
        }
        _answers.Clear();
        _hub.Clients.All.ShowAnswer();
        _playerService.Update();
    }

    public void Reset()
    {
        Question = null;
        _questionIndex = -1;
        _answers.Clear();
        _scoreboardService.Reset();
    }
}
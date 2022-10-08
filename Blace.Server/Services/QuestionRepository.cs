using Blace.Shared.Models;

namespace Blace.Server.Services;

public class QuestionRepository
{
    public Question[] Questions { get; } =
    {
        new("Question?",
            new("Correct answer"),
            new()
            {
                new("Wrong answer 1"),
                new("Wrong answer 2"),
                new("Wrong answer 3")
            },
            "img/qr-code.svg"
        ),
    };
}

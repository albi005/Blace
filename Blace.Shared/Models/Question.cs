using System.Text.Json.Serialization;

namespace Blace.Shared.Models;

public record Question(
    string Prompt,
    Answer CorrectAnswer,
    List<Answer> IncorrectAnswers,
    string? Image = null)
{
    private static readonly Random s_random = new();
    private Answer[]? _randomizedAnswers;

    [JsonIgnore]
    public IEnumerable<Answer> RandomizedAnswers
    {
        get
        {
            if (_randomizedAnswers != null) return _randomizedAnswers;

            _randomizedAnswers = new Answer[IncorrectAnswers.Count + 1];
            IncorrectAnswers.CopyTo(_randomizedAnswers);
            _randomizedAnswers[^1] = CorrectAnswer;
            Shuffle(_randomizedAnswers);
            return _randomizedAnswers;
        }
    }

    private static void Shuffle(IList<Answer> array)
    {
        for (int i = array.Count - 1; i > 0; i--)
        {
            int j = s_random.Next(i + 1);
            (array[i], array[j]) = (array[j], array[i]);
        }
    }
}
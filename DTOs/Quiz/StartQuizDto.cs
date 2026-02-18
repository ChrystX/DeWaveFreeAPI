using System.Text.Json;
using System.Text.Json.Serialization;

namespace DeWaveFreeAPI.DTOs.Quiz
{
    public class StartQuizDto
    {
        public int LessonId { get; set; }
    }

    public class SubmitQuizDto
    {
        public int AttemptId { get; set; }
        public Dictionary<string, string> Answers { get; set; } = new();  // { "blockId": "answer" }
    }

    // for deserializing data_json
    public class ExamSettingsJson     
    {
        [JsonPropertyName("passing_score")]
        public decimal PassingScore { get; set; } = 70;

        [JsonPropertyName("time_limit_seconds")]
        public int? TimeLimitSeconds { get; set; }

        [JsonPropertyName("max_retries")]
        public int? MaxRetries { get; set; }

        [JsonPropertyName("shuffle_questions")]
        public bool ShuffleQuestions { get; set; } = false;

        [JsonPropertyName("exam_mode")]
        public string ExamMode { get; set; } = "high_stakes";
    }

    public class AbandonQuizDto
    {
        public int AttemptId { get; set; }
    }

    public class UpdateExamSettingsDto
    {
        public string? SettingsJson { get; set; }
    }

    public class QuestionJson
    {
        [JsonPropertyName("type")]
        public string Type { get; set; } = string.Empty;

        [JsonPropertyName("question")]
        public string Question { get; set; } = string.Empty;

        [JsonPropertyName("correct_index")]
        public int? CorrectIndex { get; set; }

        [JsonPropertyName("correct_indices")]
        public List<int>? CorrectIndices { get; set; }

        [JsonPropertyName("correct_answer")]
        public JsonElement? CorrectAnswer { get; set; }

        [JsonPropertyName("points")]
        public int Points { get; set; } = 1;

        [JsonPropertyName("weight")]
        public double? Weight { get; set; }    // null = default 1.0
    }
}

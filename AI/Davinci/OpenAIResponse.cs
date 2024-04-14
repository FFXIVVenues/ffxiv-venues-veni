namespace FFXIVVenues.Veni.AI.Davinci
{
    class OpenAIResponse
    {
        public string id { get; set; }
        public string @object { get; set; }
        public long created { get; set; }
        public string model { get; set; }
        public OpenAIChoice[] choices { get; set; }
        public OpenAIUsage usage { get; set; }
    }

    class OpenAIChoice
    {
        public string text { get; set; }
        public int index { get; set; }
        public float?[] logprobs { get; set; }
        public string finish_reason { get; set; }
    }

    class OpenAIUsage
    {
        public int prompt_tokens { get; set; }
        public int completion_tokens { get; set; }
        public int total_tokens { get; set; }
    }

    public class Choice
    {
        public int index { get; set; }
        public Message message { get; set; }
        public object logprobs { get; set; }
        public string finish_reason { get; set; }
    }

    public class Message
    {
        public string role { get; set; }
        public string content { get; set; }
    }
}

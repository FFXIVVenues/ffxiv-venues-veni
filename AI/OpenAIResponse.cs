namespace FFXIVVenues.Veni.AI
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
}
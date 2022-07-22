using System;

namespace FFXIVVenues.Veni
{
    static class MessageRepository
    {

        public static string[] StoppedMessage = new[]
        {
            "Okay! Cancelled.",
            "Oki, we'll stop. :slight_smile:",
            "Consider it dropped! :sweat_smile:",
        };

        public static string[] CreateVenueMessage = new[]
        {
            "Awesome! Great to see another venue join the community! ♥️",
            "Wooo! Congratulations on the new venue! ♥️",
            "Exciting! ♥️",
            "Great! ♥️",
            "Pog! ♥️"
        };

        public static string[] AskForNameMessage = new[]
        {
            "Let's get started. What is the **name** of your venue?",
            "What are we **naming** it?",
            "Alrighty, what is the **name**? :smile:",
            "What's the **name** of your venue?",
            "Please, tell me the **name**!"
        };

        public static string[] AskForDataCenterMessage = new[]
        {
            "Which **Data Center** are you in? (Aether, Primal, Crystal, Elemental, Gaia, Mana, Chaos, Light)",
            "And which **Data Center** is it? (Aether, Primal, Crystal, Elemental, Gaia, Mana, Chaos, Light)",
            "Which **Data Center** is the venue in? (Aether, Primal, Crystal, Elemental, Gaia, Mana, Chaos, Light)"
        };

        public static string[] AskForWorldMessage = new[]
        {
            "Which **world** are you in?",
            "Which **world** is your venue in?"
        };

        public static string[] AskForHousingDistrictMessage = new[]
        {
            "Which **housing district** are you in?",
            "Which **housing district** is your venue in?"
        };

        public static string[] AskForWardMessage = new[]
        {
            "Which **number ward** are you in? (1-24)",
            "Which **number ward** is your venue in? (1-24)"
        };

        public static string[] AskForPlotMessage = new[]
        {
            "and the **plot number**? (1-60)",
            "Which **number plot** are you? (1-60)",
            "Which **number plot** is your venue? (1-60)"
        };

        public static string[] AskForSubdivisionMessage = new[]
        {
            "Are you in the **subdivision**?",
            "Is your venue in the **subdivision**?"
        };

        public static string[] AskForApartmentMessage = new[]
        {
            "and the **apartment number**?",
            "Which **number apartment** are you?",
            "Which **number apartment** is your venue?"
        };

        public static string[] AskForDescriptionMessage = new[]
        {
            "Could you give me an ad or **description** of the venue? Something for players to read when they click on your venue.",
            "Could you write a **introduction/advertisement** of the venue? Players will see it in your card on the index. :)"
        };

        public static string[] AskForSfwMessage = new[]
        {
            "Is it usually safe for someone looking for an **SFW** experience to walk into?",
        };

        public static string[] AskForType = new[]
        {
            "Select the **type** that most suites your venue. 🙂",
            "Which of these **types** apply to your venue most?"
        };

        public static string[] AskForTags = new[]
        {
            "Select all the **tags** that apply to your venue. :smile:",
            "Which of these **tags** apply to the venue?"
        };

        public static string[] AskForHouseOrApartmentMessage = new[]
        {
            "Is your venue in a **house or apartment**?",
            "Is it a **house or apartment**?",
        };

        public static string[] AskForWebsiteMessage = new[]
        {
            "What is your **website address**?",
        };

        public static string[] AskForDiscordMessage = new[]
        {
            "What is your venue's permanent **discord invite** link?",
        };

        public static string[] ConfirmMessage = new[]
        {
            "Great!",
            "Alright, got it!",
            "Got it!",
            "Nice!",
            "Niicuuuu!",
            "Looks good!",
            "Thanks!",
            "Mhm, understood.",
            "Alrighty.",
        };

        public static string[] VenueOpenMessage = new[]
        {
            "Woo! The doors are open. You're green and announcements have been sent! Let's have fun today! ♥️",
            "Yay! It's that time again. 😀 You're all green on the index, and everyone's been notified. ♥️",
            "Let's do it! We... are... live!!! We're green on the index and the pings are flying! So excited. 🙂"
        };

        public static string[] VenueClosedMessage = new[]
        {
            "The doors are closed! I can't wait til next time. ♥️",
            "We're no longer green! We'll close up. ",
            "Okay! I'll lock up. 😉"
        };

        public static string[] DontUnderstandResponses = new[]
        {
            "I don't understand. :smiling_face_with_tear:",
            "Huh? I don't get it.",
            "Hmm?",
            "Huh?",
            "Kweh!?!",
            "Kupo?"
        };

        public static string[] ShowVenueResponses = new[]
        {
            "Okay, here you go! 🥰",
            "Here you go, hun! 💓",
            "Here's what I've found! 💓"
        };

        public static string[] AskDaysOpenMessage = new[]
        {
            "What days are you open?",
            "What days is the venue open each week?"
        };


        static Random random = new Random();
        public static string PickRandom(this string[] messageList)
        {
            if (messageList.Length == 0)
                return null;

            var index = random.Next(messageList.Length);
            return messageList[index];
        }

    }
}

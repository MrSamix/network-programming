namespace Message
{
    public class MessageInfo
    {
        public string? Username { get; set; }
        public string? Message { get; set; }
        public string? Answer { get; set; }
        private DateTime time;
        public string Time => time.ToLongTimeString();
        public string? MyUsername { get; set; } // для відображення справа
        public MessageInfo(string username, string? myUsername, string? message, string? answer, DateTime time)
        {
            Username = username;
            Message = message;
            Answer = answer;
            this.time = time;
            MyUsername = myUsername;
        }
        public override string ToString()
        {
            return $"{Username,-20} {Message,-20} {Time}";
        }
    }
}

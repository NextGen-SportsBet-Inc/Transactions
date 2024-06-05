namespace Shared.Messages
{
    public record UserExistsRequest
    {
        public String? UserId { get; set; }
    }

    public record UserExistsResponse
    {
        public bool UserIsValid { get; set; }
    }

    public record CheckAmountRequest
    {
        public String? UserId { get; set; }

    }

    public record CheckAmountResponse
    {
        public bool Error { get; set; }

        public float? Amount { get; set; }

    }

    public record RemoveAmountRequest
    {
        public String? UserId { get; set; }

        public float? AmountToRemove { get; set; }

    }

    public record RemoveAmountResponse
    {
        public bool Success { get; set; }

        public string? ErrorMessage { get; set; }

    }

    public record AddAmountRequest
    {
        public String? UserId { get; set; }

        public float? AmountToAdd { get; set; }
    }


    public record ChangeBetStatusRequest
    {
        public required int MatchId { get; set; }

        public required int TeamId { get; set; }

        public required float Odd { get; set; }
    }

}
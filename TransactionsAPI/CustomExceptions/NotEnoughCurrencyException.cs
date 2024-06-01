namespace TransactionsAPI.CustomExceptions
{
    public class NotEnoughCurrencyException : Exception
    {
        public NotEnoughCurrencyException() { }

        public NotEnoughCurrencyException(string message)
            : base(message) { }
    }
}

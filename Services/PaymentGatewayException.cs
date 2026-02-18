namespace DeWaveFreeAPI.Services
{
    public class PaymentGatewayException : Exception
    {
        public PaymentGatewayException(string message) : base(message) { }
    }
}

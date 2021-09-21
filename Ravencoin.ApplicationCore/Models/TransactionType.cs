namespace Ravencoin.ApplicationCore.Models
{
    public enum TType
    {
        RVN,
        Asset,
        Fee
    }
    public enum CType
    {
        Receive,
        Send
    }
    public class TransactionType
    {
        public TType transactionTypes { get; set; }

        public CType categoryTypes { get; set; }

    }
}

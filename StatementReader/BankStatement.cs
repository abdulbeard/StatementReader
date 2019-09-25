using System;
using System.Collections.Generic;
using System.Linq;

namespace StatementReader
{
    public class BankStatement
    {
        public BankStatement()
        {
            LineItems = new List<LineItem>();
        }
        public BankInfo Bank { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public decimal StartingBalance { get; set; }
        public decimal EndingBalance { get; set; }
        public DocumentMeta DocumentInfo { get; set; }
        public List<LineItem> LineItems { get; set; }
        public string MaskedAccountNumber { get; set; }
        public TotalsMeta TotalsMeta { get; set; }
        public TotalsMeta CalculateTotalsMeta()
        {
            return new TotalsMeta
            {
                HighestDeposit = LineItems?.Where(x => x.Amount > 0)?.Max(x => x.Amount) ?? 0,
                HighestWithdrawal = LineItems?.Where(x => x.Amount < 0)?.Min(x => x.Amount) ?? 0,
                LowestDeposit = LineItems?.Where(x => x.Amount > 0)?.Min(x => x.Amount) ?? 0,
                LowestWithdrawal = LineItems?.Where(x => x.Amount < 0)?.Max(x => x.Amount) ?? 0,
                TotalsTally = LineItems?.Sum(x => x.Amount) == (EndingBalance - StartingBalance)
            };
        }
    }

    public class BankInfo
    {
        public string Name { get; set; }
        public Uri Website { get; set; }
        public string Address { get; set; }
        public string PhoneNumber { get; set; }
    }

    public class TotalsMeta
    {
        public bool TotalsTally { get; set; }
        public decimal HighestWithdrawal { get; set; }
        public decimal LowestWithdrawal { get; set; }
        public decimal HighestDeposit { get; set; }
        public decimal LowestDeposit { get; set; }
    }

    public class DocumentMeta
    {
        public string Format { get; set; }
        public string Version { get; set; }
        public string Author { get; set; }
        public string Creator { get; set; }
        public string Producer { get; set; }
    }

    public class LineItem
    {
        public LineItem()
        {
            InternalId = Guid.NewGuid();
        }
        public Guid InternalId { get; set; }
        public string Id { get; set; }
        public string Memo { get; set; }
        public string Merchant { get; set; }
        public int MerchantCategoryCode { get; set; }
        public DateTime Date { get; set; }
        public string Description { get; set; }
        public decimal Amount { get; set; }
    }
}

using System;
using System.Collections.Generic;

namespace StatementReader
{
    public class BankStatement
    {
        public BankInfo Bank { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public decimal StartingBalance { get; set; }
        public decimal EndingBalance { get; set; }
        public DocumentMeta DocumentInfo { get; set; }
        public List<LineItem> LineItems { get; set; }
        public string LastFourAccountNumber { get; set; }
    }

    public class BankInfo
    {
        public string Name { get; set; }
        public Uri Website { get; set; }
        public string Address { get; set; }
        public string PhoneNumber { get; set; }
    }

    public class DocumentMeta
    {
        public string Format { get; set; }
        public string Version { get; set; }
        public string Author { get; set; }
        public string Creator { get; set; }
    }

    public class LineItem
    {
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

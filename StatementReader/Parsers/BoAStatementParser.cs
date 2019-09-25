using iTextSharp.text.pdf;
using iTextSharp.text.pdf.parser;
using System;
using System.Collections.Generic;
using System.IO;
using StatementReader.Utils;
using System.Text.RegularExpressions;
using System.Linq;

namespace StatementReader.Parsers
{
    public class BoAStatementParser
    {
        private readonly Regex lineItemStartRegex = new Regex(@"^..\/..\/..");
        private readonly Regex lineItemEndRegex = new Regex(@"[+-]?\d*\.\d\d*$");
        private readonly Regex beginningBalanceRegex = new Regex(@"[b,B]eginning balance on (\w*\s)(\d\d),\s(\d\d\d\d)\s\$(.*)");
        private readonly Regex endingBalanceRegex = new Regex(@"[e,E]nding balance on (\w*\s)(\d\d),\s(\d\d\d\d)\s\$(.*)");
        private readonly Regex customerServicePhone = new Regex(@"[c,C]ustomer service: (.*)");
        private readonly Regex accountNumber = new Regex(@"[a,A]ccount number: (\d\d\d\d\s\d\d\d\d\s\d\d\d\d)");
        private readonly Regex id = new Regex(@"[i,I][d,D]:(\w*)\s");
        private readonly Regex mcc = new Regex(@"\s(\d\d\d\d)\s");

        private bool foundEndingDate = false;
        private bool foundEndingBalance = false;
        private bool foundBeginDate = false;
        private bool foundBeginBalance = false;
        private bool foundAccountNumber = false;

        public BankStatement Parse()
        {
            var result = new BankStatement();


            var pdfPath = @"C:\Users\azaheer\Downloads\Personal\eStmt_2019-09-12.pdf";
            var visaMccPath = @"C:\Users\azaheer\Downloads\visa-merchant-data-standards-manual.pdf";
            var pdfStream = new StreamReader(pdfPath);
            //var sadf = new ParserContext(pdfPath);
            //var doc = new PDFDocumentParser(sadf);
            //var sdlfkj = doc.Parse();
            //var matches = sdlfkj.Paragraphs.Where(x => lineItemStartRegex.IsMatch(x.Text)).Select(x => x).ToList();

            //var lines = new List<LineItem>();
            //var pdfReader = new PdfReader(pdfPath);
            var pdfReader = new PdfReader(pdfStream.BaseStream);

            result.DocumentInfo = GetDocumentMeta(pdfReader);
            result.Bank = new BankInfo
            {
                Address = "P.O. Box 15284, Wilmington, DE 19850",
                Name = "Bank of America",
                Website = new Uri("http://bankofamerica.com")
            };

            for (var i = 1; i <= pdfReader.NumberOfPages; i++)
            {
                //var text1 = pdfReader.GetPageContent(i);
                var lines = PdfTextExtractor.GetTextFromPage(pdfReader, i, new SimpleTextExtractionStrategy()).Split(new char[] { '\n' }, StringSplitOptions.RemoveEmptyEntries);
                for (var j = 0; j < lines.Length; j++)
                {
                    var item = lines[j].Trim();

                    if (i <= 1)
                    {
                        result = ParseDatesAndBalances(result, item);
                        result = GetBankPhoneNumber(result, item);
                        if (!foundAccountNumber) { result.MaskedAccountNumber = GetMaskedAccountNumber(item); }
                    }

                    if (lineItemStartRegex.IsMatch(item))
                    {
                        if (lineItemEndRegex.IsMatch(item))
                        {
                            var lineSplit = item.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                            var date = Parsing.ParseDate(lineSplit[0]);
                            var desc = Parsing.ParseDescription(lineSplit.Skip(1).Take(lineSplit.Length - 2));
                            var amnt = Parsing.ParseAmount(lineSplit.Last());
                            var id = FindId(desc);
                            var mcc = GetMerchantCategoryCode(desc);
                            result.LineItems.Add(new LineItem { Date = date, Amount = amnt, Description = desc, Id = id, MerchantCategoryCode = mcc });
                        }
                        else
                        {
                            var date = Parsing.ParseDate(item.Substring(0, 8));
                            var desc = new List<string>();
                            desc.AddRange(item.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries).Skip(1));
                            while (!lineItemEndRegex.IsMatch(lines[j + 1]))
                            {
                                desc.AddRange(lines[j + 1].Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries));
                                j++;
                            }
                            j++;
                            var amountSplit = lines[j].Split(' ');
                            var amnt = Parsing.ParseAmount(amountSplit.Last());
                            desc.AddRange(amountSplit.Take(amountSplit.Length - 2));
                            var description = Parsing.ParseDescription(desc);
                            var id = FindId(description);
                            var mcc = GetMerchantCategoryCode(description);
                            result.LineItems.Add(new LineItem { Date = date, Amount = amnt, Description = description, Id = id, MerchantCategoryCode = mcc });
                        }
                    }
                }
            }
            result.TotalsMeta = result.CalculateTotalsMeta();
            return result;
        }

        private string GetMaskedAccountNumber(string item)
        {
            if (accountNumber.IsMatch(item))
            {
                var matches = accountNumber.Matches(item);
                if (matches.Count == 1 && matches[0].Groups.Count == 2)
                {
                    var accountNumber = matches[0].Groups[1].Value.Replace(" ", "");
                    foundAccountNumber = true;
                    return accountNumber.Substring(accountNumber.Length - 4).PadLeft(accountNumber.Length, '*');
                }
            }
            return "";
        }

        private string FindId(string item)
        {
            if (id.IsMatch(item))
            {
                var matches = id.Matches(item);
                if (matches.Count >= 1 && matches[0].Groups.Count == 2)
                {
                    return matches[0].Groups[1].Value;
                }
            }
            return "";
        }

        private int GetMerchantCategoryCode(string item)
        {
            if (mcc.IsMatch(item))
            {
                var matches = mcc.Matches(item);
                if (matches.Count == 1 && matches[0].Groups.Count == 2)
                {
                    var code = matches[0].Groups[1].Value;
                    return int.Parse(code);
                }
            }
            return 0;
        }

        private BankStatement GetBankPhoneNumber(BankStatement result, string item)
        {
            if (customerServicePhone.IsMatch(item))
            {
                var matches = customerServicePhone.Matches(item);
                if (matches.Count == 1 && matches[0].Groups.Count == 2)
                {
                    result.Bank.PhoneNumber = matches[0].Groups[1].Value;
                }
            }
            return result;
        }

        private DocumentMeta GetDocumentMeta(PdfReader reader)
        {
            return new DocumentMeta
            {
                Author = reader.Info[nameof(DocumentMeta.Author)],
                Creator = reader.Info[nameof(DocumentMeta.Creator)],
                Format = "Pdf",
                Producer = reader.Info[nameof(DocumentMeta.Producer)],
                Version = reader.PdfVersion.ToString()
            };
        }

        private BankStatement ParseDatesAndBalances(BankStatement result, string item)
        {
            if (!foundBeginBalance && !foundBeginDate && beginningBalanceRegex.IsMatch(item))
            {
                var matches = beginningBalanceRegex.Matches(item);
                if (matches.Count == 1 && matches[0].Groups.Count == 5)
                {
                    result.StartDate = DateTime.Parse($"{matches[0].Groups[1].Value} {matches[0].Groups[2].Value}, {matches[0].Groups[3].Value}");
                    result.StartingBalance = decimal.Parse(matches[0].Groups[4].Value);
                    foundBeginBalance = true;
                    foundBeginDate = true;
                }
            }
            if (!foundEndingBalance && !foundEndingDate && endingBalanceRegex.IsMatch(item))
            {
                var matches = endingBalanceRegex.Matches(item);
                if (matches.Count == 1 && matches[0].Groups.Count == 5)
                {
                    result.EndDate = DateTime.Parse($"{matches[0].Groups[1].Value} {matches[0].Groups[2].Value}, {matches[0].Groups[3].Value}");
                    result.EndingBalance = decimal.Parse(matches[0].Groups[4].Value);
                }
                foundEndingBalance = true;
                foundEndingDate = true;
            }
            return result;
        }
    }
}

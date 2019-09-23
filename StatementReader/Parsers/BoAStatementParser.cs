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
        public BankStatement Parse()
        {
            var lineItemStartRegex = new Regex(@"^..\/..\/..");
            var lineItemEndRegex = new Regex(@"[+-]?\d*\.\d\d*$");
            var pdfPath = @"C:\Users\azaheer\Downloads\Personal\eStmt_2019-09-12.pdf";
            var visaMccPath = @"C:\Users\azaheer\Downloads\visa-merchant-data-standards-manual.pdf";
            var pdfStream = new StreamReader(pdfPath);
            //var sadf = new ParserContext(pdfPath);
            //var doc = new PDFDocumentParser(sadf);
            //var sdlfkj = doc.Parse();
            //var matches = sdlfkj.Paragraphs.Where(x => lineItemStartRegex.IsMatch(x.Text)).Select(x => x).ToList();

            var lines = new List<LineItem>();
            //var pdfReader = new PdfReader(pdfPath);
            var pdfReader = new PdfReader(pdfStream.BaseStream);
            for (var i = 1; i <= pdfReader.NumberOfPages; i++)
            {
                //var text1 = pdfReader.GetPageContent(i);
                var split = PdfTextExtractor.GetTextFromPage(pdfReader, i, new SimpleTextExtractionStrategy()).Split(new char[] { '\n' }, StringSplitOptions.RemoveEmptyEntries);
                for (var j = 0; j < split.Length; j++)
                {
                    var item = split[j].Trim();
                    if (lineItemStartRegex.IsMatch(item))
                    {
                        if (lineItemEndRegex.IsMatch(item))
                        {
                            var lineSplit = item.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                            var date = Parsing.ParseDate(lineSplit[0]);
                            var desc = Parsing.ParseDescription(lineSplit.Skip(1).Take(lineSplit.Length - 2));
                            var amnt = Parsing.ParseAmount(lineSplit.Last());
                            lines.Add(new LineItem { Date = date, Amount = amnt, Description = desc });
                        }
                        else
                        {
                            var date = Parsing.ParseDate(item.Substring(0, 8));
                            var desc = new List<string>();
                            desc.AddRange(item.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries).Skip(1));
                            while (!lineItemEndRegex.IsMatch(split[j + 1]))
                            {
                                desc.AddRange(split[j + 1].Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries));
                                j++;
                            }
                            j++;
                            var amountSplit = split[j].Split(' ');
                            var amnt = Parsing.ParseAmount(amountSplit.Last());
                            desc.AddRange(amountSplit.Take(amountSplit.Length - 2));
                            var description = Parsing.ParseDescription(desc);
                            lines.Add(new LineItem { Date = date, Amount = amnt, Description = description });
                        }
                    }
                }
            }

            var totalSubtractions = lines.Where(x => x.Amount < 0M).Sum(x => x.Amount);
            var totalAdditions = lines.Where(x => x.Amount >= 0).Sum(x => x.Amount);
            return null;
        }
    }
}

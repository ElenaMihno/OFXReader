using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Xml.Serialization;
using OFX.Objects;


namespace OFX
{
	class Program
	{
		static void Main(string[] args)
		{
			FixStatement("anz");
			FixStatement("bankdirect");
			FixStatement("homeloan");
			FixStatement("topup");
			Console.ReadLine();

		}

		public static void FixStatement(string prefix)
		{
			StringBuilder output = new StringBuilder();
			StreamReader file = new StreamReader("C:\\projects\\ofx\\ofx\\" + prefix + ".ofx");
			string line;

			Console.WriteLine("=============================================================");
			Console.WriteLine(prefix);
			Console.WriteLine();

			while ((line = file.ReadLine()) != null)
			{
				// Get past the header
				if (line.StartsWith("<") == false) continue;

				string openingTag = line.Substring(1, line.IndexOf(">") - 1);
				string closingTag = string.Empty;

				int openingTags = line.Count(r => r == '<');
				if (openingTags > 1)
				{
					// There are two tags on this line
					// Check that both are equal
					int length = line.Length;
					int lastOpeningMarkerIndex = line.LastIndexOf("<");
					closingTag = line.Substring(lastOpeningMarkerIndex + 1, length - lastOpeningMarkerIndex - 2);
					if (closingTag.StartsWith("/")) closingTag = closingTag.Substring(1);

					if (closingTag != openingTag)
					{
						line = line.Insert(lastOpeningMarkerIndex, "</" + openingTag + ">");
					}
				}
				else if (line.EndsWith(">") == false)
				{
					line += "</" + openingTag + ">";
				}
				
				output.Append(line);


			}
			file.Close();


			// Fix bad xml
			output = WrapTransactions(output);

			File.WriteAllText(@"c:\projects\ofx\ofx\" + prefix + ".xml", output.ToString());

			XmlRootAttribute xRoot = new XmlRootAttribute();
			xRoot.ElementName = "OFX";
			xRoot.IsNullable = true;

			var serializer = new XmlSerializer(typeof(OFXObject), xRoot);
			serializer.UnknownElement += new XmlElementEventHandler(helper_UnknownElement);

			OFXObject statement;

			using (TextReader reader = new StringReader(output.ToString()))
			{
				statement = (OFXObject)serializer.Deserialize(reader);
			}



			//Console.WriteLine("------------------------------------------------");
			//Console.WriteLine(bankStatement.SignOnMessageSet.Response.Status.Code);
			//Console.WriteLine(bankStatement.SignOnMessageSet.Response.ServerDate);
			//Console.WriteLine(bankStatement.SignOnMessageSet.Response.Language);
			//Console.WriteLine("Available balance: " + bankStatement.CreditCardMessages.Response.Statement.AvailableBalance.Amount);
			//Console.WriteLine("Ledger balance: " + bankStatement.CreditCardMessages.Response.Statement.LedgerBalance.Amount);
			//Console.WriteLine("From: " + bankStatement.CreditCardMessages.Response.Statement.TransactionDetails.StartDate);
			//Console.WriteLine("To: " + bankStatement.CreditCardMessages.Response.Statement.TransactionDetails.EndDate);
			//Console.WriteLine("------------------------------------------------");
			//Console.WriteLine("Transactions: " + bankStatement.CreditCardMessages.Response.Statement.TransactionDetails.Transactions.Count.ToString());
			//Console.WriteLine("Start: " + bankStatement.CreditCardMessages.Response.Statement.TransactionDetails.StartDate);
			//Console.WriteLine("End: " + bankStatement.CreditCardMessages.Response.Statement.TransactionDetails.EndDate);
			//Console.WriteLine("------------------------------------------------");

			List<OFXTransaction> transactions = statement.Transactions;
			//if (statement.StatementType == StatementType.CreditCard)
			//    transactions = statement.CreditCardMessages.Response.Statement.TransactionDetails.Transactions;
			//else if (statement.StatementType == StatementType.BankAccount)
			//    transactions = statement.BankMessages.Response.Statement.TransactionDetails.Transactions;

			//transactions.ForEach(transaction =>
			//{
			//    Console.WriteLine("ID: " + transaction.TransactionID.ToString());
			//    Console.WriteLine("Type: " + transaction.Type.ToString());
			//    Console.WriteLine("Amount: " + transaction.Amount.ToString());
			//    Console.WriteLine("Name: " + transaction.Name);
			//    Console.WriteLine("------------------------------------------------");
			//});

			Console.WriteLine("------------------------------------------------");
			Console.WriteLine("PAYEE SUMMARY");
			Console.WriteLine("------------------------------------------------");

			var query = (from t in transactions
						group t by t.Name into k
						select new { Payee = k.Key, Total = k.Sum(p=>p.Amount) })
						.OrderByDescending(i=>i.Total);


			query.ToList().ForEach(q =>
			{
				Console.WriteLine(q.Payee.PadRight(35) + ": " + q.Total.ToString("C"));

			});

			var query2 = (from t in transactions
						 group t by t.Type into k
						 select new { Type = k.Key, Total = k.Sum(p=>p.Amount) })
						 .OrderByDescending(i=>i.Total);

			Console.WriteLine("------------------------------------------------");
			Console.WriteLine("INCOME vs SPENDING");
			Console.WriteLine("------------------------------------------------");

			query2.ToList().ForEach(q =>
			{
				Console.WriteLine(q.Type.PadRight(35) + ": " + q.Total.ToString("C"));
			});
			Console.WriteLine("BALANCE".PadRight(35) + ": " + statement.LedgerBalance.ToString("C"));
			Console.WriteLine("AVAILABLE".PadRight(35) + ": " + statement.AvailableBalance.ToString("C"));
			Console.WriteLine("LIMIT".PadRight(35) + ": " + statement.Limit.ToString("C"));
		}

		

		// Repair badly formated xml for transaction list
		private static StringBuilder WrapTransactions(StringBuilder output)
		{

			// Find the first STMTTRN
			// Place an opener before it
			// Find the last STMTTRN
			// Place a closer after it
			output.Insert(output.ToString().IndexOf("<STMTTRN>"), "<TRANSACTIONS>");
			string lastTransactionClosingTag = "</STMTTRN>";
			output.Insert(output.ToString().LastIndexOf(lastTransactionClosingTag) + lastTransactionClosingTag.Length, "</TRANSACTIONS>");

			return output;
		}

		public static void helper_UnknownElement(object sender, XmlElementEventArgs e)
		{
			Console.WriteLine("Unknown element found");
			Console.WriteLine("Line: " + e.LineNumber.ToString());
			Console.WriteLine("Element name: " + e.Element.Name);
		}











	}
}

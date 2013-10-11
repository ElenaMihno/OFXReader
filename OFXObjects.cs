using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;

namespace OFX.Objects
{
	[Serializable]
	public class OFXObject
	{
		[XmlElement(ElementName = "SIGNONMSGSRSV1")]
		public OFXSignOnMessageSet SignOnMessageSet { get; set; }

		[XmlElement(ElementName = "CREDITCARDMSGSRSV1")]
		public OFXCreditCardMessageSetResponseMessages CreditCardMessages { get; set; }
		
		[XmlElement(ElementName = "BANKMSGSRSV1")]
		public OFXBankMessageSetResponseMessages BankMessages { get; set; }

		public StatementType StatementType
		{
			get
			{
				if (CreditCardMessages != null) return StatementType.CreditCard;
				if (BankMessages != null) return StatementType.BankAccount;
				return StatementType.Unknown;
			}
		}


		public List<OFXTransaction> Transactions
		{
			get
			{
				if (StatementType == Objects.StatementType.BankAccount)
					return BankMessages.Response.Statement.TransactionDetails.Transactions;
				else if (StatementType == Objects.StatementType.CreditCard)
					return CreditCardMessages.Response.Statement.TransactionDetails.Transactions;
				return null;
			}
		}

		public double AvailableBalance
		{
			get
			{
				if (StatementType == Objects.StatementType.BankAccount)
					return 0;
				else if (StatementType == Objects.StatementType.CreditCard)
					return CreditCardMessages.Response.Statement.AvailableBalance.Amount;
				return 0;
			}
		}

		public double LedgerBalance
		{
			get
			{
				if (StatementType == Objects.StatementType.BankAccount)
					return BankMessages.Response.Statement.LedgerBalance.Amount;
				else if (StatementType == Objects.StatementType.CreditCard)
					return CreditCardMessages.Response.Statement.LedgerBalance.Amount;
				return 0;
			}
		}


		public double Limit
		{
			get
			{
				return LedgerBalance - AvailableBalance;
			}
		}


		public void 


	}

	[Serializable]
	public class OFXSignOnMessageSet
	{
		[XmlElement(ElementName = "SONRS")]
		public OFXSignOnResponse Response { get; set; }


	}

	

	
	

	#region Credit Cards

	[Serializable]
	public class OFXCreditCardMessageSetResponseMessages
	{

		[XmlElement(ElementName = "CCSTMTTRNRS")]
		public OFXCreditCardStatementTransactionsResponse Response { get; set; }


	}
	
	[Serializable]
	public class OFXCreditCardStatementTransactionsResponse
	{
		[XmlElement(ElementName = "TRNUID")]
		public string UniqueID { get; set; }

		[XmlElement(ElementName = "STATUS")]
		public OFXStatus Status { get; set; }

		[XmlElement(ElementName = "CCSTMTRS")]
		public OFXCreditCardStatementResponse Statement { get; set; }

	}

	[Serializable]
	public class OFXCreditCardStatementResponse
	{
		[XmlElement(ElementName = "CURDEF")]
		public string Currency { get; set; }

		[XmlElement(ElementName = "CCACCTFROM")]
		public OFXCreditCardAccountDetails AccountDetails { get; set; }

		[XmlElement(ElementName = "LEDGERBAL")]
		public OFXBalance LedgerBalance { get; set; }

		[XmlElement(ElementName = "AVAILBAL")]
		public OFXBalance AvailableBalance { get; set; }

		[XmlElement(ElementName = "BANKTRANLIST")]
		public OFXTransactionList TransactionDetails { get; set; }


	}

	[Serializable]
	public class OFXCreditCardAccountDetails
	{
		[XmlElement(ElementName = "ACCTID")]
		public string AccountNumber { get; set; }
	}
	#endregion

	#region Bank Accounts

	[Serializable]
	public class OFXBankMessageSetResponseMessages
	{

		[XmlElement(ElementName = "STMTTRNRS")]
		public OFXBankStatementTransactionsResponse Response { get; set; }


	}

	[Serializable]
	public class OFXBankStatementTransactionsResponse
	{
		[XmlElement(ElementName = "TRNUID")]
		public string UniqueID { get; set; }

		[XmlElement(ElementName = "STATUS")]
		public OFXStatus Status { get; set; }

		[XmlElement(ElementName = "STMTRS")]
		public OFXBankStatementResponse Statement { get; set; }

	}

	[Serializable]
	public class OFXBankStatementResponse
	{
		[XmlElement(ElementName = "CURDEF")]
		public string Currency { get; set; }

		[XmlElement(ElementName = "BANKACCTFROM")]
		public OFXBankAccountDetails AccountDetails { get; set; }

		[XmlElement(ElementName = "LEDGERBAL")]
		public OFXBalance LedgerBalance { get; set; }

		[XmlElement(ElementName = "BANKTRANLIST")]
		public OFXTransactionList TransactionDetails { get; set; }


	}

	[Serializable]
	public class OFXBankAccountDetails
	{
		[XmlElement(ElementName = "ACCTID")]
		public string AccountNumber { get; set; }

		[XmlElement(ElementName = "ACCTTYPE")]
		public string Type { get; set; }

		[XmlElement(ElementName = "BANKID")]
		public string BankID { get; set; }
	}

	#endregion

	#region Generic

	public enum StatementType
	{
		CreditCard,
		BankAccount,
		Unknown
	}


	[Serializable]
	public class OFXStatus
	{
		[XmlElement(ElementName = "CODE")]
		public string Code { get; set; }

		[XmlElement(ElementName = "SEVERITY")]
		public string Severity { get; set; }
	}

	[Serializable]
	public class OFXSignOnResponse
	{
		[XmlElement(ElementName = "DTSERVER")]
		public string ServerDate { get; set; }

		[XmlElement(ElementName = "DTPROFUP")]
		public string LastProfileUpdateDate { get; set; }

		[XmlElement(ElementName = "DTACCTUP")]
		public string LastAccountUpdate { get; set; }

		[XmlElement(ElementName = "LANGUAGE")]
		public string Language { get; set; }

		[XmlElement(ElementName = "STATUS")]
		public OFXStatus Status { get; set; }
	}

	[Serializable]
	public class OFXBalance
	{
		[XmlElement(ElementName = "BALAMT")]
		public double Amount { get; set; }

		[XmlElement(ElementName = "DTASOF")]
		public string EffectiveDate { get; set; }
	}

	[Serializable]
	public class OFXTransactionList
	{
		[XmlElement(ElementName = "DTSTART")]
		public string StartDate { get; set; }

		[XmlElement(ElementName = "DTEND")]
		public string EndDate { get; set; }

		[XmlArray("TRANSACTIONS")]
		[XmlArrayItem("STMTTRN")]
		public List<OFXTransaction> Transactions { get; set; }
	}


	[Serializable]
	public class OFXTransaction
	{
		[XmlElement(ElementName = "TRNTYPE")]
		public string Type { get; set; }

		[XmlElement(ElementName = "DTPOSTED")]
		public string DatePosted { get; set; }

		[XmlElement(ElementName = "DTUSER")]
		public string DateUser { get; set; }

		[XmlElement(ElementName = "TRNAMT")]
		public double Amount { get; set; }

		[XmlElement(ElementName = "FITID")]
		public string TransactionID { get; set; }

		[XmlElement(ElementName = "REFNUM")]
		public string ReferenceNumber { get; set; }

		[XmlElement(ElementName = "NAME")]
		public string Name { get; set; }

		[XmlElement(ElementName = "MEMO")]
		public string Memo { get; set; }

	}

	#endregion














}

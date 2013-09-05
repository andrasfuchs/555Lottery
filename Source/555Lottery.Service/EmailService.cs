using _555Lottery.DataModel;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace _555Lottery.Service
{
	internal class EmailService
	{
		private object lockObject = new object();
		private LogService log;

		private List<EmailQueueItem> emailQueue = new List<EmailQueueItem>();
		private List<EmailQueueItem> discardedEmailQueue = new List<EmailQueueItem>();
		private System.Timers.Timer timer;

		private string smtpServer;
		private int smtpPort;
		private NetworkCredential smtpCredentials;
		private bool smtpSSL;
		private int smtpTimeout;
		
		private MailAddress from;
		private MailAddress bcc;
		private MailAddress replyTo;
		public MailAddress[] AdminEmails;

		public EmailService(LogService log)
		{
			this.log = log;

			string path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "email.config");
			this.LoadConfig(path);

			timer = new System.Timers.Timer(10 * 1000);
			timer.Elapsed += timer_Elapsed;
			timer.Start();
		}

		void timer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
		{
			lock (lockObject)
			{
				while (emailQueue.Count > 0)
				{
					try
					{
						log.Log(LogLevel.Information, "EMAILSENDING", "Sending e-mail to '{0}' using template '{1}'...", emailQueue[0].EmailMessage.To[0].Address, emailQueue[0].Template);
						emailQueue[0].SmtpClient.Send(emailQueue[0].EmailMessage);
					}
					catch (Exception ex)
					{
						emailQueue[0].LastError = ex;
						emailQueue[0].ErrorCount++;

						log.LogException(ex);

						if (emailQueue[0].ErrorCount >= 5)
						{
							log.Log(LogLevel.Warning, "EMAILDISCARDED", "After trying for {0} times, an e-mail to '{1}' was moved to the discraded queue.", emailQueue[0].ErrorCount, emailQueue[0].EmailMessage.To[0].Address);
							discardedEmailQueue.Add(emailQueue[0]);
							emailQueue.RemoveAt(0);
						}
					}

					if (emailQueue[0].LastError == null)
					{
						emailQueue.RemoveAt(0);
					}
				}
			}
		}

		public void LoadConfig(string configPath)
		{
			ConfigurationReader cr = new ConfigurationReader(configPath);

			this.smtpServer = cr.AppSetting("SMTPServer");
			this.smtpPort = Int32.Parse(cr.AppSetting("SMTPPort"));
			this.smtpCredentials = new NetworkCredential(cr.AppSetting("SMTPUsername"), cr.AppSetting("SMTPPassword"));
			this.smtpSSL = cr.AppSetting("SMTPEnableSSL").ToLower() == "true";
			this.smtpTimeout = Int32.Parse(cr.AppSetting("SMTPTimeout"));

			this.from = new MailAddress(cr.AppSetting("SenderAddress"), cr.AppSetting("SenderName"));
			this.bcc = new MailAddress(cr.AppSetting("BccAddress"));
			this.replyTo = new MailAddress(cr.AppSetting("ReplyToAddress"), cr.AppSetting("ReplyToName"));

			string[] adminAddresses = cr.AppSetting("AdminAddresses").Split(',');
			List<MailAddress> adminEmails = new List<MailAddress>();
			foreach (string adminAddress in adminAddresses)
			{
				adminEmails.Add(new MailAddress(adminAddress));
			}
			this.AdminEmails = adminEmails.ToArray();

			log.Log(LogLevel.Debug, "EMAILCONFIGLOADED", "E-mail configuration file was loaded successfully.");
		}

		public void Send(string templateName, string cultureName, MailAddress[] recipientEmail, Attachment[] attachments, object model)
		{
			try
			{
				CultureInfo ci = new CultureInfo(cultureName);
				Thread.CurrentThread.CurrentCulture = ci;
				Thread.CurrentThread.CurrentUICulture = ci;
			}
			catch { }

			string subject = Resources.EmailTemplates.ResourceManager.GetObject(templateName + "_SUBJECT") as string;
			if (subject == null) subject = Resources.EmailTemplates.ResourceManager.GetObject("DEFAULT_SUBJECT") as string;
			string bodyText = Resources.EmailTemplates.ResourceManager.GetObject(templateName + "_BODY") as string;
			if (bodyText == null)
			{
				throw new Exception("The template '" + templateName + "' was not found in the global resource directory.");
			}

			SmtpClient smtpClient = new SmtpClient(this.smtpServer, this.smtpPort);
			smtpClient.Credentials = this.smtpCredentials;
			smtpClient.EnableSsl = this.smtpSSL;
			smtpClient.Timeout = this.smtpTimeout;

			MailMessage emailMessage = new MailMessage();
			emailMessage.BodyEncoding = Encoding.UTF8;
			emailMessage.From = this.from;

			if (recipientEmail != null)
			{
				foreach (MailAddress toAddress in recipientEmail)
				{
					emailMessage.To.Add(toAddress);
				}
			}

			if (emailMessage.To.Count == 0)
			{
				emailMessage.To.Add(this.bcc);
			}
			
			emailMessage.Bcc.Add(this.bcc);
			emailMessage.ReplyToList.Add(this.replyTo);
			emailMessage.Subject = RazorEngine.Razor.Parse(subject, model);

			if (attachments != null)
			{
				foreach (Attachment attachment in attachments)
				{
					emailMessage.Attachments.Add(attachment);
				}
			}

			//string htmlBody = String.Format(bodyText, parameters);
			string htmlBody = RazorEngine.Razor.Parse(bodyText, model);
			string plainBody = StripHTML(htmlBody);

			emailMessage.AlternateViews.Add(AlternateView.CreateAlternateViewFromString(plainBody, Encoding.UTF8, System.Net.Mime.MediaTypeNames.Text.Plain));
			emailMessage.AlternateViews.Add(AlternateView.CreateAlternateViewFromString(htmlBody, Encoding.UTF8, System.Net.Mime.MediaTypeNames.Text.Html));

			lock (lockObject)
			{
				emailQueue.Add(new EmailQueueItem() { SmtpClient = smtpClient, EmailMessage = emailMessage, Template = templateName });
			}
		}

		private string StripHTML(string source)
		{
			try
			{
				string result;

				// Remove HTML Development formatting
				// Replace line breaks with space
				// because browsers inserts space
				result = source.Replace("\r", " ");
				// Replace line breaks with space
				// because browsers inserts space
				result = result.Replace("\n", " ");
				// Remove step-formatting
				result = result.Replace("\t", string.Empty);
				// Remove repeating spaces because browsers ignore them
				result = System.Text.RegularExpressions.Regex.Replace(result,
																	  @"( )+", " ");

				// Remove the header (prepare first by clearing attributes)
				result = System.Text.RegularExpressions.Regex.Replace(result,
						 @"<( )*head([^>])*>", "<head>",
						 System.Text.RegularExpressions.RegexOptions.IgnoreCase);
				result = System.Text.RegularExpressions.Regex.Replace(result,
						 @"(<( )*(/)( )*head( )*>)", "</head>",
						 System.Text.RegularExpressions.RegexOptions.IgnoreCase);
				result = System.Text.RegularExpressions.Regex.Replace(result,
						 "(<head>).*(</head>)", string.Empty,
						 System.Text.RegularExpressions.RegexOptions.IgnoreCase);

				// remove all scripts (prepare first by clearing attributes)
				result = System.Text.RegularExpressions.Regex.Replace(result,
						 @"<( )*script([^>])*>", "<script>",
						 System.Text.RegularExpressions.RegexOptions.IgnoreCase);
				result = System.Text.RegularExpressions.Regex.Replace(result,
						 @"(<( )*(/)( )*script( )*>)", "</script>",
						 System.Text.RegularExpressions.RegexOptions.IgnoreCase);
				//result = System.Text.RegularExpressions.Regex.Replace(result,
				//         @"(<script>)([^(<script>\.</script>)])*(</script>)",
				//         string.Empty,
				//         System.Text.RegularExpressions.RegexOptions.IgnoreCase);
				result = System.Text.RegularExpressions.Regex.Replace(result,
						 @"(<script>).*(</script>)", string.Empty,
						 System.Text.RegularExpressions.RegexOptions.IgnoreCase);

				// remove all styles (prepare first by clearing attributes)
				result = System.Text.RegularExpressions.Regex.Replace(result,
						 @"<( )*style([^>])*>", "<style>",
						 System.Text.RegularExpressions.RegexOptions.IgnoreCase);
				result = System.Text.RegularExpressions.Regex.Replace(result,
						 @"(<( )*(/)( )*style( )*>)", "</style>",
						 System.Text.RegularExpressions.RegexOptions.IgnoreCase);
				result = System.Text.RegularExpressions.Regex.Replace(result,
						 "(<style>).*(</style>)", string.Empty,
						 System.Text.RegularExpressions.RegexOptions.IgnoreCase);

				// replace <a href=''></a>'s with [...]'s
				int ahrefIndex = -1;
				string resultLower = result.ToLower();

				while ((ahrefIndex = resultLower.IndexOf("<a href=")) != -1)
				{
					int ahrefEnd = resultLower.IndexOf(">", ahrefIndex);
					int ahrefClosing = resultLower.IndexOf("</a>", ahrefIndex);

					string link = result.Substring(ahrefIndex + 9, ahrefEnd - (ahrefIndex + 9) - 1);
					string text = result.Substring(ahrefEnd + 1, ahrefClosing - (ahrefEnd + 1));

					result = result.Substring(0, ahrefIndex) + text + " [" + link + ']' + result.Substring(ahrefClosing + 4);
					resultLower = result.ToLower();
				}




				// insert tabs in spaces of <td> tags
				result = System.Text.RegularExpressions.Regex.Replace(result,
						 @"<( )*td([^>])*>", "\t",
						 System.Text.RegularExpressions.RegexOptions.IgnoreCase);

				// insert line breaks in places of <BR> and <LI> tags
				result = System.Text.RegularExpressions.Regex.Replace(result,
						 @"<( )*br( )*>", "\n\r",
						 System.Text.RegularExpressions.RegexOptions.IgnoreCase);
				result = System.Text.RegularExpressions.Regex.Replace(result,
						 @"<( )*li( )*>", "\n\r",
						 System.Text.RegularExpressions.RegexOptions.IgnoreCase);

				// insert line paragraphs (double line breaks) in place
				// if <P>, <DIV> and <TR> tags
				result = System.Text.RegularExpressions.Regex.Replace(result,
						 @"<( )*div([^>])*>", "\n\r\n\r",
						 System.Text.RegularExpressions.RegexOptions.IgnoreCase);
				result = System.Text.RegularExpressions.Regex.Replace(result,
						 @"<( )*tr([^>])*>", "\n\r\n\r",
						 System.Text.RegularExpressions.RegexOptions.IgnoreCase);
				result = System.Text.RegularExpressions.Regex.Replace(result,
						 @"<( )*p([^>])*>", "\n\r\n\r",
						 System.Text.RegularExpressions.RegexOptions.IgnoreCase);

				// Remove remaining tags like <a>, links, images,
				// comments etc - anything that's enclosed inside < >
				result = System.Text.RegularExpressions.Regex.Replace(result,
						 @"<[^>]*>", string.Empty,
						 System.Text.RegularExpressions.RegexOptions.IgnoreCase);

				// replace special characters:
				result = System.Text.RegularExpressions.Regex.Replace(result,
						 @" ", " ",
						 System.Text.RegularExpressions.RegexOptions.IgnoreCase);

				result = System.Text.RegularExpressions.Regex.Replace(result,
						 @"&bull;", " * ",
						 System.Text.RegularExpressions.RegexOptions.IgnoreCase);
				result = System.Text.RegularExpressions.Regex.Replace(result,
						 @"&lsaquo;", "<",
						 System.Text.RegularExpressions.RegexOptions.IgnoreCase);
				result = System.Text.RegularExpressions.Regex.Replace(result,
						 @"&rsaquo;", ">",
						 System.Text.RegularExpressions.RegexOptions.IgnoreCase);
				result = System.Text.RegularExpressions.Regex.Replace(result,
						 @"&trade;", "(tm)",
						 System.Text.RegularExpressions.RegexOptions.IgnoreCase);
				result = System.Text.RegularExpressions.Regex.Replace(result,
						 @"&frasl;", "/",
						 System.Text.RegularExpressions.RegexOptions.IgnoreCase);
				result = System.Text.RegularExpressions.Regex.Replace(result,
						 @"&lt;", "<",
						 System.Text.RegularExpressions.RegexOptions.IgnoreCase);
				result = System.Text.RegularExpressions.Regex.Replace(result,
						 @"&gt;", ">",
						 System.Text.RegularExpressions.RegexOptions.IgnoreCase);
				result = System.Text.RegularExpressions.Regex.Replace(result,
						 @"&copy;", "(c)",
						 System.Text.RegularExpressions.RegexOptions.IgnoreCase);
				result = System.Text.RegularExpressions.Regex.Replace(result,
						 @"&reg;", "(r)",
						 System.Text.RegularExpressions.RegexOptions.IgnoreCase);
				// Remove all others. More can be added, see
				// http://hotwired.lycos.com/webmonkey/reference/special_characters/
				result = System.Text.RegularExpressions.Regex.Replace(result,
						 @"&(.{2,6});", string.Empty,
						 System.Text.RegularExpressions.RegexOptions.IgnoreCase);

				// for testing
				//System.Text.RegularExpressions.Regex.Replace(result,
				//       this.txtRegex.Text,string.Empty,
				//       System.Text.RegularExpressions.RegexOptions.IgnoreCase);

				// make line breaking consistent
				result = result.Replace("\n", "\n\r");

				// Remove extra line breaks and tabs:
				// replace over 2 breaks with 2 and over 4 tabs with 4.
				// Prepare first to remove any whitespaces in between
				// the escaped characters and remove redundant tabs in between line breaks
				result = System.Text.RegularExpressions.Regex.Replace(result,
						 "(\r)( )+(\r)", "\n\r\n\r",
						 System.Text.RegularExpressions.RegexOptions.IgnoreCase);
				result = System.Text.RegularExpressions.Regex.Replace(result,
						 "(\t)( )+(\t)", "\t\t",
						 System.Text.RegularExpressions.RegexOptions.IgnoreCase);
				result = System.Text.RegularExpressions.Regex.Replace(result,
						 "(\t)( )+(\r)", "\t\n\r",
						 System.Text.RegularExpressions.RegexOptions.IgnoreCase);
				result = System.Text.RegularExpressions.Regex.Replace(result,
						 "(\r)( )+(\t)", "\n\r\t",
						 System.Text.RegularExpressions.RegexOptions.IgnoreCase);
				// Remove redundant tabs
				result = System.Text.RegularExpressions.Regex.Replace(result,
						 "(\r)(\t)+(\r)", "\n\r\n\r",
						 System.Text.RegularExpressions.RegexOptions.IgnoreCase);
				// Remove multiple tabs following a line break with just one tab
				result = System.Text.RegularExpressions.Regex.Replace(result,
						 "(\r)(\t)+", "\n\r\t",
						 System.Text.RegularExpressions.RegexOptions.IgnoreCase);
				// Initial replacement target string for line breaks
				string breaks = "\r\r\r";
				// Initial replacement target string for tabs
				string tabs = "\t\t\t\t\t";
				for (int index = 0; index < result.Length; index++)
				{
					result = result.Replace(breaks, "\n\r\n\r");
					result = result.Replace(tabs, "\t\t\t\t");
					breaks = breaks + "\r";
					tabs = tabs + "\t";
				}

				// That's it.
				return result;
			}
			catch
			{
				return source;
			}
		}
	}


	class EmailQueueItem
	{
		public SmtpClient SmtpClient;
		public MailMessage EmailMessage;
		public int ErrorCount;
		public Exception LastError;
		public string Template;
	}
}

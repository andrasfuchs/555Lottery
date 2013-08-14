using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace LotteryInspector
{
	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class MainWindow : Window
	{
		public MainWindow()
		{
			InitializeComponent();
		}

		private void RootWindow_Initialized(object sender, EventArgs e)
		{
			Thread listenerThread = new Thread(new ThreadStart(ListenForLogMessages));
			listenerThread.Start();
		}

		private void ListenForLogMessages()
		{
			IPEndPoint remoteEndPoint = new IPEndPoint(IPAddress.Any, 0);
			UdpClient udpClient;
			byte[] buffer;
			string loggingEvent;

			try
			{
				LogListBox.Dispatcher.Invoke(DispatcherPriority.Normal, new Action(delegate()
				{
					LogListBox.Items.Add("Listening on UDP port 55555...");
				}));

				udpClient = new UdpClient(55555);

				while (true)
				{
					buffer = udpClient.Receive(ref remoteEndPoint);
					loggingEvent = System.Text.Encoding.UTF8.GetString(buffer);

					LogListBox.Dispatcher.Invoke(DispatcherPriority.Normal, new Action(delegate()
					{
						LogListBox.Items.Add(loggingEvent);
					}));
				}
			}
			catch (Exception ex)
			{
				LogListBox.Dispatcher.Invoke(DispatcherPriority.Normal, new Action(delegate()
				{
					LogListBox.Items.Add(ex);
				}));
			}
		}
	}
}

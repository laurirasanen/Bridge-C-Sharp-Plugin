using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace bridge_c_sharp_plugin
{
	class Bridge_Server
	{
		private TcpListener tcpListener;
		private Thread tcpListenerThread;
		private TcpClient connectedTcpClient;

		public List<string> receivedMessage = new List<string>();
		//Port number to listen to. Please make sure it is the same in Bridge as well.
		public int MessageReceivingPort;

		public Bridge_Server( int port )
		{
			MessageReceivingPort = port;
		}

		public void StartServer()
		{
			// Start TcpServer background thread 		
			tcpListenerThread = new Thread( new ThreadStart( ListenForIncommingRequests ) );
			tcpListenerThread.IsBackground = true;
			tcpListenerThread.Start();
		}

		public void EndServer()
		{
			tcpListener.Stop();
		}

		private void ListenForIncommingRequests()
		{
			try
			{
				tcpListener = new TcpListener( IPAddress.Parse( "127.0.0.1" ), MessageReceivingPort );
				tcpListener.Start();
				Console.WriteLine( "Server is listening, press Enter to exit\n" );
				Byte[] bytes = new Byte[512];
				while ( true )
				{
					using ( connectedTcpClient = tcpListener.AcceptTcpClient() )
					{
						using ( NetworkStream stream = connectedTcpClient.GetStream() )
						{
							int length;
							string clientMessage = "";
							while ( ( length = stream.Read( bytes, 0, bytes.Length ) ) != 0 )
							{
								byte[] incommingData = new byte[length];
								Array.Copy( bytes, 0, incommingData, 0, length );
								clientMessage += Encoding.ASCII.GetString( incommingData );
							}
							receivedMessage.Add( clientMessage );

							if ( receivedMessage.Count > 0 )
							{
								ms_bridge_importer.AssetImporter( receivedMessage[0] );
								receivedMessage.RemoveAt( 0 );
							}
						}
					}
				}
			}
			catch ( SocketException socketException )
			{
				Console.WriteLine( "SocketException " + socketException.ToString() );
			}
		}
	}
}

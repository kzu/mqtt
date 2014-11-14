﻿using System;
using System.Reactive.Subjects;
using System.Threading.Tasks;
using Hermes.Packets;

namespace Hermes
{
	public class PacketChannel : IChannel<IPacket>
	{
		readonly IChannel<byte[]> innerChannel;
		readonly IPacketManager manager;
        readonly Subject<IPacket> receiver;
		readonly IDisposable subscription;

		public PacketChannel (IChannel<byte[]> innerChannel, IPacketManager manager)
		{
			this.innerChannel = innerChannel;
			this.manager = manager;

			this.receiver = new Subject<IPacket> ();

			this.subscription = innerChannel.Receiver.Subscribe (async bytes => {
				try {
					var packet = await this.manager.GetAsync(bytes);

					this.receiver.OnNext (packet); 
				} catch (ConnectProtocolException connEx) {
					var errorAck = new ConnectAck (connEx.ReturnCode, existingSession: false);

					this.SendAsync (errorAck).Wait();
				} catch (ProtocolException ex) {
					this.receiver.OnError (ex);
				}
			}, onError: ex => this.receiver.OnError(ex), onCompleted: () => this.receiver.OnCompleted());
		}

		public IObservable<IPacket> Receiver { get { return this.receiver; } }

		public async Task SendAsync (IPacket packet)
		{
			var bytes = await this.manager.GetAsync (packet);

			await this.innerChannel.SendAsync (bytes);
		}

		public void Close ()
		{
			this.innerChannel.Close ();
			this.subscription.Dispose ();
			this.receiver.Dispose ();
		}
	}
}

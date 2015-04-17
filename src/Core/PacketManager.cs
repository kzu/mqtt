﻿using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Hermes.Formatters;
using Hermes.Packets;
using Hermes.Properties;

namespace Hermes
{
	public class PacketManager : IPacketManager
	{
		readonly IDictionary<PacketType, IFormatter> formatters;

		public PacketManager (params IFormatter[] formatters)
			: this((IEnumerable<IFormatter>)formatters)
		{
		}

		public PacketManager (IEnumerable<IFormatter> formatters)
		{
			this.formatters = formatters.ToDictionary(f => f.PacketType);
		}

		/// <exception cref="ProtocolConnectionException">ConnectProtocolException</exception>
		/// <exception cref="ProtocolViolationException">ProtocolViolationException</exception>
		/// <exception cref="ProtocolException">ProtocolException</exception>
		public async Task<IPacket> GetPacketAsync (byte[] bytes)
		{
			var packetType = (PacketType)bytes.Byte (0).Bits (4);
			var formatter = default (IFormatter);

			if (!formatters.TryGetValue(packetType, out formatter))
				throw new ProtocolException (Resources.PacketManager_PacketUnknown);

			var packet = await formatter.FormatAsync (bytes);

			return packet;
		}

		/// <exception cref="ProtocolConnectionException">ConnectProtocolException</exception>
		/// <exception cref="ProtocolViolationException">ProtocolViolationException</exception>
		/// <exception cref="ProtocolException">ProtocolException</exception>
		public async Task<byte[]> GetBytesAsync (IPacket packet)
		{
			var formatter = default (IFormatter);

			if (!formatters.TryGetValue(packet.Type, out formatter))
				throw new ProtocolException (Resources.PacketManager_PacketUnknown);

			var bytes = await formatter.FormatAsync (packet);

			return bytes;
		}
	}
}

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using EventStore.Common.Utils;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.FileProviders;

namespace EventStore.Common.Configuration {
	public class TelemetryConfiguration {
		public static IConfiguration FromFile(string telemetryConfig = "telemetryconfig.json") {
			var configurationDirectory = Path.IsPathRooted(telemetryConfig)
				? Path.GetDirectoryName(telemetryConfig)
				: Locations
					.GetPotentialConfigurationDirectories()
					.FirstOrDefault(directory => File.Exists(Path.Combine(directory, telemetryConfig)));

			if (configurationDirectory == null) {
				throw new FileNotFoundException(
					$"Could not find {telemetryConfig} in the following directories: {string.Join(", ", Locations.GetPotentialConfigurationDirectories())}");
			}

			var configurationRoot = new ConfigurationBuilder()
				.AddJsonFile(config => {
					config.Optional = false;
					config.FileProvider = new PhysicalFileProvider(configurationDirectory);
					config.OnLoadException = context => Serilog.Log.Error(context.Exception, "err");
					config.Path = Path.GetFileName(telemetryConfig);
				})
				.Build();

			return configurationRoot;
		}

		public enum StatusTracker {
			Index = 1,
			Node,
			Scavenge,
		}

		public enum Checkpoint {
			Chaser = 1,
			Epoch,
			Index,
			Proposal,
			Replication,
			StreamExistenceFilter,
			Truncate,
			Writer,
		}

		public enum IncomingGrpcCall {
			Current = 1,
			Total,
			Failed,
			Unimplemented,
			DeadlineExceeded,
		}

		public enum GrpcMethod {
			StreamRead = 1,
			StreamAppend,
			StreamBatchAppend,
			StreamDelete,
			StreamTombstone,
		}

		public enum Gossip {
			PullFromPeer = 1,
			PushToPeer,
			ProcessingPushFromPeer,
			ProcessingRequestFromPeer,
			ProcessingRequestFromGrpcClient,
			ProcessingRequestFromHttpClient,
		}

		public enum WriterTracker {
			FlushSize = 1,
			FlushDuration,
		}

		public enum EventTracker {
			Read = 1,
			Written,
		}

		public enum Cache {
			StreamInfo = 1,
			Chunk,
		}

		public enum KestrelTracker {
			ConnectionCount = 1,
		}

		public enum SystemTracker {
			Cpu = 1,
			LoadAverage1m,
			LoadAverage5m,
			LoadAverage15m,
			FreeMem,
			TotalMem,
			DriveTotalBytes,
			DriveUsedBytes,
		}

		public enum ProcessTracker {
			UpTime = 1,
			Cpu,
			MemWorkingSet,
			ThreadCount,
			LockContentionCount,
			ExceptionCount,
			Gen0CollectionCount,
			Gen1CollectionCount,
			Gen2CollectionCount,
			Gen0Size,
			Gen1Size,
			Gen2Size,
			LohSize,
			TimeInGc,
			HeapSize,
			HeapFragmentation,
			TotalAllocatedBytes,
			DiskReadBytes,
			DiskReadOps,
			DiskWrittenBytes,
			DiskWrittenOps,
		}

		public class LabelMappingCase {
			public string Regex { get; set; }
			public string Label { get; set; }
		}

		public string[] Meters { get; set; } = Array.Empty<string>();

		public StatusTracker[] StatusTrackers { get; set; } = Array.Empty<StatusTracker>();

		public Checkpoint[] Checkpoints { get; set; } = Array.Empty<Checkpoint>();

		public Dictionary<IncomingGrpcCall, bool> IncomingGrpcCalls { get; set; } = new();

		public Dictionary<GrpcMethod, string> GrpcMethods { get; set; } = new();

		public Gossip[] GossipTrackers { get; set; } = Array.Empty<Gossip>();

		public Dictionary<KestrelTracker, bool> Kestrel { get; set; } = new();

		public Dictionary<SystemTracker, bool> System { get; set; } = new();

		public Dictionary<ProcessTracker, bool> Process { get; set; } = new();

		public Dictionary<WriterTracker, bool> Writer { get; set; } = new();

		public Dictionary<EventTracker, bool> Events { get; set; } = new();

		public Dictionary<Cache, bool> CacheHitsMisses { get; set; } = new();

		// must be 0, 1, 5, 10 or a multiple of 15
		public int ExpectedScrapeIntervalSeconds { get; set; }

		public LabelMappingCase[] Queues { get; set; } = Array.Empty<LabelMappingCase>();

		public LabelMappingCase[] MessageTypes { get; set; } = Array.Empty<LabelMappingCase>();
	}
}

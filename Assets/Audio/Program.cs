using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace FmodAudioCatalogGenerator;

internal static class Program {
	public static int Main( string[] args ) {
		try {
			var options = GeneratorOptions.Parse( args );

			if ( !File.Exists( options.InputPath ) ) {
				Console.Error.WriteLine( $"Input file not found: {options.InputPath}" );
				return 1;
			}

			var lines = File.ReadAllLines( options.InputPath );
			var entries = GuidExportParser.Parse( lines );

			if ( entries.Count == 0 ) {
				Console.Error.WriteLine( "No FMOD entries were parsed from the input file." );
				return 2;
			}

			var source = CSharpCatalogEmitter.Emit(
				options.Namespace,
				options.ClassName,
				entries );

			Directory.CreateDirectory( Path.GetDirectoryName( options.OutputPath )! );
			File.WriteAllText( options.OutputPath, source, new UTF8Encoding( encoderShouldEmitUTF8Identifier: false ) );

			Console.WriteLine( $"Generated {entries.Count} FMOD definitions -> {options.OutputPath}" );
			return 0;
		} catch ( Exception ex ) {
			Console.Error.WriteLine( ex );
			return 99;
		}
	}
}

internal sealed record GeneratorOptions(
	string InputPath,
	string OutputPath,
	string Namespace,
	string ClassName ) {
	public static GeneratorOptions Parse( string[] args ) {
		if ( args.Length < 2 ) {
			throw new ArgumentException(
				"Usage:\n" +
				"  FmodAudioCatalogGenerator <input-guid-file> <output-cs-file> [namespace] [className]\n\n" +
				"Example:\n" +
				"  FmodAudioCatalogGenerator GUIDs.txt GeneratedAudioCatalog.cs Nomad.Audio.Generated GeneratedAudioCatalog" );
		}

		string inputPath = args[ 0 ];
		string outputPath = args[ 1 ];
		string ns = args.Length >= 3 ? args[ 2 ] : "Nomad.Audio.Generated";
		string className = args.Length >= 4 ? args[ 3 ] : "GeneratedAudioCatalog";

		return new GeneratorOptions( inputPath, outputPath, ns, className );
	}
}

internal enum FmodEntryKind {
	Unknown = 0,
	Event,
	Snapshot,
	Bus
}

internal sealed record FmodCatalogEntry(
	FmodEntryKind Kind,
	Guid Guid,
	string Path,
	string SymbolName );

internal static class GuidExportParser {
	// Accepts lines like:
	// {01234567-89ab-cdef-0123-456789abcdef} event:/Player/Jump
	// 01234567-89ab-cdef-0123-456789abcdef snapshot:/BulletTime
	// and ignores everything else.
	private static readonly Regex EntryRegex = new(
		@"^\s*\{?(?<guid>[0-9a-fA-F\-]{36})\}?\s+(?<path>(event|snapshot|bus):\/.*)\s*$",
		RegexOptions.Compiled | RegexOptions.CultureInvariant );

	public static IReadOnlyList<FmodCatalogEntry> Parse( IEnumerable<string> lines ) {
		var rawEntries = new List<(FmodEntryKind Kind, Guid Guid, string Path)>();

		foreach ( string originalLine in lines ) {
			string line = originalLine.Trim();

			if ( string.IsNullOrWhiteSpace( line ) )
				continue;

			if ( line.StartsWith( "//", StringComparison.Ordinal ) ||
				line.StartsWith( "#", StringComparison.Ordinal ) ||
				line.StartsWith( ";", StringComparison.Ordinal ) ) {
				continue;
			}

			Match match = EntryRegex.Match( line );
			if ( !match.Success )
				continue;

			Guid guid = Guid.Parse( match.Groups[ "guid" ].Value );
			string path = match.Groups[ "path" ].Value.Trim();

			FmodEntryKind kind = Classify( path );
			if ( kind == FmodEntryKind.Unknown )
				continue;

			rawEntries.Add( (kind, guid, path) );
		}

		return AssignUniqueSymbolNames( rawEntries );
	}

	private static FmodEntryKind Classify( string path ) {
		if ( path.StartsWith( "event:/", StringComparison.OrdinalIgnoreCase ) )
			return FmodEntryKind.Event;

		if ( path.StartsWith( "snapshot:/", StringComparison.OrdinalIgnoreCase ) )
			return FmodEntryKind.Snapshot;

		if ( path.StartsWith( "bus:/", StringComparison.OrdinalIgnoreCase ) )
			return FmodEntryKind.Bus;

		return FmodEntryKind.Unknown;
	}

	private static IReadOnlyList<FmodCatalogEntry> AssignUniqueSymbolNames(
		IReadOnlyList<(FmodEntryKind Kind, Guid Guid, string Path)> rawEntries ) {
		var usedNamesByKind = new Dictionary<FmodEntryKind, HashSet<string>> {
			[ FmodEntryKind.Event ] = new( StringComparer.Ordinal ),
			[ FmodEntryKind.Snapshot ] = new( StringComparer.Ordinal ),
			[ FmodEntryKind.Bus ] = new( StringComparer.Ordinal )
		};

		var results = new List<FmodCatalogEntry>( rawEntries.Count );

		foreach ( var raw in rawEntries
					 .OrderBy( x => x.Kind )
					 .ThenBy( x => x.Path, StringComparer.OrdinalIgnoreCase ) ) {
			string baseName = SymbolNameSanitizer.ToPascalIdentifier( raw.Kind, raw.Path );
			string finalName = baseName;
			int suffix = 2;

			while ( !usedNamesByKind[ raw.Kind ].Add( finalName ) ) {
				finalName = $"{baseName}{suffix.ToString( CultureInfo.InvariantCulture )}";
				suffix++;
			}

			results.Add( new FmodCatalogEntry( raw.Kind, raw.Guid, raw.Path, finalName ) );
		}

		return results;
	}
}

internal static class SymbolNameSanitizer {
	private static readonly Regex NonAlphaNumeric = new( @"[^A-Za-z0-9]+", RegexOptions.Compiled );

	public static string ToPascalIdentifier( FmodEntryKind kind, string path ) {
		string remainder = kind switch {
			FmodEntryKind.Event => path[ "event:/".Length.. ],
			FmodEntryKind.Snapshot => path[ "snapshot:/".Length.. ],
			FmodEntryKind.Bus => path[ "bus:/".Length.. ],
			_ => path
		};

		if ( kind == FmodEntryKind.Bus && string.IsNullOrWhiteSpace( remainder ) )
			return "Master";

		string[] pathSegments = remainder
			.Split( new[] { '/' }, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries );

		var sb = new StringBuilder();

		foreach ( string segment in pathSegments ) {
			string cleaned = NonAlphaNumeric.Replace( segment, " " );
			string[] words = cleaned
				.Split( new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries );

			foreach ( string word in words ) {
				string pascalWord = ToPascalWord( word );
				if ( !string.IsNullOrEmpty( pascalWord ) )
					sb.Append( pascalWord );
			}
		}

		if ( sb.Length == 0 )
			sb.Append( "Item" );

		if ( char.IsDigit( sb[ 0 ] ) )
			sb.Insert( 0, '_' );

		return sb.ToString();
	}

	private static string ToPascalWord( string value ) {
		if ( string.IsNullOrWhiteSpace( value ) )
			return string.Empty;

		if ( value.Length == 1 )
			return char.ToUpperInvariant( value[ 0 ] ).ToString();

		return char.ToUpperInvariant( value[ 0 ] ) + value[ 1.. ];
	}
}

internal static class CSharpCatalogEmitter {
	public static string Emit(
		string @namespace,
		string className,
		IReadOnlyList<FmodCatalogEntry> entries ) {
		var events = entries.Where( x => x.Kind == FmodEntryKind.Event ).ToList();
		var snapshots = entries.Where( x => x.Kind == FmodEntryKind.Snapshot ).ToList();
		var buses = entries.Where( x => x.Kind == FmodEntryKind.Bus ).ToList();

		var sb = new StringBuilder();

		AppendHeader( sb );
		sb.AppendLine( "using System;" );
		sb.AppendLine( "using System.Collections.Generic;" );
		sb.AppendLine();
		sb.AppendLine( $"namespace {@namespace};" );
		sb.AppendLine();

		EmitEventIdEnum( sb, "AudioEventId", events );
		EmitEventIdEnum( sb, "AudioSnapshotId", snapshots );
		EmitEventIdEnum( sb, "AudioBusId", buses );

		sb.AppendLine( "public readonly struct AudioEventDefinition {" );
		sb.AppendLine( "\tpublic AudioEventId Id { get; }" );
		sb.AppendLine( "\tpublic Guid Guid { get; }" );
		sb.AppendLine( "\tpublic string Path { get; }" );
		sb.AppendLine( "\tpublic AudioEventDefinition( AudioEventId id, Guid guid, string path ) {" );
		sb.AppendLine( "\t\tId = id;" );
		sb.AppendLine( "\t\tGuid = guid;" );
		sb.AppendLine( "\t\tPath = path;" );
		sb.AppendLine( "\t}" );
		sb.AppendLine( "};" );
		sb.AppendLine();

		sb.AppendLine( "public readonly struct AudioSnapshotDefinition {" );
		sb.AppendLine( "\tpublic AudioSnapshotId Id { get; }" );
		sb.AppendLine( "\tpublic Guid Guid { get; }" );
		sb.AppendLine( "\tpublic string Path { get; }" );
		sb.AppendLine( "\tpublic AudioSnapshotDefinition( AudioSnapshotId id, Guid guid, string path ) {" );
		sb.AppendLine( "\t\tId = id;" );
		sb.AppendLine( "\t\tGuid = guid;" );
		sb.AppendLine( "\t\tPath = path;" );
		sb.AppendLine( "\t}" );
		sb.AppendLine( "};" );
		sb.AppendLine();

		sb.AppendLine( "public readonly struct AudioBusDefinition {" );
		sb.AppendLine( "\tpublic AudioBusId Id { get; }" );
		sb.AppendLine( "\tpublic Guid Guid { get; }" );
		sb.AppendLine( "\tpublic string Path { get; }" );
		sb.AppendLine( "\tpublic AudioBusDefinition( AudioBusId id, Guid guid, string path ) {" );
		sb.AppendLine( "\t\tId = id;" );
		sb.AppendLine( "\t\tGuid = guid;" );
		sb.AppendLine( "\t\tPath = path;" );
		sb.AppendLine( "\t}" );
		sb.AppendLine( "};" );
		sb.AppendLine();

		sb.AppendLine( $"public static partial class {className}" );
		sb.AppendLine( "{" );

		EmitDictionary(
			sb,
			"AudioEventId",
			"AudioEventDefinition",
			"_events",
			events,
			e => $"new AudioEventDefinition(AudioEventId.{e.SymbolName}, new Guid(\"{e.Guid:D}\"), \"{EscapeString( e.Path )}\")" );

		EmitDictionary(
			sb,
			"AudioSnapshotId",
			"AudioSnapshotDefinition",
			"_snapshots",
			snapshots,
			e => $"new AudioSnapshotDefinition(AudioSnapshotId.{e.SymbolName}, new Guid(\"{e.Guid:D}\"), \"{EscapeString( e.Path )}\")" );

		EmitDictionary(
			sb,
			"AudioBusId",
			"AudioBusDefinition",
			"_buses",
			buses,
			e => $"new AudioBusDefinition(AudioBusId.{e.SymbolName}, new Guid(\"{e.Guid:D}\"), \"{EscapeString( e.Path )}\")" );

		EmitGetter(
			sb,
			"AudioEventId",
			"AudioEventDefinition",
			"_events",
			"GetEvent",
			"TryGetEvent" );

		EmitGetter(
			sb,
			"AudioSnapshotId",
			"AudioSnapshotDefinition",
			"_snapshots",
			"GetSnapshot",
			"TryGetSnapshot" );

		EmitGetter(
			sb,
			"AudioBusId",
			"AudioBusDefinition",
			"_buses",
			"GetBus",
			"TryGetBus" );

		sb.AppendLine( "}" );

		return sb.ToString();
	}

	private static void AppendHeader( StringBuilder sb ) {
		sb.AppendLine( "// <auto-generated />" );
		sb.AppendLine( "// Generated by FmodAudioCatalogGenerator." );
		sb.AppendLine( "// Do not edit by hand." );
		sb.AppendLine();
	}

	private static void EmitEventIdEnum(
		StringBuilder sb,
		string enumName,
		IReadOnlyList<FmodCatalogEntry> entries ) {
		sb.AppendLine( $"public enum {enumName}" );
		sb.AppendLine( "{" );
		sb.AppendLine( "    None = 0," );
		foreach ( var entry in entries ) {
			sb.AppendLine( $"    {entry.SymbolName}," );
		}
		sb.AppendLine( "}" );
		sb.AppendLine();
	}

	private static void EmitDictionary(
		StringBuilder sb,
		string keyType,
		string valueType,
		string fieldName,
		IReadOnlyList<FmodCatalogEntry> entries,
		Func<FmodCatalogEntry, string> valueFactory ) {
		sb.AppendLine( $"    private static readonly IReadOnlyDictionary<{keyType}, {valueType}> {fieldName} =" );
		sb.AppendLine( $"        new Dictionary<{keyType}, {valueType}>" );
		sb.AppendLine( "        {" );

		foreach ( var entry in entries ) {
			sb.AppendLine( $"            [{keyType}.{entry.SymbolName}] = {valueFactory( entry )}," );
		}

		sb.AppendLine( "        };" );
		sb.AppendLine();
	}

	private static void EmitGetter(
		StringBuilder sb,
		string keyType,
		string valueType,
		string fieldName,
		string getMethodName,
		string tryGetMethodName ) {
		sb.AppendLine( $"    public static {valueType} {getMethodName}({keyType} id)" );
		sb.AppendLine( "    {" );
		sb.AppendLine( $"        if ({fieldName}.TryGetValue(id, out var value))" );
		sb.AppendLine( "            return value;" );
		sb.AppendLine();
		sb.AppendLine( $"        throw new KeyNotFoundException($\"No generated FMOD definition exists for {keyType}.\");" );
		sb.AppendLine( "    }" );
		sb.AppendLine();

		sb.AppendLine( $"    public static bool {tryGetMethodName}({keyType} id, out {valueType} value)" );
		sb.AppendLine( "    {" );
		sb.AppendLine( $"        return {fieldName}.TryGetValue(id, out value);" );
		sb.AppendLine( "    }" );
		sb.AppendLine();
	}

	private static string EscapeString( string value ) {
		return value.Replace( "\\", "\\\\" ).Replace( "\"", "\\\"" );
	}
}
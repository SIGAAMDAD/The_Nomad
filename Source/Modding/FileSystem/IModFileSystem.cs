/*
===========================================================================
The Nomad MPLv2 Source Code
Copyright (C) 2025-2026 Noah Van Til

This Source Code Form is subject to the terms of the Mozilla Public
License, v2. If a copy of the MPL was not distributed with this
file, You can obtain one at https://mozilla.org/MPL/2.0/.

This software is provided "as is", without warranty of any kind,
express or implied, including but not limited to the warranties
of merchantability, fitness for a particular purpose and noninfringement.
===========================================================================
*/

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Nomad.Modding.FileSystem
{
    public interface IModFileSystem
    {
        bool ContentFileExists(string relativePath);
		bool DataFileExists(string relativePath);

		bool DataDirectoryExists(string relativePath);
		void CreateDataDirectory(string relativePath);

		IReadOnlyList<string> EnumerateContentFiles(
			string relativeDirectory,
			string searchPattern = "*",
			bool recursive = false
		);

		IReadOnlyList<string> EnumerateDataFiles(
			string relativeDirectory,
			string searchPattern = "*",
			bool recursive = false
		);

		byte[] ReadContentBytes(string relativePath);
		byte[] ReadDataBytes(string relativePath);

		string ReadContentText(string relativePath);
		string ReadDataText(string relativePath);

		T ReadContentJson<T>(string relativePath);
		T ReadDataJson<T>(string relativePath);

		bool TryReadContentText(string relativePath, out string text);
		bool TryReadDataText(string relativePath, out string text);

		void WriteDataBytes(string relativePath, ReadOnlySpan<byte> bytes);
		void WriteDataText(string relativePath, string text);
		void WriteDataJson<T>(string relativePath, T value);

		ValueTask WriteDataBytesAsync(
			string relativePath,
			ReadOnlyMemory<byte> bytes,
			CancellationToken ct = default
		);

		void DeleteDataFile(string relativePath);
    }
}

/*
===========================================================================
The Nomad Framework
Copyright (C) 2025-2027 Noah Van Til

This Source Code Form is subject to the terms of the Mozilla Public
License, v2. If a copy of the MPL was not distributed with this
file, You can obtain one at https://mozilla.org/MPL/2.0/.

This software is provided "as is", without warranty of any kind,
express or implied, including but not limited to the warranties
of merchantability, fitness for a particular purpose and noninfringement.
===========================================================================
*/

#ifndef __NMD_RENOWN_SNAPSHOT_CODEC_H__
#define __NMD_RENOWN_SNAPSHOT_CODEC_H__

#include "NMDRenownBlackboard.h"

/*
===========================================================================
NMDRenownSnapshotCodec

Portable little-endian snapshot I/O. Only the externally validated codec
interprets snapshot contents. The blackboard performs typed storage only.
All validation occurs before replacing live records; borrowed scratch lives
inside the database's renown arena segment.
===========================================================================
*/
class NMDRenownSnapshotCodec
{
public:
	static size_t getSnapshotBytes( const NMDRenownBlackboard &blackboard );
	static bool writeSnapshot( const NMDRenownBlackboard &blackboard,
		void *pOutput, size_t nCapacity, size_t &nBytesWritten );
	static nmdRenownResult_t readSnapshot( NMDRenownBlackboard &blackboard,
		const void *pInput, size_t nBytes );
};

#endif // __NMD_RENOWN_SNAPSHOT_CODEC_H__

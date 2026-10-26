#ifndef __NMD_MEMORY_SYSTEM_H__
#define __NMD_MEMORY_SYSTEM_H__

#include <Framework/Core/Platform/NMDPCH.h>

#ifdef NOMAD_PRAGMA_ONCE_SUPPORTED
	#pragma once
#endif

class NMDVirtualMemorySegmentAllocator
{
public:
	NMDVirtualMemorySegmentAllocator( size_t nBytes );

private:
	~NMDVirtualMemorySegmentAllocator();
};

class NMDMemorySystem
{
public:
	NMDMemorySystem( size_t nMaxReservationSize );
	~NMDMemorySystem();

	NMDMemorySegment commit( size_t nBytes );
	void decommit( NMDMemorySegment& memorySegment );

	size_t getTotalReservedBytes( void ) const;
	size_t getTotalResidentBytes( void ) const;
	size_t getTotalCommittedBytes( void ) const;

private:
	size_t m_nMemoryBudget;
};

#endif // __NMD_MEMORY_SYSTEM_H__

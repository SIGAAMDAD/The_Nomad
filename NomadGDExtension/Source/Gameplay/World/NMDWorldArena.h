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

#ifndef __NMD_WORLD_ARENA_H__
#define __NMD_WORLD_ARENA_H__

#include <Framework/Core/Platform/NMDPCH.h>

#ifdef NOMAD_PRAGMA_ONCE_SUPPORTED
	#pragma once
#endif

#include <cassert>
#include <cstdint>
#include <new>
#include <type_traits>


/*
===========================================================================
A non-owning typed view into the database's single preallocated block.
Capacity never grows implicitly. These views are rebound only when the
owner explicitly repacks the block or precommits more pages.
===========================================================================
*/
template<typename T>
class NMDWorldArenaArray
{
	static_assert( std::is_standard_layout<T>::value && std::is_trivially_copyable<T>::value &&
		std::is_trivially_destructible<T>::value, "World arena records must be POD-like" );

public:
	NMDWorldArenaArray( void ) : m_pData( NULL ), m_nCount( 0u ), m_nCapacity( 0u ), m_nCommittedCapacity( 0u ) {}

	void bind( T *pData, uint32_t nCapacity, uint32_t nCount, uint32_t nCommittedCapacity )
	{
		assert( nCount <= nCommittedCapacity && nCommittedCapacity <= nCapacity &&
			( nCapacity == 0u || pData != NULL ) );
		m_pData = pData;
		m_nCapacity = nCapacity;
		m_nCommittedCapacity = nCommittedCapacity;
		m_nCount = nCount;
	}

	uint32_t size( void ) const { return m_nCount; }
	uint32_t capacity( void ) const { return m_nCapacity; }
	uint32_t committedCapacity( void ) const { return m_nCommittedCapacity; }
	void setCommittedCapacity( uint32_t nCapacity )
	{
		assert( nCapacity >= m_nCount && nCapacity <= m_nCapacity );
		m_nCommittedCapacity = nCapacity;
	}
	bool empty( void ) const { return m_nCount == 0u; }
	T *data( void ) { return m_pData; }
	const T *data( void ) const { return m_pData; }
	T *begin( void ) { return m_pData; }
	T *end( void ) { return m_pData == NULL ? NULL : m_pData + m_nCount; }
	const T *begin( void ) const { return m_pData; }
	const T *end( void ) const { return m_pData == NULL ? NULL : m_pData + m_nCount; }
	T& operator[]( uint32_t nIndex ) { assert( nIndex < m_nCount ); return m_pData[ nIndex ]; }
	const T& operator[]( uint32_t nIndex ) const { assert( nIndex < m_nCount ); return m_pData[ nIndex ]; }

	bool push_back( const T& value )
	{
		if ( m_nCount >= m_nCommittedCapacity ) {
			return false;
		}
		m_pData[ m_nCount++ ] = value;
		return true;
	}

	bool resize( uint32_t nCount, const T& value = T{} )
	{
		if ( nCount > m_nCommittedCapacity ) {
			return false;
		}
		for ( uint32_t i = m_nCount; i < nCount; ++i ) {
			m_pData[ i ] = value;
		}
		m_nCount = nCount;
		return true;
	}

	void clear( void ) { m_nCount = 0u; }

private:
	T *m_pData;
	uint32_t m_nCount;
	uint32_t m_nCapacity;
	uint32_t m_nCommittedCapacity;
};

template<typename T>
using nmdWorldArenaArray_t = NMDWorldArenaArray<T>;

static_assert( sizeof( NMDWorldArenaArray<uint32_t> ) <= 64u,
	"Arena view must fit inside one cache line" );

#endif // __NMD_WORLD_ARENA_H__

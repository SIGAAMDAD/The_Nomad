#ifndef __NMD_DATABASE_H__
#define __NMD_DATABASE_H__

#include <Framework/Core/Platform/NMDPCH.h>

#ifdef NOMAD_PRAGMA_ONCE_SUPPORTED
	#pragma once
#endif

#include <EASTL/intrusive_hash_map.h>
#include <Framework/Core/Containers/NMDStrings.h>
#include <Framework/Core/Containers/NMDHashMap.h>
#include <Framework/Core/Memory/NMDIntrusivePtr.h>

template <typename TValue>
class NMDDatabase
{
public:
	NMDDatabase( void );
	virtual ~NMDDatabase();

	virtual void clear( void );
	virtual NMDIntrusivePtr<TValue> get( const NMDStringAtom& rId );

	const eastl::intrusive_hash_map<NMDStringAtom, NMDIntrusivePtr<TValue>, 256>& getEntries( void ) const;
protected:
	virtual void load() = 0;

	eastl::intrusive_hash_map<NMDStringAtom, NMDIntrusivePtr<TValue>, 256> m_CacheEntries;
};

template<typename TValue>
NOMAD_FORCE_INLINE const eastl::intrusive_hash_map<NMDStringAtom, NMDIntrusivePtr<TValue>, 256>& NMDDatabase<TValue>::getEntries( void ) const
{
	return m_CacheEntries;
}

#endif // __NMD_DATABASE_H__

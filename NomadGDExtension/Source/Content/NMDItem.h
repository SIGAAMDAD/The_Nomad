#ifndef __NMD_ITEM_H__
#define __NMD_ITEM_H__

#ifdef NOMAD_PRAGMA_ONCE_SUPPORTED
	#pragma once
#endif

#include <Framework/Core/Memory/NMDRefCounted.h>
#include <Framework/Core/Containers/NMDString.h>

class NMDItem : public NMDRefCounted
{
public:
	NMDItem( void );
	virtual ~NMDItem();

	uintptr_t operator()( const NMDStringAtom& key )
	{
		return key.hash();
	}
private:
};

#endif // __NMD_ITEM_H__

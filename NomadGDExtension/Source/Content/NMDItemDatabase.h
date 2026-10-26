#ifndef __NMD_ITEM_DATABASE_H__
#define __NMD_ITEM_DATABASE_H__

#include <Framework/Core/Platform/NMDPCH.h>

#ifdef NOMAD_PRAGMA_ONCE_SUPPORTED
	#pragma once
#endif

#include "NMDDatabase.h"
#include "NMDItem.h"

class NMDItemDatabase : public NMDDatabase<NMDItem>
{
public:
	NMDItemDatabase( void );
	~NMDItemDatabase();
private:
};

#endif // __NMD_ITEM_DATABASE_H__

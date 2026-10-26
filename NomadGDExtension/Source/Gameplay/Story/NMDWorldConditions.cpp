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

#include "NMDWorldConditions.h"

// Comparison logic is shared between checked external evaluation and the
// validated compiled-quest path (which skips repeated cold type-array reads).
static bool nmdWorldEvaluateValue( const nmdWorldCondition_t& condition,
	nmdWorldFactType_t type, nmdWorldValue_t value )
{
	if ( condition.eComparison > NMD_WORLD_COMP_LTE ) {
		return false;
	}
	if ( type == NMD_WORLD_FACT_BOOLEAN && condition.eComparison > NMD_WORLD_COMP_NEQ ) {
		return false;
	}

	// Signed integer comparisons must not accidentally become unsigned.
	if ( type == NMD_WORLD_FACT_INTEGER ) {
		const int32_t a = value.asInteger(), b = condition.requiredValue.asInteger();
		switch ( condition.eComparison ) {
			case NMD_WORLD_COMP_EQ: return a == b;
			case NMD_WORLD_COMP_NEQ: return a != b;
			case NMD_WORLD_COMP_GT: return a > b;
			case NMD_WORLD_COMP_GTE: return a >= b;
			case NMD_WORLD_COMP_LT: return a < b;
			case NMD_WORLD_COMP_LTE: return a <= b;
			default: return false;
		}
	}
	if ( type == NMD_WORLD_FACT_FLOAT ) {
		const float a = value.asFloat(), b = condition.requiredValue.asFloat();
		switch ( condition.eComparison ) {
			case NMD_WORLD_COMP_EQ: return a == b;
			case NMD_WORLD_COMP_NEQ: return a != b;
			case NMD_WORLD_COMP_GT: return a > b;
			case NMD_WORLD_COMP_GTE: return a >= b;
			case NMD_WORLD_COMP_LT: return a < b;
			case NMD_WORLD_COMP_LTE: return a <= b;
			default: return false;
		}
	}
	if ( type == NMD_WORLD_FACT_BOOLEAN ) {
		const bool same = value.asBoolean() == condition.requiredValue.asBoolean();
		return condition.eComparison == NMD_WORLD_COMP_EQ ? same : !same;
	}
	return false;
}

bool nmdWorldEvaluateCondition( const NMDWorldDatabase& db, const nmdWorldCondition_t& condition )
{
	nmdWorldFactType_t type;
	nmdWorldValue_t value;
	if ( !db.getFact( condition.nFactId, type, value ) || type != condition.eValueType ) {
		return false;
	}
	return nmdWorldEvaluateValue( condition, type, value );
}

bool nmdWorldEvaluateConditions( const NMDWorldDatabase& db,
	const nmdWorldCondition_t *pConditions, uint32_t nCount )
{
	if ( nCount && pConditions == NULL ) {
		return false;
	}
	for ( uint32_t i = 0u; i < nCount; ++i ) {
		if ( !nmdWorldEvaluateCondition( db, pConditions[ i ] ) ) {
			return false;
		}
	}
	return true;
}

bool nmdWorldEvaluateObjective( const NMDWorldDatabase& db,
	nmdWorldHandle_t hQuest, uint32_t nObjective, bool& bSatisfied )
{
	const nmdWorldCondition_t *pConditions = NULL;
	uint8_t nCount = 0u;
	if ( !db.getObjectiveConditions( hQuest, nObjective, pConditions, nCount ) ) {
		return false;
	}
	// Objective conditions were checked when the quest was authored. Their IDs
	// and expected types remain immutable; only the live values are read here.
	const uint32_t generation = db.getFactDefinitionGeneration();
	bSatisfied = true;
	for ( uint32_t i = 0u; i < nCount; ++i ) {
		const nmdWorldCondition_t& condition = pConditions[i];
		const nmdWorldCompiledFactHandle_t handle = {
			condition.nFactId, condition.eValueType, 0u, generation
		};
		nmdWorldValue_t value = {};
		if ( !db.getCompiledFactValue( handle, value ) ||
			!nmdWorldEvaluateValue( condition,
				static_cast<nmdWorldFactType_t>( condition.eValueType ), value ) ) {
			bSatisfied = false;
			break;
		}
	}
	return true;
}

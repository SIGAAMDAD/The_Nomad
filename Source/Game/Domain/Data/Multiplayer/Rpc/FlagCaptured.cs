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

[AttributeUsage( AttributeTargets.Method, AllowMultiple = false )]
public sealed class RpcMethodAttribute : Attribute
{
	public string Name { get; }

	public RpcMethodAttribute(string name)
	{
		Name = name;
	}
}

[AttributeUsage( AttributeTargets.Method, AllowMultiple = true )]
public sealed class RpcMethodPayloadAttribute : Attribute
{
	public string Name { get; }
	public Type Type { get; }
	public int Order { get; init; }

	public RpcMethodPayloadAttribute(string name, Type type)
	{
		Name = name;
		Type = type;
	}
}

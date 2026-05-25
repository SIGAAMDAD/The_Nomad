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
using System.IO;
using System.Linq;
using Mono.Cecil;
using Mono.Cecil.Cil;
using Nomad.Game.Domain.Data.Mods;

namespace Nomad.Game.Infrastructure.Mods
{
	internal sealed class ModAssemblyValidator
	{
		private readonly ModSecurityPolicy _policy;

		public ModAssemblyValidator( ModSecurityPolicy policy )
		{
			_policy = policy ?? throw new ArgumentNullException( nameof( policy ) );
		}

		public ModuleValidationReport Validate( string assemblyPath )
		{
			if ( string.IsNullOrWhiteSpace( assemblyPath ) ) {
				throw new ArgumentException( "Assembly path cannot be null or whitespace.", nameof( assemblyPath ) );
			}

			var report = new ModuleValidationReport();

			if ( !File.Exists( assemblyPath ) ) {
				report.Error(
					"MOD001",
					$"Mod assembly '{assemblyPath}' does not exist.",
					assemblyPath
				);

				return report;
			}

			ReaderParameters readerParameters = new ReaderParameters {
				ReadSymbols = false,
				ReadingMode = ReadingMode.Deferred,
				AssemblyResolver = new DefaultAssemblyResolver()
			};

			try {
				using AssemblyDefinition assembly = AssemblyDefinition.ReadAssembly( assemblyPath, readerParameters );

				ValidateAssemblyReferences( assembly, report );
				ValidateModules( assembly, report );
			} catch ( BadImageFormatException ex ) {
				report.Error(
					"MOD002",
					$"File is not a valid .NET assembly: {ex.Message}",
					assemblyPath
				);
			} catch ( Exception ex ) {
				report.Error(
					"MOD003",
					$"Failed to validate mod assembly: {ex.Message}",
					assemblyPath
				);
			}

			return report;
		}

		private void ValidateAssemblyReferences(
			AssemblyDefinition assembly,
			ModuleValidationReport report
		)
		{
			foreach ( AssemblyNameReference reference in assembly.MainModule.AssemblyReferences ) {
				string name = reference.Name;

				if ( IsForbiddenAssembly( name ) ) {
					report.Error(
						"MOD100",
						$"Forbidden assembly reference '{name}'. Mods must use Nomad.Mods.Abstractions / Nomad.Game.ModSdk instead of raw framework assemblies.",
						assembly.MainModule.FileName
					);
				}
			}
		}

		private void ValidateModules(
			AssemblyDefinition assembly,
			ModuleValidationReport report
		)
		{
			foreach ( ModuleDefinition module in assembly.Modules ) {
				if ( _policy.ForbidPInvoke && module.HasModuleReferences ) {
					foreach ( ModuleReference moduleReference in module.ModuleReferences ) {
						report.Error(
							"MOD200",
							$"Native module reference '{moduleReference.Name}' is not allowed in alpha mods.",
							module.FileName
						);
					}
				}

				foreach ( TypeDefinition type in module.Types ) {
					ValidateTypeRecursive( type, report );
				}
			}
		}

		private void ValidateTypeRecursive(
			TypeDefinition type,
			ModuleValidationReport report
		)
		{
			ValidateTypeDefinition( type, report );

			foreach ( FieldDefinition field in type.Fields ) {
				ValidateField( type, field, report );
			}

			foreach ( MethodDefinition method in type.Methods ) {
				ValidateMethod( type, method, report );
			}

			foreach ( PropertyDefinition property in type.Properties ) {
				ValidateTypeReference(
					property.PropertyType,
					report,
					$"{type.FullName}.{property.Name}"
				);
			}

			foreach ( EventDefinition eventDefinition in type.Events ) {
				ValidateTypeReference(
					eventDefinition.EventType,
					report,
					$"{type.FullName}.{eventDefinition.Name}"
				);
			}

			foreach ( TypeDefinition nestedType in type.NestedTypes ) {
				ValidateTypeRecursive( nestedType, report );
			}
		}

		private void ValidateTypeDefinition(
			TypeDefinition type,
			ModuleValidationReport report
		)
		{
			if ( _policy.ForbidStaticConstructors ) {
				MethodDefinition? cctor = type.Methods.FirstOrDefault( method => method.IsConstructor && method.IsStatic );

				if ( cctor != null ) {
					report.Error(
						"MOD300",
						$"Static constructor on '{type.FullName}' is not allowed in alpha mods.",
						FormatMethodLocation( cctor )
					);
				}
			}

			if ( type.BaseType != null ) {
				ValidateTypeReference( type.BaseType, report, type.FullName );
			}

			foreach ( InterfaceImplementation implementation in type.Interfaces ) {
				ValidateTypeReference(
					implementation.InterfaceType,
					report,
					type.FullName
				);
			}
		}

		private void ValidateField(
			TypeDefinition owner,
			FieldDefinition field,
			ModuleValidationReport report
		)
		{
			ValidateTypeReference(
				field.FieldType,
				report,
				$"{owner.FullName}.{field.Name}"
			);

			if ( _policy.ForbidUnsafePointers && field.FieldType.IsPointer ) {
				report.Error(
					"MOD301",
					$"Pointer field '{owner.FullName}.{field.Name}' is not allowed in alpha mods.",
					$"{owner.FullName}.{field.Name}"
				);
			}
		}

		private void ValidateMethod(
			TypeDefinition owner,
			MethodDefinition method,
			ModuleValidationReport report
		)
		{
			if ( _policy.ForbidPInvoke && method.IsPInvokeImpl ) {
				report.Error(
					"MOD201",
					$"P/Invoke method '{method.FullName}' is not allowed in alpha mods.",
					FormatMethodLocation( method )
				);
			}

			ValidateTypeReference( method.ReturnType, report, FormatMethodLocation( method ) );

			foreach ( ParameterDefinition parameter in method.Parameters ) {
				ValidateTypeReference(
					parameter.ParameterType,
					report,
					FormatMethodLocation( method )
				);
			}

			if ( _policy.ForbidUnsafePointers ) {
				ValidateUnsafeMethodSignature( method, report );
			}

			if ( !method.HasBody ) {
				return;
			}

			foreach ( VariableDefinition variable in method.Body.Variables ) {
				ValidateTypeReference(
					variable.VariableType,
					report,
					FormatMethodLocation( method )
				);
			}

			foreach ( Instruction instruction in method.Body.Instructions ) {
				ValidateInstruction( method, instruction, report );
			}
		}

		private void ValidateUnsafeMethodSignature(
			MethodDefinition method,
			ModuleValidationReport report
		)
		{
			if ( method.ReturnType.IsPointer ) {
				report.Error(
					"MOD302",
					$"Pointer return type in '{method.FullName}' is not allowed in alpha mods.",
					FormatMethodLocation( method )
				);
			}

			foreach ( ParameterDefinition parameter in method.Parameters ) {
				if ( parameter.ParameterType.IsPointer ) {
					report.Error(
						"MOD303",
						$"Pointer parameter '{parameter.Name}' in '{method.FullName}' is not allowed in alpha mods.",
						FormatMethodLocation( method )
					);
				}
			}
		}

		private void ValidateInstruction(
			MethodDefinition method,
			Instruction instruction,
			ModuleValidationReport report
		)
		{
			switch ( instruction.Operand ) {
				case MethodReference methodReference:
					ValidateMethodReference( method, methodReference, report );
					break;

				case TypeReference typeReference:
					ValidateTypeReference(
						typeReference,
						report,
						FormatMethodLocation( method )
					);
					break;

				case FieldReference fieldReference:
					ValidateFieldReference( method, fieldReference, report );
					break;
			}

			if ( _policy.ForbidUnsafePointers && IsUnsafeOpcode( instruction.OpCode ) ) {
				report.Error(
					"MOD304",
					$"Unsafe IL instruction '{instruction.OpCode}' is not allowed in alpha mods.",
					FormatMethodLocation( method )
				);
			}
		}

		private void ValidateMethodReference(
			MethodDefinition caller,
			MethodReference callee,
			ModuleValidationReport report
		)
		{
			string declaringType = callee.DeclaringType.FullName;
			string methodFullName = callee.FullName;

			if ( IsForbiddenType( declaringType ) ) {
				report.Error(
					"MOD400",
					$"Forbidden call to '{methodFullName}'. Mods must use the provided IModContext capability APIs.",
					FormatMethodLocation( caller )
				);
			}

			if ( IsForbiddenMethod( methodFullName ) ) {
				report.Error(
					"MOD401",
					$"Forbidden method call '{methodFullName}'.",
					FormatMethodLocation( caller )
				);
			}

			ValidateTypeReference( callee.DeclaringType, report, FormatMethodLocation( caller ) );
			ValidateTypeReference( callee.ReturnType, report, FormatMethodLocation( caller ) );

			foreach ( ParameterDefinition parameter in callee.Parameters ) {
				ValidateTypeReference(
					parameter.ParameterType,
					report,
					FormatMethodLocation( caller )
				);
			}
		}

		private void ValidateFieldReference(
			MethodDefinition caller,
			FieldReference field,
			ModuleValidationReport report
		)
		{
			string declaringType = field.DeclaringType.FullName;

			if ( IsForbiddenType( declaringType ) ) {
				report.Error(
					"MOD402",
					$"Forbidden field access '{field.FullName}'. Mods must use the provided IModContext capability APIs.",
					FormatMethodLocation( caller )
				);
			}

			ValidateTypeReference( field.DeclaringType, report, FormatMethodLocation( caller ) );
			ValidateTypeReference( field.FieldType, report, FormatMethodLocation( caller ) );
		}

		private void ValidateTypeReference(
			TypeReference type,
			ModuleValidationReport report,
			string location
		)
		{
			if ( type == null ) {
				return;
			}

			if ( IsForbiddenType( type.FullName ) ) {
				report.Error(
					"MOD500",
					$"Forbidden type reference '{type.FullName}'. Mods must use Nomad mod APIs instead.",
					location
				);
			}

			if ( _policy.ForbidUnsafePointers && type.IsPointer ) {
				report.Error(
					"MOD305",
					$"Pointer type reference '{type.FullName}' is not allowed in alpha mods.",
					location
				);
			}

			if ( type is GenericInstanceType genericInstance ) {
				foreach ( TypeReference genericArgument in genericInstance.GenericArguments ) {
					ValidateTypeReference( genericArgument, report, location );
				}
			}

			if ( type is ArrayType arrayType ) {
				ValidateTypeReference( arrayType.ElementType, report, location );
			}

			if ( type is ByReferenceType byReferenceType ) {
				ValidateTypeReference( byReferenceType.ElementType, report, location );
			}
		}

		private bool IsForbiddenAssembly( string assemblyName )
		{
			for ( int i = 0; i < _policy.ForbiddenAssemblyPrefixes.Length; i++ ) {
				if ( assemblyName.StartsWith( _policy.ForbiddenAssemblyPrefixes[i], StringComparison.Ordinal ) ) {
					return true;
				}
			}

			return false;
		}

		private bool IsForbiddenType( string fullTypeName )
		{
			for ( int i = 0; i < _policy.ForbiddenTypePrefixes.Length; i++ ) {
				if ( fullTypeName.StartsWith( _policy.ForbiddenTypePrefixes[i], StringComparison.Ordinal ) ) {
					return true;
				}
			}

			return false;
		}

		private bool IsForbiddenMethod( string fullMethodName )
		{
			for ( int i = 0; i < _policy.ForbiddenMethodFullNames.Length; i++ ) {
				if ( string.Equals( fullMethodName, _policy.ForbiddenMethodFullNames[i], StringComparison.Ordinal ) ) {
					return true;
				}
			}

			return false;
		}

		private static bool IsUnsafeOpcode( OpCode opCode )
		{
			return opCode == OpCodes.Calli
				|| opCode == OpCodes.Cpblk
				|| opCode == OpCodes.Initblk
				|| opCode == OpCodes.Localloc
				|| opCode == OpCodes.Ldind_I
				|| opCode == OpCodes.Ldind_I1
				|| opCode == OpCodes.Ldind_I2
				|| opCode == OpCodes.Ldind_I4
				|| opCode == OpCodes.Ldind_I8
				|| opCode == OpCodes.Ldind_R4
				|| opCode == OpCodes.Ldind_R8
				|| opCode == OpCodes.Ldind_Ref
				|| opCode == OpCodes.Stind_I
				|| opCode == OpCodes.Stind_I1
				|| opCode == OpCodes.Stind_I2
				|| opCode == OpCodes.Stind_I4
				|| opCode == OpCodes.Stind_I8
				|| opCode == OpCodes.Stind_R4
				|| opCode == OpCodes.Stind_R8
				|| opCode == OpCodes.Stind_Ref;
		}

		private static string FormatMethodLocation( MethodDefinition method )
		{
			return method.FullName;
		}
	};
};

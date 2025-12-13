# FORMATTING

This document outlines the strict coding standards and formatting rules for this project. All contributors are expected to follow these guidelines to maintain consistency across the codebase.

## Table of Contents
1. [Indentation](#indentation)
2. [Brace-Style](#brace-style)
3. [Documentation](#documentation)
	1. [Class Documentation](#class-documentation)
	2. [Method Documentation](#method-documentation)
	3. [Property & Field Docuemntation](#property--field-docuemntation)
	4. [File Header](#file-header)
4. [Naming Conventions](#naming-conventions)
5. [Code Layout](#code-layout)
	1. [Method Layout](#method-layout)
	2. [Property Layout](#property-layout)
	3. [Constructor Layout](#constructor-layout)
	4. [Namespace Layout](#namespace-layout)
	5. [Class Layout](#class-layout)
6. [General Rules](#general-rules)

## Indentation

This project uses tabs for indentation, 4 spaces each

## Brace Style

This project follows a K&R (Kernighan & Ritchie) brace style:

```csharp
// Correct
if ( condition ) {
	// code
}

for ( int i = 0; i < count; i++ ) {
	// code
}

public void MyMethod() {
	// code
}

// Incorrect
if (condition)
{
	// code
}

for (int i = 0; i < count; i++)
{
	// code
}

public void MyMethod()
{
	// code
}
```

along with the following:

``` csharp
// Correct
public int[] list = [ 1, 2, 3 ];
public List<int> data = [ 1, 2, 3 ];

// Incorrect
public int[] list = [1, 2, 3];
public List<int> data = new(){1, 2, 3};
```

## Documentation

### Class Documentation

``` csharp
/*
===================================================================================
	
ClassName
	
===================================================================================
*/
/// <summary>
/// Detailed description of the class's purpose and what it does
/// </summary>
```

### Method Documentation

``` csharp
/*
===============
MethodName
===============
*/
/// <summary>
/// Detailed description of the method's purpose
/// </summary>
/// <remarks>
/// If needed, add detailed information about what the method does if it's complex
/// enough (the code doesn't self-document).
/// </remarks>
```

### Property & Field Docuemntation

``` csharp
/// <summary>
/// Description of the property or field's purpose and use cases
/// </summary>
```

### File Header

Each file must contain a copyright header with attributions to all contributors and a changelog if the changes are made on a fork.

``` csharp
/*
===========================================================================
The Nomad AGPL Source Code
Copyright (C) 2025 Noah Van Til, (other contributors if applicable)

The Nomad Source Code is free software: you can redistribute it and/or modify
it under the terms of the GNU Affero General Public License as published
by the Free Software Foundation, either version 3 of the License, or
(at your option) any later version.

The Nomad Source Code is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
GNU Affero General Public License for more details.

You should have received a copy of the GNU Affero General Public License
along with The Nomad Source Code.  If not, see <http://www.gnu.org/licenses/>.

If you have questions concerning this license or the applicable additional
terms, you may contact me via email at nyvantil@gmail.com.
===========================================================================
*/
// CHANGELOG
```

## Naming Conventions
* PascalCase: Classes, Methods, Properties, Fields (only public fields)
* camelCase: Parameters and Local Variables
* SNAKE_CASE: Constants

The only exception to the casing is if you're creating a static readonly StringName to reduce string allocations, in which case, use Godot's PascalCasing for those constants as that's consistent with the source-generators and reduces refactoring costs.

## Code Layout

### Switch Statement Layout
Switch statements must be declared like this:
``` csharp
// Correct
switch ( Value ) {
	case 0:
		break;
	case 1:
		break;
	default:
		// not expected path, so throwing exception is fine here
		throw new ArgumentOutOfRangeException( nameof( Value ) );
}

// Incorrect
switch ( Value ) {
case 0:
	break;
case 1:
	break;
};
```
ALWAYS. ALWAYS. Include a default with ArgumentOutOfRangeException or something adjacent to catch and scream when edge cases occur.

### Method Layout
Methods must be declared like this:
``` csharp
// Correct
public void MyMethod( int value ) {
	// code
}

// Incorrect
public void MyMethod(int value)
{
	// code
}
```

### Property Layout
Each property that's marked as public must use { get; private set; }

Public properties & fields must be declared as the following:
``` csharp
// Correct
public int Property { get; private set; }

// Incorrect
public int Property {
	get; private set;
};
```

If a property's setter is a single line, make it inlined:
``` csharp
public int Property { get; private set; } = 0;

[MethodImpl( MethodImplOptions.AggressiveInlining )]
public void SetProperty( int value ) {
	Property = value;
}
```
This is so that private setters can be as efficient as this:
``` csharp
Property = value;
```
This is to avoid outside classes from simply accessing insider-class data without the class actually knowing it's happening. This is to avoid undefined or unknown behavior from outside the class.

### Constructor Layout
If you have a constructor that is calling its base class, format it as the following:
``` csharp
// Correct (no body case)
public ExampleClass()
	: base( [YourArgumentsHere] )
{ }

// Correct (with body case)
public ExampleClass()
	: base( [YourArgumentsHere] )
{
	// Code goes here
}

// Incorrect (no body case)
public ExampleClass() : base( [YourArgumentsHere] ) {
}

// Incorrect (no body case)
public ExampleClass()
	: base( [YourArgumentsHere] ) {
}

// Incorrect (with body case)
public ExampleClass() : base( [YourArgumentsHere] ) {
	// Code goes here
}

// Incorrect (with body case)
public ExampleClass()
	: base( [YourArgumentsHere] ) {
	// Code goes here
}
```
If you have to fight the .editorconfig for it, then so be it: FIGHT IT!

### Namespace Layout
Always put a semicolon after declaring a class or namespace as follows:
``` csharp
// Correct
namespace NameSpace {
	/*
	===================================================================================
	
	ClassName

	===================================================================================
	*/
	/// <summary>
	/// Detailed description of the class's purpose and what it does
	/// </summary>
	public class ClassName {
	};
};

// Incorrect
namespace NameSpace {
	/*
	===================================================================================
	
	ClassName

	===================================================================================
	*/
	/// <summary>
	/// Detailed description of the class's purpose and what it does
	/// </summary>
	public class ClassName {
	}
}
```

### Class Layout
A class must follow the layout detailed below, also follow the indentation:

``` csharp
File Header

using directives

namespace declaration {
	Class Documentation
	class_name {
		public enums
		internal enums
		private enums
		structs/classes (should only ever be very small, extremely focused classes/structs, very rarely public)

		public constants
		internal constants
		protected constants
		private constants
		public static readonly constants
		internal static readonly constants
		protected static reaonly constants
		private static readonly constants
		public exports
		internal exports
		protected exports
		private exports
		public readonly properties
		internal readonly properties
		protected readonly properties
		private readonly properties
		public properties
		internal properties
		protected properties
		private properties

		singleton instances
		
		godot signals/game events

		constructors/destructors/dispose methods

		public methods
		internal methods
		protected methods
		private methods

		godot overrides
	};
};

```

## General Rules
* Don't use modern new() unless the name of the class is really long and it reduces readability to type it out

* Only use "var" if you're using it alongside a "using var", or the type's name is longer than the line, therefore using the var keyword would be appropriate for readability.

* If it can be readonly, make it readonly.

* Almost never use a comment to explain what you're doing, but instead why. I don't want to read the same thing twice.

* Prefer POD's (Plain Old Datatypes), primitives, and structs over complex data types. This is to encourage simplistic and efficient design and reduce abstraction and overhead.

* A variable should never have a public setter unless there is a VERY good reason and said reason is documented clearly

* Enums, unless only used by ONE SINGLE CLASS should always be separated into their own dedicated file

* Never use Enum.IsDefined, I know it's tempting, but it's extremely slow, uses reflection, and it hurts literally nobody to do a manual enum check. Simply put a "Count" at the bottom of your enum and use that.

* Structs declared within classes should serve an extremely focused and documented purpose, such as having a storage container for multiple collections/chunks of data.

* ALWAYS use the nameof() operator whenever you can to avoid string literals and catch possible typos during compile time or even just from the IDE itself. This is to enforce compile-time safety.

* If it can be thread-safe and it doesn't impact performance to do so, make it thread-safe.

* NEVER. EVER. EVER. Use a foreach loop on a fixed-size array or List, or any collection that has a direct index operator. It has a legitimate and annoying impact on performance, and a manual loop makes it much easier to understand what you're doing from a quick glance. The only times you should ever use a foreach loop is if a for loop is literally slower, such as with a Dictionary where indexing it is slower than just using a foreach loop.

* Events that are simply the GameEvent type should always be declared as public static readonly GameEvent unless it requires a specific object reference, in that case it should be a public readonly GameEvent.

* Don't use exceptions in the hot path that is run every frame, let exceptions bubble up in paths triggered by events for both performance and for ease of bug tracking.
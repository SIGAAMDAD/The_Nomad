# FORMATTING

This document outlines the strict coding standards and formatting rules for this project. All contributors are expected to follow these guidelines to maintain consistency across the codebase.

## Table of Contents
1. [Indentation](#indentation)
2. [Brace-Style](#brace-style)
3. [Docuemntation](#documentation)

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
public List<int> data = new List<int>() { 1, 2, 3 };

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
* PascalCase: Classes, Methods, Properties, Fields (both public and private properties/fields)
* camelCase: Parameters and Local Variables
* SNAKE_CASE: Constants

## Class Layout

Method parameters must be declared like this:
``` csharp
// Correct
public void MyMethod( int value ) {
	// code
}

// Incorrect
public void MyMethod(int value) {
	// code
}
```

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
}
```

A class must follow the layout detailed below, also follow the indentation:

``` csharp
File Header

using directives

namespace declaration {
	Class Documentation
	class_name {
		public enums
		private enums
		structs/classes (should only ever be small, extremely focused classes, very rarely public)

		public static readonly constants
		private static readonly constants
		public exports
		private exports
		public properties

		singleton instances

		godot signals

		constructors/destructors/dispose methods

		public methods
		private methods

		godot overrides
	};
};

```

## General Rules
* Don't use modern new()
* Only use "var" if you're using it alongside a "using var", or the type's name is longer than the line, therefore using the var keyword would be appropriate for readability
* If a variable can be readonly, make it readonly
* Almost never use a comment to explain what you're doing, but instead why
* Prefer POD's (Plain Old Datatypes), primitives, and structs over complex data types. This is to encourage simplistic and efficient design and reduce abstraction and overhead.
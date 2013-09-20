Crossflip
=========

Solution to www.hacker.org/cross

This is a project created during the year 2012 to solve the Crossflip puzzle. The puzzle is easily converted to one of inverting a matrix modulo 2, and we solve this by Gaussian elimination.
It was also intended as an exercise in object orientation and abstraction. Hence an "equation" object is split off from the "matrix" object, and so forth. This allows different implementations to be swapped in extremely easily; for instance, one very inefficient implementation I give is where each entry is stored as a character rather than an integer, while one extremely efficient version packs eight entries into a single byte.

Classes used
----------

We solve the matrix equation `M.**x** == b` for `**x**`.

An equation is a class which implements IEquation. It presents a method `coefficient` to get the nth coefficient of the equation, a method `add` to add this row to another row (mod 2), a method `ToByteArray` to convert the equation into an array of bytes, and a method `RHS` to return the right-hand side of the equation.

So, for example, the equation `a+b+d=0` would have `ToByteArray() == {1,1,0,1}` and `RHS() == 0`, while `coefficient(2)` would be 0.

A system of equations is a class which implements IEquationSystem. It is the way we represent `M` and `b` in the matrix equation. It presents methods:

* `byte coefficient(row,col)` to get an entry in the matrix representing the equations
* `void gaussianEliminate()` to convert the system in-place to upper triangular form
* `byte[] backSubstitute()` to produce the vector **x** such that `M.**x** == b`, from a system in upper triangular form
* `void swapTwoEquations(a,b)` moves row `a` to row `b` and vice versa
* `byte[] nthRowOfEquations(n)` to produce the coefficients of the nth equation
* `void replaceEquation(rownum, replacementEquation)` replaces the equation in row `rownum` by the `replacementEquation` [note that the `replacementEquation` need not necessarily be the same class as the row it is replacing; it only needs to implement IEquation]
* `void addTo(addingTo, adding)` adds row number `adding` to row number `addingTo`, storing the result in row number `addingTo`.

We provide a class SystemOfEquations implementing IEquationSystem (while still not making judgements about how the equations are stored).

We also provide several example equation implementations:

* `ByteStoredEquation` stores the coefficients eight to a byte, and is the most efficient provided according to the comments (I haven't run the code for over a year, as of this writing)
* `CharStoredEquation` stores the coefficients as characters
* `PackedIntStoredEquation` stores the coefficients 64 to a 64-bit word

We do subclass SystemOfEquations for each type of equation, but this is only for the purposes of ensuring the constructor is valid. In fact, this should have been abstracted away completely by moving the relevant constructor code into the equation class rather than the system-of-equations class.

There is also a separate abstract class ParallelSystemOfEquations which parallelises the operations of Gaussian elimination. Again, to instantiate it, you need only subclass it and write a constructor; parallelisation is taken care of automatically.

Then there is a Board class to represent the problem before it's been turned into a matrix, and to deal with submission of the solution to hacker.org. By the way, the password parameter given in the code is no longer valid.

A .NET 6 app to calculate prime numbers in parallel, and in a distributed manner.
PrimeCalcMaster is the controlling node
PrimeCalcSlave is the calculating node.

A list of prime numbers is entered in a console screen, the list is broken down into sub-lists of length 10, and each is equally distributed among the three docker slave images created. The prime numbers are then calculated and returned and displayed in the master console app.

(**
# ISADotNet

ISA compliant experimental metadata toolkit in F#

The library contains types and functionality for creating and working on experimental metadata in ISA format. 

Additionally, the types can easily be written to and read from `Json` files in [ISAJson format](https://isa-specs.readthedocs.io/en/latest/isajson.html) and Microsoft `Excel` files in [ISATab format](https://isa-specs.readthedocs.io/en/latest/isatab.html).

#### Table of contents 

- [Aim of the project](#Aim-of-the-project)
- [Quick content rundown](#Quick-content-rundown)
- [Installation](#Installation)
- [Usage](#Usage)


## Aim of the project

With the advent of modern measurement and communication technology, reseach data is becoming both increasingly abundant and accessible. Following the reasoning of 
[FAIR](https://www.go-fair.org/fair-principles/) (Findability, Accessibility, Interoperability, Reuse) principles for scientific data management and stewardship, 
this is just part of the equation. For another researcher to find and reuse previously created research data, besides other necessities, `annotation` is especially important. 

For this task the ISA datamodel was established. The three basic layers of organization are [investigation](Investigation.html), possibly containing several [studies](Study.html), 
each possibly containing multiple [assays](Assay.html). This hierarchically allows for a great range of detail. Where at the top in the investigation general information 
like the institution is stored. On the other hand assays contain very fine-grained information like the instrument model used for measurement.




## Quick content rundown


## Installation

The ISADotNet nuget package can be found here

The ISADotNet.XLSX nuget package can be found here

Adding a package reference via dotnet: dotnet add package ISADotNet --version 0.2.6

Adding a package reference in F# interactive: #r "nuget: ISADotNet, 0.2.6"


## Usage



*)


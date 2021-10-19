# topnoided
A simple top command for Windows.

## Notes
You know that feel when your computer's fans spin like no tommorow, but once you open Task Manager your fans are dead silent? Stuff like that keeps you noided. That's why I got me a simple recreation of [top](https://en.wikipedia.org/wiki/Top_(software)) for Windows.

## Work in progress
- Command line arguments
  - [x] Do not hide Session 0 processes
- Address known crashes
  - [ ] Do not crash
  - [ ] Inform what processes have stopped or attemted to hide from you
- Improve the looks, or make a better looking fork while keeping it simple here

## Command line arguments
`topnoided.exe showall` doesn't let the app hide Session 0 processes

## Build
Clone into your latest Visual Studio installation, and build.

## FAQ
Q1: I get "error CS1056: Unexpected Character '$'", what can be done?

A: Update Visual Studio. Note, this might happen if instead of cloning this repo you copy and paste the code from Program.cs into the wrong type of project; Net 5 is used here.

Q2: I get "warning CA1416: This call site is reachable on all platforms", what can be done?

A: All the code is Windows only, Net 5 is just nicer to write for right now. Let this warning be a reminder that you're not supposed to build this for any other system.

## Contatcs

Feel free to reach me at nodes:matrix.org, though keep in mind that in accordance with [the guidelines](CODE_OF_CONDUCT.md) all interactions there do not represent this project.

## Licence
0BSD

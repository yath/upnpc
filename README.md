upnpc
=====

A simple UPnP command-line client, intended for scripting. It uses
[Intel’s *Developer Tools for UPnP™ Technologies*](http://opentools.homeip.net/upnptools)
library (which happened to work with my TV, contrary to the one
supplied with Windows 10) and is therefore written in C#.

Building
--------

On Linux:

```
$ sudo apt-get install mono-xbuild # FIXME: and a lot of other stuff

$ git clone --recursive https://github.com/yath/upnpc

$ cd upnpc

$ xbuild [/p:Configuration=Debug|Release] # (default is Debug)
# or, if you prefer:
$ make bin/Debug/upnpc.exe

$ ls bin/Debug
```

On Windows:
* Get prerequisites (FIXME)
* Open an *MSBuild Command Prompt*

```
> cd \proj
> …\path\to\git.exe clone --recursive https://github.com/yath/upnpc

> cd upnpc

> msbuild [/p:Configuration=Debug|Release] # (default is Debug)

> dir bin\Debug
```

Usage
-----

### General Advise

upnpc is intended to be used in scripts; for fiddling around with your device and see
its services and actions, use “Device Spy” from [Intel’s Tools for UPnP Technologies](
https://software.intel.com/en-us/articles/intel-software-for-upnp-technology-technology-overview)
or [Coherence’s UPnP Inspector](https://github.com/coherence-project/UPnP-Inspector).

The [Wikipedia page on UPnP](https://en.wikipedia.org/wiki/Universal_Plug_and_Play) should
provide a rough introduction to the general architecture, but the tl;dr is: UPnP uses
some weird discovery mechanism based on Zeroconf and XML that is best left to a library;
a *device* is identified by its UDN (a UUID, essentially, therefore in flags abbreviated
with an `i`) and its URN (something like urn:com.foobar…). Two devices can share the same
URN if they are exposing the same behaviour, but the UDN is intended to be distinct.
A device then exposes one or more *services* that are identified by another set of UDN
and URN. These services provide variables and actions that may be called or set, respectively,
and are identified by a human-readable string (e.g. `SetMainTVChannel`, `TVSource`, …).

upnpc accepts “Windows-style” options, i.e. introduced with a `/` and parameters
introduced by a `:`.

### General options

* `/r:1000`: The interval to send discovery requests at, in milliseconds
* `/dt:30000`: The discovery timeout im milliseconds
* `/vd`: Verbose discovery (boolean). If set, prints out additional debugging
  information during discovery

The default specifies that a discovery request is sent every second; if no
matching device is found within 30 seconds, the program will exit
unsuccessfully. The `/vd` flag may be specified to print out devices and
services found during discovery.

### Device / Service matching

* `/du:urn:com.foo.bar:…`: Match a device by the specified URN
* `/di:10b07600-cafebabe-…` Match a device by the specified UDN (i.e. UU*I*D)
* `/df:MyAwesomeDevice`: Match a device by its friendly name (case-sensitive)

* `/su:urn:com.foo.barservice:…` Match a service by the specified URN
* `/si:10b07600-deadbeef-…`: Match a service by the specified UDN (UU*I*D)

Either one of `/si` or `/su` needs to be specified. The device matching flags
are entirely optional and can be used to further constraint the match, if
you happen to have more than one device providing a specific service on your
network.

### Actions

* `/a:MyAwesomeAction`: Call the specified action on the service
* `/sv:variable=value`: Set a variable before calling the action
* `/gv:variable`: Print (*G*et) a variable’s content before exiting
* `/ev:variable=expected_value`: Expect the specified variable to
  contain the specified content after calling the action; otherwise,
  exit unsuccessfully.
* `/dv`: Dump variables (boolean). If set, dumps all variables
  belonging to the specified action.


The `/a` option is mandatory and specifies the service to be called. Actions
take a set of (optional) input variables and may set output variables as
a result of their call (usually a success status). The input variables may
be set with `/sv`, output variables may be dumped to stdout with `/gv`.
Additionally, `/ev` defines an assertion of a variable’s content to
equal the specified value; this may used for checking the action’s
success status.

Bugs
----

When compiled (or ran?) with Mono, it’s painfully slow. I haven’t yet looked
into it yet, though.


Author, License, Legalese
-------------------------

Sebastian Schmidt `<yath@yath.de>`, 2016. MIT License due to the UPnP
library, but feel free to do what you want with upnpc’s code itself. No
warranties for destroying your home equipment, aquarium, or anything at all.

XBUILD := xbuild
SOLUTION := upnpc.sln

SRCS := Options.cs Program.cs $(shell find intel-upnp-dlna/UPnP/ -name '*.cs')

bin/Debug/upnpc.exe: $(SRCS)
	$(XBUILD) /p:Configuration=Debug /t:Build $(SOLUTION)

bin/Release/upnpc.exe: $(SRCS)
	$(XBUILD) /p:Configuration=Release /t:Build $(SOLUTION)

.PHONY: clean
clean:
	$(XBUILD) $(SOLUTION) /t:Clean
	rm -rf bin obj

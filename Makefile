MSB=MSBuild.exe /verbosity:m
paket=.paket/paket.exe

.PHONY: pack
pack: build
	mkdir -p tmp
	rm -f tmp/*.nupkg
	${paket} pack tmp/

.PHONY: publish
publish: pack
	${paket} push tmp/*.nupkg --url https://www.myget.org/F/pragmatrix/api/v2/package --api-key ${MYGETAPIKEY}

.PHONY: build
build:
	${MSB} Konstruktor2.sln -p:Configuration=Release -t:Konstruktor2:Rebuild


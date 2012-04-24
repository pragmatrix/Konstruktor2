REVISION:=$(shell svn update >/dev/null && svn info | grep "Revision: .*" | cut -d ' ' -f 2)

DIST_NAME=Konstruktor_${REVISION}
DIST_SRC=${DIST_NAME}_src
DIST_BIN=${DIST_NAME}_bin

MSB=msbuild.exe /verbosity:m
DIST_FILES=Release/Konstruktor.dll Release/Konstruktor.pdb

GC_PW:=$(shell cat googlecode.pw)
GC_PROJECT=tiny-konstruktor
GC_USER=paramatrix

.PHONY: distribute
distribute: package
	./googlecode_upload.py -s "Release binaries (.NET 4), Konstruktor revision ${REVISION}" -p ${GC_PROJECT} -u ${GC_USER} -w ${GC_PW} -l Type-Executable /tmp/${DIST_BIN}.zip
	./googlecode_upload.py -s "Sources, Konstruktor revision ${REVISION}" -p ${GC_PROJECT} -u ${GC_USER} -w ${GC_PW} -l Type-Source /tmp/${DIST_SRC}.zip 

.PHONY: package
package: build packagesource packagebinary

.PHONY: packagesource
packagesource: 
	rm -rf /tmp/${DIST_SRC}
	svn export . /tmp/${DIST_SRC}
	cd /tmp && rm -f ${DIST_SRC}.zip && zip -r ${DIST_SRC}.zip ${DIST_SRC}

.PHONY: build
build:
	${MSB} Konstruktor.sln /p:Configuration=Release /target:Konstruktor:Rebuild	

.PHONY: packagebinary
packagebinary:
	rm -rf /tmp/${DIST_BIN}
	mkdir /tmp/${DIST_BIN}
	cp ${DIST_FILES} /tmp/${DIST_BIN}
	cd /tmp && rm -f ${DIST_BIN}.zip && zip -r ${DIST_BIN}.zip ${DIST_BIN}

